using Player.Application.Abstractions;

namespace Player.Application.Features.HeroUseCase.SelectHero;

public sealed record SelectHeroCommand(Guid PlayerId, Guid HeroId) : ICommand;
