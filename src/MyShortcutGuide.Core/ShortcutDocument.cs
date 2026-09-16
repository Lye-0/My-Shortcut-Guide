using System.Text.Json.Serialization;

namespace MyShortcutGuide.Core;

public sealed class ShortcutDocument
{
    [JsonRequired] public int SchemaVersion { get; set; } = 1;
    [JsonRequired] public AppSettings Settings { get; set; } = new();
    [JsonRequired] public List<Section> Sections { get; set; } = [];

    public static ShortcutDocument CreateDefault() => new()
    {
        Sections = new[] { "Favorites", "AI", "Windows", "PowerToys", "Development", "Browser", "Creative" }
            .Select(name => new Section { Name = name }).ToList()
    };

    public void Validate()
    {
        if (SchemaVersion != 1) throw new InvalidDataException($"未対応のデータ形式です (version {SchemaVersion})。");
        if (Settings is null || Sections is null) throw new InvalidDataException("設定またはセクションがありません。");
        if (Sections.Count > 200) throw new InvalidDataException("セクションは200個まで登録できます。");
        var ids = new HashSet<Guid>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var total = 0;
        foreach (var section in Sections)
        {
            if (section is null) throw new InvalidDataException("空のセクションがあります。");
            CheckText(section.Name, "セクション名", 80, true);
            if (section.Id == Guid.Empty || !ids.Add(section.Id)) throw new InvalidDataException("セクションIDが無効または重複しています。");
            if (!names.Add(section.Name.Trim())) throw new InvalidDataException("同じ名前のセクションがあります。");
            if (section.Shortcuts is null) throw new InvalidDataException("ショートカット一覧がありません。");
            total += section.Shortcuts.Count;
            if (total > 10000) throw new InvalidDataException("ショートカットは合計10,000件まで登録できます。");
            foreach (var shortcut in section.Shortcuts)
            {
                if (shortcut is null) throw new InvalidDataException("空のショートカットがあります。");
                if (shortcut.Id == Guid.Empty || !ids.Add(shortcut.Id)) throw new InvalidDataException("ショートカットIDが無効または重複しています。");
                CheckText(shortcut.Name, "名前", 120, true);
                CheckText(shortcut.Description, "説明", 2000, false);
                if (shortcut.DisplayText is not null)
                {
                    CheckText(shortcut.DisplayText, "表示テキスト", 120, true);
                    if (shortcut.DisplayText.Any(char.IsControl)) throw new InvalidDataException("表示テキストは1行で入力してください。");
                }
                if (!KeyCatalog.IsValid(shortcut.KeyCode)) throw new InvalidDataException("メインキーが未選択または未対応です。");
            }
        }
    }

    private static void CheckText(string? value, string label, int max, bool required)
    {
        if (value is null || value.Length > max || (required && string.IsNullOrWhiteSpace(value)))
            throw new InvalidDataException($"{label}は{(required ? "1" : "0")}〜{max}文字で入力してください。");
        if (value.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t'))
            throw new InvalidDataException($"{label}に使用できない制御文字があります。");
    }
}

public sealed class AppSettings
{
    public bool StartWithWindows { get; set; }
    public bool CloseToTray { get; set; } = true;
    public bool ShowInShortcutGuide { get; set; } = true;
}

public sealed class Section
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public string Name { get; set; } = "";
    [JsonRequired] public List<ShortcutEntry> Shortcuts { get; set; } = [];
    public override string ToString() => Name;
}

public sealed class ShortcutEntry
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Win { get; set; }
    public bool Ctrl { get; set; }
    public bool Shift { get; set; }
    public bool Alt { get; set; }
    [JsonRequired] public int KeyCode { get; set; } = 65;
    public bool Recommended { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? DisplayText { get; set; }
    [JsonIgnore] public string Gesture => string.Join(" + ", Parts);
    [JsonIgnore] public IEnumerable<string> Parts
    {
        get
        {
            if (DisplayText is not null) { yield return DisplayText; yield break; }
            if (Win) yield return "Win";
            if (Ctrl) yield return "Ctrl";
            if (Shift) yield return "Shift";
            if (Alt) yield return "Alt";
            yield return KeyCatalog.Name(KeyCode);
        }
    }
    public override string ToString() => $"{Name}   {Gesture}{(Recommended ? "   おすすめ" : "")}";
}

public sealed record KeyOption(int Code, string Label)
{
    public override string ToString() => Label;
}

public static class KeyCatalog
{
    public static IReadOnlyList<KeyOption> All { get; } = Build();
    public static bool IsValid(int code) => All.Any(k => k.Code == code);
    public static string Name(int code) => All.FirstOrDefault(k => k.Code == code)?.Label ?? $"VK {code}";
    private static List<KeyOption> Build()
    {
        var result = new List<KeyOption>();
        for (var c = 65; c <= 90; c++) result.Add(new(c, ((char)c).ToString()));
        for (var c = 48; c <= 57; c++) result.Add(new(c, ((char)c).ToString()));
        for (var c = 112; c <= 135; c++) result.Add(new(c, $"F{c - 111}"));
        result.AddRange(new[] {
            new KeyOption(8,"Backspace"), new(9,"Tab"), new(13,"Enter"), new(19,"Pause"), new(20,"Caps Lock"),
            new(27,"Esc"), new(32,"Space"), new(33,"Page Up"), new(34,"Page Down"), new(35,"End"), new(36,"Home"),
            new(37,"←"), new(38,"↑"), new(39,"→"), new(40,"↓"), new(44,"Print Screen"), new(45,"Insert"), new(46,"Delete"),
            new(93,"Menu"), new(144,"Num Lock"), new(145,"Scroll Lock"), new(186,";"), new(187,"="),
            new(188,","), new(189,"-"), new(190,"."), new(191,"/"),
            new(192,"`"), new(219,"["), new(220,"\\"), new(221,"]"),
            new(222,"'"), new(226,"\\"), new(28,"変換"), new(29,"無変換"), new(21,"かな"),
            new(173,"Mute"), new(174,"Volume −"), new(175,"Volume +"), new(176,"Next track"), new(177,"Previous track"),
            new(178,"Stop media"), new(179,"Play / Pause") });
        for (var c = 96; c <= 105; c++) result.Add(new(c, $"Num {c - 96}"));
        result.AddRange(new[] { new KeyOption(106,"Num *"), new(107,"Num +"), new(109,"Num −"), new(110,"Num ."), new(111,"Num /") });
        return result;
    }
}
