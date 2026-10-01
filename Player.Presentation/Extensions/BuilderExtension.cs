using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Player.Presentation.Configuration;
using Player.Presentation.Extensions.LogExtension;
using Player.Presentation.Grpc.Interceptors;
using Player.Presentation.Hubs.Filters;
using Player.Presentation.Middleware;
using Serilog;

namespace Player.Presentation.Extensions;

public static class BuilderExtension
{
    public const string GameClientCorsPolicy = "GameClient";

    public static WebApplicationBuilder ConfigureApi(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();
        builder.Services.AddHealthChecks();
        builder
            .Services.AddOptions<GameClientCorsOptions>()
            .BindConfiguration(GameClientCorsOptions.SectionName)
            .Validate(HasTrustedOrigins, "Cors:AllowedOrigins must contain valid HTTP(S) origins.")
            .ValidateOnStart();
        builder.Services.AddCors();
        builder.Services.AddSingleton<
            IConfigureOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>,
            GameClientCorsPolicyConfiguration
        >();
        // Gameplay is a Hub concern. REST controllers remain reserved for
        // non-gameplay resources (ADR-GLOB-001 and ADR 0018).
        builder.Services.AddSignalR(options => options.AddFilter<SignalRCommandExceptionFilter>());
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<CorrelationIdInterceptor>();
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });

        ConfigureLogger(builder);

        builder.Services.AddTransient<ExceptionHandlingMiddleware>();
        builder.Services.AddTransient<CorrelationIdInterceptor>();
        builder.Services.AddTransient<GrpcExceptionInterceptor>();
        builder.Services.AddTransient<SignalRCommandExceptionFilter>();
        builder.Services.AddHttpClient();

        return builder;
    }

    private static void ConfigureLogger(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog(
            (context, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.With<LowercaseLevelEnricher>()
                    .Destructure.With<IgnoreLoggingDestructuringPolicy>();
            },
            preserveStaticLogger: true
        );
    }

    private static bool HasTrustedOrigins(GameClientCorsOptions options)
    {
        return options.AllowedOrigins.Length > 0 && options.AllowedOrigins.All(IsTrustedOrigin);
    }

    private static bool IsTrustedOrigin(string origin) =>
        Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        && uri.GetLeftPart(UriPartial.Authority) == origin.TrimEnd('/');
}
