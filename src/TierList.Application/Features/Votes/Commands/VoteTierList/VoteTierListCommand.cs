using MediatR;
using TierList.Application.Features.Votes.DTOs;

namespace TierList.Application.Features.Votes.Commands.VoteTierList;

public record VoteTierListCommand(Guid TierListId, int Value) : IRequest<VoteResultDto>;
