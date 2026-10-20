namespace Player.Domain.Entities;

/// <summary>
/// Appearance choices offered when a hero is created. Each value maps to a pre-colored layer of
/// the Mana Seed character base: skin tone N is the body sheet <c>0bas_humn_vNN</c>, hair color N
/// is the hair sheet <c>4har_{style}_vNN</c>. Variant 00 (the asset's base ramp) is not offered.
/// The headwear is not chosen: it follows the hero class.
/// </summary>
public static class HeroAppearanceCatalog
{
    public const int MinimumSkinTone = 1;
    public const int MaximumSkinTone = 10;
    public const int MinimumHairColor = 1;
    public const int MaximumHairColor = 13;

    public const string BaldHairStyle = "bald";
    public const string BobHairStyle = "bob1";
    public const string DapperHairStyle = "dap1";

    public const int DefaultSkinTone = 1;
    public const string DefaultHairStyle = BobHairStyle;
    public const int DefaultHairColor = 1;

    public static readonly IReadOnlyList<string> HairStyles =
    [
        BaldHairStyle,
        BobHairStyle,
        DapperHairStyle,
    ];

    public static bool IsSkinToneSupported(int skinTone) =>
        skinTone is >= MinimumSkinTone and <= MaximumSkinTone;

    public static bool IsHairStyleSupported(string? hairStyle) =>
        hairStyle is not null && HairStyles.Contains(hairStyle.Trim().ToLowerInvariant());

    /// <summary>A bald hero has no hair color; any other hair style requires one.</summary>
    public static bool IsHairColorSupported(string? hairStyle, int? hairColor) =>
        string.Equals(hairStyle?.Trim(), BaldHairStyle, StringComparison.OrdinalIgnoreCase)
            ? hairColor is null
            : hairColor is >= MinimumHairColor and <= MaximumHairColor;
}
