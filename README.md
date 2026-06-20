# windows-context-git

Rozszerzenie menu kontekstowego Eksploratora Windows dodające podmenu **Git** przy kliknięciu PPM na folder. Podmenu pojawia się wyłącznie w katalogach z repozytorium git.

![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-blue)
![.NET](https://img.shields.io/badge/.NET-net48%20%7C%20net10--windows-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## Funkcje

| Opcja | Opis |
|---|---|
| **Status** | Lista zmodyfikowanych, staged i untracked plików z kolorami |
| **Fetch** | Pobierz zmiany ze zdalnego repozytorium (streaming output) |
| **Pull** | Pobierz i połącz zmiany |
| **Push** | Wyślij commity — wyszarzone gdy nie ma co wysyłać |
| **Commit** | Staging/unstaging plików, pole commit message, Commit & Push |
| **Branch / Checkout** | Lista gałęzi, checkout, tworzenie nowej gałęzi |

Każda operacja otwiera osobne okienko WPF z ciemnym motywem (OLED dark, paleta Slate).

## Struktura projektu

```
GitContextMenu.slnx
├── src/
│   ├── GitContextMenu.Core/            # netstandard2.0 — shared logic
│   │   ├── Detection/                  # GitRepositoryDetector (filesystem only, bez git.exe)
│   │   └── Git/                        # GitService, parsery, modele
│   ├── GitContextMenu.ShellExtension/  # net48 — COM DLL (SharpShell)
│   └── GitContextMenu.UI/              # net10.0-windows — WPF App
├── tests/
│   └── GitContextMenu.Core.Tests/      # xUnit, 20 testów
└── tools/
    ├── Register.ps1                    # rejestracja DLL (wymaga admina)
    ├── Unregister.ps1                  # wyrejestrowanie
    └── Deploy-Dev.ps1                  # build + deploy dla środowiska dev
```

## Wymagania

- Windows 10 lub Windows 11
- [.NET SDK 10](https://dotnet.microsoft.com/download) (build)
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework) (runtime dla ShellExtension)
- Git zainstalowany i dostępny na `PATH`

## Instalacja (dev)

```powershell
git clone https://github.com/Guliveer/windows-context-git
cd windows-context-git

# Build + skopiuj UI.exe obok DLL + zarejestruj (jako Administrator)
.\tools\Deploy-Dev.ps1
```

Skrypt automatycznie buduje oba projekty, kopiuje pliki i restartuje Explorer.

### Ręczna rejestracja

```powershell
# jako Administrator
dotnet build src/GitContextMenu.ShellExtension -c Debug
dotnet build src/GitContextMenu.UI -c Debug
.\tools\Register.ps1
```

### Wyrejestrowanie

```powershell
# jako Administrator
.\tools\Unregister.ps1
```

## Windows 11

Na Windows 11 menu kontekstowe domyślnie pokazuje uproszczone nowe menu. Podmenu Git pojawi się po kliknięciu **„Pokaż więcej opcji"** lub przez **Shift + PPM**.

Aby przywrócić stare menu kontekstowe globalnie:

```powershell
reg add "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32" /f /ve
```

## Testowanie UI bez rejestracji

```powershell
.\src\GitContextMenu.UI\bin\Debug\net10.0-windows\GitContextMenu.UI.exe `
    --operation status --repo "C:\ścieżka\do\repo"
```

Dostępne operacje: `status`, `commit`, `branch`, `fetch`, `pull`, `push`

## Testy

```powershell
dotnet test tests/GitContextMenu.Core.Tests
```

## Architektura

```
Explorer.exe
  └── COM → GitShellExtension.dll (net48, SharpShell)
               ├── GitRepositoryDetector  — sprawdzenie .git przez filesystem
               ├── CommitCache            — cache TTL 30s dla Push enabled/disabled
               └── Process.Start() → GitContextMenu.UI.exe (net10, WPF)
                                         ├── StatusViewModel  → git status --porcelain=v2
                                         ├── CommitViewModel  → git add / git commit
                                         ├── BranchViewModel  → git branch / git checkout
                                         └── OperationViewModel → streaming git fetch/pull/push
```

UI działa jako **osobny proces** — crash okienka nie crashuje Eksploratora.

## Licencja

MIT
