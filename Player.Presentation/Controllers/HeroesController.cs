using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.Application.Features.HeroUseCase.CreateHero;
using Player.Presentation.DTO;

namespace Player.Presentation.Controllers;

[ApiController]
[Route("api/v1/players/{playerId:guid}/heroes")]
public sealed class HeroesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid playerId,
        CreateHeroRequest request,
        CancellationToken cancellationToken
    )
    {
        CreateHeroResult result = await sender.Send<CreateHeroResult>(
            new CreateHeroCommand(playerId, request.Name, request.ClassCode, request.IdempotencyKey),
            cancellationToken
        );

        return result.AlreadyExists
            ? Ok(result)
            : Created($"/api/v1/players/{playerId}/heroes/{result.Id}", result);
    }
}
