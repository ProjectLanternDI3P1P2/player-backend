using Player.Presentation.Hubs;
using Player.Presentation.Middleware;
using Scalar.AspNetCore;

namespace Player.Presentation.Extensions;

public static class ApplicationExtension
{
    public static WebApplication ConfigureStart(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseCors(BuilderExtension.GameClientCorsPolicy);

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.MapControllers();
        app.MapHub<PlayerHub>("/hubs/player");
        app.MapGrpcServices();
        app.MapHealthChecks("/health/live");
        app.MapHealthChecks("/health/ready");

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(opt =>
            {
                opt.Title = "Player API";
                opt.Theme = ScalarTheme.DeepSpace;
                opt.AddApiKeyAuthentication("UserId", scheme => scheme.WithName("X-User-Id"));
                opt.EnablePersistentAuthentication();
            });
        }

        return app;
    }
}
