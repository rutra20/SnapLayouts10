# Contribuții

Mulțumesc pentru interes. Proiectul este mic și își propune să rămână ușor, fără dependențe externe.

## Cerințe

- Windows 10 22H2 x64 (sau Windows 11 pentru testare secundară)
- .NET Framework 4.8 (inclus în Windows 10)
- PowerShell 5.1
- Opțional: Git și un editor C#

## Build

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

Teste geometrice:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\test-geometry.ps1
```

Pachete de release:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\package.ps1
```

## Reguli de cod

- Codul trebuie să compileze cu **C# 5** și `csc.exe` din .NET Framework 4.8; nu folosiți
  funcționalități C# 6+ (string interpolation, `?.`, `nameof`, tuples, local functions etc.).
- Nu adăugați pachete NuGet, DLL-uri terțe sau componente care necesită instalare.
- P/Invoke-urile se adaugă în `src/Native.cs`, cu comentarii pentru constante.
- Orice apel către alt proces trebuie să aibă timeout sau fallback; aplicația nu trebuie să
  blocheze firul UI.
- Păstrați compatibilitatea cu ferestre restaurate și maximizate.
- Nu includeți date personale, capturi cu conținut sensibil sau jurnale reale în commit-uri.

## Testare manuală minimă

Înainte de PR:

1. Notepad restaurat: hover pe Maximize → flyout → alege o zonă.
2. Notepad maximizat: hover pe Restore Down → flyout → alege o zonă.
3. Un layout cu 3 și unul cu 4 zone.
4. Snap Assist: alege o previzualizare; `Esc` și click în afara selectorului.
5. Setări: schimbă întârzierea, spațierea, un layout; salvează; verifică flyout-ul.
6. Tray: activare/dezactivare și ieșire.

## Commit-uri și PR-uri

- Mesaje scurte, la timp prezent: `fix: ...`, `feat: ...`, `docs: ...`.
- Un PR = o schimbare logică.
- Completați template-ul de PR și spuneți ce aplicații ați testat.
- Pentru schimbări vizuale, atașați capturi înainte/după (fără date personale).

## Licență

Prin contribuție acceptați ca modificările să fie distribuite sub licența MIT a proiectului.
