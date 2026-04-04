using System.Text.Json;
using AIATC.BFF;
using AIATC.BFF.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Npgsql;
using Serilog;
using Yarp.ReverseProxy.Configuration;

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
    builder.Services.AddHttpClient("piper-tts");

    builder.Services.AddControllers();

    builder.Services.Configure<BffOAuthOptions>(builder.Configuration.GetSection("OAuth"));
    builder.Services.Configure<AzureSpeechBffOptions>(builder.Configuration.GetSection("AzureSpeech"));
    builder.Services.Configure<FlightAwareBffOptions>(builder.Configuration.GetSection("FlightAware"));
    builder.Services.Configure<PiperTtsBffOptions>(builder.Configuration.GetSection("PiperTts"));

    // Singleton speech-token cache shared across requests
    builder.Services.AddSingleton<SpeechTokenCache>();

    // ── DataProtection — persist keys to Postgres so auth cookies survive restarts
    // and work correctly across multiple replicas.
    //
    // We test the connection and create the table BEFORE builder.Build() so that
    // if Postgres is unavailable (cold start race, misconfiguration, etc.) we fall
    // back to in-memory keys gracefully rather than crashing the process.
    var pgConnStr = builder.Configuration.GetConnectionString("ScenarioUsageDb");
    var useDbDataProtection = false;

    if (!string.IsNullOrWhiteSpace(pgConnStr))
    {
        try
        {
            await using var conn = new Npgsql.NpgsqlConnection(pgConnStr);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS "DataProtectionKeys" (
                    "Id"           serial NOT NULL,
                    "FriendlyName" text   NULL,
                    "Xml"          text   NULL,
                    CONSTRAINT "PK_DataProtectionKeys" PRIMARY KEY ("Id")
                )
                """;
            await cmd.ExecuteNonQueryAsync();
            useDbDataProtection = true;
            Log.Information("DataProtection keys will be persisted to Postgres");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Postgres unavailable for DataProtection; using in-memory keys (sessions will not survive restarts)");
        }
    }

    if (useDbDataProtection)
    {
        builder.Services.AddDbContext<BffDbContext>(options =>
            options.UseNpgsql(pgConnStr));
        builder.Services
            .AddDataProtection()
            .SetApplicationName("aiatc-bff")
            .PersistKeysToDbContext<BffDbContext>();
    }
    else
    {
        builder.Services
            .AddDataProtection()
            .SetApplicationName("aiatc-bff");
    }

    // ── YARP reverse proxy — forwards browser gRPC-Web calls to ScenarioService.
    // The browser points its gRPC channel at the BFF origin; YARP forwards any
    // request whose path starts with the gRPC package path to the internal service.
    // ScenarioService no longer needs external ingress.
    var scenarioAddress = builder.Configuration["ScenarioService:Address"]
        ?? "http://localhost:5001";

    // HTTP/2 over cleartext (h2c) is disabled by default in .NET's HttpClient.
    // YARP forwards gRPC calls using HTTP/2; when the upstream is http:// (local dev)
    // we must opt in. Production uses https:// so TLS ALPN handles HTTP/2 negotiation.
    if (scenarioAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
    }

    builder.Services.AddReverseProxy()
        .LoadFromMemory(
            routes:
            [
                new RouteConfig
                {
                    RouteId   = "scenario-grpc",
                    ClusterId = "scenario",
                    Match     = new RouteMatch { Path = "/aiatc.scenario.ScenarioService/{**catch-all}" }
                }
            ],
            clusters:
            [
                new ClusterConfig
                {
                    ClusterId    = "scenario",
                    Destinations = new Dictionary<string, DestinationConfig>
                    {
                        ["d1"] = new DestinationConfig { Address = scenarioAddress }
                    }
                }
            ]);

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

    // In Development, _content/ NuGet package static web assets from referenced WASM
    // projects are never propagated to the BFF's static web assets manifest (only
    // wwwroot and obj/ content roots are propagated). Parse the WASM project's own
    // development manifest to find any _content/ NuGet packages and register a
    // PhysicalFileProvider for each one so they can be served at their _content/ paths.
    // This is required for Microsoft.DotNet.HotReload.WebAssembly.Browser which the
    // WASM runtime loads as a library initializer in Debug builds.
    if (app.Environment.IsDevelopment())
    {
        var wasmManifestPath = Path.GetFullPath(Path.Combine(
            app.Environment.ContentRootPath,
            "../AIATC.Web/obj/Debug/net10.0/staticwebassets.development.json"));

        if (File.Exists(wasmManifestPath))
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(wasmManifestPath));
                var roots = doc.RootElement.GetProperty("ContentRoots")
                    .EnumerateArray().Select(r => r.GetString()!).ToArray();

                if (doc.RootElement.TryGetProperty("Root", out var rootNode) &&
                    rootNode.TryGetProperty("Children", out var topChildren) &&
                    topChildren.TryGetProperty("_content", out var contentNode) &&
                    contentNode.TryGetProperty("Children", out var packages))
                {
                    foreach (var pkg in packages.EnumerateObject())
                    {
                        if (!pkg.Value.TryGetProperty("Children", out var files)) continue;

                        // Find the ContentRootIndex used by any file in this package
                        foreach (var file in files.EnumerateObject())
                        {
                            if (file.Value.TryGetProperty("Asset", out var asset) &&
                                asset.TryGetProperty("ContentRootIndex", out var idxEl) &&
                                idxEl.GetInt32() < roots.Length)
                            {
                                var physRoot = roots[idxEl.GetInt32()].TrimEnd('\\', '/');
                                if (Directory.Exists(physRoot))
                                {
                                    app.UseStaticFiles(new StaticFileOptions
                                    {
                                        FileProvider = new PhysicalFileProvider(physRoot),
                                        RequestPath = $"/_content/{pkg.Name}",
                                        ContentTypeProvider = wasmContentTypes,
                                        ServeUnknownFileTypes = true,
                                        DefaultContentType = "application/octet-stream",
                                    });
                                    Log.Information(
                                        "Dev: serving /_content/{Package} from {Path}",
                                        pkg.Name, physRoot);
                                }
                                break; // one registration per package is enough
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Dev: could not parse WASM static web assets manifest at {Path}",
                    wasmManifestPath);
            }
        }
        else
        {
            Log.Warning(
                "Dev: WASM static web assets manifest not found at {Path}. " +
                "Run 'dotnet build src/AIATC.Web' first.", wasmManifestPath);
        }
    }

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

    // gRPC-Web proxy — forwards ScenarioService calls from the browser to the
    // internal ScenarioService container. Must be mapped before the SPA fallback.
    app.MapReverseProxy();

    // ── Blazor SPA fallback ────────────────────────────────────────────────────
    //
    // In .NET 10, index.html is NOT in the static-web-assets manifest — the
    // Blazor SDK publishes it as a physical file to the BFF's wwwroot/.
    // MapFallbackToFile serves it for every unmatched client-side route
    // (/, /simulation, /challenge-mode, etc.) using the physical file provider.
    app.MapFallbackToFile("index.html");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BFF terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
