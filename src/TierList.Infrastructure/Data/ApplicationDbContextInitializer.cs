using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TierList.Domain.Entities;
using TierList.Infrastructure.Identity;

namespace TierList.Infrastructure.Data;

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _configuration;

    public ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsNpgsql())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // 1. Seed Roles: User, Moderator, Admin
        var roles = new[] { "User", "Moderator", "Admin" };
        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                _logger.LogInformation("Role '{RoleName}' created.", roleName);
            }
        }

        // 2. Seed Admin User from configuration
        var adminUserName = _configuration["AdminSettings:UserName"] ?? "admin";
        var adminEmail = _configuration["AdminSettings:Email"] ?? "admin@tierverse.local";
        var adminPassword = _configuration["AdminSettings:Password"];

        if (!string.IsNullOrWhiteSpace(adminPassword))
        {
            var adminUser = await _userManager.FindByNameAsync(adminUserName);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = adminUserName,
                    Email = adminEmail,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRolesAsync(adminUser, new[] { "Admin", "Moderator", "User" });

                    // Ensure domain user exists with same Id
                    var domainUser = await _context.Users.FindAsync(adminUser.Id);
                    if (domainUser == null)
                    {
                        _context.Users.Add(new User(adminUser.Id, adminUserName));
                        await _context.SaveChangesAsync();
                    }

                    _logger.LogInformation("Admin user '{UserName}' seeded successfully.", adminUserName);
                }
                else
                {
                    _logger.LogWarning("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
