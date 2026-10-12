using MediatR;
using TierList.Application.Common.Models;
using TierList.Application.Features.Comments.DTOs;

namespace TierList.Application.Features.Comments.Queries.GetComments;

public record GetCommentsQuery(Guid TierListId, int Page = 1, int PageSize = 20)
    : IRequest<PaginatedList<CommentDto>>;
