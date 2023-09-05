using Microsoft.Extensions.Options;

namespace TenantedOptions.Core;

/// <summary>
/// Implementation of <see cref="ITenantedOptionsMonitorCache{TOptions}"/>.
/// this implementation based on <see cref=OptionsCache{TOptions}"/>
/// it is cache were value is a cache <see cref="IOptionsMonitorCache{TOptions}"/>
/// </summary>
/// <typeparam name="TOptions">Options type.</typeparam>
public class TenantedOptionsMonitorCache<TOptions> : ITenantedOptionsMonitorCache<TOptions>
    where TOptions : class
{
    protected readonly IOptionsMonitorCache<IOptionsMonitorCache<TOptions>> _cache;

    public TenantedOptionsMonitorCache(
        IOptionsMonitorCache<IOptionsMonitorCache<TOptions>> cache
        )
    {
        _cache = cache;
    }
    public void Clear()
        => _cache.Clear();

    public TOptions GetOrAdd(string name, string tenant, Func<TOptions> createOptions)
    {
        name ??= Options.DefaultName;
        var tenantCache = GetTenantOptionsCache(name);

        return tenantCache.GetOrAdd(tenant, createOptions);
    }

    public bool TryAdd(string name, string tenant, TOptions options)
    {
        name ??= Options.DefaultName;
        var tenantCache = GetTenantOptionsCache(name);

        return tenantCache.TryAdd(tenant, options);
    }

    public bool TryRemove(string name)
        => _cache.TryRemove(name ?? Options.DefaultName);

    protected IOptionsMonitorCache<TOptions> GetTenantOptionsCache(string name)
        => _cache.GetOrAdd(
            name,
            () => new OptionsCache<TOptions>()
        );
}
