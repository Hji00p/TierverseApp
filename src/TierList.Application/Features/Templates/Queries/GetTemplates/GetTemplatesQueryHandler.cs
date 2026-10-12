using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Interfaces;
using TierList.Application.Common.Models;
using TierList.Application.Features.Templates.DTOs;
using TierList.Domain.Enums;

namespace TierList.Application.Features.Templates.Queries.GetTemplates;

public class GetTemplatesQueryHandler : IRequestHandler<GetTemplatesQuery, PaginatedList<TemplateBriefDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTemplatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<TemplateBriefDto>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Templates
            .AsNoTracking()
            .Where(t => t.Visibility == TemplateVisibility.Public);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.Trim().ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(searchLower));
        }

        var projected = query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TemplateBriefDto(
                t.Id,
                t.AuthorId,
                t.Author.UserName,
                t.Title,
                t.Description,
                t.Visibility,
                t.Items.Count,
                t.CreatedAt));

        var page = request.Page > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? (request.PageSize > 50 ? 50 : request.PageSize) : 20;

        return await PaginatedList<TemplateBriefDto>.CreateAsync(projected, page, pageSize, cancellationToken);
    }
}
