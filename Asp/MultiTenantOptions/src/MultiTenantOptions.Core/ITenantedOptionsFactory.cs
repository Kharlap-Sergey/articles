namespace TenantedOptions.Core;

/// <summary>
/// Used to create TOptions instances
/// </summary>
/// <typeparam name="TOptions">The type of options being requested.</typeparam>
public interface ITenantedOptionsFactory<TOptions>
{
    /// <summary>
    /// Returns a configured TOptions instance with the given name and tenant.
    /// </summary>
    TOptions Create(string name, string tenant);
}
