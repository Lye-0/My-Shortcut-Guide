using System.Text.Json;
using MyShortcutGuide.Core;

var root = Path.Combine(Path.GetTempPath(), "MyShortcutGuide.Tests", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
var passed = 0;
void Test(string name, Action action) { action(); passed++; Console.WriteLine($"PASS {name}"); }
void Check(bool condition) { if (!condition) throw new Exception("Assertion failed."); }
void Reject(Action action) { try { action(); } catch (Exception e) when (e is InvalidDataException or JsonException or IOException) { return; } throw new Exception("Invalid data was accepted."); }
try
{
    var store = new DocumentStore(Path.Combine(root, "shortcuts.json"));
    var doc = ShortcutDocument.CreateDefault();
    var entry = new ShortcutEntry { Name = "引用: \"yes\" # true", Description = "日本語\nline 2\t\\path", Ctrl = true, Win = true, KeyCode = 49, Recommended = true };
    doc.Sections[0].Shortcuts.Add(entry);
    Test("defaults and JSON restart", () => { store.Save(doc); var read = store.Load(); Check(read.Sections.Count == 7 && read.Sections[0].Shortcuts[0].Description == entry.Description); });
    Test("atomic replacement and previous version backup", () => { var next = DocumentStore.Clone(doc); next.Sections[0].Name = "Changed"; store.Save(next); Check(store.Load().Sections[0].Name == "Changed"); Check(DocumentStore.Read(store.Path + ".bak").Sections[0].Name == "Favorites"); });
    Test("CRUD, ordering and section move survive restart", () =>
    {
        var next = DocumentStore.Clone(doc); var s = new Section { Name = "New section" }; next.Sections.Insert(0, s);
        var moved = next.Sections[1].Shortcuts[0]; next.Sections[1].Shortcuts.Clear(); moved.Name = "Edited"; s.Shortcuts.Add(moved);
        s.Shortcuts.Add(new ShortcutEntry { Name = "Second", KeyCode = 112 }); s.Shortcuts.Reverse(); next.Sections.RemoveAt(2); store.Save(next);
        var read = store.Load(); Check(read.Sections.Count == 7 && read.Sections[0].Shortcuts[1].Name == "Edited");
        read.Sections[0].Shortcuts.RemoveAt(0); store.Save(read); Check(store.Load().Sections[0].Shortcuts.Count == 1);
    });
    Test("export/import round trip", () => { var path = Path.Combine(root, "export.json"); DocumentStore.Export(doc, path); Check(JsonSerializer.Serialize(DocumentStore.Read(path)) == JsonSerializer.Serialize(doc)); });
    Test("malformed JSON leaves original untouched", () => { var before = File.ReadAllText(store.Path); var bad = Path.Combine(root, "bad.json"); File.WriteAllText(bad, "{broken"); Reject(() => DocumentStore.Read(bad)); Check(File.ReadAllText(store.Path) == before); });
    Test("invalid model never overwrites JSON", () => { var before = File.ReadAllText(store.Path); var bad = DocumentStore.Clone(doc); bad.Sections[0].Shortcuts[0].KeyCode = 17; Reject(() => store.Save(bad)); Check(before == File.ReadAllText(store.Path)); });
    Test("future schema rejected", () => { var bad = DocumentStore.Clone(doc); bad.SchemaVersion = 2; Reject(bad.Validate); });
    Test("missing required fields and duplicate JSON properties rejected", () =>
    {
        var path = Path.Combine(root, "incomplete.json"); File.WriteAllText(path, "{}"); Reject(() => DocumentStore.Read(path));
        File.WriteAllText(path, "{\"schemaVersion\":1,\"schemaVersion\":1,\"settings\":{},\"sections\":[]}"); Reject(() => DocumentStore.Read(path));
    });
    Test("null lists rejected", () => { var bad = DocumentStore.Clone(doc); bad.Sections = null!; Reject(bad.Validate); });
    Test("duplicate identities and names rejected", () => { var bad = DocumentStore.Clone(doc); bad.Sections[1].Id = bad.Sections[0].Id; Reject(bad.Validate); bad = DocumentStore.Clone(doc); bad.Sections[1].Name = "favorites"; Reject(bad.Validate); });
    Test("oversize import rejected", () => { var path = Path.Combine(root, "large.json"); using (var f = File.Create(path)) f.SetLength(DocumentStore.MaxFileBytes + 1); Reject(() => DocumentStore.Read(path)); });
    Test("manifest contract and escaping", () =>
    {
        var yaml = ManifestWriter.Generate(doc); Check(yaml.Contains("BackgroundProcess: true") && yaml.Contains("Name: My Shortcuts") && yaml.Contains("WindowFilter: MyShortcutGuide.exe"));
        Check(yaml.Contains("Keys: [\"49\"]") && yaml.Contains("Recommended: true") && yaml.Contains("Properties: []"));
        Check(yaml.Contains(JsonSerializer.Serialize(entry.Name)) && yaml.Contains(JsonSerializer.Serialize(entry.Description)));
        File.WriteAllText(Path.Combine(root, "contract.yml"), yaml);
    });
    Test("manifest enable/disable only touches owned file", () => { var dir = Path.Combine(root, "manifests"); Directory.CreateDirectory(dir); File.WriteAllText(Path.Combine(dir, "other.yml"), "untouched"); ManifestWriter.Write(doc, dir); Check(File.Exists(Path.Combine(dir, ManifestWriter.FileName))); var disabled = DocumentStore.Clone(doc); disabled.Settings.ShowInShortcutGuide = false; ManifestWriter.Write(disabled, dir); Check(!File.Exists(Path.Combine(dir, ManifestWriter.FileName))); Check(File.ReadAllText(Path.Combine(dir, "other.yml")) == "untouched"); });
    Test("empty library is valid YAML sequence", () => { Check(ManifestWriter.Generate(new ShortcutDocument()).Contains("Shortcuts: []")); });
    Console.WriteLine($"{passed} tests passed. Test fixtures: {root}");
    return 0;
}
catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
