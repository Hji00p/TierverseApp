using MediatR;
using Microsoft.EntityFrameworkCore;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.TierLists.DTOs;
using TierList.Domain.Enums;

namespace TierList.Application.Features.TierLists.Queries.GetTierListById;

public class GetTierListByIdQueryHandler : IRequestHandler<GetTierListByIdQuery, TierListDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetTierListByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<TierListDto> Handle(GetTierListByIdQuery request, CancellationToken cancellationToken)
    {
        var tierList = await _context.TierLists
            .AsNoTracking()
            .Include(t => t.Author)
            .Include(t => t.Template)
            .Include(t => t.Tiers)
            .Include(t => t.Items)
            .Include(t => t.Votes)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tierList == null)
        {
            throw new NotFoundException("TierList", request.Id);
        }

        var likes = tierList.Votes.Count(v => v.Value == VoteValue.Like);
        var dislikes = tierList.Votes.Count(v => v.Value == VoteValue.Dislike);
        var score = likes - dislikes;

        int? userVote = null;
        if (_currentUserService.UserId.HasValue)
        {
            var vote = tierList.Votes.FirstOrDefault(v => v.UserId == _currentUserService.UserId.Value);
            if (vote != null)
            {
                userVote = (int)vote.Value;
            }
        }

        var tiers = tierList.Tiers
            .OrderBy(t => t.Order)
            .Select(t => new TierDto(t.Id, t.Name, t.Color, t.Order))
            .ToList();

        var items = tierList.Items
            .OrderBy(i => i.Position)
            .Select(i => new TierItemDto(i.Id, i.ImageUrl, i.Caption, i.TierId, i.Position))
            .ToList();

        return new TierListDto(
            tierList.Id,
            tierList.Title,
            tierList.Description,
            tierList.AuthorId,
            tierList.Author.UserName,
            tierList.TemplateId,
            tierList.Template?.Title,
            tiers,
            items,
            likes,
            dislikes,
            score,
            userVote,
            tierList.CreatedAt,
            tierList.UpdatedAt
        );
    }
}
