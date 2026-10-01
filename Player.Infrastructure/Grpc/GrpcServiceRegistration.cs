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
        DungeonGrpcClientOptions dungeonOptions =
            configuration
                .GetSection(DungeonGrpcClientOptions.SectionName)
                .Get<DungeonGrpcClientOptions>()
            ?? new DungeonGrpcClientOptions();

        ValidateDungeonGrpcClientOptions(dungeonOptions);

        services
            .AddGrpcClient<DungeonRunService.DungeonRunServiceClient>(options =>
                options.Address = new Uri(dungeonOptions.Address)
            )
            // A cancelled request surfaces as a cancellation, not as a gRPC failure.
            .ConfigureChannel(channel => channel.ThrowOperationCanceledOnCancellation = true);

        return services
            .AddSingleton(Options.Create(dungeonOptions))
            .AddScoped<IDungeonClient, DungeonGrpcClient>();
    }

    private static void ValidateDungeonGrpcClientOptions(DungeonGrpcClientOptions options)
    {
        if (
            !Uri.TryCreate(options.Address, UriKind.Absolute, out Uri? address)
            || (address.Scheme != Uri.UriSchemeHttp && address.Scheme != Uri.UriSchemeHttps)
        )
        {
            throw new InvalidOperationException(
                "Grpc:Dungeon:Address must be an absolute http:// or https:// URI."
            );
        }

        if (options.TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException(
                "Grpc:Dungeon:TimeoutSeconds must be greater than zero."
            );
        }
    }
}
