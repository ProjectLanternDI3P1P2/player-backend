using FluentValidation;

namespace Player.Application.Features.GameSessionUseCase.StartSoloRun;

public sealed class StartSoloRunCommandValidator : AbstractValidator<StartSoloRunCommand>
{
    public StartSoloRunCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.HeroId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty();
    }
}
