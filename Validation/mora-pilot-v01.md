# SHIFT × MORA visual pilot — v0.4.1 (visibility test)

## Observed issue
The player imported the five high-res expression PNGs and confirmed the MORA character appeared **very small** next to the instruction card. In v0.3, the MORA dock was only 15.6% of screen width. The large transparent border around the 1254 px PNGs compounded the problem.

## Safe changes in this branch
- The opt-in preview dock now occupies x: **0–22%**, y: **13.5–27.7%** of the safe area.
- This increases the effective sprite display without changing gameplay board or HUD text layout.
- Use the supplied `SHIFT_MORA_v04_Framed_Sprites.zip` containing five **individually rendered** expressions, trimmed to the same 972 × 1150 frame, preserving pixel resolution and transparency. This only removes external transparent padding, not body details.
- `Mora_Idle.png`, `Mora_Curious.png`, `Mora_Chain.png`, `Mora_Happy.png`, `Mora_Perfect.png`. The current pilot does not invoke Curious yet.
- These image files remain **local assets**; they are not committed to this branch. Missing optional sprites still fall back to Idle.
- Normal taps remain silent. Chain, Complete and verified Perfect keep the previous triggers and durations.
- The pilot remains **OFF by default** and does not modify BoardManager, saves, 40 levels, chapter ending, ads or economy.

## Local test (Unity)
1. Close Unity. In `D:\UnityProjects\SHIFT\SHIFT-MORA`, run `git status` then `git pull --ff-only`. Do NOT reset or delete the local modified `Prototype.unity` and `PackageManagerSettings.asset`.
2. **Replace** local five MORA sprite PNGs with the framed versions from the supplied ZIP under `Assets/_Game/Resources/Mora/`. Back up the previous set first.
3. Unity: Texture Type `Sprite (2D and UI)`, Sprite Mode `Single`, Preserve Aspect enabled through UI Image, Max Size **1024** for first quality inspection; disable compression if color/alpha edges show artifacts.
4. Reopen Prototype, confirm `Enable Mora Pilot`, enter Play mode, test a strong chain and a normal/Perfect completion.
5. On 9:16 and taller portrait test aspect ratios, check legibility of instructions, no occlusion of pieces/controls, and menu/modal stacking. Reduced Motion should remain static.
6. Run `MoraCompanionPilotTests`, `MoraReactionPolicyTests` and existing regression tests.

## Exit criteria
MORA's face and flow core should be readable without hiding the board or text. If the overlay still competes with gameplay, stop enlarging it and relocate larger interactions to a dedicated Living World view.

This is a **visual pilot, not production-ready**. Remote GitHub edits were not Unity-compiled or device-tested. Do not merge until local tests pass.
