using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspNetCoreRateLimit.Redis.BodyParameter;

public class RateLimitActionFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new ObjectResult(context.ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage)
            {
                StatusCode = StatusCodes.Status429TooManyRequests
            };
        }

        base.OnActionExecuting(context);
    }
}