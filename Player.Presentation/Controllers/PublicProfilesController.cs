using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.Application.Features.PlayerUseCase.GetPublicProfile;
using Player.Presentation.DTO;

namespace Player.Presentation.Controllers;

/// <summary>
/// US-PLAYER-06. Anonymous and cacheable: deliberately does not use ICurrentPlayerResolver,
/// which requires the gateway identity headers and creates players.
/// </summary>
[ApiController]
[Route("api/player/v1/players")]
public sealed class PublicProfilesController(ISender sender) : ControllerBase
{
    private const string FoundCacheControl = "public, max-age=60, stale-while-revalidate=300";
    private const string NotFoundCacheControl = "public, max-age=60";

    [HttpGet("{playerId:guid}/public-profile")]
    [HttpHead("{playerId:guid}/public-profile")]
    public async Task<IActionResult> GetByIdAsync(
        Guid playerId,
        CancellationToken cancellationToken
    )
    {
        PublicProfile? profile = await sender.Send<PublicProfile?>(
            GetPublicProfileQuery.ById(playerId),
            cancellationToken
        );
        return ToResponse(profile);
    }

    [HttpGet("by-handle/{handle}/public-profile")]
    [HttpHead("by-handle/{handle}/public-profile")]
    public async Task<IActionResult> GetByHandleAsync(
        string handle,
        CancellationToken cancellationToken
    )
    {
        PublicProfile? profile = await sender.Send<PublicProfile?>(
            GetPublicProfileQuery.ByPublicHandle(handle),
            cancellationToken
        );
        return ToResponse(profile);
    }

    private IActionResult ToResponse(PublicProfile? profile)
    {
        if (profile is null)
        {
            Response.Headers.CacheControl = NotFoundCacheControl;
            return NotFound();
        }

        Response.Headers.CacheControl = FoundCacheControl;
        return Ok(PublicProfileResponse.From(profile));
    }
}
