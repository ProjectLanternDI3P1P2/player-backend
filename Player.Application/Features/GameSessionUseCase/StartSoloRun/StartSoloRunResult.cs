namespace Player.Application.Features.GameSessionUseCase.StartSoloRun;

public sealed record StartSoloRunResult(
    Guid SessionId,
    SessionHero Hero,
    string State,
    Guid? DungeonRunId,
    string? DungeonSeed,
    string? FailureReason,
    bool AlreadyExists
);

public sealed record SessionHero(Guid Id, string Name, string ClassCode, int Level);
