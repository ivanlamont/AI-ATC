FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["AIATC.slnx", "."]
COPY ["src/AIATC.BFF/AIATC.BFF.csproj", "src/AIATC.BFF/"]
COPY ["src/AIATC.Web/AIATC.Web.csproj", "src/AIATC.Web/"]
COPY ["src/AIATC.Domain/AIATC.Domain.csproj", "src/AIATC.Domain/"]
COPY ["src/AIATC.Common/AIATC.Common.csproj", "src/AIATC.Common/"]
COPY ["src/AIATC.ScenarioService/AIATC.ScenarioService.csproj", "src/AIATC.ScenarioService/"]

RUN dotnet restore "src/AIATC.BFF/AIATC.BFF.csproj"

# Copy sources
COPY src/AIATC.BFF ./src/AIATC.BFF
COPY src/AIATC.Web ./src/AIATC.Web
COPY src/AIATC.Domain ./src/AIATC.Domain
COPY src/AIATC.Common ./src/AIATC.Common
COPY src/AIATC.ScenarioService ./src/AIATC.ScenarioService

# ── Stage: wasm-publish ──────────────────────────────────────────────────────
# Publish AIATC.Web standalone so the Blazor SDK runs
# OverrideBlazorWebAssemblyHtmlAssets and produces a fully-processed index.html
# (fingerprint placeholders resolved, import map populated). This does NOT happen
# when the WASM project is only *built* as a Private="false" dependency of the BFF.
#
# In .NET 10, dotnet publish for Blazor WASM only puts _framework/ and index.html
# as physical files in the output wwwroot/. All other assets (lib/, css/, js/) are
# served via the static-web-assets manifest (MapStaticAssets endpoints), not as
# physical copies. We handle this in the publish stage below.
FROM build AS wasm-publish
RUN dotnet publish "src/AIATC.Web/AIATC.Web.csproj" -c Release -o /wasm/publish
RUN echo "=== Processed index.html ===" \
    && head -40 /wasm/publish/wwwroot/index.html \
    && echo "=== wasm-publish wwwroot contents (excl _framework) ===" \
    && find /wasm/publish/wwwroot -not -path '*/_framework/*' | sort \
    && echo "=== _framework file count ===" \
    && find /wasm/publish/wwwroot/_framework -type f | wc -l

# ── Stage: publish ────────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "src/AIATC.BFF/AIATC.BFF.csproj" -c Release -o /app/publish

# ── Seed wwwroot with source static files ────────────────────────────────────
#
# In .NET 10, dotnet publish for Blazor WASM does NOT copy lib/, css/, js/ as
# physical files — they're only in the static-web-assets manifest. Because we
# serve everything via UseStaticFiles() (physical files), we must populate the
# physical wwwroot ourselves.
#
# Step 1: Copy ALL source AIATC.Web wwwroot files (bootstrap, css, js, data,
#         images, appsettings, source index.html) into the BFF publish output.
RUN mkdir -p /app/publish/wwwroot \
    && cp -r src/AIATC.Web/wwwroot/. /app/publish/wwwroot/

# Step 2: Overwrite with wasm-publish output. This replaces the source
#         index.html (placeholder fingerprints) with the processed version
#         (resolved fingerprints, populated importmap) and adds _framework/.
#         docker COPY merges directories; source overwrites on name collision.
COPY --from=wasm-publish /wasm/publish/wwwroot /app/publish/wwwroot

RUN echo "=== Final BFF wwwroot (excl _framework) ===" \
    && find /app/publish/wwwroot -not -path '*/_framework/*' | sort \
    && echo "=== _framework file count ===" \
    && find /app/publish/wwwroot/_framework -type f | wc -l

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "AIATC.BFF.dll"]
