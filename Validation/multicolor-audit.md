# Multi-color objective foundation

Authorized extension: legacy `targetColor` remains serialized; empty/null `targetColors` resolves to it. Explicit lists contain one to three distinct valid colors, with at least one normal piece of each color. Playable validation requires an accepting exit for each color. `Configure` resets explicit objectives; callers cloning multi-color data must also call `ConfigureTargetColors`.

BoardManager has exactly three substitutions: objective field, objective snapshot at Load, and target membership in CompleteResolution. RequestMove, Move, exit filters, rollback, cost, event sequencing and reaction limits are text-identical to the baseline under `batch36-baseline/Scripts/Board/BoardManager.cs`. Once no configured target remains, the existing win-before-loss rule applies. Load copies the objective so subsequent authoring changes cannot affect a running board.

Audit of all C# targetColor/TargetColor/win references:

- LevelData: resolved objective and validation updated; no migration of campaign assets 1–35.
- GameHud: single-color branch retains existing geometry, art, size and English text. Multiple colors use two/three compact discs and Clear Both/Clear All. ObjectiveText has explicit EN/TR formatting, without adding a language-setting feature or redesigning the HUD.
- VerifiedOptimality: old fingerprints stay identical. A tagged, canonical objective suffix before placements distinguishes changed multi-color semantics; certificates cannot be reused just by retaining a legacy primary color.
- AnalyticsService: old targetColor retains its existing meaning; additive targetColors string records the actual objective. Attempt completion, counters, event names, local-only provider and save semantics remain unchanged.
- PrototypeGame, Progression, SaveService: consume authoritative GameState.Won and level indexes, not color counts. No changes required.
- PuzzleBatchAnalysis, DesignValidation, LevelValidation: replay BoardManager and use its terminal state; no alternate single-color completion predicate. No algorithm changes required.
- Existing Configure-based diagnostic clones concern legacy single-color levels. New multi-color diagnostics explicitly copy the resolved objective. Historical authoring scripts remain historical; the new scoped harness passes targets explicitly.
- Old frozen hashes are advanced only for the five authorized runtime files. A baseline records all prior files, and BoardManager's three substitutions are separately compared. No old movement, push, rotator or gate assertion is removed.

Foundation test evidence: `multicolor-results.xml` and `multicolor-unity.log`. Foundation full suite: **206/206 passed**, zero failed/skipped, zero C# compiler errors/warnings.
