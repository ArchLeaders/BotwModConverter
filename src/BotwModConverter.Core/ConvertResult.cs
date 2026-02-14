namespace BotwModConverter.Core;

public struct ConvertResult
{
    public static readonly ConvertResult Converted = new() {
        WasConverted = true
    };

    public static readonly ConvertResult Copied = new() {
        WasCopied = true
    };

    public static readonly ConvertResult Delayed = new() {
        WasDependency = true
    };

    public static ConvertResult FromError(string? error)
    {
        return new ConvertResult {
            Error = error
        };
    }
    
    public bool WasDependency { get; set; }
    
    public bool WasConverted { get; set; }
    
    public bool WasCopied { get; set; }
    
    public string? Error { get; set; }
}