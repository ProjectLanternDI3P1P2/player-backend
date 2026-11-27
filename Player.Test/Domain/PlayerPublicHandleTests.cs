using FluentAssertions;
using Player.Domain.Entities;

namespace Player.Test.Domain;

public sealed class PlayerPublicHandleTests
{
    private static readonly Guid PlayerId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [Fact]
    public void Create_SimpleDisplayName_ReturnsSlugAndIdPrefix()
    {
        string handle = PlayerPublicHandle.Create("MaelisRune", PlayerId);

        handle.Should().Be("maelisrune-00000000");
    }

    [Fact]
    public void Create_AccentsAndSpecialCharacters_StripsAccentsAndCollapsesDashes()
    {
        string handle = PlayerPublicHandle.Create(
            "  Élodie  du_Bois!!",
            Guid.Parse("abcdef12-3456-7890-abcd-ef1234567890")
        );

        handle.Should().Be("elodie-du-bois-abcdef12");
    }

    [Fact]
    public void Create_NoSlugCharacter_UsesPlayerFallback()
    {
        string handle = PlayerPublicHandle.Create("日本語 !!", PlayerId);

        handle.Should().Be("player-00000000");
    }

    [Fact]
    public void Create_LongDisplayName_TruncatesSlugToFitMaximumLength()
    {
        string handle = PlayerPublicHandle.Create("abcdefghijklmnopqrstuvwxyz0123456789", PlayerId);

        handle.Should().Be("abcdefghijklmnopqrstuvw-00000000");
        handle.Should().HaveLength(32);
    }

    [Fact]
    public void Create_TruncationEndingOnDash_DropsTrailingDash()
    {
        string handle = PlayerPublicHandle.Create("abcdefghijklmnopqrstuv wxyz", PlayerId);

        handle.Should().Be("abcdefghijklmnopqrstuv-00000000");
    }

    [Theory]
    [InlineData("maelisrune-00000000", true)]
    [InlineData("player-1", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("MaelisRune", false)]
    [InlineData("with space", false)]
    [InlineData("é-1", false)]
    [InlineData("abcdefghijklmnopqrstuvwxyz0123456", false)]
    public void IsWellFormed_Handle_ReturnsExpected(string? handle, bool expected)
    {
        bool result = PlayerPublicHandle.IsWellFormed(handle);

        result.Should().Be(expected);
    }
}
