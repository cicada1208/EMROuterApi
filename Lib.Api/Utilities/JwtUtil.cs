using Lib.Api.Configs;
using Lib.Api.Models;
using Lib.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Lib.Api.Utilities
{
    public class JwtUtil
    {
        private readonly AppSettings settings;

        public JwtUtil(AppSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// 簽發 JWT Token
        /// </summary>
        public string GenerateToken(string userId, IEnumerable<string>? userRoles = null, double expireMinutes = 30)
        {
            // 設定要加入到 JWT Token 中的聲明資訊(Claims)，可自行擴充
            var claims = new List<Claim>();
            //claims.Add(new Claim(JwtRegisteredClaimNames.NameId, userId)); // 可設定值給 User.Identity.Name
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId)); // 可設定值給 User.Identity.Name (同上擇一)
            userRoles?.ToList().ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));  // JWT ID，使 JWT Token 不重複

            // 建立一組對稱式加密的金鑰，主要用於 JWT 簽章之用
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Jwt.SigningKey));

            // HmacSha256 有要求必須要大於 128 bits，所以 key 不能太短，至少要 16 字元以上
            // https://stackoverflow.com/questions/47279947/idx10603-the-algorithm-hs256-requires-the-securitykey-keysize-to-be-greater
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var securityToken = new JwtSecurityToken
            (
                issuer: settings.Jwt.Issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }

        /// <summary>
        /// 驗證 JWT Token
        /// </summary>
        /// <returns>validation fails return null</returns>
        public JwtSecurityToken? ValidateToken(string token)
        {
            JwtSecurityToken? securityToken = null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, JwtBearerConfig.GetTokenValidationParameters(settings),
                    out SecurityToken validatedToken);

                securityToken = validatedToken as JwtSecurityToken;

                //var userId = securityToken?.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
            }
            catch { }   //  jwt validation fails

            return securityToken;
        }

    }

    public static class JwtExUtil
    {
        /// <summary>
        /// 取得 Authorization Credentials
        /// </summary>
        /// <param name="headers">Headers</param>
        /// <param name="type">Bearer or Basic</param>
        /// <remarks>Authorization: Bearer eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMDk2NCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiMjI3YWI3ZWMtMTViYi00ZjQ0LTlmMTUtMDAzZjcwYjA0NzE1IiwiZXhwIjoxNjM1OTAwNDYzLCJpc3MiOiJDeWNoIn0.JY0yPIVJCP-ENEFgunviAxUbs9VOn8eArj0MenACFcI</remarks>
        public static string AuthorizationCredentials(this IHeaderDictionary headers, string type)
        {
            string authorization = headers[HeaderNames.Authorization].ToString();
            string credentials = string.Empty;

            if (authorization.StartsWith($"{type} "))
                credentials = authorization.SubStr($"{type} ".Length);

            return credentials;
        }

        /// <summary>
        /// Decode Basic Authorization Credentials
        /// </summary>
        /// <param name="type">Basic</param>
        /// <remarks>
        /// <para>e.g. Authorization: Basic dnVsY2FuOjEyM2FiYw==</para>
        /// <para>若發生登入 call api 時好時壞，報的錯誤訊息為 CORS，需確認 webf01、webf02 IIS基本驗證設定是否相同 (若使用 HTTP Basic Auth，基本驗證需關閉)</para>
        /// </remarks>
        public static bool DecodeBasicCredentials(this IHeaderDictionary headers, string type,
            out string userId, out string password)
        {
            bool result = false;
            userId = string.Empty;
            password = string.Empty;

            // 取出 Base64 編碼的帳號與密碼
            var encodedCredentials = headers.AuthorizationCredentials(type);
            // 進行 Base64 解碼
            var credentialBytes = Convert.FromBase64String(encodedCredentials);
            // 取得 .NET 字串
            var credentials = Encoding.UTF8.GetString(credentialBytes);
            // 判斷格式是否正確
            var credentialParts = credentials.Split(':');
            if (credentialParts.Length == 2)
            {
                // 取出使用者傳送過來的帳號與密碼
                userId = credentialParts[0];
                password = credentialParts[1];
                result = true;
            }

            return result;
        }

    }
}
