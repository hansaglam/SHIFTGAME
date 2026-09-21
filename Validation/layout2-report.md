# SHIFT visual conversion — Step 2

Completed layout, top HUD, Settings relocation, instruction card and temporary background only. Final full Unity 6000.3.23f1 suite: **144/144 passed, 0 failed, 0 skipped**. No C# compiler errors/warnings or null-reference exceptions in the final run.

## A) Files modified

Relative to the Unity project root:

- `Assets/_Game/Scripts/UI/GameHud.cs`: typography, HUD, instruction/result card, existing action layout.
- `Assets/_Game/Scripts/Systems/PrototypeGame.cs`: board presentation anchors, HUD circle argument, Settings construction, compensating hit padding.
- `Assets/_Game/Scripts/UI/Atmosphere.cs`: blue/teal gradient and required CanvasRenderer.
- `Assets/_Game/Scripts/UI/VisualTheme.cs`: reduced atmospheric-light opacity.
- `Assets/_Game/Tests/Editor/PrototypeSceneTests.cs`: move value assertions reflect the separate number/label.
- `Assets/_Game/Tests/Editor/SprintPresentationTests.cs`: same presentation-only assertion adjustment.

Created `Scripts/UI/LayoutControls.cs` and `Tests/Editor/LayoutStepTwoTests.cs`, with Unity meta files. No imported or custom art assets, new fonts, shaders, packages or game features.

## B) Exact layout changes

All gameplay coordinates below are normalized to the existing Safe Area, measured from bottom-left. The 1080×1920 reference resolution remains.

| Region | X range | Y range |
|---|---|---|
| SHIFT live title | .16–.84 | .906–.990 |
| Live level subtitle | .10–.90 | .875–.901 |
| Goal/Moves HUD | .13–.87 | .805–.868 |
| Chain feedback | .18–.82 | .774–.799 |
| Board fitting region | .045–.955 | .257–.770 |
| Instruction/result card | .12–.88 | .155–.244 |
| Restart during play | .32–.68 | .077–.133 |
| Levels | .36–.64 | .020–.068 |

The board's fitting region is 91% of safe-area width; AspectRatioFitter still preserves the actual board's aspect ratio. The existing frame extends slightly beyond its fitting rect. Title uses the current font at up to 138, bold, near-white with a restrained lower-right shadow. Subtitle is 28; existing finale emphasis is warm/light. Gameplay tap callbacks and locks remain unchanged. An 8-unit outward raycast padding compensates for the small reduction in board presentation size without changing the piece rendering or button transform.

## C) HUD changes

Dark navy rounded panel replaces the large white panel. Left: 78×78 circular target indicator made from the existing circle resource, color, raised face and highlight treatment; explicit live `Clear Red` text at 36 bold. Divider separates the right group: `Moves:` at 32 and the live move number at 58 bold. Gold number, peach at the existing low-move threshold. Only presentation changed; values still come directly from BoardManager. No hardcoded move count or goal logic.

## D) Settings relocation

Top-right under SafeArea, 20 reference units inset from top/right. Invisible 112×112 hit surface, approximately 94×94 circular visible face. Temporary gear assembled from existing UI circles/rectangles. Existing press tint and PresentationMotion feedback retained, including Reduced Motion behavior. Opens the existing Settings panel with its existing preferences and callbacks. Bottom Settings button removed. Modal panels still cover gameplay and preserve their existing input behavior.

## E) Bottom controls

Restart becomes a compact centered 36%-width control, 5.6% safe-area height. Levels is smaller/secondary and has a 2% bottom margin. On completion Replay occupies x=.12–.38 and Next Level x=.43–.88, sharing the same y=.077–.133 row; Next retains the stronger teal fill. Finale without Next remains centered. Sentence-case labels reduce the prior all-caps density. No Undo or Hint added.

## F) Instruction card

Warm-neutral opaque surface (242,232,211), soft shadow, centered text, 6.5% horizontal and 12% vertical inner margins. Text uses 30 with a 26 lower best-fit bound instead of dropping to 12. Existing strings remain unchanged. Level40 fits two lines in the reference capture and three in the inset-safe-area check. Existing success/failure/mastery colors and nonblocking behavior remain. This is wrapping-friendly, not a new localization system or a guarantee for arbitrary-length translations.

## G) Background

Existing procedural mesh now renders a gradient from blue (57,109,164) to light teal (159,208,209), with low-opacity existing atmospheric shapes. Fixed a pre-existing missing CanvasRenderer on the custom Atmosphere graphic; without it the gradient geometry was not visible. Added a regression check for the renderer and its four-vertex mesh. No texture import, illustration, custom material or shader. Board and piece surface code remains untouched.

## H) Tests and preservation

Final results: `layout2-final-results.xml`; log: `layout2-final-unity.log`. **144 passed / 0 failed / 0 skipped**. Includes all prior 143 cases plus one new presentation integration test covering separated move label/value, live goal/instruction, 91% region, top-right Settings hit area, compensated piece hit padding, settings toggle, modal access, gradient renderer and screenshot capture.

Existing tests retain solutions, optimality, all 40 ordered references, progression/save/migration, gate rollback, mastery and reduced-motion checks. Initial layout run also passed 144/144. A subsequent test-only CanvasRenderer API mismatch was corrected before the successful final run.

Baseline hash comparison confirms all **90 protected files unchanged**, including all level assets/meta, BoardManager, BoardView, piece code, SaveService, LevelProgression, SettingsService and SafeArea. Final tested C# and asset sources match the delivered files. No scene or level authoring changes.

## I) Screenshots produced

Actual Unity 1080×1920 renders retained under `Validation/layout2/`:

- `layout2-level1.png`: normal teaching-level gameplay.
- `layout2-level32.png`: two-room topology and HUD.
- `layout2-level40.png`: advanced gameplay and long instruction.
- `layout2-inset-safe-area.png`: simulated safe-area insets and three-line instruction.
- `layout2-settings.png`: existing settings panel reached through new button.
- `layout2-chapters.png`: existing chapter-selection spacing.
- `sprint5-perfect-shift.png`: success, Replay/Next hierarchy and mastery.
- `chapter2-complete.png`: finale result and centered Replay.

Reviewed the final gameplay, safe-area, Settings and success captures. Existing chapter/settings screens needed no internal repositioning because they already occupy safe-area modal roots.

## J) Known limitations

Temporary live-text wordmark, procedural gear and existing font/sprites are intentional. Board, pieces and special-tile art are not redesigned. Safe-area geometry was exercised with simulated insets; physical devices and all aspect ratios were not tested. Target width is subject to board aspect fitting on shorter screens. The current two-group English HUD and long translations still warrant localized-device review. No native builds performed. No next visual/art step started.
