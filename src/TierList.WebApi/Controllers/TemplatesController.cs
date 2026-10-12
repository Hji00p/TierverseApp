using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TierList.Application.Common.Models;
using TierList.Application.Features.Templates.Commands.CreateTemplate;
using TierList.Application.Features.Templates.Commands.DeleteTemplate;
using TierList.Application.Features.Templates.Commands.UpdateTemplate;
using TierList.Application.Features.Templates.DTOs;
using TierList.Application.Features.Templates.Queries.GetTemplateById;
using TierList.Application.Features.Templates.Queries.GetTemplates;
using TierList.Domain.Enums;

namespace TierList.WebApi.Controllers;

[Route("templates")]
public class TemplatesController : ApiControllerBase
{
    public record CreateTemplateRequest(
        string Title,
        string? Description,
        TemplateVisibility Visibility,
        List<CreateTemplateTierModel> Tiers,
        List<CreateTemplateItemModel> Items);

    public record UpdateTemplateRequest(
        string Title,
        string? Description,
        TemplateVisibility Visibility,
        List<CreateTemplateTierModel> Tiers,
        List<CreateTemplateItemModel> Items);

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedList<TemplateBriefDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetTemplatesQuery(search, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<TemplateDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetTemplateByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateTemplateRequest request)
    {
        var command = new CreateTemplateCommand(
            request.Title,
            request.Description,
            request.Visibility,
            request.Tiers,
            request.Items);

        var templateId = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = templateId }, new { id = templateId });
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateRequest request)
    {
        var command = new UpdateTemplateCommand(
            id,
            request.Title,
            request.Description,
            request.Visibility,
            request.Tiers,
            request.Items);

        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteTemplateCommand(id));
        return NoContent();
    }
}
