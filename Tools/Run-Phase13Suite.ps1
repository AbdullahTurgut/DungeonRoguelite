param([string]$Name = 'gate13_1', [int]$TimeoutSeconds = 240)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$log = Join-Path $project "Logs/$Name.log"
$args = "-batchmode -nographics -projectPath `"$project`" -executeMethod DungeonRoguelite.Editor.Phase13VerificationRunner.Run -logFile `"$log`""
$editor = Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.23f1/Editor/Unity.exe' -ArgumentList $args -PassThru -WindowStyle Hidden
if (!$editor.WaitForExit($TimeoutSeconds * 1000)) { Stop-Process -Id $editor.Id -Force; exit 124 }
$editor.Refresh(); Get-Content $log | Select-String 'GATE 13.1 COMPLETE|PHASE 13 STATE RESTORED|CHECK FAILED|error CS'
if ($editor.ExitCode -ne 0) { exit $editor.ExitCode }
$content = Get-Content $log -Raw
if ($content -notmatch '\[GATE 13\.1 COMPLETE\] All Gate 13\.1 checks PASSED\.' -or $content -notmatch '\[PHASE 13 STATE RESTORED\]' -or $content -match 'error CS|\[CHECK FAILED\]') { exit 2 }
exit 0
