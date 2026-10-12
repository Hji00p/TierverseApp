using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using TierList.Application.Common.Exceptions;

namespace TierList.WebApi.Middleware;

public record ErrorResponse(
    int Status,
    string Title,
    string? Detail = null,
    IDictionary<string, string[]>? Errors = null);

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException validationEx => new ErrorResponse(
                (int)HttpStatusCode.BadRequest,
                "Validation Failed",
                Errors: validationEx.Errors),

            NotFoundException notFoundEx => new ErrorResponse(
                (int)HttpStatusCode.NotFound,
                "Not Found",
                Detail: notFoundEx.Message),

            ForbiddenException forbiddenEx => new ErrorResponse(
                (int)HttpStatusCode.Forbidden,
                "Forbidden",
                Detail: forbiddenEx.Message),

            UnauthorizedAccessException unauthorizedEx => new ErrorResponse(
                (int)HttpStatusCode.Unauthorized,
                "Unauthorized",
                Detail: unauthorizedEx.Message),

            _ => new ErrorResponse(
                (int)HttpStatusCode.InternalServerError,
                "Internal Server Error",
                Detail: _env.IsDevelopment() ? exception.Message : "An unexpected error occurred.")
        };

        context.Response.StatusCode = response.Status;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}
