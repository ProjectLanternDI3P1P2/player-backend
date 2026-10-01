using System.Security.Cryptography;
using System.Text;
using MediatR;
using Player.Domain.Entities;
using Player.Domain.Exceptions;
using Player.Domain.Repositories;
using Player.Domain.Services;

namespace Player.Application.Features.GameSessionUseCase.ChangeSessionHero;

public sealed class ChangeSessionHeroCommandHandler(IGameSessionRepository repository, IClock clock)
    : IRequestHandler<ChangeSessionHeroCommand, GameSessionSnapshot>
{
    private const string Scope = "game-session.change-hero";

    public async Task<GameSessionSnapshot> Handle(
        ChangeSessionHeroCommand request,
        CancellationToken cancellationToken
    )
    {
        string fingerprint = Fingerprint(request.SessionId, request.HeroId);
        LobbyCommandPreparation preparation = await LobbyCommandPreparation.CreateAsync(
            repository,
            request.IdempotencyKey,
            request.PlayerId,
            request.SessionId,
            Scope,
            fingerprint,
            "Only the lobby creator can change the solo hero.",
            "The roster is locked after the run starts.",
            cancellationToken
        );
        if (preparation.ExistingSnapshot is not null)
            return preparation.ExistingSnapshot;
        GameSession session = preparation.Lobby!;

        GameSessionMember? currentMember = session.Members.SingleOrDefault(member =>
            member.MemberStatus == "Active"
        );
        if (currentMember is null)
            throw new ConflictException("The lobby has no active hero.");

        Hero? hero = await repository.GetActiveHeroForUpdateAsync(
            request.HeroId,
            request.PlayerId,
            cancellationToken
        );
        if (hero is null)
            throw new KeyNotFoundException($"Hero '{request.HeroId}' was not found.");
        if (
            hero.Id != currentMember.HeroId
            && await repository.HasOpenSessionAsync(hero.Id, cancellationToken)
        )
            throw new ConflictException("This hero already has an open session.");

        DateTimeOffset now = clock.UtcNow;
        if (hero.Id != currentMember.HeroId)
        {
            repository.RemoveMember(currentMember);
            session.Members.Remove(currentMember);
            var replacement = new GameSessionMember
            {
                SessionId = session.Id,
                HeroId = hero.Id,
                GameSession = session,
                Hero = hero,
                MemberStatus = "Active",
                JoinedAt = now,
            };
            session.Members.Add(replacement);
            repository.AddMember(replacement);
            repository.AddTransition(
                new GameSessionTransition
                {
                    Id = Guid.NewGuid(),
                    SessionId = session.Id,
                    SourceStatus = "Lobby",
                    TargetStatus = "Lobby",
                    Cause = "HeroChanged",
                    Actor = "player",
                    OccurredAt = now,
                }
            );
            session.LastActiveAt = now;
        }

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

    private static string Fingerprint(Guid sessionId, Guid heroId) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{sessionId:N}:{heroId:N}")));
}
