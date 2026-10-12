using TierList.Domain.Enums;

namespace TierList.Application.Features.Templates.DTOs;

public record TemplateTierDto(Guid Id, string Name, string Color, int Order);

public record TemplateItemDto(Guid Id, string ImageUrl, string? Caption, int Order);

public record TemplateDto(
    Guid Id,
    Guid AuthorId,
    string AuthorUserName,
    string Title,
    string? Description,
    TemplateVisibility Visibility,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TemplateTierDto> Tiers,
    IReadOnlyList<TemplateItemDto> Items);

public record TemplateBriefDto(
    Guid Id,
    Guid AuthorId,
    string AuthorUserName,
    string Title,
    string? Description,
    TemplateVisibility Visibility,
    int ItemsCount,
    DateTime CreatedAt);
