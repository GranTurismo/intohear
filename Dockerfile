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

# Install ffmpeg (for audio conversion) and python3/pip to install yt-dlp
# Keep image slim by removing apt caches
RUN set -eux; \
    apt-get update; \
    DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        ffmpeg python3 python3-pip ca-certificates curl; \
    pip3 install --no-cache-dir yt-dlp; \
    apt-get purge -y --auto-remove -o APT::AutoRemove::RecommendsImportant=false; \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Add a non-root user to run the app (optional but recommended)
RUN groupadd -g 1000 app \
    && useradd -m -u 1000 -g app app \
    && chown -R app:app /app
USER app

ENTRYPOINT ["dotnet", "IntoHear.Cli.dll"]
