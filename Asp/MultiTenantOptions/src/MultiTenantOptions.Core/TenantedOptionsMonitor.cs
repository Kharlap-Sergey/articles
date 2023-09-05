using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace TenantedOptions.Core;

/// <summary>
/// Implementation of <see cref="IOptionsMonitor{TOptions}"/>.
/// code is copright of <see cref=OptionsMonitor{TOptions}"/>
/// </summary>
/// <typeparam name="TTenantProvider">Type of a tenant provider.</typeparam>
/// <typeparam name="TOptions">Options type.</typeparam>
public class TenantedOptionsMonitor<TTenantProvider, TOptions> : IOptionsMonitor<TOptions>, IDisposable
   where TOptions : class
   where TTenantProvider : ITenantProvider
{
    private readonly ITenantedOptionsFactory<TOptions> _factory;
    private readonly TTenantProvider _tenantProvider;
    private readonly ITenantedOptionsMonitorCache<TOptions> _cache;

    private readonly List<IDisposable> _registrations = new List<IDisposable>();
    internal event Action<TOptions, string> _onChange;

    public TenantedOptionsMonitor(
        TTenantProvider tenantProvider,
        ITenantedOptionsFactory<TOptions> factory,
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

    /// <summary>
    /// The present value of the options.
    /// </summary>
    public TOptions CurrentValue => Get(Options.DefaultName);

    /// <summary>
    /// Returns a configured <typeparamref name="TOptions"/> instance with the given <paramref name="name"/>.
    /// </summary>
    public TOptions Get(string name)
    {
        name ??= Options.DefaultName;
        var tenant = _tenantProvider.GetCurrentTenant();

        return _cache.GetOrAdd(name, tenant, () => _factory.Create(name, tenant));
    }

    /// <summary>
    /// Registers a listener to be called whenever <typeparamref name="TOptions"/> changes.
    /// </summary>
    /// <param name="listener">The action to be invoked when <typeparamref name="TOptions"/> has changed.</param>
    /// <returns>An <see cref="IDisposable"/> which should be disposed to stop listening for changes.</returns>
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

    /// <summary>
    /// Removes all change registration subscriptions.
    /// </summary>
    public void Dispose()
    {
        // Remove all subscriptions to the change tokens
        foreach (IDisposable registration in _registrations)
        {
            registration.Dispose();
        }

        _registrations.Clear();
    }
    
    internal sealed class ChangeTrackerDisposable: IDisposable
    {
        private readonly Action<TOptions, string> _listener;
        private readonly TenantedOptionsMonitor<TTenantProvider, TOptions> _monitor;

        public ChangeTrackerDisposable(TenantedOptionsMonitor<TTenantProvider, TOptions> monitor, Action<TOptions, string> listener)
        {
            _listener = listener;
            _monitor = monitor;
        }

        public void OnChange(TOptions options, string name) => _listener.Invoke(options, name);

        public void Dispose() => _monitor._onChange -= OnChange;
    }
}
