using MediatR;

namespace Player.Application.Features.HeroUseCase.GetCombatantSnapshot;

/// <summary>Service-to-service read of what Combat needs to put a hero in a fight.</summary>
public sealed record GetCombatantSnapshotQuery(Guid HeroId) : IRequest<CombatantSnapshot?>;
