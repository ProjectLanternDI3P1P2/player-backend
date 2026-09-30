using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.Application.Features.HeroUseCase.CreateHero;
using Player.Application.Features.HeroUseCase.DeselectHero;
using Player.Application.Features.HeroUseCase.GetHeroSheet;
using Player.Application.Features.HeroUseCase.ListHeroes;
using Player.Application.Features.HeroUseCase.SelectHero;
using Player.Presentation.DTO;

namespace Player.Presentation.Controllers;

[ApiController]
[Route("api/v1/players/{playerId:guid}/heroes")]
public sealed class HeroesController(ISender sender) : ControllerBase
{
    [HttpGet("{heroId:guid}")]
    public async Task<IActionResult> GetSheetAsync(
        Guid playerId,
        Guid heroId,
        CancellationToken cancellationToken
    )
    {
        HeroSheet? hero = await sender.Send<HeroSheet?>(
            new GetHeroSheetQuery(playerId, heroId),
            cancellationToken
        );
        return hero is null ? NotFound() : Ok(hero);
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(Guid playerId, CancellationToken cancellationToken)
    {
        IReadOnlyList<HeroSummary> heroes = await sender.Send<IReadOnlyList<HeroSummary>>(
            new ListHeroesQuery(playerId),
            cancellationToken
        );
        return Ok(heroes);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid playerId,
        CreateHeroRequest request,
        CancellationToken cancellationToken
    )
    {
        CreateHeroResult result = await sender.Send<CreateHeroResult>(
            new CreateHeroCommand(
                playerId,
                request.Name,
                request.ClassCode,
                request.IdempotencyKey
            ),
            cancellationToken
        );

        return result.AlreadyExists
            ? Ok(result)
            : Created($"/api/v1/players/{playerId}/heroes/{result.Id}", result);
    }

    [HttpPut("{heroId:guid}/selection")]
    public async Task<IActionResult> SelectAsync(
        Guid playerId,
        Guid heroId,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(new SelectHeroCommand(playerId, heroId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{heroId:guid}/selection")]
    public async Task<IActionResult> DeselectAsync(
        Guid playerId,
        Guid heroId,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(new DeselectHeroCommand(playerId, heroId), cancellationToken);
        return NoContent();
    }
}
