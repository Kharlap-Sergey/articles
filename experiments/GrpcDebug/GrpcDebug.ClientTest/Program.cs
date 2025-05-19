// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using Grpc.Core;
using GrpcDebug.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddCommandLine(args);

var type = builder.Configuration["Type"] ?? "plain";
var runs = int.TryParse(builder.Configuration["Runs"], out var r) ? r : 1;
var repeatsInRun = int.TryParse(builder.Configuration["RepeatsInRun"], out var rr) ? rr :5;
var parallelRequests = int.TryParse(builder.Configuration["ParallelRequests"], out var p) ? p : 100;
var count = int.TryParse(builder.Configuration["Count"], out var c) ? c : 100000;

Console.WriteLine("Starting client with type: {0}, runs: {1}, repeatsInRun: {2}, parallelRequests: {3}, count: {4}", type, runs, repeatsInRun, parallelRequests, count);

for (int i = 0; i < runs; i++)
{
    Console.WriteLine("Start run {0}", i);
    HostApplicationBuilder localBuilder = Host.CreateApplicationBuilder(args);
    localBuilder.Logging.ClearProviders();
    localBuilder.Services.AddGrpcClient<SimpleService.SimpleServiceClient>(o =>
    {
        o.Address = new Uri("http://localhost:5172");
    });
    await Run(localBuilder, type, repeatsInRun, parallelRequests, count).ConfigureAwait(false);
    await Task.Delay(1000);
}


static async Task Run(
    HostApplicationBuilder builder,
    string type,
    int repeatInRun,
    int parallelRequests,
    int count
    )
{
    var app = builder.Build();

    for (var i = 0; i < repeatInRun; i++)
    {
        Console.WriteLine("Start repeat {0}", i);
        
        var streamingStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var bufferedCalls = Enumerable.Range(0, parallelRequests).Select(
            async _ =>
            {
                await using var scope = app.Services.CreateAsyncScope();
                var client = scope.ServiceProvider.GetRequiredService<SimpleService.SimpleServiceClient>();
                var request = new SimpleRequest { Count = count };
                var work = type == "plain" ? TestStreaming(client, request) : TestBufferedStreaming(client, request);
                await work;
            }
        );
        await Task.WhenAll(bufferedCalls).ConfigureAwait(false);
        streamingStopwatch.Stop();
        Console.WriteLine($"{type} streaming: {streamingStopwatch.ElapsedMilliseconds} ms", type);
    }
}

static async Task TestStreaming(
    SimpleService.SimpleServiceClient client,
    SimpleRequest request)
{
    // using var channel = GrpcChannel.ForAddress("http://localhost:5172");
    // var client = new SimpleService.SimpleServiceClient(channel);
    using var response = client.SimpleStreaming(request);
    var responses = new ConcurrentBag<string>();
    await foreach (var item in response.ResponseStream.ReadAllAsync())
    {
        responses.Add(item.Message);
    }
}


static async Task TestBufferedStreaming(
    SimpleService.SimpleServiceClient client,
    SimpleRequest request)
{
    using var response = client.SimpleStreamingWithBuffer(request);
    var responses = new ConcurrentBag<string>();
    await foreach (var item in response.ResponseStream.ReadAllAsync())
    {
        responses.Add(item.Message);
    }
}