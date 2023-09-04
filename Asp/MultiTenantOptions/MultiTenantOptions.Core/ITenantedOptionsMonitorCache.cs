namespace TenantedOptions.Core;

public interface ITenantedOptionsMonitorCache<TOptions>
{
    void Clear();

    TOptions GetOrAdd(string name, string tenant, Func<TOptions> createOptions);

    bool TryAdd(string name, string tenant, TOptions options);

    bool TryRemove(string name);
}