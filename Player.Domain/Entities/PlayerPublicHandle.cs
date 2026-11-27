using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Player.Domain.Entities;

/// <summary>
/// US-PLAYER-06. URL-safe public identifier of a player, derived once from the display name
/// and the player id: <c>{slug}-{first 8 hex digits of the id}</c>. The id suffix keeps
/// handles distinct without a lookup. Migration <c>AddPlayerPublicHandle</c> applies the same
/// rule in SQL to existing players: keep both in sync.
/// </summary>
public static class PlayerPublicHandle
{
    public const int MaximumLength = 32;
    public const string FallbackSlug = "player";
    private const int IdSuffixLength = 8;
    private const int MaximumSlugLength = MaximumLength - IdSuffixLength - 1;

    public static string Create(string displayName, Guid playerId)
    {
        string slug = Slugify(displayName);
        if (slug.Length == 0)
            slug = FallbackSlug;

        return $"{slug}-{playerId.ToString("N")[..IdSuffixLength]}";
    }

    /// <summary>
    /// True when <paramref name="handle"/> only contains lowercase letters, digits and dashes,
    /// within <see cref="MaximumLength"/> characters.
    /// </summary>
    public static bool IsWellFormed([NotNullWhen(true)] string? handle) =>
        !string.IsNullOrEmpty(handle)
        && handle.Length <= MaximumLength
        && handle.All(character => character == '-' || IsSlugCharacter(character));

    private static string Slugify(string displayName)
    {
        string decomposed = displayName.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (char character in decomposed)
        {
            // Combining accents left by the decomposition: "e" + U+0301 becomes "e".
            // Written as numeric constants (U+0300 and U+036F), not as literal characters.
            if (character is >= (char)0x0300 and <= (char)0x036F)
                continue;

            char lowered = char.ToLowerInvariant(character);
            if (IsSlugCharacter(lowered))
                builder.Append(lowered);
            else if (builder.Length > 0 && builder[builder.Length - 1] != '-')
                builder.Append('-');
        }

        string slug = builder.ToString().TrimEnd('-');
        return slug.Length <= MaximumSlugLength ? slug : slug[..MaximumSlugLength].TrimEnd('-');
    }

    private static bool IsSlugCharacter(char character) =>
        character is (>= 'a' and <= 'z') or (>= '0' and <= '9');
}
