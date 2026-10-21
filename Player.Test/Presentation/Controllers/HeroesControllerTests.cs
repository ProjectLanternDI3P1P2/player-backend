using MediatR;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Player.Application.Features.HeroUseCase;
using Player.Application.Features.HeroUseCase.ListHeroes;
using Player.Presentation.Authentication;
using Player.Presentation.Controllers;
using Player.Presentation.Hubs;

namespace Player.Test.Presentation.Controllers;

public sealed class HeroesControllerTests
{
    [Fact]
    public async Task List_UsesTheAuthenticatedPlayer()
    {
        Guid authenticatedPlayerId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender
            .Setup(service =>
                service.Send<IReadOnlyList<HeroSummary>>(
                    It.IsAny<ListHeroesQuery>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);
        var currentPlayer = new Mock<ICurrentPlayerResolver>();
        currentPlayer
            .Setup(resolver =>
                resolver.ResolveAsync(It.IsAny<HttpContext?>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(authenticatedPlayerId);
        var controller = new HeroesController(
            sender.Object,
            Mock.Of<IHubContext<PlayerHub>>(),
            currentPlayer.Object
        )
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };

        IActionResult result = await controller.ListAsync(TestContext.Current.CancellationToken);

        result.Should().BeOfType<OkObjectResult>();
        sender.Verify(
            service =>
                service.Send<IReadOnlyList<HeroSummary>>(
                    It.Is<ListHeroesQuery>(query => query.PlayerId == authenticatedPlayerId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
