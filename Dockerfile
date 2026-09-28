FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Player.Presentation.slnx ./
COPY Player.Contracts/Player.Contracts.csproj Player.Contracts/
COPY Player.Domain/Player.Domain.csproj Player.Domain/
COPY Player.Application/Player.Application.csproj Player.Application/
COPY Player.Infrastructure/Player.Infrastructure.csproj Player.Infrastructure/
COPY Player.Presentation/Player.Presentation.csproj Player.Presentation/
COPY Player.Test/Player.Test.csproj Player.Test/

RUN dotnet restore Player.Presentation.slnx

COPY . .
RUN dotnet publish Player.Presentation/Player.Presentation.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

EXPOSE 8080 8081

COPY --from=build /app/publish .

# Unprivileged "app" user shipped by the aspnet image.
USER $APP_UID

ENTRYPOINT ["dotnet", "Player.Presentation.dll"]
