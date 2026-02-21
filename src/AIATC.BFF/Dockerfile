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
FROM build AS wasm-publish
RUN dotnet publish "src/AIATC.Web/AIATC.Web.csproj" -c Release -o /wasm/publish
RUN echo "=== Processed index.html ===" \
    && head -40 /wasm/publish/wwwroot/index.html \
    && echo "=== wasm-publish wwwroot top-level ===" \
    && find /wasm/publish/wwwroot -maxdepth 2 -not -path '*/_framework/*' | sort \
    && echo "=== _framework file count ===" \
    && find /wasm/publish/wwwroot/_framework -type f | wc -l

# ── Stage: publish ────────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "src/AIATC.BFF/AIATC.BFF.csproj" -c Release -o /app/publish

# Copy the ENTIRE WASM publish wwwroot into the BFF wwwroot so that every file
# referenced by index.html (framework JS, WASM binaries, CSS, JS) comes from the
# SAME publish run and therefore has matching fingerprints.
#
# Private="false" on the ProjectReference means the BFF's own publish doesn't
# run AIATC.Web's publish targets, so its MapStaticAssets() endpoints may carry
# different fingerprints than what the standalone wasm-publish produced.
# UseStaticFiles() (physical files, runs before endpoint routing) will serve the
# wasm-publish copies; MapStaticAssets() endpoints are a secondary path that
# only matters for paths not found physically.
COPY --from=wasm-publish /wasm/publish/wwwroot /app/publish/wwwroot
RUN echo "=== Final BFF wwwroot top-level ===" \
    && find /app/publish/wwwroot -maxdepth 2 -not -path '*/_framework/*' | sort \
    && echo "=== bootstrap.min.css present ===" \
    && ls -la /app/publish/wwwroot/lib/bootstrap/dist/css/bootstrap.min.css 2>&1 \
    && echo "=== _framework file count ===" \
    && find /app/publish/wwwroot/_framework -type f | wc -l

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "AIATC.BFF.dll"]
