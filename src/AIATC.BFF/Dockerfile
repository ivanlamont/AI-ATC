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

# Publish BFF — this also builds and embeds the WASM output via the ProjectReference
FROM build AS publish
RUN dotnet publish "src/AIATC.BFF/AIATC.BFF.csproj" -c Release -o /app/publish

# index.html is NOT copied to BFF wwwroot when Private="false" is set on the
# ProjectReference (that attribute suppresses the Blazor SDK's hosting copy target).
# The source wwwroot/index.html has unresolved placeholders (fingerprint hashes,
# import map); the processed version is written to AIATC.Web's intermediate output
# by the OverrideBlazorWebAssemblyHtmlAssets target during publish.
# Find it there and copy it to BFF wwwroot so MapFallbackToFile("index.html") works.
RUN echo "=== All index.html files in build tree ===" \
    && find /src/src/AIATC.Web/obj -name "index.html" 2>/dev/null \
    && echo "=== Copying to wwwroot ===" \
    && mkdir -p /app/publish/wwwroot \
    && PROCESSED=$(find /src/src/AIATC.Web/obj/Release -name "index.html" 2>/dev/null | head -1) \
    && if [ -n "$PROCESSED" ]; then \
           echo "Using processed: $PROCESSED"; \
           cp "$PROCESSED" /app/publish/wwwroot/index.html; \
       else \
           echo "Processed index.html not found in obj/Release — falling back to source"; \
           cp /src/src/AIATC.Web/wwwroot/index.html /app/publish/wwwroot/index.html; \
       fi \
    && echo "=== Copied index.html (first 35 lines) ===" \
    && head -35 /app/publish/wwwroot/index.html

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "AIATC.BFF.dll"]
