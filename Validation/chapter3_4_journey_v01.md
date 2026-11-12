# SHIFT — Chapter 3–4 Purpose & Progression / Editor Prototype v01

**Date:** 2026-11-12
**Scope:** Level 41–80 only, design preview in Unity Editor.
**Status:** **NOT IN SHIPPING GAME**. No changes to Levels 1–40, MORA, runtime chapter selection, save format, Perfect Shift, ads or production build.

## Design decision

Players should not feel they complete disconnected grids. They should see a **quiet, persistent world restoration** across each 20-level chapter. The core action remains solving deterministic single-tap chain-reaction puzzles.

The alpine lake/mountain art direction stays intact; no visual reboot. We layer environmental **state changes** (muted/connected lines, illumination, distant beacons, gentle signal ripples) onto that world. Do not block input, force lore modal screens, or introduce a resource economy.

### Chapter 3 — Kopan Hatlar / Fractured Lines (41–60)

**Purpose:** Reconnect the broken flow beneath the valley. Earlier mechanics combine to restore a coherent route.

| At level | Visible environmental state | Design meaning |
|---|---|---|
| 41 | Faint, disconnected lines on the lakeshore | A broken world, with a comprehensible repair goal |
| 45 | First continuous shore light trail | Immediate evidence of making a difference |
| 50 | Distant relay node linked to near shore | The world is wider than one board |
| 55 | One remaining dim gap in the network | Foreshadow the Chapter 3 finale without revealing its route |
| 60 | Central beacon/valley flow restored **after actual victory** | Concrete chapter payoff, new signal appears in distant region |

**Interaction rules:** Short environmental feedback on reaching a milestone; no repeated full-screen interruption. The Chapter finale is not marked completed merely because Level 60 is unlocked. Do not award any Perfect Shift just for a world-state transition.

### Chapter 4 — Yankı Alanı / Echo Field (61–80)

**Purpose:** Reveal that a move can awaken a distant node, and culminate in a dual-channel convergence.

| At level | Visible environmental state | Design meaning |
|---|---|---|
| 61 | Single faint ripple above the lake | First remote reaction |
| 65 | Two signal nodes with one connection | A visible relation between distant sites |
| 70 | Two shore-side routes light separately | Two-stage signal insight, matching Level 70's revised puzzle |
| 75 | Gate lights illuminate in deliberate sequence | Order and dependency, matching Level 75's revised puzzle |
| 80 | Two signal paths meet at the center **after actual victory** | Distinct finale and transition toward the future region |

**Pacing:** Level 73 now has two real signals on separate steps; Level 75's redirected push and later remote gate opening give a different kind of insight. Level 80 preserves the stronger gated two-blue endgame handoff. Between discoveries, a quieter puzzle may be acceptable if it introduces a meaningful choice rather than padding.

## Current implementation

Added `Assets/_Game/Editor/ChapterJourneyPrototype.cs` (and `.meta`), accessible through:

- **SHIFT → Chapter Journey → Preview 41-80:** interactive Unity Editor window with level slider, TR/EN text, level-win state, accessible reduced-motion alternatives, milestone cards, progress and color/state cues
- **SHIFT → Chapter Journey → Export EN-TR Design Draft:** writes `Validation/chapter3_4_journey_editor_draft.json` locally (not to the repo unless intentionally committed)

`Assets/_Game/Tests/Editor/ChapterJourneyPreviewTests.cs` validates milestone count, language coverage, end-of-chapter completion boundaries and chapters 41–60 / 61–80. These engine tests are **added, not run**.

**Crucial:** The Editor preview is **not connected to a runtime HUD or world scene**, and the exported JSON is not loaded in the game. The preview is a reviewable implementation of the concept and rules, not a claim that players already see these changes.

## Later runtime connection (after concept/design signoff)

1. Review the Editor stage cards for narrative clarity. Keep the snippets short enough to be understood while actually playing. Adjust EN/TR copy before committing them to production.
2. Bind chapter world-state exclusively to the existing **authoritative, saved completed-level count / actual completion events**. Do not infer a completed finale just from opening its level.
3. Introduce a small persistent progress element in the Chapter screen / existing world view (not over the active puzzle board). Only show the changed connection at milestone boundaries, then keep it visible.
4. Support `Reduced Motion`: set final lighting state directly and never require animation to convey the signal result. Every step must be comprehensible with motion off.
5. Use a single subtle one-shot environmental transition after Level 60/80 win. No mandatory extra tap or blocking tutorial to proceed, and no new menus.
6. Verify persistence over force-close, replay, chapter switching, locale toggles, platform restore and chapter access gating.
7. Only after design completeness, test with real players, run Unity EditMode, QA on physical Android and iOS, validate existing saves and EN/TR, then decide whether the prototype should be integrated.

## Forbidden shortcuts

- No new currencies, pet care loop, timers or quest claims attached to these world cues.
- No rewrites of the 1–40 published game experience.
- No fake unlocks or rewards without actual saved completion.
- No importing unapproved assets or changing the existing alpine-lake visual identity.
- No public claims that chapter progression is playable until runtime is actually connected.
