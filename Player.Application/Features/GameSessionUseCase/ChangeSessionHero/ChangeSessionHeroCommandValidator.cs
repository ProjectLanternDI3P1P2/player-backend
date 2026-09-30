using FluentValidation;

namespace Player.Application.Features.GameSessionUseCase.ChangeSessionHero;

public sealed class ChangeSessionHeroCommandValidator : AbstractValidator<ChangeSessionHeroCommand>
{
    public ChangeSessionHeroCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.HeroId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty();
    }
}
