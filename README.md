# My Shortcut Guide

自分のショートカットを編集し、Microsoft PowerToys Shortcut Guide に **My Shortcuts** として表示するWindows常駐アプリ。

## 起動

1. 配布ZIPを任意の固定フォルダーへ展開します。
2. `MyShortcutGuide.exe` を起動します。初回は空の7セクションを用意します。
3. セクションを選び「＋ ショートカット」で名前・説明・キーを登録し、保存します。
4. PowerToysを起動しShortcut Guideを有効にして、「Shortcut Guide を開く」を押します。

Windows 11 x64推奨。利用者の.NETインストールは不要です。PowerToysは別途必要です。
EXEは仮アイコン・未署名です。アイコンの差し替えは `src/MyShortcutGuide/Assets/README.md` を参照してください。

## 操作

- セクション見出しのアイコンから追加・名称変更・削除。一覧は上下キーで選択、Alt＋上下キーで並べ替え。
- ショートカット：追加、編集、削除、上下移動、別セクションへ移動。
- ガイド起動・再反映・Import/Export・設定・Exitはサイドバー下部に配置。アイコンやアプリ操作にはツールチップを表示します。検索欄には内側の余白があり、入力欄の外の余白や見出しをクリックするとフォーカスを外せます。
- 選択セクション内の検索：`Ctrl+F`。追加：`Ctrl+N`。一覧の編集：ダブルクリックまたは`F2`。
- 「キーを記録」は記録画面が開いている間だけキーを受け取ります。全キーを離すと確定。Escも登録可能で、キャンセルは画面のボタンを使います。Ctrl+Alt+Delete等のOS予約キーは手動選択してください。IME/JISのキー表示はキーボード配列に依存します。
- Recommendedをオンにした項目はガイドのおすすめ欄にも表示されます。
- ウィンドウを閉じるとエディターを破棄して通知領域へ戻ります。設定で無効にした場合はタスクバーへ最小化します。アプリを終了するには **Exit** を使用します。
- EXEを再度起動すると既存プロセスのエディターを開きます。`--background` はログイン起動用です。
- 通知領域メニュー：`Open Editor` / `Open Shortcut Guide` / `Exit`。
- Importはライブラリ全体の置換です。読み込み検証と確認後に適用し、このPCのアプリ設定は維持します。Exportは設定を含むJSONを書き出します。
- 設定は「保存」で適用。自動起動は現在のEXEの絶対パスをHKCU Runへ登録します。EXEを移動した場合は新しい場所から起動して設定を保存し直してください。Windowsのスタートアップ管理側で無効化されている場合はそちらも確認してください。
- アンインストール：自動起動とガイド表示をオフにして保存し、Exit後に配布フォルダーを削除します。JSONは残るので復元できます。

## 保存先とPowerToys連携

|用途|パス|
|---|---|
|正本|`%LOCALAPPDATA%\MyShortcutGuide\shortcuts.json`|
|直前のJSONバックアップ|同フォルダーの `shortcuts.json.bak`|
|生成YAML|`%LOCALAPPDATA%\Microsoft\WinGet\KeyboardShortcuts\Local.MyShortcutGuide.en-US.yml`|

`PackageName: Local.MyShortcutGuide`、`Name: My Shortcuts`、`WindowFilter: MyShortcutGuide.exe`、`BackgroundProcess: true` を出力します。現行PowerToysの読み込み実装が `<PackageName>.en-US.yml` を探すため、このファイル1個を生成します。EXE名は変更しないでください。

JSONとYAMLは同じフォルダー内の一時ファイルへ書き、ディスクへフラッシュ後に置換します。JSON保存の失敗時は編集中の変更を適用しません。JSON保存後のガイド反映失敗は警告として区別し、「ガイドへ再反映」で再試行できます。壊れたJSONや未対応スキーマを検出した場合、起動を中止して元のファイルを保持します。必要なら元ファイルを退避して `.bak` を `shortcuts.json` として戻してください。

保存／再反映／起動時にのみYAMLを書きます。PowerToys付属の `PowerToys.ShortcutGuide.IndexYmlGenerator.exe` にユーザー用索引の再生成を依頼します。PowerToys本体や組み込みマニフェストは編集しません。開いているガイドは閉じて開き直してください。PowerToys未導入でもJSONの編集・保存・Exportは使えます。

「ガイドを開く」はPowerToysの名前付きイベントを通知します。このイベントと索引生成EXEは内部実装なので、将来変更される可能性があります。非対応時は案内を表示し、PowerToys側の起動ショートカットを使えます。アダプターは `Services/AppServices.cs`、YAMLスキーマはCoreの `ManifestWriter.cs` に分離しています。

参考：[Microsoft Learn](https://learn.microsoft.com/en-us/windows/powertoys/shortcut-guide)、[ManifestInterpreter](https://github.com/microsoft/PowerToys/blob/main/src/modules/ShortcutGuide/ShortcutGuide.Ui/Helpers/ManifestInterpreter.cs)、[マニフェスト仕様](https://github.com/microsoft/PowerToys/blob/main/doc/specs/WinGet%20Manifest%20Keyboard%20Shortcuts%20schema.md)、[共有イベント](https://github.com/microsoft/PowerToys/blob/main/src/common/interop/shared_constants.h)。

## 設計

**.NET 10 / C# / WinForms** を採用。NotifyIconとWindowsメッセージループが標準で使え、ブラウザーランタイムを必要としません。待機時はメッセージと名前付きイベント待ちのみです。ポーリング、常駐タイマー、FileSystemWatcher、常時キーボードフック、テレメトリー、ネットワーク通信はありません。キー記録の一時フックと、保存中の処理期限のみを必要時に作成します。

モデル・検証・JSON・YAMLはUI非依存のCoreプロジェクトに分離。YAMLは固定スキーマの出力専用で、文字列にはYAML互換のJSON引用を使用します。キーはWindows仮想キーコードを文字列として出力し、数字キーの誤解釈を防ぎます。外部NuGet依存はありません。

UIはチャコールの2ペイン、淡い青の選択表示、キーキャップ表現、説明付きの空状態を使用。通常操作はWindowsのフォーカス・キーボード操作を保持します。ダークモードは.NET 10 WinFormsの試験的APIを使用しています。独自描画の一覧は項目全体の読み上げ名を持ちますが、スクリーンリーダー全般と高コントラストの網羅検証は未実施です。

WindowsのSystem-aware DPIを使用。フォームの組立て後に96 DPI基準から一括スケールし、一覧の項目高も同じ倍率で調整します。異なる倍率のディスプレイ間を移動するとWindows側の拡縮によるぼやけが発生する場合があります。

## 開発と配布

Windowsと.NET 10 SDKを使用します。

```powershell
dotnet build src/MyShortcutGuide/MyShortcutGuide.csproj -c Release
dotnet run --project tests/MyShortcutGuide.Tests -c Release
pwsh -File scripts/publish.ps1
```

`artifacts/MyShortcutGuide-0.1.5-win-x64/` にself-contained single-file EXE、README、LICENSEを生成し、同名ZIPとSHA-256チェックサムも作ります。単一EXEにネイティブライブラリを同梱するため、実行時に.NETの一時展開が発生します。トリミングはWinFormsの互換性維持のため無効です。

GitHub Actionsはビルド・テスト・配布ZIPの生成に対応。`v*` タグでGitHub Releaseのドラフトと添付ファイルを作成します。署名証明書や発行先の認証情報は含めません。v1はポータブル配布で、ユーザーデータは配布フォルダーから分離しているため、後からinstaller/MSIXのライフサイクルへ移行できます。MSIXでは自動起動アダプターの差し替えが必要です。

検証項目と結果は `docs/verification.md` を参照してください。

## License

MIT。PowerToysとは独立したコミュニティアプリです。

### UI部品の方針

アプリ内の確認・エラー表示、選択欄の展開状態、スクロールバーも共通の色・余白・操作状態で描画します。既定の外観を完成形として採用せず、各状態を確認します。選択欄は上下/Home/End、Enter/Space/F4で展開、候補ではEnterで確定・Escで取消ができます。ファイルのImport/Export先を選ぶ画面はWindowsのファイル選択ダイアログを使用します。

### UI 0.1.3

- セクションとショートカットは行をドラッグして並べ替えます。青い挿入線で位置を確認し、一覧の外で離すかEscでキャンセルできます。長い一覧は端でドラッグを動かすとスクロールします。Alt + 上下キーも利用できます。検索中の並べ替えは無効です。
- 追加は現在選択したセクション内に固定。編集時は移動先を変更できます。
- 入力欄は控えめな角丸、キー設定は区切り線と「組み合わせの結果」でまとめています。
- アイコンの内側は均一な #282828、外側は透明。16〜256pxのICOと1024px PNGを同梱ソースに保存しています。
- アイコン再生成: PowerShell 7.6 (.NET 10) で `./scripts/build-icon.ps1 -OutputDirectory ./src/MyShortcutGuide/Assets`。

### 0.1.4

ショートカットの編集はブロック内のダブルクリック、編集ボタン、F2から開きます。一覧の空白やブロック間の隙間をクリックしても編集を開きません。

### 0.1.5

ショートカット一覧の空白をクリックすると選択を解除します。青い強調とフォーカス枠が消え、編集・削除は無効になります。検索中も同じ動作です。

### 0.1.6

設定・Exit横の情報ボタンから、実行中EXE、JSON、バックアップ、生成YAML、自動起動レジストリと現在のコマンドを確認できます。環境から動的に取得し、各パスの個別コピーに対応します。

### 0.1.7

通知領域アイコンは左クリックでShortcut Guideを開き、右クリックでエディター・ガイド・終了のメニューを表示します。メニューはアプリのダーク配色・細線アイコン・角丸選択表示を使用します。

### 0.1.8

情報画面の各パスに小さなコピーボタンを配置。コピー値に説明や未作成の注記は含めません。サイドバー最下段は左から情報・設定・Exitとし、待機表示の高さを確保しています。

### 0.2.0

ショートカット登録・編集で「自由テキスト」を選択できます。1〜120文字、1行の文字列を入力します。PowerToysではキーと同じ枠付きで表示されます。JSONのdisplayTextがnullまたは未指定なら従来のキー設定を使用し、自由テキスト時は修飾キーをYAMLへ出力しません。数値や予約語のキー解釈を避けるため、生成YAMLだけにゼロ幅文字を付加します。保存JSONと一覧表示には付加しません。旧版は自由テキスト付きJSONを読み込めません。
