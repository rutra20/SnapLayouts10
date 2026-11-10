# Compatibility

## Reference configuration

- Windows 10 IoT Enterprise 22H2, build 19045.6456, x64
- 1920 × 1080, 100% DPI
- .NET Framework 4.8

## Tested applications

| Application | Menu on hover | Snapping | Notes |
|-------------|---------------|----------|-------|
| Classic Notepad | yes | yes | primary test target |
| Google Chrome | yes | yes | standard title bar |
| Windows Explorer | yes | yes | also used for previews |
| Snipping Tool | yes | yes | DWM preview |
| Electron apps (e.g. VS Code) | usually | usually | depends on title bar |
| UWP apps | variable | variable | may use a different frame |
| Apps running as administrator | variable | sometimes refused | UIPI; run SnapLayouts as admin |

The table is indicative. Windows 10 lets applications draw their own title bars, so behavior may
differ.

## Known limitations

- Windows that do not respond to `WM_NCHITTEST` with `HTMAXBUTTON` are not detected.
- Elevated windows may reject `SetWindowPos`.
- Multi-monitor support uses per-monitor APIs, but hardware testing used one physical monitor.
- This is an independent application, not a shell integration.
- The executable is not code-signed.

## Reporting compatibility

Open an issue using the **Compatibility report** template and include:

- application and version;
- Windows version and build;
- restored or maximized state;
- result and reproduction steps;
- screenshots without personal data.

Thank you!
