$mixes = @{
'Wave_01' = @(10,0,0,0); 'Wave_02' = @(10,3,0,0); 'Wave_03' = @(7,7,1,0)
'Wave_02_01' = @(5,2,1,0); 'Wave_02_02' = @(5,2,0,1); 'Wave_02_03' = @(4,4,0,1); 'Wave_02_04' = @(4,3,1,2)
'Wave_03_01' = @(5,4,1,0); 'Wave_03_02' = @(5,4,0,2); 'Wave_03_03' = @(4,3,1,2); 'Wave_03_04' = @(5,3,1,2)
}
$names = @('Zombie','Runner','Tank','Ranged')
$refs = @{}
foreach ($enemyName in $names) {
 $prefabPath = "Assets/Prefabs/Enemies/$enemyName.prefab"
 $prefabText = Get-Content $prefabPath -Raw
 $guid = [regex]::Match((Get-Content "$prefabPath.meta" -Raw), 'guid: (\w+)').Groups[1].Value
 $obj = [regex]::Matches($prefabText, '(?s)--- !u!1 &(\d+)\r?\nGameObject:.*?(?=---|\z)') | Where-Object { $_.Value -match "m_Name: $enemyName\r?\n" }
 $refs[$enemyName] = "{fileID: $($obj.Groups[1].Value), guid: $guid, type: 3}"
}
foreach ($waveName in $mixes.Keys) {
 $wavePath = "Assets/ScriptableObjects/Waves/$waveName.asset"
 $waveText = Get-Content $wavePath -Raw
 $entries = "  enemyEntries:`n"
 $order = if ($waveName -match '^Wave_0[23]_') { @(2,3,0,1) } else { @(0,2,1) }
 foreach ($i in $order) {
  if ($mixes[$waveName][$i] -gt 0) { $entries += "  - enemyPrefab: $($refs[$names[$i]])`n    count: $($mixes[$waveName][$i])`n" }
 }
 $waveText = [regex]::Replace($waveText, '(?s)  enemyEntries:.*?(?=  spawnInterval:)', $entries)
 [IO.File]::WriteAllText((Join-Path (Get-Location) $wavePath), $waveText)
}
