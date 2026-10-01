using MediatR;
using Player.Domain.Entities;
using Player.Domain.Exceptions;
using Player.Domain.Repositories;

namespace Player.Application.Features.GameSessionUseCase.GetSessionSnapshot;

public sealed class GetSessionSnapshotQueryHandler(IGameSessionRepository repository)
    : IRequestHandler<GetSessionSnapshotQuery, GameSessionSnapshot>
{
    public async Task<GameSessionSnapshot> Handle(
        GetSessionSnapshotQuery request,
        CancellationToken cancellationToken
    )
    {
        GameSession? session = await repository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null)
            throw new KeyNotFoundException($"Session '{request.SessionId}' was not found.");
        if (!session.Members.Any(member => member.Hero.PlayerId == request.PlayerId))
            throw new ConflictException("You are not a participant in this session.");
        return GameSessionSnapshot.From(session);
    }
}
