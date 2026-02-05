# This script copies TypeScript types/endpoints from Soe.Angular.Spa, originally generated based on the Soe.WebApi & Soe.Common C# classes holding the [TSInclude] attribute.
# Execute ".\scripts\CopyWebApiModels.ps1" from the root of Soe.UI.Test using PowerShell.

param (
    [string]$SourcePath1 = "NewSource\Soe.Angular.Spa\src\app\shared\models\generated-interfaces",
    [string]$SourcePath2 = "NewSource\Soe.Angular.Spa\src\app\shared\services\generated-service-endpoints",
    [string]$DestinationPath1 = "NewSource\Soe.UI.Test\models\webapi\generated-interfaces",
    [string]$DestinationPath2 = "NewSource\Soe.UI.Test\models\webapi\generated-service-endpoint"
)

if (!(Test-Path -Path $DestinationPath1)) {
    Write-Host "Creating destination folder at $DestinationPath1"
    New-Item -ItemType Directory -Force -Path $DestinationPath1
}

if (!(Test-Path -Path $DestinationPath2)) {
    Write-Host "Creating destination folder at $DestinationPath2"
    New-Item -ItemType Directory -Force -Path $DestinationPath2
}

if (Test-Path -Path $SourcePath1) {
    Write-Host "Copying files from $SourcePath1 to $DestinationPath1"
    Copy-Item -Path "$SourcePath1\*" -Destination $DestinationPath1 -Recurse -Force
} else {
    Write-Host "Source path $SourcePath1 does not exist."
}

if (Test-Path -Path $SourcePath2) {
    Write-Host "Copying files from $SourcePath2 to $DestinationPath2"
    Copy-Item -Path "$SourcePath2\*" -Destination $DestinationPath2 -Recurse -Force
} else {
    Write-Host "Source path $SourcePath2 does not exist."
}

Write-Host "File copy operation completed."