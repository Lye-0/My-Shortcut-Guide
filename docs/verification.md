# 検証記録

環境：Windows 11 build 26200 / x64、.NET SDK 10.0.401、PowerToys 0.101.2362.0。
日付：2026-09-15〜16。

## 自動検証

- Releaseビルド：警告0、エラー0。
- Coreテスト：14件成功。JSON再読込、バックアップ、CRUD・順序・移動、Import/Export、不正形式・欠損フィールド・重複ID・重複JSONプロパティ・サイズ制限、YAMLエスケープ、空リスト、表示ON/OFFを含みます。
- Windowsフォームテスト：実フォームの入力→保存、セクション選択、修飾キー、メインキー、Recommended、設定保存を検証。ラベルの実寸が必要な高さを下回らないことも確認。
- 自動起動：実際のHKCU RunエントリーのON/OFF、引用符付きEXEパス、`--background` を検証後、元の登録内容に復元。
- PowerToys同梱YamlDotNetによる生成YAMLの解析成功。索引にMyShortcutGuide.exe / Local.MyShortcutGuideが登録されることを確認。
- Windows x64 self-contained single-fileのpublish成功。

```powershell
dotnet run --project tests/MyShortcutGuide.Tests -c Release
dotnet run --project tests/MyShortcutGuide.WindowsTests -c Release
# 自動起動まで確認する場合（検証後に元へ戻します）
dotnet run --project tests/MyShortcutGuide.WindowsTests -c Release -- --startup "C:\path\MyShortcutGuide.exe"
```

## UIの実機確認

エディターの空状態、7セクション、追加フォーム、日本語ラベル、暗色表示を目視確認。DPI初期化の順序とラベル行の高さを修正しました。Exitによるプロセス終了を確認。

GUI操作の一部はWindows操作ツールが所有者付きダイアログへ文字入力を渡せなかったため、実フォーム内の入力とSaveボタンを呼び出すWindowsテストで補完しています。全操作を人が一通り操作したユーザビリティテストとは区別します。

## 確認の限界

- 自動ログイン／Windows再起動そのものは行っていません。自動起動は登録内容までの検証です。
- OS予約キー、IMEの全組合せ、スクリーンリーダー、高コントラスト、異なる全表示倍率の網羅検証は未実施です。
- PowerToysの起動イベントはログで受信を確認しました。実表示の結果は追加確認中です。
- GitHub Actionsはローカルで実行できないため未実行。GitHubへのpush、タグ作成、Release公開は行っていません。
- コード署名とクリーンな別PCでのインストール試験は未実施です。
