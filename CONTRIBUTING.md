# Contributing

Thanks for your interest. This project is small and aims to stay lightweight with no external
dependencies.

## Requirements

- Windows 10 22H2 x64 (or Windows 11 for secondary testing)
- .NET Framework 4.8 (included in Windows 10)
- Windows PowerShell 5.1
- Optional: Git and a C# editor

## Build

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

Geometry tests:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\test-geometry.ps1
```

Release packages:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\package.ps1
```

## Code rules

- Code must compile with **C# 5** and the .NET Framework 4.8 `csc.exe`; do not use C# 6+ features
  (string interpolation, `?.`, `nameof`, tuples, local functions, etc.).
- Do not add NuGet packages, third-party DLLs, or components that require installation.
- Add P/Invoke declarations to `src/Native.cs` with comments for constants.
- Every cross-process call must have a timeout or fallback; the app must not block the UI thread.
- Keep compatibility with both restored and maximized windows.
- Do not commit personal data, screenshots with sensitive content, or real logs.

## Minimum manual testing

Before a PR:

1. Restored Notepad: hover Maximize → flyout → choose a zone.
2. Maximized Notepad: hover Restore Down → flyout → choose a zone.
3. One 3-zone and one 4-zone layout.
4. Snap Assist: choose a preview; test `Esc` and click outside.
5. Settings: change delay, gap, and one layout; save; verify the flyout.
6. Tray: enable/disable and exit.

## Commits and pull requests

- Use short, present-tense messages: `fix: ...`, `feat: ...`, `docs: ...`.
- One PR = one logical change.
- Complete the pull request template and list the applications you tested.
- For visual changes, attach before/after screenshots without personal data.

## License

By contributing, you agree that your changes are distributed under the project's MIT license.
