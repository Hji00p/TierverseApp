using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Votes.DTOs;
using TierList.Domain.Entities;
using TierList.Domain.Enums;

namespace TierList.Application.Features.Votes.Commands.VoteComment;

public class VoteCommentCommandHandler : IRequestHandler<VoteCommentCommand, VoteResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public VoteCommentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<VoteResultDto> Handle(VoteCommentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new ForbiddenException("You must be logged in to vote.");
        }

        var userId = _currentUserService.UserId.Value;

        var commentExists = await _context.Comments
            .AnyAsync(c => c.Id == request.CommentId, cancellationToken);

        if (!commentExists)
        {
            throw new NotFoundException("Comment", request.CommentId);
        }

        var existingVote = await _context.CommentVotes
            .FirstOrDefaultAsync(v => v.UserId == userId && v.CommentId == request.CommentId, cancellationToken);

        if (request.Value == 0)
        {
            if (existingVote != null)
            {
                _context.CommentVotes.Remove(existingVote);
            }
        }
        else
        {
            var voteValue = (VoteValue)request.Value;
            if (existingVote != null)
            {
                existingVote.ChangeVote(voteValue);
            }
            else
            {
                var newVote = new CommentVote(userId, request.CommentId, voteValue);
                _context.CommentVotes.Add(newVote);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var likesCount = await _context.CommentVotes
            .CountAsync(v => v.CommentId == request.CommentId && v.Value == VoteValue.Like, cancellationToken);

        var dislikesCount = await _context.CommentVotes
            .CountAsync(v => v.CommentId == request.CommentId && v.Value == VoteValue.Dislike, cancellationToken);

        int? currentVote = request.Value == 0 ? null : request.Value;

        return new VoteResultDto(likesCount, dislikesCount, currentVote);
    }
}
