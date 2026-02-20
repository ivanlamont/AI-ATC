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

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "AIATC.BFF.dll"]
