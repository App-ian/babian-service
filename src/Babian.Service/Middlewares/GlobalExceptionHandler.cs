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
        _logger.LogError(exception, "Une exception non gérée s'est produite : {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        if (exception is BaseException baseException)
        {
            httpContext.Response.StatusCode = baseException.StatusCode;
            problemDetails.Title = baseException.GetType().Name;
            problemDetails.Status = baseException.StatusCode;
            problemDetails.Detail = baseException.Message;

            if (exception is ValidationException validationException && validationException.Errors.Any())
            {
                problemDetails.Extensions.Add("errors", validationException.Errors);
            }
        }
        else
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            problemDetails.Title = "Internal Server Error";
            problemDetails.Status = StatusCodes.Status500InternalServerError;
            problemDetails.Detail = "Une erreur inattendue s'est produite.";
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Return true to indicate the exception was handled
    }
}
