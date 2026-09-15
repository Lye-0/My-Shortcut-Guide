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

エディターの空状態、7セクション、追加フォーム、日本語ラベル、暗色表示、2件のサンプルのキーキャップと選択表示を目視確認。GUIの「下へ」で順序が変わりJSONへ保存されることを確認。DPI初期化の順序とラベル行の高さを修正しました。

- キー記録：実入力 `Ctrl + Shift + F` → `OK: Ctrl + Shift + F` を確認。同じCaptureDialogを単独表示するテストホストを使用しました。
- 閉じる：エディターが消え、同じプロセスが常駐継続。
- 二重起動：再起動操作で既存エディターが開き、プロセスIDは7848のまま、プロセス数は1。
- Exit：配布版のプロセスが終了。
- 再起動：一時的な2件のJSONサンプルを読み込み、名前・説明・キー・Recommendedが復元されることを確認。検証後は元の空ライブラリへ戻しました。

## 常駐時の測定

配布EXEを起動してエディターを閉じ、20.0056秒測定：CPU時間の増加 **0 ms**。ワーキングセット148.5 MB、プライベートメモリ66.2 MB。UI操作と操作ツール接続後の測定値です。常に同じ数値になる保証ではありません。

定期監視・常駐タイマー・FileSystemWatcherはありません。保存処理の15秒の期限は保存中だけ存在します。

## PowerToys連携

インストール済みPowerToys 0.101.2362.0の実DLL内にある `ManifestInterpreter.GetShortcutsOfApplication("Local.MyShortcutGuide")` を呼び出し、My Shortcuts / Local.MyShortcutGuide / MyShortcutGuide.exe / BackgroundProcess=true が返ることを確認。項目を含むYAMLは同梱YamlDotNetで名前・Ctrl・VK70/78・Recommendedまで解析できました。

索引登録と起動イベント受信は確認済みです。ガイドのオーバーレイはWindows操作ツールの対象ウィンドウ一覧に現れず、**PowerToys画面内での最終表示は目視確認できていません**。PowerToysのガイドを開き、左側のMy Shortcutsを確認する手動チェックが残ります。

GUI操作の一部はWindows操作ツールが所有者付きダイアログへ文字入力を渡せなかったため、実フォーム内の入力とSaveボタンを呼び出すWindowsテストで補完しています。全操作を人が一通り操作したユーザビリティテストとは区別します。

## 確認の限界

- 自動ログイン／Windows再起動そのものは行っていません。自動起動は登録内容までの検証です。
- OS予約キー、IMEの全組合せ、スクリーンリーダー、高コントラスト、異なる全表示倍率の網羅検証は未実施です。
- GitHub Actionsはローカルで実行できないため未実行。GitHubへのpush、タグ作成、Release公開は行っていません。
- コード署名とクリーンな別PCでのインストール試験は未実施です。

## 0.1.1 UI改修（2026-09-16）

- セクション見出しに追加・名称変更・削除のアイコンを配置。アクセス可能な名前とツールチップを付与。一覧の選択は上下キー、並べ替えはAlt＋上下キー。
- ガイド起動・Import/Export・再反映・設定・Exitをサイドバーの共通領域へ配置。
- 検索欄の左右・上下の内側余白を確保。背景クリックで入力欄のフォーカスを解除し、関連する入力ラベルは入力欄へフォーカスする。
- Windows実フォームのテストで通常幅と狭幅の配置、入力欄の余白、背景クリック後のフォーカス解除を確認。既存の編集・設定フォームのテストも成功。
- 修正後の画面を目視確認。データモデル・保存先・PowerToys連携の仕様変更はなし。
