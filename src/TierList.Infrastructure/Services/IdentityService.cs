using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Interfaces;
using TierList.Application.Common.Models;
using TierList.Application.Features.Admin.DTOs;
using TierList.Domain.Entities;
using TierList.Infrastructure.Data;
using TierList.Infrastructure.Identity;

namespace TierList.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext dbContext,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<(bool Succeeded, Guid UserId, IEnumerable<string> Errors)> RegisterUserAsync(
        string userName, string email, string password, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByNameAsync(userName);
        if (existingUser != null)
        {
            return (false, Guid.Empty, new[] { "Username is already taken." });
        }

        var existingEmail = await _userManager.FindByEmailAsync(email);
        if (existingEmail != null)
        {
            return (false, Guid.Empty, new[] { "Email is already registered." });
        }

        var appUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(appUser, password);
        if (!result.Succeeded)
        {
            return (false, Guid.Empty, result.Errors.Select(e => e.Description));
        }

        await _userManager.AddToRoleAsync(appUser, "User");

        // Create Domain User entity with same Id
        var domainUser = new User(appUser.Id, userName);
        _dbContext.Users.Add(domainUser);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (true, appUser.Id, Array.Empty<string>());
    }

    public async Task<(bool Succeeded, Guid UserId, string UserName, IList<string> Roles, IEnumerable<string> Errors)> ValidateUserCredentialsAsync(
        string userName, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return (false, Guid.Empty, string.Empty, Array.Empty<string>(), new[] { "Invalid username or password." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return (false, Guid.Empty, string.Empty, Array.Empty<string>(), new[] { "Invalid username or password." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        return (true, user.Id, user.UserName ?? userName, roles, Array.Empty<string>());
    }

    public async Task<IList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Array.Empty<string>();
        }

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserRoleAsync(
        Guid userId, string newRole, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return (false, new[] { "User not found." });
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            return (false, removeResult.Errors.Select(e => e.Description));
        }

        var addResult = await _userManager.AddToRoleAsync(user, newRole);
        if (!addResult.Succeeded)
        {
            return (false, addResult.Errors.Select(e => e.Description));
        }

        return (true, Array.Empty<string>());
    }

    public async Task<PaginatedList<UserRoleDto>> GetUsersAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            query = query.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(searchTerm)) ||
                (u.Email != null && u.Email.ToLower().Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var p = page > 0 ? page : 1;
        var ps = pageSize > 0 ? (pageSize > 50 ? 50 : pageSize) : 20;

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((p - 1) * ps)
            .Take(ps)
            .ToListAsync(cancellationToken);

        var list = new List<UserRoleDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            list.Add(new UserRoleDto(user.Id, user.UserName ?? string.Empty, user.Email, roles, user.CreatedAt));
        }

        return new PaginatedList<UserRoleDto>(list, totalCount, p, ps);
    }

    public async Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var token = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(14),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return token;
    }

    public async Task<(bool Succeeded, Guid UserId, string UserName, IList<string> Roles, string NewRefreshToken, string? Error)> RotateRefreshTokenAsync(
        string refreshToken, CancellationToken cancellationToken = default)
    {
        var existingToken = await _dbContext.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (existingToken == null || !existingToken.IsActive)
        {
            return (false, Guid.Empty, string.Empty, Array.Empty<string>(), string.Empty, "Invalid or expired refresh token.");
        }

        // Revoke the current token
        existingToken.RevokedAt = DateTime.UtcNow;

        // Generate and save new rotated refresh token
        var newToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = existingToken.UserId,
            Token = newToken,
            ExpiresAt = DateTime.UtcNow.AddDays(14),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(newRefreshTokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(existingToken.User);

        return (true, existingToken.User.Id, existingToken.User.UserName ?? string.Empty, roles, newToken, null);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (token != null && token.RevokedAt == null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
