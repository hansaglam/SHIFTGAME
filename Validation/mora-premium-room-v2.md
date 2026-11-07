# SHIFT × MORA Premium Room — v2 integration (2026-11-07)

Status: code committed in feature/mora-visual-pilot-v01. Unity Editor compilation and device tests remain unverified. Do not merge to main yet.

## Target reference
The user-approved portrait design features a dramatic Alpine Lake environment, cream and gold borders, teal glass controls, a selected MORA companion slot with future locked slots, an animated living seed on a tiny island, a warm speech ribbon, four-stage growth, Feed/Play/Decor, Growth/Collection, and Back to Chapters.

## Implemented in native Unity UI
- Dedicated MoraPremium asset paths with imported UI sprites and individual icons; no screenshot underneath fake hotspots.
- Four separated living-seed character art sprites and four separate islands aligned through a consistent silhouette base. When premium pack isn't present, the existing composite images are used as a fallback.
- Stage 1 Happy/Curious overlay anchored above an always-visible base. .28-second touch rate limit, breathing + scale reaction, star glints, and synchronized island/character stage crossfade.
- Reduced Motion removes breathing/glint drift and makes stage transitions immediate; facial pose may still change.
- Main room remains interactive. Feed and Play demonstrate simple session-only interactions. Decor, Growth and Collection remain truthful about features and unlocks not yet shipped. Locked new companions are not awarded.
- All previous gameplay logic, BoardManager, first 40 levels, Perfect Shift, save keys and ads remain untouched.
- Original teal experimental pet remains separate from primary MORA.

## REQUIRED binary asset package
The image files are delivered separately because the GitHub connector was used for text source updates; they were NOT committed to the branch.
Download SHIFT_MORA_PremiumRoom_v2_Assets.zip from the companion ChatGPT message.
Copy the package folder Assets/_Game/Resources/MoraPremium into the Unity project at
D:\UnityProjects\SHIFT\SHIFT-MORA\Assets\_Game\Resources\MoraPremium.
Keep the old Resources/Mora assets. After the branch update, Unity's MoraPremiumAssetImporter ensures the correct Sprite and border settings for the newly copied files. If assets were imported before the script compiled, use Assets > Reimport on the MoraPremium folder.

## Combined acceptance test
1. Close Unity; git status and git pull --ff-only on SHIFT-MORA. Don't hard-reset local scene changes or the untracked image files.
2. Open Prototype, Enable Mora Pilot. Go Chapters > MORA.
3. Confirm icon/button alignment and legibility on 1080x1920, tall phone and small phone preview. MORA's feet should sit on the island; no duplicate island art or subtitle overlap.
4. Tap MORA 15+ times; Happy/Curious shows without any blank frame. On Stage1, the face overlay uses original transparent expression sprites.
5. Feed and Play toy controls and visuals must respond, then return to MORA. Decor/Growth/Collection open properly without false purchases.
6. Editor-only four-form preview may be selected in Growth; in a non-Editor build they stay locked. Check island and avatar change together. Return to Chapters and solve a level.
7. Toggle Reduced Motion and EN/TR; verify no continuous motion in reduced mode and readable labels. Check Unity Console for errors.
8. Unity Test Runner: MoraPremiumIntegrationTests, MoraCompanionHubTests, MoraCompanionPilotTests, MoraGrowthPolicyTests and existing regressions.
9. Profile midrange Android and iOS release builds for memory/frame stability. No background game-side updates should occur when the companion UI is closed.

## Remaining release gates
The source art is illustrative, and Stage2–Stage4 lack individual Happy/Curious/Perfect sprite states and leaf/eyelid rigs. Movement is currently procedural body/room animation, not full skeletal animation. The premium room remains a vertical slice, not a claim of fully production-tested character art, economy, real pet care or 41+ growth. These missing systems must be separately approved and built before final release.
