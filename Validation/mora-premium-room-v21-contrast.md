# MORA Premium Room v2.1 — Contrast and scale patch

Branch: feature/mora-visual-pilot-v01. Requires Unity smoke test before any merge.

## Cause
The FrameCream, FrameGlass, and FrameMuted PNGs had opaque black interiors.
Dark text on those backgrounds was almost invisible in the supplied screenshot.

## Fix
Replacement binary package: SHIFT_MORA_PremiumRoom_v2_1_ContrastFix.zip
Same Resources/MoraPremium paths; recolors only black frame interiors.
Gold borders, characters, islands, icons, and original gameplay assets untouched.
The Unity hub now uses ivory text over darker skins, deep-teal on cream/gold.
Growth step 02–04 numerals are light, text sizes raised slightly.
MORA and the island rectangles are enlarged but the art itself unchanged.

## Install and test
Close Unity, run git status and git pull --ff-only in D:\UnityProjects\SHIFT\SHIFT-MORA.
Do not hard reset local changes. Extract ZIP's Assets/_Game/Resources/MoraPremium
over that folder in the test project. Keep Resources/Mora unchanged.
Reimport MoraPremium in Unity if necessary. Prototype > Enable Mora Pilot > MORA.
Check 1080x1920 and tall portrait: speech, tile labels, inactive growth numbers,
upper title/character separation, island contact, Stage4 clipping.
Verify Feed, Play, Decor, Growth, Collection, TR/EN, Reduced Motion, rapid taps,
Back to Chapters, original puzzle flow and absence of red Console errors.
Unity compilation and on-device tests have not been run here.

No permanent growth, energy, purchases or unearned special characters granted.