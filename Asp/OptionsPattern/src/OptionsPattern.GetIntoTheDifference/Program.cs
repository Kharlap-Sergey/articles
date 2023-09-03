using Microsoft.Extensions.Options;
using OptionsPattern.GetIntoTheDifference;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

#region TestOptions configurations
builder.Services.AddOptions<TestOptions>()
    .Bind(builder.Configuration.GetSection(TestOptions.SectionName));
builder.Services.PostConfigure<TestOptions>(options =>
{
    options.AppendingProperty += "[in_post_configure]";
});
builder.Services.Configure<TestOptions>(options =>
{
    //ussed to show that the configuration is applied in order of registration
    options.AppendingProperty += "[after_binding]";
});
#endregion

#region TestValidationOptions configurations
builder.Services.AddOptions<TestValidationOptions>()
    .Configure(options =>
    {
        options.DataAnatationProperty = "Configured in class";
        options.ValidationPredicateProperty = "Configured in class";
        options.ExternalValidatorProperty = "Configured in class";
    })
    .ValidateDataAnnotations()
    .Validate(options =>
    {
        return options.ValidationPredicateProperty != null;
    }, "ConfigurationProperty is null")
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<TestValidationOptions>, TestValidationOptionsValidator>();
#endregion

#region TestNamedOptions configurations
builder.Services.ConfigureAll<TestNamedOptions>(options =>
{
    options.PropertyToDemoAll = "Configured in ConfigureAll";
});
builder.Services.AddOptions<TestNamedOptions>("Name1")
    .Configure(options =>
    {
        options.Property = "Name1 otpions property";
    });
builder.Services.AddOptions<TestNamedOptions>("Name2")
     .Configure(options =>
     {
         options.Property = "Name2 otpions property";
     });
#endregion

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();