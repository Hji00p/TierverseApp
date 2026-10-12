namespace TierList.Application.Features.Admin.DTOs;

public record UserRoleDto(
    Guid Id,
    string UserName,
    string? Email,
    IList<string> Roles,
    DateTime CreatedAt);
