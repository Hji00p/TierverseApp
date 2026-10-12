using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Interfaces;
using TierList.Application.Common.Models;
using TierList.Application.Features.Comments.DTOs;
using TierList.Domain.Enums;

namespace TierList.Application.Features.Comments.Queries.GetComments;

public class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, PaginatedList<CommentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCommentsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<CommentDto>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var page = request.Page > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? (request.PageSize > 50 ? 50 : request.PageSize) : 20;

        var rootQuery = _context.Comments
            .AsNoTracking()
            .Where(c => c.TierListId == request.TierListId && c.ParentCommentId == null && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await rootQuery.CountAsync(cancellationToken);

        var rootItems = await rootQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.TierListId,
                c.AuthorId,
                AuthorUserName = c.Author.UserName,
                c.Text,
                c.CreatedAt,
                c.IsDeleted,
                LikesCount = c.Votes.Count(v => v.Value == VoteValue.Like),
                DislikesCount = c.Votes.Count(v => v.Value == VoteValue.Dislike),
                UserVote = currentUserId.HasValue
                    ? c.Votes.Where(v => v.UserId == currentUserId.Value).Select(v => (int?)v.Value).FirstOrDefault()
                    : null
            })
            .ToListAsync(cancellationToken);

        var rootIds = rootItems.Select(r => r.Id).ToList();

        var replies = await _context.Comments
            .AsNoTracking()
            .Where(c => c.TierListId == request.TierListId && c.ParentCommentId.HasValue && rootIds.Contains(c.ParentCommentId.Value) && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.TierListId,
                c.AuthorId,
                AuthorUserName = c.Author.UserName,
                c.Text,
                c.CreatedAt,
                c.IsDeleted,
                c.ParentCommentId,
                LikesCount = c.Votes.Count(v => v.Value == VoteValue.Like),
                DislikesCount = c.Votes.Count(v => v.Value == VoteValue.Dislike),
                UserVote = currentUserId.HasValue
                    ? c.Votes.Where(v => v.UserId == currentUserId.Value).Select(v => (int?)v.Value).FirstOrDefault()
                    : null
            })
            .ToListAsync(cancellationToken);

        var repliesLookup = replies
            .GroupBy(r => r.ParentCommentId!.Value)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<CommentDto>)g.Select(c => new CommentDto(
                    c.Id,
                    c.TierListId,
                    c.AuthorId,
                    c.AuthorUserName,
                    c.Text,
                    c.CreatedAt,
                    c.IsDeleted,
                    c.LikesCount,
                    c.DislikesCount,
                    c.UserVote,
                    c.ParentCommentId,
                    null)).ToList());

        var resultItems = rootItems.Select(c => new CommentDto(
            c.Id,
            c.TierListId,
            c.AuthorId,
            c.AuthorUserName,
            c.Text,
            c.CreatedAt,
            c.IsDeleted,
            c.LikesCount,
            c.DislikesCount,
            c.UserVote,
            null,
            repliesLookup.TryGetValue(c.Id, out var repList) ? repList : new List<CommentDto>()
        )).ToList();

        return new PaginatedList<CommentDto>(resultItems, totalCount, page, pageSize);
    }
}
