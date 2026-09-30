using System.Security.Cryptography;
using System.Text;
using MediatR;
using Player.Application.Ports;
using Player.Domain.Entities;
using Player.Domain.Exceptions;
using Player.Domain.Repositories;
using Player.Domain.Services;

namespace Player.Application.Features.GameSessionUseCase.StartSession;

public sealed class StartSessionCommandHandler(
    IGameSessionRepository repository,
    IDungeonClient dungeonClient,
    IClock clock
) : IRequestHandler<StartSessionCommand, GameSessionSnapshot>
{
    private const string Scope = "game-session.start";

    public async Task<GameSessionSnapshot> Handle(
        StartSessionCommand request,
        CancellationToken cancellationToken
    )
    {
        string fingerprint = Fingerprint(request.SessionId);
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

        GameSession? session = await repository.GetByIdForUpdateAsync(
            request.SessionId,
            cancellationToken
        );
        if (session is null)
            throw new KeyNotFoundException($"Session '{request.SessionId}' was not found.");
        if (session.CreatorPlayerId != request.PlayerId)
            throw new ConflictException("Only the lobby creator can start this session.");
        if (session.Status != "Lobby")
            throw new ConflictException("This session is no longer ready to start.");

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

        IReadOnlyList<DungeonParticipant> participants = session
            .Members.Where(member => member.MemberStatus == "Active")
            .Select(member => new DungeonParticipant(member.HeroId))
            .ToArray();
        if (participants.Count == 0)
            throw new ConflictException("A session requires at least one active hero.");

        DungeonRun run;
        try
        {
            run = await dungeonClient.StartRunAsync(
                request.IdempotencyKey,
                session.Id,
                participants,
                cancellationToken
            );
        }
        catch (Exception exception) when (IsDungeonUnavailable(exception, cancellationToken))
        {
            throw new InvalidOperationException(
                "Dungeon is unavailable. The lobby remains ready to start.",
                exception
            );
        }

        DateTimeOffset now = clock.UtcNow;
        session.Status = "Active";
        session.DungeonRunId = run.Id;
        session.DungeonSeed = run.Seed;
        session.StartedAt = now;
        session.LastActiveAt = now;
        repository.AddTransition(
            new GameSessionTransition
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                SourceStatus = "Lobby",
                TargetStatus = "Active",
                Cause = "DungeonRunCreated",
                Actor = "dungeon",
                OccurredAt = now,
            }
        );
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

    private static bool IsDungeonUnavailable(
        Exception exception,
        CancellationToken cancellationToken
    ) =>
        exception is HttpRequestException
        || exception is TimeoutException
        || (exception is OperationCanceledException && !cancellationToken.IsCancellationRequested);

    private static string Fingerprint(Guid sessionId) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sessionId.ToString("N"))));
}
