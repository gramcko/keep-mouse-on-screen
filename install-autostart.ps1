#Requires -RunAsAdministrator
# Registers Touch Monitor to start at logon with highest privileges and starts it now.
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$exe = Join-Path $root 'dist\TouchMonitor.exe'
if (-not (Test-Path -LiteralPath $exe)) {
    throw "TouchMonitor.exe not found. Run build.ps1 first (expected: $exe)"
}

$user = "$env:USERDOMAIN\$env:USERNAME"
$action = New-ScheduledTaskAction -Execute $exe
$trigger = New-ScheduledTaskTrigger -AtLogOn -User $user
$principal = New-ScheduledTaskPrincipal -UserId $user -RunLevel Highest -LogonType Interactive
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit ([TimeSpan]::Zero) -MultipleInstances IgnoreNew

Register-ScheduledTask -TaskName 'TouchMonitor' -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null

Get-Process -Name 'TouchMonitor' -ErrorAction SilentlyContinue | Stop-Process -Force
Start-ScheduledTask -TaskName 'TouchMonitor'
Write-Host 'Touch Monitor installed and started. It will run at every logon with administrator rights.'
