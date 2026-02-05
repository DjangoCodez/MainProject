[CmdletBinding()]

param(
                           [Parameter(Position=1)]
                           [string]$dacPacPath,
                                                       [Parameter(Position=2)]
                           [string]$databaseName,
                                                       [Parameter(Position=3)]
                           [string]$databaseServer,
                                                       [Parameter(Position=4)]
                           [string]$databaseLogin,
                                                       [Parameter(Position=5)]
                           [string]$databasePassword,
                                                        [Parameter(Position=6)]
                           [string]$blockOnPossibleDataLoss = "true"
)
$path = "C:\Program Files\Microsoft SQL Server\150\DAC\bin\sqlpackage.exe”

Write-Verbose -Verbose ("Running $path /Action:Publish /SourceFile:$dacPacPath /TargetDatabaseName:$databaseName /TargetServerName:$databaseServer /TargetUser:$databaseLogin/TargetPassword:$databasePassword")

$output = & $path /Action:Publish /p:BlockOnPossibleDataLoss=$blockOnPossibleDataLoss /SourceFile:"$dacPacPath" /TargetDatabaseName:$databaseName /TargetServerName:$databaseServer /TargetUser:$databaseLogin /TargetPassword:$databasePassword 2>&1 

Write-Verbose ($output | Out-String) -Verbose