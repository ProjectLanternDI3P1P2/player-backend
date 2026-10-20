using System.Text.Json.Serialization;

namespace Player.Presentation.DTO;

/// <param name="Appearance">Optional: the default appearance is used when omitted.</param>
public sealed record CreateHeroRequest(
    [property: JsonRequired] string Name,
    [property: JsonRequired] string ClassCode,
    [property: JsonRequired] Guid IdempotencyKey,
    HeroAppearanceRequest? Appearance = null
);

/// <param name="HairColor">Must be null when <paramref name="HairStyle"/> is <c>bald</c>.</param>
public sealed record HeroAppearanceRequest(
    [property: JsonRequired] int SkinTone,
    [property: JsonRequired] string HairStyle,
    int? HairColor
);
