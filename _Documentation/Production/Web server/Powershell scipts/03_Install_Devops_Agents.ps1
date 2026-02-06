# Load configuration
$config = Get-Content -Raw -Path ".\config.json" | ConvertFrom-Json
$PoolName = $config.PoolName
$PAT = $config.PAT
$WorkFolder = "_work"
$ServiceAccount = $config.ServiceAccount
$ServicePassword = $config.ServicePassword
$ServerUrl = $config.ServerUrl
$AgentDownloadURL = $config.AgentDownloadURL
$AgentZip = $config.AgentZip
$AgentsRoot = "E:\Agents"

# Create the main Agents directory if it doesn't exist
if (-Not (Test-Path -Path $AgentsRoot)) {
    New-Item -Path $AgentsRoot -ItemType Directory -Force
    Write-Host "Created directory: $AgentsRoot"
}

# Ensure the agent package exists
if (-Not (Test-Path -Path $AgentZip)) {
    Write-Host "Agent ZIP not found at $AgentZip. Downloading from $AgentDownloadURL..."
    Invoke-WebRequest -Uri $AgentDownloadURL -OutFile $AgentZip
}

# Loop through 5 agents
for ($i = 1; $i -le 5; $i++) {
    $AgentName = "Agent$i"
    $AgentPath = Join-Path -Path $AgentsRoot -ChildPath $AgentName

    # Check if the agent directory exists
    if (Test-Path -Path $AgentPath) {
        Write-Host "The folder $AgentPath already exists. This might be from another machine."
        $UserInput = Read-Host "Do you want to delete and recreate it? (Y/N)"
        
        if ($UserInput -eq "Y" -or $UserInput -eq "y") {
            Write-Host "Deleting $AgentPath..."
            # Use robocopy to handle long path issues
            robocopy $AgentPath "E:\TempDelete" /mir /purge
            Remove-Item -Path $AgentPath -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "Deleted $AgentPath."
        } else {
            Write-Host "Skipping agent setup for $AgentName."
            continue
        }
    }

    # Create Agent folder
    New-Item -Path $AgentPath -ItemType Directory -Force
    Write-Host "Created directory: $AgentPath"

    # Extract agent files
    Write-Host "Extracting agent files to $AgentPath..."
    Expand-Archive -Path $AgentZip -DestinationPath $AgentPath -Force

    # Verify config.cmd exists
    $ConfigCmdPath = Join-Path -Path $AgentPath -ChildPath "config.cmd"
    if (-Not (Test-Path -Path $ConfigCmdPath)) {
        Write-Host "Error: config.cmd not found in $AgentPath! Extraction may have failed."
        exit 1
    }

    Write-Host "Configuring Agent: $AgentName..."

    # Run configuration command
    Set-Location $AgentPath
    Start-Sleep -Seconds 2
    Start-Process -FilePath "cmd.exe" -ArgumentList "/c config.cmd --unattended --url $ServerUrl --auth pat --token $PAT --pool $PoolName --agent $AgentName --work $WorkFolder --runAsService --windowsLogonAccount $ServiceAccount --windowsLogonPassword $ServicePassword" -Wait -NoNewWindow

    Write-Host "Agent '$AgentName' configured successfully!"
}

Write-Host "All agents installed!"
