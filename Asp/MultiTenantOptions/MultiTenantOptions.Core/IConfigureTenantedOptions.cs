namespace TenantedOptions.Core;

public interface IConfigureTenantedOptions<TOptions>
    where TOptions : class
{
    void Configure(string name, string tenant, TOptions options);
}