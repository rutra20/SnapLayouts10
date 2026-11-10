# Changelog

Toate modificările notabile sunt documentate aici. Formatul urmează aproximativ
[Keep a Changelog](https://keepachangelog.com/ro/1.1.0/), iar versiunile folosesc
[Semantic Versioning](https://semver.org/).

## [1.3.0] - 2026-11-10

### Changed

- Redesign vizual inspirat din capturile oficiale Windows 11.
- Flyout fără carduri individuale: zone mari desenate direct pe panou, cu spații mai generoase.
- Flyout mai aerisit, 2 × 3, cu rotiță de setări în rail-ul din stânga.
- Fundal acrilic blurat pentru flyout.
- Snap Assist fără panou/titlu/listă: ferestre-previzualizare plutitoare, cu umbră, peste desktopul
  blurat.
- Previzualizările Snap Assist rămân live prin DWM și păstrează proporțiile ferestrelor.

### Added

- Documentație de analiză vizuală Windows 11.
- Raport de compatibilitate.
- Build CI, teste geometrice și pachet de release.

## [1.2.0] - 2026-11-10

### Changed

- Snap Assist mutat în zona liberă, în locul unui panou centrat.
- Grilă adaptivă de previzualizări live DWM.

## [1.1.0] - 2026-11-10

### Added

- Snap Assist vizual cu până la 6 ferestre, previzualizări live DWM și selectare vizuală.

## [1.0.1] - 2026-11-10

### Fixed

- Meniul apare și când fereastra este maximizată (buton Restore Down); fereastra este restaurată
  înainte de ancorare.

## [1.0.0] - 2026-11-10

### Added

- Detectare Maximize prin `WM_NCHITTEST`.
- Șase layout-uri, ancorare multi-monitor, Snap Assist, tray, setări și autostart.
- Build fără dependențe externe, pentru Windows 10 22H2 x64.
