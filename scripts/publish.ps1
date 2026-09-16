param([string]$Version = '0.2.1')
$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$') { throw 'Version must be a semantic version.' }
$repoRoot = Split-Path -Parent $PSScriptRoot
$destination = Join-Path $repoRoot "artifacts/MyShortcutGuide-$Version-win-x64"
dotnet publish (Join-Path $repoRoot 'src/MyShortcutGuide/MyShortcutGuide.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false -p:Version=$Version -o $destination
if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
Copy-Item -LiteralPath (Join-Path $repoRoot 'README.md'), (Join-Path $repoRoot 'LICENSE') -Destination $destination
Copy-Item -LiteralPath (Join-Path $repoRoot 'docs/verification.md') -Destination $destination
$zip = "$destination.zip"
Compress-Archive -Path "$destination/*" -DestinationPath $zip -Force
$hash = Get-FileHash -LiteralPath $zip -Algorithm SHA256
"$($hash.Hash.ToLowerInvariant())  $([IO.Path]::GetFileName($zip))" | Set-Content -LiteralPath "$zip.sha256" -Encoding utf8NoBOM
Write-Output "Release: $destination"
Write-Output "Archive: $zip"
