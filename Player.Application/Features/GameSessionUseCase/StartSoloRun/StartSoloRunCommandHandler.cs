using System.Security.Cryptography;
using System.Text;
using MediatR;
using Player.Application.Ports;
using Player.Domain.Entities;
using Player.Domain.Exceptions;
using Player.Domain.Repositories;
using Player.Domain.Services;

namespace Player.Application.Features.GameSessionUseCase.StartSoloRun;

public sealed class StartSoloRunCommandHandler(
    IGameSessionRepository repository,
    IDungeonClient dungeonClient,
    IClock clock
) : IRequestHandler<StartSoloRunCommand, StartSoloRunResult>
{
    private const string Scope = "game-session.start-solo";
    private const string Active = "Active";
    private const string Failed = "Failed";

    public async Task<StartSoloRunResult> Handle(
        StartSoloRunCommand request,
        CancellationToken cancellationToken
    )
    {
        string fingerprint = CreateFingerprint(request.HeroId);
        IdempotencyKey? existing = await repository.GetIdempotencyKeyAsync(
            request.IdempotencyKey,
            cancellationToken
        );
        if (existing is not null)
        {
            return await GetIdempotentResultAsync(
                existing,
                request.PlayerId,
                fingerprint,
                cancellationToken
            );
        }

        Hero? hero = await repository.GetActiveHeroForUpdateAsync(
            request.HeroId,
            request.PlayerId,
            cancellationToken
        );
        if (hero is null)
        {
            throw new KeyNotFoundException($"Hero '{request.HeroId}' was not found.");
        }

        existing = await repository.GetIdempotencyKeyAsync(
            request.IdempotencyKey,
            cancellationToken
        );
        if (existing is not null)
        {
            return await GetIdempotentResultAsync(
                existing,
                request.PlayerId,
                fingerprint,
                cancellationToken
            );
        }

        if (await repository.HasActiveSessionAsync(hero.Id, cancellationToken))
        {
            throw new ConflictException("This hero already has an active session.");
        }

        DateTimeOffset now = clock.UtcNow;
        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            Status = Failed,
            Mode = "Solo",
            TerminationReason = "DungeonUnavailable",
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
                    TargetStatus = Failed,
                    Cause = "DungeonRunRequested",
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

        try
        {
            DungeonRun run = await dungeonClient.StartRunAsync(
                session.Id,
                hero.Id,
                cancellationToken
            );
            session.Status = Active;
            session.DungeonRunId = run.Id;
            session.DungeonSeed = run.Seed;
            session.StartedAt = now;
            session.TerminationReason = null;
            session.Transitions.Add(
                new GameSessionTransition
                {
                    Id = Guid.NewGuid(),
                    SessionId = session.Id,
                    SourceStatus = Failed,
                    TargetStatus = Active,
                    Cause = "DungeonRunCreated",
                    Actor = "dungeon",
                    OccurredAt = now,
                }
            );
        }
        catch (Exception exception) when (IsDungeonUnavailable(exception, cancellationToken))
        {
            session.TerminationReason = "DungeonUnavailable";
        }

        return ToResult(session, false);
    }

    private async Task<StartSoloRunResult> GetIdempotentResultAsync(
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
        {
            throw new ConflictException(
                "This idempotency key was already used for a different request."
            );
        }

        GameSession? session = await repository.GetByIdAsync(
            key.ProducedResourceId.Value,
            cancellationToken
        );
        if (session is null)
        {
            throw new InvalidOperationException(
                "The idempotent session result is no longer available."
            );
        }

        return ToResult(session, true);
    }

    private static StartSoloRunResult ToResult(GameSession session, bool alreadyExists)
    {
        Hero hero = session.Members.Single().Hero;
        return new(
            session.Id,
            new SessionHero(hero.Id, hero.Name, hero.ClassCode, hero.Level),
            session.Status,
            session.DungeonRunId,
            session.DungeonSeed,
            session.TerminationReason,
            alreadyExists
        );
    }

    private static bool IsDungeonUnavailable(
        Exception exception,
        CancellationToken cancellationToken
    ) =>
        exception is HttpRequestException
        || exception is TimeoutException
        || (exception is OperationCanceledException && !cancellationToken.IsCancellationRequested);

    private static string CreateFingerprint(Guid heroId) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(heroId.ToString("N"))));
}
