using FluentAssertions;
using Grpc.Core;
using MediatR;
using Moq;
using Player.Application.Features.HeroUseCase;
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
}
