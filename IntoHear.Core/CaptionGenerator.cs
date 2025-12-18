using System;
using System.Collections.Generic;
using Whisper.net;
using Whisper.net.Ggml;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IntoHear.Core;

public class CaptionGenerator
{
    private readonly string _modelPath;
    private readonly GgmlType _modelType;

    public CaptionGenerator(string modelPath = "ggml-medium.bin", GgmlType modelType = GgmlType.Medium)
    {
        _modelPath = modelPath;
        _modelType = modelType;
    }

    public async Task<string> GenerateCaptionsAsync(string videoUrl)
    {
        // 1. Download Model if not exists
        if (!File.Exists(_modelPath))
        {
            Console.WriteLine($"Downloading Whisper model ({_modelType})...");
            using var httpClient = new HttpClient();
            var downloader = new WhisperGgmlDownloader(httpClient);
            using var modelStream = await downloader.GetGgmlModelAsync(_modelType);
            using var fileWriter = File.OpenWrite(_modelPath);
            await modelStream.CopyToAsync(fileWriter);
        }

        // 2. Download Video Audio using the yt-dlp CLI
        var tempBase = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var outputTemplate = tempBase + ".%(ext)s";

        Console.WriteLine($"Downloading audio for: {videoUrl}");
        var ytDlpExit = await RunProcessAsync("yt-dlp", $"-f bestaudio -o \"{outputTemplate}\" \"{videoUrl}\" --no-playlist");
        if (ytDlpExit != 0)
        {
            throw new Exception("yt-dlp failed to download the audio. Ensure yt-dlp is installed and available in PATH.");
        }

        // Find the downloaded file (yt-dlp will replace %(ext)s with actual extension)
        var downloaded = Directory.GetFiles(Path.GetDirectoryName(tempBase)!, Path.GetFileName(tempBase) + ".*").FirstOrDefault();
        if (downloaded == null)
        {
            throw new Exception("Failed to locate the downloaded audio file.");
        }

        var tempAudioFile = downloaded;
        var wavFile = Path.ChangeExtension(tempAudioFile, ".wav");

        try 
        {
            // audio already downloaded via yt-dlp earlier

            // 3. Convert to 16kHz WAV
            Console.WriteLine("Converting audio to 16kHz WAV...");
            await AudioHelper.ConvertToWav16KhzAsync(tempAudioFile, wavFile);

            // 4. Transcribe
            Console.WriteLine("Transcribing...");
            using var factory = WhisperFactory.FromPath(_modelPath);
            using var processor = factory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            using var fileStream = File.OpenRead(wavFile);
            var segments = new List<SegmentData>();
            
            await foreach (var segment in processor.ProcessAsync(fileStream))
            {
                segments.Add(segment);
            }

            // 5. Generate SRT
            return SubtitleHelper.FormatToSrt(segments);
        }
        finally
        {
            if (File.Exists(tempAudioFile)) File.Delete(tempAudioFile);
            if (File.Exists(wavFile)) File.Delete(wavFile);
        }

    
    }

    private static bool IsExecutableInPath(string fileName)
    {
        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
        var candidateNames = new List<string> { fileName };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var exts = Environment.GetEnvironmentVariable("PATHEXT")?.Split(';') ?? new[] { ".exe", ".bat", ".cmd" };
            candidateNames = exts.Select(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase) ? fileName : fileName + ext).ToList();
        }

        foreach (var dir in paths)
        {
            try
            {
                foreach (var cand in candidateNames)
                {
                    var full = Path.Combine(dir, cand);
                    if (File.Exists(full)) return true;
                }
            }
            catch
            {
                // Ignore malformed PATH entries
            }
        }

        return false;
    }

    private static async Task<int> RunProcessAsync(string fileName, string arguments)
    {
        // quick PATH check - provide actionable hint early
        if (!IsExecutableInPath(fileName))
        {
            string hint = fileName switch
            {
                "yt-dlp" when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "brew install yt-dlp",
                "yt-dlp" when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "choco install yt-dlp (or scoop install yt-dlp)",
                "yt-dlp" when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "sudo apt install yt-dlp (or pip install -U yt-dlp)",
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => $"brew install {fileName}",
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => $"choco install {fileName}",
                _ => $"sudo apt install {fileName} (or pip install {fileName})"
            };

            throw new Exception($"Executable '{fileName}' not found in PATH. Ensure '{fileName}' is installed and accessible. Example: {hint}");
        }

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process? process;
        try
        {
            process = Process.Start(psi);
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            // Provide platform-specific install hints
            string hint = fileName switch
            {
                "yt-dlp" when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "brew install yt-dlp",
                "yt-dlp" when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "choco install yt-dlp (or scoop install yt-dlp)",
                "yt-dlp" when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "sudo apt install yt-dlp (or pip install -U yt-dlp)",
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => $"brew install {fileName}",
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => $"choco install {fileName}",
                _ => $"sudo apt install {fileName} (or pip install {fileName})"
            };

            throw new Exception($"Failed to start process '{fileName}': {ex.Message}. Make sure '{fileName}' is installed and available in your PATH. Example: {hint}", ex);
        }

        if (process == null)
        {
            throw new Exception($"Failed to start process '{fileName}'. Make sure it is installed and in your PATH.");
        }

        var stdOutTask = process.StandardOutput.ReadToEndAsync();
        var stdErrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var stdout = await stdOutTask;
        var stderr = await stdErrTask;

        if (!string.IsNullOrWhiteSpace(stdout)) Console.WriteLine(stdout);
        if (!string.IsNullOrWhiteSpace(stderr)) Console.Error.WriteLine(stderr);

        return process.ExitCode;
    }
}

