using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;

namespace TierList.Application.Features.Templates.Commands.DeleteTemplate;

public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTemplateCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.Templates
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null)
        {
            throw new NotFoundException("Template", request.Id);
        }

        var isOwner = _currentUserService.UserId.HasValue && template.AuthorId == _currentUserService.UserId.Value;
        var isModOrAdmin = _currentUserService.IsInRole("Moderator") || _currentUserService.IsInRole("Admin");

        if (!isOwner && !isModOrAdmin)
        {
            throw new ForbiddenException("You are not allowed to delete this template.");
        }

        _context.Templates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
