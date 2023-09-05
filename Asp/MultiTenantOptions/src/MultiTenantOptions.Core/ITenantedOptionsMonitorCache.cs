namespace TenantedOptions.Core;

/// <summary>
/// Used by <see cref="TenantedOptionsMonitor{TOptions}"/> to cache <typeparamref name="TOptions"/> instances.
/// </summary>
/// <typeparam name="TOptions">The type of options being requested.</typeparam>
public interface ITenantedOptionsMonitorCache<TOptions>
{
    void Clear();

    TOptions GetOrAdd(string name, string tenant, Func<TOptions> createOptions);

    bool TryAdd(string name, string tenant, TOptions options);

    bool TryRemove(string name);
}