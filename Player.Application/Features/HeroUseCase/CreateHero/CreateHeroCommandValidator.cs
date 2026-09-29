using FluentValidation;

namespace Player.Application.Features.HeroUseCase.CreateHero;

public sealed class CreateHeroCommandValidator : AbstractValidator<CreateHeroCommand>
{
    public CreateHeroCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty();
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Hero name is required.")
            .Must(IsValidHeroName)
            .WithMessage(
                "Hero name must contain 3 to 24 letters and may include spaces, hyphens, or apostrophes."
            );
        RuleFor(x => x.ClassCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Hero class is required.")
            .Must(IsSupportedClassCode)
            .WithMessage("Hero class must be Warrior, Shaman, or Mage.");
    }

    private static bool IsValidHeroName(string name)
    {
        string normalized = name.Trim();
        return normalized.Length is >= 3 and <= 24
            && char.IsLetter(normalized[0])
            && normalized.All(character =>
                char.IsLetter(character) || character is ' ' or '-' or '\''
            );
    }

    private static bool IsSupportedClassCode(string classCode) =>
        classCode.Trim().ToLowerInvariant() is "warrior" or "shaman" or "mage";
}
