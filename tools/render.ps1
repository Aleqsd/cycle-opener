param([string]$Dotnet='dotnet',[string]$DalamudHome="$env:APPDATA\XIVLauncher\addon\Hooks\dev")
$ErrorActionPreference='Stop'
python "$PSScriptRoot/fetch-icons.py"
if($LASTEXITCODE -ne 0){throw 'Icônes indisponibles'}
& $Dotnet run --project "$PSScriptRoot/Preview/Preview.csproj" -c Release "-p:DalamudHome=$DalamudHome" -- $DalamudHome (Split-Path $PSScriptRoot)
if($LASTEXITCODE -ne 0){throw 'Rendu échoué'}
