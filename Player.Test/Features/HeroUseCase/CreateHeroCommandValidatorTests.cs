using FluentAssertions;
using Player.Application.Features.HeroUseCase.CreateHero;

namespace Player.Test.Features.HeroUseCase;

public sealed class CreateHeroCommandValidatorTests
{
    [Theory]
    [InlineData("Aster")]
    [InlineData("O'Rin")]
    [InlineData("Jean-Luc")]
    [InlineData("Maelle Dawn")]
    public void Validate_ValidHeroName_HasNoNameErrors(string name)
    {
        var validator = new CreateHeroCommandValidator();

        var result = validator.Validate(
            new CreateHeroCommand(Guid.NewGuid(), name, "mage", Guid.NewGuid())
        );

        result.Errors.Should().NotContain(error => error.PropertyName == "Name");
    }

    [Theory]
    [InlineData("12")]
    [InlineData("A1ex")]
    [InlineData(" This name is longer than twenty four letters")]
    public void Validate_InvalidHeroName_ReturnsThePublicValidationMessage(string name)
    {
        var validator = new CreateHeroCommandValidator();

        var result = validator.Validate(
            new CreateHeroCommand(Guid.NewGuid(), name, "mage", Guid.NewGuid())
        );

        result
            .Errors.Should()
            .ContainSingle(error =>
                error.PropertyName == "Name"
                && error.ErrorMessage
                    == "Hero name must contain 3 to 24 letters and may include spaces, hyphens, or apostrophes."
            );
    }

    [Fact]
    public void Validate_EmptyHeroName_ReturnsTheRequiredMessage()
    {
        var validator = new CreateHeroCommandValidator();

        var result = validator.Validate(
            new CreateHeroCommand(Guid.NewGuid(), "", "mage", Guid.NewGuid())
        );

        result
            .Errors.Should()
            .ContainSingle(error =>
                error.PropertyName == "Name" && error.ErrorMessage == "Hero name is required."
            );
    }

    [Theory]
    [InlineData("warrior")]
    [InlineData("Shaman")]
    [InlineData("MAGE")]
    public void Validate_SupportedClassCode_HasNoClassCodeErrors(string classCode)
    {
        var validator = new CreateHeroCommandValidator();

        var result = validator.Validate(
            new CreateHeroCommand(Guid.NewGuid(), "Aster", classCode, Guid.NewGuid())
        );

        result.Errors.Should().NotContain(error => error.PropertyName == "ClassCode");
    }

    [Fact]
    public void Validate_UnsupportedClassCode_ReturnsThePublicValidationMessage()
    {
        var validator = new CreateHeroCommandValidator();

        var result = validator.Validate(
            new CreateHeroCommand(Guid.NewGuid(), "Aster", "rogue", Guid.NewGuid())
        );

        result
            .Errors.Should()
            .ContainSingle(error =>
                error.PropertyName == "ClassCode"
                && error.ErrorMessage == "Hero class must be Warrior, Shaman, or Mage."
            );
    }
}
