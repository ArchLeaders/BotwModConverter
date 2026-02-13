using BotwModConverter.Core;
using ConsoleAppFramework;

var app = ConsoleApp.Create();

app.Add("convert", Convert);

await app.RunAsync(args);

#if !DEBUG
Console.WriteLine("\r\nPress any key to exit...");
Console.ReadKey();
#endif

return;

void Convert([Argument] string modFolderPath, string? output = null, bool parallel = false, bool clear = false)
{
    using var engine = new ConverterEngine(modFolderPath, output);
    
    if (clear) {
        foreach (var folder in Directory.EnumerateDirectories(engine.OutputFolder)) {
            Directory.Delete(folder, true);
        }
    }

    if (parallel ? engine.ConvertParallel() : engine.Convert()) {
        // TODO: Log success
    }

    // TODO: Log error
}