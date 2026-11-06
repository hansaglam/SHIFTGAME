# SHIFT × MORA v0.6 — Living World Preview

## Context
The player verified v0.5 in Unity (chain/completion/Perfect body reactions), and approved continuing to a MORA home experience. This pilot remains on `feature/mora-visual-pilot-v01`, feature flag OFF by default. No changes are made to the retail `main` branch.

## What is included
- A **MORA** entry on the Chapter Select screen, enabled only if `Enable Mora Pilot` is on and `Mora_Idle` exists.
- It shares the existing Continue button row (does not add a button over puzzle pieces or the HUD).
- A full-screen, Alpine Lake-themed companion home with a larger MORA sprite, soft idle breathing, tap reaction and EN/TR text.
- A **Back to Chapters** button restores the Chapter Select screen.
- Existing MORA expression images are loaded from local `Resources/Mora`; no additional art import is required.
- Reduced Motion disables idle breathing and touch enlargement.
- Board input is modal-locked while the MORA Home is open.

## Explicitly not included
No new mechanics, growth, Flow Energy, saved pet state, garden upgrades, market, coins, monetization, daily pressure, or chapter 41. The home is an interaction test only, and tells the player in both languages that growth/rewards aren't active.

## Test
1. Close Unity. In `D:\UnityProjects\SHIFT\SHIFT-MORA`, run `git status` then `git pull --ff-only`. Do not discard the locally modified `Prototype.unity`, `PackageManagerSettings.asset` or the untracked MORA PNGs.
2. Open the `Prototype` scene, confirm `Enable Mora Pilot` is still checked.
3. Open **Bölümler / Chapters**, then tap the new **MORA** button beside Continue.
4. Verify MORA appears large, breathes subtly, responds to touch, and the greeting shows in TR/EN.
5. Press **Bölümlere Dön / Back to Chapters**, then choose a level and solve it.
6. Test narrow and tall portrait, Reduced Motion, menu transitions, no board taps through the panel, absence of sprite fallback, and no red Console errors.
7. Run `MoraReactionPolicyTests`, `MoraCompanionPilotTests`, `MoraHomePanelTests` and the project regression suite.

## Safety and scope
The pilot uses local in-memory UI state only and creates no PlayerPrefs keys. The first 40 levels and their saved progress, Perfect Shift, BoardManager, CampaignEnding and AdMob logic are unchanged. Unity compilation and device tests are **not performed** by the connector. Keep this draft PR unmerged pending validation.


## v0.6.1 — visual feedback after the first home screenshot
The user supplied screenshots proving the initial home and tap-to-happy reaction render on the scene. The screen still showed a distracting pale circular halo and tiny supporting type. Minor opt-in home-only polish:
- Subtle golden halo at 9% opacity instead of a large opaque disc.
- Soft oval base shadow, drawn behind the character, so it feels more grounded against the lake.
- Larger subtitle, greeting, supporting note and chapter-return button typography.
- Alternate cheerful and curious local reactions after taps, with matching Turkish and English messages; no new assets needed.
- Remains in-memory: no level progress manipulation, growth, currency, PlayerPrefs, store or new companion requirements.

**Local test:** pull into SHIFT-MORA after closing Unity. With the pilot enabled, open Chapters > MORA, look for softer glow and shadow, verify first and second taps produce Happy/Curious messages and sprite poses, then return to Chapters. Check narrow phone, Reduced Motion, Unity Console. Existing MORA and broader regressions should still pass. No remote Unity compile/device verification has occurred.