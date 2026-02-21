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
    // Diagnostic version: tries three strategies to serve index.html, and dumps
    // full endpoint/file-provider info to the response body if all three fail.
    // Once we can see what is actually registered we will simplify this.
    var appDataSources = ((IEndpointRouteBuilder)app).DataSources;

    app.MapFallback(async (HttpContext ctx) =>
    {
        // ── Strategy 1: find index.html RouteEndpoint via IEndpointRouteBuilder.DataSources
        var indexEndpoint = appDataSources
            .SelectMany(ds => ds.Endpoints)
            .OfType<RouteEndpoint>()
            .FirstOrDefault(e => string.Equals(
                e.RoutePattern.RawText?.TrimStart('/'), "index.html",
                StringComparison.OrdinalIgnoreCase));

        if (indexEndpoint?.RequestDelegate is RequestDelegate rd1)
        {
            ctx.Request.Path = "/index.html";
            ctx.SetEndpoint(indexEndpoint);
            await rd1(ctx);
            return;
        }

        // ── Strategy 2: find via DI-registered CompositeEndpointDataSource
        var compositeSource = ctx.RequestServices.GetService<EndpointDataSource>();
        if (compositeSource is not null)
        {
            var ep2 = compositeSource.Endpoints
                .OfType<RouteEndpoint>()
                .FirstOrDefault(e => string.Equals(
                    e.RoutePattern.RawText?.TrimStart('/'), "index.html",
                    StringComparison.OrdinalIgnoreCase));
            if (ep2?.RequestDelegate is RequestDelegate rd2)
            {
                ctx.Request.Path = "/index.html";
                ctx.SetEndpoint(ep2);
                await rd2(ctx);
                return;
            }
        }

        // ── Strategy 3: WebRootFileProvider (works if UseBlazorFrameworkFiles()
        //    extended the provider to include WASM wwwroot files)
        var env = ctx.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var fi = env.WebRootFileProvider.GetFileInfo("index.html");
        if (fi.Exists && !fi.IsDirectory)
        {
            ctx.Response.ContentType = "text/html; charset=utf-8";
            await ctx.Response.SendFileAsync(fi);
            return;
        }

        // ── All strategies failed — dump diagnostic info so we can see exactly
        //    what is registered without needing to open Azure log stream.
        var allFromDataSources  = appDataSources.SelectMany(ds => ds.Endpoints).ToList();
        var allFromComposite    = compositeSource?.Endpoints.ToList() ?? [];
        var routeFromDataSrc    = allFromDataSources.OfType<RouteEndpoint>().ToList();
        var routeFromComposite  = allFromComposite.OfType<RouteEndpoint>().ToList();

        ctx.Response.StatusCode  = StatusCodes.Status200OK; // 200 so body is visible in browser
        ctx.Response.ContentType = "text/plain; charset=utf-8";
        await ctx.Response.WriteAsync("=== SPA Fallback Diagnostic ===\n\n");

        await ctx.Response.WriteAsync($"appDataSources.Count : {appDataSources.Count}\n");
        foreach (var ds in appDataSources)
            await ctx.Response.WriteAsync($"  {ds.GetType().Name}: {ds.Endpoints.Count} endpoints\n");

        await ctx.Response.WriteAsync($"\nRouteEndpoints from appDataSources ({routeFromDataSrc.Count}):\n");
        foreach (var e in routeFromDataSrc.Take(60))
            await ctx.Response.WriteAsync($"  Pattern='{e.RoutePattern.RawText}' | Display='{e.DisplayName}'\n");

        await ctx.Response.WriteAsync($"\nNon-RouteEndpoints from appDataSources:\n");
        foreach (var e in allFromDataSources.Where(x => x is not RouteEndpoint).Take(10))
            await ctx.Response.WriteAsync($"  {e.GetType().Name}: '{e.DisplayName}'\n");

        await ctx.Response.WriteAsync($"\nCompositeEndpointDataSource type : {compositeSource?.GetType().Name ?? "null"}\n");
        await ctx.Response.WriteAsync($"RouteEndpoints from composite ({routeFromComposite.Count}):\n");
        foreach (var e in routeFromComposite.Take(60))
            await ctx.Response.WriteAsync($"  Pattern='{e.RoutePattern.RawText}' | Display='{e.DisplayName}'\n");

        await ctx.Response.WriteAsync($"\nWebRootFileProvider: {env.WebRootFileProvider.GetType().Name}\n");
        await ctx.Response.WriteAsync($"index.html via provider: Exists={fi.Exists}\n");
        await ctx.Response.WriteAsync($"WebRootPath: {env.WebRootPath}\n");
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
