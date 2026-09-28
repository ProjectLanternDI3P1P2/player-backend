using MediatR;
using Player.Application.Messaging;
using Player.Domain.Repositories;
using PlayerEntity = global::Player.Domain.Entities.Player;

namespace Player.Application.Features.PlayerUseCase.CreatePlayer;

public sealed class CreatePlayerCommandHandler(
    IPlayerRepository playerRepository,
    IMessagePublisher messagePublisher
) : IRequestHandler<CreatePlayerCommand>
{
    public async Task Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        PlayerEntity player = new()
        {
            Id = request.Id,
            Name = request.Name,
            Attack = request.Attack,
            Health = request.Health,
            MaxHealth = request.MaxHealth,
        };

        await playerRepository.AddPlayerAsync(player, cancellationToken);

        await messagePublisher.PublishAsync(
            PlayerCreatedMessageFactory.Create(
                player.Id,
                player.Name,
                player.Attack,
                player.Health,
                player.MaxHealth
            ),
            cancellationToken
        );
    }
}
