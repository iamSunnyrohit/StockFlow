using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.Common.Exceptions;
using ValidationException =
    StockFlow.Application.Common.Exceptions.ValidationException;

namespace StockFlow.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "An unhandled exception occurred while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path
            );

            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        if (context.Response.HasStarted)
        {
            throw exception;
        }

        var (statusCode, title, detail) = exception switch
        {
            NotFoundException ex => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                ex.Message
            ),

            ConflictException ex => (
                StatusCodes.Status409Conflict,
                "Resource conflict",
                ex.Message
            ),

            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                ex.Message
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred."
            )
        };

        context.Response.Clear();
        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problem);
    }
}
