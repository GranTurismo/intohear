# Multi-stage build for IntoHear CLI (net10.0)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files to leverage layer caching
COPY ["IntoHear.Cli/IntoHear.Cli.csproj", "IntoHear.Cli/"]
COPY ["IntoHear.Core/IntoHear.Core.csproj", "IntoHear.Core/"]
COPY ["IntoHear.sln", "."]

RUN dotnet restore

# Copy remaining sources and publish
COPY . .
RUN dotnet publish "IntoHear.Cli/IntoHear.Cli.csproj" -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "IntoHear.Cli.dll"]
