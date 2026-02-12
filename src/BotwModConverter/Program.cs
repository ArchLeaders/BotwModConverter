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
        var res = Parallel.ForEach(ModHelper.EnumerateFiles(modFolderPath, outputFolderPath, isSwitch), (file, token) => {
            var operation = ConverterHelper.ConvertOrCopy(file.Input, file.Output, context);
            ConsoleHelper.LogOperation(operation, file.Output);
        });

        if (res.IsCompleted) {
            goto Completed;
        }

        ConsoleHelper.LogTaskFailed("Convert Mod in Parallel");
        return;
    }

    foreach (var file in ModHelper.EnumerateFiles(modFolderPath, outputFolderPath, isSwitch)) {
        var operation = ConverterHelper.ConvertOrCopy(file.Input, file.Output, context);
        ConsoleHelper.LogOperation(operation, file.Output);
    }

Completed:
    Console.WriteLine(Chalk.BrightGreen + "Mod converted successfully.");
}