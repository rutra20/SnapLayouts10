# Compatibilitate

## Configurația de referință

- Windows 10 IoT Enterprise 22H2, build 19045.6456, x64
- 1920 × 1080, DPI 100%
- .NET Framework 4.8

## Aplicații testate

| Aplicație | Meniu la hover | Ancorare | Observații |
|-----------|----------------|----------|------------|
| Notepad (clasic) | da | da | test principal |
| Google Chrome | da | da | titlebar standard |
| Windows Explorer | da | da | folosit și pentru previzualizări |
| Snipping Tool | da | da | previzualizare DWM |
| Aplicații Electron (ex. VS Code) | de regulă da | de regulă da | depinde de titlebar |
| Aplicații UWP | variabil | variabil | pot folosi alt frame |
| Aplicații rulate ca administrator | variabil | uneori refuzat | UIPI; rulați și SnapLayouts ca admin |

Tabelul este orientativ. Windows 10 permite aplicațiilor să deseneze propriul titlebar, deci
comportamentul poate diferi.

## Limitări cunoscute

- Ferestrele care nu răspund la `WM_NCHITTEST` cu `HTMAXBUTTON` nu sunt detectate.
- Ferestrele elevate pot refuza `SetWindowPos`.
- Multi-monitor este implementat cu API-uri per-monitor, dar testarea hardware a fost făcută pe un
  singur monitor.
- Nu este o integrare în shell-ul Windows; este o aplicație independentă.
- Executabilul nu este semnat digital.

## Cum raportezi compatibilitatea

Deschide un issue cu template-ul **Raport de compatibilitate** și include:

- aplicația + versiunea;
- Windows + build;
- restaurată / maximizată;
- rezultatul și pașii de reproducere;
- capturi fără date personale.

Mulțumim!
