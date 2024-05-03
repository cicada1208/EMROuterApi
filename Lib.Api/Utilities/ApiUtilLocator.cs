using Lib.Api.Models;
using Microsoft.Extensions.Options;

namespace Lib.Api.Utilities
{
    public class ApiUtilLocator
    {
        private readonly AppSettings settings;

        public ApiUtilLocator(IOptionsMonitor<AppSettings> settings)
        {
            this.settings = settings.CurrentValue;
        }

        //public ApiUtilLocator(AppSettings settings)
        //{
        //    this.settings = settings;
        //}

        private JwtUtil? _Jwt;
        public JwtUtil Jwt =>
            _Jwt ??= new JwtUtil(settings);

    }
}
