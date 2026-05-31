using System.Net;
using System.Text.Json;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.Common.Validation;

namespace ClaimsModule.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteValidationErrorAsync(context, ex.Errors);
        }
        catch (StatusTransitionBlockedException ex)
        {
            await WriteValidationErrorAsync(context, ex.Errors, ex.BlockingConditions);
        }
        catch (JsonException ex) when (ex.Message == ClaimValidationMessages.InvalidReserveComponentType)
        {
            await WriteValidationErrorAsync(
                context,
                new Dictionary<string, string[]>
                {
                    ["ReserveComponent"] = [ClaimValidationMessages.InvalidReserveComponentType]
                });
        }
        catch (KeyNotFoundException ex)
        {
            await WriteNotFoundAsync(context, ex.Message);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "Stored file not found.");
            await WriteNotFoundAsync(context, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt.");
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = "application/json";

            var response = new
            {
                type = "Forbidden",
                title = "Access denied.",
                status = 403
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (ConcurrencyException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            context.Response.ContentType = "application/json";

            var response = new
            {
                type = "ConcurrencyConflict",
                title = ex.Message,
                status = 409
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                type = "InternalServerError",
                title = _environment.IsEnvironment("Testing")
                    ? ex.Message
                    : "An unexpected error occurred.",
                status = 500
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }

    private static async Task WriteNotFoundAsync(HttpContext context, string title)
    {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        context.Response.ContentType = "application/json";

        var response = new
        {
            type = "NotFound",
            title,
            status = 404
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static async Task WriteValidationErrorAsync(
        HttpContext context,
        IReadOnlyDictionary<string, string[]> errors,
        IReadOnlyList<ClaimsModule.Application.Abstractions.Services.ClaimClosureConditionDto>? blockingConditions = null)
    {
        context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        context.Response.ContentType = "application/json";

        if (blockingConditions is { Count: > 0 })
        {
            var responseWithConditions = new
            {
                type = "ValidationError",
                title = "One or more validation errors occurred.",
                status = 422,
                errors,
                blockingConditions
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(responseWithConditions));
            return;
        }

        var response = new
        {
            type = "ValidationError",
            title = "One or more validation errors occurred.",
            status = 422,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
