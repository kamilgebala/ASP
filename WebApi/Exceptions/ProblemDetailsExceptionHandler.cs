using CoreApp.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace WebApi.Exceptions;

public class ProblemDetailsExceptionHandler(
    ProblemDetailsFactory factory,
    ILogger<ProblemDetailsExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            GateNotFoundException or CaptureNotFoundException or KeyNotFoundException
                => (StatusCodes.Status404NotFound, "Not found"),
            ForbiddenAccessException
                => (StatusCodes.Status403Forbidden, "Forbidden"),
            InvalidOperationException
                => (StatusCodes.Status400BadRequest, "Invalid operation"),
            _ => (0, string.Empty)
        };

        if (status == 0) return false;

        logger.LogInformation("Exception '{Message}' handled with status {Status}.",
            exception.Message, status);

        var problem = factory.CreateProblemDetails(context, status, title, detail: exception.Message);
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}