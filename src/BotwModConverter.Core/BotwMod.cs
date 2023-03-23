using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace BotwModConverter.Core;

public enum BotwPlatform { Switch, Wiiu }

public class InvalidBotwModException : Exception
{
    public InvalidBotwModException() { }
    public InvalidBotwModException(string? message) : base(message) { }
    public InvalidBotwModException(string? message, Exception? innerException) : base(message, innerException) { }
    protected InvalidBotwModException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

public class BotwMod
{
    public List<string?> RootFolders => Directory.GetDirectories(Root).Select(Path.GetFileName).ToList();

    public string Root { get; }
    public BotwPlatform Platform { get; }

    public static implicit operator BotwMod(string root) => new(root);

    [SetsRequiredMembers]
    public BotwMod(string root)
    {
        Root = root;
        List<string?> rootFolders = RootFolders;
        BotwPlatform platform = rootFolders.Contains("aoc") || rootFolders.Contains("content") ? BotwPlatform.Wiiu
            : rootFolders.Contains("01007EF00011E000") || rootFolders.Contains("01007EF00011F001") ? BotwPlatform.Switch
            : throw new InvalidBotwModException("The provided path did not reprensent a valid botw mod root folder");

        Platform = platform;
    }

    public IEnumerable<string?> GetModFolders()
    {
        if (Platform == BotwPlatform.Wiiu) {
            return RootFolders.Where(x => x is "aoc" or "content");
        }
        else if (Platform == BotwPlatform.Switch) {
            return RootFolders.Where(x => x is "01007EF00011E000" or "01007EF00011F001");
        }
        else {
            throw new InvalidBotwModException($"The mod folder '{Root}' is no longer a valid mod root");
        }
    }
}
