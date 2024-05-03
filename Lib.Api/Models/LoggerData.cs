namespace Lib.Api.Models
{
    public class LoggerData
    {
        public string ControllerName { get; set; } = string.Empty;

        public string ActionName { get; set; } = string.Empty;

        public string Method { get; set; } = string.Empty;

        public string RequestIP { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string? QueryString { get; set; }

        public string? Body { get; set; }

        public string? FormData { get; set; }

        public string? Cookies { get; set; }

        public string? Headers { get; set; }

        public object? ActionArguments { get; set; }

        public string? ActionException { get; set; }

        public string? ResponseBody { get; set; }

        public int StatusCode { get; set; }

        public string? LoggerException { get; set; }
    }
}
