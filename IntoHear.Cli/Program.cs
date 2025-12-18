using IntoHear.Core;
using Whisper.net.Ggml;

// Helper: convert string to GgmlType
GgmlType ParseModelType(string? modelArg)
{
    if (string.IsNullOrWhiteSpace(modelArg)) return GgmlType.Medium;
    return modelArg.ToLowerInvariant() switch
    {
        "tiny" => GgmlType.Tiny,
        "small" => GgmlType.Small,
        "base" => GgmlType.Base,
        "medium" => GgmlType.Medium,
        "large" => GgmlType.Medium,
        _ => GgmlType.Medium
    };
}

string GetModelFileName(GgmlType modelType) => modelType switch
{
    GgmlType.Tiny => "ggml-tiny.bin",
    GgmlType.Small => "ggml-small.bin",
    GgmlType.Base => "ggml-base.bin",
    GgmlType.Medium => "ggml-medium.bin",
    _ => "ggml-medium.bin",
};

// Get default model from arg or env
var modelArg = args.Length > 1 ? args[1].ToLowerInvariant() : Environment.GetEnvironmentVariable("INTOHEAR_MODEL")?.ToLowerInvariant();
var defaultModel = ParseModelType(modelArg);

// If arguments were supplied, keep previous behaviour (non-interactive)
if (args.Length > 0)
{
    var url = args[0];
    var modelType = defaultModel;
    var modelFileName = GetModelFileName(modelType);

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

    return;
}

// Interactive mode
string? currentTarget = null; // URL or file path
var currentModel = defaultModel;

while (true)
{
    Console.WriteLine("\n=== IntoHear Interactive CLI ===");
    Console.WriteLine($"Current target: {currentTarget ?? "(not set)"}");
    Console.WriteLine($"Current model: {currentModel}");
    Console.WriteLine("1) Enter YouTube URL or local audio file path");
    Console.WriteLine("2) Choose model (tiny, small, base, medium, large)");
    Console.WriteLine("3) Start processing");
    Console.WriteLine("4) Help");
    Console.WriteLine("5) Quit");
    Console.Write("Select an option (1-5): ");

    var choice = Console.ReadLine()?.Trim();

    switch (choice)
    {
        case "1":
            Console.Write("Enter YouTube URL or local file path: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Empty input — target not changed.");
            }
            else
            {
                currentTarget = input;
                Console.WriteLine($"Target set to: {currentTarget}");
            }
            break;

        case "2":
            Console.WriteLine("Choose model: 1) tiny  2) small  3) base  4) medium  5) large");
            Console.Write("Enter number or name: ");
            var modelInput = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(modelInput))
            {
                Console.WriteLine("No change to model.");
                break;
            }

            currentModel = modelInput switch
            {
                "1" or "tiny" => GgmlType.Tiny,
                "2" or "small" => GgmlType.Small,
                "3" or "base" => GgmlType.Base,
                "4" or "medium" => GgmlType.Medium,
                "5" or "large" => GgmlType.Medium,
                _ => ParseModelType(modelInput)
            };

            Console.WriteLine($"Model set to: {currentModel}");
            break;

        case "3":
            if (string.IsNullOrWhiteSpace(currentTarget))
            {
                Console.WriteLine("Please set a target first (option 1).");
                break;
            }

            var modelFileName2 = GetModelFileName(currentModel);
            var generator2 = new CaptionGenerator(modelPath: modelFileName2, modelType: currentModel);

            try
            {
                Console.WriteLine($"Processing {currentTarget} with model {currentModel}...");
                var srt = await generator2.GenerateCaptionsAsync(currentTarget);

                var outputFile = "captions.srt";
                await File.WriteAllTextAsync(outputFile, srt);

                Console.WriteLine($"Captions saved to {outputFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            break;

        case "4":
            Console.WriteLine("Select option 1 to provide a YouTube URL or local audio file path. If you provide a local path, ensure the file exists.");
            Console.WriteLine("Option 2 allows you to change the model. Option 3 starts processing and saves captions as 'captions.srt' in the current directory.");
            break;

        case "5":
            Console.WriteLine("Goodbye.");
            return;

        default:
            Console.WriteLine("Invalid option. Enter a number between 1 and 5.");
            break;
    }
}













