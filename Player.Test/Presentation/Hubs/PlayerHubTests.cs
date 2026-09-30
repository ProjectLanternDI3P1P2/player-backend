using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Player.Application.Features.GameSessionUseCase.StartSoloRun;
using Player.Presentation.Hubs;

namespace Player.Test.Presentation.Hubs;

public sealed class PlayerHubTests
{
    [Fact]
    public async Task CreateSession_AcceptsCommand_AddsAuthorizedGroup_AndBroadcastsSnapshot()
    {
        Guid playerId = Guid.NewGuid();
        Guid heroId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender
            .Setup(service =>
                service.Send<StartSoloRunResult>(
                    It.IsAny<StartSoloRunCommand>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new StartSoloRunResult(
                    sessionId,
                    new SessionHero(heroId, "Merlin", "mage", 1),
                    "Active",
                    Guid.NewGuid(),
                    "seed",
                    null,
                    false
                )
            );
        var context = new Mock<HubCallerContext>();
        context.SetupGet(value => value.ConnectionId).Returns("connection-1");
        var groups = new Mock<IGroupManager>();
        var clients = new Mock<IHubCallerClients>();
        var clientProxy = new Mock<IClientProxy>();
        string groupName = $"session:{sessionId}";
        clients.Setup(value => value.Group(groupName)).Returns(clientProxy.Object);
        var hub = new PlayerHub(sender.Object)
        {
            Context = context.Object,
            Groups = groups.Object,
            Clients = clients.Object,
        };

        SessionCommandAcknowledgement acknowledgement = await hub.CreateSession(
            new CreateSoloSessionCommand(Guid.NewGuid(), playerId, heroId)
        );

        acknowledgement.Accepted.Should().BeTrue();
        acknowledgement.Session!.SessionId.Should().Be(sessionId);
        groups.Verify(
            value =>
                value.AddToGroupAsync("connection-1", groupName, It.IsAny<CancellationToken>()),
            Times.Once
        );
        clientProxy.Verify(
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
