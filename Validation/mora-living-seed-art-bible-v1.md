# SHIFT | MORA Living Seed — Character Art Bible & Production Gates v1.0
Status: **creative direction / NOT approved game assets** (2026-11-06)
Source of truth: user-provided "MORA — Yaşayan Tohum" concept board; the teal, curled-horn, glowing diamond creature is a different prospective collectible. 
Scope: new content after Level 40 and companion UI. No modifications to Levels 1–40 or the current published title.

## 01. Creative promise
SHIFT remains a deterministic, satisfying, clever chain-reaction puzzle. MORA provides an optional emotional companion layer: solve puzzles -> care for a small world -> see genuine visual change -> want to return. The character must be endearing enough to work as a tiny icon and expressive enough in a large habitat. Do not turn basic puzzle success into a grind or virtual-pet chore.

## 02. Primary character: MORA (LOCKED direction pending final model sign-off)
- Family: round, soft cream/peach living seed, with a cap/collar of glossy overlapping green leaves.
- Face: small dark vertical oval eyes, short simple mouth, subtle blush, stable eye spacing.
- Signature: distinctive asymmetrical leaf canopy and a warm golden luminous bud/tendril (the configuration develops with stage).
- Silhouette: broadly rounded, grounded, non-humanoid, no arms/legs that redefine species.
- Surface: soft velvet seed body contrasting with waxy leaf surfaces, warm light and gentle subsurface response.
- Personality: curious, warm, peaceful, sometimes exuberant but never sarcastic or intrusive.
- NOT MORA: current teal triangular mascot with large asymmetrical eyes, golden curled protuberance, and glowing diamond. It stays as a **separate, optional surprise companion candidate**, not an evolutionary stage.

## 03. Four distinct growth forms — visible silhouette change required
The user explicitly rejected stage changes that only add flowers, an aura, a progress bar, or a "TEST +" label. New forms must remain unmistakably the same seed-family character while a player can tell adjacent forms apart at 200px on a phone.

| Stage | Character form / identification | Habitat consequences | Art gate |
|---|---|---|---|
| I: Awakening | Tiny cream seed in a protective curled leaf, a single sprout tip, compact silhouette. Sleepy-to-curious baseline. | Sparse stone island, sprout of moss, still water, one magical thread. | One distinct single leaf silhouette |
| II: Sprout | Seed larger; clear green leaf hood/collar + first longer golden bud/stem; childlike open eyes. | Green spreads across stones, little flowers and bright trickle. | Leaf canopy clearly reads at 200px |
| III: Bloom | Broad overlapping leaf crown, 1–2 articulated warm golden bud stems, first blossoms; seed remains the same facial identity. | More plant coverage, active water, flowers, small renewed ruin. | Distinct foliage weight, do NOT simply enlarge stage II |
| IV: Flourish | Mature, layered asymmetric leaf crown, warm luminous flowers, special readable outline; still round cream face / same eyes. | Lush living sanctuary, flourishing flowers, warm ambient magic, transformed central landmark. | More complex silhouette but not visually cluttered |

Names are working terms until final in-game bilingual copy is approved. Previous threshold suggestions (levels 50, 70, 100) are not release-locked and must be tested against actual 41+ level pacing. Early 40 never grant companion progression resources.

## 04. Production kit structure (separate concepts from assets)
- `Concept/MORA/`: exploratory boards, silhouettes, reference, comparisons. Do not import directly as game runtime sprites.
- `Source/MORA/`: approved layered artwork or authored 3D model, materials, lighting rig and camera. Keep locally in art source project, versioned with legal redistribution rights.
- `Unity/Resources/Mora/` (pilot paths only): transparent, consistent final front-facing expression sprites. Production-ready assets should be moved to a controlled Asset/Addressables pipeline after profile/performance validation.
- Primary sprite naming: `MoraSeed_S1_Idle`, `MoraSeed_S1_Happy`, `MoraSeed_S1_Curious` ... to `S4`; reserve generic `MoraSeed_Idle` only as a temporary stage-1 compatibility alias. Avoid ambiguous teal `Mora_Idle` for the main character.
- Special companion assets live under a different folder/name; never reuse primary MORA filenames or IDs.
- Never generate "expression" art by crop-upscaling small character illustrations out of a presentation.

## 05. 2D/3D visual consistency contract
- Establish one master front 3/4 pose (camera focal length, yaw, pitch, eye line, world-space centre, scale, all locked).
- All stages: same world orientation, normalized avatar bounding box, neutral key/fill/rim lighting, background-free transparent alpha.
- Production sources: 2048×2048 or higher lossless RGBA, with layered face, eyelids, leafy cap, bud stems, body and shadow when independent motion is required.
- Unity reference preview sprites: 1024×1024 RGBA; preserve aligned pixels-per-unit and identical bottom-centre pivot, avoid tight destructive crops, keep sufficient transparent margin for bounce and leaf growth.
- Motion masks/assets separately authored (eyes closed/open, expressions, leaf stems) instead of trying to distort a static all-in-one PNG for full facial animation.
- Deliver each stage in consistent expression set: Idle, Curious, Happy, Chain, Perfect, Sleep; prioritize 3 expressions for vertical slice; never imply all are ready when only prototype.
- Gold illumination only for key reactions; no repeated giant flash discs, no cropped leaf tips, no unreadable faces at 200px.

## 06. Animation rules / accessible mode
- Living World idle: very subtle breathing / gentle leaf drift. Touch: warm responsive anticipation, readable 0.6–1.2s reaction, smooth return. Continuous base character **never fades out**.
- Puzzle HUD: MORA only at important moments. Ordinary taps must remain silent. Full character animations cannot obscure board tile information, instruction card or input actions.
- Reduced Motion: static expression changes, no bounce, camera shake, particle drift or moving environment.
- Face transitions: guaranteed-visible base render layer, crossfade overlays, bounded cooldown. No sprite object destruction/recreation per tap.
- Animate at 60 fps target via skeletal/layer transform where practical; effect logic uses unscaled deltaTime and no per-frame heap allocations.

## 07. Habitat as visual evidence of progress
- Four actual environment states, not six glow dots: stone + water baseline; moss + flowers; waterway + recovering ruins; mature living sanctuary.
- Preserve framing and a navigable central space for MORA. Separate background / island / foreground foliage / VFX layers with transparent artwork.
- Use small environmental loops: soft ripples, glints, leaf sway; disable during Reduced Motion.
- State transitions show **what changed** on the world; design a before-after moment that a player could share without an explanatory caption.

## 08. Engineering & save model
- MORA growth belongs in a separate versioned companion domain and must not mutate BoardManager, level certification, Perfect verification or existing `SHIFT.Progress.v1`.
- First unlocks for 41+ require monotonic completion / unique rewarded milestones, not repeated replay farming. The exact source of truth must be audited once new chapter progression architecture exists.
- Current `MoraGrowthPolicy` / `MoraGrowthVisuals` are exploratory and **not approved to ship**. Remove or replace after art and chapter designs are validated.
- Teal collectible unlock, if approved, must use verified Perfect completion, once-only unlock with explicit criteria; do not silently grant or monetize it now.
- Feature gate remains OFF on main and production until asset, QA, localization and save migration tests pass.

## 09. 2027-quality gates / invisible work
**Gate A: identity** 5 independent viewers can recognize S1..S4 as the same species and distinguish adjacent phases without stage labels; pass at 128px and 200px.
**Gate B: production** exact pivot, camera, composition, skin/shading, transparent alpha, consistent silhouette and expression alignment; no jagged edges/halo.
**Gate C: runtime** Android and iOS portrait safe area, memory/texture budget profiled on a midrange target phone, no GC spikes on touch, stable frame pacing, scene enter/exit.
**Gate D: accessibility** Reduced Motion, high enough text contrast, no rapid flashes, reachability of touch target.
**Gate E: content** actual post-40 puzzles drive progression; normal success is fun without MORA, companion progression is optional and never gates solving.
**Gate F: outcome measurement** track MORA entry rate, 41+ chapter completion, next-day/7-day return cohort, progression-to-stage rate, and optional player feedback. Analytics remains consent-aware and avoids personal data where unnecessary. No retention claim without measured evidence.

## 10. Next production sprint
1. Approve a single MORA Stage I master concept (front 3/4, transparent, aligned).
2. Approve stage II–IV silhouettes in same pose and neutral studio light.
3. Deliver layered assets, three aligned expressions per stage, and four island states.
4. Build isolated Unity vertical-slice preview with explicit stage selector (Editor only), validate art gates.
5. Only after visual acceptance: wire real 41+ campaign flow, once-only rewards and verified Perfect special companion.
6. Full Unity Test Runner, Android+iOS performance passes, and qualitative playtest before main merge.

**Decision status:** The concept board and mockups are **illustrative visual direction**, NOT texture-ready sprites, animation rigs, playable growth systems or evidence that player retention will improve.
