using MediatR;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Auth.DTOs;

namespace TierList.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IIdentityService identityService, ITokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, userId, userName, roles, newRefreshToken, error) =
            await _identityService.RotateRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (!succeeded)
        {
            throw new ForbiddenException(error ?? "Invalid or expired refresh token.");
        }

        var accessToken = _tokenService.GenerateAccessToken(userId, userName, roles);

        return new AuthResultDto(userId, userName, accessToken, newRefreshToken, 900, roles);
    }
}
