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
| 91 | 8 | 9 | 4 | 1 | 2 |
| 92 | 8 | 9 | 4 | 2 | 1 |
| 93 | 9 | 10 | 6 | 1 | 2 |
| 94 | 9 | 10 | 7 | 1 | 2 |
| 95 | 9 | 10 | 4 | 1 | 2 |
| 96 | 10 | 11 | 7 | 1 | 2 |
| 97 | 9 | 10 | 4 | 1 | 2 |
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

