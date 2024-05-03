using DomainRule.Models.EMRDB;
using DomainRule.Repositories.EMRDB;
using Lib.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMROuterApi.Controllers
{
    public class PatientNoMappingController(PatientNoMappingRepository patientNoMappingRepository) : BaseController
    {
        [HttpGet]
        public Task<ApiResult<List<PatientNoMapping>>> Get([FromQuery] PatientNoMappingQuery param) =>
            patientNoMappingRepository.Get(param);

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("[action]")]
        public Task<ApiResult<PatientNoMapping>> Write(PatientNoMapping param) =>
            patientNoMappingRepository.Write(param);

    }
}
