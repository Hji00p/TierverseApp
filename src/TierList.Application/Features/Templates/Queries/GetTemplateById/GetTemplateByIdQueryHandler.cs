using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Templates.DTOs;

namespace TierList.Application.Features.Templates.Queries.GetTemplateById;

public class GetTemplateByIdQueryHandler : IRequestHandler<GetTemplateByIdQuery, TemplateDto>
{
    private readonly IApplicationDbContext _context;

    public GetTemplateByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TemplateDto> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _context.Templates
            .AsNoTracking()
            .Include(t => t.Author)
            .Include(t => t.Tiers)
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null)
        {
            throw new NotFoundException("Template", request.Id);
        }

        var tierDtos = template.Tiers
            .OrderBy(t => t.Order)
            .Select(t => new TemplateTierDto(t.Id, t.Name, t.Color, t.Order))
            .ToList();

        var itemDtos = template.Items
            .OrderBy(i => i.Order)
            .Select(i => new TemplateItemDto(i.Id, i.ImageUrl, i.Caption, i.Order))
            .ToList();

        return new TemplateDto(
            template.Id,
            template.AuthorId,
            template.Author.UserName,
            template.Title,
            template.Description,
            template.Visibility,
            template.CreatedAt,
            template.UpdatedAt,
            tierDtos,
            itemDtos);
    }
}
