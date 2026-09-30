using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Player.Application.Ports;
using Player.Infrastructure.Grpc.Clients;

namespace Player.Infrastructure.Grpc;

public static class GrpcServiceRegistration
{
    public static IServiceCollection AddGrpcConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Dungeon has not released its producer-owned Dungeon.Contracts package yet.
        // Keep the outbound integration on the gRPC seam so switching to the generated
        // typed client only replaces this registration and adapter.
        return services.AddScoped<IDungeonClient, MockDungeonGrpcClient>();
    }
}
