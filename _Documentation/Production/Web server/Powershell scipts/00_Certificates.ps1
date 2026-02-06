# Load configuration
$config = Get-Content -Raw -Path ".\config.json" | ConvertFrom-Json
$CertPath = $config.CertificatePath
$Certificates = $config.Certificates
$StoreLocation = $config.CertificateStore

# Ensure the certificate path exists
if (!(Test-Path $CertPath)) {
    Write-Host "Error: Certificate path $CertPath does not exist!"
    exit 1
}

foreach ($CertName in $Certificates) {
    $PfxFile = Join-Path -Path $CertPath -ChildPath "$CertName.pfx"

    if (Test-Path $PfxFile) {
        Write-Host "Installing PFX certificate: $CertName"

        # Prompt for password
        $Password = Read-Host "Enter password for $CertName.pfx" -AsSecureString

        # Install PFX certificate in Local Machine Personal Store
        Import-PfxCertificate -FilePath $PfxFile -CertStoreLocation $StoreLocation -Password $Password

        Write-Host "Installed: $CertName.pfx"
    }
    else {
        Write-Host "Error: Certificate file $CertName.pfx not found!"
    }
}

Write-Host "All certificates installed successfully!"
