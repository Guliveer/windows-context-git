<#
.SYNOPSIS
    Kopiuje GitContextMenu.UI.exe obok ShellExtension DLL dla środowiska deweloperskiego.
    Następnie rejestruje DLL jeśli uruchomiony jako administrator.
#>

$root = "$PSScriptRoot\.."
$dllDir = "$root\src\GitContextMenu.ShellExtension\bin\Debug\net48"
$uiExe  = "$root\src\GitContextMenu.UI\bin\Debug\net10.0-windows\GitContextMenu.UI.exe"

# Buduj projekty
Write-Host "Budowanie..." -ForegroundColor Cyan
dotnet build "$root\src\GitContextMenu.ShellExtension\GitContextMenu.ShellExtension.csproj" -c Debug -v q
dotnet build "$root\src\GitContextMenu.UI\GitContextMenu.UI.csproj" -c Debug -v q

# Kopiuj UI exe obok DLL
Write-Host "Kopiowanie UI.exe -> $dllDir" -ForegroundColor Cyan
Copy-Item $uiExe -Destination $dllDir -Force

# Kopiuj też runtimey (.NET 10 self-contained nie jest potrzebny jeśli .NET zainstalowany)
# Kopiuj wszystkie pliki z katalogu UI exe (dlls, runtimeconfig.json)
$uiDir = Split-Path $uiExe
Get-ChildItem $uiDir -File | ForEach-Object {
    Copy-Item $_.FullName -Destination $dllDir -Force
}

Write-Host "Skopiowano. Pliki w $dllDir" -ForegroundColor Green
Get-ChildItem $dllDir | Select-Object Name | Format-Table

# Zarejestruj jeśli admin
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)

if ($isAdmin) {
    & "$PSScriptRoot\Register.ps1" -DllPath "$dllDir\GitContextMenu.ShellExtension.dll"
} else {
    Write-Host ""
    Write-Host "Uruchom jako Administrator i wywołaj:" -ForegroundColor Yellow
    Write-Host "  .\tools\Register.ps1 -DllPath `"$dllDir\GitContextMenu.ShellExtension.dll`"" -ForegroundColor White
}
