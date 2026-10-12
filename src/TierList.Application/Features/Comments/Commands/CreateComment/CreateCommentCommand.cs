using MediatR;

namespace TierList.Application.Features.Comments.Commands.CreateComment;

public record CreateCommentCommand(Guid TierListId, string Text, Guid? ParentCommentId = null) : IRequest<Guid>;
