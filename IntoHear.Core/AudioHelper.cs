using System.Diagnostics;

namespace IntoHear.Core;

public static class AudioHelper
{
    public static async Task ConvertToWav16KhzAsync(string inputFile, string outputFile)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-i \"{inputFile}\" -ar 16000 -ac 1 -c:a pcm_s16le \"{outputFile}\" -y",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processStartInfo);
        if (process == null)
        {
            throw new Exception("FFmpeg process failed to start. Please ensure ffmpeg is installed and in your PATH.");
        }

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"FFmpeg conversion failed: {error}");
        }
    }
}
