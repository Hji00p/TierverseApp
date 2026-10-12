using MediatR;
using TierList.Application.Features.Auth.DTOs;

namespace TierList.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string UserName, string Email, string Password) : IRequest<AuthResultDto>;
