using System.Text.Json.Serialization;
using Readiness.Api;
using Readiness.Api.Readiness;

var builder = WebApplication.CreateBuilder(args);

var defaultCutoverDate = builder.Configuration.GetValue<DateOnly?>("Cutover:DefaultDate")
    ?? throw new InvalidOperationException("Set Cutover:DefaultDate in appsettings.json.");

builder.Services.AddSingleton(new CutoverSettings(defaultCutoverDate));
builder.Services.AddSingleton<IProductCatalog>(new InMemoryProductCatalog(SeedData.MappedProductCodes));
builder.Services.AddSingleton<IClientRepository>(
    new InMemoryClientRepository(SeedData.CreateClients(), SeedData.CreatePolicies()));
builder.Services.AddSingleton<ReadinessEvaluator>();

// Send enums as their names ("Blocked"), not numbers.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.MapReadinessEndpoints();
app.MapPolicyEndpoints();

app.Run();
