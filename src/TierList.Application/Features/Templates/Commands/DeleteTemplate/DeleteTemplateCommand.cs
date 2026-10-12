using MediatR;

namespace TierList.Application.Features.Templates.Commands.DeleteTemplate;

public record DeleteTemplateCommand(Guid Id) : IRequest;
