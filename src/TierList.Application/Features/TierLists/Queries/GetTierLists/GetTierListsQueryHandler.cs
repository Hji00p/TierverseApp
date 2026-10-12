using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Interfaces;
using TierList.Application.Common.Models;
using TierList.Application.Features.TierLists.DTOs;
using TierList.Domain.Enums;

namespace TierList.Application.Features.TierLists.Queries.GetTierLists;

public class GetTierListsQueryHandler : IRequestHandler<GetTierListsQuery, PaginatedList<TierListBriefDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTierListsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<TierListBriefDto>> Handle(GetTierListsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TierLists
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.Trim().ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(searchLower));
        }

        var projected = query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TierListBriefDto(
                t.Id,
                t.Title,
                t.Description,
                t.AuthorId,
                t.Author.UserName,
                t.TemplateId,
                t.Template != null ? t.Template.Title : null,
                t.Items.Count,
                t.Votes.Count(v => v.Value == VoteValue.Like),
                t.Votes.Count(v => v.Value == VoteValue.Dislike),
                t.Votes.Count(v => v.Value == VoteValue.Like) - t.Votes.Count(v => v.Value == VoteValue.Dislike),
                t.CreatedAt,
                t.UpdatedAt));

        var page = request.Page > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? (request.PageSize > 50 ? 50 : request.PageSize) : 20;

        return await PaginatedList<TierListBriefDto>.CreateAsync(projected, page, pageSize, cancellationToken);
    }
}
