using MediatR;
using Player.Domain.Repositories;
using PlayerEntity = global::Player.Domain.Entities.Player;

namespace Player.Application.Features.PlayerUseCase.GetPlayerById;

public sealed class GetPlayerByIdQueryHandler(IPlayerRepository playerRepository)
    : IRequestHandler<GetPlayerByIdQuery, GetPlayerByIdResult>
{
    public async Task<GetPlayerByIdResult> Handle(
        GetPlayerByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        PlayerEntity? player = await playerRepository.GetPlayerByIdAsync(
            request.PlayerId,
            cancellationToken
        );

        return player == null
            ? throw new KeyNotFoundException(
                $"Player not found with PlayerId '{request.PlayerId}'."
            )
            : new GetPlayerByIdResult
            {
                Id = player.Id,
                Name = player.Name,
                Health = player.Health,
                MaxHealth = player.MaxHealth,
                Attack = player.Attack,
            };
    }
}
