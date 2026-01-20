using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Popsies.Shared.Abstractions.Exceptions;

namespace Popsies.API.Exceptions;

/// <summary>
/// Global exception handler that converts exceptions to Problem Details responses
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the exception
        LogException(exception);

        // Create problem details based on exception type
        var problemDetails = CreateProblemDetails(httpContext, exception);

        // Write the problem details response
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            Exception = exception,
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }

    private ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        var (status, title, type) = MapExceptionToStatusAndTitle(exception);

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = type,
            Detail = exception.Message,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        // Add error code if it's a PopsiesException
        if (exception is PopsiesException popsiesException)
        {
            problemDetails.Extensions["errorCode"] = popsiesException.ErrorCode;
        }

        // Add validation errors if it's a ValidationException
        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        // Add trace ID for debugging
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        return problemDetails;
    }

    private static (int Status, string Title, string Type) MapExceptionToStatusAndTitle(Exception exception)
    {
        return exception switch
        {
            ValidationException => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1"
            ),
            ConflictException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10"
            ),
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Not Found",
                "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5"
            ),
            UnauthorizedException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.2"
            ),
            AuthenticationException => (
                StatusCodes.Status401Unauthorized,
                "Authentication Failed",
                "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.2"
            ),
            DomainException => (
                StatusCodes.Status400BadRequest,
                "Domain Error",
                "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1"
            ),
            PopsiesException => (
                StatusCodes.Status400BadRequest,
                "Application Error",
                "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1"
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1"
            )
        };
    }

    private void LogException(Exception exception)
    {
        var logLevel = exception switch
        {
            ValidationException => LogLevel.Warning,
            NotFoundException => LogLevel.Warning,
            ConflictException => LogLevel.Warning,
            UnauthorizedException => LogLevel.Warning,
            AuthenticationException => LogLevel.Warning,
            DomainException => LogLevel.Warning,
            PopsiesException => LogLevel.Error,
            _ => LogLevel.Error
        };

        _logger.Log(
            logLevel,
            exception,
            "An exception occurred: {ExceptionType} - {Message}",
            exception.GetType().Name,
            exception.Message);
    }
}
