namespace TierList.Application.Features.Auth.DTOs;

public record AuthResultDto(
    Guid Id,
    string UserName,
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    IEnumerable<string> Roles);
