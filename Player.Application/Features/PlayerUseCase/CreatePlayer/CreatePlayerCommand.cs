using Player.Application.Abstractions;

namespace Player.Application.Features.PlayerUseCase.CreatePlayer;

public record CreatePlayerCommand(Guid Id, string Name, int Attack, int Health, int MaxHealth)
    : ICommand;
