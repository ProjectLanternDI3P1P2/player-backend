using FluentAssertions;
using Moq;
using Player.Application.Features.HeroUseCase.DeselectHero;
using Player.Domain.Entities;
using Player.Domain.Repositories;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Test.Features.HeroUseCase;

public sealed class DeselectHeroCommandHandlerTests
{
    [Fact]
    public async Task Handle_SelectedActiveHero_ClearsTheSelection()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        var player = new PlayerEntity { Id = playerId, SelectedHeroId = heroId };
        var repository = new Mock<IHeroRepository>();
        repository
            .Setup(x => x.GetPlayerForUpdateAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);
        repository
            .Setup(x =>
                x.GetActiveByIdAndPlayerIdAsync(heroId, playerId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new Hero { Id = heroId, PlayerId = playerId });
        var handler = new DeselectHeroCommandHandler(repository.Object);

        await handler.Handle(
            new DeselectHeroCommand(playerId, heroId),
            TestContext.Current.CancellationToken
        );

        player.SelectedHeroId.Should().BeNull();
    }
}
