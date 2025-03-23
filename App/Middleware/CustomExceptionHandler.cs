using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace App.Middleware;

public class CustomExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int statusCode;
        var message = exception.Message;

        var exceptionType = exception.GetType();

        if (exceptionType == typeof(NotFoundException))
        {
            statusCode = StatusCodes.Status404NotFound;
        }
        else if (exceptionType == typeof(ForbiddenVehicleTypeException))
        {
            statusCode = StatusCodes.Status400BadRequest;
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            message = "An unexpected error occurred.";
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            error = message,
            statusCode,
            traceId = httpContext.TraceIdentifier
        }, cancellationToken);

        return true;
    }
} 