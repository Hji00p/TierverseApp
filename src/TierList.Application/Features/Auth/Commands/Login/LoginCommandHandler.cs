using FluentValidation.Results;
using MediatR;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Auth.DTOs;

namespace TierList.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IIdentityService identityService, ITokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, userId, userName, roles, errors) = await _identityService.ValidateUserCredentialsAsync(
            request.UserName, request.Password, cancellationToken);

        if (!succeeded)
        {
            var failures = errors.Select(e => new ValidationFailure("Authentication", e));
            throw new ValidationException(failures);
        }

        var accessToken = _tokenService.GenerateAccessToken(userId, userName, roles);
        var refreshToken = await _identityService.GenerateAndSaveRefreshTokenAsync(userId, cancellationToken);

        return new AuthResultDto(userId, userName, accessToken, refreshToken, 900, roles);
    }
}
