using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TierList.Application.Features.Comments.Commands.DeleteComment;
using TierList.Application.Features.Votes.Commands.VoteComment;
using TierList.Application.Features.Votes.DTOs;

namespace TierList.WebApi.Controllers;

[Route("comments")]
public class CommentsController : ApiControllerBase
{
    public record VoteRequest(int Value);

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteCommentCommand(id));
        return NoContent();
    }

    [HttpPut("{id:guid}/vote")]
    [Authorize]
    public async Task<ActionResult<VoteResultDto>> Vote(Guid id, [FromBody] VoteRequest request)
    {
        var result = await Mediator.Send(new VoteCommentCommand(id, request.Value));
        return Ok(result);
    }
}
