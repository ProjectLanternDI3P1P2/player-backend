using MediatR;
using Player.Domain.Entities;
using Player.Domain.Repositories;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Application.Features.HeroUseCase.DeselectHero;

public sealed class DeselectHeroCommandHandler(IHeroRepository repository)
    : IRequestHandler<DeselectHeroCommand>
{
    public async Task Handle(DeselectHeroCommand request, CancellationToken cancellationToken)
    {
        PlayerEntity? player = await repository.GetPlayerForUpdateAsync(
            request.PlayerId,
            cancellationToken
        );
        if (player is null)
            throw new KeyNotFoundException($"Player '{request.PlayerId}' was not found.");

        Hero? hero = await repository.GetActiveByIdAndPlayerIdAsync(
            request.HeroId,
            request.PlayerId,
            cancellationToken
        );
        if (hero is null)
            throw new KeyNotFoundException($"Hero '{request.HeroId}' was not found.");

        if (player.SelectedHeroId == hero.Id)
            player.SelectedHeroId = null;
    }
}
