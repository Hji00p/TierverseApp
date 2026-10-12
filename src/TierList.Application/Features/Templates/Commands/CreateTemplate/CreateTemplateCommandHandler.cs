using MediatR;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Domain.Entities;

namespace TierList.Application.Features.Templates.Commands.CreateTemplate;

public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateTemplateCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new ForbiddenException("You must be logged in to create a template.");
        }

        var authorId = _currentUserService.UserId.Value;
        var template = new Template(authorId, request.Title, request.Description, request.Visibility);

        if (request.Tiers != null)
        {
            foreach (var tier in request.Tiers)
            {
                template.AddTier(tier.Name, tier.Color, tier.Order);
            }
        }

        if (request.Items != null)
        {
            foreach (var item in request.Items)
            {
                template.AddItem(item.ImageUrl, item.Caption, item.Order);
            }
        }

        _context.Templates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        return template.Id;
    }
}
