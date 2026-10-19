# syntax=docker/dockerfile:1.10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# No SDK telemetry or banner during the build.
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1

WORKDIR /src
COPY NuGet.config ./

COPY Player.Presentation.slnx ./
COPY Player.Contracts/Player.Contracts.csproj Player.Contracts/
COPY Player.Domain/Player.Domain.csproj Player.Domain/
COPY Player.Application/Player.Application.csproj Player.Application/
COPY Player.Infrastructure/Player.Infrastructure.csproj Player.Infrastructure/
COPY Player.Presentation/Player.Presentation.csproj Player.Presentation/
COPY Player.Test/Player.Test.csproj Player.Test/

RUN --mount=type=secret,id=github_packages_credentials,env=NuGetPackageSourceCredentials_github \
    dotnet restore Player.Presentation.slnx

COPY . .
# UseAppHost=false: no native executable, the app is started with "dotnet <dll>".
RUN dotnet publish Player.Presentation/Player.Presentation.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    -p:UseAppHost=false

# Chiseled (distroless) runtime: no shell, no package manager, no curl.
# The "extra" variant keeps ICU and tzdata for cultures and time zones.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS runtime

# DOTNET_EnableDiagnostics=0 disables the debugger, profiler and diagnostic
# sockets. Nothing is written to /tmp any more, so the root filesystem can be
# read-only.
#
# Logging__Console__*: JSON logs on stdout, one event per line, ISO 8601 UTC
# timestamps. Ready for Loki / ELK / Datadog. The log level stays driven by
# appsettings.json or the environment.
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0 \
    Logging__Console__FormatterName=json \
    Logging__Console__FormatterOptions__UseUtcTimestamp=true \
    Logging__Console__FormatterOptions__TimestampFormat=O \
    Logging__Console__FormatterOptions__IncludeScopes=true

WORKDIR /app

# 8080 = public HTTP, 8081 = internal gRPC (never behind the ingress).
EXPOSE 8080 8081

# Owned by root: the app user can read its binaries but not rewrite them.
COPY --from=build --chown=0:0 /app/publish .

# Unprivileged numeric "app" user (1654) shipped by the aspnet image, so
# Kubernetes can enforce runAsNonRoot.
USER $APP_UID

# No HEALTHCHECK: the image has no shell or curl, and Kubernetes ignores it.
# Health goes through the /health/live and /health/ready HTTP probes.
ENTRYPOINT ["dotnet", "Player.Presentation.dll"]
