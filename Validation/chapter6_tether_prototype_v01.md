# SHIFT — Chapter 6 / BAĞ (Tether) Prototype v01

**Branch:** `feature/chapter6-tether-prototype-v01`  
**Parent:** `feature/chapter5-level81-100-design-v01` (draft PR #3)  
**Date:** 13 November 2026  
**Stage:** isolated mechanic experiment with four authored puzzle assets, **NOT a release-ready Level101–120 campaign**.

## The product decision

After 100, SHIFT needs a **new decision type**, not just more moves or a re-skinned Rotator/Resonance. The first candidate is **BAĞ (Tether)**: two genuine Normal pieces bear the same gold-numbered bond and a continuously visible rope. The player can tap either linked Normal, but can move only if the bond can remain within its strict length.

**Why this is distinct from Resonance:** Resonance sends a one-time signal when a piece enters a node. BAĞ changes the physical *permission to move* at every step, enabling dragging, tension blocking, direction changes, helper pushes and dependency puzzles without adding swipes or new control gestures. No RNG; identical state+taps must always produce identical events.

### Exact deterministic rules

1. Two Normal pieces may share the optional `tetherId` 1 or 2. Each id must appear **exactly twice**, with initial Manhattan separation of **one to three cells**. A normal without an id is unaffected.
2. A link's permitted Manhattan distance is **three cells**. If a proposed movement would exceed it, the partner is pulled **one cell toward the moving piece's prospective position**, using the longer of the X/Y axis, **X on ties**.
3. The pull is normal BoardManager movement: walls, closed gates, colored exits, pushes, switches, Direction/Rotator and Resonance reactions are not bypassed. A pulled piece turns to face the pull direction; direction tiles may redirect it.
4. If the partner **cannot make a legal pull**, the initiating movement is blocked. A partially successful pull that makes the source's move fail is **rolled back locally**—including switch/gate states, signs and every action—not a secret extra tap.
5. Both moves and the pull cue happen within **one committed tap**. A blocked pull spends **zero moves**. A cyclic/oversized reaction uses the existing full-tap overflow rollback.
6. When either linked piece is delivered into its exit, that endpoint becomes inactive, so the remaining piece is no longer constrained by the bond. **Undo** returns to exactly the saved positions, directions, gate states and binding condition.
7. This is a new serialized **optional field**, not a new `PieceType`; all 1–100 level data have the default 0 (no bond). `VerifiedOptimality.Fingerprint` binds only nonzero tether ids, retaining legacy hashes unchanged.

### First four distinct puzzle pilots

These are **independent Python-model shortest routes**, not Unity-proven certificates.

| Prototype | New player insight | Shortest | Alternative | More than tapping corridors |
|---|---|---:|---:|---|
| 101 — The First Bond / İlk Bağ | Pull blue onto a switch while moving red, then finish the linked targets | **3** | 4 (manual switch) | First pull opens the gate; second pulls blue into exit |
| 102 — Blocked Tension / Gerilen Yol | A blocked source cannot drag its partner through a wall; moving the other endpoint first pulls red into an Up direction tile | **3** | 3 (different exit order) | The denied initial tap communicates a real constraint |
| 103 — Triple Reaction / Üçlü Tepki | A single BAĞ pull carries blue into Resonance; Resonance drives yellow onto a switch; blue exits while red advances | **4** | 5 (manual Resonance) | **Three different Normal pieces** move in the first tap |
| 104 — Two Keys, One Bond / İki Anahtar, Tek Bağ | First open a gate so the blue partner can be dragged; blue opens the next gate, then the last red tap drags blue into exit | **5** | 6 (manual blue gate) | Both gate channels genuinely gate the solution |

**Expected pace:** 101 establishes the concept, 102 teaches a blocked bond/different direction, 103 shows surprising but fully explainable BAĞ+Resonance, and 104 asks for a multi-stage gate dependency. The short routes intentionally avoid 15-tap repetitive corridors. An alternate win may be longer or equally short but reorder objectives; never force a fabricated longer route as padding.

### Implemented files

- `LevelData.cs`: optional `PiecePlacement.tetherId` + validation (pair count, type, initial distance); default zero for old content.
- `BoardAction.cs`: `BoardActionType.TetherPull` appended *after* existing event types; `BoardPiece.TetherId` for renderer/model.
- `BoardManager.cs`: mapping to true linked pieces, atomic stretch-and-pull, local snapshots on attempted tension moves, deterministic X/Y tie, undo-compatible rollback. No drag/hold input or RNG.
- `Piece.cs` and `TetherLinkGraphic.cs`: gold endpoint badges and passive dynamically drawn link following animated stone positions; raycasts disabled on all decorations. The link vanishes when one endpoint is delivered. No external textures.
- `BoardView.cs`: draws link **above terrain, under Normal pieces**, shows a small tactile feedback on the tension event; Reduced Motion still conveys the visible link without requiring movement animation.
- `MechanicGuideState.cs`, `MechanicGuidePanel.cs`, `LocalizationCatalog.cs`: adds bilingual first-encounter BAĞ explanation with an example line, no new overlay/input model and no new quest/reward economy.
- `TetherPrototypeLocalization.cs`: specific EN/TR titles and hints for 101–104 while preserving 1–100 copy.
- Four `.asset` `LevelData` prototypes and unique `.asset.meta` GUIDs under `Assets/_Game/Data/Levels/Chapter6_TetherPrototypes`.
- `Chapter6TetherPrototypes.cs` Editor: isolated preview and chapter pilot validator; tests under `TetherMechanicTests.cs` and `Chapter6PreviewTests.cs`. Preview scene `SHIFT_Tether_101_104_Preview.unity` and .meta are `.gitignore`d.

### How to view it in Unity (ONLY on this branch)

1. Checkout `feature/chapter6-tether-prototype-v01` in a separate working tree and open **Unity 6.3 LTS**.
2. Let Unity import/compile; fix **any Console errors before opening Play Mode**.
3. Select `SHIFT → Chapter 6 → Validate BAĞ Prototype Levels 101-104`. This calls the authoritative `LevelValidation.VerifySolution` and `CampaignCertification.Prove` for all four, checks a real alternative win, genuine TetherPull events and meaningful multi-piece action, and writes `Validation/chapter6_tether_unity_proofs.json`.
4. Select `SHIFT → Chapter 6 → Create or Open Isolated BAĞ Preview`. This loads the existing 1–100 assets and four pilot assets into a *separate local scene* copied from the original Prototype scene. **Do not add the preview to Build Settings**. A separate `SHIFT.Preview.Chapter6Tether.v1.*` save is used, never the real player namespace.
5. To jump directly to a pilot, use `SHIFT → Chapter 6 → Preview Level 101` (or 102/103/104) **outside Play Mode**, then enter Play Mode.
6. Use a narrow phone-resolution Game view with TR and EN, Reduced Motion on/off, undo/restart, first-encounter guide and rapid taps, then inspect board/rope readability and haptic sequence. This is internal UI/engine QA, **not yet the final user study**.

**The preview scene does not exist as a committed file**; it is created only by running the above Unity Editor menu locally.

### Automated acceptance gates (authored, not executed in Unity)

`TetherMechanicTests` checks: pair validation, actual switch activation by a pulled partner, first-tap multi-piece count, deterministic same-tap replays, Undo, blocked source when the partner hits a wall, redirected forced follow, full local rollback when a pulled partner opens a switch but the source then cannot push, deterministic longer-axis/X-on-tie pulls, **two independent bond pairs** that never cross-signal, BAĞ+Resonance with three moved Normal pieces, two-gate dependency, all shortest/alternative routes, a nonzero bond changing the future Perfect-proof fingerprint, and old 1–100 assets remaining unlinked.

`Chapter6PreviewTests` checks: unchanged 1–100 asset identity, isolated preview/build settings/save prefix, specific bilingual hints, distinct board layouts and recorded shortest routes. No new speculative Perfect Shift certificates.

**Unverified here:** real Unity compilation, Editor tests, device layouts, visual feedback, true player difficulty/retention, effects in unusual nested reactions and physical-device performance. Independent Python replays prove only consistency with a separately reconstructed rule model.

### 105–120 Chapter 6 roadmap — do not bulk-generate

Only if the first four pilots are understandable, fun and robust in engine checks:

| Levels | Planned learning arc | Exit criteria |
|---|---|---|
| 105–108 | Tension with walls/one-way routes and partner direction | No invisible blocked taps, clear anticipatory visual state |
| 109–112 | Branching solutions: manually prepare partner vs intentional bond pull | Different *reasons* for short and long wins |
| 113–116 | BAĞ + existing Rezonans, dual gates, two pairs in selected puzzles | Each interaction must move a real piece or change the legal decision graph |
| 117–119 | Mastery without bulk: fewer routine taps, more chain-based surprises | Visual readability, bounded solver cost, causal originality |
| 120 | Chapter 6 finale with an environmental bridge payoff | Two-stage chain, distinct finale mechanic, a world that visibly reconnects |

Recommended Chapter identity: **Bağlı Ufuk / Bound Horizon**—two distant shores gradually connect by bridges. Do not insert another mandatory lore popup, currency, streak-pressure timer or unrelated minigame. The purpose is to make players curious about the next **puzzle**, not trap them in an engagement system. Keep Mora and holistic blind player tests at the agreed final phase.

### Stop / redesign criteria

- If players cannot predict the pull direction (especially diagonal positions), add a clear direction preview or simplify tie rules before shipping.
- If blocked-pull states feel unfair or uninformative, improve feedback and/or remove the board design.
- If a level relies on excessive single-piece tapping, remove the padding rather than increase moveLimit.
- If BAĞ mostly imitates a remote Resonance event without meaningful space/constraint decisions, **reject or redesign the mechanic**.
- If 101–104 fail Unity validation or regress 1–100, do not merge and do not expand to 105–120.
- Retain the user-requested order: 101+ game systems → world progression → Mora LAST → final real-player/device/release tests; mechanical regressions are checked during development.

**Branch remains DRAFT. No merge to `main`, no production build, no Level 101–120 unlock, no Perfect Shift registration.**


### Source audit addendum — safety and technical constraints

The new puzzle-folder `.meta` is tracked with a stable GUID so Unity does not generate different folder metadata on each development computer. A focused source-level check verified **4/4** prototype asset/alternate lists, matching GUIDs, unique layer positions, legal initial Tether distances and targets. GitHub comparison confirms no Level1–100 `.asset` files or real production scene were changed.

The new Tether event is displayed **before** partner movement and followed by the actual board actions; there is never a second virtual tap. Forced partner movement propagates the "already following this link" flag through Direction/Rotator/Resonance automatic continuation, preventing an accidental second pull during the same forced step.

The internal candidate export now records **multi-piece taps, Rezonans pulse events, gate openings and distinct moved pieces** instead of using raw animation/event count as a fun score. Every pilot must have a real bond pull and at least one multi-piece decision; these numbers screen for inert mechanics but do not establish player enjoyment.

Note that the **preview's optional direct-level jumps** can mark earlier pilot levels completed *in the isolated development save only*, to preserve the ordered completion frontier. Real player progress and shipped Level1–100 saves are never touched.



## Visibility fix — 13 November 2026

**User observation:** “Chapter 6 isn't visible.” Source audit identified two separate UI locations:

- **In the Unity Editor top menu:** The command `SHIFT → Chapter 6 → Create or Open Isolated BAĞ Preview` is declared in `Assets/_Game/Editor/Chapter6TetherPrototypes.cs` **on this feature branch only**. If the entire Editor menu is missing, confirm the currently checked-out branch and inspect Console for red script compilation errors. A normal 40-level main checkout cannot show this editor extension.
- **Inside the game:** The previous `ChapterSelect` loop used `Mathf.Min(5, count / 20)`, intentionally showing only five **complete 20-level chapters**. A 104-level pilot therefore had no sixth chapter tab even after a successful preview scene creation. The prototype wasn't accessible from the chapter selector.

**Implemented corrective change (prototype-only):**
`ChapterSelect` uses `tetherPreview = count == 104` and creates **six tabs only in that isolated 104-level scene**. In narrow portrait layouts the first five tabs become numeric and the sixth shows `6 · BAĞ` (TR) or `6 · BOND` (EN). The page title is `BÖLÜM 6 · BAĞ PROTOTİPİ` / `CHAPTER 6 · TETHER PILOT`, clearly explaining that only four 101–104 puzzles exist. No fake five-beat restored Chapter 6 world is displayed. The existing LevelProgression unlock/save logic remains unchanged.

The normal 40-, 80- and 100-level selectors remain unchanged; main and the shipped Prototype scene remain untouched. Updated `ChapterJourneyScenePreviewTests` checks that 104 adds the sixth tab and `Level 104` but not `Level 105`, and that the tab is absent in the 40/80/100 screens.

**IMPORTANT:** The GitHub source fix is committed. It does **not** automatically update a local Unity checkout. In GitHub Desktop select `feature/chapter6-tether-prototype-v01` and **Fetch/Pull**; then wait for Unity to finish compiling. Use the top menu to create or open the isolated preview scene. If Unity's top menu itself is missing, local Console errors/branch state still require inspection; this workspace has not executed Unity compilation.



## Unity device-report follow-up — 13 November 2026

A developer opened the generated `SHIFT_Tether_101_104_Preview` scene and ran **`SHIFT → Chapter 6 → Validate BAĞ Prototype Levels 101-104`**. Unity's log reported **4/4 engine checks passing**, including Level 104's minimum five-tap route, six-tap alternate and solver state count. This establishes engine-level proof for the four *mechanical* candidates, **not** successful in-game presentation or player enjoyment.

However, two distinct issues appeared in the Game view:

1. `InvalidOperationException: Exit Play Mode first.` followed an attempt to choose a pilot Level 104 from the Editor menu **while already in Play Mode**. This is an invalid editor action, not a BoardManager failure. The `Preview Level 101–104` and `Create or Open Isolated BAĞ Preview` menus are now disabled during Play Mode; underlying methods also issue an informational warning instead of throwing in that case.
2. `resume.no_levels` ("Kampanya kullanılamıyor") was shown on the scenic startup shell. In `PrototypeGame.ReturningStartup`, that message means `CurrentLevel` is either null or fails `LevelData.ValidatePlayable`. The specific cause cannot be identified from that screenshot because the previous code suppressed the validation reason.

**Corrective implementation:** The old `CreateOrOpenPreview` returned immediately if the scene file already existed, without revalidating its 104 `LevelData` references. It now **rewires and saves any existing isolated preview scene**, just like a newly copied scene, ensuring all 104 references, `useLevelSet=true`, and separate `SHIFT.Preview.Chapter6Tether.v1.` save/settings prefixes match the generated campaign. Selecting a pilot level outside Play Mode also performs this repair, using the expected level set rather than trusting old serialized state. A post-save `SerializedObject` check ensures all 104 references and key settings remain correct.

For any *remaining* startup failure, `PrototypeGame` now writes one explicit Console error with the **actual one-based level index, assigned array count, asset name and validation reason**, without resetting progress or bypassing broken content. This is intentionally diagnostic; it does not hide the error.

**How to retest:** Stop Unity Play Mode, close Unity, pull the latest `feature/chapter6-tether-prototype-v01` into the isolated clone, reopen Unity, choose `SHIFT → Chapter 6 → Create or Open Isolated BAĞ Preview` and then `SHIFT → Chapter 6 → Preview Level 101` **while not playing**. Press Play only after the level is selected. If "Campaign unavailable" persists, report the new precise Console error; do not assume the repair worked or edit production/PlayerPrefs keys.

The Play-mode fix and scene rewiring have been source-reviewed but **have not been rerun in Unity after the update**. The main scene, 1–100 asset data, Mora and live player records remain unchanged.
