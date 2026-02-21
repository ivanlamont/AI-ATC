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
    // All WASM static files (including _framework/*) are physical files in
    // /app/wwwroot, copied there by the Dockerfile from the standalone wasm-publish
    // stage. UseStaticFiles() serves them all via the PhysicalFileProvider.
    //
    // UseBlazorFrameworkFiles() is intentionally omitted: it uses the BFF's
    // compiled-in static-web-assets manifest which has DIFFERENT fingerprints than
    // the wasm-publish files (two separate dotnet publish runs). If it were enabled,
    // it would intercept _framework/* requests and return 500 for fingerprints it
    // doesn't recognise, and serve a stale blazor.boot.json causing the preload
    // fingerprint mismatch warning.
    //
    // .wasm files need the application/wasm content type for the browser to accept
    // them; the default provider in ASP.NET Core includes this mapping.
    // UseStaticFiles serves physical files from /app/wwwroot (populated by the
    // Dockerfile cp + COPY steps: source wwwroot files + wasm-publish output).
    app.UseStaticFiles();

    // MapStaticAssets serves compressed files from the BFF's static-web-assets
    // manifest. This is required for files that dotnet publish only emits as
    // compressed variants (e.g. _framework/icudt_EFIGS.*.dat is published as
    // .dat.br only — UseStaticFiles() would 404 it). The BFF manifest fingerprints
    // match the wasm-publish fingerprints because both builds use the same SDK
    // version and NuGet packages (same file content → same SHA256 fingerprint).
    //
    // UseBlazorFrameworkFiles() is intentionally absent — it caused 500s by
    // intercepting _framework/* requests using a manifest-backed provider that
    // failed to match files it should have passed through.
    app.MapStaticAssets();

    app.MapControllers();

    // ── Blazor SPA fallback ────────────────────────────────────────────────────
    //
    // In .NET 10, index.html is NOT in the static-web-assets manifest — the
    // Blazor SDK publishes it as a physical file to the BFF's wwwroot/.
    // MapFallbackToFile serves it for every unmatched client-side route
    // (/, /simulation, /challenge-mode, etc.) using the physical file provider.
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
