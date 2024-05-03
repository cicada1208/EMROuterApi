using DomainRule.Models.EMRDB;
using DomainRule.Services;
using Lib.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMROuterApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UploadController(UploadService uploadService) : BaseController
    {
        [HttpPost]
        public Task<ApiResult<EMROuter>> UploadEMROuter([FromForm] EMROuter param, IFormFile File) =>
            uploadService.UploadEMROuter(param, File);

        [HttpPost("[action]")]
        public Task<ApiResult<object>> UploadMch_emr([FromForm] Mch_emr_Upload param) =>
            uploadService.UploadMch_emr(param);

    }
}
