using MediatR;
using TierList.Application.Features.Auth.DTOs;

namespace TierList.Application.Features.Auth.Commands.Login;

public record LoginCommand(string UserName, string Password) : IRequest<AuthResultDto>;
