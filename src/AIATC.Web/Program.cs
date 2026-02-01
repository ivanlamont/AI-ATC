using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AIATC.Web;
using AIATC.Web.Services;
using AIATC.Domain.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register domain services
builder.Services.AddScoped<AIAgentService>();
builder.Services.AddScoped<ChallengeModeService>();

// Register web services
builder.Services.AddScoped<SpeechRecognitionService>();
builder.Services.AddScoped<TextToSpeechService>();

await builder.Build().RunAsync();
