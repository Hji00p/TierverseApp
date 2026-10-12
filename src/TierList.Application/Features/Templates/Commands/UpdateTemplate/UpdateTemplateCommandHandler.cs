using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Domain.Entities;

namespace TierList.Application.Features.Templates.Commands.UpdateTemplate;

public class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTemplateCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.Templates
            .Include(t => t.Tiers)
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null)
        {
            throw new NotFoundException("Template", request.Id);
        }

        var isOwner = _currentUserService.UserId.HasValue && template.AuthorId == _currentUserService.UserId.Value;
        var isModOrAdmin = _currentUserService.IsInRole("Moderator") || _currentUserService.IsInRole("Admin");
        if (!isOwner && !isModOrAdmin)
        {
            throw new ForbiddenException("You are not allowed to edit this template.");
        }

        template.Update(request.Title, request.Description, request.Visibility);

        var existingTiers = template.Tiers.ToList();
        _context.TemplateTiers.RemoveRange(existingTiers);
        template.Tiers.Clear();

        var existingItems = template.Items.ToList();
        _context.TemplateItems.RemoveRange(existingItems);
        template.Items.Clear();

        if (request.Tiers != null)
        {
            foreach (var tier in request.Tiers)
            {
                var newTier = template.AddTier(tier.Name, tier.Color, tier.Order);
                _context.TemplateTiers.Add(newTier);
            }
        }

        if (request.Items != null)
        {
            foreach (var item in request.Items)
            {
                var newItem = template.AddItem(item.ImageUrl, item.Caption, item.Order);
                _context.TemplateItems.Add(newItem);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
