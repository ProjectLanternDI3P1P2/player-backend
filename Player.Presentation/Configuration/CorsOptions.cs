namespace Player.Presentation.Configuration;

/// <summary>Trusted browser origins allowed to call REST endpoints and SignalR Hubs.</summary>
public sealed class GameClientCorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; } = [];
}
