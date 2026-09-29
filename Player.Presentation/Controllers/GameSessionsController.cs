using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.Application.Features.GameSessionUseCase.StartSoloRun;
using Player.Presentation.DTO;

namespace Player.Presentation.Controllers;

[ApiController]
[Route("api/v1/players/{playerId:guid}/heroes/{heroId:guid}/sessions")]
public sealed class GameSessionsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> StartSoloAsync(
        Guid playerId,
        Guid heroId,
        StartSoloRunRequest request,
        CancellationToken cancellationToken
    )
    {
        StartSoloRunResult result = await sender.Send<StartSoloRunResult>(
            new StartSoloRunCommand(playerId, heroId, request.IdempotencyKey),
            cancellationToken
        );

        return result.AlreadyExists
            ? Ok(result)
            : Created($"/api/v1/sessions/{result.SessionId}", result);
    }
}
