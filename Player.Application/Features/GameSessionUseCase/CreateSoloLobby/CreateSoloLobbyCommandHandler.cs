using System.Security.Cryptography;
using System.Text;
using MediatR;
using Player.Domain.Entities;
using Player.Domain.Exceptions;
using Player.Domain.Repositories;
using Player.Domain.Services;

namespace Player.Application.Features.GameSessionUseCase.CreateSoloLobby;

public sealed class CreateSoloLobbyCommandHandler(IGameSessionRepository repository, IClock clock)
    : IRequestHandler<CreateSoloLobbyCommand, GameSessionSnapshot>
{
    private const string Scope = "game-session.create-solo-lobby";

    public async Task<GameSessionSnapshot> Handle(
        CreateSoloLobbyCommand request,
        CancellationToken cancellationToken
    )
    {
        string fingerprint = Fingerprint(request.HeroId);
        IdempotencyKey? existing = await repository.GetIdempotencyKeyAsync(
            request.IdempotencyKey,
            cancellationToken
        );
        if (existing is not null)
            return await GetExistingAsync(
                existing,
                request.PlayerId,
                fingerprint,
                cancellationToken
            );

        Hero? hero = await repository.GetActiveHeroForUpdateAsync(
            request.HeroId,
            request.PlayerId,
            cancellationToken
        );
        if (hero is null)
            throw new KeyNotFoundException($"Hero '{request.HeroId}' was not found.");

        existing = await repository.GetIdempotencyKeyAsync(
            request.IdempotencyKey,
            cancellationToken
        );
        if (existing is not null)
            return await GetExistingAsync(
                existing,
                request.PlayerId,
                fingerprint,
                cancellationToken
            );

        if (await repository.HasOpenSessionAsync(hero.Id, cancellationToken))
            throw new ConflictException("This hero already has an open session.");

        DateTimeOffset now = clock.UtcNow;
        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            CreatorPlayerId = request.PlayerId,
            Status = "Lobby",
            Mode = "Solo",
            LastActiveAt = now,
            Members =
            [
                new GameSessionMember
                {
                    HeroId = hero.Id,
                    Hero = hero,
                    MemberStatus = "Active",
                    JoinedAt = now,
                },
            ],
            Transitions =
            [
                new GameSessionTransition
                {
                    Id = Guid.NewGuid(),
                    SourceStatus = "None",
                    TargetStatus = "Lobby",
                    Cause = "LobbyCreated",
                    Actor = "player",
                    OccurredAt = now,
                },
            ],
        };
        repository.Add(session);
        repository.AddIdempotencyKey(
            new IdempotencyKey
            {
                Key = request.IdempotencyKey,
                PlayerId = request.PlayerId,
                Scope = Scope,
                RequestFingerprint = fingerprint,
                ProducedResourceId = session.Id,
                ExpiresAt = now.AddDays(1),
            }
        );
        return GameSessionSnapshot.From(session);
    }

    private async Task<GameSessionSnapshot> GetExistingAsync(
        IdempotencyKey key,
        Guid playerId,
        string fingerprint,
        CancellationToken cancellationToken
    )
    {
        if (
            key.PlayerId != playerId
            || key.Scope != Scope
            || key.RequestFingerprint != fingerprint
            || key.ProducedResourceId is null
        )
            throw new ConflictException(
                "This idempotency key was already used for a different request."
            );
        GameSession? session = await repository.GetByIdAsync(
            key.ProducedResourceId.Value,
            cancellationToken
        );
        if (session is null)
            throw new InvalidOperationException(
                "The idempotent session result is no longer available."
            );
        return GameSessionSnapshot.From(session, true);
    }

    private static string Fingerprint(Guid heroId) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(heroId.ToString("N"))));
}
