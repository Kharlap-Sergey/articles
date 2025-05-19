using System.Runtime.CompilerServices;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet(
        "dataPoints",
        ([FromQuery] int count = 1_000_000, CancellationToken cancellationToken = default) =>
            Results.Ok(GetDataPoints(count, cancellationToken)))
    .WithName("GetDataPoints")
    .WithOpenApi();

app.Run();

record SomeData(string SomeString, int SomeInt, string SomeOtherString, Guid SomeGuid);

partial class Program
{
    static async IAsyncEnumerable<SomeData> GetDataPoints(
        int dataPoints,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var fixture = new Fixture();
        for (var i = 0; i < dataPoints; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            yield return fixture.Create<SomeData>();

            if(i % 200 == 0)
                await Task.Delay(10, cancellationToken);
        }
    }
}