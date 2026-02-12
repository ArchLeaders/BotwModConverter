using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace BotwModConverter.Core.Utils;

public static class ModHelper
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
    
    public static ReadOnlySpan<char> GetCanonName(string filePath)
    {
        var name = Path.GetFileName(filePath.AsSpan());

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
    
    public static IEnumerable<string> EnumerateFiles(string modFolderPath, bool isSwitch)
    {
        return isSwitch
            ? EnumerateFiles(modFolderPath, NxBaseTitleId, NxAocTitleId)
            : EnumerateFiles(modFolderPath, WiiuBaseFolderId, WiiuAocFolderId);
    }
    
    private static IEnumerable<string> EnumerateFiles(string modFolderPath, params ImmutableArray<string> folderNames)
    {
        foreach (string folderName in folderNames) {
            if (TryFindFolderPath(modFolderPath, folderName, out var baseFolderPath)) {
                foreach (var file in Directory.EnumerateFiles(baseFolderPath, "*.*", SearchOption.AllDirectories)) {
                    yield return file;
                }
            }
        }
    }

    private static bool TryFindFolderPath(string baseFolder, string folderName, [MaybeNullWhen(false)] out string path)
    {
        path = Directory.EnumerateDirectories(baseFolder).FirstOrDefault(
            path => Path.GetFileName(path).Equals(folderName, StringComparison.InvariantCultureIgnoreCase)
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