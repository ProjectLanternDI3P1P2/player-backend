using Grpc.Core;
using MediatR;
using Player.Application.Features.HeroUseCase;
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
        if (!Guid.TryParse(request.HeroId, out Guid heroId) || heroId == Guid.Empty)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "hero_id must be a valid, non-empty UUID.")
            );
        }

        HeroAppearance appearance =
            await sender.Send(new GetHeroAppearanceQuery(heroId), context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Hero not found."));

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
