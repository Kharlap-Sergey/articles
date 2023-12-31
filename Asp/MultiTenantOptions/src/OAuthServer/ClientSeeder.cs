﻿using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace OAuthServer
{
    public class ClientSeeder : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public ClientSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var databaseContext = scope.ServiceProvider.GetRequiredService<DbContext>();
            databaseContext.Database.EnsureCreated();

            await PopulateScopes(scope, cancellationToken);

            await PopulateInternalApps(scope, cancellationToken);
        }

        private async ValueTask PopulateScopes(IServiceScope scope, CancellationToken cancellationToken)
        {
            var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            var scopeDescriptor = new OpenIddictScopeDescriptor
            {
                Name = "test_scope",
                Resources = { "test_resource" }
            };

            var scopeInstance = await scopeManager.FindByNameAsync(scopeDescriptor.Name, cancellationToken);

            if (scopeInstance == null)
            {
                await scopeManager.CreateAsync(scopeDescriptor, cancellationToken);
            }
            else
            {
                await scopeManager.UpdateAsync(scopeInstance, scopeDescriptor, cancellationToken);
            }
        }

        private async ValueTask PopulateInternalApps(IServiceScope scopeService, CancellationToken cancellationToken)
        {
            var appManager = scopeService.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            var appDescriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = "test_client",
                ClientSecret = "test_secret",
                Type = OpenIddictConstants.ClientTypes.Confidential,
                Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Token,

                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,

                        OpenIddictConstants.Permissions.Prefixes.Scope + "test_scope"
                    }
            };

            var client = await appManager.FindByClientIdAsync(appDescriptor.ClientId, cancellationToken);

            if (client == null)
            {
                await appManager.CreateAsync(appDescriptor, cancellationToken);
            }
            else
            {
                await appManager.UpdateAsync(client, appDescriptor, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
