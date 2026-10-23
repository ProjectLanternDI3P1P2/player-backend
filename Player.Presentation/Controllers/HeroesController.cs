using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Player.Application.Features.GameSessionUseCase;
using Player.Application.Features.HeroUseCase;
using Player.Application.Features.HeroUseCase.CreateHero;
using Player.Application.Features.HeroUseCase.DeleteHero;
using Player.Application.Features.HeroUseCase.DeselectHero;
using Player.Application.Features.HeroUseCase.GetHeroSheet;
using Player.Application.Features.HeroUseCase.ListHeroes;
using Player.Application.Features.HeroUseCase.SelectHero;
using Player.Presentation.Authentication;
using Player.Presentation.DTO;
using Player.Presentation.Hubs;

namespace Player.Presentation.Controllers;

[ApiController]
[Route("api/player/v1/me/heroes")]
[Route("api/player/v1/{playerId:guid}/heroes")]
public sealed class HeroesController(
    ISender sender,
    IHubContext<PlayerHub> playerHub,
    ICurrentPlayerResolver currentPlayer
) : ControllerBase
{
    [HttpGet("{heroId:guid}")]
    public async Task<IActionResult> GetSheetAsync(Guid heroId, CancellationToken cancellationToken)
    {
        Guid currentPlayerId = await ResolvePlayerIdAsync(cancellationToken);
        HeroSheet? hero = await sender.Send<HeroSheet?>(
            new GetHeroSheetQuery(currentPlayerId, heroId),
            cancellationToken
        );
        return hero is null ? NotFound() : Ok(hero);
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        Guid currentPlayerId = await ResolvePlayerIdAsync(cancellationToken);
        IReadOnlyList<HeroSummary> heroes = await sender.Send<IReadOnlyList<HeroSummary>>(
            new ListHeroesQuery(currentPlayerId),
            cancellationToken
        );
        return Ok(heroes);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        CreateHeroRequest request,
        CancellationToken cancellationToken
    )
    {
        Guid currentPlayerId = await ResolvePlayerIdAsync(cancellationToken);
        CreateHeroResult result = await sender.Send<CreateHeroResult>(
            new CreateHeroCommand(
                currentPlayerId,
                request.Name,
                request.ClassCode,
                request.IdempotencyKey,
                request.Appearance is null
                    ? null
                    : new HeroAppearance(
                        request.Appearance.SkinTone,
                        request.Appearance.HairStyle,
                        request.Appearance.HairColor
                    )
            ),
            cancellationToken
        );

        return result.AlreadyExists
            ? Ok(result)
            : Created($"/api/player/v1/me/heroes/{result.Id}", result);
    }

    [HttpPut("{heroId:guid}/selection")]
    public async Task<IActionResult> SelectAsync(Guid heroId, CancellationToken cancellationToken)
    {
        Guid currentPlayerId = await ResolvePlayerIdAsync(cancellationToken);
        await sender.Send(new SelectHeroCommand(currentPlayerId, heroId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{heroId:guid}/selection")]
    public async Task<IActionResult> DeselectAsync(Guid heroId, CancellationToken cancellationToken)
    {
        Guid currentPlayerId = await ResolvePlayerIdAsync(cancellationToken);
        await sender.Send(new DeselectHeroCommand(currentPlayerId, heroId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// US-PLAYER-10. Idempotent: replaying the request on a deleted hero returns 200 without
    /// further effect. A hero in a running dungeon is refused (409); a hero waiting in a lobby
    /// first closes that lobby, whose members are notified through PlayerHub.
    /// </summary>
    [HttpDelete("{heroId:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid heroId, CancellationToken cancellationToken)
    {
        Guid currentPlayerId = await ResolvePlayerIdAsync(cancellationToken);
        DeleteHeroResult result = await sender.Send<DeleteHeroResult>(
            new DeleteHeroCommand(currentPlayerId, heroId),
            cancellationToken
        );

        foreach (GameSessionSnapshot session in result.ClosedSessions)
        {
            await playerHub
                .Clients.Group(PlayerHub.GroupName(session.SessionId))
                .SendAsync(
                    nameof(SessionStateChanged),
                    SessionStateChanged.From(session),
                    // The deletion is committed: notify members even if the caller disconnected.
                    CancellationToken.None
                );
        }

        return Ok(
            new DeleteHeroResponse(
                result.HeroId,
                true,
                result.DeletedAt,
                result.AlreadyDeleted,
                result.ClosedSessions.Select(session => session.SessionId).ToArray()
            )
        );
    }

    private Task<Guid> ResolvePlayerIdAsync(CancellationToken cancellationToken)
    {
        return currentPlayer.ResolveAsync(HttpContext, cancellationToken);
    }
}
