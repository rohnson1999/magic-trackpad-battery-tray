$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
& (Join-Path $PSScriptRoot 'build.ps1') -RunTests
$taskVersion = ([IO.File]::ReadAllText((Join-Path $taskRoot 'VERSION'))).Trim()
if ($taskVersion -notmatch '^\d+\.\d+\.\d+$') { throw 'VERSION must contain a semantic version such as 1.0.0.' }
$taskArtifacts = Join-Path $taskRoot 'artifacts'
$taskStage = Join-Path $taskArtifacts ('MagicTrackpadBattery-v' + $taskVersion + '-windows-x64')
New-Item -ItemType Directory -Path $taskStage -Force | Out-Null
$taskFiles = @('README.md','README.en.md','LICENSE','CHANGELOG.md')
foreach ($taskFile in $taskFiles) { Copy-Item -LiteralPath (Join-Path $taskRoot $taskFile) -Destination $taskStage -Force }
Copy-Item -LiteralPath (Join-Path $taskArtifacts 'MagicTrackpadBattery.exe') -Destination $taskStage -Force
$taskPayload = @('MagicTrackpadBattery.exe') + $taskFiles
$taskPackageFiles = @($taskPayload | ForEach-Object { Join-Path $taskStage $_ })
$taskZip = Join-Path $taskArtifacts ('MagicTrackpadBattery-v' + $taskVersion + '-windows-x64.zip')
Compress-Archive -LiteralPath $taskPackageFiles -DestinationPath $taskZip -Force
$taskChecksums = Join-Path $taskArtifacts 'SHA256SUMS.txt'
$taskHashLines = @($taskZip,(Join-Path $taskArtifacts 'MagicTrackpadBattery.exe')) | ForEach-Object {
    $taskHash = Get-FileHash -LiteralPath $_ -Algorithm SHA256
    $taskHash.Hash.ToLowerInvariant() + '  ' + [IO.Path]::GetFileName($_)
}
[IO.File]::WriteAllLines($taskChecksums, [string[]]$taskHashLines, [Text.Encoding]::ASCII)
Get-Item -LiteralPath $taskZip,$taskChecksums | Select-Object FullName,Length
