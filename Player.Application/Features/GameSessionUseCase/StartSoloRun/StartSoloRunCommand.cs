using Player.Application.Abstractions;

namespace Player.Application.Features.GameSessionUseCase.StartSoloRun;

public sealed record StartSoloRunCommand(Guid PlayerId, Guid HeroId, Guid IdempotencyKey)
    : ICommand<StartSoloRunResult>;
