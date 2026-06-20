#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Registers GitContextMenu.ShellExtension.dll as a COM shell extension.
.PARAMETER DllPath
    Path to the DLL. Defaults to the dll in the same directory as this script.
#>
param(
    [string]$DllPath = "$PSScriptRoot\..\src\GitContextMenu.ShellExtension\bin\Debug\net48\GitContextMenu.ShellExtension.dll"
)

$DllPath = Resolve-Path $DllPath -ErrorAction Stop

$regasm = "$env:SystemRoot\Microsoft.NET\Framework64\v4.0.30319\regasm.exe"
if (-not (Test-Path $regasm)) {
    Write-Error "regasm.exe not found at: $regasm"
    exit 1
}

Write-Host "Registering: $DllPath" -ForegroundColor Cyan
& $regasm $DllPath /codebase

if ($LASTEXITCODE -ne 0) {
    Write-Error "regasm failed with exit code $LASTEXITCODE"
    exit 1
}

# Approve the shell extension (required on Windows 10/11)
$clsid = "{7FA2CE94-8132-426E-9C90-D997F34AB77D}"
$approvedKey = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved"
Set-ItemProperty -Path $approvedKey -Name $clsid -Value "Git Context Menu" -Type String -Force
Write-Host "Shell extension approved in registry." -ForegroundColor Green

# Restart Explorer to pick up changes
Write-Host "Restarting Explorer..." -ForegroundColor Yellow
Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500
Start-Process explorer.exe

Write-Host "Done. Right-click a git folder to test." -ForegroundColor Green
