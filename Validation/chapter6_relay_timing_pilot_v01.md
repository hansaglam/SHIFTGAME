# SHIFT — Chapter 6 RÖLE / Waiting Move Pilot v01

**Status:** isolated playable-source candidate, **not Unity-certified and not release-ready**.  
**Date:** 13 November 2026  
**Branch:** `feature/chapter6-relay-timing-pilot-v01`  
**Base:** `feature/chapter5-level81-100-design-v01` (draft Chapter 5)  
**Decision:** Tek Geçiş / One-Use Crossing is **not implemented**; closing access after use would resemble the existing Gate. Rejected BAĞ PR #4 remains closed and unmerged. This is a clean alternate mechanic.

## The mechanically distinct decision

| Existing system | What the player controls | RÖLE difference |
|---|---|---|
| Switch / Gate | A switch toggles whether a passage is traversable | **No passage ever opens/closes**; Relay has no channel, initialOpen or lock |
| Resonance | Entering one linked node instantly moves another piece at its paired node | **No paired nodes**; one local hourglass stores a stone's next move for a later tap |
| Rotator / Direction tile | A stone's facing or continuation changes immediately on entry | A stone **waits** for a separate successful tap, then moves in its own current direction |
| BAĞ | Distance constraints and physical pull between two stones | **No bond, connector line, distance tracking or overlay** |

### Exact rules

1. A Normal entering `PieceType.Relay` (one compact hourglass tile) stops there and gains `RelayReady`. The matching small hourglass badge is displayed only while that Normal waits. Existing push/resonance dynamics still apply.
2. The waiting Normal **cannot be tapped directly**. After the player successfully taps a **different** Normal, the tapped piece's full ordinary reaction resolves first, then each previously queued Normal advances **one legal move** in deterministic placement order, in its own stored direction. A Relay entered on the same tap cannot fire early.
3. A blocked tap (zero BoardActions) does not release waiting pieces and spends zero moves. A delayed move that hits a wall/exit rule/blocked piece remains queued, without falsely spending a second player tap.
4. A delayed Normal can push, deliver, activate switches or trigger Resonance just like an ordinary moving Normal. Leaving its Relay clears its waiting state. Entering a subsequent Relay arms it for another turn.
5. The tap and all resulting reactions remain one deterministic transaction. `RelayArmed` and `RelayReleased` are appended BoardAction types after the existing enum values. Undo restores waiting state; Restart reloads it; PuzzleBatchAnalysis hashes the waiting bit.
6. RÖLE is purely opt-in. No existing Level1–100 asset uses the new type; no new Perfect Shift certificates are registered and no old fingerprint format is changed.

### Small proof-of-concept levels (3; no gate or switch on ANY board)

| Level | Hypothesis | Authored winning taps | Length |
|---|---|---|---|
| 101: The Waiting Move | Queue red with hourglass, blue's tap releases red, then finish both | (0,1) → (0,2) → (2,1) → (1,2) | 4 |
| 102: One Tap, Two Paths | Queue red; yellow's one tap delivers yellow, releases red, **pushes blue into an exit** | (0,1) → (0,2) → (2,1) | 3 |
| 103: Delayed Triple | Queue red; a single blue tap triggers Resonance/yellow, blue delivery AND delayed red delivery | (0,1) → (0,2) | 2 |

Level101 also has a four-tap alternative with a different order; Level102 has a four-tap alternative. Level103 intentionally prioritizes one striking two-tap three-delivery chain rather than a padded alternate.

These authored routes were independently replayed against a separate action model. **Their shortest lengths are not Unity-certified until `CampaignCertification.Prove` runs in Unity**.

### Appearance and UX

- A **single cyan hourglass inside a small navy tile** identifies RÖLE, without gate posts, channel diamonds, switch gold plates, moving board-wide link lines or other visual clutter.
- A tiny non-raycasting hourglass badge marks a currently waiting colored Normal. Badge appears after `RelayArmed`, disappears after release, and restores after Undo.
- Level101 and the first-encounter guide explicitly explain the **two-stage interaction** in TR and EN. Level102 asks about a useful second-piece push, and Level103 combines RÖLE + Resonance as a payoff.
- No long-press, swipe, drag, user account, currency or extra controls.
- **RÖLE should be rejected** if an actual player still confuses it with a gate or cannot predict which move will release the wait.

### Unity testing

The isolated editor commands are:

- `SHIFT → Chapter 6 RÖLE → Validate 101-103`
- `SHIFT → Chapter 6 RÖLE → Create or Repair Preview`
- `SHIFT → Chapter 6 RÖLE → Preview Level 101` (and 102/103)

Run these **outside Play Mode**, then press Play for an actual 1080×1920 Game view. The generated scene is `SHIFT_Relay_101_103_Preview.unity` (ignored from Git and never added to build scenes). The developer-specific save prefix is `SHIFT.Preview.Chapter6Relay.v1.`; an Editor-only resolver restores only any missing 101–103 references in that 103-slot preview and is not in a player build. Normal player saves and shipped scenes are untouched.

The 103-level preview, **and only that preview**, shows a sixth navigation tab `6 · RÖLE` / `6 · RELAY`. Level103 is *not* presented as a completed twenty-level Chapter 6 finale.

### Regression and release barriers

Authored `Chapter6RelayPilotTests` and extended existing selector tests check: no switches/gates in pilot boards; waiting stone not manually tappable; next successful other tap releases it at no additional tap cost; blocked other tap does not release or spend a move; Undo and exact action replay; yellow-triggered delayed red push/blue delivery; RÖLE + Resonance three-colored delivery; authored winning route and exhaustive minimum proof; 1–100 unmodified references; TR/EN; and isolated preview reference recovery.

**Tests are written but have NOT been executed in Unity in this session.** Source-level independent replays are not Unity validation, device screenshots or enjoyment proof. Before any Level104–120 authoring, require: successful Unity compilation, 3/3 editor certification, a clear screenshot without visual obstruction, correct reduced-motion semantics, and manual gameplay confirmation that the delayed decision feels novel rather than an awkward pause.

Mora and holistic blind player tests remain at the user's approved later phase, but **mechanic-level compile and regressions must be resolved now**.

## Safe branch contract

`feature/chapter6-one-use-bridge-pilot-v01` remains a design-only historical proposal; **do not merge it**. `feature/chapter6-tether-prototype-v01` / PR #4 remains a closed rejected experiment; **do not merge it**. This RÖLE pilot is based directly on Chapter5, not on either rejected mechanical branch. No changes to Level1–100 .asset files, saved-game schema, MORA, ad systems or `Assets/_Game/Scenes/Prototype.unity`.
