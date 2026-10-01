using Player.Application.Abstractions;

namespace Player.Application.Features.GameSessionUseCase.StartSession;

public sealed record StartSessionCommand(Guid PlayerId, Guid SessionId, Guid IdempotencyKey)
    : ICommand<GameSessionSnapshot>;
