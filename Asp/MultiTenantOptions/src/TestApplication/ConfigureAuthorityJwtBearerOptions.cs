using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using TenantedOptions.Core;

namespace TestApplication
{
    public class ConfigureAuthorityJwtBearerOptions : IConfigureTenantedOptions<JwtBearerOptions>
    {
        private string _authority;
        private readonly ILogger<ConfigureAuthorityJwtBearerOptions> _logger;

        public ConfigureAuthorityJwtBearerOptions(string authority, ILogger<ConfigureAuthorityJwtBearerOptions> logger)
        {
            _authority = authority;
            _logger = logger;
        }

        public void Configure(string name, string tenant, JwtBearerOptions options)
        {
            options.Authority = _authority+"/"+tenant;
            _logger.LogInformation("configured authority: {authority}", options.Authority);
        }
    }
}
