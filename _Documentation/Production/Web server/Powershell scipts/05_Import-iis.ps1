Write-Host "Starting IIS Import..."

# Get the directory where the script is located
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition

# Define XML file paths dynamically
$AppPoolsXml = Join-Path -Path $ScriptPath -ChildPath "IIS_AppPools.xml"
$SitesXml = Join-Path -Path $ScriptPath -ChildPath "IIS_Sites.xml"

# Ensure IIS is installed
if (-not (Get-WindowsFeature -Name Web-Server).Installed) {
    Write-Host "IIS is not installed. Installing now..."
    Install-WindowsFeature -Name Web-Server -IncludeManagementTools
}

# Ensure appcmd.exe exists
$AppCmd = "C:\Windows\System32\inetsrv\appcmd.exe"
if (-not (Test-Path $AppCmd)) {
    Write-Host "❌ Error: appcmd.exe not found! IIS may not be installed correctly."
    exit 1
}

# Import Application Pools (Using Get-Content for Redirection)
if (Test-Path $AppPoolsXml) {
    Write-Host "Importing IIS Application Pools from $AppPoolsXml..."
    Get-Content $AppPoolsXml | & $AppCmd add apppool /in
    Write-Host "✅ IIS Application Pools imported successfully!"
} else {
    Write-Host "❌ Error: Application Pools XML file not found at $AppPoolsXml"
}

# Import Sites (Using Get-Content for Redirection)
if (Test-Path $SitesXml) {
    Write-Host "Importing IIS Sites from $SitesXml..."
    Get-Content $SitesXml | & $AppCmd add site /in
    Write-Host "✅ IIS Sites imported successfully!"
} else {
    Write-Host "❌ Error: Sites XML file not found at $SitesXml"
}

Write-Host "IIS Import Completed!"
