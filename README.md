# windows-context-git

A Windows Explorer context menu extension that adds a **Git** submenu when right-clicking a folder. The submenu only appears in directories that contain a git repository.

![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-blue)
![.NET](https://img.shields.io/badge/.NET-net48%20%7C%20net10--windows-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

| Option | Description |
|---|---|
| **Status** | Lists modified, staged, and untracked files with color coding |
| **Fetch** | Fetch changes from the remote repository (streaming output) |
| **Pull** | Fetch and merge remote changes |
| **Push** | Send local commits — grayed out when there is nothing to push |
| **Commit** | Stage/unstage files, enter a commit message, Commit & Push |
| **Branch / Checkout** | List branches, switch branch, create a new branch |

Each operation opens a separate WPF window with a dark theme (OLED dark, Slate palette).

## Project structure

```
GitContextMenu.slnx
├── src/
│   ├── GitContextMenu.Core/            # netstandard2.0 — shared logic
│   │   ├── Detection/                  # GitRepositoryDetector (filesystem only, no git.exe)
│   │   └── Git/                        # GitService, parsers, models
│   ├── GitContextMenu.ShellExtension/  # net48 — COM DLL (SharpShell)
│   └── GitContextMenu.UI/              # net10.0-windows — WPF App
├── tests/
│   └── GitContextMenu.Core.Tests/      # xUnit, 20 tests
└── tools/
    ├── Register.ps1                    # register the DLL (requires admin)
    ├── Unregister.ps1                  # unregister the DLL
    └── Deploy-Dev.ps1                  # build + deploy for local dev
```

## Requirements

- Windows 10 or Windows 11
- [.NET SDK 10](https://dotnet.microsoft.com/download) (build)
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework) (runtime for ShellExtension)
- Git installed and available on `PATH`

## Installation (dev)

```powershell
git clone https://github.com/Guliveer/windows-context-git
cd windows-context-git

# Build + copy UI.exe next to the DLL + register (run as Administrator)
.\tools\Deploy-Dev.ps1
```

The script builds both projects, copies the files, and restarts Explorer automatically.

### Manual registration

```powershell
# Run as Administrator
dotnet build src/GitContextMenu.ShellExtension -c Debug
dotnet build src/GitContextMenu.UI -c Debug
.\tools\Register.ps1
```

### Unregistration

```powershell
# Run as Administrator
.\tools\Unregister.ps1
```

## Windows 11

Windows 11 shows a simplified context menu by default. The Git submenu will appear after clicking **"Show more options"** or by using **Shift + Right-click**.

To restore the classic context menu globally:

```powershell
reg add "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32" /f /ve
```

## Testing the UI without registration

```powershell
.\src\GitContextMenu.UI\bin\Debug\net10.0-windows\GitContextMenu.UI.exe `
    --operation status --repo "C:\path\to\repo"
```

Available operations: `status`, `commit`, `branch`, `fetch`, `pull`, `push`

## Tests

```powershell
dotnet test tests/GitContextMenu.Core.Tests
```

## Architecture

```
Explorer.exe
  └── COM → GitShellExtension.dll (net48, SharpShell)
               ├── GitRepositoryDetector  — detects .git via filesystem (no process spawn)
               ├── CommitCache            — 30s TTL cache for Push enabled/disabled state
               └── Process.Start() → GitContextMenu.UI.exe (net10, WPF)
                                         ├── StatusViewModel  → git status --porcelain=v2
                                         ├── CommitViewModel  → git add / git commit
                                         ├── BranchViewModel  → git branch / git checkout
                                         └── OperationViewModel → streaming git fetch/pull/push
```

The UI runs as a **separate process** — a crash in the window cannot crash Explorer.

## License

MIT
