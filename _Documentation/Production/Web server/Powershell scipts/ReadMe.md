# Configuration for PowerShell Scripts

This document explains the parameters in the `config.json` file used by the PowerShell scripts.

## Parameters

- **PoolName**: The name of the agent pool in Azure DevOps where the agents will be registered.
- **ServiceAccount**: The account under which the agent service will run.
- **ServicePassword**: The password for the service account. Make sure to set this securely.
- **UNCPath**: The UNC path to the shared folder where the sites are located.
- **InstallationPath**: The local path where installation files are stored.
- **PAT**: The Personal Access Token (PAT) required to authenticate with Azure DevOps. You need to generate a PAT and set it here.
- **ServerUrl**: The URL of the Azure DevOps server.
- **AgentDownloadURL**: The URL to download the Azure DevOps agent package.
- **AgentZip**: The local path where the downloaded agent package is stored.
- **CertificatePath**: The local path where the certificates are stored.
- **Certificates**: A list of certificates required for the setup.
  - **SoftOneId**: Certificate for SoftOneId.
  - **SoftOneKeyVault**: Certificate for SoftOneKeyVault.
  - **StarSoftOneExp2503**: Certificate for StarSoftOneExp2503.
- **CertificateStore**: The certificate store location in the local machine.

## Example `config.json`

```json
{
    "PoolName": "SoftOnes67",
    "ServiceAccount": "Agent",
    "ServicePassword": "SET THE PASSWORD HERE",
    "UNCPath": "\\\\192.168.50.69\\Sites",
    "InstallationPath": "E:\\Installation files",
    "PAT": "YOU NEED TO GET A PAT AND SET THE PAT HERE",
    "ServerUrl" : "https://dev.azure.com/softonedev",
    "AgentDownloadURL" : "https://vstsagentpackage.azureedge.net/agent/4.251.0/vsts-agent-win-x64-4.251.0.zip",
    "AgentZip" : "E:\\Installation files\\vsts-agent-win-x64-4.251.0.zip",
    "CertificatePath": "E:\\Installation files",
    "Certificates": [
        "SoftOneId",
        "SoftOneKeyVault",
        "StarSoftOneExp2503"
    ],
    "CertificateStore": "Cert:\\LocalMachine\\My"
}