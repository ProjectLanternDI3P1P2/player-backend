using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.Application.Abstractions;
using Player.Infrastructure.Persistence;

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
        TResponse transactionalResponse = await next(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return transactionalResponse;
    }
}
