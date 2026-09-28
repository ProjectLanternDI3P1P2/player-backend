using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.Application.Features.HeroUseCase.CreateHero;
using Player.Application.Features.HeroUseCase.ListHeroes;
using Player.Presentation.DTO;

namespace Player.Presentation.Controllers;

[ApiController]
[Route("api/v1/players/{playerId:guid}/heroes")]
public sealed class HeroesController(ISender sender) : ControllerBase
{
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
}
