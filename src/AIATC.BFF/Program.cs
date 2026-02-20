using AIATC.BFF;
using AIATC.BFF.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Distributed memory cache is required by session middleware
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.Name = ".AIATC.Session";
        options.IdleTimeout = TimeSpan.FromMinutes(30);
    });

    builder.Services
        .AddAuthentication("AiatcCookie")
        .AddCookie("AiatcCookie", options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.Name = ".AIATC.Auth";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            // For API and auth-info endpoints return 401 instead of redirecting to
            // a login page — these are called via fetch() and expect JSON, not HTML.
            options.Events.OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api") ||
                    ctx.Request.Path.StartsWithSegments("/auth"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            };
        });

    builder.Services.AddAuthorization();

    // Named HttpClients — base addresses and API keys are NOT stored here;
    // they are injected per-request from Options so secrets stay in config files.
    builder.Services.AddHttpClient("azure-oauth");
    builder.Services.AddHttpClient("google-oauth");
    builder.Services.AddHttpClient("azure-speech");
    builder.Services.AddHttpClient("flightaware", client =>
    {
        client.BaseAddress = new Uri("https://aeroapi.flightaware.com");
    });

    builder.Services.AddControllers();

    builder.Services.Configure<BffOAuthOptions>(builder.Configuration.GetSection("OAuth"));
    builder.Services.Configure<AzureSpeechBffOptions>(builder.Configuration.GetSection("AzureSpeech"));
    builder.Services.Configure<FlightAwareBffOptions>(builder.Configuration.GetSection("FlightAware"));

    // Singleton speech-token cache shared across requests
    builder.Services.AddSingleton<SpeechTokenCache>();

    var app = builder.Build();

    // ACA terminates TLS — trust X-Forwarded-Proto so Request.Scheme is 'https',
    // which is required for correct redirect URIs and Secure cookie issuance.
    // KnownNetworks/KnownProxies are cleared so ACA's internal proxy (10.x.x.x)
    // is trusted; by default only loopback is trusted.
    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    };
    forwardedHeadersOptions.KnownNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedHeadersOptions);

    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();

    // Serve the Blazor WASM static files (produced by the ProjectReference to AIATC.Web)
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.MapControllers();

    // All unmatched routes fall back to index.html so the Blazor router takes over
    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BFF terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
