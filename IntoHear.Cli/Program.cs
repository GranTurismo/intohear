using IntoHear.Core;
using Whisper.net.Ggml;

if (args.Length == 0)
{
    Console.WriteLine("Usage: IntoHear.Cli <youtube-url | local-audio-file>");
    return;
}






















var url = args[0];
var modelArg = args.Length > 1 ? args[1].ToLowerInvariant() : Environment.GetEnvironmentVariable("INTOHEAR_MODEL")?.ToLowerInvariant();

GgmlType modelType = modelArg switch
{
    "tiny" => GgmlType.Tiny,
    "small" => GgmlType.Small,
    "base" => GgmlType.Base,
    "medium" => GgmlType.Medium,
    "large" => GgmlType.Medium,
    _ => GgmlType.Medium
};

var modelFileName = modelType switch
{
    GgmlType.Tiny => "ggml-tiny.bin",
    GgmlType.Small => "ggml-small.bin",
    GgmlType.Base => "ggml-base.bin",
    GgmlType.Medium => "ggml-medium.bin",
    _ => "ggml-medium.bin",
};

var generator = new CaptionGenerator(modelPath: modelFileName, modelType: modelType);

try
{
    Console.WriteLine($"Processing {url} with model {modelType}...");
    var srt = await generator.GenerateCaptionsAsync(url);
    
    var outputFile = "captions.srt";
    await File.WriteAllTextAsync(outputFile, srt);
    
    Console.WriteLine($"Captions saved to {outputFile}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
