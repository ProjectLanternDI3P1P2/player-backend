using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.Application.Features.HeroUseCase.ListHeroClasses;

namespace Player.Presentation.Controllers;

[ApiController]
[Route("api/v1/hero-classes")]
public sealed class HeroClassesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<HeroClassSummary> heroClasses = await sender.Send<
            IReadOnlyList<HeroClassSummary>
        >(new ListHeroClassesQuery(), cancellationToken);
        return Ok(heroClasses);
    }
}
