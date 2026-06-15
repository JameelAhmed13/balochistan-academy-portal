using System.Net;
using System.Text.Json;

namespace BalochiAcademy.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Path}", ctx.Request.Path);
            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode  = ex switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException        => (int)HttpStatusCode.NotFound,
                InvalidOperationException   => (int)HttpStatusCode.BadRequest,
                _                           => (int)HttpStatusCode.InternalServerError,
            };
            var body = JsonSerializer.Serialize(new
            {
                error   = ex.Message,
                status  = ctx.Response.StatusCode,
                traceId = ctx.TraceIdentifier,
            });
            await ctx.Response.WriteAsync(body);
        }
    }
}
