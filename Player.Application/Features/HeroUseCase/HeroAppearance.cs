using Player.Domain.Entities;

namespace Player.Application.Features.HeroUseCase;

/// <summary>Cosmetic choices made at hero creation (skin tone, hair style, hair color).</summary>
/// <param name="HairColor">Null when <paramref name="HairStyle"/> is <c>bald</c>.</param>
public sealed record HeroAppearance(int SkinTone, string HairStyle, int? HairColor)
{
    public static HeroAppearance Default { get; } =
        new(
            HeroAppearanceCatalog.DefaultSkinTone,
            HeroAppearanceCatalog.DefaultHairStyle,
            HeroAppearanceCatalog.DefaultHairColor
        );

    public static HeroAppearance From(Hero hero) => new(hero.SkinTone, hero.HairStyle, hero.HairColor);

    public bool IsSupported =>
        HeroAppearanceCatalog.IsSkinToneSupported(SkinTone)
        && HeroAppearanceCatalog.IsHairStyleSupported(HairStyle)
        && HeroAppearanceCatalog.IsHairColorSupported(HairStyle, HairColor);

    public HeroAppearance Normalize() => this with { HairStyle = HairStyle.Trim().ToLowerInvariant() };
}
