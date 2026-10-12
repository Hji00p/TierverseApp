using MediatR;
using TierList.Application.Features.Templates.Commands.CreateTemplate;
using TierList.Domain.Enums;

namespace TierList.Application.Features.Templates.Commands.UpdateTemplate;

public record UpdateTemplateCommand(
    Guid Id,
    string Title,
    string? Description,
    TemplateVisibility Visibility,
    List<CreateTemplateTierModel> Tiers,
    List<CreateTemplateItemModel> Items) : IRequest;
