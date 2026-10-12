using MediatR;
using TierList.Application.Common.Models;
using TierList.Application.Features.Templates.DTOs;

namespace TierList.Application.Features.Templates.Queries.GetTemplates;

public record GetTemplatesQuery(string? Search, int Page = 1, int PageSize = 20) : IRequest<PaginatedList<TemplateBriefDto>>;
