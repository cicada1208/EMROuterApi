using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using System.Text;

namespace Lib.Utilities
{
    public class ConfigUtil
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defaultVal, StringBuilder returnVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        public static IConfigurationRoot GetAppSettings()
        {
            string? envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
            string? envNameWin = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Machine);
            if (!envNameWin.IsNullOrWhiteSpace())
                envName = envNameWin;

            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{envName}.json", true, true);

            IConfigurationRoot configuration = builder.Build();
            return configuration;
        }

        public static string GetEnvAppRun()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("env.json", true, true);

            IConfigurationRoot configuration = builder.Build();
            return configuration.GetValue<string>("APP_RUN") ?? string.Empty;
        }

        /// <summary>
        /// 讀取 ini
        /// </summary>
        public string ReadINI(string section, string key, string filePath, string defaultVal = "")
        {
            StringBuilder returnVal = new StringBuilder(255);
            int ini = GetPrivateProfileString(section, key, defaultVal, returnVal, 255, filePath);
            return returnVal.ToString();
        }

        /// <summary>
        /// 寫入 ini
        /// </summary>
        /// <remarks>
        /// <para>刪除 key：將 value 設為 null 即可刪除 key，WriteINI(section, key, null)</para>
        /// <para>刪除整個 section：將 key 設為 null 即可刪除整個 section，WriteINI(section, null, null)</para>
        /// </remarks>
        public void WriteINI(string section, string key, string value, string filePath)
        {
            WritePrivateProfileString(section, key, value, filePath);
        }

    }
}
