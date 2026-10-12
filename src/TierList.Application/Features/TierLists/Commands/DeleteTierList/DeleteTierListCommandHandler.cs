using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;

namespace TierList.Application.Features.TierLists.Commands.DeleteTierList;

public class DeleteTierListCommandHandler : IRequestHandler<DeleteTierListCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTierListCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteTierListCommand request, CancellationToken cancellationToken)
    {
        var tierList = await _context.TierLists
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tierList == null)
        {
            throw new NotFoundException("TierList", request.Id);
        }

        var isOwner = _currentUserService.UserId.HasValue && tierList.AuthorId == _currentUserService.UserId.Value;
        var isModeratorOrAdmin = _currentUserService.IsInRole("Moderator") || _currentUserService.IsInRole("Admin");

        if (!isOwner && !isModeratorOrAdmin)
        {
            throw new ForbiddenException("You are not allowed to delete this tier list.");
        }

        _context.TierLists.Remove(tierList);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
