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
    // UseBlazorFrameworkFiles() is intentionally absent — it uses the BFF's
    // compiled-in static-web-assets manifest (fingerprints from the BFF build)
    // and caused 500s when those fingerprints differed from what wasm-publish
    // put in index.html.
    //
    // UseStaticFiles() serves physical files from /app/wwwroot (populated by the
    // Dockerfile: source wwwroot files + wasm-publish output).
    //
    // By default UseStaticFiles silently skips files whose extension isn't in the
    // MIME registry (it calls next() rather than returning 404). Blazor WASM uses
    // several non-standard extensions: .dat (ICU data), .dll (app assemblies),
    // .blat (satellite resources), .pdb (debug symbols). Without explicit mappings,
    // requests for these fall through to MapFallbackToFile which returns index.html
    // (wrong content), causing the WASM runtime to fail after the loading bar.
    // ServeUnknownFileTypes = true ensures every physical file in wwwroot is served.
    var wasmContentTypes = new FileExtensionContentTypeProvider();
    wasmContentTypes.Mappings[".dat"]  = "application/octet-stream";
    wasmContentTypes.Mappings[".dll"]  = "application/octet-stream";
    wasmContentTypes.Mappings[".blat"] = "application/octet-stream";
    wasmContentTypes.Mappings[".pdb"]  = "application/octet-stream";
    app.UseStaticFiles(new StaticFileOptions
    {
        ContentTypeProvider  = wasmContentTypes,
        ServeUnknownFileTypes = true,
        DefaultContentType   = "application/octet-stream",
    });

    // MapStaticAssets serves compressed files from the BFF's static-web-assets
    // manifest. This is required for files that dotnet publish for Blazor WASM
    // emits as compressed-only variants (e.g. .dll.br, .dat.br): UseStaticFiles
    // sees no matching physical file and falls through; MapStaticAssets serves
    // the .br variant with Content-Encoding: br.
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
