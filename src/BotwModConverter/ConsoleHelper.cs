using BotwModConverter.Core.Utils;
using Kokuban;

namespace BotwModConverter;

public static class ConsoleHelper
{
    public static void LogSkipFile(string file, string reason)
    {
        Console.WriteLine(
            Chalk.BrightYellow + "[WARNING] [SKIP] " +
            Chalk.Gray + $"[{reason}]: '{file}'"
        );
    }

    public static void LogTaskFailed(string taskName)
    {
        Console.WriteLine(
            Chalk.BrightRed + "[FAILURE] [TASK] " +
            Chalk.Gray + $"Failed to complete task: '{taskName}'"
        );
    }

    public static void LogOperation(ConverterOperation operation, string file)
    {
        Console.WriteLine(
            Chalk.BrightBlue + $"[OP] [{operation.ToString().ToUpper()}] " +
            Chalk.Gray + $"'{file}'"
        );
    }
}