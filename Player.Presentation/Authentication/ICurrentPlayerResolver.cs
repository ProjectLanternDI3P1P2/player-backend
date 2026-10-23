namespace Player.Presentation.Authentication;

/// <summary>
/// Resolves the Player aggregate belonging to the identity asserted by the gateway.
/// </summary>
public interface ICurrentPlayerResolver
{
    Task<Guid> ResolveAsync(HttpContext? context, CancellationToken cancellationToken);
}
