# Changelog

All notable changes are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and versions use
[Semantic Versioning](https://semver.org/).

## [1.4.0] - 2026-11-10

### Changed

- Translated the application UI, runtime messages, documentation, issue templates, and metadata to
  English.
- Renamed documentation files to English names.
- Updated screenshots and social preview for the English release.

## [1.3.0] - 2026-11-10

### Changed

- Visual redesign based on official Windows 11 reference material.
- Flyout without individual cards: large zones drawn directly on the panel.
- Roomier 2 × 3 flyout with a settings gear in the left rail.
- Blurred acrylic-style flyout background.
- Snap Assist without a panel, title, or list: floating window previews with shadows over a blurred
  desktop.
- Live DWM previews preserve each window's aspect ratio.

### Added

- Visual design analysis of Windows 11.
- Compatibility documentation.
- CI build, geometry tests, and release packaging.

## [1.2.0] - 2026-11-10

### Changed

- Snap Assist moved into the free zone instead of a centered panel.
- Adaptive grid of live DWM previews.

## [1.1.0] - 2026-11-10

### Added

- Visual Snap Assist with up to six windows, live DWM previews, and visual selection.

## [1.0.1] - 2026-11-10

### Fixed

- The menu now appears for maximized windows (Restore Down); the window is restored before snapping.

## [1.0.0] - 2026-11-10

### Added

- Maximize detection via `WM_NCHITTEST`.
- Six layouts, multi-monitor snapping, Snap Assist, tray, settings, and autostart.
- Dependency-free build for Windows 10 22H2 x64.
