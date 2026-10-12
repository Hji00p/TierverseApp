using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TierList.Application.Common.Models;
using TierList.Application.Features.Admin.Commands.UpdateUserRole;
using TierList.Application.Features.Admin.DTOs;
using TierList.Application.Features.Admin.Queries.GetUsers;

namespace TierList.WebApi.Controllers;

[Route("admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ApiControllerBase
{
    public record UpdateRoleRequest(string Role);

    [HttpGet("users")]
    public async Task<ActionResult<PaginatedList<UserRoleDto>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetUsersQuery(search, page, pageSize));
        return Ok(result);
    }

    [HttpPut("users/{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        await Mediator.Send(new UpdateUserRoleCommand(id, request.Role));
        return NoContent();
    }
}
