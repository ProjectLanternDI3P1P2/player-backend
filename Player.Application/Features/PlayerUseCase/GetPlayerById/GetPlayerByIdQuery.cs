using MediatR;

namespace Player.Application.Features.PlayerUseCase.GetPlayerById;

public record GetPlayerByIdQuery(Guid PlayerId) : IRequest<GetPlayerByIdResult>;
