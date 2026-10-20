using MediatR;
using Player.Application.Features.GameSessionUseCase;
using Player.Domain.Entities;
using Player.Domain.Exceptions;
using Player.Domain.Repositories;
using Player.Domain.Services;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Application.Features.HeroUseCase.DeleteHero;

public sealed class DeleteHeroCommandHandler(
    IHeroRepository heroRepository,
    IGameSessionRepository sessionRepository,
    IClock clock
) : IRequestHandler<DeleteHeroCommand, DeleteHeroResult>
{
    public async Task<DeleteHeroResult> Handle(
        DeleteHeroCommand request,
        CancellationToken cancellationToken
    )
    {
        // Lock order (player, sessions, hero) matches the session use cases.
        PlayerEntity? player = await heroRepository.GetPlayerForUpdateAsync(
            request.PlayerId,
            cancellationToken
        );
        if (player is null)
            throw new KeyNotFoundException($"Player '{request.PlayerId}' was not found.");

        IReadOnlyList<GameSession> openSessions =
            await sessionRepository.ListOpenSessionsForHeroForUpdateAsync(
                request.HeroId,
                cancellationToken
            );

        Hero? hero = await heroRepository.GetByIdAndPlayerIdForUpdateAsync(
            request.HeroId,
            request.PlayerId,
            cancellationToken
        );
        if (hero is null)
            throw new KeyNotFoundException($"Hero '{request.HeroId}' was not found.");

        if (hero.IsDeleted)
            return new DeleteHeroResult(hero.Id, hero.DeletedAt ?? hero.CreatedAt, true, []);

        if (openSessions.Any(session => session.Status == GameSessionTermination.ActiveStatus))
            throw new ConflictException(
                "This hero is playing a running session. It can be deleted once the run is over or abandoned."
            );

        DateTimeOffset now = clock.UtcNow;
        foreach (GameSession session in openSessions)
        {
            // Lobby: close it explicitly (terminal state + hero release) before deleting.
            GameSessionTermination.Close(
                sessionRepository,
                session,
                GameSessionTermination.AbandonedStatus,
                cause: "HeroDeleted",
                actor: "player",
                now
            );
        }

        if (player.SelectedHeroId == hero.Id)
            player.SelectedHeroId = null;

        hero.IsDeleted = true;
        hero.DeletedAt = now;

        return new DeleteHeroResult(
            hero.Id,
            now,
            false,
            openSessions.Select(session => GameSessionSnapshot.From(session)).ToArray()
        );
    }
}
