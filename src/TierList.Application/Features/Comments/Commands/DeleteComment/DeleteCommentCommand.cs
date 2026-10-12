using MediatR;

namespace TierList.Application.Features.Comments.Commands.DeleteComment;

public record DeleteCommentCommand(Guid Id) : IRequest;
