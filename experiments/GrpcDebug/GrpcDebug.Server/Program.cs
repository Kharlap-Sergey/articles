using GrpcDebug.Server.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxConcurrentConnections = 10;
    serverOptions.Limits.MinRequestBodyDataRate = 
        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(20);
    serverOptions.Limits.Http2.MaxStreamsPerConnection = 10;
    serverOptions.Limits.Http2.MaxFrameSize = 16_384;
    serverOptions.Limits.Http2.MaxRequestHeaderFieldSize = 8192;
    serverOptions.Limits.Http2.InitialStreamWindowSize = 65535;
    serverOptions.Limits.Http2.InitialConnectionWindowSize = 65535;
    serverOptions.Limits.Http2.KeepAlivePingDelay = TimeSpan.FromSeconds(10);
    serverOptions.Limits.Http2.KeepAlivePingTimeout = TimeSpan.FromSeconds(20);
});

// Add services to the container.
builder.Services.AddGrpc(
    options =>
    {
        options.MaxSendMessageSize = 2 * 1024 * 1024 / 10;
        
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<SimpleGrpcService>();
app.MapGet(
    "/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();