using Lib.Api.Attributes;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Authorization;

namespace EMROuterApi.Controllers
{
    [Route("[controller]")]
    [Produces("application/json")]
    [ApiController]
    [ApiActionFilter]
    [ApiExceptionFilter]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public abstract class BaseController : ControllerBase
    {
        protected string RemoteIpAddress =>
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

    }
}
