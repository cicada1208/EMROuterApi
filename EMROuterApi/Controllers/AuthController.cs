using DomainRule.Services;
using Lib.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMROuterApi.Controllers
{
    public class AuthController(AuthService authService) : BaseController
    {
        /// <summary>
        /// 驗證
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public Task<ApiResult<string>> PostLogin() =>
            authService.PostLogin(Request);

    }
}
