namespace TenantedOptions.Core;

public interface ITenantProvider
{
    string GetCurrentTenant();
}