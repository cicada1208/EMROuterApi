namespace Lib.Api.Models
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; } = new();

        public Jwt Jwt { get; set; } = new();
    }

    public class ConnectionStrings
    {
        public string NIS { get; set; } = string.Empty;
        public string SYB1 { get; set; } = string.Empty;
        public string SYB2 { get; set; } = string.Empty;
        public string UAAC { get; set; } = string.Empty;
        public string EMRDB { get; set; } = string.Empty;
        public string PeriExam { get; set; } = string.Empty;
        public string MISSYS { get; set; } = string.Empty;
    }

    public class Jwt
    {
        public string Issuer { get; set; } = string.Empty;
        public string SigningKey { get; set; } = string.Empty;
    }

}
