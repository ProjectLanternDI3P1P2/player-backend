using Grpc.Core;
using MediatR;
using Player.Application.Features.HeroUseCase;
using Player.Application.Features.HeroUseCase.GetCombatantSnapshot;
using Player.Application.Features.HeroUseCase.GetHeroAppearance;
using Player.Contracts.V1;

namespace Player.Presentation.Grpc.Services;

/// <summary>
/// Internal gRPC surface through which other services read hero data owned by Player.
/// Never exposed to clients: REST and SignalR stay the public APIs.
/// </summary>
public sealed class PlayerHeroGrpcService(ISender sender) : PlayerHeroService.PlayerHeroServiceBase
{
    public override async Task<HeroAppearanceReply> GetHeroAppearance(
        GetHeroAppearanceRequest request,
        ServerCallContext context
    )
    {
        Guid heroId = ParseHeroId(request.HeroId);

        HeroAppearance appearance =
            await sender.Send(new GetHeroAppearanceQuery(heroId), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Hero not found."));

        return ToReply(heroId, appearance);
    }

    public override async Task<CombatantSnapshotReply> GetCombatantSnapshot(
        GetCombatantSnapshotRequest request,
        ServerCallContext context
    )
    {
        Guid heroId = ParseHeroId(request.HeroId);

        CombatantSnapshot snapshot =
            await sender.Send(new GetCombatantSnapshotQuery(heroId), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Hero not found."));

        var reply = new CombatantSnapshotReply
        {
            HeroId = snapshot.HeroId.ToString(),
            Name = snapshot.Name,
            ClassCode = snapshot.ClassCode,
            Level = snapshot.Level,
            CurrentHp = snapshot.MaxHp,
            MaxHp = snapshot.MaxHp,
            CurrentMana = snapshot.MaxMana,
            MaxMana = snapshot.MaxMana,
            Attack = snapshot.Attack,
            Defense = snapshot.Defense,
            Speed = snapshot.Speed,
            Appearance = ToReply(snapshot.HeroId, snapshot.Appearance),
        };
        foreach (CombatantAbilitySnapshot ability in snapshot.Abilities)
        {
            reply.Abilities.Add(
                new CombatantAbility
                {
                    Code = ability.Code,
                    Name = ability.Name,
                    ManaCost = ability.ManaCost,
                    TargetType = ability.TargetType,
                    Power = ability.Power,
                }
            );
        }

        return reply;
    }

    private static Guid ParseHeroId(string value)
    {
        if (!Guid.TryParse(value, out Guid heroId) || heroId == Guid.Empty)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "hero_id must be a valid, non-empty UUID.")
            );
        }

        return heroId;
    }

    private static HeroAppearanceReply ToReply(Guid heroId, HeroAppearance appearance)
    {
        var reply = new HeroAppearanceReply
        {
            HeroId = heroId.ToString(),
            SkinTone = appearance.SkinTone,
            HairStyle = appearance.HairStyle,
        };
        if (appearance.HairColor is int hairColor)
            reply.HairColor = hairColor;

        return reply;
    }
}
