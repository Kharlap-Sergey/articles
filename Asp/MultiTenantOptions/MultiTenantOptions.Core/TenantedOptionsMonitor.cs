using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace TenantedOptions.Core;

public class TenantedOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>
   where TOptions : class
{
    private readonly ITenantedOptionsFactory<TOptions> _factory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ITenantedOptionsMonitorCache<TOptions> _cache;

    private readonly List<IDisposable> _registrations = new List<IDisposable>();
    internal event Action<TOptions, string> _onChange;

    public TenantedOptionsMonitor(
        ITenantedOptionsFactory<TOptions> factory,
        ITenantProvider tenantProvider,
        ITenantedOptionsMonitorCache<TOptions> cache,
        IEnumerable<IOptionsChangeTokenSource<TOptions>> sources
        )
    {
        _factory = factory;
        _tenantProvider = tenantProvider;
        _cache = cache;

        void RegisterSource(IOptionsChangeTokenSource<TOptions> source)
        {
            IDisposable registration = ChangeToken.OnChange(
                      () => source.GetChangeToken(),
                      (name) => InvokeChanged(name),
                      source.Name);

            _registrations.Add(registration);
        }

        // The default DI container uses arrays under the covers. Take advantage of this knowledge
        // by checking for an array and enumerate over that, so we don't need to allocate an enumerator.
        if (sources is IOptionsChangeTokenSource<TOptions>[] sourcesArray)
        {
            foreach (IOptionsChangeTokenSource<TOptions> source in sourcesArray)
            {
                RegisterSource(source);
            }
        }
        else
        {
            foreach (IOptionsChangeTokenSource<TOptions> source in sources)
            {
                RegisterSource(source);
            }
        }
    }

    public TOptions CurrentValue => Get(Options.DefaultName);

    public TOptions Get(string name)
    {
        name ??= Options.DefaultName;
        var tenant = _tenantProvider.GetCurrentTenant();

        return _cache.GetOrAdd(name, tenant, () => _factory.Create(name, tenant));
    }

    public IDisposable OnChange(Action<TOptions, string> listener)
    {
        var disposable = new ChangeTrackerDisposable(this, listener);
        _onChange += disposable.OnChange;
        return disposable;
    }

    private void InvokeChanged(string name)
    {
        name = name ?? Options.DefaultName;
        _cache.TryRemove(name);
        TOptions options = Get(name);
        if (_onChange != null)
        {
            _onChange.Invoke(options, name);
        }
    }

    internal sealed class ChangeTrackerDisposable : IDisposable
    {
        private readonly Action<TOptions, string> _listener;
        private readonly TenantedOptionsMonitor<TOptions> _monitor;

        public ChangeTrackerDisposable(TenantedOptionsMonitor<TOptions> monitor, Action<TOptions, string> listener)
        {
            _listener = listener;
            _monitor = monitor;
        }

        public void OnChange(TOptions options, string name) => _listener.Invoke(options, name);

        public void Dispose() => _monitor._onChange -= OnChange;
    }
}
