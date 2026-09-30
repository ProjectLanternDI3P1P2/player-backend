using FluentAssertions;
using Moq;
using Player.Application.Features.HeroUseCase.ListHeroClasses;
using Player.Domain.Entities;
using Player.Domain.Repositories;

namespace Player.Test.Features.HeroUseCase;

public sealed class ListHeroClassesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsClassDescriptionsAndBaseHealth()
    {
        var repository = new Mock<IHeroRepository>();
        repository
            .Setup(x => x.ListHeroClassesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new HeroClass
                {
                    Code = "mage",
                    Label = "Mage",
                    Description = "Master of the arcane.",
                    BaseHealth = 45,
                },
            ]);
        var handler = new ListHeroClassesQueryHandler(repository.Object);

        IReadOnlyList<HeroClassSummary> result = await handler.Handle(
            new ListHeroClassesQuery(),
            TestContext.Current.CancellationToken
        );

        result
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new HeroClassSummary("mage", "Mage", "Master of the arcane.", 45));
    }
}
