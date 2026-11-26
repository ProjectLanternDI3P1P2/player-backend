using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Player.Application.Abstractions;
using Player.Domain.Entities;
using Player.Domain.Enums;
using Player.Domain.Exceptions;
using Player.Infrastructure.Persistence;
using Player.Infrastructure.Persistence.Configurations;

namespace Player.Infrastructure.PipelineBehavior;

public sealed class CommandTransactionBehavior<TRequest, TResponse>(PlayerDbContext dbContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);

        if (request is not ICommand)
        {
            return await next(cancellationToken);
        }

        if (!dbContext.Database.IsRelational())
        {
            TResponse response = await next(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return response;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        try
        {
            TResponse transactionalResponse = await next(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return transactionalResponse;
        }
        catch (DbUpdateException exception) when (IsActiveHeroViolation(exception))
        {
            // Backstop of the persisted constraint: a concurrent command won the race.
            throw await ToHeroAlreadyInSessionAsync(exception, transaction, cancellationToken);
        }
    }

    private static bool IsActiveHeroViolation(DbUpdateException exception) =>
        exception.InnerException
            is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: GameSessionMemberConfiguration.ActiveHeroIndexName
            };

    private async Task<Exception> ToHeroAlreadyInSessionAsync(
        DbUpdateException exception,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken
    )
    {
        Guid? heroId = exception
            .Entries.Select(entry => entry.Entity)
            .OfType<GameSessionMember>()
            .Where(member => member.MemberStatus == GameSessionMemberStatus.Active)
            .Select(member => (Guid?)member.HeroId)
            .FirstOrDefault();
        await transaction.RollbackAsync(cancellationToken);
        if (heroId is null)
            return exception;

        dbContext.ChangeTracker.Clear();
        Guid? activeSessionId = await dbContext
            .GameSessionMembers.Where(member =>
                member.HeroId == heroId && member.MemberStatus == GameSessionMemberStatus.Active
            )
            .Select(member => (Guid?)member.SessionId)
            .FirstOrDefaultAsync(cancellationToken);
        return activeSessionId is null
            ? exception
            : new HeroAlreadyInSessionException(heroId.Value, activeSessionId.Value);
    }
}
