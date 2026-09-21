# SHIFT visual conversion — Step 3

Board, cells, pieces and special-tile presentation only. Step 2 layout/HUD/background, level design, authoritative rules, progression and input-region geometry are preserved.

## A) Files created

Paths relative to the Unity project root:

- `Assets/_Game/Scripts/Presentation/BoardVisualResources.cs`: board-owned, reusable procedural surface sprites; explicit texture/sprite cleanup in runtime and Edit Mode.
- `Assets/_Game/Scripts/Presentation/BoardIcon.cs`: UI-mesh arrows, single clockwise arc, channel diamonds and crate braces.
- `Assets/_Game/Scripts/Presentation/PieceAppearance.cs`: shared view composition and gate/direction display updates.
- `Assets/_Game/Tests/Editor/BoardVisualSystemTests.cs`: three new tests for frozen game data, resource reuse/lifetime, and integrated visual/input/action compatibility.
- `Assets/_Game/Tests/Editor/BoardVisualFrozenFiles.txt`: 52 baseline file hashes used by the permanent regression test.
- Corresponding Unity `.meta` files; validation baseline, report, test logs/results and portrait captures.

## B) Files modified

- `Assets/_Game/Scripts/Board/BoardView.cs`: generated frame/cell surfaces, shared resource owner, enlarged channel badges, appearance resource injection. Action playback and timings unchanged.
- `Assets/_Game/Scripts/Pieces/Piece.cs`: delegates visual construction to PieceAppearance; updates geometric arrows and gate geometry from the existing action playback. Existing movement, pulse, exit, reduced-motion and input callbacks remain.

No existing test was weakened or changed. BoardManager, all 40 levels, core enums/models, progression/save, GameFeelSettings, PrototypeGame, GameHud and SafeArea remain unchanged. Independent baseline comparison covers 96 protected files.

## C) Board frame

Rounded navy outer rim uses a reusable nine-sliced shaded surface, a lower shadow and darker inner lip. Outer frame extends 18 reference units beyond the existing board rect; inner lip extends 5. Existing board AspectRatioFitter, room/corridor topology and cell coordinates remain. Dark blocked regions receive no decorative cells.

## D) Cells

Shared slate surface with shallow bevel, upper-left highlight and darker lower/right edge. Existing four-unit cell insets and grid dimensions are unchanged. Sculpted wall cells remain omitted. Static wall visuals elsewhere are quiet dark raised surfaces, distinct from the warm pushable block.

## E) Colored pieces

Disc surfaces use a dark outer rim, colored edge, shaded center and small upper-left highlight, retaining contact shadows. Red/blue/yellow/green identities remain. Every movable Normal piece has a centered thick navy arrow with a subtle relief layer. Body anchors remain (.12,.12)–(.88,.88); existing Button, callback, input lock and -8 raycast padding are unchanged. Decorative layers do not receive raycasts.

## F) Direction and Rotator

Direction uses a beveled slate-blue tile and bold white geometric arrow, distinct from the navy disc arrow. Arrow changes follow BoardAction playback, not guessed final-state reads. Rotator uses a large single clockwise arc and arrowhead on purple; no generic refresh or bidirectional icon. Glyph rendering is independent of the font.

## G) PushBlock and Exit

PushBlock uses a warm beveled crate-like face, recessed center, diagonal braces and small corner rivets. It has no direction arrow and remains the same mechanic. Exit retains target color and explicit EXIT text, with a nested bright rim, dark well and threshold highlight. Existing subtle idle glow, stronger delivery pulse, exit animation timing and reduced-motion behavior remain.

## H) Switch and Gate

Switch has a dark recessed base, raised gold pad and large one-/two-diamond geometric symbol. The occupied-switch badge is widened to 54% of the cell and consistently aligned at its upper-left edge. Both badge and icon are raycast-transparent.

Closed Gate shows vertical slats and a crossbar between side posts. Open Gate removes the central barrier and exposes a clear dark passage with a restrained threshold indicator. The posts and channel header remain, so pairing is readable in either state. Open/closed differs by geometry, not merely color. ShowGate changes only the view; tests verify model state remains untouched and displayed state matches the model after each completed reaction.

## I) Remaining visual assets

No external/custom artwork is required for this implementation. Original runtime-generated 128×128 sliced surfaces and 192×192 disc surfaces are shared within each board. UI icons are vector-like meshes using the existing UI material. No custom shader, imported font, internet artwork, new mechanics, Undo or Hint.

Optional later art work, not started: a custom wordmark, illustrated environment and artist-authored anti-aliased material/icon atlas if closer matching to reference A is desired. The current HUD target icon intentionally retains Step 2's visual treatment.

## J) Testing

**Final full suite: 147/147 passed, 0 failed, 0 skipped.** Final run has no C# compiler errors/warnings or NullReferenceException. The resource-lifetime regression passes after the cleanup fix.

The full suite includes all previous 144 tests plus 3 new cases. The new integrated test checks Level10/32/40 topology, unchanged hit-region anchors/padding, actual EventSystem raycasts through decorative layers, arrow playback without model mutation, closed/open gate shape, nonintercepting channel badges, full finale solution, and Perfect Shift. Existing reduced-motion, rollback, save, progression and optimality tests remain.

The first completed run found an Edit Mode cleanup issue in the new resource owner (146/147 passed). ExecuteAlways enables its lifecycle cleanup in Edit Mode as well as runtime. Final results are recorded in `board3-final-results.xml`; the final log is `board3-final-unity.log`.

## K) Screenshots

Actual Unity 1080×1920 captures under `Validation/board3/`:

- `board3-level10.png`: core board, crate and colored-piece arrows.
- `board3-level32.png`: paired gate states and Switch readability.
- `board3-level40.png`: complex sculpted topology, rotator, exit and pieces.
- `board3-gate-closed-and-open.png`: both gate silhouettes together on Level32.
- `board3-occupied-switch.png`: channel badge above the box on Level40's pad.
- `board3-perfect-shift.png`: completed finale, open gates, MEGA SHIFT x8 and Perfect Shift.

The three requested boards and the finale feedback capture were visually reviewed. Open/closed states coexist in Level32, allowing a direct comparison without altering level data.

## L) Known limitations

This is a procedural approximation of reference A's material family, not a pixel-identical reproduction. Thin bevel edges and geometric icon edges may need artist-authored antialiasing for a closer final finish. No physical-device GPU/memory profiling, native build or broad display-density study was performed. Surfaces are generated at board construction and released with their owner, not generated per frame; per-board rebuild CPU cost remains a device-profiling item. No extra reaction delays were introduced. The next visual step has not started.
