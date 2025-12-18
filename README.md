# IntoHear

A C# library to generate YouTube video captions using Whisper.

## Features

- Downloads YouTube videos (audio only).
- Converts audio to 16kHz WAV (requires `ffmpeg`).
- Transcribes audio using `Whisper.net` (CPU only).
- Generates SRT subtitles.

## Prerequisites

- .NET 10.0 SDK
- `ffmpeg` installed and available in PATH.
- `yt-dlp` installed and available in PATH — required for downloading YouTube audio. Example installation commands:
  - macOS: `brew install yt-dlp`
  - Linux: `sudo apt install yt-dlp` (or `pip install -U yt-dlp`)
  - Windows: `choco install yt-dlp` (or `scoop install yt-dlp`)

Run `yt-dlp --version` to verify it is accessible from your shell.

If you prefer to run Inside Docker, the provided `Dockerfile` installs `ffmpeg` and `yt-dlp` in the runtime image so you don't need to install them on the host.

## Usage

### Library

1. Add reference to `IntoHear.Core`.
2. Use `CaptionGenerator`:

```csharp
using IntoHear.Core;

var generator = new CaptionGenerator(); // Defaults to Base model
var srt = await generator.GenerateCaptionsAsync("https://www.youtube.com/watch?v=...");
File.WriteAllText("captions.srt", srt);
```

### CLI

- Non-interactive (pass URL and optional model):

```bash
dotnet run --project IntoHear.Cli "https://www.youtube.com/watch?v=..." tiny
```

- Interactive mode (run without args) — choose options, set URL or file path, pick model, and start processing:

```bash
dotnet run --project IntoHear.Cli
```
## Linux Support

This library uses `Whisper.net.Runtime` which includes native binaries. Ensure you have the necessary dependencies for `Whisper.net` on Linux (usually `libdl`, `libpthread`, etc., which are standard).
Also ensure `ffmpeg` is installed:
```bash
sudo apt update && sudo apt install ffmpeg
```
