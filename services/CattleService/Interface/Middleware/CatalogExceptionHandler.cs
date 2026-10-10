using Microsoft.AspNetCore.Diagnostics;
using SmartFarm.Cattle.Api.Domain.Exceptions;

namespace SmartFarm.Cattle.Api.Interface.Middleware;

public class CatalogExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, code, title) = exception switch
        {
            CatalogNotFoundException => (404, "NOT_FOUND", "Resource not found"),
            CatalogConflictException conflict => (409, conflict.Code, "Conflict"),
            ArgumentException => (400, "INVALID_REQUEST", "Invalid request"),
            _ => (0, "", "")
        };

        if (status == 0)
            return false;

        await Results.Problem(statusCode: status, title: title, detail: exception.Message,
            extensions: new Dictionary<string, object?> { ["code"] = code })
            .ExecuteAsync(httpContext);
        return true;
    }
}
