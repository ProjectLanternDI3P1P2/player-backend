using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Player.Application.Ports;
using Player.Infrastructure.Grpc;
using Player.Infrastructure.Grpc.Clients;

namespace Player.Test.Infrastructure.Grpc;

public sealed class GrpcServiceRegistrationTests
{
    [Fact]
    public void AddGrpcConfiguration_StartsRunsThroughTheDungeonGrpcClient()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddLogging();

        // Act
        services.AddGrpcConfiguration(Configuration("http://dungeon:8081"));
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        // Assert
        scope.ServiceProvider.GetRequiredService<IDungeonClient>()
            .Should()
            .BeOfType<DungeonGrpcClient>();
    }

    [Fact]
    public void AddGrpcConfiguration_RelativeAddress_IsRefusedAtStartup()
    {
        // Act
        Action act = () =>
            new ServiceCollection().AddGrpcConfiguration(Configuration("dungeon:8081"));

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*Grpc:Dungeon:Address*");
    }

    private static IConfiguration Configuration(string address) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["Grpc:Dungeon:Address"] = address }
            )
            .Build();
}
