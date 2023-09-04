namespace TenantedOptions.Core;

public interface ITenantedOptionsFactory<TOptions>
{
    TOptions Create(string name, string tenant);
}
