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

```bash
dotnet run --project IntoHear.Cli "https://www.youtube.com/watch?v=..."
```

## Linux Support

This library uses `Whisper.net.Runtime` which includes native binaries. Ensure you have the necessary dependencies for `Whisper.net` on Linux (usually `libdl`, `libpthread`, etc., which are standard).
Also ensure `ffmpeg` is installed:
```bash
sudo apt update && sudo apt install ffmpeg
```
