using TierList.Application.Common.Models;
using TierList.Application.Features.Admin.DTOs;

namespace TierList.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Succeeded, Guid UserId, IEnumerable<string> Errors)> RegisterUserAsync(
        string userName, string email, string password, CancellationToken cancellationToken = default);

    Task<(bool Succeeded, Guid UserId, string UserName, IList<string> Roles, IEnumerable<string> Errors)> ValidateUserCredentialsAsync(
        string userName, string password, CancellationToken cancellationToken = default);

    Task<IList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserRoleAsync(
        Guid userId, string newRole, CancellationToken cancellationToken = default);

    Task<PaginatedList<UserRoleDto>> GetUsersAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<(bool Succeeded, Guid UserId, string UserName, IList<string> Roles, string NewRefreshToken, string? Error)> RotateRefreshTokenAsync(
        string refreshToken, CancellationToken cancellationToken = default);

    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
