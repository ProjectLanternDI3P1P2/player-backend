using System.Text.Json;
using FluentAssertions;
using Player.Application.Features.PlayerUseCase.GetPublicProfile;
using Player.Presentation.DTO;

namespace Player.Test.Presentation.DTO;

public sealed class PublicProfileResponseTests
{
    private static readonly Guid PlayerId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [Fact]
    public void From_PublicProfile_CopiesEveryPublicField()
    {
        var profile = new PublicProfile(
            PlayerId,
            "maelisrune-00000000",
            "MaelisRune",
            2,
            [new PublicProfileHero("Corvin", "mage", "Mage", 11)],
            new PublicProfileCounters(1, 11, 3)
        );

        PublicProfileResponse response = PublicProfileResponse.From(profile);

        response
            .Should()
            .BeEquivalentTo(
                new PublicProfileResponse(
                    PlayerId,
                    "maelisrune-00000000",
                    "MaelisRune",
                    2,
                    [new PublicProfileHeroResponse("Corvin", "mage", "Mage", 11)],
                    new PublicProfileCountersResponse(1, 11, 3)
                )
            );
    }

    [Fact]
    public void Serialize_WithWebDefaults_ExposesOnlyPublicProperties()
    {
        var response = new PublicProfileResponse(
            Guid.NewGuid(),
            "maelisrune-00000000",
            "MaelisRune",
            2,
            [new PublicProfileHeroResponse("Corvin", "mage", "Mage", 11)],
            new PublicProfileCountersResponse(1, 11, 0)
        );

        string json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
        );
        using JsonDocument document = JsonDocument.Parse(json);

        document
            .RootElement.EnumerateObject()
            .Select(property => property.Name)
            .Should()
            .BeEquivalentTo([
                "playerId",
                "publicHandle",
                "displayName",
                "newGamePlusLevel",
                "heroes",
                "counters",
            ]);
        document
            .RootElement.GetProperty("heroes")[0]
            .EnumerateObject()
            .Select(property => property.Name)
            .Should()
            .BeEquivalentTo(["name", "classCode", "className", "level"]);
        document
            .RootElement.GetProperty("counters")
            .EnumerateObject()
            .Select(property => property.Name)
            .Should()
            .BeEquivalentTo(["heroCount", "highestHeroLevel", "completedSessionCount"]);
        json.Should()
            .NotContainEquivalentOf("externalUserId")
            .And.NotContainEquivalentOf("accountStatus")
            .And.NotContainEquivalentOf("email");
    }
}
