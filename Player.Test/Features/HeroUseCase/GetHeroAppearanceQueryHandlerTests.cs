using FluentAssertions;
using Moq;
using Player.Application.Features.HeroUseCase;
using Player.Application.Features.HeroUseCase.GetHeroAppearance;
using Player.Domain.Entities;
using Player.Domain.Repositories;

namespace Player.Test.Features.HeroUseCase;

public sealed class GetHeroAppearanceQueryHandlerTests
{
    [Fact]
    public async Task Handle_KnownHero_ReturnsTheChosenAppearance()
    {
        Guid heroId = Guid.NewGuid();
        Mock<IHeroRepository> repository = RepositoryReturning(
            heroId,
            new Hero
            {
                Id = heroId,
                SkinTone = 4,
                HairStyle = "dap1",
                HairColor = 7,
            }
        );
        var handler = new GetHeroAppearanceQueryHandler(repository.Object);

        HeroAppearance? result = await handler.Handle(
            new GetHeroAppearanceQuery(heroId),
            TestContext.Current.CancellationToken
        );

        result.Should().Be(new HeroAppearance(4, "dap1", 7));
    }

    [Fact]
    public async Task Handle_BaldHero_ReturnsNoHairColor()
    {
        Guid heroId = Guid.NewGuid();
        Mock<IHeroRepository> repository = RepositoryReturning(
            heroId,
            new Hero
            {
                Id = heroId,
                SkinTone = 2,
                HairStyle = "bald",
                HairColor = null,
            }
        );
        var handler = new GetHeroAppearanceQueryHandler(repository.Object);

        HeroAppearance? result = await handler.Handle(
            new GetHeroAppearanceQuery(heroId),
            TestContext.Current.CancellationToken
        );

        result.Should().Be(new HeroAppearance(2, "bald", null));
    }

    [Fact]
    public async Task Handle_HeroNeverCustomized_ReturnsTheDefaultAppearance()
    {
        Guid heroId = Guid.NewGuid();
        Mock<IHeroRepository> repository = RepositoryReturning(heroId, new Hero { Id = heroId });
        var handler = new GetHeroAppearanceQueryHandler(repository.Object);

        HeroAppearance? result = await handler.Handle(
            new GetHeroAppearanceQuery(heroId),
            TestContext.Current.CancellationToken
        );

        result.Should().Be(HeroAppearance.Default);
    }

    [Fact]
    public async Task Handle_DeletedHero_StillReturnsItsAppearance()
    {
        Guid heroId = Guid.NewGuid();
        Mock<IHeroRepository> repository = RepositoryReturning(
            heroId,
            new Hero
            {
                Id = heroId,
                IsDeleted = true,
                SkinTone = 3,
                HairStyle = "bob1",
                HairColor = 5,
            }
        );
        var handler = new GetHeroAppearanceQueryHandler(repository.Object);

        HeroAppearance? result = await handler.Handle(
            new GetHeroAppearanceQuery(heroId),
            TestContext.Current.CancellationToken
        );

        result.Should().Be(new HeroAppearance(3, "bob1", 5));
    }

    [Fact]
    public async Task Handle_UnknownHero_ReturnsNull()
    {
        var repository = new Mock<IHeroRepository>();
        repository
            .Setup(x => x.GetHeroByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hero?)null);
        var handler = new GetHeroAppearanceQueryHandler(repository.Object);

        HeroAppearance? result = await handler.Handle(
            new GetHeroAppearanceQuery(Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.Should().BeNull();
    }

    private static Mock<IHeroRepository> RepositoryReturning(Guid heroId, Hero hero)
    {
        var repository = new Mock<IHeroRepository>();
        repository
            .Setup(x => x.GetHeroByIdAsync(heroId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hero);
        return repository;
    }
}
