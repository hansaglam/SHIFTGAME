# SHIFT × MORA visual pilot (v0.1)

This branch is an **opt-in presentation experiment**, not a finished companion system.

## Asset preparation
- Import a *transparent, original* MORA front portrait at `Assets/_Game/Resources/Mora/Mora_Idle.png`.
- Set **Texture Type: Sprite (2D and UI)** and Sprite Mode: Single.
- The sprite is intentionally *not bundled* in this PR; the draft extract supplied separately is only for testing and needs a production-quality replacement.
- Without this sprite the preview component does not appear.

## Enable
In the existing gameplay scene, select the `PrototypeGame` component and enable **Enable Mora Pilot** (default: OFF).
Play a puzzle, trigger a chain, complete a level, and compare Perfect Shift / ordinary success. The MORA portrait appears briefly, without receiving input.
Reduced Motion disables the bounce.

## Regression gates before any merge
- Open with the flag OFF: no UX changes.
- Play standard and Daily puzzle; verify win/lose, Undo, Restart, Hint and input lock.
- Test portrait HUD and Safe Area on narrow screens; the provisional anchor may need tuning.
- Confirm Level 40 finale, its Seen save flag, 40 existing certified levels and consent / ads are unchanged.
- Run editor tests including `MoraReactionPolicyTests`; run full Unity regression and Android/iOS device checks.
- **No Unity or device compilation was performed by this change.**

## Future (not in this PR)
MORA home / Living World, character interactions, multi-frame animations, persistence, Flow Energy, cosmetics, chapter 41 and economy.
