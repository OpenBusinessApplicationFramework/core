using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Npgsql;

namespace Core.Filter;

public class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var ex = context.Exception;
        var status = ex switch
        {
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        if (ex?.InnerException is PostgresException)
            status = StatusCodes.Status409Conflict;

        var pd = new ProblemDetails
        {
            Status = status,
            Title = ex.Message,
            Detail = ex.InnerException is PostgresException ? ((PostgresException)ex.InnerException).MessageText : ex?.InnerException?.Message ?? null,
        };

        context.Result = new ObjectResult(pd)
        {
            StatusCode = status,
            DeclaredType = typeof(ProblemDetails)
        };
        context.ExceptionHandled = true;
    }
}
