# Virus XP — approved old-tech art specification

## Direction

The supplied moodboard establishes recognizable XP desktop software, the Bliss hill/sky composition, blue Luna windows, small shaded system icons, and a beige CRT. The approved main menu changes the physical world to a used early-2000s office workstation: yellowed plastic, worn laminate, old tower, speakers, cables, floppy disks, optical media and wired input devices. Physical wear remains subtle enough to preserve the CRT as the focal point. No cozy lifestyle props, pastel room styling, or clean modern hardware are used.

The software layer remains recognizable Windows XP: cobalt title bars, square controls, beveled buttons, warm off-white panels, familiar folder/system silhouettes and early messenger proportions. Physical dust is not painted over software controls, because the display UI should remain readable.

## Native grid and appearance

- Full scenes: 320×180 logical pixels; 640×360 and 1920×1080 previews are exact 2× and 6× nearest-neighbor exports. Use integer scaling or letterboxing at other sizes.
- Icons/cursors: 32×32 native; avatars: 64×64 plus 32×32 contact variants; virus: 48×48; skill icons: 48×48. Each art pixel is one logical pixel at native placement. Do not arbitrarily scale individual objects within a scene.
- Color: `palette_oldtech.hex` is the hardware/scene ramp; `palette.hex` is the XP software/icon ramp. The two share dark blue outlines and restrained highlights. Bright green is reserved for virus behavior, gold for folders/value, cobalt for XP system chrome, red for alerts.
- Light: upper-left. Outlines are one logical pixel in blue-gray or dark neutral. Highlights occupy clustered upper/left planes; lower-right shadows use darker nearby palette steps. Selective 1–2 pixel scuffs imply use without all-over dither.
- Icons use frontal or gentle three-quarter perspective and consistent padding. Faces and important symbols remain readable at native size.

## Unity import and layout

Import PNGs as Sprite (2D and UI), Filter Mode Point, Compression None, Mip Maps Off, Wrap Clamp. For SpriteRenderer, the manifest recommends 32 pixels per unit for a consistent world grid; UI should use native dimensions and integer canvas scaling. Use the per-file pivots/hotspots/9-slice borders in `UnityImportManifest.json`, `Cursors/hotspots.json` and `SharedUI/Unity9Slice.json`. Invisible cursor should normally use `Cursor.visible = false`.

The main menu is one opaque illustration; all its static props are integrated. `MainMenu/MenuLayout.json` supplies the screen position and blank button positions. The official Virus XP logo is a separate transparent PNG, so it can animate or localize independently. Do not flatten interactive UI into the background. Window frames, title bars, controls, chat bubbles, rows and sliders remain modular. Use Unity text for labels and all story dialogue. `Messenger/UI/dialogue_examples_ru.json` contains the supplied Russian strings; the target Unity project must include a TextMeshPro font/fallback with Cyrillic coverage.

## QA and limits

`ProductionQA.json` checks files, alpha, native scene dimensions and inventory. `Previews/` contains actual-size and enlarged review views. Runtime import, nine-slice behavior, final interaction states and Cyrillic font behavior still need checking inside the target Unity project, which was not supplied.
