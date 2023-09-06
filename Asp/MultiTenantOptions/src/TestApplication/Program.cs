using Microsoft.AspNetCore.Authentication.JwtBearer;
using TenantedOptions.Core;
using TestApplication;
using TestApplication.Multitenancy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOptions();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<HttpContextTenantProvider>();
builder.Services.AddSingleton<ITenantProvider, HttpContextTenantProvider>();

builder.Services.AddTenantedOptions<HttpContextTenantProvider, JwtBearerOptions>();
builder.Services.AddSingleton<IConfigureTenantedOptions<JwtBearerOptions>>(
    sf => ActivatorUtilities.CreateInstance<ConfigureAuthorityJwtBearerOptions>(sf, "https://localhost:7041")
    );
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Audience = "test_resource";
    });
var app = builder.Build();

// Configure the HTTP request pipeline.

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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
