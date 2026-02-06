# Define Orleans Silo Ports
$OrleansSiloPort = 11111
$OrleansGatewayPort = 30000
$OrleansDashboardPort = 8080

# Create Inbound Rule for Orleans Silo Port
New-NetFirewallRule -DisplayName "Orleans Silo Inbound" -Direction Inbound -Action Allow -Protocol TCP -LocalPort $OrleansSiloPort

# Create Outbound Rule for Orleans Silo Port
New-NetFirewallRule -DisplayName "Orleans Silo Outbound" -Direction Outbound -Action Allow -Protocol TCP -LocalPort $OrleansSiloPort

# Create Inbound Rule for Orleans Gateway Port
New-NetFirewallRule -DisplayName "Orleans Gateway Inbound" -Direction Inbound -Action Allow -Protocol TCP -LocalPort $OrleansGatewayPort

# Create Outbound Rule for Orleans Gateway Port
New-NetFirewallRule -DisplayName "Orleans Gateway Outbound" -Direction Outbound -Action Allow -Protocol TCP -LocalPort $OrleansGatewayPort

# Optional: Create Inbound Rule for Orleans Dashboard
New-NetFirewallRule -DisplayName "Orleans Dashboard Inbound" -Direction Inbound -Action Allow -Protocol TCP -LocalPort $OrleansDashboardPort

# Optional: Create Outbound Rule for Orleans Dashboard
New-NetFirewallRule -DisplayName "Orleans Dashboard Outbound" -Direction Outbound -Action Allow -Protocol TCP -LocalPort $OrleansDashboardPort

# Confirm Rules Were Created
Get-NetFirewallRule -DisplayName "Orleans*"
