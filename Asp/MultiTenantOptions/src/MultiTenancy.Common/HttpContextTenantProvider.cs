using Microsoft.AspNetCore.Http;
using TenantedOptions.Core;

namespace TestApplication.Multitenancy;

public class HttpContextTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public string GetCurrentTenant()
    {
        return _httpContextAccessor.HttpContext.Features.Get<ITenantFeature>()?.TenantId ?? throw new Exception("Tenant feature wasn't provided!");
    }
}
