using BotwModConverter;
using BotwModConverter.Core;
using BotwModConverter.Core.Utils;
using ConsoleAppFramework;
using Kokuban;

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
    bool isSwitch = ModHelper.IsSwitchMod(modFolderPath);
    output ??= $"{modFolderPath}-{(isSwitch ? "WiiU" : "NX")}";

    var context = new ModContext {
        IsSwitch = isSwitch
    };
    
    if (clear) {
        foreach (var folder in Directory.EnumerateDirectories(output)) {
            Directory.Delete(folder, true);
        }
    }

    if (parallel) {
        var res = Parallel.ForEach(ModHelper.EnumerateFiles(modFolderPath, output, isSwitch), (file, token) => {
            var operation = ConverterHelper.ConvertOrCopy(file.Input, file.Output, file.RelativePath, context);
            ConsoleHelper.LogOperation(operation, file.RelativePath);
        });

        if (res.IsCompleted) {
            goto Completed;
        }

        ConsoleHelper.LogTaskFailed("Convert Mod in Parallel");
        return;
    }

    foreach (var file in ModHelper.EnumerateFiles(modFolderPath, output, isSwitch)) {
        var operation = ConverterHelper.ConvertOrCopy(file.Input, file.Output, file.RelativePath, context);
        ConsoleHelper.LogOperation(operation, file.RelativePath);
    }

Completed:
    Console.WriteLine(Chalk.BrightGreen + "Mod converted successfully.");
}