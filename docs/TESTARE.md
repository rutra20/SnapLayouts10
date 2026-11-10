# Raport de testare — SnapLayouts 10 v1.3.0

Data: 10 noiembrie 2026  
Mașină de test: Dell, Windows 10 IoT Enterprise 22H2, build **19045.6456**, x64  
Afisaj: 1920 × 1080, un monitor, DPI 100% (96 dpi), arie de lucru 1920 × 1040

> Testele au folosit ferestre Notepad create special pentru test și o fereastră Chrome.
> Nu s-au modificat fișiere ale utilizatorului.

## 1. Compilare

```
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

Rezultat:

```
OK: ...\dist\SnapLayouts10.exe (84.480 bytes)
Versiune: 1.3.0.0
```

- compilator: `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe` (C# 5, .NET Framework 4.8)
- fără SDK suplimentar, fără pachete NuGet, fără rețea
- manifest: `asInvoker`, `PerMonitorV2`, compatibilitate Windows 10

## 2. Detectarea butonului Maximize

Metodă: cursor poziționat pe butonul Maximize, apoi `WM_NCHITTEST` verificat în jurnal.

| Fereastră | Rezultat `WM_NCHITTEST` | Detectat |
|---|---|---|
| Notepad clasic | `9` (`HTMAXBUTTON`) | da |
| Chrome | `9` (`HTMAXBUTTON`) | da |

Jurnal (fragment):

```
[DEBUG] Buton Maximize detectat (WM_NCHITTEST): hwnd=class=Notepad title="..." hit=9
[DEBUG] Candidat Maximize: hwnd=... class=Notepad title="..." hit=9
```

Sunt acceptate atât ferestrele restaurate, cât și cele maximizate. Pentru o fereastră maximizată,
`WM_NCHITTEST` pe butonul **Restore Down** returnează tot `9`; aplicația afișează meniul și
restaurează fereastra înainte de ancorare. Butonul este detectat doar pentru fereastra din prim-plan.

## 3. Afișarea meniului

- întârziere configurată: **300 ms**
- măsurat între primul `Candidat Maximize` și `Flyout afișat`: **360–427 ms**
  (include polling-ul de 40 ms, verificarea ferestrei și afișarea formularului)
- dimensiune la 96 dpi: **416 × 198 px**
- poziționare: sub butonul Maximize, aliniat la dreapta ferestrei, cu replasare dacă nu încape
- meniul nu fură focusul ferestrei țintă (`WS_EX_NOACTIVATE`, `ShowWithoutActivation`)

Vizual: [`preview-flyout.png`](preview-flyout.png)

## 4. Ancorarea ferestrelor

Toate testele au folosit aceeași arie de lucru 1920 × 1040 și `gap = 8`.

### 2 coloane egale

| Zonă | Dreptunghi vizibil (L,T,R,B) |
|---|---|
| Stânga | 4, 4, 956, 1036 |
| Dreapta | 964, 4, 1916, 1036 |

### 2 coloane 70/30

| Zonă | Dreptunghi vizibil |
|---|---|
| Stânga mare | 4, 4, 1340, 1036 |
| Dreapta | 1348, 4, 1916, 1036 |

### 3 coloane egale

| Zonă | Dreptunghi vizibil |
|---|---|
| Stânga | 4, 4, 636, 1036 |
| Centru | 644, 4, 1276, 1036 |
| Dreapta | 1284, 4, 1916, 1036 |

### 4 cadrane

| Zonă | Dreptunghi vizibil |
|---|---|
| Stânga sus | 4, 4, 956, 516 |
| Dreapta sus | 964, 4, 1916, 516 |
| Stânga jos | 4, 524, 956, 1036 |
| Dreapta jos | 964, 524, 1916, 1036 |

Spațiul de 8 px dintre ferestre și marginea de 4 px față de ecran sunt vizibile în captură.
Compensarea marginilor invizibile DWM a fost verificată prin `DWMWA_EXTENDED_FRAME_BOUNDS`:
dreptunghiul vizibil al ferestrei mutat este exact cel calculat, deși `GetWindowRect` raportează
o fereastră cu ~7 px mai mare pe fiecare latură.

Captură cu patru ferestre reale ancorate: `dist` nu păstrează imaginile de test; testul a fost
verificat vizual în timpul dezvoltării.

## 5. Snap Assist vizual

- După ancorarea primei ferestre, selectorul a apărut automat **în zona rămasă**, nu ca un panou
  centrat.
- Ferestrele candidate au apărut ca **previzualizări plutitoare separate**, peste desktopul blurat,
  fără titlu de dialog, footer sau listă.
- Previzualizările sunt live (`DwmRegisterThumbnail` + `DwmUpdateThumbnailProperties`), păstrează
  proporțiile ferestrei și sunt eliminate la închiderea selectorului (`DwmUnregisterThumbnail`).
- Selectarea unei previzualizări a completat următoarea zonă liberă.
- Jurnal:

```
[INFO] Snap Assist: fereastră aleasă hwnd=... title="...".
[INFO] Fereastră ancorată: hwnd=..., layout=2 coloane egale, zona=Dreapta,
       rect=964,4,952,1032
```

- Dacă previzualizarea DWM nu poate fi creată, se afișează iconița mare a aplicației.
- `Esc` și click-ul în afara previzualizărilor închid sesiunea fără a muta alte ferestre.
- Snap Assist poate fi dezactivat din setări.

## 6. Setări

Verificat vizual și funcțional:

- comutatoarele pentru activare, pornire automată și Snap Assist;
- combo-uri dark pentru întârziere și spațiere (deschidere, selecție, salvare);
- șase machete cu previzualizare, fiecare activabilă/dezactivabilă;
- salvarea a scris `hoverdelayms=400` în `%APPDATA%\SnapLayouts10\settings.ini`;
- după salvare, meniul a folosit noile valori fără repornire;
- fișierul este păstrat la `File.Replace` atomic.

## 7. Pornire automată

- Activare din setări → a fost creată valoarea:

```
HKCU\Software\Microsoft\Windows\CurrentVersion\Run
SnapLayouts10 = "C:\...\SnapLayouts10.exe"
```

- Dezactivare → valoarea a fost ștearsă.
- Se folosește numai `HKCU`, fără drepturi de administrator.

## 8. Monitoare multiple

Pe mașina de test există un singur monitor. Logica a fost testată prin simularea unei arii de lucru
secundare:

- arie: `(1920, 0, 2560, 1400)`
- layout 4 cadrane, zona Dreapta jos → `(3204, 704, 1272, 692)`

Coordonatele sunt absolute, deci funcționează și cu monitoare aflate la stânga/sus (coordonate
negative). Meniul folosește monitorul de sub cursor, iar ferestrele folosesc monitorul țintă.
Testarea pe hardware cu două monitoare nu a fost posibilă pe această configurație.

## 9. Stabilitate și resurse

- procesul a rulat în System Tray fără fereastră principală;
- memorie de lucru observată: ~27 MB;
- fără hook global de mouse; polling la 40 ms doar pentru poziția cursorului;
- nicio eroare în jurnal în timpul testelor;
- instanță unică: a doua lansare afișează un mesaj și nu creează al doilea proces funcțional.

## 10. Corectare 1.0.1 — fereastră maximizată (Restore Down)

Test suplimentar:

- fereastră Notepad maximizată (`IsZoomed = true`);
- cursor pe butonul Restore Down → `WM_NCHITTEST = 9`;
- meniul a apărut la poziția `1496, 29`, dimensiune `416 × 198`;
- click pe zona stângă a layout-ului 2 coloane;
- fereastra a fost restaurată și ancorată la dreptunghiul vizibil `4, 4, 956, 1036`.

În versiunea 1.0.0 ferestrele maximizate erau excluse intenționat; în 1.0.1 au fost incluse.

## 11. Redesign vizual 1.3.0

Analiza de design este documentată în [`ANALIZA-WINDOWS11.md`](ANALIZA-WINDOWS11.md), cu sursele
Microsoft folosite.

Flyout:

- dimensiune `392 × 382` la 96 dpi, șase machete în 2 × 3;
- machete fără carduri, zone mari, contur subțire;
- rotiță de setări în rail-ul din stânga; click pe rotiță a deschis fereastra de setări;
- fundal acrilic blurat, capturat din ecranul din spate, cu tentă dark;
- hover pe zonă cu accent.

Snap Assist:

- nu mai există panou, titlu, footer sau grilă de carduri;
- previzualizările sunt forme independente, poziționate în zona liberă;
- fundalul este wallpaper-ul monitorului, blurat, cu tentă;
- umbre desenate sub previzualizări;
- fereastra țintă rămâne vizibilă în cealaltă zonă.

Test funcțional:

- fereastră țintă ancorată în jumătatea stângă;
- previzualizări afișate în dreapta, fără suprapunerea ferestrei țintă;
- click pe o previzualizare a ancorat fereastra aleasă în dreapta (`964, 4, 956, 1036`);
- după închidere, toate ferestrele-previzualizare au fost eliminate, fără artefacte.

Teste anterioare păstrate: 2 coloane, 70/30, 3 coloane, 4 cadrane, Snap Assist pe zonă îngustă.

## 12. Limitări rămase

- ferestre cu titlu desenat complet custom, care nu răspund cu `HTMAXBUTTON`, pot necesita adaptare;
- aplicațiile elevate pot refuza `SetWindowPos` dintr-un proces normal (UIPI);
- testarea multi-monitor reală rămâne de făcut pe hardware cu două ecrane;
- executabilul nu este semnat digital.
