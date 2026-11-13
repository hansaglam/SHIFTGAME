# SHIFT — Chapter 6 replacement mechanism: Tek Geçiş / One-Use Crossing

**Status:** DESIGN-ONLY, not coded or playable.  
**Date:** 13 November 2026  
**Branch:** `feature/chapter6-one-use-bridge-pilot-v01` (created from `feature/chapter5-level81-100-design-v01`)  
**Decision:** BAĞ/Tether PR #4 was closed without merge after user rejected its visual execution. This clean branch intentionally contains **zero BAĞ-specific game code, level assets, save migrations, new HUD artwork or mechanics**.

## Product goal

101+ needs one genuinely new *decision*, not more moves or another decorative effect. The mechanic must be understandable in one glance, suit SHIFT's compact navy board / cyan-gold visual language, keep LOOK → PREDICT → TAP → CHAIN REACTION, and lead to multiple distinct solution strategies.

**Candidate: a single-use crossing tile.** It looks like one ordinary board tile with a subtle interrupted bridge outline, a tiny gold "1" only if needed and a visible center seam. No full-board line, extra gesture, large overlay, pop-up economy or second input mode.

### Proposed deterministic behavior — test before implementation

- Terrain state starts **intact**. Pieces may enter normally. Once a **Normal** piece **successfully leaves** the tile, that tile becomes a closed/used crossing: **no subsequent Normal may enter**, even through a push or Resonance chain.
- A failed attempt to leave does **not** consume the crossing. A Normal resting there may still leave it successfully; collapse applies only **after** that move, so no trapping-on-entry surprise.
- First prototype excludes PushBlocks as consumers, to limit ambiguity. If a PushBlock can occupy/cross the tile, its interaction must be specified before any puzzle is authored; do not silently treat blocks differently in the engine.
- A crossing can remain intact for several actions until the first successful departure. One used crossing cannot be restored within that attempt; Undo restores its prior state, Restart resets it.
- Collapse is part of a **single tap's deterministic sequence**, with source-model action `CrossingSpent` and short tactile change (small tile cracks / muted color), not a global shake or obstructive flash.
- Existing gates, rotators, switches, Resonance, exits, multi-piece pushes and save state must retain their behavior. New tile logic activates only when explicitly present in a Chapter 6 experimental level.
- State must be included in BoardManager's authoritative snapshots, solution BFS hashing and future optimality fingerprints; old 1–100 fingerprints must remain bitwise unchanged.
- UI must show **intact vs used** as two obvious static states under Reduced Motion. A tile must remain recognizable on small portrait screens and when a stone stands on top of it.

### 101–104 learning micro-pilots (concepts only)

1. **101 — Son Geçiş / Last Crossing.** One crossing, two Normal pieces; first obvious choice makes the second route inaccessible. Teach that *leaving*, not entering, spends the crossing.
2. **102 — Önce Kim? / Who First?** Two colored objectives share a shortcut. The same intact tile should favor one route order. Contrast shorter optimum with a genuine longer win, not padded taps.
3. **103 — Zincir Kararı / Chain Commitment.** A push or Resonance chain crosses the tile unexpectedly but explainably; the optimal solution makes the remote movement productive **before** the crossing closes.
4. **104 — Açılan Yol / New Route.** A switch/gate plus one-use crossing. Players must open an alternative route first, then intentionally spend the direct route. A satisfying multi-piece tap after commitment.

**These are hypotheses, not yet constructed levels or proofs.** Reject any level whose only difficulty is 15+ routine taps or hiding a rule.

## Test and aesthetic gate before expanding to 105–120

- Make a **single small isolated mechanics pilot** outside `main`, not a full chapter. First verify Unity EditMode compile + deterministic replay + BFS shortest and at least one valid alternate for each new board.
- Tests must cover: no spend on blocked move; spend on successful departure; second entry blocked; same-tap multi-piece ordering; Undo/Restart; replay; remote/push interactions; old level fingerprints untouched; no changes to old scenes or saves.
- Visual acceptance from a **real 1080×1920 Game view screenshot**: players can distinguish two tile states with no full-width lines, and understand why a tile closes without reading a long paragraph.
- If first prototype does not create a genuinely new route-order dilemma or reads too similarly to a Gate/Switch, **discard it** instead of iterating another 20 levels.
- After acceptance: design 105–120 around introduction → exploration → surprising combos → Chapter 6 finale, then atmosphere/world change. Mora and end-stage holistic real-player testing remain later in the user-approved sequence; mechanical engine tests happen during development.

## Repo isolation

PR #4 (`feature/chapter6-tether-prototype-v01`) is **closed, unmerged** and preserved as a rejected research sample; *do not merge it into this branch*. All 1–100 assets, shipped scene, saves, ads and Mora stay unchanged. The new Chapter 6 branch begins on the Chapter5 81–100 draft baseline and currently contains **only this design note**. None of its proposed feature behavior is implemented.
