using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Domain.Entities;

namespace TierList.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateCommentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new ForbiddenException("You must be logged in to comment.");
        }

        var tierListExists = await _context.TierLists
            .AnyAsync(t => t.Id == request.TierListId, cancellationToken);

        if (!tierListExists)
        {
            throw new NotFoundException("TierList", request.TierListId);
        }

        if (request.ParentCommentId.HasValue)
        {
            var parent = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == request.ParentCommentId.Value, cancellationToken);

            if (parent == null)
            {
                throw new NotFoundException("Comment", request.ParentCommentId.Value);
            }

            if (parent.TierListId != request.TierListId)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure("ParentCommentId", "Parent comment does not belong to this tier list.")
                });
            }
        }

        var authorId = _currentUserService.UserId.Value;
        var comment = new Comment(request.TierListId, authorId, request.Text, request.ParentCommentId);

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}
