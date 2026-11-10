# Analiza vizuală Windows 11 și aplicarea în SnapLayouts 10

Documentație de design pentru versiunea 1.3.0. Analiza a pornit de la materialele și capturile
Microsoft pentru Snap Layouts și Snap Assist.

## Surse consultate

- Microsoft Windows — *Snap your windows to stay organized*:
  https://www.microsoft.com/en-us/windows/tips/snap
- Microsoft Q&A — *How to: Use Snap Layouts and Snap groups in Windows 11* (include capturile
  Microsoft pentru Snap Layouts și Snap Assist):
  https://learn.microsoft.com/en-us/answers/questions/2337358/how-to-use-snap-layouts-and-snap-groups-in-windows
- Microsoft Support — *Snap your windows*:
  https://support.microsoft.com/windows/snap-your-windows-885a9b1e-a983-a3b1-16cd-c531795e6241

## Ce face Windows 11, vizual

### 1. Snap Layouts

- Panoul este un **flyout compact, cu colțuri rotunjite, umbră fină și efect acrilic**; nu este o
  fereastră de dialog.
- Zonele sunt desenate ca **dreptunghiuri mari, rotunjite, umplute cu gri**, cu un contur subțire.
  Nu există carduri individuale în jurul fiecărei machete.
- Machetele au spații generoase între zone și între variante.
- Pe ecrane normale Windows 11 arată patru variante; pe ecrane ultrawide pot apărea șase.
- În partea stângă există un **buton cu rotiță (Settings)**.
- La hover, zona vizată se colorează cu accentul sistemului.
- Animația este scurtă: fade + o mică deplasare, fără elemente grele.

### 2. Snap Assist

- După ancorarea primei ferestre, **nu apare un panou central cu listă**.
- Ferestrele candidate apar ca **previzualizări plutitoare, în zona rămasă liberă**, direct peste
  fundalul desktopului.
- Fiecare previzualizare arată fereastra reală, păstrează proporțiile și are umbră + colțuri
  rotunjite.
- Nu există titlu mare, listă de rânduri sau buton „OK”; alegerea se face direct pe previzualizare.
- Fundalul din spatele previzualizărilor este curat (desktopul), nu o suprafață de dialog opacă.

## Ce am schimbat în SnapLayouts 10 1.3.0

### Flyout

- Am eliminat cardurile întunecate din jurul machetelor.
- Zonele sunt desenate direct pe panou, mai mari, cu colțuri rotunjite și contur subțire.
- Flyout-ul este mai aerisit (2 coloane × 3 rânduri), cu spații mai mari.
- Am adăugat **butonul cu rotiță** în rail-ul din stânga; deschide setările aplicației.
- Fundalul folosește o **captură blurată a ecranului din spate**, peste care se aplică o tentă
  acrilică; astfel panoul nu mai este o suprafață plană, ci seamănă cu acrylic-ul Windows 11.
- La hover, zona se colorează cu accentul Windows.

### Snap Assist

- Am eliminat complet panoul cu titlu, footer și carduri.
- Ferestrele candidate sunt acum **ferestre-previzualizare independente**, poziționate în zona
  liberă, cu umbră și contur subtil.
- În spatele lor este afișat **desktopul blurat** (imaginea de fundal a monitorului), nu o suprafață
  de dialog.
- Previzualizările folosesc DWM (`DwmRegisterThumbnail`) și sunt live.
- Click pe o previzualizare ancoră fereastra în zonă; `Esc` sau click în afara previzualizărilor
  închide selectorul.
- Pentru zone mici, previzualizările sunt scalate și păstrează proporțiile ferestrelor.

## Compromisuri conștiente

- Windows 11 afișează patru layout-uri pe 16:9; cerința proiectului este de șase, deci folosim o
  grilă de 2 × 3.
- Previzualizările sunt ferestre WinForms separate, nu o suprafață compusă de shell. Rezultatul
  vizual este apropiat, dar umbrele și colțurile nu sunt identice pixel-cu-pixel cu DWM-ul Windows 11.
- Fundalul blurat este generat local din wallpaper-ul utilizatorului; nu se folosește API-ul Mica,
  care nu este disponibil pe Windows 10.
