using System.Net;
using System.Text.Json;
using Ecommerce.Application.Common.Exceptions;

namespace Ecommerce.API.Middlewares;

/// <summary>
/// Catches ALL unhandled exceptions and converts them to clean JSON.
/// No stack traces ever reach the client.
/// Register as: app.UseMiddleware&lt;ExceptionMiddleware&gt;()
/// </summary>
public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleAsync(ctx, ex);
        }
    }

    private static async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        var (code, msg, errors) = ex switch
        {
            ValidationException ve  => (HttpStatusCode.BadRequest, "Validation failed", ve.Errors.SelectMany(e => e.Value)),
            NotFoundException       => (HttpStatusCode.NotFound, ex.Message, Enumerable.Empty<string>()),
            ExternalApiException    => (HttpStatusCode.BadGateway, ex.Message, Enumerable.Empty<string>()),
            InvalidOperationException => (HttpStatusCode.Conflict, ex.Message, Enumerable.Empty<string>()),
            _                       => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", Enumerable.Empty<string>())
        };

        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = (int)code;

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            Success = false,
            StatusCode = (int)code,
            Message = msg,
            Errors = errors,
            TraceId = ctx.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
