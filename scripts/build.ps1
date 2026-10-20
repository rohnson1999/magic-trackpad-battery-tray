param(
    [switch]$RunTests,
    [string]$OutputDirectory = 'artifacts'
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$taskCompiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $taskCompiler)) { throw 'The Windows .NET Framework x64 C# compiler was not found.' }
if (-not [IO.Path]::IsPathRooted($OutputDirectory)) { $OutputDirectory = Join-Path $taskRoot $OutputDirectory }
$taskOutput = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
$taskSources = @(Get-ChildItem -LiteralPath (Join-Path $taskRoot 'src') -Filter '*.cs' | Sort-Object Name | ForEach-Object { $_.FullName })
$taskBinary = Join-Path $taskOutput 'MagicTrackpadBattery.exe'
& $taskCompiler /nologo /target:winexe /platform:x64 /optimize+ /codepage:65001 /r:System.Windows.Forms.dll /r:System.Drawing.dll "/out:$taskBinary" $taskSources
if ($LASTEXITCODE -ne 0) { throw 'Application compilation failed.' }
if ($RunTests) {
    $taskTestSources = @($taskSources | Where-Object { [IO.Path]::GetFileName($_) -ne 'Program.cs' })
    $taskTestSources += Join-Path $taskRoot 'tests\LogicTests.cs'
    $taskTestBinary = Join-Path $taskOutput 'LogicTests.exe'
    & $taskCompiler /nologo /target:exe /platform:x64 /optimize+ /codepage:65001 /r:System.Windows.Forms.dll /r:System.Drawing.dll /main:MagicTrackpadBattery.LogicTests "/out:$taskTestBinary" $taskTestSources
    if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed.' }
    $taskResults = Join-Path $taskOutput 'test-results.txt'
    & $taskTestBinary $taskResults
    if ($LASTEXITCODE -ne 0) { throw ('Logic tests failed: ' + [IO.File]::ReadAllText($taskResults)) }
    Write-Output ([IO.File]::ReadAllText($taskResults))
}
Get-Item -LiteralPath $taskBinary | Select-Object FullName,Length
