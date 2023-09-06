using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenantedOptions.Core;

namespace TestApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class TestController : ControllerBase
    {
        private readonly ITenantProvider _tenantProvider;

        public TestController(ITenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_tenantProvider.GetCurrentTenant());
        }
    }
}