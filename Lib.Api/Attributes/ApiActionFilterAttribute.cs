using Lib.Consts;
using Lib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;
using System.Net;

namespace Lib.Api.Attributes
{
    public class ApiActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 記錄 ActionArguments，用於 ApiExceptionFilterAttribute or LoggerMiddleware
            context.HttpContext.Items["__ActionArguments"] = context.ActionArguments;

            if (context.Result == null && !context.ModelState.IsValid)
            {
                var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

                var modelValidationErrors = context.ModelState.Where(ms => ms.Value?.ValidationState == ModelValidationState.Invalid).ToDictionary(
                    p => p.Key,
                    p => p.Value?.Errors.Select(e => e.ErrorMessage).ToList());

                // Error Response：
                context.Result = new BadRequestObjectResult(new ApiError(
                    HttpStatusCode.BadRequest, MsgConst.ApiModelValidationError, modelValidationErrors, traceId));
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result != null)
            {
                // When Response Result is ApiResult<>, rewriting HttpContext.Response.StatusCode by ApiResult.Code
                Type? resultType = (context.Result as ObjectResult)?.Value?.GetType();
                if (resultType != null && resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(ApiResult<>))
                {
                    HttpStatusCode? resultStatusCode = (((context.Result as ObjectResult)?.Value) as dynamic)?.Code;
                    if (resultStatusCode != null)
                        context.HttpContext.Response.StatusCode = (int)resultStatusCode;
                }
            }

            base.OnActionExecuted(context);
        }
    }
}
