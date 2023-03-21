using System.Text.Json;

namespace BotwModConverter.Core;

/// <summary>
/// Global configuration for the BOTW game files
/// </summary>
public class BotwConfig
{
    public static BotwConfig Shared { get; set; }

    public required string GamePath { get; set; }
    public required string UpdatePath { get; set; }
    public required string DlcPath { get; set; }
    public required string GamePathNx { get; set; }
    public required string DlcPathNx { get; set; }

    static BotwConfig()
    {
        using FileStream fs = File.OpenRead(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Botw", "config.json")
        );

        Shared = JsonSerializer.Deserialize<BotwConfig>(fs) ??
            throw new NullReferenceException("The JSON serializer returned null when parsing the global BOTW configuration");
    }
}
