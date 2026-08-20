using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ValidationException = FluentValidation.ValidationException;

namespace BaseRepository.Api.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = Map(exception);
        _logger.LogError(exception, "Unhandled exception mapped to {StatusCode}", statusCode);

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static (int StatusCode, string Title) Map(Exception exception) => exception switch
    {
        NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
        ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
        AuthenticationFailedException => (StatusCodes.Status401Unauthorized, "Authentication Failed"),
        BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Business Rule Violation"),
        ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
    };
}
