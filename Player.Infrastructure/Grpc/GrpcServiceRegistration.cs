using Dungeon.Contracts.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Player.Application.Ports;
using Player.Infrastructure.Grpc.Clients;
using Player.Infrastructure.Grpc.Configuration;

namespace Player.Infrastructure.Grpc;

public static class GrpcServiceRegistration
{
    public static IServiceCollection AddGrpcConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        DungeonGrpcClientOptions dungeonGrpcClientOptions =
            configuration
                .GetSection(DungeonGrpcClientOptions.SectionName)
                .Get<DungeonGrpcClientOptions>()
            ?? new DungeonGrpcClientOptions();

        ValidateDungeonGrpcClientOptions(dungeonGrpcClientOptions);

        return services
            .AddSingleton(Options.Create(dungeonGrpcClientOptions))
            .AddGrpcClient<DungeonRunService.DungeonRunServiceClient>(options =>
                options.Address = new Uri(dungeonGrpcClientOptions.Address)
            )
            .Services.AddScoped<IDungeonClient, DungeonGrpcClient>();
    }

    private static void ValidateDungeonGrpcClientOptions(DungeonGrpcClientOptions options)
    {
        if (!Uri.TryCreate(options.Address, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("Grpc:Dungeon:Address must be an absolute URI.");
        }

        if (options.TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException(
                "Grpc:Dungeon:TimeoutSeconds must be greater than zero."
            );
        }
    }
}
