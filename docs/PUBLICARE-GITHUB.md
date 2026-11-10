# Publicare pe GitHub

Repository publicat: <https://github.com/rutra20/SnapLayouts10>

Ghid practic pentru publicarea proiectului. Pe mașina de dezvoltare Git nu era instalat; prima
secțiune acoperă instalarea.

## 1. Pregătire locală

Instalați Git for Windows:

```powershell
winget install --id Git.Git -e --source winget
```

Deschideți un terminal nou și verificați:

```powershell
git --version
```

Apoi, în folderul proiectului:

```powershell
cd C:\path\to\SnapLayouts10
git init
git add .
git status
```

Verificați că **nu** apar în listă:

- `dist/` (binarele se pun în Releases);
- `*.log`, `settings.ini`;
- capturi de test sau date personale.

Primul commit:

```powershell
git commit -m "feat: SnapLayouts 10 1.3.0"
git branch -M main
```

## 2. Creați repository-ul pe GitHub

În GitHub: **New repository**.

Recomandări:

- Nume: `SnapLayouts10` sau `snap-layouts-10`
- Descriere: `Unofficial Snap Layouts for Windows 10 — hover the Maximize button and choose a zone. Lightweight WinForms utility.`
- Public
- Fără README, fără .gitignore, fără licență (există deja în proiect)
- License: MIT (după primul push, GitHub ar trebui să detecteze `LICENSE`)

Topics sugerate:

`windows-10`, `windows`, `snap-layouts`, `snap-assist`, `winforms`, `csharp`, `desktop`, `productivity`, `dotnet-framework`

## 3. Conectați și urcați codul

```powershell
git remote add origin https://github.com/UTILIZATOR/SnapLayouts10.git
git push -u origin main
```

Înlocuiți `UTILIZATOR` cu numele contului.

Dacă preferați fără Git: puteți folosi **GitHub Desktop** sau puteți încărca arhiva prin interfața
web, dar folderul ascuns `.github` trebuie inclus; Git rămâne varianta recomandată.

## 4. Prima versiune / primul release

Workflow-ul `.github/workflows/release.yml` creează automat release-ul când apare un tag `v*`.

Asigurați-vă că versiunea din `src/AssemblyInfo.cs` este `1.3.0.0`, apoi:

```powershell
git tag -a v1.3.0 -m "SnapLayouts 10 1.3.0"
git push origin v1.3.0
```

GitHub Actions va:

1. compila;
2. rula testele geometrice;
3. crea pachetele portable și sursă;
4. genera `SHA256SUMS.txt`;
5. publica release-ul cu asset-urile.

Dacă lucrați manual:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\package.ps1
```

și încărcați din `dist/`:

- `SnapLayouts10.exe`
- `SnapLayouts10-1.3.0-portable.zip`
- `SnapLayouts10-1.3.0-source.zip`
- `SHA256SUMS.txt`

Nu urcați `dist/` în Git.

## 5. Setări recomandate în GitHub

- **About**: descriere + topics + link către Releases.
- **Releases**: publicați `v1.3.0`, marcat „Latest”.
- **Security → Private vulnerability reporting**: activați.
- **Issues**: păstrați șabloanele; dezactivați blank issues (deja configurat).
- **Discussions**: opțional, util pentru întrebări.
- **Social preview**: încărcați `docs/social-preview.png` (1280 × 640).
- **Wiki**: nu este necesar; documentația stă în `docs/`.

## 6. Înainte de a face repository-ul public

- [ ] `README.md` are disclaimer clar că proiectul este neoficial.
- [ ] Nu există logo-uri sau assets Microsoft.
- [ ] `LICENSE` este MIT.
- [ ] `dist/` și jurnalele nu sunt în Git.
- [ ] Capturile nu conțin date personale.
- [ ] Build-ul și testele geometrice trec local.
- [ ] Versiunea din `AssemblyInfo.cs` corespunde tag-ului.
- [ ] Release-ul conține SHA-256.
- [ ] Ați citit `NOTICE.md`.

## 7. SmartScreen și semnare

Executabilul este nesemnat. La prima descărcare, SmartScreen poate afișa „Windows protected your PC”.
Soluții:

- publicați codul și hash-ul, ca utilizatorii să poată verifica;
- semnați executabilul cu un certificat de code signing (cost anual);
- nu promiteți că antivirusul nu va reacționa — aplicația trimite mesaje Win32 către alte ferestre
  și poate genera false pozitive.

## 8. Identitate și mărci

- Nu folosiți „Microsoft”, logo-uri Microsoft sau sintagma „official” în nume/descriere.
- Formula recomandată: **„Unofficial Snap Layouts for Windows 10”**.
- Păstrați `NOTICE.md` în repository și în pachetul portabil.
