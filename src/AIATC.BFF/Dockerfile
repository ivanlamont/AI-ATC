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
RUN dotnet publish "src/AIATC.Web/AIATC.Web.csproj" -c Release -o /wasm/publish \
    && echo "=== Processed index.html (first 35 lines) ===" \
    && head -35 /wasm/publish/wwwroot/index.html

# ── Stage: publish ────────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "src/AIATC.BFF/AIATC.BFF.csproj" -c Release -o /app/publish

# Private="false" on the ProjectReference suppresses the SDK target that would
# normally copy the processed index.html into the BFF wwwroot. Copy it from the
# dedicated wasm-publish stage instead.
COPY --from=wasm-publish /wasm/publish/wwwroot/index.html /app/publish/wwwroot/index.html

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "AIATC.BFF.dll"]
