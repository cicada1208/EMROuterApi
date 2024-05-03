using Lib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Lib.Api.Attributes
{
    /// <summary>
    /// action exception handle and error response
    /// </summary>
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            // ExceptionFilter：可攔截所有類型 Exception，除了 HttpResponseException。
            // 可撰寫多個 ExceptionFilter class 篩選特定 Exception，使每種篩選器僅處理特定 Exception。
            // 例如：if (context.Exception is XyzException) ...

            // 記錄 ActionException，用於 LoggerMiddleware
            context.HttpContext.Items["__ActionException"] = context.Exception.ToString();

            // Error Response：
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Result = new JsonResult(new ApiError(HttpStatusCode.InternalServerError));

            // 手動 throw HttpResponseException，後續其他 ExceptionFilter 便不會執行
            //throw new HttpResponseException(context.Response);

            base.OnException(context);
        }
    }
}
