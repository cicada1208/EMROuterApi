using Lib.Api.Models;
using Lib.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;

namespace Lib.Api.Configs
{
    public static class JwtBearerConfig
    {
        /// <summary>
        /// 設定驗證 JWT Token 參數
        /// </summary>
        public static TokenValidationParameters GetTokenValidationParameters(AppSettings settings)
        {
            return new TokenValidationParameters
            {
                // 透過這項宣告，可設定值給 User.Identity.Name
                NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                // 透過這項宣告，可讓 [Authorize] 判斷角色
                RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                ValidateIssuer = true,
                ValidIssuer = settings.Jwt.Issuer, //Configuration.GetValue<string>("Jwt:Issuer"),
                ValidateAudience = false,
                ValidateLifetime = true,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Jwt.SigningKey))
            };
        }

        /// <summary>
        /// 設定驗證 JWT Token，透過 [Authorize] 驗證
        /// </summary>
        public static AuthenticationBuilder AddJwtBearerConfig(this AuthenticationBuilder builder, AppSettings settings)
        {
            return builder.AddJwtBearer(options =>
            {
                // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
                options.IncludeErrorDetails = true;

                options.TokenValidationParameters = GetTokenValidationParameters(settings);

                options.Events = new JwtBearerEvents()
                {
                    OnChallenge = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        if (context.Response.ContentType == null ||
                            context.Response.ContentType.Contains("application/json"))
                        {
                            //context.HandleResponse(); // Skips any default logic for this challenge. 此方法不會 response WWW-Authenticate header
                            // Ensure we always have an error and error description.
                            if (string.IsNullOrEmpty(context.Error))
                                context.Error = "invalid_token";
                            if (string.IsNullOrEmpty(context.ErrorDescription))
                                context.ErrorDescription = "Unauthorized";
                            var body = new ApiError(HttpStatusCode.Unauthorized, context.ErrorDescription);
                            return context.Response.WriteAsJsonAsync(body);
                        }
                        else
                            return Task.CompletedTask;
                    },

                    OnForbidden = context =>
                    {
                        if (context.Response.ContentType == null ||
                            context.Response.ContentType.Contains("application/json"))
                        {
                            var body = new ApiError(HttpStatusCode.Forbidden, "Forbidden");
                            return context.Response.WriteAsJsonAsync(body);
                        }
                        else
                            return Task.CompletedTask;
                    }
                };
            });
        }
    }
}
