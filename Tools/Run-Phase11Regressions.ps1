param([string]$Prefix, [string]$Suites)
$ErrorActionPreference = 'Stop'
foreach ($suite in $Suites.Split(',')) {
    $method = if ($suite -eq '11_1') { 'DungeonRoguelite.Editor.Milestone11_1_VerificationRunner.Run' }
        elseif ($suite.StartsWith('11_')) { 'DungeonRoguelite.Editor.Phase11VerificationRunner.Run' }
        else { 'DungeonRoguelite.Editor.Milestone11_1_VerificationRunner.RunRegression' }
    & "$PSScriptRoot/Run-Phase11Suite.ps1" -Method $method -Name "${Prefix}_${suite}" -Extra "-gateSuite $suite"
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
exit 0
