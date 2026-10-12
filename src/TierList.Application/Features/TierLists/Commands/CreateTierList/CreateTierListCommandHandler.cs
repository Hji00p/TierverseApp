using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Domain.Entities;
using TierListEntity = TierList.Domain.Entities.TierList;

namespace TierList.Application.Features.TierLists.Commands.CreateTierList;

public class CreateTierListCommandHandler : IRequestHandler<CreateTierListCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateTierListCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateTierListCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("You must be logged in to create a tier list.");
        }

        var userId = _currentUserService.UserId.Value;
        var tierList = new TierListEntity(userId, request.Title, request.Description, request.TemplateId);

        var tierMap = new Dictionary<Guid, Guid>();
        var itemMap = new Dictionary<Guid, TierItem>();

        if (request.TemplateId.HasValue)
        {
            var template = await _context.Templates
                .Include(t => t.Tiers)
                .Include(t => t.Items)
                .FirstOrDefaultAsync(t => t.Id == request.TemplateId.Value, cancellationToken);

            if (template == null)
            {
                throw new NotFoundException("Template", request.TemplateId.Value);
            }

            if (request.Tiers != null && request.Tiers.Any())
            {
                foreach (var tierDto in request.Tiers)
                {
                    var tier = tierList.AddTier(tierDto.Name, tierDto.Color, tierDto.Order);
                    _context.Tiers.Add(tier);
                    if (tierDto.Id.HasValue)
                    {
                        tierMap[tierDto.Id.Value] = tier.Id;
                    }
                }
            }
            else
            {
                foreach (var templateTier in template.Tiers.OrderBy(t => t.Order))
                {
                    var tier = tierList.AddTier(templateTier.Name, templateTier.Color, templateTier.Order);
                    _context.Tiers.Add(tier);
                    tierMap[templateTier.Id] = tier.Id;
                }
            }

            if (request.Items != null && request.Items.Any())
            {
                foreach (var itemDto in request.Items)
                {
                    var item = tierList.AddItem(itemDto.ImageUrl, itemDto.Caption);
                    _context.TierItems.Add(item);
                    if (itemDto.Id.HasValue)
                    {
                        itemMap[itemDto.Id.Value] = item;
                    }
                }
            }
            else
            {
                foreach (var templateItem in template.Items.OrderBy(i => i.Order))
                {
                    var item = tierList.AddItem(templateItem.ImageUrl, templateItem.Caption);
                    _context.TierItems.Add(item);
                    itemMap[templateItem.Id] = item;
                }
            }
        }
        else
        {
            if (request.Tiers != null && request.Tiers.Any())
            {
                foreach (var tierDto in request.Tiers)
                {
                    var tier = tierList.AddTier(tierDto.Name, tierDto.Color, tierDto.Order);
                    _context.Tiers.Add(tier);
                    if (tierDto.Id.HasValue)
                    {
                        tierMap[tierDto.Id.Value] = tier.Id;
                    }
                }
            }
            else
            {
                var defaultTiers = new[]
                {
                    ("S", "#ff7f7f", 0),
                    ("A", "#ffbf7f", 1),
                    ("B", "#ffff7f", 2),
                    ("C", "#bfff7f", 3),
                    ("D", "#7fbfff", 4)
                };

                foreach (var (name, color, order) in defaultTiers)
                {
                    var tier = tierList.AddTier(name, color, order);
                    _context.Tiers.Add(tier);
                }
            }

            if (request.Items != null && request.Items.Any())
            {
                foreach (var itemDto in request.Items)
                {
                    var item = tierList.AddItem(itemDto.ImageUrl, itemDto.Caption);
                    _context.TierItems.Add(item);
                    if (itemDto.Id.HasValue)
                    {
                        itemMap[itemDto.Id.Value] = item;
                    }
                }
            }
        }

        if (request.Layout != null)
        {
            var createdTiersList = tierList.Tiers.OrderBy(t => t.Order).ToList();
            if (request.Layout.Tiers != null)
            {
                for (int rowIndex = 0; rowIndex < request.Layout.Tiers.Count; rowIndex++)
                {
                    var row = request.Layout.Tiers[rowIndex];
                    if (!tierMap.TryGetValue(row.TierId, out var targetTierId))
                    {
                        if (rowIndex < createdTiersList.Count)
                        {
                            targetTierId = createdTiersList[rowIndex].Id;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    for (int i = 0; i < row.ItemIds.Count; i++)
                    {
                        if (itemMap.TryGetValue(row.ItemIds[i], out var item))
                        {
                            item.PlaceInTier(targetTierId, i);
                        }
                    }
                }
            }

            if (request.Layout.UnplacedItemIds != null)
            {
                for (int i = 0; i < request.Layout.UnplacedItemIds.Count; i++)
                {
                    if (itemMap.TryGetValue(request.Layout.UnplacedItemIds[i], out var item))
                    {
                        item.PlaceInTier(null, i);
                    }
                }
            }
        }

        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync(cancellationToken);

        return tierList.Id;
    }
}
