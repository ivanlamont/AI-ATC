using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using AIATC.Data;
using AIATC.Data.Repositories;
using AIATC.Data.Seeding;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add OpenAPI/Swagger
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "AIATC World Data Service", 
        Version = "v1",
        Description = "REST API for aviation world data including airports, runways, approaches, and aircraft types"
    });
});

// Entity Framework Configuration
builder.Services.AddDbContext<AviationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Use in-memory database for development
        options.UseInMemoryDatabase("AviationDb");
    }
    else
    {
        // Use SQL Server for production
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=AviationDb;Trusted_Connection=true;MultipleActiveResultSets=true";
        options.UseSqlServer(connectionString);
    }
});

// Repository Registration
builder.Services.AddScoped<IAircraftTypeRepository, AircraftTypeRepository>();
builder.Services.AddScoped<AviationDataSeeder>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AIATC World Data Service v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthorization();

app.MapControllers();

// Seed the database on startup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<AviationDataSeeder>();
    await seeder.SeedAsync();
}

try
{
    Log.Information("Starting AIATC.WorldDataService");
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
