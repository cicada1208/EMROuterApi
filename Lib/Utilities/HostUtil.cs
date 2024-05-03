using System.Net;
using System.Net.NetworkInformation;

namespace Lib.Utilities
{
    public class HostUtil
    {
        /// <summary>
        /// 取得 HostName
        /// </summary>
        public static string GetHostName() =>
            Dns.GetHostName();

        /// <summary>
        /// 取得內網 IP Address
        /// </summary>
        public static string GetHostAddress()
        {
            string result = string.Empty;
            var ipAddress = Dns.GetHostAddresses(GetHostName());
            foreach (IPAddress ipa in ipAddress)
            {
                if (ipa.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                !IPAddress.IsLoopback(ipa) &&  // ignore loopback addresses
                !ipa.ToString().StartsWith("169.254") && // ignore link-local addresses
                (ipa.ToString().StartsWith("172.16") || ipa.ToString().StartsWith("172.17")))  // cych 網段
                    result = ipa.ToString();
            }
            return result;
        }

        /// <summary>
        /// 取得內網 IP Address 與 HostName
        /// </summary>
        public static string GetHostNameAndAddress() =>
            $"{GetHostAddress()}({GetHostName()})";

        /// <summary>
        /// 確認伺服器可否連線
        /// </summary>
        public bool CheckServerAvalible(string server, int timeout = 3000)
        {
            bool avalible;
            Ping ping = new Ping();

            try
            {
                PingReply pingReply = ping.Send(server, timeout);
                avalible = pingReply.Status == IPStatus.Success;
            }
            catch (Exception)
            {
                avalible = false;
            }
            finally
            {
                ping?.Dispose();
            }

            return avalible;
        }

    }
}
