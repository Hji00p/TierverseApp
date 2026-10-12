using FluentValidation.Results;
using MediatR;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Auth.DTOs;

namespace TierList.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(IIdentityService identityService, ITokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, userId, errors) = await _identityService.RegisterUserAsync(
            request.UserName, request.Email, request.Password, cancellationToken);

        if (!succeeded)
        {
            var failures = errors.Select(e => new ValidationFailure("Registration", e));
            throw new ValidationException(failures);
        }

        var roles = await _identityService.GetUserRolesAsync(userId, cancellationToken);
        var accessToken = _tokenService.GenerateAccessToken(userId, request.UserName, roles);
        var refreshToken = await _identityService.GenerateAndSaveRefreshTokenAsync(userId, cancellationToken);

        return new AuthResultDto(userId, request.UserName, accessToken, refreshToken, 900, roles);
    }
}
