using System.Text.Json;
using LasMelis.Api.Exceptions;

namespace LasMelis.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            var (status, mensaje) = ex switch
            {
                NotFoundAppException => (StatusCodes.Status404NotFound, ex.Message),
                ValidationAppException => (StatusCodes.Status400BadRequest, ex.Message),
                ConflictAppException => (StatusCodes.Status409Conflict, ex.Message),
                UnauthorizedAppException => (StatusCodes.Status401Unauthorized, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado en el servidor.")
            };

            if (status == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(ex, "Error no controlado procesando {Method} {Path}", context.Request.Method, context.Request.Path);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = status;
            var body = JsonSerializer.Serialize(new { mensaje });
            await context.Response.WriteAsync(body);
        }
    }
}
