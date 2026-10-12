using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;

namespace TierList.Application.Features.TierLists.Commands.UpdateTierList;

public class UpdateTierListCommandHandler : IRequestHandler<UpdateTierListCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTierListCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateTierListCommand request, CancellationToken cancellationToken)
    {
        var tierList = await _context.TierLists
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tierList == null)
        {
            throw new NotFoundException("TierList", request.Id);
        }

        var isOwner = _currentUserService.UserId.HasValue && tierList.AuthorId == _currentUserService.UserId.Value;
        if (!isOwner)
        {
            throw new ForbiddenException("You are not allowed to edit this tier list.");
        }

        tierList.UpdateDetails(request.Title, request.Description);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
