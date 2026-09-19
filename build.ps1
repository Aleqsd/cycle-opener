param([string]$Dotnet='dotnet',[string]$DalamudHome="$env:APPDATA\XIVLauncher\addon\Hooks\dev")
$ErrorActionPreference='Stop'
$cycleManifest=Get-Content -LiteralPath "$PSScriptRoot/src/CycleOpener.json" -Raw | ConvertFrom-Json
$cycleVersion=([version]$cycleManifest.AssemblyVersion).ToString(3)
& $Dotnet build "$PSScriptRoot/src/CycleOpener.csproj" -c Release "-p:DalamudHome=$DalamudHome" --nologo
if($LASTEXITCODE -ne 0){throw 'Compilation échouée'}
& $Dotnet run --project "$PSScriptRoot/tests/CoreChecks.csproj" -c Release --nologo
if($LASTEXITCODE -ne 0){throw 'Contrôles échoués'}
$cycleSourceDll="$PSScriptRoot/src/bin/Release/net10.0-windows/CycleOpener.dll"
if([System.Reflection.AssemblyName]::GetAssemblyName($cycleSourceDll).Version.ToString() -ne $cycleManifest.AssemblyVersion){throw 'Version DLL/manifeste différente'}
foreach($cycleDestination in @("$PSScriptRoot/plugin","$PSScriptRoot/releases/$cycleVersion")) {
 New-Item -ItemType Directory -Path $cycleDestination -Force | Out-Null
 foreach($cycleFile in @('CycleOpener.json','CycleOpener.dll')) {Copy-Item -LiteralPath "$PSScriptRoot/src/bin/Release/net10.0-windows/$cycleFile" -Destination $cycleDestination -Force}
 Copy-Item -LiteralPath "$PSScriptRoot/assets/icon.png","$PSScriptRoot/LICENSE" -Destination $cycleDestination -Force
 if((Get-FileHash "$cycleDestination/CycleOpener.dll").Hash -ne (Get-FileHash "$PSScriptRoot/src/bin/Release/net10.0-windows/CycleOpener.dll").Hash){throw 'DLL différente'}
}
$cycleArchive="$PSScriptRoot/releases/CycleOpener-$cycleVersion.zip"
$cycleVersionDirectory="$PSScriptRoot/releases/$cycleVersion"
Compress-Archive -LiteralPath "$cycleVersionDirectory/CycleOpener.dll","$cycleVersionDirectory/CycleOpener.json","$cycleVersionDirectory/icon.png","$cycleVersionDirectory/LICENSE" -DestinationPath $cycleArchive -Force
$cycleSumFiles=@($cycleArchive,"$cycleVersionDirectory/CycleOpener.dll","$cycleVersionDirectory/icon.png")
$cycleSumLines=foreach($cycleSumFile in $cycleSumFiles){"$((Get-FileHash -LiteralPath $cycleSumFile).Hash.ToLowerInvariant())  $([IO.Path]::GetFileName($cycleSumFile))"}
$cycleSumLines | Set-Content -LiteralPath "$PSScriptRoot/releases/SHA256SUMS.txt" -Encoding utf8NoBOM
Get-Item "$PSScriptRoot/plugin/CycleOpener.dll" | Select-Object FullName,Length
