using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;

namespace TierList.Application.Features.TierLists.Commands.UpdateTierListLayout;

public class UpdateTierListLayoutCommandHandler : IRequestHandler<UpdateTierListLayoutCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTierListLayoutCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateTierListLayoutCommand request, CancellationToken cancellationToken)
    {
        var tierList = await _context.TierLists
            .Include(t => t.Tiers)
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tierList == null)
        {
            throw new NotFoundException("TierList", request.Id);
        }

        var isOwner = _currentUserService.UserId.HasValue && tierList.AuthorId == _currentUserService.UserId.Value;
        if (!isOwner)
        {
            throw new ForbiddenException("You are not allowed to update the layout of this tier list.");
        }

        var validTierIds = tierList.Tiers.Select(t => t.Id).ToHashSet();
        foreach (var tierRow in request.Layout.Tiers)
        {
            if (!validTierIds.Contains(tierRow.TierId))
            {
                throw new ValidationException("Tiers", $"Tier with ID '{tierRow.TierId}' does not belong to this tier list.");
            }
        }

        var itemLookup = tierList.Items.ToDictionary(i => i.Id);
        var seenItemIds = new HashSet<Guid>();

        foreach (var tierRow in request.Layout.Tiers)
        {
            foreach (var itemId in tierRow.ItemIds)
            {
                if (!seenItemIds.Add(itemId))
                {
                    throw new ValidationException("ItemIds", $"Duplicate item ID '{itemId}' found in layout.");
                }

                if (!itemLookup.ContainsKey(itemId))
                {
                    throw new ValidationException("ItemIds", $"Item with ID '{itemId}' does not belong to this tier list.");
                }
            }
        }

        foreach (var itemId in request.Layout.UnplacedItemIds)
        {
            if (!seenItemIds.Add(itemId))
            {
                throw new ValidationException("ItemIds", $"Duplicate item ID '{itemId}' found in layout.");
            }

            if (!itemLookup.ContainsKey(itemId))
            {
                throw new ValidationException("ItemIds", $"Item with ID '{itemId}' does not belong to this tier list.");
            }
        }

        foreach (var tierRow in request.Layout.Tiers)
        {
            for (int i = 0; i < tierRow.ItemIds.Count; i++)
            {
                var item = itemLookup[tierRow.ItemIds[i]];
                item.PlaceInTier(tierRow.TierId, i);
            }
        }

        for (int i = 0; i < request.Layout.UnplacedItemIds.Count; i++)
        {
            var item = itemLookup[request.Layout.UnplacedItemIds[i]];
            item.PlaceInTier(null, i);
        }

        tierList.MarkUpdated();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
