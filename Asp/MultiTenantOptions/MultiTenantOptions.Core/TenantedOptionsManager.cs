using Microsoft.Extensions.Options;

namespace TenantedOptions.Core;

public class TenantedOptionsManager<TOptions> :
    IOptions<TOptions>,
    IOptionsSnapshot<TOptions>
    where TOptions : class
{
    private readonly ITenantedOptionsFactory<TOptions> _factory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ITenantedOptionsMonitorCache<TOptions> _cache = new TenantedOptionsMonitorCache<TOptions>(
        new OptionsCache<IOptionsMonitorCache<TOptions>>()
        );

    public TenantedOptionsManager(
        ITenantedOptionsFactory<TOptions> factory,
        ITenantProvider tenantProvider
        )
    {
        _factory = factory;
        _tenantProvider = tenantProvider;
    }

    public TOptions Value => Get(Options.DefaultName);

    public virtual TOptions Get(string name)
    {
        name = name ?? Options.DefaultName;

        // Store the options in our instance cache. Avoid closure on fast path by storing state into scoped locals.
        ITenantedOptionsFactory<TOptions> localFactory = _factory;
        string localName = name;
        string localtenant = _tenantProvider.GetCurrentTenant();
        TOptions options = _cache.GetOrAdd(name, localtenant, () => localFactory.Create(localName, localtenant));

        return options;
    }
}