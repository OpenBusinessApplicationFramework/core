using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Core.Filter;

public class RequireSelectFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var req = context.HttpContext.Request;
        var path = req.Path.Value ?? string.Empty;
        bool isProdEnvironment = true;

#if DEBUG
        isProdEnvironment = false;
#endif

        if ((path.EndsWith("/odata") || path.EndsWith("/inmemoryodata")) && !req.Query.ContainsKey("$select") && isProdEnvironment)
        {
            context.Result = new BadRequestObjectResult(new
            {
                error = new
                {
                    code = "MissingSelect",
                    message = "The $select query option is required."
                }
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // no-op
    }
}
