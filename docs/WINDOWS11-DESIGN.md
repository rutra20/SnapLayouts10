# Windows 11 visual analysis and SnapLayouts 10 design

Design documentation for version 1.4.0. The analysis started from Microsoft's Snap Layouts and
Snap Assist material and reference screenshots.

## Sources

- Microsoft Windows — *Snap your windows to stay organized*:
  https://www.microsoft.com/en-us/windows/tips/snap
- Microsoft Q&A — *How to: Use Snap Layouts and Snap groups in Windows 11* (includes Microsoft
  reference captures):
  https://learn.microsoft.com/en-us/answers/questions/2337358/how-to-use-snap-layouts-and-snap-groups-in-windows
- Microsoft Support — *Snap your windows*:
  https://support.microsoft.com/windows/snap-your-windows-885a9b1e-a983-a3b1-16cd-c531795e6241

## What Windows 11 does visually

### Snap Layouts

- The panel is a **compact flyout with rounded corners, a soft shadow, and an acrylic effect**;
  it is not a dialog window.
- Zones are **large rounded gray rectangles with a thin outline**. There are no individual cards
  around each layout.
- Layouts have generous spacing between zones and between variants.
- Normal displays show four layouts; ultrawide displays can show six.
- A **settings gear** sits on the left side.
- On hover, the targeted zone picks up the system accent color.
- The animation is short: fade plus a small movement, with no heavy elements.

### Snap Assist

- After the first window is snapped, **no centered panel with a list appears**.
- Candidate windows appear as **floating previews in the free zone**, over the desktop background.
- Each preview shows the real window, preserves its aspect ratio, and has a shadow and rounded
  corners.
- There is no large title, row list, or "OK" button; the choice is made directly on the preview.
- The background behind the previews is clean desktop, not an opaque dialog surface.

## What SnapLayouts 10 1.3.0/1.4.0 implements

### Flyout

- Removed dark cards around the layouts.
- Zones are drawn directly on the panel, larger, with rounded corners and a thin outline.
- Airier 2 × 3 flyout with larger spacing.
- Added a **settings gear** in the left rail; it opens the app settings.
- Background uses a **blurred screen capture** and an acrylic-style tint, so the panel is not a flat
  surface.
- On hover, the zone uses the Windows accent color.

### Snap Assist

- Removed the title, footer, and card grid.
- Candidate windows are now **independent preview windows** positioned in the free zone, with
  shadows and a subtle outline.
- Behind them is a **blurred desktop wallpaper**, not a dialog surface.
- Previews use DWM (`DwmRegisterThumbnail`) and are live.
- Clicking a preview snaps the window into the zone; `Esc` or clicking outside closes the selector.
- For small zones, previews are scaled while preserving aspect ratio.

## Deliberate trade-offs

- Windows 11 shows four layouts on 16:9; the project requirement is six, so the app uses a 2 × 3
  grid.
- Previews are separate WinForms windows, not a shell-composited surface. The result is close, but
  shadows and corners are not pixel-identical to DWM on Windows 11.
- The blurred background is generated locally from the user's wallpaper; the Mica API is not
  available on Windows 10.
- Windows 10 does not provide native rounded window corners or Mica, so these are approximated.
