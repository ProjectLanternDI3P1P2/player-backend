using System.Text.Json.Serialization;

namespace Player.Presentation.DTO;

public sealed record CreateHeroRequest(
    [property: JsonRequired] string Name,
    [property: JsonRequired] string ClassCode,
    [property: JsonRequired] Guid IdempotencyKey
);
