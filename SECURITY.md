# Security

## Supported versions

| Version | Supported |
|---------|-----------|
| 1.4.x | yes |
| < 1.4 | no |

## Reporting a vulnerability

Do not open a public issue for security problems. Use **GitHub Security Advisories** in this
repository (Security → Report a vulnerability) or contact the maintainer through the GitHub profile.

Please include:

- SnapLayouts 10 version and Windows build;
- reproduction steps;
- estimated impact;
- a proof of concept without personal data, if needed.

## Security model

- The app has no network, telemetry, auto-update, or external services.
- It runs with the current user's privileges (`asInvoker`) and does not require administrator.
- It reads the pointer position and sends `WM_NCHITTEST` to the foreground window.
- It moves windows with `SetWindowPos`; it does not inject code into other processes.
- Elevated windows may refuse to move (UIPI). Run the app as administrator only if required.

## Privacy and logs

`%APPDATA%\SnapLayouts10\snap.log` may contain window titles and class names when `--debug` is
enabled or during snapping events. Before sharing a log publicly, remove document titles, user
names, and other sensitive data.

## Unsigned binaries

Release binaries are not code-signed. Windows SmartScreen or antivirus software may warn. Verify
`SHA256SUMS.txt` from the release and/or build from source.
