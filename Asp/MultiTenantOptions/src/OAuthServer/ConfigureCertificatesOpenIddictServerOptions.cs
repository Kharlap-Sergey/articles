using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using TenantedOptions.Core;

namespace OAuthServer;

public class ConfigureCertificatesOpenIddictServerOptions : IConfigureTenantedOptions<OpenIddictServerOptions>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigureCertificatesOpenIddictServerOptions> _logger;

    public ConfigureCertificatesOpenIddictServerOptions(
        IConfiguration configuration,
        ILogger<ConfigureCertificatesOpenIddictServerOptions> logger
        )
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Configure(string name, string tenant, OpenIddictServerOptions options)
    {
        _logger.LogInformation("Configuring OpenIddictServerOptions for tenant {tenant}", tenant);
        string cerdata;

        if (tenant.Equals("tenant1", StringComparison.OrdinalIgnoreCase))
        {
            cerdata = _configuration["OpenIddict:Certificate1"];
        }
        else
        {
            cerdata = _configuration["OpenIddict:Certificate2"];
        }

        var cer = new X509Certificate2(
            Convert.FromBase64String(cerdata),
            password: (string)null,
            keyStorageFlags: X509KeyStorageFlags.EphemeralKeySet
            );
        var signingCertificate = cer;
        options.SigningCredentials.Add(
            new(new X509SecurityKey(signingCertificate), SecurityAlgorithms.RsaSha256)
            );

        var encryptionCertificate = cer;
        options.EncryptionCredentials.Add(
            new(new X509SecurityKey(encryptionCertificate), SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512)
            );
    }
}