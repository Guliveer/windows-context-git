#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Unregisters GitContextMenu.ShellExtension.dll COM shell extension.
.PARAMETER DllPath
    Path to the DLL.
#>
param(
    [string]$DllPath = "$PSScriptRoot\..\src\GitContextMenu.ShellExtension\bin\Release\net48\GitContextMenu.ShellExtension.dll"
)

$DllPath = Resolve-Path $DllPath -ErrorAction SilentlyContinue
if (-not $DllPath) {
    Write-Warning "DLL not found, removing registry entries only."
} else {
    $regasm = "$env:SystemRoot\Microsoft.NET\Framework64\v4.0.30319\regasm.exe"
    Write-Host "Unregistering: $DllPath" -ForegroundColor Cyan
    & $regasm $DllPath /unregister
}

# Remove approval entry
$clsid = "{7FA2CE94-8132-426E-9C90-D997F34AB77D}"
$approvedKey = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved"
Remove-ItemProperty -Path $approvedKey -Name $clsid -ErrorAction SilentlyContinue
Write-Host "Shell extension approval removed." -ForegroundColor Green

# Restart Explorer
Write-Host "Restarting Explorer..." -ForegroundColor Yellow
Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500
Start-Process explorer.exe

Write-Host "Done." -ForegroundColor Green
