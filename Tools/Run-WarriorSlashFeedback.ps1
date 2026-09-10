param([int]$TimeoutSeconds = 120)

$ErrorActionPreference = 'Stop'
if (Get-Process Unity -ErrorAction SilentlyContinue) { throw 'Close the existing Unity Editor before running the Warrior slash feedback verifier.' }
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$log = Join-Path $project 'Logs/warrior_slash_feedback.log'
$arguments = "-batchmode -nographics -projectPath `"$project`" -executeMethod DungeonRoguelite.Editor.WarriorSlashFeedbackRunner.Run -logFile `"$log`""
$editor = Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.23f1/Editor/Unity.exe' -ArgumentList $arguments -PassThru -WindowStyle Hidden
if (!$editor.WaitForExit($TimeoutSeconds * 1000)) {
    Stop-Process -Id $editor.Id -Force
    $editor.WaitForExit()
    Write-Output 'UNITY TIMEOUT: warrior slash feedback; RED'
    exit 124
}

$content = Get-Content -LiteralPath $log -Raw
Select-String -LiteralPath $log -Pattern '^\[WARRIOR SLASH FEEDBACK (COMPLETE|STATE RESTORED)\]|FAILED|error CS|warning CS|Exception:' | ForEach-Object { $_.Line }
if ($editor.ExitCode -ne 0) { exit $editor.ExitCode }
if ($content -match 'error CS\d+|Exception:|\[WARRIOR SLASH FEEDBACK CHECK FAILED\]') { exit 2 }
if ($content -notmatch '\[WARRIOR SLASH FEEDBACK COMPLETE\] PASSED:') { Write-Output 'RED: missing pass marker'; exit 2 }
if ($content -notmatch '\[WARRIOR SLASH FEEDBACK STATE RESTORED\]') { Write-Output 'RED: missing state restoration marker'; exit 2 }
exit 0
