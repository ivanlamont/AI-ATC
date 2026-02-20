FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file and project files
COPY ["AIATC.slnx", "."]
COPY ["src/AIATC.ScenarioService/AIATC.ScenarioService.csproj", "src/AIATC.ScenarioService/"]
COPY ["src/AIATC.ScenarioService.Data/AIATC.ScenarioService.Data.csproj", "src/AIATC.ScenarioService.Data/"]
COPY ["src/AIATC.ReferenceData/AIATC.ReferenceData.csproj", "src/AIATC.ReferenceData/"]
COPY ["src/AIATC.ReferenceData.Context/AIATC.ReferenceData.Context.csproj", "src/AIATC.ReferenceData.Context/"]
COPY ["src/AIATC.Domain/AIATC.Domain.csproj", "src/AIATC.Domain/"]
COPY ["src/AIATC.Common/AIATC.Common.csproj", "src/AIATC.Common/"]

# Restore dependencies
RUN dotnet restore "src/AIATC.ScenarioService/AIATC.ScenarioService.csproj"

# Copy all source code
COPY src/ ./src/

# Build and publish
WORKDIR /src/src/AIATC.ScenarioService
RUN dotnet publish -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Configure ports
ENV ASPNETCORE_URLS=http://+:5001
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s \
  CMD curl -f http://localhost:5001/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "AIATC.ScenarioService.dll"]
