using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Player.Application.Features.PlayerUseCase.GetPublicProfile;
using Player.Presentation.Controllers;
using Player.Presentation.DTO;

namespace Player.Test.Presentation.Controllers;

public sealed class PublicProfilesControllerTests
{
    private static readonly Guid PlayerId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly PublicProfile Profile = new(
        PlayerId,
        "maelisrune-00000000",
        "MaelisRune",
        2,
        [new PublicProfileHero("Corvin", "mage", "Mage", 11)],
        new PublicProfileCounters(1, 11, 3)
    );

    [Fact]
    public async Task GetByIdAsync_ProfileFound_ReturnsPublicResponseWithFreshnessPolicy()
    {
        var sender = SenderReturning(Profile);
        PublicProfilesController controller = CreateController(sender.Object);

        IActionResult result = await controller.GetByIdAsync(
            PlayerId,
            TestContext.Current.CancellationToken
        );

        result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
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
        string cacheControl = controller.Response.Headers.CacheControl.ToString();
        cacheControl.Should().Be("public, max-age=60, stale-while-revalidate=300");
        sender.Verify(
            service =>
                service.Send<PublicProfile?>(
                    It.Is<GetPublicProfileQuery>(query =>
                        query.PlayerId == PlayerId && query.PublicHandle == null
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetByIdAsync_ProfileNotFound_ReturnsNotFoundWithShortFreshnessPolicy()
    {
        var sender = SenderReturning(null);
        PublicProfilesController controller = CreateController(sender.Object);

        IActionResult result = await controller.GetByIdAsync(
            PlayerId,
            TestContext.Current.CancellationToken
        );

        result.Should().BeOfType<NotFoundResult>();
        string cacheControl = controller.Response.Headers.CacheControl.ToString();
        cacheControl.Should().Be("public, max-age=60");
    }

    [Fact]
    public async Task GetByHandleAsync_ProfileFound_ForwardsTheRawHandle()
    {
        var sender = SenderReturning(Profile);
        PublicProfilesController controller = CreateController(sender.Object);

        IActionResult result = await controller.GetByHandleAsync(
            "MaelisRune-00000000",
            TestContext.Current.CancellationToken
        );

        result.Should().BeOfType<OkObjectResult>();
        string cacheControl = controller.Response.Headers.CacheControl.ToString();
        cacheControl.Should().Be("public, max-age=60, stale-while-revalidate=300");
        sender.Verify(
            service =>
                service.Send<PublicProfile?>(
                    It.Is<GetPublicProfileQuery>(query =>
                        query.PlayerId == null && query.PublicHandle == "MaelisRune-00000000"
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetByHandleAsync_ProfileNotFound_ReturnsNotFoundWithShortFreshnessPolicy()
    {
        var sender = SenderReturning(null);
        PublicProfilesController controller = CreateController(sender.Object);

        IActionResult result = await controller.GetByHandleAsync(
            "unknown-00000000",
            TestContext.Current.CancellationToken
        );

        result.Should().BeOfType<NotFoundResult>();
        string cacheControl = controller.Response.Headers.CacheControl.ToString();
        cacheControl.Should().Be("public, max-age=60");
    }

    private static Mock<ISender> SenderReturning(PublicProfile? profile)
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(service =>
                service.Send<PublicProfile?>(
                    It.IsAny<GetPublicProfileQuery>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(profile);
        return sender;
    }

    private static PublicProfilesController CreateController(ISender sender) =>
        new(sender)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
}
