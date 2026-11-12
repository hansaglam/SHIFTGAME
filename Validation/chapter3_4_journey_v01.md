# SHIFT — Chapter 3–4 Purpose & Progression / Design Prototype v02

**Date:** 2026-11-12
**Scope:** Level 41–80 only, design preview in Unity Editor.
**Status:** **DRAFT RUNTIME UI INTEGRATION, NOT IN SHIPPING GAME**. Chapter screen code is conditionally extended for 60/80-level campaigns, while the checked-in live scene remains 40 levels. No changes to Levels 1–40, MORA, saved data format, Perfect Shift, ads or production build.

## Implementation update — Runtime Chapter Journey integration (design candidate, 2026-11-12)

This section supersedes the earlier statement that **only** the Editor preview exists. The content source is now shared between Editor and runtime, and the Chapter selection UI now supports the additional world/route presentation **when a complete 60/80-level campaign is supplied**. The currently checked-in production scene is still **40 levels**, so this change does **not** automatically make 41–80 available in published builds.

### What was implemented

- `Assets/_Game/Scripts/Systems/ChapterJourneyCatalog.cs` is the single EN/TR catalog for Chapter 3 and 4. The Editor preview `ChapterJourneyPrototype.cs` now reads it directly; it no longer contains a duplicate copy that could drift.
- `ChapterJourneyCatalog.Available(progress, chapter)` requires the entire Chapter's level array to exist and its first level to be unlocked. **Unlocked does not mean completed**. All five restoration beats and progress percentage use the **existing `LevelProgression.HighestCompleted`**, with no new PlayerPrefs keys, external services or mastery reward claims.
- `ChapterSelect.cs` preserves the **original two-tab 40-level layout**, while dynamically adding Chapter 3/4 tabs for complete 60/80 level arrays. Those tabs are disabled until their existing chapter frontier unlocks. The extended layout includes a five-node route-restoration strip, progress and a short localized current world-state label. The 20-level card grid remains accessible.
- `ChapterJourneyPanel.cs` renders restored vs muted nodes and a calm connecting route. Stage colors follow the established lake/teal and echo/purple direction, not a brand-new UI system. The strip is passive (no raycast capture, currencies or mandatory next steps), and localizes whenever the existing GameLanguageService changes. Its normal state is static, including with Reduced Motion enabled.
- `ChapterJourneyMilestoneNotice.cs` displays a **brief non-blocking** EN/TR world milestone when a level 41/45/50/55/60 or 61/65/70/75/80 is **newly completed** in a fully wired preview campaign. It relies on the before/after saved completion frontier; replay, Daily, debug/invalid wins and mere unlocks cannot trigger a restoration. Reduced Motion displays the final state directly (no fade); it does not interfere with board input.
- `PrototypeGame.cs` only instantiates this notice when `levels.Length >= 60` and uses its existing completion callback to show it. Existing CampaignEnding, save format, audio, ads and the first 40 levels are unchanged.

### Safe way to see it in a complete 80-level campaign

A new **opt-in** Editor menu action can create an independent Preview scene:

1. Checkout `feature/level41-80-design-quality-v02` in a separate local working tree.
2. Open Unity **6.3 LTS 6000.3.23f1** and first review the Console for compile issues.
3. Select **SHIFT → Chapter Journey → Create or Open Isolated 80-Level Preview Scene**. The tool checks all 80 assets first, then copies `Assets/_Game/Scenes/Prototype.unity` to `Assets/_Game/Scenes/SHIFT_Journey_80Level_Preview.unity` (only if this separate path does not already exist). It assigns levels 1–40 and the 41–80 draft candidates in order **without modifying the original scene**.
4. To browse unlocked draft levels without grinding the first 40 again, choose **SHIFT → Chapter Journey → Unlock Draft Chapters in Preview Save Only**. This writes **only** `SHIFT.Preview.ChapterJourney.v1.*` keys. It marks first 40 levels as completed *only inside the isolated debug save*; Chapter 3/4 world milestones still start un-restored. It never uses or overwrites the real player progress namespace.
5. Enter Play Mode in the copied preview scene, open Chapters 3 and 4, check milestone progression, page switching, text length, Reduced Motion, and win-only finale states. The copied scene is not automatically added to Build Settings; it is not production content.

The preview generation is **not** executed by this GitHub-only workflow. The actual copied Unity scene file is created on the user's computer by the Editor tool. All newly added runtime code and tests still require local Unity compilation and validation.

### Engine acceptance checks added

- `ChapterJourneyRuntimeTests`: legacy 40-level isolation, unlocked-vs-completed, real 60/80 finale victory, reopening saved progress, incomplete chapter visibility, and new-milestone-only feedback.
- `ChapterJourneyScenePreviewTests`: original 40 references retained, every Level 41–80 candidate assigned to expected index, copied scene excluded from build, and layout 40-vs-80 selector shape.
- Existing `ChapterJourneyPreviewTests`: bilingual stage copy and reduced-motion descriptions, editor stage boundaries and finale preview guard.

**Tests authored but not executed in Unity**. Real-player testing is intentionally deferred until content design is complete. Before publication, inspect small-screen typography/touch targets and ensure existing progress, returning player menu, onboarding, campaign ending, Daily Shift, EN/TR and Perfect Shift continue working.

### Intentionally not included yet

- No external original background illustrations, complete scenic environment transformations or full new Chapter-world 3D/2D scenes. This is a **reusable route/progress presentation plus a subtle milestone cue**, not a claim that the entire lake landscape is dynamically rebuilt.
- No automatic assignment of the 80-level candidate array to the live production `Prototype.unity`.
- No new reward economy, Mora dependencies or offboard minigames.

### Release boundary

**Keep PR #2 draft.** Do not modify main or production build settings until authored levels are Unity-certified and the complete player experience has been reviewed. A directly cloned/remote Github source branch is not a running Unity build.

---

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
