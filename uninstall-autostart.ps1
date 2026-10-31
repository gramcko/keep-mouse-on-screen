#Requires -RunAsAdministrator
# Stops Touch Monitor and removes the logon task. Settings are kept.
$ErrorActionPreference = 'SilentlyContinue'

Get-Process -Name 'TouchMonitor' | Stop-Process -Force
Unregister-ScheduledTask -TaskName 'TouchMonitor' -Confirm:$false
Write-Host 'Touch Monitor removed. Settings kept in HKCU\Software\TouchMonitor.'
