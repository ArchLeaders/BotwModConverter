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

void Convert([Argument] string modFolderPath, string? outputFolderPath = null, bool parallel = false)
{
    bool isSwitch = ModHelper.IsSwitchMod(modFolderPath);
    outputFolderPath ??= $"{modFolderPath}-{(isSwitch ? "WiiU" : "NX")}";

    var context = new ModContext {
        IsSwitch = isSwitch
    };

    if (parallel) {
        var res = Parallel.ForEach(ModHelper.EnumerateFiles(modFolderPath, isSwitch), (file, token) => {
            string outputFilePath = Path.Combine(outputFolderPath, Path.GetRelativePath(modFolderPath, file), Path.GetFileName(file));
            var operation = ConverterHelper.ConvertOrCopy(file, outputFilePath, context);
            ConsoleHelper.LogOperation(operation, file);
        });

        if (res.IsCompleted) {
            goto Completed;
        }

        ConsoleHelper.LogTaskFailed("Convert Mod in Parallel");
        return;
    }

    foreach (string file in ModHelper.EnumerateFiles(modFolderPath, isSwitch)) {
        string outputFilePath = Path.Combine(outputFolderPath, Path.GetRelativePath(modFolderPath, file), Path.GetFileName(file));
        var operation = ConverterHelper.ConvertOrCopy(file, outputFilePath, context);
        ConsoleHelper.LogOperation(operation, file);
    }

Completed:
    Console.WriteLine(Chalk.BrightGreen + "Mod converted successfully.");
}