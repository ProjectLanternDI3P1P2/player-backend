using FluentValidation;

namespace Player.Application.Features.GameSessionUseCase.StartSession;

public sealed class StartSessionCommandValidator : AbstractValidator<StartSessionCommand>
{
    public StartSessionCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty();
    }
}
