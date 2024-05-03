using DomainRule.Models.EMRDB;
using DomainRule.Repositories.EMRDB;
using Lib.Models;
using Lib.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMROuterApi.Controllers
{
    public class OtherHospitalController(OtherHospitalRepository otherHospitalRepository) : BaseController
    {
        [HttpGet]
        public Task<ApiResult<List<OtherHospital>>> Get([FromQuery] OtherHospitalQuery param) =>
            otherHospitalRepository.Get(param);

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public Task<ApiResult<OtherHospital>> Insert(OtherHospital param) =>
           otherHospitalRepository.Insert(param);

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        public Task<ApiResult<OtherHospital>> Update(OtherHospital param) =>
            otherHospitalRepository.Update(param);

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPatch]
        public Task<ApiResult<OtherHospital>> Patch(object param)
        {
            param.GetModelAndProps(out OtherHospital model, out HashSet<string> props);
            model.ModifyDateTime = DateTime.Now;
            props.AddProps<OtherHospital>(m => new { m.ModifyDateTime });
            return otherHospitalRepository.Patch(model, props);
        }

    }
}
