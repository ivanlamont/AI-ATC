using AIATC.ReferenceData.Context;
using AIATC.ScenarioService.Data;
using AIATC.ScenarioService.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;

// Load configuration from appsettings.json (already loaded by CreateDefault, but ensuring it's there)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    ;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Register configuration as a singleton so services can use it
builder.Services.AddSingleton(builder.Configuration);

// Add gRPC with gRPC-Web support for browser clients
builder.Services.AddGrpc();

// Add CORS for gRPC-Web browser clients
builder.Services.AddCors();
builder.Services.AddMemoryCache();

// Add Dapr (if needed for service discovery)
// builder.Services.AddDaprClient();

// Add database contexts
builder.Services.AddDbContext<AirspaceReferenceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AirspaceDb")));

builder.Services.AddDbContext<ScenarioUsageDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ScenarioUsageDb")));

// Add HttpClient for FlightAware service
builder.Services.AddHttpClient<IFlightAwareService, FlightAwareService>();

// Configure FlightAware options
builder.Services.Configure<FlightAwareOptions>(
    builder.Configuration.GetSection("FlightAware"));

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AirspaceReferenceDbContext>("airspace_db")
    .AddDbContextCheck<ScenarioUsageDbContext>("usage_db");

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    try
    {
        var usageDb = scope.ServiceProvider.GetRequiredService<ScenarioUsageDbContext>();
        await usageDb.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error applying database migrations");
    }
}

// Configure the HTTP request pipeline
// Enable CORS for gRPC-Web
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseCors(policy =>
{
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader()
          .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
});

app.MapGrpcService<ScenarioServiceImpl>().EnableGrpcWeb();
app.MapHealthChecks("/health");

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

try
{
    Log.Information("Starting AIATC.ScenarioService");
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
