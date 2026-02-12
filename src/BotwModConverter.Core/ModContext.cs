using System.Collections.Concurrent;

namespace BotwModConverter.Core;

public sealed class ModContext
{
    public required bool IsSwitch { get; set; }
    
    /// <summary>
    /// Collection of files waiting for other files (e.g. Text1 waiting for the correct Tex2 file to be found)
    /// </summary>
    public ConcurrentBag<string> WaitingFiles { get; } = [];
}