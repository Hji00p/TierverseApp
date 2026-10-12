using MediatR;
using TierList.Application.Features.Templates.DTOs;

namespace TierList.Application.Features.Templates.Queries.GetTemplateById;

public record GetTemplateByIdQuery(Guid Id) : IRequest<TemplateDto>;
