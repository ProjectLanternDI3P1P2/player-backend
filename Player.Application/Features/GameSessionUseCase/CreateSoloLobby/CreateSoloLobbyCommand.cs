using Player.Application.Abstractions;

namespace Player.Application.Features.GameSessionUseCase.CreateSoloLobby;

public sealed record CreateSoloLobbyCommand(
    Guid PlayerId,
    Guid HeroId,
    string? DungeonSeed,
    Guid IdempotencyKey
) : ICommand<GameSessionSnapshot>;
