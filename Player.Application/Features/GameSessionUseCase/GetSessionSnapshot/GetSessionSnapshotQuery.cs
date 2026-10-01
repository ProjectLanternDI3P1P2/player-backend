using MediatR;

namespace Player.Application.Features.GameSessionUseCase.GetSessionSnapshot;

public sealed record GetSessionSnapshotQuery(Guid PlayerId, Guid SessionId)
    : IRequest<GameSessionSnapshot>;
