using FluentValidation;

namespace Player.Application.Features.GameSessionUseCase.CreateSoloLobby;

public sealed class CreateSoloLobbyCommandValidator : AbstractValidator<CreateSoloLobbyCommand>
{
    public CreateSoloLobbyCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.HeroId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty();
    }
}
