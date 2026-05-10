using CoreApp.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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
        if (exception is GateNotFoundException or CaptureNotFoundException)
        {
            logger.LogInformation("Exception '{Message}' handled!", exception.Message);
            var problem = factory.CreateProblemDetails(
                context,
                StatusCodes.Status404NotFound,
                "Not found",
                detail: exception.Message);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }
        return false;
    }
}