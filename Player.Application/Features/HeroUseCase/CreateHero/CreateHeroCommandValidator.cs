using FluentValidation;

namespace Player.Application.Features.HeroUseCase.CreateHero;

public sealed class CreateHeroCommandValidator : AbstractValidator<CreateHeroCommand>
{
    public CreateHeroCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty();
        RuleFor(x => x.Name).NotNull();
        RuleFor(x => x.ClassCode).NotNull();
    }
}
