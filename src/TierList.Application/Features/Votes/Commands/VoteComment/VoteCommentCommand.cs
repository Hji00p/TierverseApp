using MediatR;
using TierList.Application.Features.Votes.DTOs;

namespace TierList.Application.Features.Votes.Commands.VoteComment;

public record VoteCommentCommand(Guid CommentId, int Value) : IRequest<VoteResultDto>;
