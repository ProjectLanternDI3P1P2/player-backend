using Microsoft.Extensions.DependencyInjection;

namespace Player.Infrastructure.Grpc;

public static class GrpcServiceRegistration
{
    public static IServiceCollection AddGrpcConfiguration(this IServiceCollection services)
    {
        // Player does not call itself. Add outbound clients here when a real
        // producer-owned gRPC contract (for example Dungeon) is introduced.
        return services;
    }
}
