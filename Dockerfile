# Multi-stage build for AIATC Web Application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AIATC.slnx", "."]
COPY ["src/AIATC.Domain/AIATC.Domain.csproj", "src/AIATC.Domain/"]
COPY ["src/AIATC.Web/AIATC.Web.csproj", "src/AIATC.Web/"]
COPY ["src/AIATC.Common/AIATC.Common.csproj", "src/AIATC.Common/"]
COPY ["src/AIATC.AudioService/AIATC.AudioService.csproj", "src/AIATC.AudioService/"]
COPY ["src/AIATC.SimulationService/AIATC.SimulationService.csproj", "src/AIATC.SimulationService/"]
COPY ["src/AIATC.UserService/AIATC.UserService.csproj", "src/AIATC.UserService/"]
COPY ["src/AIATC.ScenarioService/AIATC.ScenarioService.csproj", "src/AIATC.ScenarioService/"]
COPY ["src/AIATC.AIAgentService/AIATC.AIAgentService.csproj", "src/AIATC.AIAgentService/"]

RUN dotnet restore "AIATC.slnx"

COPY . .
RUN dotnet build "AIATC.slnx" -c Release --no-restore

FROM build AS publish
RUN dotnet publish "src/AIATC.Web/AIATC.Web.csproj" -c Release -o /app/publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000
ENTRYPOINT ["dotnet", "AIATC.Web.dll"]
