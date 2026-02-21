using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AIATC.Web;
using AIATC.Web.Services;
using AIATC.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.BrowserConsole()
    .CreateLogger();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton(builder.Configuration);

// Domain services
builder.Services.AddScoped<AIAgentService>();
builder.Services.AddScoped<ChallengeModeService>();

// Web services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AuthService>());
builder.Services.AddScoped<SpeechRecognitionService>();
builder.Services.AddScoped<TextToSpeechService>();
builder.Services.AddScoped<AviationVocabularyService>();
builder.Services.AddScoped<IFlightAwareService, FlightAwareService>();

// ScenarioService gRPC client
builder.Services.AddScoped<IScenarioServiceClient, ScenarioServiceClient>();

// Azure services
builder.Services.AddScoped<IAzureConfigurationService, AzureConfigurationService>();
builder.Services.AddScoped<IAzureSpeechService, AzureSpeechService>();

// Navigation service
builder.Services.AddScoped<INavigationService, NavigationService>();

// Airport data service
builder.Services.AddScoped<IAirportDataService, AirportDataService>();

var host = builder.Build();
await host.RunAsync();
