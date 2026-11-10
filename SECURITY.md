# Securitate

## Versiuni acceptate

| Versiune | Suport |
|----------|--------|
| 1.3.x | da |
| < 1.3 | nu |

## Cum raportezi o vulnerabilitate

Nu deschide un issue public pentru probleme de securitate. Folosește
**GitHub Security Advisories** din repository (Security → Report a vulnerability) sau contactează
mentenitorul prin profilul GitHub.

Include:

- versiunea SnapLayouts 10 și build-ul Windows;
- pașii de reproducere;
- impactul estimat;
- dacă este nevoie, un proof of concept fără date personale.

## Model de securitate

- Aplicația nu are rețea, telemetrie, auto-update sau servicii.
- Rulează cu privilegiile utilizatorului curent (`asInvoker`), nu cere administrator.
- Citește poziția cursorului și trimite `WM_NCHITTEST` către fereastra activă.
- Mută ferestre prin `SetWindowPos`; nu injectează cod în alte procese.
- Ferestrele elevate pot refuza mutarea (UIPI); rulați aplicația ca administrator doar dacă aveți
  nevoie de acest comportament.

## Confidențialitate și jurnale

`%APPDATA%\SnapLayouts10\snap.log` poate conține titluri de ferestre și nume de clase, dar numai
când este pornit `--debug` sau la evenimente de ancorare. Înainte de a atașa un jurnal public,
eliminați titlurile documentelor, numele de utilizator și alte date sensibile.

## Binare nesemnate

Executabilele publicate nu sunt semnate digital. Windows SmartScreen sau antivirusul pot afișa
avertismente. Verificați `SHA256SUMS.txt` din release și, dacă doriți, compilați din sursă.
