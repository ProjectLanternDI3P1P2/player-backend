using Microsoft.AspNetCore.Mvc;
using Player.Infrastructure.Persistence;
using Player.Infrastructure.Persistence.Seeding;

namespace Player.Presentation.Controllers;

/// <summary>Local development utilities. These operations are deliberately unavailable elsewhere.</summary>
[ApiController]
[Route("api/player/v1/development/database")]
public sealed class DevelopmentDatabaseController(
    PlayerDbContext context,
    IWebHostEnvironment environment
) : ControllerBase
{
    /// <summary>Deletes Player data and restores the deterministic demonstration dataset.</summary>
    [HttpPost("reset")]
    public async Task<IActionResult> ResetAndSeedAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        await DataSeeder.SeedAsync(context, cancellationToken);
        return NoContent();
    }
}
