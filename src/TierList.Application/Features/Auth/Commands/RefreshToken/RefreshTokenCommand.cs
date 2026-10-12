using MediatR;
using TierList.Application.Features.Auth.DTOs;

namespace TierList.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResultDto>;
