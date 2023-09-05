using TestApplication.Multitenancy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOptions();

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

    await next(context);
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
