using System.Security.Cryptography;
using System.Text;
using MediatR;
using Player.Domain.Entities;
using Player.Domain.Exceptions;
using Player.Domain.Repositories;
using Player.Domain.Services;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Application.Features.HeroUseCase.CreateHero;

public sealed class CreateHeroCommandHandler(IHeroRepository repository, IClock clock)
    : IRequestHandler<CreateHeroCommand, CreateHeroResult>
{
    private const string Scope = "hero.create";
    private const int MaximumHeroesPerPlayer = 10;

    public async Task<CreateHeroResult> Handle(
        CreateHeroCommand request,
        CancellationToken cancellationToken
    )
    {
        string name = NormalizeName(request.Name);
        string classCode = request.ClassCode.Trim().ToLowerInvariant();
        string fingerprint = CreateFingerprint(name, classCode);

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

        PlayerEntity? player = await repository.GetPlayerForUpdateAsync(
            request.PlayerId,
            cancellationToken
        );
        if (player is null)
        {
            throw new KeyNotFoundException($"Player '{request.PlayerId}' was not found.");
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

        if (
            await repository.CountActiveHeroesAsync(request.PlayerId, cancellationToken)
            >= MaximumHeroesPerPlayer
        )
        {
            throw new ConflictException("A player cannot own more than 10 active heroes.");
        }

        if (await repository.HeroNameExistsAsync(request.PlayerId, name, cancellationToken))
        {
            throw new BadRequestException("A hero with this name already exists for this player.");
        }

        HeroClass? heroClass = await repository.GetHeroClassAsync(classCode, cancellationToken);
        if (heroClass is null || classCode is not ("warrior" or "shaman" or "mage"))
        {
            throw new BadRequestException("Hero class must be Warrior, Shaman, or Mage.");
        }

        Skill? firstSkill = await repository.GetFirstSkillAsync(classCode, cancellationToken);
        if (firstSkill is null)
        {
            throw new InvalidOperationException(
                $"Hero class '{classCode}' has no level 1 skill configured."
            );
        }

        DateTimeOffset now = clock.UtcNow;
        var hero = new Hero
        {
            Id = Guid.NewGuid(),
            PlayerId = request.PlayerId,
            Name = name,
            ClassCode = classCode,
            HeroClass = heroClass,
            Level = 1,
            CreatedAt = now,
        };
        hero.HeroSkills.Add(
            new HeroSkill
            {
                HeroId = hero.Id,
                SkillCode = firstSkill.Code,
                Skill = firstSkill,
                UnlockedAt = now,
                IsActive = true,
            }
        );
        repository.AddHero(hero);
        repository.AddIdempotencyKey(
            new IdempotencyKey
            {
                Key = request.IdempotencyKey,
                PlayerId = request.PlayerId,
                Scope = Scope,
                RequestFingerprint = fingerprint,
                ProducedResourceId = hero.Id,
                ExpiresAt = now.AddDays(1),
            }
        );

        return ToResult(hero, false);
    }

    private async Task<CreateHeroResult> GetIdempotentResultAsync(
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

        // The resource ID is recorded only after the command has built the hero, so this lookup is safe.
        Hero? hero = await repository.GetHeroByIdAsync(
            key.ProducedResourceId.Value,
            cancellationToken
        );
        if (hero is null)
        {
            throw new InvalidOperationException(
                "The idempotent hero creation result is no longer available."
            );
        }

        return ToResult(hero, true);
    }

    private static CreateHeroResult ToResult(Hero hero, bool alreadyExists) =>
        new(
            hero.Id,
            hero.Name,
            hero.ClassCode,
            hero.Level,
            hero.HeroClass.BaseHealth + (hero.Endurance * 6),
            hero.CreatedAt,
            hero.HeroSkills.Where(x => x.IsActive).Select(x => x.SkillCode).ToList(),
            alreadyExists
        );

    private static string NormalizeName(string name)
    {
        string normalized = name.Trim();
        if (
            normalized.Length is < 3 or > 24
            || !char.IsLetter(normalized[0])
            || normalized.Any(character =>
                !char.IsLetter(character) && character is not (' ' or '-' or '\'')
            )
        )
        {
            throw new BadRequestException(
                "Hero name must contain 3 to 24 letters and may include spaces, hyphens, or apostrophes."
            );
        }

        return normalized;
    }

    private static string CreateFingerprint(string name, string classCode) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{name}\n{classCode}")));
}
