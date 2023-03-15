using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BotwModConverter.Core;

public static class ConverterLog
{
    public static string? CurrentLog { get; private set; }
    internal static string LogPath => Path.Combine(AppContext.BaseDirectory, "Logs", CurrentLog ?? "default.log");

    static ConverterLog()
    {
        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Logs"));
        CurrentLog = $"{DateTime.Now:yyyy-MM-dd-HH-mm}.log";
        AddListener(new TextWriterTraceListener(LogPath));
        Trace.AutoFlush = true;
    }

    public static void WriteLine(object? value, [CallerMemberName] string method = "", [CallerLineNumber] int lineNumber = 0)
    {
        Trace.WriteLine(
            FormatHeader(method, lineNumber, out int len) +
            value?.ToString()?.Replace("\n", $"\n{new string(' ', len)}")
        );
    }

    public static void Write(object? value, [CallerMemberName] string method = "", [CallerLineNumber] int lineNumber = 0)
    {
        Trace.Write(
            FormatHeader(method, lineNumber, out int len) +
            value?.ToString()?.Replace("\n", $"\n{new string(' ', len)}")
        );
    }

    public static void AddListener(TraceListener listener)
    {
        Trace.Listeners.Add(listener);
    }

    public static void ResetLog()
    {
        Trace.Listeners[0] = new TextWriterTraceListener(LogPath);
    }

    private static string FormatHeader(string method, int lineNumber, out int len)
    {
        string result = $"[{DateTime.Now:hh:mm:ss:fff}] [{method}:{lineNumber}] | "; ;
        len = result.Length;
        return result;
    }
}
