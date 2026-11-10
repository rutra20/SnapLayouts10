# Testing report — SnapLayouts 10 v1.4.0

Date: 10 November 2026  
Test machine: Dell, Windows 10 IoT Enterprise 22H2, build **19045.6456**, x64  
Display: 1920 × 1080, one monitor, 100% DPI (96 dpi), work area 1920 × 1040

> Tests used Notepad windows created for testing and a Chrome window. No user files were modified.
> The v1.4.0 release changes UI language and documentation; the behavior is the same as the tested
> v1.3.0 build.

## 1. Build

```text
OK: ...\dist\SnapLayouts10.exe
Version: 1.4.0.0
```

- compiler: `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe` (C# 5, .NET Framework 4.8)
- no SDK, NuGet packages, or network access
- manifest: `asInvoker`, `PerMonitorV2`, Windows 10 compatibility

## 2. Maximize detection

The pointer was placed over the Maximize/Restore Down button and `WM_NCHITTEST` was inspected.

| Window | `WM_NCHITTEST` | Detected |
|--------|----------------|----------|
| Classic Notepad | `9` (`HTMAXBUTTON`) | yes |
| Chrome | `9` (`HTMAXBUTTON`) | yes |

Log excerpt:

```text
[DEBUG] Maximize button detected (WM_NCHITTEST): hwnd=class=Notepad title="..." hit=9
[DEBUG] Maximize candidate: hwnd=... class=Notepad title="..." hit=9
```

Maximized windows are accepted through the Restore Down button. The window is restored before
snapping. Inactive windows are ignored.

## 3. Flyout

- configured delay: **300 ms**
- measured interval from first candidate to flyout: **360–430 ms** (includes 40 ms polling and UI
  work)
- size at 96 dpi: **392 × 382 px** (six layouts, 2 × 3)
- position: below the Maximize button, right-aligned to the window; flips/clamps when needed
- does not steal focus (`WS_EX_NOACTIVATE`, `ShowWithoutActivation`)
- settings gear opens the Settings dialog
- blurred acrylic-style background captured from the screen behind the flyout

## 4. Snapping

All tests used a 1920 × 1040 work area and `gap = 8`.

### 2 equal columns

| Zone | Visible rectangle (L, T, R, B) |
|------|--------------------------------|
| Left | 4, 4, 956, 1036 |
| Right | 964, 4, 1916, 1036 |

### 2 columns 70/30

| Zone | Visible rectangle |
|------|-------------------|
| Large left | 4, 4, 1340, 1036 |
| Right | 1348, 4, 1916, 1036 |

### 3 equal columns

| Zone | Visible rectangle |
|------|-------------------|
| Left | 4, 4, 636, 1036 |
| Center | 644, 4, 1276, 1036 |
| Right | 1284, 4, 1916, 1036 |

### 4 quadrants

| Zone | Visible rectangle |
|------|-------------------|
| Top left | 4, 4, 956, 516 |
| Top right | 964, 4, 1916, 516 |
| Bottom left | 4, 524, 956, 1036 |
| Bottom right | 964, 524, 1916, 1036 |

The 8 px gaps between windows and 4 px outer margin are visible in captures. DWM invisible-border
compensation was verified with `DWMWA_EXTENDED_FRAME_BOUNDS`: the visible rectangle matched the
calculated zone.

## 5. Visual Snap Assist

- the selector opens inside the remaining zone
- candidates appear as separate floating window previews with shadows over a blurred wallpaper
- previews are live and rendered by DWM
- selecting a preview filled the remaining zone:

```text
[INFO] Snap Assist: selected window hwnd=... title="...".
[INFO] Window snapped: hwnd=..., layout=2 equal columns, zone=Right,
       rect=964,4,952,1032
```

- if DWM cannot create a thumbnail, the application icon is shown
- `Esc` and clicking outside close the session
- up to six previews are displayed

## 6. Settings

Verified:

- toggles for enabled, startup, and Snap Assist
- dark combo boxes for delay and gap (open, select, save)
- six layout cards with previews and individual enable/disable
- saving writes `hoverdelayms=400` to `%APPDATA%\SnapLayouts10\settings.ini`
- new values are applied without restart
- settings are written with an atomic `File.Replace`

## 7. Autostart

- enabling writes:

```text
HKCU\Software\Microsoft\Windows\CurrentVersion\Run
SnapLayouts10 = "C:\...\SnapLayouts10.exe"
```

- disabling removes the value
- only `HKCU` is used; no administrator rights

## 8. Multiple monitors

Only one physical monitor was available. A secondary work area was simulated:

- work area: `(1920, 0, 2560, 1400)`
- layout 4 quadrants, bottom-right zone → `(3204, 704, 1272, 692)`

Coordinates are absolute, so monitors placed above or to the left (negative coordinates) work as
well. The flyout uses the monitor under the cursor; snapping uses the target monitor's work area.
Physical multi-monitor testing remains to be done.

## 9. Stability and resources

- runs in the system tray with no main window
- observed working set: about 27 MB
- no global mouse hook; 40 ms pointer polling
- no errors in the log during testing
- single instance: a second launch shows a message and does not create a second functional instance

## 10. Restore Down fix (1.0.1)

- maximized Notepad (`IsZoomed = true`)
- pointer over Restore Down → `WM_NCHITTEST = 9`
- flyout appeared at `1496, 29`, size `416 × 198` in the older build
- clicking the left zone of the 2-column layout restored and snapped the window to
  `4, 4, 956, 1036`

## 11. Visual redesign (1.3.0) and English UI (1.4.0)

Visual design was based on Microsoft's reference material; see
[WINDOWS11-DESIGN.md](WINDOWS11-DESIGN.md).

- flyout without cards, 392 × 382 at 96 dpi
- settings gear works
- floating previews, blurred wallpaper, shadows
- English UI strings in the app, tray, settings, tooltips, and messages
- CI build and geometry tests pass on GitHub Actions

## 12. Remaining limitations

- windows with fully custom title bars that do not return `HTMAXBUTTON` may not be detected
- elevated apps may reject `SetWindowPos` from a normal process (UIPI)
- physical multi-monitor testing is still required
- the executable is not code-signed
