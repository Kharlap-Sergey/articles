using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TenantedOptions.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class AuthTenantOptionsServiceCollectionExtensions
{
    public static IServiceCollection AddTenantedOptions<TTenantProvider, TOptions>(this IServiceCollection services)
        where TTenantProvider : class, ITenantProvider
        where TOptions : class, new()
    {
        services.AddOptions();

        services.TryAdd(ServiceDescriptor.Singleton(typeof(ITenantProvider), typeof(TTenantProvider)));
        services.TryAdd(ServiceDescriptor.Singleton(typeof(IOptionsMonitor<TOptions>), typeof(TenantedOptionsMonitor<TTenantProvider, TOptions>)));
        services.TryAdd(ServiceDescriptor.Transient(typeof(ITenantedOptionsFactory<TOptions>), typeof(TenantedOptionsFactory<TOptions>)));
        services.TryAdd(ServiceDescriptor.Singleton(typeof(ITenantedOptionsMonitorCache<>), typeof(TenantedOptionsMonitorCache<>)));

        return services;
    }
}