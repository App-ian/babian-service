using Babian.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Babian.Service.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int statusCode = StatusCodes.Status500InternalServerError;
        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path,
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "Une erreur inattendue s'est produite."
        };

        if (exception is BaseException baseException)
        {
            statusCode = baseException.StatusCode;
            problemDetails.Title = baseException.GetType().Name;
            problemDetails.Status = baseException.StatusCode;
            problemDetails.Detail = baseException.Message;

            if (exception is ValidationException validationException && validationException.Errors.Any())
            {
                problemDetails.Extensions.Add("errors", validationException.Errors);
            }
            
            _logger.LogWarning("Handled Exception: {Method} {Path} returned {StatusCode}. Message: {Message}", 
                httpContext.Request.Method, httpContext.Request.Path, statusCode, exception.Message);
        }
        else
        {
            _logger.LogCritical(exception, "UNHANDLED 500 ERROR: {Method} {Path} failed. Message: {Message}. StackTrace: {StackTrace}", 
                httpContext.Request.Method, httpContext.Request.Path, exception.Message, exception.StackTrace);
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Return true to indicate the exception was handled
    }
}
