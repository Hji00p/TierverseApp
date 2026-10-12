using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TierList.Application.Features.Images.Commands.UploadImage;

namespace TierList.WebApi.Controllers;

[Route("images")]
public class ImagesController : ApiControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ImagesController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB max request size
    public async Task<ActionResult<UploadImageResponse>> Upload(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { title = "Bad Request", detail = "File is required and cannot be empty." });
        }

        await using var stream = file.OpenReadStream();
        var url = await Mediator.Send(new UploadImageCommand(stream, file.ContentType, file.Length));

        return Ok(new UploadImageResponse(url));
    }

    [HttpGet("proxy")]
    [AllowAnonymous]
    public async Task<IActionResult> Proxy([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
             !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new { detail = "A valid HTTP or HTTPS image URL is required." });
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            if (Uri.TryCreate(url, UriKind.Absolute, out var targetUri))
            {
                if (targetUri.Host.Contains("wikia", StringComparison.OrdinalIgnoreCase) ||
                    targetUri.Host.Contains("fandom", StringComparison.OrdinalIgnoreCase))
                {
                    client.DefaultRequestHeaders.Referrer = new Uri("https://www.fandom.com/");
                }
                else
                {
                    client.DefaultRequestHeaders.Referrer = new Uri($"{targetUri.Scheme}://{targetUri.Host}/");
                }
            }

            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/png";
            Response.Headers.Append("Access-Control-Allow-Origin", "*");
            Response.Headers.Append("Cache-Control", "public, max-age=86400");
            var stream = await response.Content.ReadAsStreamAsync();
            return File(stream, contentType);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { detail = $"Failed to proxy image: {ex.Message}" });
        }
    }

    public record UploadImageResponse(string Url);
}
