using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyShortcutGuide.Core;

public static class AtomicFile
{
    public static void Write(string path, string text, bool backup = false)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        Directory.CreateDirectory(directory);
        var temp = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                var bytes = new UTF8Encoding(false).GetBytes(text);
                stream.Write(bytes);
                stream.Flush(true);
            }
            if (File.Exists(path)) File.Replace(temp, path, backup ? path + ".bak" : null);
            else File.Move(temp, path);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}

public sealed class DocumentStore(string path)
{
    public const int MaxFileBytes = 16 * 1024 * 1024;
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowDuplicateProperties = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };
    public string Path { get; } = path;
    public ShortcutDocument Load() => File.Exists(Path) ? Read(Path) : ShortcutDocument.CreateDefault();
    public static ShortcutDocument Read(string path)
    {
        using var stream = File.OpenRead(path);
        if (stream.Length > MaxFileBytes) throw new InvalidDataException("JSONは16 MB以下にしてください。");
        var result = JsonSerializer.Deserialize<ShortcutDocument>(stream, JsonOptions)
            ?? throw new InvalidDataException("JSONにデータがありません。");
        result.Validate();
        return result;
    }
    public void Save(ShortcutDocument document)
    {
        document.Validate();
        AtomicFile.Write(Path, JsonSerializer.Serialize(document, JsonOptions), backup: true);
    }
    public static ShortcutDocument Clone(ShortcutDocument document) =>
        JsonSerializer.Deserialize<ShortcutDocument>(JsonSerializer.Serialize(document, JsonOptions), JsonOptions)!;
    public static void Export(ShortcutDocument document, string path)
    {
        document.Validate();
        AtomicFile.Write(path, JsonSerializer.Serialize(document, JsonOptions));
    }
}
