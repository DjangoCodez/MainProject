# Load configuration
$config = Get-Content -Raw -Path ".\config.json" | ConvertFrom-Json
$UNCPath = $config.UNCPath
$Destination = "E:\Sites"

if (!(Test-Path $Destination)) {
    New-Item -Path $Destination -ItemType Directory
}

Write-Host "Copying sites from $UNCPath to $Destination..."
Copy-Item -Path "$UNCPath\*" -Destination $Destination -Recurse -Force

Write-Host "Sites copied successfully!"
