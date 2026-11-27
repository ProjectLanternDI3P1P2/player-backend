using MediatR;

namespace Player.Application.Features.PlayerUseCase.GetPublicProfile;

/// <summary>US-PLAYER-06. Exactly one of <see cref="PlayerId"/> or <see cref="PublicHandle"/> is set.</summary>
public sealed record GetPublicProfileQuery(Guid? PlayerId, string? PublicHandle)
    : IRequest<PublicProfile?>
{
    public static GetPublicProfileQuery ById(Guid playerId) => new(playerId, null);

    public static GetPublicProfileQuery ByPublicHandle(string publicHandle) =>
        new(null, publicHandle);
}
