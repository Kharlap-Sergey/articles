using Microsoft.EntityFrameworkCore;
using OAuthServer;
using OpenIddict.Core;
using OpenIddict.Server;
using TenantedOptions.Core;
using TestApplication.Multitenancy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<DbContext>(options =>
{
    options.UseInMemoryDatabase(nameof(DbContext));

    options.UseOpenIddict();
});

builder.Services.AddHttpContextAccessor();
//builder.Services.AddSingleton<HttpContextTenantProvider>();
builder.Services.AddTenantedOptions<HttpContextTenantProvider, OpenIddictServerOptions>();
builder.Services.AddSingleton<IConfigureTenantedOptions<OpenIddictServerOptions>, ConfigureCertificatesOpenIddictServerOptions>();
builder.Services.AddOpenIddict()
    .AddCore(
        options =>
        {
            options.UseEntityFrameworkCore()
                .UseDbContext<DbContext>();
        })
    .AddServer(
        options =>
        {
            options.AllowClientCredentialsFlow();

            options.SetTokenEndpointUris("token");

            options.DisableAccessTokenEncryption();

            options.UseAspNetCore()
                .EnableTokenEndpointPassthrough();
        });

builder.Services.AddControllers();

builder.Services.AddHostedService<ClientSeeder>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var firstPathPart = context.Request.Path.Value.Split('/')[1];

    var tenantFeature = new TenantFeature
    {
        TenantId = firstPathPart
    };
    context.Features.Set<ITenantFeature>(tenantFeature);

    context.Request.Path = context.Request.Path.Value.Replace($"/{firstPathPart}", "");
    context.Request.PathBase += $"/{firstPathPart}";
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"New request for tenant: {tenantFeature.TenantId}");
    await next(context);
});

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
