using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDataProtection()
    .SetApplicationName(builder.Configuration["ApplicationName"])
    .DisableAutomaticKeyGeneration()
    .PersistKeysToFileSystem(
        new DirectoryInfo(builder.Configuration["dpkeys"] 
         ?? throw new ArgumentNullException("Data protection keys weren't provided, check dpkeys config value"))
    );

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet(
    "/protect",
    ([FromQuery] string text,
     IDataProtectionProvider dataProtectionProvider) =>
    {
        var protector = dataProtectionProvider.CreateProtector("test");

        return protector.Protect(text);
    }
    );

app.MapGet(
    "/unprotect",
    ([FromQuery] string text,
     IDataProtectionProvider dataProtectionProvider) =>
    {
        var protector = dataProtectionProvider.CreateProtector("test");

        return protector.Unprotect(text);
    }
    );

app.MapGet(
    "/getAllKeys",
    (IKeyManager keyManager) =>
    {
        return keyManager.GetAllKeys();
    }
    );

app.MapGet(
    "/getCurrentKey",
    (IKeyRingProvider keyRing) =>
    {
        return keyRing.GetCurrentKeyRing().DefaultKeyId;
    }
    );

app.MapGet("/refresh_keys", (IKeyRingProvider provider) =>
{
    var type = provider.GetType();
    MethodInfo methodInfo = type.GetMethod("RefreshCurrentKeyRing", BindingFlags.NonPublic | BindingFlags.Instance);

    methodInfo.Invoke(provider, null);
});

app.Run();