namespace TierList.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string userName, IEnumerable<string> roles);
    string GenerateRefreshToken();
}
