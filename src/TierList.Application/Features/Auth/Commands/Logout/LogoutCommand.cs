using MediatR;

namespace TierList.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string? RefreshToken) : IRequest;
