# Load configuration safely
$configPath = ".\config.json"

if (Test-Path $configPath) {
    $config = Get-Content -Raw -Path $configPath | ConvertFrom-Json
} else {
    Write-Host "Error: Configuration file not found!"
    exit 1
}

# Validate configuration
if (-not $config.ServiceAccount -or -not $config.ServicePassword) {
    Write-Host "Error: ServiceAccount or ServicePassword is missing in config.json!"
    exit 1
}

$ServiceAccount = $config.ServiceAccount
$ServicePassword = $config.ServicePassword

# Ensure Secure String conversion works
$SecurePassword = ConvertTo-SecureString -AsPlainText $ServicePassword -Force

# Check if user exists
$UserExists = Get-LocalUser -Name $ServiceAccount -ErrorAction SilentlyContinue
if (-not $UserExists) {
    Write-Host "Creating user: $ServiceAccount"
    New-LocalUser -Name $ServiceAccount -Password $SecurePassword -FullName "Agent User" -Description "Azure Pipelines Agent User" -AccountNeverExpires
}

# Add user to Administrators group safely
$AdminGroup = "Administrators"
$ExistingMembers = Get-LocalGroupMember -Group $AdminGroup | Select-Object -ExpandProperty Name

if ($ExistingMembers -notcontains $ServiceAccount) {
    Write-Host "Adding $ServiceAccount to Administrators group..."
    Add-LocalGroupMember -Group $AdminGroup -Member $ServiceAccount
} else {
    Write-Host "$ServiceAccount is already in Administrators group."
}

Write-Host "User setup completed successfully!"
