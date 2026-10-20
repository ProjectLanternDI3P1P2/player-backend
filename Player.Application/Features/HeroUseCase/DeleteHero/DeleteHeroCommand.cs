using Player.Application.Abstractions;

namespace Player.Application.Features.HeroUseCase.DeleteHero;

/// <summary>US-PLAYER-10: soft-deletes a hero to free one of the player's ten slots.</summary>
public sealed record DeleteHeroCommand(Guid PlayerId, Guid HeroId) : ICommand<DeleteHeroResult>;
