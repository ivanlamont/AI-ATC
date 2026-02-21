using AIATC.BFF;
using AIATC.BFF.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;
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

    // Log every HTTP request (including health probe hits) so we can see
    // exactly what status code is returned for each path.
    app.UseSerilogRequestLogging();

    // ── Minimal-API endpoints ──────────────────────────────────────────────────
    //
    // /healthz — dedicated liveness/readiness probe target. Always returns 200.
    app.MapGet("/healthz", () => "healthy");

    // /favicon.ico — browsers request this automatically. The WASM project uses
    // favicon.png so no .ico file exists. Redirect to the actual file.
    app.MapGet("/favicon.ico", () => Results.Redirect("/favicon.png", permanent: true));

    // ── Static file serving ────────────────────────────────────────────────────
    //
    // UseBlazorFrameworkFiles handles /_framework/* (the WASM runtime).
    // MapStaticAssets serves everything else via the .NET 10 static-web-assets
    // manifest. In Production, WASM wwwroot files are NOT physically copied to
    // /app/wwwroot — they exist only in the manifest. MapStaticAssets() is the
    // only way to serve them.
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();     // serves any BFF-owned physical wwwroot files
    app.MapStaticAssets();    // serves WASM project wwwroot files from the assets manifest

    app.MapControllers();

    // ── Blazor SPA fallback ────────────────────────────────────────────────────
    //
    // In .NET 10 Production, index.html lives only in the static-web-assets
    // manifest (Physical=False, Provider=False confirmed in logs). The only way
    // to serve it is via the MapStaticAssets() endpoint for "/index.html".
    //
    // Strategy: capture app.DataSources (the same ICollection<EndpointDataSource>
    // that MapStaticAssets() populates), then find the RouteEndpoint whose pattern
    // is "index.html" and invoke its RequestDelegate directly for every unmatched
    // Blazor client-side route (/, /simulation, /challenge-mode, etc.).
    var blazorEndpoints = ((IEndpointRouteBuilder)app).DataSources;

    app.MapFallback(async (HttpContext ctx) =>
    {
        var indexEndpoint = blazorEndpoints
            .SelectMany(ds => ds.Endpoints)
            .OfType<RouteEndpoint>()
            .FirstOrDefault(e => string.Equals(
                e.RoutePattern.RawText?.TrimStart('/'), "index.html",
                StringComparison.OrdinalIgnoreCase));

        if (indexEndpoint?.RequestDelegate is RequestDelegate rd)
        {
            // Rewrite path so the static-assets delegate serves the right file;
            // set the endpoint so downstream middleware has correct context.
            ctx.Request.Path = "/index.html";
            ctx.SetEndpoint(indexEndpoint);
            await rd(ctx);
            return;
        }

        // If we reach here MapStaticAssets() didn't register /index.html.
        // Log available routes to diagnose the publish pipeline.
        var logger = ctx.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("AIATC.BFF.SpaFallback");

        logger.LogError(
            "index.html endpoint not found in MapStaticAssets(). Registered routes: {Routes}",
            string.Join(", ", blazorEndpoints
                .SelectMany(ds => ds.Endpoints)
                .OfType<RouteEndpoint>()
                .Take(30)
                .Select(e => e.RoutePattern.RawText)));

        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        await ctx.Response.WriteAsync("index.html not found in static web assets manifest");
    });

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
