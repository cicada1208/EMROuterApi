using DomainRule.Models.EMRDB;
using DomainRule.Repositories.EMRDB;
using Lib.Api.Utilities;
using Lib.Consts;
using Lib.Models;
using Lib.Utilities;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace DomainRule.Services
{
    public class AuthService(
        ApiUtilLocator apiUtils,
        OtherHospitalRepository otherHospitalRepository)
    {
        public async Task<ApiResult<string>> PostLogin(HttpRequest request)
        {
            bool decodeOK = request.Headers.DecodeBasicCredentials("Basic", out string userId, out _);

            ApiResult<string> result;
            if (!decodeOK || userId.IsNullOrWhiteSpace())
                result = new ApiResult<string>(false, msg: MsgConst.LoginErrorFormat, code: HttpStatusCode.Unauthorized);
            else
            {
                var otherHospital = (await otherHospitalRepository.Get(new OtherHospitalQuery
                {
                    HospitalId = userId,
                    Activate = true
                })).Data?.FirstOrDefault();

                if (otherHospital == null)
                    result = new ApiResult<string>(false, msg: "機構代碼不存在！", code: HttpStatusCode.Unauthorized);
                else
                    result = new ApiResult<string>(apiUtils.Jwt.GenerateToken(userId, expireMinutes: 1 * 24 * 60));
            }

            return result;
        }
    }
}
