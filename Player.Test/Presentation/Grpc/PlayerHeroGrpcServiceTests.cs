using FluentAssertions;
using Grpc.Core;
using MediatR;
using Moq;
using Player.Application.Features.HeroUseCase;
using Player.Application.Features.HeroUseCase.GetCombatantSnapshot;
using Player.Application.Features.HeroUseCase.GetHeroAppearance;
using Player.Contracts.V1;
using Player.Presentation.Grpc.Services;

namespace Player.Test.Presentation.Grpc;

public sealed class PlayerHeroGrpcServiceTests
{
    [Fact]
    public async Task GetHeroAppearance_KnownHero_ReturnsTheAppearance()
    {
        Guid heroId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender
            .Setup(x =>
                x.Send(
                    It.Is<GetHeroAppearanceQuery>(query => query.HeroId == heroId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new HeroAppearance(4, "dap1", 7));
        var service = new PlayerHeroGrpcService(sender.Object);

        HeroAppearanceReply reply = await service.GetHeroAppearance(
            new GetHeroAppearanceRequest { HeroId = heroId.ToString() },
            TestServerCallContext.Create()
        );

        reply.HeroId.Should().Be(heroId.ToString());
        reply.SkinTone.Should().Be(4);
        reply.HairStyle.Should().Be("dap1");
        reply.HasHairColor.Should().BeTrue();
        reply.HairColor.Should().Be(7);
    }

    [Fact]
    public async Task GetHeroAppearance_BaldHero_LeavesTheHairColorAbsent()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(x => x.Send(It.IsAny<GetHeroAppearanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroAppearance(2, "bald", null));
        var service = new PlayerHeroGrpcService(sender.Object);

        HeroAppearanceReply reply = await service.GetHeroAppearance(
            new GetHeroAppearanceRequest { HeroId = Guid.NewGuid().ToString() },
            TestServerCallContext.Create()
        );

        reply.HasHairColor.Should().BeFalse();
    }

    [Fact]
    public async Task GetHeroAppearance_UnknownHero_ReturnsNotFound()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(x => x.Send(It.IsAny<GetHeroAppearanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HeroAppearance?)null);
        var service = new PlayerHeroGrpcService(sender.Object);

        Func<Task> action = () =>
            service.GetHeroAppearance(
                new GetHeroAppearanceRequest { HeroId = Guid.NewGuid().ToString() },
                TestServerCallContext.Create()
            );

        (await action.Should().ThrowAsync<RpcException>())
            .Which.StatusCode.Should()
            .Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-uuid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task GetHeroAppearance_InvalidHeroId_ReturnsInvalidArgumentWithoutQuerying(
        string heroId
    )
    {
        var sender = new Mock<ISender>();
        var service = new PlayerHeroGrpcService(sender.Object);

        Func<Task> action = () =>
            service.GetHeroAppearance(
                new GetHeroAppearanceRequest { HeroId = heroId },
                TestServerCallContext.Create()
            );

        (await action.Should().ThrowAsync<RpcException>())
            .Which.StatusCode.Should()
            .Be(StatusCode.InvalidArgument);
        sender.Verify(
            x => x.Send(It.IsAny<GetHeroAppearanceQuery>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetCombatantSnapshot_KnownHero_MapsEveryField()
    {
        Guid heroId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender
            .Setup(x =>
                x.Send(
                    It.Is<GetCombatantSnapshotQuery>(query => query.HeroId == heroId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new CombatantSnapshot(
                    heroId,
                    "Lyra",
                    "mage",
                    3,
                    99,
                    60,
                    20,
                    4,
                    14,
                    [
                        new CombatantAbilitySnapshot(
                            "mage-bolt",
                            "Arcane Bolt",
                            6,
                            "SingleEnemy",
                            170
                        ),
                    ],
                    new HeroAppearance(5, "bald", null)
                )
            );
        var service = new PlayerHeroGrpcService(sender.Object);

        CombatantSnapshotReply reply = await service.GetCombatantSnapshot(
            new GetCombatantSnapshotRequest { HeroId = heroId.ToString() },
            TestServerCallContext.Create()
        );

        reply.HeroId.Should().Be(heroId.ToString());
        reply.Name.Should().Be("Lyra");
        reply.ClassCode.Should().Be("mage");
        reply.Level.Should().Be(3);
        reply.CurrentHp.Should().Be(99);
        reply.MaxHp.Should().Be(99);
        reply.CurrentMana.Should().Be(60);
        reply.MaxMana.Should().Be(60);
        reply.Attack.Should().Be(20);
        reply.Defense.Should().Be(4);
        reply.Speed.Should().Be(14);
        reply.Abilities.Should().ContainSingle();
        reply.Abilities[0].Code.Should().Be("mage-bolt");
        reply.Abilities[0].Name.Should().Be("Arcane Bolt");
        reply.Abilities[0].ManaCost.Should().Be(6);
        reply.Abilities[0].TargetType.Should().Be("SingleEnemy");
        reply.Abilities[0].Power.Should().Be(170);
        reply.Appearance.SkinTone.Should().Be(5);
        reply.Appearance.HairStyle.Should().Be("bald");
        reply.Appearance.HasHairColor.Should().BeFalse();
    }

    [Fact]
    public async Task GetCombatantSnapshot_UnknownHero_ReturnsNotFound()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(x =>
                x.Send(It.IsAny<GetCombatantSnapshotQuery>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((CombatantSnapshot?)null);
        var service = new PlayerHeroGrpcService(sender.Object);

        Func<Task> action = () =>
            service.GetCombatantSnapshot(
                new GetCombatantSnapshotRequest { HeroId = Guid.NewGuid().ToString() },
                TestServerCallContext.Create()
            );

        (await action.Should().ThrowAsync<RpcException>())
            .Which.StatusCode.Should()
            .Be(StatusCode.NotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-uuid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task GetCombatantSnapshot_InvalidHeroId_ReturnsInvalidArgument(string heroId)
    {
        var service = new PlayerHeroGrpcService(new Mock<ISender>().Object);

        Func<Task> action = () =>
            service.GetCombatantSnapshot(
                new GetCombatantSnapshotRequest { HeroId = heroId },
                TestServerCallContext.Create()
            );

        (await action.Should().ThrowAsync<RpcException>())
            .Which.StatusCode.Should()
            .Be(StatusCode.InvalidArgument);
    }
}
