using Player.Application.Abstractions;

namespace Player.Application.Features.HeroUseCase.DeselectHero;

public sealed record DeselectHeroCommand(Guid PlayerId, Guid HeroId) : ICommand;
