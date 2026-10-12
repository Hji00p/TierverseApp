using MediatR;
using TierList.Domain.Enums;

namespace TierList.Application.Features.Templates.Commands.CreateTemplate;

public record CreateTemplateTierModel(string Name, string Color, int Order);
public record CreateTemplateItemModel(string ImageUrl, string? Caption, int Order);

public record CreateTemplateCommand(
    string Title,
    string? Description,
    TemplateVisibility Visibility,
    List<CreateTemplateTierModel> Tiers,
    List<CreateTemplateItemModel> Items) : IRequest<Guid>;
