using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TierList.Application.Common.Models;
using TierList.Application.Features.Comments.Commands.CreateComment;
using TierList.Application.Features.Comments.DTOs;
using TierList.Application.Features.Comments.Queries.GetComments;
using TierList.Application.Features.TierLists.Commands.CreateTierList;
using TierList.Application.Features.TierLists.Commands.DeleteTierList;
using TierList.Application.Features.TierLists.Commands.UpdateTierList;
using TierList.Application.Features.TierLists.Commands.UpdateTierListLayout;
using TierList.Application.Features.TierLists.DTOs;
using TierList.Application.Features.TierLists.Queries.GetTierListById;
using TierList.Application.Features.TierLists.Queries.GetTierLists;
using TierList.Application.Features.Votes.Commands.VoteTierList;
using TierList.Application.Features.Votes.DTOs;

namespace TierList.WebApi.Controllers;

[Route("tierlists")]
public class TierListsController : ApiControllerBase
{
    public record CreateTierListRequest(
        string Title,
        string? Description,
        Guid? TemplateId,
        List<CreateTierDto>? Tiers,
        List<CreateTierItemDto>? Items,
        SaveLayoutDto? Layout);

    public record UpdateTierListRequest(
        string Title,
        string? Description);

    public record VoteRequest(int Value);

    public record CreateCommentRequest(string Text, Guid? ParentCommentId = null);

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedList<TierListBriefDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetTierListsQuery(search, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<TierListDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetTierListByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateTierListRequest request)
    {
        var command = new CreateTierListCommand(
            request.Title,
            request.Description,
            request.TemplateId,
            request.Tiers,
            request.Items,
            request.Layout);

        var tierListId = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = tierListId }, new { id = tierListId });
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTierListRequest request)
    {
        var command = new UpdateTierListCommand(id, request.Title, request.Description);
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpPut("{id:guid}/layout")]
    [Authorize]
    public async Task<IActionResult> UpdateLayout(Guid id, [FromBody] SaveLayoutDto layout)
    {
        var command = new UpdateTierListLayoutCommand(id, layout);
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteTierListCommand(id));
        return NoContent();
    }

    [HttpPut("{id:guid}/vote")]
    [Authorize]
    public async Task<ActionResult<VoteResultDto>> Vote(Guid id, [FromBody] VoteRequest request)
    {
        var result = await Mediator.Send(new VoteTierListCommand(id, request.Value));
        return Ok(result);
    }

    [HttpGet("{id:guid}/comments")]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedList<CommentDto>>> GetComments(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetCommentsQuery(id, page, pageSize));
        return Ok(result);
    }

    [HttpPost("{id:guid}/comments")]
    [Authorize]
    public async Task<ActionResult<Guid>> CreateComment(
        Guid id,
        [FromBody] CreateCommentRequest request)
    {
        var command = new CreateCommentCommand(id, request.Text, request.ParentCommentId);
        var commentId = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetComments), new { id }, new { id = commentId });
    }
}
