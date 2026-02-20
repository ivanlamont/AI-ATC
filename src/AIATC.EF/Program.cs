using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

try
{
    Log.Information("Starting AIATC.EF");
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
