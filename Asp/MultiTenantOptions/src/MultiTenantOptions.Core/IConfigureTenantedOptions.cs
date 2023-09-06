namespace TenantedOptions.Core;

/// <summary>
/// Represents something that configures the <typeparamref name="TOptions"/> type.
/// </summary>
/// <typeparam name="TOptions"></typeparam>
public interface IConfigureTenantedOptions<TOptions>
    where TOptions : class
{
    /// <summary>
    /// Invoked to configure a <typeparamref name="TOptions"/> instance.
    /// </summary>
    /// <param name="name">The name of the options instance being configured.</param>
    /// <param name="tenant">The tenant of the options instance being configured.</param>
    /// <param name="options">The options instance to configure.</param>
    void Configure(string name, string tenant, TOptions options);
}