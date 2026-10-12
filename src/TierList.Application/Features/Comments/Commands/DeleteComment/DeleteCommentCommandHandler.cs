using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;

namespace TierList.Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCommentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (comment == null)
        {
            throw new NotFoundException("Comment", request.Id);
        }

        var isOwner = _currentUserService.UserId.HasValue && comment.AuthorId == _currentUserService.UserId.Value;
        var isModOrAdmin = _currentUserService.IsInRole("Moderator") || _currentUserService.IsInRole("Admin");

        if (!isOwner && !isModOrAdmin)
        {
            throw new ForbiddenException("You are not allowed to delete this comment.");
        }

        comment.Delete();

        // Also delete all replies to this comment (cascade soft delete)
        var currentParentIds = new List<Guid> { comment.Id };
        while (currentParentIds.Count > 0)
        {
            var childReplies = await _context.Comments
                .Where(c => c.ParentCommentId.HasValue && currentParentIds.Contains(c.ParentCommentId.Value) && !c.IsDeleted)
                .ToListAsync(cancellationToken);

            if (childReplies.Count == 0) break;

            foreach (var child in childReplies)
            {
                child.Delete();
            }

            currentParentIds = childReplies.Select(c => c.Id).ToList();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
