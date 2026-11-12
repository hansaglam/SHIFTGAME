# SHIFT — Chapter 5 (Levels 81–100): first authored quality pass

**Date:** 2026-11-12  
**Git branch:** `feature/chapter5-level81-100-design-v01`  
**Prerequisite branch:** `feature/level41-80-design-quality-v02` (draft PR #2)  
**Status:** **20/20 independent simulation candidates, NOT Unity-certified, NOT real-player-tested.**

## Product direction and sequencing

The first 100 levels are intended to stay readable, satisfying and creative rather than simply adding taps. SHIFT's distinct pleasure is a **predictable single-tap action with surprising but explainable cascading consequences**. New Chapter 5 uses currently implemented **push/redirect, switch/gate, Resonance 2.0, multi-color exits, and shared-route timing** in progressively richer combinations.

Do **not** introduce a complex new gameplay-system family before Level 100. After 100, the next phase may introduce deeper mechanics in short, isolated pilots—after they demonstrate a genuinely different decision rather than another Rotator clone. **Mora enhancements come after world/content development**, and comprehensive blind user, iOS/Android device and release testing come at the end. Internal independent simulation and source regression checks continue throughout authoring.

## Chapter 5 — Birleşen Akışlar / The Confluence

**Purpose:** Restore converging routes from the distant shores and connect them to one central current without a mandatory lore modal or added metagame. Use the already established alpine lake palette and subtle gold/teal highlights.

| Milestone | Intended visible stage | Puzzle-development purpose |
|---|---|---|
| 81 | First bridge between two separated routes | Introduce linked consequences clearly |
| 85 | Second route joins the first | Compare remote node and direct actions |
| 90 | Two gates reach a common junction | Require sequencing rather than clutter |
| 95 | Three streams almost converge | Set up Level 100 as a genuine final challenge |
| 100 | Three streams converge; distant horizon revealed only after win | Visible chapter payoff, curiosity beyond 100 |

All milestones have EN/TR copy, reduced-motion static descriptions, saved-completion-only progression, and non-blocking notification logic. The existing route strip and preview are extended to five Chapters. **No complete new 2D environmental art scene has been delivered;** this is runtime UI/progression plus Editor design concept.

## Puzzle metrics — verified in a separately reconstructed BoardManager model

The following are **candidate properties**, not gameplay metrics from users or Unity Editor certificates. All have a winning recorded shortest route and a distinct +1-tap win, checked using a separate BFS/replay implementation of SHIFT's source rules.

| Level | Shortest | Alternate | Multi-piece taps | Productive remote moves | Gate openings |
|---:|---:|---:|---:|---:|---:|
| 81 | 7 | 8 | 3 | 1 | 1 |
| 82 | 7 | 8 | 5 | 1 | 1 |
| 83 | 7 | 8 | 3 | 2 | 0 |
| 84 | 7 | 8 | 5 | 1 | 1 |
| 85 | 7 | 8 | 3 | 2 | 1 |
| 86 | 7 | 8 | 4 | 1 | 1 |
| 87 | 8 | 9 | 6 | 1 | 1 |
| 88 | 8 | 9 | 5 | 1 | 1 |
| 89 | 8 | 9 | 5 | 1 | 1 |
| 90 | 8 | 9 | 4 | 1 | 1 |
| 91 | 8 | 9 | 4 | 2 | 2 |
| 92 | 8 | 9 | 4 | 2 | 2 |
| 93 | 9 | 10 | 6 | 1 | 2 |
| 94 | 9 | 10 | 7 | 1 | 2 |
| 95 | 8 | 9 | 2 | 2 | 2 |
| 96 | 10 | 11 | 7 | 1 | 2 |
| 97 | 9 | 10 | 5 | 2 | 2 |
| 98 | 9 | 10 | 4 | 1 | 2 |
| 99 | 10 | 11 | 7 | 1 | 2 |
| **100** | **9** | **10** | **4** | **2** | **2** |

- `20/20` recorded shortest paths and `20/20` alternate paths independently replay to victory, with separate BFS minimum searches. Each level uses at least one real remote receiver motion.
- `0/20` have four or more consecutive single-piece-only taps on the recorded shortest path **in the independent model**. This cannot prove fun.
- Target palette is two or three target colors; no enormous 40-tap solutions or newly introduced gameplay types. A +2-tap budget gives headroom beyond both authored winning routes. These are candidate values until Unity checks.
- Level 100: three target colors, five target normal pieces, two signal pairs, two switch/gate channels, two productive remote movements and four multi-piece taps; nine-tap shortest and ten-tap alternative.

### Quality fixes during this pass

Similarity analysis flagged recycled solution motifs despite different board coordinates. Instead of approving them, Level **88** was rebuilt around additional helper/route interactions; **89** changed from a seed-79-like motif to a different push/direction setup; **92** moved to a three-color, two-effective-signal puzzle; **98** now has three target colors and two distinct gates; **99** now uses a different two-gate sequence. All changes retained original new Chapter5 asset GUIDs, updated the shortest route, editor-only longer win and source manifest, and were replayed again.

**Remaining editorial risk:** A number of candidates still descend from Chapter4 templates, and pairs such as 85/92 or 93/96/99 can feel related despite different placements. Creative originality is **not proven** by their file names or metrics. Run the new `SHIFT → Chapter 5 → Export Decision Motif Similarities 81-100` action in Unity to flag repeated tap/action signatures, then review adjacent levels by their *required insight*, not solely their minimum taps. No claim that 20 new decisions are completely unique.

## Source changes and safe preview

- `Assets/_Game/Data/Levels/Chapter5_ConfluenceDrafts/Level81_*.asset` through `Level100_*.asset`: **20 newly created Unity level files plus 20 stable .meta GUIDs**, *never* replace shipped levels.
- `Assets/_Game/Editor/Chapter5Drafts.cs`: names, alternate routes, authoritative `CampaignCertification` test command, productive Resonance/rhythm checks, JSON results, pairwise motif scan.
- `Validation/chapter5_level81_100_authoring_manifest_v01.txt`: auditable 20-level proposal source used to author the assets and alternate routes.
- `Assets/_Game/Scripts/Systems/ChapterJourneyCatalog.cs`, `ChapterSelect.cs`, `ChapterJourneyPrototype.cs`: Chapter5 stages, 5-tab navigation and 41–100 Editor preview.
- `ChapterJourneyScenePreview.cs`: **SHIFT → Chapter Journey → Create or Open Isolated 100-Level Preview Scene**. It builds a *separate local* preview from the unchanged 40-level production scene and loads all 100 assets in order, never modifying the shipped scene or Build Settings. Preview-only unlock has its own `SHIFT.Preview.Chapter5.v1.` save prefix. Scene and `.meta` excluded by `.gitignore`. **The local scene has not been generated here.**
- `ChapterDraftLocalization.cs` + `LocalizationCatalog.cs`: repaired a real copy problem: the prior fallback titled draft levels “SHIFT” and could show Turkish hints in English. Draft levels **41–100** now provide EN/TR titles and hints; Level 1–40 original localization entries take priority and are not rewritten.
- Unity tests added/extended: `Chapter5DraftTests`, `ChapterDraftLocalizationTests`, `ChapterJourneyPreviewTests`, `ChapterJourneyScenePreviewTests`, `ChapterJourneyRuntimeTests`.

## Validation not yet performed (release blockers)

- No Unity 6.3 LTS compiler, Editor test runner, device, or graphical scene is available through this connected GitHub workflow. **Unity `CampaignCertification.Prove`, EditMode/PlayMode tests and preview test are NOT run**. Independent Python validation is not a substitute.
- Before integration, run **SHIFT → Chapter 5 → Validate All Draft Levels 81-100** and **Export Decision Motif Similarities 81-100**; review generated JSON.
- Exercise 40/60/80/100 chapter selector modes, real frontier vs unlocked-only, EN/TR text on each 41–100 level, Reduced Motion, undo/restart and effects; ensure Chapter5 100 world milestone only appears after an actual first victory.
- Verify chapter content registration/references from source (not only created assets) and on-device visual readability before any release decision. Test force-close/relaunch and older saves without altering them.
- The creator wants **final player-experience tests after content and Mora work**, rather than blind player testing now. This ordering is respected; mechanical/compile failures must still be fixed whenever discovered.
- Published Levels 1–40, original 40-level `Prototype.unity` scene, live saves, ads, Mora, and `VerifiedOptimality` registry remain untouched.
- Draft branch **must not** be merged to `main` or production. No Perfect Shift certificates on 41–100 until engine proofs and final design approval.

## Next design sequence (no automatic release)

**A.** Iterate the high-similarity Level81–100 candidates and final-world visuals using the new audit. **B.** Optionally prototype new, truly distinct complex mechanics for **Level101+** after this first 100-level baseline is convincingly strong. **C.** Revisit Mora's scene quality/evolution once chapters and worlds feel cohesive. **D.** Run all Unity/regression, retention/UX, real-player and Android/iOS tests at the final acceptance stage, with publication conditioned on zero release blockers.


## Integration compatibility fixes discovered during final source audit

The 100-level plan exposed three genuine issues outside the new puzzle asset folder. Each fix is **guarded** to avoid altering the shipped 40-level experience:

1. **Returning 40-level completers:** Existing `LevelProgression` saved `HighestCompleted = HighestUnlocked = 39` on a finished, 40-level release. Merely giving Unity a 100-level array would strand these players before Level 41. The constructor now advances **only the unlocked frontier to 40** when `levelCount > 40` and 40 was already completed. Completion, earlier Perfect flags, low-level replay choice and the original PlayerPrefs keys remain untouched. Added tests for old-40 stability, Level41 access, relaunch and replay selection.
2. **Correct Chapter 3/4/5 final message:** The existing `GameHud` treated every chapter other than Chapter 1 as Chapter 2. It now retains existing Chapter 1–2 completion keys, while using chapter-number-aware EN/TR text for Chapter 3–5, with a separate terminal Chapter 5 horizon message. Unregistered Level60/80/100 finale candidates receive a proper finale presentation without changing published Level20/40 metadata.
3. **Onboarding copy:** The first-launch card previously said **40 handcrafted levels** even in a fully wired 100-level preview. For campaigns whose level array is **greater than 40 only**, the card now shows the actual dynamic count in EN/TR. The shipped 40-level text and onboarding flow remain unchanged. Added dynamic 60/80/100 formatting regression checks.

No user's prior completion is overwritten. These are source changes on the isolated Chapter5 draft branch, **not applied to the published app**.

**Next engine verification must explicitly exercise** a completed 40-level save opened in the 100-level preview, reentering Chapter3, and the correct 60/80/100 result screens. Failures here block release even if Level81–100 all solve.



## Second gameplay + world-quality pass — 12 Nov 2026 (latest)

**This section supersedes earlier interpretations of Level 91, 92, 95 and 97.** The table above has also been updated to match the current candidate boards. Nothing below is a Unity certificate or a player satisfaction claim.

### Removed repeated/decorative interactions

- **Level 91, Three Currents:** A formerly decorative second signal now remotely moves the blue helper waiting at its paired node. The helper is delivered by the same chain, after the yellow switch opens the second gate. Red's exit has shifted one step to preserve an eight-tap optimal route; a **nine-tap manual blue-helper alternative** remains. Both Resonance channels are now productive.
- **Level 92, Silent Relay:** Added a channel-2 switch at `(4,2)` and gate at `(5,1)`. The first distant signal opens channel 1, while the separate yellow side-route tap opens channel 2. Both channels matter, still **8/9 taps**. Relative shape-independent decision motif overlap with Level 85 decreases from ~86% to **~71%**, under the editor's 75% review threshold.
- **Level 95, Interlocked:** Moved the channel-2 receiver under the blue helper at `(5,6)` and moved the red exit to `(6,4)`. The second signal now contributes real motion and a late blue delivery. Shortest **8**, alternate **9**; on the longer path, the player chooses to open the *first gate manually* instead of using the first remote signal. Two productive channels rather than one and no four-tap solo corridor in the recorded shortest.
- **Level 97, Late Handoff:** Rebuilt the second signal receiver under the movable blue helper at `(2,1)`, moved yellow's starting target to `(3,6)`, moved red's exit to `(1,3)`, and added a second real yellow target at `(4,6)`. Two yellow targets now push as a convoy; the second signal remotely advances and delivers the blue helper. This creates **five multi-piece taps**, **two productive signals**, shortest **9**, alternate **10**. The longer route delivers the leading yellow before the trailing yellow instead of using the convoy's optimal push. Level 95/97 formerly had **identical** decision labels; after both redesigns, LCS overlap is **5/8 (62.5%)**.

Added `Chapter5Drafts.ProductiveResonanceChannels` to replay ordered BoardAction events and count actual **normal-piece movement at the remote partner**. The Unity candidate validator and `Chapter5DraftTests` now require **every Resonance pair shown on a Level81–100 board** to affect a piece on the recorded optimal route. This rejects the misleading situation where a decorative pulse is presented as a meaningful puzzle mechanic. Regressions lock the 91/92/95/97 route lengths, signal timings, channels, opening gates and late deliveries.

### In-game world visualization upgraded

Added `Assets/_Game/Scripts/UI/ChapterWorldBackdrop.cs` and its stable `.meta`. It draws a **noninteractive alpine backdrop inside the extended Chapter selector**, behind all buttons and level tiles, using Unity's existing UI VertexHelper (no downloads/new textures). Low-contrast mountain silhouettes, a lake ellipse, interconnected network routes and five beacon nodes are mapped to the real saved Chapter 3/4/5 completion state.

The three worlds use different route shapes, and completed connections gain a distinct static highlight. The end-of-Chapter horizon/beacon lights up **only after Level 60/80/100 is recorded complete**. Subtle .42s interpolation plays only on newly advanced milestones; Reduced Motion immediately displays the final state. The first-40 legacy selector never creates this component.

The existing top route strip had only ~4% screen height, risking unreadable status text. The expanded layout now reserves approximately 6% of the Chapter view for the strip and arranges title, purpose, route and Daily entry in **separate nonoverlapping vertical bands**, with auto-resizing text. In-game puzzles still receive all taps, unchanged.

New `ChapterWorldBackdropTests` checks legacy isolation, no fake restoration from unlocked Level 100, milestone-by-milestone completion, final horizon gating, save reset and Reduced Motion. Updated `ChapterJourneyScenePreviewTests` checks the backdrop is non-raycasting and the title/route text areas do not overlap.

**Important rendering limitation:** Procedural UI paths/silhouettes and the existing scenic background are implemented, but bespoke full-screen Chapter-world illustrations or a dedicated illustrated world map are not yet shipped. Device-level readability and visual appeal are not verified.

### Verification after edits

- Latest source-controlled Level81–100 YAML files, source manifest and editor alternate-route registry remain aligned; the changes preserved existing Chapter 5 asset GUIDs.
- Independent BoardManager-style search confirmed **8/9** (91), **8/9** (92), **8/9** (95), **9/10** (97), with actual winning replays, action-event logs and minimum searches.
- Level81–90 source/route/layer checks remain intact; Level91–100 passed a fresh source-vs-manifest static consistency check including legal gates, signal pairing, layer collisions, objective colors and move limits.
- The model-based LCS, action counts and BFS are design-screening evidence, not live Unity results. **Unity Editor compilation, CampaignCertification, new EditMode tests and actual device presentation still have NOT been executed** in this connector-only workflow.

### Remaining quality review

Pairs in the **93/96/99** design cluster still share some reasoning patterns (roughly 75–80% label overlap), even though their geometry, gates and chain lengths differ. The motif report is a **warning**, not proof of duplication, and does not justify changing a level just to game a metric. Review these next for a distinct causal decision *before* expanding to 101+. Mora and holistic blind player tests remain deferred per development plan.



## Third quality pass — Levels 93, 96, 99, 100 (13 November 2026)

**Design status: draft candidate, not Unity-certified or player-tested.** This section supersedes earlier editorial claims about Levels 93 and 99.

### Why the apparent Level 93/96/99 similarity is not the same design

A coarse action-label LCS reports about **78%** overlap for Level 93/96 and Level 93/99. The labels omit what an opened gate actually blocks and whether a pulse causes a real remote move. We compared the BoardManager action order and the dependency of each gate before changing anything.

- **93 — Hidden Crossing: delayed remote release.** Moved the channel-2 gate from `(1,5)` to `(3,4)`, directly ahead of the yellow target after it triggers the first Resonance. Yellow is **held on the sending node**. On the following red decision, the second switch opens that gate *before* a return pulse moves yellow through it. Previously this pulse could fire without a remote move. New optimal route: 9 taps, alternate: 10; **both pulses now productive**, 2 gates opened, and 6 multi-piece taps in independent replay. This level is about *holding and releasing a remote receiver*, not a corridor.
- **96 — Back Through: redirected push chain.** Retained the already distinct two-gate, push/redirect setup. The recorded ten-tap shortest includes multiple helper/target pushes and a different initial preparation than 93. We deliberately **did not change a functional Level 96 merely to beat a similarity score**.
- **99 — Convergence Test: true second-gate dependency.** Relocated channel-2 gate from unused `(6,0)` to `(3,3)`, which the red target **must cross after** the channel-2 switch opens on tap 2. The red target crosses on tap 4; previously opening this gate produced a visual event but did not constrain the solution. Shortest remains 10 taps; alternate 11. In a BoardManager-style counterfactual with the channel-2 switch moved out of reach, no winning route was found under the 11-tap search horizon. It still uses one productive Resonance pair, two distinct gate channels and a long coupled push route. Updated both TR and EN hints to describe **two gates**, not two nonexistent signal channels.

Original Unity asset paths and GUIDs are unchanged; only their serialized placement/hint data changed. `Validation/chapter5_level81_100_authoring_manifest_v01.txt` stays byte-for-byte aligned with the amended assets; Chapter5 editor alternate-route registry remains aligned.

### Level 100 — protect the meaningful final chain, improve its payoff

We specifically examined extending the last three taps. Adding extra blue targets or shifting starts/exits **did not yield a better, verified late multi-piece chain** in the screened candidates; most introduced extra single-piece preparation. We rejected those changes rather than inflate complexity.

The **existing gameplay board is retained intentionally**: 9-tap shortest, 10-tap alternate, 3 target colors, 5 genuine delivered targets, 2 productive Resonance channels on taps 1 and 4, 2 gates opened on taps 1 and 5, and a late blue-to-blue shared-exit push on tap 8 before the last blue delivery on tap 9. This already provides a causal multi-stage finale.

Added a distinctly **non-blocking, first-win-only Level 100 world convergence** to `ChapterJourneyMilestoneNotice`: two understated palette-colored lines draw toward the center for 0.42 seconds, alongside the bilingual Chapter 5 finale description. The cue is shown only if the save frontier records a new Level 100 win; replay, unlocking, Daily and previously completed 100 do not retrigger it. Reduced Motion renders its complete static state without animation. No new taps, rewards, sound tracks or Chapter 1–2 changes.

The 100-level finale regression now asserts exact remote pulse steps, gate openings, *five delivery events in order*, the blue-to-blue push and late multi-piece payoff. New test coverage also checks Level 93 action ordering (gate → pulse → remote yellow through gate), Level 99 red crossing the formerly decorative second gate, and Level100's one-shot presentation gating.

### Verification and remaining gates

- Independently replayed and breadth-first searched the amended **Level 93 (9+10)** and **Level 99 (10+11)**, plus the retained Level 100 (9+10). Source-level checks reconfirmed placement / known route / alternate registry / +2 move budget alignment.
- New/edited C# sources passed basic balanced-delimiter and Editor-versus-runtime reference inspection; **this is not a C# or Unity compile**.
- Unity 6.3 LTS compilation, `CampaignCertification.Prove`, EditMode suites, phone presentation, blind player tests and device QA have not been run in this connected GitHub workflow. These remain **explicit release blockers**.
- First 80 level assets, original production scene, published 40-level levels, `main`, saved-game schema, Mora and ads remain protected.
- Current rubric for the upcoming Unity review: a pulse must move a real receiver, a gate must alter the winning decision graph, finales should have multi-stage causal payoffs, and no metric-only changes should sacrifice readability.

