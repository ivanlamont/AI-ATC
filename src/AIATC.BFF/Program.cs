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
    // Log index.html availability at startup so the ACA log stream immediately
    // shows whether the file is physically present in the publish wwwroot.
    var webRoot = app.Environment.WebRootPath ?? "(null)";
    var indexPhysical = File.Exists(Path.Combine(webRoot, "index.html"));
    var indexProvider = app.Environment.WebRootFileProvider.GetFileInfo("index.html").Exists;
    app.Logger.LogInformation(
        "WebRootPath={WebRootPath} | index.html physical={Physical} provider={Provider}",
        webRoot, indexPhysical, indexProvider);

    app.UseForwardedHeaders(forwardedHeadersOptions);

    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();

    // Log every HTTP request (including health probe hits) so we can see
    // exactly what status code is returned for each path.
    app.UseSerilogRequestLogging();

    // ── Minimal-API endpoints registered BEFORE static-file middleware ──────────
    //
    // /healthz — dedicated liveness/readiness probe target. Always returns 200.
    // Using a minimal API keeps this completely independent of MVC, sessions,
    // auth, and static-file pipelines. Update the Container App probe path to
    // /healthz in the Azure Portal or by re-running infra/main.bicep.
    app.MapGet("/healthz", () => "healthy");

    // /favicon.ico — browsers request this automatically. The WASM project uses
    // favicon.png so no .ico file exists. Redirect to the actual file so the
    // browser gets the icon and ACA never times out on the request.
    app.MapGet("/favicon.ico", () => Results.Redirect("/favicon.png", permanent: true));

    // ── Static file serving ────────────────────────────────────────────────────
    //
    // UseBlazorFrameworkFiles handles /_framework/* (the WASM runtime).
    // MapStaticAssets serves everything else in wwwroot (js/, css/, appsettings.json, etc.)
    // via the .NET 10 static-web-assets manifest — UseStaticFiles() alone is insufficient
    // because WASM wwwroot files are only in the manifest, not physically copied to wwwroot.
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();     // serves any BFF-owned physical wwwroot files
    app.MapStaticAssets();    // serves WASM project wwwroot files from the assets manifest

    app.MapControllers();

    // ── Blazor SPA fallback ────────────────────────────────────────────────────
    //
    // In .NET 10 Production mode, WASM wwwroot files (including index.html) live
    // in the static-web-assets manifest and are served by MapStaticAssets().
    // MapFallbackToFile("index.html") relies on IWebHostEnvironment.WebRootFileProvider
    // which in Production only covers the PHYSICAL wwwroot directory. If index.html
    // is not physically copied there at publish time, it returns 404 for every
    // Blazor client-side route (/, /simulation, /challenge-mode, etc.).
    //
    // This explicit MapFallback:
    //   1. Logs where it looked (visible in the ACA log stream — very helpful).
    //   2. Reads index.html directly from the physical publish output path.
    //   3. Falls back to IWebHostEnvironment.WebRootFileProvider (which may
    //      include the manifest-based provider depending on .NET version/config).
    app.MapFallback(async (HttpContext ctx) =>
    {
        var env = ctx.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var logger = ctx.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("AIATC.BFF.SpaFallback");

        // Strategy 1: physical file at <WebRootPath>/index.html
        var physicalPath = Path.Combine(env.WebRootPath ?? string.Empty, "index.html");
        if (File.Exists(physicalPath))
        {
            ctx.Response.ContentType = "text/html; charset=utf-8";
            await ctx.Response.SendFileAsync(physicalPath);
            return;
        }

        // Strategy 2: IWebHostEnvironment.WebRootFileProvider (may include manifest)
        var fileInfo = env.WebRootFileProvider.GetFileInfo("index.html");
        if (fileInfo.Exists)
        {
            ctx.Response.ContentType = "text/html; charset=utf-8";
            await ctx.Response.SendFileAsync(fileInfo);
            return;
        }

        logger.LogError(
            "index.html not found. WebRootPath={WebRootPath} Physical={Physical} Provider={Provider}",
            env.WebRootPath,
            File.Exists(physicalPath),
            fileInfo.Exists);

        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        await ctx.Response.WriteAsync("index.html not found");
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
