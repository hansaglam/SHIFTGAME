# SHIFT — MORA Companion Hub v1 (opt-in visual/navigation vertical slice)
Date: 2026-11-07. Branch: feature/mora-visual-pilot-v01. Not merged into main.

## Player direction
Replace the old art-preview MORA screen with a delightful, Pou-like companion room using the original SHIFT aesthetic, rather than copy another game's UI. Prepare for multiple characters and gradual progression. Keep puzzle chain reactions as the product core. Then prioritize real Level 41+ content.

## Implemented
- New full-screen companion hub with SHIFT header, a MORA selected slot, two future locked slots, and an add/collection entry.
- Existing approved four grounded composite art stages as the character focal point, with a stable base, cross-fade, subtle idle/tap response and Reduced Motion.
- Functional menu routing: Feed, Play, Decor, Growth and Collection. Back to MORA and Back to Chapters actually navigate.
- Feed and Play are tiny **session-only** toy interactions. No energy, XP, currency, hunger, rewards, timers, paid unlocks or player-save mutation; labelled as previews.
- Decor explains current habitat and future decoration. Collection shows MORA plus upcoming companion slots, not fake earned characters.
- Growth shows four candidate visual stages, with Editor-only art-selection. The production user cannot skip/unlock stages via the UI; Level 41+ is required before real progression can begin.
- Turkish and English menu copy updates on language change using LocalizedLabel. Existing safe-area hierarchy and motion accessibility remain.
- Uses native Unity buttons and dynamic text, NOT a screenshot containing baked-in controls.

## Asset dependency / limitations
The accepted Stage 1–4 PNG assets are user-imported, low-resolution masked composite art from the source concept illustration. To show the accepted look, the project needs `Assets/_Game/Resources/Mora/MoraScene_S1.png` through S4. If missing, the hub uses the existing `MoraSeed_Idle.png` if available; without either it is not installed. A composite cannot have separate eyes, leaves or waterfall animated; real layered assets remain outstanding. This is a styled functional UI prototype and requires Unity screenshots and a device pass before claiming production quality.

## Smoke test checklist
- Unity closed, inspect local git status then pull `feature/mora-visual-pilot-v01`, preserve .unity scene and local PNGs. Run a backup/commit if necessary, do not reset.
- Prototype > Enable Mora Pilot ON > Chapters > MORA.
- Home: check portrait framing on 1080×1920 and tall/narrow phones; no overlaps with scene art, rail, progress ribbon or navigation; no blank expression.
- Tap MORA rapidly: stays visible. Visit Feed > Give Berry and Play > Toss Ball, messages change, no saves/counters persisted.
- Decor, Growth, Collection views and two locked tabs; close each and return to home. Editor growth cards preview actual four sprites. Build must not unlock forms.
- Toggle Reduced Motion, test TR+EN, close/reenter, return to Chapters, solve/restart a level, verify Perfect and ending remain unchanged.
- Run MoraCompanionHubTests, MoraCharacterDirectionTests, MoraHomePanelTests and regression suite; record red Console errors, mobile memory/profile before merge.

## Future work
1. Art polish: clean layered sprites, small room background accents/icons, font/localization alignment, safe-area/device QA.
2. Actual companion progression with 41+ levels, versioned save migration and once-only verified rewards; special teal character is a *candidate* future collectible, not currently granted.
3. 41–50 certified puzzle boards and 51–60 continuation; no filler levels.
No Unity compile, Editor Test Runner or device test has been run in the connector.
