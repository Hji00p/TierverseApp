namespace TierList.Application.Features.TierLists.DTOs;

public record TierDto(
    Guid Id,
    string Name,
    string Color,
    int Order
);

public record TierItemDto(
    Guid Id,
    string ImageUrl,
    string? Caption,
    Guid? TierId,
    int Position
);

public record TierListDto(
    Guid Id,
    string Title,
    string? Description,
    Guid AuthorId,
    string AuthorUserName,
    Guid? TemplateId,
    string? TemplateTitle,
    List<TierDto> Tiers,
    List<TierItemDto> Items,
    int Likes,
    int Dislikes,
    int Score,
    int? UserVote,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record TierListBriefDto(
    Guid Id,
    string Title,
    string? Description,
    Guid AuthorId,
    string AuthorUserName,
    Guid? TemplateId,
    string? TemplateTitle,
    int ItemCount,
    int Likes,
    int Dislikes,
    int Score,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record TierLayoutItemDto(
    Guid TierId,
    IReadOnlyList<Guid> ItemIds
);

public record SaveLayoutDto(
    IReadOnlyList<TierLayoutItemDto> Tiers,
    IReadOnlyList<Guid> UnplacedItemIds
);

public record CreateTierDto(
    string Name,
    string Color,
    int Order,
    Guid? Id = null
);

public record CreateTierItemDto(
    string ImageUrl,
    string? Caption = null,
    Guid? Id = null
);
