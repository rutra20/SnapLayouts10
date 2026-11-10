# SnapLayouts 10

**Unofficial Snap Layouts for Windows 10.** Hover over a window's **Maximize** button for about
300 ms and a Windows 11-style menu appears with six layouts. Click a zone to snap the window there —
no keyboard shortcuts and no FancyZones.

[![build](https://github.com/rutra20/SnapLayouts10/actions/workflows/build.yml/badge.svg)](https://github.com/rutra20/SnapLayouts10/actions/workflows/build.yml)
![Windows 10](https://img.shields.io/badge/Windows-10%20x64-0078D4)
![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)
![No telemetry](https://img.shields.io/badge/telemetry-none-success)

> **Unofficial project.** Not affiliated with, sponsored by, or approved by Microsoft. Windows,
> Snap Layouts, and Snap Assist are trademarks of Microsoft Corporation. See [NOTICE.md](NOTICE.md).

- Version: **1.4.0**
- Platform: **Windows 10 22H2 x64**, .NET Framework 4.8
- Tested on: **Windows 10 IoT Enterprise 22H2, build 19045.6456**, 1920 × 1080, 100% DPI
- UI language: English
- Portable: no installer, no network access, no telemetry

![SnapLayouts 10 flyout](docs/preview-flyout.png)

## Download

Latest release: **[GitHub Releases](https://github.com/rutra20/SnapLayouts10/releases/latest)**

- [SnapLayouts10.exe](https://github.com/rutra20/SnapLayouts10/releases/latest/download/SnapLayouts10.exe) — portable app
- [SnapLayouts10-1.4.0-portable.zip](https://github.com/rutra20/SnapLayouts10/releases/latest/download/SnapLayouts10-1.4.0-portable.zip) — app + documentation
- [SnapLayouts10-1.4.0-source.zip](https://github.com/rutra20/SnapLayouts10/releases/latest/download/SnapLayouts10-1.4.0-source.zip) — full source
- [SHA256SUMS.txt](https://github.com/rutra20/SnapLayouts10/releases/latest/download/SHA256SUMS.txt) — integrity

## Documentation

| Document | Contents |
|----------|----------|
| [TESTING.md](docs/TESTING.md) | environment, test steps, results, limitations |
| [COMPATIBILITY.md](docs/COMPATIBILITY.md) | tested applications and known limitations |
| [WINDOWS11-DESIGN.md](docs/WINDOWS11-DESIGN.md) | visual analysis of Windows 11 and design decisions |
| [PUBLISHING-GITHUB.md](docs/PUBLISHING-GITHUB.md) | release and publication guide |
| [CHANGELOG.md](CHANGELOG.md) | version history |
| [CONTRIBUTING.md](CONTRIBUTING.md) | build, style, testing, pull requests |
| [SECURITY.md](SECURITY.md) | vulnerability reporting and privacy |

## Features

1. Detects the Maximize button with `WM_NCHITTEST` (`HTMAXBUTTON`), without global mouse hooks.
2. Shows the layout menu after 300 ms of pointer rest (configurable).
3. Six layouts with 2, 3, or 4 zones, styled after Windows 11.
4. Highlights the zone under the cursor and snaps on click.
5. Works with the monitor under the cursor, per-monitor DPI, and physical coordinates.
6. Compensates for invisible DWM window borders so snapped windows align visually.
7. Opens a visual **Snap Assist** in the remaining zone with live window previews.
8. Runs in the system tray with settings and optional automatic startup.

## Layouts

| # | Layout | Zones |
|---|--------|-------|
| 1 | 2 equal columns | 2 |
| 2 | 2 columns 70/30 | 2 |
| 3 | 3 equal columns | 3 |
| 4 | Left + 2 right | 3 |
| 5 | 4 quadrants | 4 |
| 6 | Large left + 3 right | 4 |

Layouts can be enabled or disabled individually in Settings.

## Install and run

1. Copy `SnapLayouts10.exe` to a permanent folder, for example
   `C:\Users\<user>\AppData\Local\Programs\SnapLayouts10\`.
2. Run `SnapLayouts10.exe`. The app appears in the system tray.
3. Right-click the tray icon:
   - **Snap Layouts: enabled** — toggle detection;
   - **Start with Windows: yes/no** — manage the `HKCU\...\Run` entry;
   - **Settings...** — delay, gap, Snap Assist, active layouts;
   - **About**, **Open data folder**, **Exit**.
4. Double-click the tray icon to open Settings.

Run with `--settings` to open Settings immediately. Run with `--debug` to enable detailed logging.

## Usage

1. Focus the window you want to snap (Notepad, Explorer, Chrome, etc.).
2. Rest the pointer on the **Maximize** button for about 300 ms.
3. Choose a zone in the flyout.
4. The window moves to that zone. If Snap Assist is enabled, choose another window for the next
   zone, or press `Esc`.

The flyout closes when the pointer leaves it, when the target loses focus, or when the Maximize
button is pressed. Hovering the **Restore Down** button of a maximized window works too; the window
is restored before snapping.

## Settings

Settings file: `%APPDATA%\SnapLayouts10\settings.ini`

```ini
enabled=1
hoverdelayms=300
gap=8
snapassist=1
startwithwindows=0
firstrun=0
layouts=1,1,1,1,1,1
```

- `hoverdelayms` — 200…700 ms in the UI (1000 ms maximum in the file).
- `gap` — gap between windows, 0…16 px in the UI.
- `layouts` — one `1`/`0` for each of the six layouts.

Log: `%APPDATA%\SnapLayouts10\snap.log` (automatic rotation at 256 KB).

## How it works

### Maximize button detection

A 40 ms timer:

1. reads the pointer position (`GetCursorPos`);
2. finds the window under the pointer (`WindowFromPoint` + `GetAncestor(GA_ROOT)`);
3. verifies that it is the foreground window, visible, and has `WS_MAXIMIZEBOX`;
4. sends `WM_NCHITTEST` and checks for `HTMAXBUTTON (9)`.

If the window does not respond within 80 ms, a fallback region in the top-right corner is used.
No global mouse hook is installed, so the input thread is not blocked.

### Flyout

After 300 ms, the app computes the monitor work area (`MonitorFromPoint` + `GetMonitorInfo`) and
the target DPI (`GetDpiForWindow`). The flyout is a borderless, rounded, topmost window with
`WS_EX_NOACTIVATE`, so it does not steal focus. It opens below the Maximize button and flips above
when there is not enough room.

Layout previews are drawn directly on the panel, without card backgrounds, with large rounded zones
and a settings gear. The background uses a blurred screen capture with an acrylic-style tint.

### Snapping

For each zone, the app computes a physical-pixel rectangle, restores maximized/minimized windows,
and calls `SetWindowPos`. The difference between `GetWindowRect` and
`DWMWA_EXTENDED_FRAME_BOUNDS` is compensated so invisible borders do not produce uneven gaps.

### Visual Snap Assist

After the first snap, the selector opens **inside the remaining zone**. Each candidate window is
shown as a separate floating preview with a shadow, over a blurred wallpaper background. Previews
are rendered live with DWM (`DwmRegisterThumbnail` + `DwmUpdateThumbnailProperties`) and preserve
window aspect ratios. Click a preview to snap it; `Esc` or a click outside cancels. Up to six
windows are shown.

### Multiple monitors

The work area, DPI, and flyout placement are computed for the monitor under the cursor. Snapping
uses the target monitor's work area. APIs such as `GetDpiForWindow` and `MonitorFromPoint` are
per-monitor aware, and the manifest declares `PerMonitorV2`.

## Build from source

Only components included in Windows 10 are required:

- `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`
- .NET Framework 4.8 (included in Windows 10 22H2)
- Windows PowerShell 5.1

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

Output: `dist\SnapLayouts10.exe`. The script also generates `assets\app.ico` if missing. Nothing is
downloaded from the network.

Automated geometry tests:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\test-geometry.ps1
```

Release packages:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\package.ps1
```

CI (`.github/workflows/build.yml`) builds, tests, and packages on `windows-latest`.
`.github/workflows/release.yml` publishes a release when a `v*` tag is pushed. See
[PUBLISHING-GITHUB.md](docs/PUBLISHING-GITHUB.md).

## Project structure

```
SnapLayouts10/
├── .github/
│   ├── workflows/             # build CI + automatic release
│   ├── ISSUE_TEMPLATE/
│   └── PULL_REQUEST_TEMPLATE.md
├── build.ps1                  # csc build
├── package.ps1                # portable/source packages + SHA-256
├── README.md
├── LICENSE
├── NOTICE.md
├── CHANGELOG.md
├── CONTRIBUTING.md
├── SECURITY.md
├── assets/app.ico
├── dist/                      # generated locally, ignored by Git
├── docs/
│   ├── TESTING.md
│   ├── COMPATIBILITY.md
│   ├── WINDOWS11-DESIGN.md
│   ├── PUBLISHING-GITHUB.md
│   ├── preview-flyout.png
│   └── social-preview.png
├── src/                       # application source
└── tools/                     # icon generator + geometry tests
```

## Testing

See [TESTING.md](docs/TESTING.md). On the test machine:

- Maximize detection worked for Notepad and Chrome (`WM_NCHITTEST = 9`);
- the flyout appeared after roughly 360–430 ms from the first detection tick;
- snapping worked for 2 columns, 70/30, 3 columns, and 4 quadrants;
- Snap Assist opened in the free zone with six floating live previews and snapped the selected
  window;
- settings, startup, and tray behavior were verified;
- automated geometry tests pass for all six layouts and a simulated second monitor.

## Known limitations

- Windows with fully custom title bars that do not return `HTMAXBUTTON` may not be detected.
- Elevated applications may reject `SetWindowPos` from a normal-integrity process (UIPI). Run
  SnapLayouts 10 as administrator if you need that.
- Multi-monitor support is implemented with per-monitor APIs, but hardware testing used one
  physical monitor.
- Snap Assist shows up to six windows, selected by Z-order.
- Preview rendering depends on DWM; if a thumbnail cannot be created, the app icon is shown.
- The executable is **not code-signed**. SmartScreen may warn on first run. Verify the hash or build
  from source.
- Windows 7/8 are not supported; target is Windows 10 22H2 x64 (also runs on Windows 11).

## Privacy

The app has no network, telemetry, or auto-update. It writes only:

- `%APPDATA%\SnapLayouts10\settings.ini`
- `%APPDATA%\SnapLayouts10\snap.log`
- optionally, at your request, `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\SnapLayouts10`

The log may contain window titles when `--debug` is enabled. Remove sensitive data before sharing it.
See [SECURITY.md](SECURITY.md).

## Contributing

Contributions are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) and
[CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md). For vulnerabilities, use [SECURITY.md](SECURITY.md), not
a public issue.

## License

[MIT](LICENSE) © 2026 SnapLayouts 10 contributors.

Unofficial project; see [NOTICE.md](NOTICE.md).
