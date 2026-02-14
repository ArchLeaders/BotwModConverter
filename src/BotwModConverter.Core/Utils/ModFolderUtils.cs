using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace BotwModConverter.Core.Utils;

public static class ModFolderUtils
{
    public const string NxBaseTitleId = "01007ef00011e000";
    public const string NxAocTitleId = "01007ef00011f001";
    public const string WiiuBaseFolderId = "content";
    public const string WiiuAocFolderId = "aoc";

    public static bool IsSwitchMod(string modFolderPath)
    {
        if (TryFindAnyFolderPath(modFolderPath, WiiuBaseFolderId, WiiuAocFolderId)) {
            return false;
        }

        if (TryFindAnyFolderPath(modFolderPath, NxBaseTitleId, NxAocTitleId)) {
            return true;
        }

        throw new InvalidDataException(
            $"'{modFolderPath}' is not a valid mod folder.");
    }

    public static ReadOnlySpan<char> ToCanon(this string filePath, bool trimToName = true) 
    {
        var name = trimToName ? Path.GetFileName(filePath.AsSpan()) : filePath.AsSpan();

        // TL;DR if the file ext. starts with 's' remove it, unless it's 'sarc'
        if (name.LastIndexOf('.') is var pIdx and > -1 && name.Length > ++pIdx && name[pIdx] is 's' && (name.Length < pIdx + 4 || name[pIdx..(pIdx + 4)] is not "sarc")) {
            return string.Create(name.Length - 1, name, (dst, src) => {
                int index = 0;
                for (int i = 0; i < src.Length; i++) {
                    if (i == pIdx) continue;
                    dst[index++] = src[i];
                }
            });
        }

        return name;
    }

    public static string GetPlatformContentOutput(string outputFolder, Platform platform)
    {
        return platform switch {
            _ => throw new NotSupportedException($"Unsupported platform: {platform}")
        };
    }

    public static string GetPlatformOutput(string folder, Platform platform, ModOutputFolder outputFolder)
    {
        return (platform, outputFolder) switch {
            (Platform.Switch, ModOutputFolder.Aoc) => Path.Combine(folder, WiiuAocFolderId),
            (Platform.Switch, ModOutputFolder.Content) => Path.Combine(folder, WiiuBaseFolderId),
            (Platform.WiiU, ModOutputFolder.Content) => Path.Combine(folder, NxBaseTitleId, "romfs"),
            (Platform.WiiU, ModOutputFolder.Aoc) => Path.Combine(folder, NxAocTitleId, "romfs"),
            _ => throw new NotSupportedException($"Unsupported platform and/or output folder: {platform}, {outputFolder}")
        };
    }

    public static unsafe ReadOnlySpan<char> ToTexN(this ReadOnlySpan<char> canon, char num)
    {
        fixed (char* ptr = canon) {
            return new Span<char>(ptr, canon.Length) {
                [^7] = num
            };
        }
    }

    public static IEnumerable<(string Input, string Output, string RelativePath)> EnumerateFiles(string modFolderPath, string outputFolderPath, Platform platform)
    {
        return platform is Platform.Switch
            ? EnumerateFiles(modFolderPath, outputFolderPath,
                (NxBaseTitleId, WiiuBaseFolderId, "romfs"),
                (NxAocTitleId, Path.Combine(WiiuAocFolderId, "0010"), "romfs"))
            : EnumerateFiles(modFolderPath, outputFolderPath,
                (WiiuBaseFolderId, Path.Combine(NxBaseTitleId, "romfs"), ""),
                (WiiuAocFolderId, Path.Combine(NxAocTitleId, "romfs"), "0010"));
    }

    private static IEnumerable<(string Input, string Output, string RelativePath)> EnumerateFiles(string modFolderPath, string outputFolderPath, params ImmutableArray<(string, string, string)> folders)
    {
        foreach (var (inputFolderName, outputFolderName, subFolder) in folders) {
            if (TryFindFolderPath(modFolderPath, inputFolderName, out var baseFolderPath)) {
                var actualBaseFolder = subFolder is "" ? baseFolderPath : Path.Combine(baseFolderPath, subFolder);
                foreach (var file in Directory.EnumerateFiles(actualBaseFolder, "*.*", SearchOption.AllDirectories)) {
                    var relativePath = Path.GetRelativePath(actualBaseFolder, file);
                    yield return (
                        Input: file,
                        Output: Path.Combine(outputFolderPath, outputFolderName, relativePath),
                        RelativePath: relativePath
                    );
                }
            }
        }
    }

    private static bool TryFindFolderPath(string baseFolder, string folderName, [MaybeNullWhen(false)] out string path)
    {
        path = Directory.EnumerateDirectories(baseFolder).FirstOrDefault(path => Path.GetFileName(path).Equals(folderName, StringComparison.InvariantCultureIgnoreCase)
        );

        return path is not null;
    }

    private static bool TryFindAnyFolderPath(string baseFolder, params ImmutableArray<string> folderNames)
    {
        return Directory.EnumerateDirectories(baseFolder)
            .Select(Path.GetFileName)
            .Any(path => path is not null && folderNames.Contains(path, StringComparer.InvariantCultureIgnoreCase));
    }
}