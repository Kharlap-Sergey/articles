using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

var kyesValidFor = int.TryParse(builder.Configuration["dpKeysLifetime"], out var parsed)
    ? parsed
    : 90;

var svcs = builder.Services.BuildServiceProvider();
var logger = svcs.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Keys lifetime is set to {lifetime}", kyesValidFor);

builder.Services.AddDataProtection()
    .SetDefaultKeyLifetime(TimeSpan.FromDays(kyesValidFor))
    .PersistKeysToFileSystem(new DirectoryInfo(builder.Configuration["dpkeys"] ?? "./data_protection_keys"));

var app = builder.Build();

var keyRingProvider = app.Services.GetRequiredService<IKeyRingProvider>();

//triggers keys retrieval and rotation mechanisms
keyRingProvider.GetCurrentKeyRing();

public partial class Program { }