using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Player.Application.Features.GameSessionUseCase;
using Player.Application.Features.GameSessionUseCase.CreateSoloLobby;
using Player.Presentation.Hubs;

namespace Player.Test.Presentation.Hubs;

public sealed class PlayerHubTests
{
    [Fact]
    public async Task CreateSoloLobby_AddsTheCallerToTheAuthorizedGroupAndBroadcastsTheSnapshot()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender
            .Setup(service =>
                service.Send<GameSessionSnapshot>(
                    It.IsAny<CreateSoloLobbyCommand>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new GameSessionSnapshot(
                    sessionId,
                    playerId,
                    "Lobby",
                    "Solo",
                    [new SessionHero(heroId, "Merlin", "mage", 1)],
                    null,
                    null,
                    null,
                    false
                )
            );
        var context = new Mock<HubCallerContext>();
        context.SetupGet(value => value.ConnectionId).Returns("connection-1");
        var groups = new Mock<IGroupManager>();
        var clients = new Mock<IHubCallerClients>();
        var proxy = new Mock<IClientProxy>();
        string groupName = $"session:{sessionId}";
        clients.Setup(value => value.Group(groupName)).Returns(proxy.Object);
        var hub = new PlayerHub(sender.Object)
        {
            Context = context.Object,
            Groups = groups.Object,
            Clients = clients.Object,
        };

        SessionCommandAcknowledgement acknowledgement = await hub.CreateSoloLobby(
            new CreateSoloLobbyRequest(Guid.NewGuid(), playerId, heroId)
        );

        acknowledgement.Accepted.Should().BeTrue();
        acknowledgement.Session!.State.Should().Be("Lobby");
        groups.Verify(
            value =>
                value.AddToGroupAsync("connection-1", groupName, It.IsAny<CancellationToken>()),
            Times.Once
        );
        proxy.Verify(
            value =>
                value.SendCoreAsync(
                    nameof(SessionStateChanged),
                    It.Is<object?[]>(arguments => arguments.Length == 1),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
