# Load configuration
$config = Get-Content -Raw -Path ".\config.json" | ConvertFrom-Json
$InstallPath = $config.InstallationPath
$StatusSourcePath = "$InstallPath\Status"
$StatusDestination = "C:\Status"
$ServiceExecutable = "$StatusDestination\service\SoftOne.Status.Service.exe"
$ServiceName = "SoftOne Status"  # Updated Service Name
$InstallUtilPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\installUtil.exe"
$EventLogSource = "SoftOne Status"
$DacpacInstaller = "$InstallPath\DacFramework.msi"
$WebDeployInstaller = "$InstallPath\WebDeploy_amd64_en-US.msi"
$DotNetInstaller = "$InstallPath\dotnet-hosting-9.0.0-win.exe"

# Function to install MSI packages using msiexec
function Install-MSI {
    param (
        [string]$InstallerPath
    )

    if (Test-Path $InstallerPath) {
        Write-Host "Opening MSI Installer: $InstallerPath"
        Start-Process -FilePath "msiexec.exe" -ArgumentList "/i `"$InstallerPath`"" -Verb RunAs -Wait
        Write-Host "Installation completed: $InstallerPath"
    } else {
        Write-Host "Error: MSI Installer not found: $InstallerPath"
    }
}

# Function to install EXE packages normally
function Install-EXE {
    param (
        [string]$InstallerPath
    )

    if (Test-Path $InstallerPath) {
        Write-Host "Opening EXE Installer: $InstallerPath"
        Start-Process -FilePath $InstallerPath -Verb RunAs -Wait
        Write-Host "Installation completed: $InstallerPath"
    } else {
        Write-Host "Error: EXE Installer not found: $InstallerPath"
    }
}

# Install Dacpac (MSI)
Install-MSI $DacpacInstaller

# Install WebDeploy (MSI)
Install-MSI $WebDeployInstaller

# Install .NET 9 Web Hosting
if (Test-Path $DotNetInstaller) {
    Start-Process $DotNetInstaller -ArgumentList "/quiet /norestart" -Wait -NoNewWindow
    Write-Host ".NET 9 Web Hosting Package installed successfully!"
} else { Write-Host ".NET 9 Web Hosting installer not found!" }


Write-Host "All installations completed!"

Write-Host "Checking for missing Status files..."

# Ensure the C:\Status directory exists
if (!(Test-Path $StatusDestination)) {
    Write-Host "Creating Status folder at $StatusDestination..."
    New-Item -Path $StatusDestination -ItemType Directory -Force
}

# Move missing files and folders from source to destination
Get-ChildItem -Path $StatusSourcePath -Recurse | ForEach-Object {
    $DestinationFile = $_.FullName -replace [regex]::Escape($StatusSourcePath), $StatusDestination
    if (!(Test-Path $DestinationFile)) {
        if ($_.PSIsContainer) {
            # Create directories if missing
            Write-Host "Creating folder: $DestinationFile"
            New-Item -Path $DestinationFile -ItemType Directory -Force
        } else {
            # Move files
            Write-Host "Moving file: $_.FullName -> $DestinationFile"
            Move-Item -Path $_.FullName -Destination $DestinationFile -Force
        }
    }
}

Write-Host "✅ Status files updated!"

# Unblock service executable if blocked by Windows
if (Test-Path $ServiceExecutable) {
    Write-Host "Unblocking service executable..."
    Unblock-File -Path $ServiceExecutable
    Write-Host "✅ Service executable unblocked!"

    # Check if service exists before installing
    $ExistingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($ExistingService) {
        Write-Host "✅ Service '$ServiceName' is already installed."
    } else {
        Write-Host "Installing Windows Service: $ServiceName..."
        Start-Process -FilePath $InstallUtilPath -ArgumentList "/username=$ServiceAccount /password=$ServicePassword /unattended `"$ServiceExecutable`"" -Wait -NoNewWindow
        
        # Create service with correct parameters
        sc.exe create "$ServiceName" binPath= "`"$ServiceExecutable`"" DisplayName= "`"$ServiceName`"" start= auto obj= "$ServiceAccount" password= "$ServicePassword"
        Write-Host "✅ Windows Service Installed!"
    }

    # Start the service if not running
    if ((Get-Service -Name $ServiceName).Status -ne 'Running') {
        Write-Host "Starting Windows Service: $ServiceName..."
        Start-Service -Name $ServiceName -ErrorAction SilentlyContinue
        Write-Host "✅ Windows Service Started Successfully!"
    } else {
        Write-Host "✅ Service '$ServiceName' is already running."
    }

} else {
    Write-Host "❌ Error: Service executable not found at $ServiceExecutable"
}

# Install Event Log Source if missing
if (!(Get-EventLog -LogName Application -Source $EventLogSource -ErrorAction SilentlyContinue)) {
    Write-Host "Registering Event Log Source: $EventLogSource..."
    New-EventLog -LogName Application -Source $EventLogSource
    Write-Host "✅ Event Log Source Registered!"
} else {
    Write-Host "✅ Event Log Source already exists!"
}

Write-Host "🎉 All installations and configurations completed!"
