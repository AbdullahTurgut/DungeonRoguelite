param(
    [ValidateSet('setup', 'carousel', '10_QA', '12_5', '14_1', '14_5')]
    [string]$Suite = 'carousel',
    [int]$TimeoutSeconds = 240
)
$ErrorActionPreference = 'Stop'
if (Get-Process Unity -ErrorAction SilentlyContinue) { throw 'Close the existing Unity Editor before running an isolated UI suite.' }
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$log = Join-Path $project "Logs/carousel_$Suite.log"
$method = switch ($Suite) {
    'setup' { 'DungeonRoguelite.Editor.WorldMapCarouselSetup.Run' }
    'carousel' { 'DungeonRoguelite.Editor.WorldMapCarouselVerificationRunner.Run' }
    '10_QA' { 'DungeonRoguelite.Editor.Milestone11_1_VerificationRunner.RunRegression' }
    '12_5' { 'DungeonRoguelite.Editor.Phase12VerificationRunner.Run' }
    default { 'DungeonRoguelite.Editor.Phase14VerificationRunner.Run' }
}
$extra = if ($Suite -eq 'setup') { '-quit' } else { "-gateSuite $Suite" }
$arguments = "-batchmode -nographics -projectPath `"$project`" -executeMethod $method $extra -logFile `"$log`""
$editor = Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.23f1/Editor/Unity.exe' -ArgumentList $arguments -PassThru -WindowStyle Hidden
if (!$editor.WaitForExit($TimeoutSeconds * 1000)) {
    Stop-Process -Id $editor.Id -Force
    $editor.WaitForExit()
    Write-Output "UNITY TIMEOUT: $Suite; RED"
    exit 124
}
$editor.Refresh()
Write-Output "UNITY EXIT: $Suite code $($editor.ExitCode)"
$content = Get-Content -LiteralPath $log -Raw
Select-String -LiteralPath $log -Pattern '^\[.*COMPLETE\]|^\[.*STATE RESTORED\]|FAILED|error CS|warning CS|Exception:' | ForEach-Object { $_.Line }
if ($editor.ExitCode -ne 0) { exit $editor.ExitCode }
if ($content -match 'error CS\d+|Exception:|\[[^\r\n]*FAILED') { exit 2 }
# Only these exact diagnostics at the three verified legacy sites are accepted.
# A different file, location, warning code or message remains RED.
$baselineSites = @(
    'Assets/Editor/Milestone8_5_Setup.cs(368,17)',
    'Assets/Editor/Milestone9_Setup.cs(593,13)',
    'Assets/Editor/Milestone6_1_Setup.cs(307,17)'
)
$baselineMessage = "warning CS0618: 'TMP_Text.enableWordWrapping' is obsolete: 'The enabledWordWrapping property is now obsolete. Please use the textWrappingMode property instead.'"
$acceptedWarnings = @($baselineSites | ForEach-Object { "${_}: $baselineMessage" })
$warnings = @($content -split '\r?\n' | Where-Object { $_ -match 'warning CS\d+' } | ForEach-Object { $_.Replace('\', '/').Trim() } | Select-Object -Unique)
$newWarnings = @($warnings | Where-Object { $_ -cnotin $acceptedWarnings })
foreach ($warning in $warnings) {
    if ($warning -cin $acceptedWarnings) { Write-Output "BASELINE WARNING (pre-existing): $warning" }
}
if ($newWarnings.Count -gt 0) {
    $newWarnings | ForEach-Object { Write-Output "RED: unbaselined compiler warning: $_" }
    exit 2
}
$pass = switch ($Suite) {
    'setup' { '\[WORLD MAP CAROUSEL SETUP COMPLETE\]' }
    'carousel' { '\[WORLD MAP CAROUSEL COMPLETE\] PASSED:' }
    '10_QA' { '\[MANUAL QA COMPLETE\] Verification PASSED\.' }
    '12_5' { '\[GATE 12.5 COMPLETE\] PASSED:' }
    default { "\[GATE $($Suite.Replace('_','.')) COMPLETE\] All checks PASSED\." }
}
if ($content -notmatch $pass) { Write-Output 'RED: missing PASS marker'; exit 2 }
if ($Suite -ne 'setup') {
    $restore = if ($Suite -match '^14_') { '\[PHASE 14 STATE RESTORED\]' } else { '\[PHASE 12 STATE RESTORED\]' }
    if ($content -notmatch $restore) { Write-Output 'RED: missing state restoration'; exit 2 }
}
if ($Suite -eq 'carousel' -and $content -notmatch '\[WORLD MAP CAROUSEL STATE RESTORED\]') { exit 2 }
exit 0
