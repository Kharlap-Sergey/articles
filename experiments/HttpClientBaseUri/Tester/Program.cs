using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddHttpClient(
    "test_without",
    (configure) =>
    {
        configure.BaseAddress = new Uri("http://localhost:5000/prefix_without");
    });

builder.Services.AddHttpClient(
    "test_with",
    (configure) =>
    {
        configure.BaseAddress = new Uri("http://localhost:5000/prefix_with/");
    });

var app = builder.Build();


var factory = app.Services.GetService<IHttpClientFactory>() ?? throw new NullReferenceException();

var clientWithout = factory.CreateClient("test_without");
var clientWith = factory.CreateClient("test_with");

await clientWithout.GetAsync("/after_with"); // http://localhost:5000/after_with 
await clientWithout.GetAsync("after_without"); // http://localhost:5000/after_without

await clientWith.GetAsync("/after_with"); // http://localhost:5000/after_with
await clientWith.GetAsync("after_without"); // http://localhost:5000/prefix_with/after_without

await Task.Delay(10000);