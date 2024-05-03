using Lib.Api.Models;
using Lib.Models;
using Lib.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
//using NLog;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Lib.Api.Middlewares
{
    public class LoggerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<LoggerMiddleware> logger;
        //private static Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly JsonSerializerOptions logJsonSerializerOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public LoggerMiddleware(RequestDelegate next, ILogger<LoggerMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var loggerData = new LoggerData();
            Stream? originalBody = default!;

            try
            {
                context.Request.EnableBuffering();

                // copy a pointer to the original response body stream
                originalBody = context.Response.Body;

                await SetRequestLoggerData(context.Request, loggerData);

                using (var ms = new MemoryStream())
                {
                    // using MemoryStream for the temporary response body, so response body can read
                    context.Response.Body = ms;

                    // continue down the Middleware pipeline, eventually returning to this class
                    await next(context);

                    await SetResponseLoggerData(context.Response, ms, loggerData);

                    ms.Position = 0;
                    await ms.CopyToAsync(originalBody);
                }
            }
            catch (Exception ex)
            {
                SetExceptionLoggerData(ex, loggerData);
            }
            finally
            {
                context.Response.Body = originalBody;

                //if (string.IsNullOrEmpty(loggerData.ResponseBody) &&
                //    !string.IsNullOrEmpty(loggerData.LoggerException))
                if (string.IsNullOrEmpty(loggerData.ResponseBody) &&
                    (context.Response.StatusCode.ToString().StartsWith("4") ||
                    context.Response.StatusCode.ToString().StartsWith("5") ||
                    !string.IsNullOrEmpty(loggerData.LoggerException)))
                {
                    context.Response.StatusCode = loggerData.StatusCode;
                    if (context.Response.ContentType == null ||
                        context.Response.ContentType.Contains("application/json"))
                    {
                        var body = new ApiError((HttpStatusCode)context.Response.StatusCode);
                        await context.Response.WriteAsJsonAsync(body);
                        loggerData.ResponseBody = JsonSerializer.Serialize(body, logJsonSerializerOptions); //Newtonsoft.Json.JsonConvert.SerializeObject(body, Formatting.Indented);
                    }
                }

                WriteLoggerData(loggerData, context);
            }
        }

        private async Task SetRequestLoggerData(HttpRequest request, LoggerData loggerData)
        {
            //var sr = new StreamReader(request.Body);
            //var body = sr.ReadToEnd();

            // we now need to read the request stream.
            // first, we create a new byte[] with the same length as the request stream.
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            // then we copy the entire request stream into the new buffer.
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            // we convert the byte[] into a string using UTF8 encoding.
            loggerData.Body = Encoding.UTF8.GetString(buffer);
            request.Body.Position = 0;

            loggerData.RequestIP = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            loggerData.Url = request.HttpContext.Request.GetEncodedUrl();
            loggerData.Method = request.Method;
            loggerData.QueryString = request.QueryString.Value ?? string.Empty;
            loggerData.Cookies = BuildCookieString(request.Cookies);
            loggerData.Headers = BuildHeaderString(request.Headers);

            if (request.ContentType != null && (
                request.ContentType.Contains("multipart/form-data") ||
                request.ContentType.Contains("application/x-www-form-urlencoded")))
            {
                loggerData.FormData = BuildFormString(request.Form);
            }
        }

        private async Task SetResponseLoggerData(HttpResponse response, MemoryStream ms, LoggerData loggerData)
        {
            // read the response stream from the beginning
            ms.Position = 0;
            //loggerData.ResponseBody = await new StreamReader(ms).ReadToEndAsync();
            using (var sr = new StreamReader(stream: ms,
                                        encoding: Encoding.UTF8,
                                        detectEncodingFromByteOrderMarks: false,
                                        leaveOpen: true))
            {
                loggerData.ResponseBody = await sr.ReadToEndAsync();
            }
            loggerData.StatusCode = response.StatusCode;
        }

        private string BuildFormString(IFormCollection formCollection)
        {
            if (formCollection == null) return string.Empty;

            var stringBuilder = new StringBuilder();

            foreach (var item in formCollection)
            {
                stringBuilder.Append($"{item.Key}:{item.Value};");
            }

            return stringBuilder.ToString();
        }

        private string BuildCookieString(IRequestCookieCollection cookieCollection)
        {
            if (cookieCollection == null) return string.Empty;

            var stringBuilder = new StringBuilder();

            foreach (var item in cookieCollection)
            {
                stringBuilder.Append($"{item.Key}:{item.Value};");
            }

            return stringBuilder.ToString();
        }

        private string BuildHeaderString(IHeaderDictionary headerDictionary)
        {
            if (headerDictionary == null) return string.Empty;

            var stringBuilder = new StringBuilder();

            foreach (var item in headerDictionary)
            {
                stringBuilder.Append($"{item.Key}:{item.Value};");
            }

            return stringBuilder.ToString();
        }

        private void SetExceptionLoggerData(Exception exception, LoggerData loggerData)
        {
            loggerData.LoggerException = exception.ToString();
            loggerData.StatusCode = (int)HttpStatusCode.InternalServerError;
        }

        private void WriteLoggerData(LoggerData loggerData, HttpContext context)
        {
            if (loggerData.StatusCode.ToString().StartsWith("4") ||
                loggerData.StatusCode.ToString().StartsWith("5"))
            {
                loggerData.ControllerName = (context.Request.RouteValues["controller"] as string) ?? string.Empty;
                loggerData.ActionName = (context.Request.RouteValues["action"] as string) ?? string.Empty;
                loggerData.ActionArguments = context.Items["__ActionArguments"] ?? string.Empty;
                loggerData.ActionException = Regex.Replace(context.Items["__ActionException"].NullableToStr(), "\r\n", "@@");

                loggerData.Body = Regex.Replace(loggerData.Body.NullableToStr(), "\r\n", "@@");
                loggerData.ResponseBody = Regex.Replace(loggerData.ResponseBody.NullableToStr(), "\r\n", "@@");
                loggerData.LoggerException = Regex.Replace(loggerData.LoggerException.NullableToStr(), "\r\n", "@@");

                var loggerDataJson = JsonSerializer.Serialize(loggerData, logJsonSerializerOptions); //Newtonsoft.Json.JsonConvert.SerializeObject(loggerData, Formatting.Indented);
                loggerDataJson = Regex.Replace(loggerDataJson, "@@", Environment.NewLine);
                logger.LogError(loggerDataJson); //logger.Error(loggerDataJson);
            }
        }
    }

    public static class LoggerMiddlewareEx
    {
        public static IApplicationBuilder UseLoggerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggerMiddleware>();
        }
    }
}
