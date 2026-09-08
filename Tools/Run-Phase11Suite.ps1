param(
    [string]$Method = 'DungeonRoguelite.Editor.Phase11VerificationRunner.Run',
    [string]$Name = 'gate11_2',
    [string]$Extra = '',
    [int]$TimeoutSeconds = 240
)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$log = Join-Path $project "Logs/$Name.log"
$arguments = "-batchmode -nographics -projectPath `"$project`" -executeMethod $Method -logFile `"$log`" $Extra"
$editor = Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.23f1/Editor/Unity.exe' -ArgumentList $arguments -PassThru -WindowStyle Hidden
if (!$editor.WaitForExit($TimeoutSeconds * 1000)) {
    Stop-Process -Id $editor.Id -Force
    $editor.WaitForExit()
    Write-Output "UNITY TIMEOUT: $Name, PID $($editor.Id), process terminated; suite RED"
    exit 124
}
$editor.Refresh()
Write-Output "UNITY EXIT: $Name, PID $($editor.Id), code $($editor.ExitCode)"
Select-String -LiteralPath $log -Pattern 'COMPLETE|STATE RESTORED|FAILED|error CS|warning CS|Exception:' | ForEach-Object { $_.Line }
if ($editor.ExitCode -eq 0 -and $Extra -notmatch '-quit') {
    $content = Get-Content -LiteralPath $log -Raw
    if ($content -notmatch '(?m)^\[(?:[^\r\n]*COMPLETE|M8\.4 TEST SUCCESS)\][^\r\n]*PASSED' -or
        $content -notmatch '\[PHASE 11 STATE RESTORED\]' -or
        $content -match '(?m)^.*(?:\[.*FAILED|error CS\d+)') {
        Write-Output 'SUITE RED: missing completion/restoration marker or failed assertion/compiler error'
        exit 2
    }
}
exit $editor.ExitCode
