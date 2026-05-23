using ProjectManagementAPI.Core.Application.Common.Exceptions;
using ProjectManagementAPI.Core.Application.Common.Models;
using System.Net;
using System.Text.Json;

namespace ProjectManagementAPI.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = exception switch
        {
            NotFoundException ex => (HttpStatusCode.NotFound, ApiResponse.Fail(ex.Message)),
            UnauthorizedException ex => (HttpStatusCode.Forbidden, ApiResponse.Fail(ex.Message)),
            ConflictException ex => (HttpStatusCode.Conflict, ApiResponse.Fail(ex.Message)),
            ValidationException ex => (HttpStatusCode.BadRequest, ApiResponse.Fail("Validation failed.", ex.Errors)),
            _ => (HttpStatusCode.InternalServerError, ApiResponse.Fail("An unexpected error occurred. Please try again later."))
        };

        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
