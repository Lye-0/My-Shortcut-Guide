# My Shortcut Guide

**自分専用のショートカット早見表を、PowerToysからすぐに開く。**

Windows向けの小さな常駐アプリです。ショートカットをセクションごとに登録し、Microsoft PowerToys Shortcut Guideに **My Shortcuts** として表示します。

キーの組み合わせだけでなく、「右クリックして選択」「Ctrlを2回押す」などの自由テキストも登録できます。

> このアプリは早見表を編集するためのものです。登録したキーに機能を割り当てたり、他のアプリのショートカット設定を書き換えたりするものではありません。

## はじめに

- **動作環境**：Windows 11 x64推奨。
- **必要なもの**：ガイド表示にはMicrosoft PowerToysが必要です。PowerToys側でShortcut Guideを有効にしてください。
- **配布形式**：ポータブル版。利用者による.NETのインストールは不要です。
- **現在の版**：0.2.3。EXEは未署名です。
- **連携確認環境**：PowerToys 0.101.2362.0。PowerToysの内部仕様変更によって連携方法の調整が必要になる場合があります。

### 最初のショートカットを登録する

1. 配布ZIPを、継続して使うフォルダーへ展開します。例：`C:\Tools\MyShortcutGuide`。
2. `MyShortcutGuide.exe` を起動します。
3. 左側のセクションを選び、右側の **＋ ショートカット** を押します。
4. 名前を入力し、**キーの組み合わせ** または **自由テキスト** を選びます。
5. プレビューを確認して **保存** を押します。
6. **Shortcut Guide を開く** を押し、ガイド内の **My Shortcuts** を選びます。

すでにガイドを開いている場合は、一度閉じて開き直してください。

初回は Favorites / AI / Windows / PowerToys / Development / Browser / Creative の空のセクションを用意します。名前や順序は変更できます。

## 画面の見方

編集画面は、**左がセクションとアプリ全体の操作、右が選択したセクションの内容**です。

```text
┌────────────────────────┬──────────────────────────────────┐
│ My Shortcut Guide      │ 選択したセクション名             │
│                        │ 件数                             │
│ セクション  ＋  ✎  削除│ ＋ ショートカット                │
│                        │ このセクションを検索             │
│ Favorites              │                                  │
│ AI                     │ ┌──────────────────────────────┐ │
│ Windows                │ │ 名前                         │ │
│ PowerToys              │ │ 説明                         │ │
│ …                      │ │ [Win] [Shift] [V]            │ │
│                        │ └──────────────────────────────┘ │
│ ────────────────────── │                                  │
│ Shortcut Guide を開く  │                                  │
│ Import       Export    │                                  │
│ ガイドへ再反映         │ 編集    削除                     │
│ ⓘ    設定    Exit      │ 保存・反映の状態                 │
│ バックグラウンドで待機 │                                  │
└────────────────────────┴──────────────────────────────────┘
```

※配置を説明する模式図です。実際のアイコンや表示幅とは異なります。

|場所|できること|
|---|---|
|左上のセクション見出し|セクションの追加・名前変更・削除|
|左側の一覧|セクションの選択とドラッグによる並べ替え|
|右側の一覧|選択したセクションのショートカットを確認・編集|
|検索欄|現在のセクション内で名前・説明・キー／表示テキストを検索|
|左下の共通操作|ガイド起動、Import / Export、設定、情報、終了|
|右下の状態表示|保存やガイド反映の結果を確認|

小さなアイコンにポインターを置くと、操作内容のツールチップが表示されます。

## 日常の使い方

### ショートカットを追加・編集する

**追加先は、左側で選択しているセクションです。**

編集するときは、ブロックを選んで **編集**、ブロック内をダブルクリック、または一覧にフォーカスがある状態で `F2` を押します。編集画面では別セクションへの移動もできます。

一覧の空白をクリックすると選択を解除します。空白から編集画面は開きません。

<details>
<summary><strong>登録画面の項目と、2つの入力モード</strong></summary>

|項目|内容|
|---|---|
|名前|必須。早見表に表示する操作名|
|説明|任意。操作の補足|
|キーの組み合わせ|Win / Ctrl / Shift / Altと、メインキーを選択|
|自由テキスト|1〜120文字・1行の任意の表示テキスト|
|ガイドに表示する内容|選択中のモードのプレビュー|
|おすすめに表示|PowerToysのおすすめ欄にも表示するための指定|

**選んだモードの入力欄とラベルだけが表示されます。** プレビューは両方のモードで利用できます。ダイアログ内でモードを切り替えても、入力中の内容は保持されます。

自由テキストも、PowerToysではキーと同じ**枠付き**の表示になります。枠なしの文章をこの位置へ表示する機能ではありません。

記号キーの表示名はWindowsの現在のキーボード配列から取得します。保存する情報はキーの表示文字ではなく、Windowsの仮想キーコードです。

最後に **保存** を押すと変更を適用します。**キャンセル** で閉じた場合は、その編集内容を適用しません。

</details>

<details>
<summary><strong>実際にキーを押して登録する</strong></summary>

1. 「キーの組み合わせ」を選びます。
2. **キーを記録** を押します。
3. 登録したい修飾キーとメインキーを押します。
4. すべてのキーを離すと確定し、登録画面へ戻ります。
5. プレビューを確認して保存します。

- キーの記録は記録画面が開いている間だけ行います。
- `Esc` 自体も登録できます。記録の中止には画面の **キャンセル** を使ってください。
- 記録画面からフォーカスが外れると記録を中止します。
- `Ctrl + Alt + Delete` などWindowsが保護する操作は、手動のキー選択を使ってください。

</details>

<details>
<summary><strong>並べ替え・削除・キーボード操作</strong></summary>

**並べ替え**：セクションやショートカットをドラッグし、青い挿入線の位置で離します。一覧の外で離すか、ドラッグ中にEscを押すとキャンセルできます。長い一覧では端でドラッグを動かすとスクロールします。検索中はショートカットを並べ替えできません。

**削除**：対象を選んで削除し、確認画面で確定します。セクションの削除は、その中のショートカットも削除します。

|キー|操作|
|---|---|
|`Ctrl + F`|選択セクション内の検索欄へ移動|
|`Ctrl + N`|選択セクションへショートカットを追加|
|`F2`|一覧で選択したショートカットを編集|
|`Alt + ↑ / ↓`|フォーカスがある一覧の選択項目を並べ替え|

</details>

### 常駐と通知領域アイコン

|操作|動作|
|---|---|
|通知領域アイコンを左クリック|PowerToys Shortcut Guideを開く|
|通知領域アイコンを右クリック|Open Editor / Open Shortcut Guide / Exit のメニューを表示|
|Open Editor|編集画面を開く|
|編集画面の閉じるボタン|標準設定では通知領域へ戻り、常駐を継続|
|Exit|常駐を含めてアプリを終了|
|EXEをもう一度起動|既存プロセスの編集画面を表示|

**ログイン時の自動起動では、編集画面は表示せず通知領域で待機します。** アイコンが見当たらない場合は、通知領域の隠れているアイコンも確認してください。

### 設定と情報

**設定** の変更は、画面内の **保存** で適用します。

|設定|動作|
|---|---|
|Windowsログイン時に自動起動|現在のEXEを、現在のWindowsユーザーのログイン時に起動するよう登録|
|閉じるボタンで通知領域へ|オフにすると、閉じる操作でタスクバーへ最小化|
|Shortcut Guideに My Shortcuts を表示|生成YAMLによるガイドへの表示を切り替え|

**ⓘ 情報** では、本体・JSON・バックアップ・生成YAML・自動起動の登録先とコマンドを確認できます。各パスの横の小さなコピーボタンで、その値だけをコピーできます。最下部には **本体の移動方法** の折りたたみ案内があります。

## バックアップと設置場所の変更

<details>
<summary><strong>Export / Importでデータを持ち運ぶ</strong></summary>

- **Export**：ライブラリとアプリ設定をJSONへ書き出します。定期的なバックアップや別のPCへ渡す用途に使えます。
- **Import**：JSONを検証し、確認後に**ライブラリ全体を置き換えます**。既存データへの追記・マージではありません。
- Import時は、移行先で現在使っているアプリ設定を維持します。

手動で編集したJSONや、旧版で扱えない形式は読み込みを拒否する場合があります。自由テキストを含むJSONは0.2.0より前の版では読み込めません。

</details>

<details>
<summary><strong>本体を移動する・更新する</strong></summary>

1. 必要に応じてExportでバックアップを取ります。
2. **Exit** でアプリを終了します。
3. **移動**の場合は本体のフォルダーを移します。**更新**の場合は配布ZIPを展開し、本体と付属ファイルを置き換えます。
4. 移動先・更新後の `MyShortcutGuide.exe` を起動します。
5. 移動後も自動起動を使う場合は、設定で自動起動をオンにして **保存** します。すでにオンでも、保存すると現在のEXEの場所へ登録を更新します。

EXEの名前は変更しないでください。ガイド連携は `MyShortcutGuide.exe` という名前で常駐プロセスを検出します。

本体と登録データの保存先は別です。同じPC・同じWindowsユーザー内では、本体フォルダーを移動しても同じデータを参照します。別のPC・ユーザーへ移行する場合はExport / Importを使用してください。

デスクトップやタスクバーの起動用ショートカットが古い場所を指している場合は作り直してください。

</details>

<details>
<summary><strong>保存先の一覧とバックアップの扱い</strong></summary>

以下の `%LOCALAPPDATA%` は、実行中のWindowsユーザーのローカルアプリデータフォルダーです。実際のパスは **ⓘ 情報** で確認できます。

|用途|保存先|
|---|---|
|ショートカットとアプリ設定の正本|`%LOCALAPPDATA%\MyShortcutGuide\shortcuts.json`|
|直前のJSONバックアップ|同じフォルダーの `shortcuts.json.bak`|
|ガイド用の生成YAML|`%LOCALAPPDATA%\Microsoft\WinGet\KeyboardShortcuts\Local.MyShortcutGuide.en-US.yml`|
|索引再生成時の退避ファイル|`%LOCALAPPDATA%\MyShortcutGuide\powertoys-index.last-good.yml.bak`|
|自動起動の登録|レジストリ `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run` の `MyShortcutGuide` 値|

自動起動のコマンドは `"本体の絶対パス" --background` です。

JSONとYAMLは一時ファイルへ書いた後に置換します。`.bak` は直前の保存内容で、世代別の履歴ではありません。長期保存したい状態はExportしてください。

YAMLは生成物です。編集内容はJSONが正本なので、YAMLを直接編集しても、起動・保存・再反映で再生成されます。

</details>

<details>
<summary><strong>アンインストール</strong></summary>

1. 設定で自動起動とShortcut Guideへの表示をオフにして保存します。
2. **Exit** で終了します。
3. 本体のフォルダーを削除します。

ユーザーデータのJSONは残ります。データも削除する場合は、必要なExportを済ませてから上記のデータフォルダーを削除してください。

</details>

## 困ったとき

<details>
<summary><strong>ガイドに表示されない・変更が反映されない</strong></summary>

1. PowerToysが起動していて、Shortcut Guideが有効か確認します。
2. My Shortcut Guideが常駐しているか確認します。
3. 設定の **Shortcut Guideに My Shortcuts を表示** を確認します。
4. **ガイドへ再反映** を押し、状態表示を確認します。
5. 開いているガイドを閉じて開き直します。

JSONの保存とガイドへの反映は別の処理です。反映に失敗しても、保存済みのJSONは保持されます。

PowerToysのバージョンによって連携用の内部機能が変わる場合があります。アプリから開けない場合は、PowerToys側の起動ショートカットでも確認してください。

</details>

<details>
<summary><strong>再起動後、編集画面が空になったように見える</strong></summary>

まず、データを追加・Import・保存する前に、JSONとバックアップを別の場所へコピーしてください。

- 選択セクションや検索条件が原因で項目が隠れていないか確認します。
- **ⓘ 情報** にある本体とJSONのパスを確認します。
- 本体が複数の場所にある場合は、意図したEXEを起動しているか確認します。
- JSONにデータが残っている場合は、アプリをExitで終了してから起動し直して確認します。

**既知の未解決事象**：保存JSONにはデータがあるのに、起動した編集画面が空の初期データを表示した事例があります。終了・起動し直すことで表示が戻ったことは確認していますが、根本原因と再発防止は未確認です。

壊れたJSONや未対応の形式の場合はエラーを表示します。バックアップを戻す場合も、まず現状のファイルを退避してください。`.bak` の内容を確認したうえで、アプリ終了中に `shortcuts.json` へ戻します。

</details>

<details>
<summary><strong>Windowsログイン時に自動起動しない</strong></summary>

1. **ⓘ 情報** で登録されている起動コマンドを確認します。
2. 本体を移動した場合は、新しい場所から起動し、自動起動をオンにして保存し直します。
3. Windowsのスタートアップアプリ管理で無効化されていないか確認します。
4. 自動起動では編集画面を開かないため、通知領域やタスクマネージャーで常駐を確認します。

登録が存在する状態で自動起動しなかったという報告については、原因を調査中です。登録コマンドによるバックグラウンド起動・データ読み込みは確認していますが、実際の再ログインによる再発解消は未確認です。

</details>

<details>
<summary><strong>ガイドのアイコン・キー・自由テキストの表示が違う</strong></summary>

- **未選択時のアイコン**：確認したPowerToysの実装では、最初は共通アイコンを表示し、項目を選択した時点でEXEのアイコンを読み込みます。
- **記号キー**：Windowsのキーボード配列に依存します。JIS配列とUS配列では、同じ仮想キーコードの表示文字が異なる場合があります。
- **自由テキスト**：PowerToys側では枠付きで表示されます。枠なしの表示への切り替えはありません。

PowerToys本体や組み込みマニフェストを変更して、外観を上書きすることはしていません。

</details>

## 開発者向け

<details>
<summary><strong>ビルド・テスト・配布ZIPの作成</strong></summary>

Windowsと.NET 10 SDKを使用します。リポジトリのルートから実行してください。

```powershell
dotnet build MyShortcutGuide.slnx -c Release
dotnet run --project tests/MyShortcutGuide.Tests -c Release
dotnet run --project tests/MyShortcutGuide.WindowsTests -c Release
pwsh -File scripts/publish.ps1 -Version 0.2.3
```

Windowsフォームのテストには対話可能なWindows環境が必要です。

配布物は `artifacts/MyShortcutGuide-<バージョン>-win-x64/` と、同名のZIP・SHA-256チェックサムへ出力します。EXEはWindows x64向けのself-contained single-fileです。ネイティブライブラリの一時展開があり、WinFormsの互換性維持のためトリミングは無効にしています。

GitHub Actionsはビルド・Coreテスト・配布物生成を実行します。`v*` タグでは、添付ファイル付きGitHub Releaseのドラフトを作成します。Windowsフォームのテストは現在のCIには含めていません。

</details>

<details>
<summary><strong>構成・常駐負荷・PowerToys連携</strong></summary>

**C# / .NET 10 / WinForms** を採用しています。NotifyIconとWindowsメッセージループを利用でき、常駐・単一EXE配布に適しているためです。

|場所|役割|
|---|---|
|`src/MyShortcutGuide.Core`|データモデル、検証、JSON保存、YAML出力|
|`src/MyShortcutGuide`|WinForms UI、単一起動、通知領域、Windows・PowerToys連携|
|`tests/`|Coreテスト、Windowsフォームテスト、表示確認用ツール|
|`scripts/publish.ps1`|配布EXE・ZIP・チェックサムの生成|

- 待機はWindowsメッセージと名前付きイベントで行います。定期的なポーリングやファイル監視は行いません。
- キーボードフックはキー記録画面の使用中だけ有効です。
- アプリ独自のテレメトリー・ネットワーク通信はありません。
- UI非依存のCoreと、Windows／PowerToys連携を分離しています。
- 外部NuGet依存はありません。YAMLは固定スキーマの出力専用で、文字列をYAML互換のJSON引用でエスケープします。

マニフェストは次の設定で生成します。

```yaml
PackageName: Local.MyShortcutGuide
Name: My Shortcuts
WindowFilter: MyShortcutGuide.exe
BackgroundProcess: true
```

出力ファイルは `Local.MyShortcutGuide.en-US.yml` です。キーは仮想キーコードで出力します。自由テキストでは、数値や予約語をキーへ変換させないため、YAML出力時のみ先頭へゼロ幅文字を付けます。JSONにはその文字を追加しません。

ガイド起動にはPowerToysの名前付きイベント、索引更新には同梱のIndexYmlGeneratorを使用します。これらはPowerToys内部実装への依存であり、将来の仕様変更に追従が必要です。

**表示上の制約**：System-aware DPIを使用するため、倍率が異なる画面間ではぼやけが生じる場合があります。高コントラスト・スクリーンリーダー全般の網羅検証は未実施です。

詳しい検証記録は、ソースリポジトリの `docs/verification.md` を参照してください。

</details>

<details>
<summary><strong>アイコンの差し替え</strong></summary>

アイコンは `src/MyShortcutGuide/Assets/App.ico`、確認用PNGは同フォルダーの `App.png` です。図形定義と生成処理は `scripts/build-icon.ps1` にあります。

PowerShell 7.6 / .NET 10環境で再生成できます。

```powershell
./scripts/build-icon.ps1 -OutputDirectory ./src/MyShortcutGuide/Assets
```

EXEへ反映するには、その後にビルドまたはpublishを実行してください。

</details>

## 関連資料とライセンス

- [PowerToys Shortcut Guide — Microsoft Learn](https://learn.microsoft.com/windows/powertoys/shortcut-guide)
- [PowerToysのマニフェスト仕様](https://github.com/microsoft/PowerToys/blob/main/doc/specs/WinGet%20Manifest%20Keyboard%20Shortcuts%20schema.md)

**MIT License**。利用条件は同梱の `LICENSE` を参照してください。My Shortcut GuideはMicrosoft PowerToysとは独立したアプリです。
