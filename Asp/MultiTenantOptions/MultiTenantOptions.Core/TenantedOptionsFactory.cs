using Microsoft.Extensions.Options;

namespace TenantedOptions.Core;

public class TenantedOptionsFactory<TOptions> : ITenantedOptionsFactory<TOptions>
    where TOptions : class, new()
{
    private readonly IConfigureOptions<TOptions>[] _setups;
    private readonly IPostConfigureOptions<TOptions>[] _postConfigures;
    private readonly IConfigureTenantedOptions<TOptions>[] _tenantSetups;
    private readonly IValidateOptions<TOptions>[] _validations;

    public TenantedOptionsFactory(
        IEnumerable<IConfigureOptions<TOptions>> setups,
        IEnumerable<IPostConfigureOptions<TOptions>> postConfigures,
        IEnumerable<IConfigureTenantedOptions<TOptions>> tenantSetups
        ) : this(setups, postConfigures, tenantSetups, validations: Array.Empty<IValidateOptions<TOptions>>())
    {
    }

    public TenantedOptionsFactory(
        IEnumerable<IConfigureOptions<TOptions>> setups,
        IEnumerable<IPostConfigureOptions<TOptions>> postConfigures,
        IEnumerable<IConfigureTenantedOptions<TOptions>> tenantSetups,
        IEnumerable<IValidateOptions<TOptions>> validations
        )
    {
        // The default DI container uses arrays under the covers. Take advantage of this knowledge
        // by checking for an array and enumerate over that, so we don't need to allocate an enumerator.
        // When it isn't already an array, convert it to one, but don't use System.Linq to avoid pulling Linq in to
        // small trimmed applications.

        _setups = setups as IConfigureOptions<TOptions>[] ?? new List<IConfigureOptions<TOptions>>(setups).ToArray();
        _postConfigures = postConfigures as IPostConfigureOptions<TOptions>[] ?? new List<IPostConfigureOptions<TOptions>>(postConfigures).ToArray();
        _tenantSetups = tenantSetups as IConfigureTenantedOptions<TOptions>[] ?? new List<IConfigureTenantedOptions<TOptions>>(tenantSetups).ToArray();
        _validations = validations as IValidateOptions<TOptions>[] ?? new List<IValidateOptions<TOptions>>(validations).ToArray();
    }

    public TOptions Create(string name, string tenant)
    {
        TOptions options = CreateInstance(name);
        foreach (IConfigureOptions<TOptions> setup in _setups)
        {
            if (setup is IConfigureNamedOptions<TOptions> namedSetup)
            {
                namedSetup.Configure(name, options);
            }
            else if (name == Options.DefaultName)
            {
                setup.Configure(options);
            }
        }
        foreach (IConfigureTenantedOptions<TOptions> setup in _tenantSetups)
        {
            setup.Configure(name, tenant, options);
        }
        foreach (IPostConfigureOptions<TOptions> post in _postConfigures)
        {
            post.PostConfigure(name, options);
        }

        if (_validations != null)
        {
            var failures = new List<string>();
            foreach (IValidateOptions<TOptions> validate in _validations)
            {
                ValidateOptionsResult result = validate.Validate(name, options);
                if (result is not null && result.Failed)
                {
                    failures.AddRange(result.Failures);
                }
            }
            if (failures.Count > 0)
            {
                throw new OptionsValidationException(name, typeof(TOptions), failures);
            }
        }

        return options;
    }

    protected virtual TOptions CreateInstance(string name)
    {
        return Activator.CreateInstance<TOptions>();
    }
}
