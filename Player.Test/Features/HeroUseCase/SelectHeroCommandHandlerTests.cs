using FluentAssertions;
using Moq;
using Player.Application.Features.HeroUseCase.SelectHero;
using Player.Domain.Entities;
using Player.Domain.Repositories;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Test.Features.HeroUseCase;

public sealed class SelectHeroCommandHandlerTests
{
    [Fact]
    public async Task Handle_ActiveHeroOwnedByPlayer_PersistsItsSelection()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        var player = new PlayerEntity { Id = playerId };
        var repository = new Mock<IHeroRepository>();
        repository
            .Setup(x => x.GetPlayerForUpdateAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);
        repository
            .Setup(x =>
                x.GetActiveByIdAndPlayerIdAsync(heroId, playerId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new Hero { Id = heroId, PlayerId = playerId });
        var handler = new SelectHeroCommandHandler(repository.Object);

        await handler.Handle(
            new SelectHeroCommand(playerId, heroId),
            TestContext.Current.CancellationToken
        );

        player.SelectedHeroId.Should().Be(heroId);
    }
}
