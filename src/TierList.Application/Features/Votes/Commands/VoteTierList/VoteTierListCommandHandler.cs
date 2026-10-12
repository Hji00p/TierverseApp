using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Votes.DTOs;
using TierList.Domain.Entities;
using TierList.Domain.Enums;

namespace TierList.Application.Features.Votes.Commands.VoteTierList;

public class VoteTierListCommandHandler : IRequestHandler<VoteTierListCommand, VoteResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public VoteTierListCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<VoteResultDto> Handle(VoteTierListCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new ForbiddenException("You must be logged in to vote.");
        }

        var userId = _currentUserService.UserId.Value;

        var tierListExists = await _context.TierLists
            .AnyAsync(t => t.Id == request.TierListId, cancellationToken);

        if (!tierListExists)
        {
            throw new NotFoundException("TierList", request.TierListId);
        }

        var existingVote = await _context.TierListVotes
            .FirstOrDefaultAsync(v => v.UserId == userId && v.TierListId == request.TierListId, cancellationToken);

        if (request.Value == 0)
        {
            if (existingVote != null)
            {
                _context.TierListVotes.Remove(existingVote);
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
                var newVote = new TierListVote(userId, request.TierListId, voteValue);
                _context.TierListVotes.Add(newVote);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Compute updated vote tallies without storing counters
        var likesCount = await _context.TierListVotes
            .CountAsync(v => v.TierListId == request.TierListId && v.Value == VoteValue.Like, cancellationToken);

        var dislikesCount = await _context.TierListVotes
            .CountAsync(v => v.TierListId == request.TierListId && v.Value == VoteValue.Dislike, cancellationToken);

        int? currentVote = request.Value == 0 ? null : request.Value;

        return new VoteResultDto(likesCount, dislikesCount, currentVote);
    }
}
