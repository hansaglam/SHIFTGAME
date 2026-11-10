# SHIFT — Chapter 4 / Rezonans Vadisi — mechanic pilot v0.1

Status: **prototype only** in `feature/chapter4-resonance-pilot`. No release or mobile build is claimed. Branch starts from Chapter 3 Levels 41–60, *not* from the rejected Echo/Relay pilots. Original 1–60 assets, MORA and release scene are unchanged.

## Final proposed rule to test
- Two teal Resonance nodes with the **same channel symbol** form a pair; up to two pairs per board.
- When a piece enters one node, a signal reaches the other node.
- If a **Normal colored piece** is **currently standing on the receiving node**, that piece attempts its own movement in its **existing direction**. It can push, hit switches, enter another Resonance node or exit normally.
- The triggering piece remains at the node it entered. No teleportation, rotation, random effects, resource consumption or extra tap cost for remote movement.
- A paired node pulses **once per channel per committed tap**; the guard resets at the next tap or after Undo. If the distant piece cannot move, it stays where it was. A missing receiver still produces an understandable visual ping.
- A blocked/cyclic overall reaction is rolled back atomically using the existing BoardManager snapshot and reaction cap; Undo/Restart use the original game's systems. No new persistent save schema.
- A secondary Normal piece may be moved independently by a normal tap; players can win less efficiently. Certified *shortest* sequences will eventually qualify for Perfect Shift, not simply authored knownSolution.

## Draft level learning arc (solver prototype, NOT certified by Unity)
| Level | Prototype title | Shortest recorded route | Intended puzzle |
|--|--|--|--|
| 61 | First Signal | 2 taps | Learn that A triggers a piece waiting on B |
| 62 | Quiet Shortcut | 3 taps | A normal four-tap win versus a three-tap linked win |
| 63 | Distant Switch | 4 taps | Remote piece opens a gate for itself |
| 64 | Chain of Signals | 3 taps | Two channels cause a three-color relay reaction |
| 65 | Remote Cascade | 4 taps | Push a non-objective piece into a node, activate a distant piece and open a gate |

These routes have been inspected in an independent behavioral model matching the existing BoardManager logic. **This is not proof of Unity compilation nor final level quality.** Unity CampaignCertification, automated editor tests and real playtests are mandatory before acceptance.

## Safe QA sequence
1. Create a separate worktree from `feature/chapter4-resonance-pilot`. Do not overwrite SHIFT, SHIFT-MORA or SHIFT-RELAY.
2. Open in Unity 6.3 LTS. If Safe Mode appears, use it and capture all Console compilation errors.
3. Run Test Runner > EditMode > `ResonanceMechanicTests`. Require green tests, especially pulse/Undo/two-channel/longer-win.
4. SHIFT > Chapter 4 > Validate Resonance Levels 61-65. Require `5/5 certified` with actual LevelValidation and CampaignCertification before accepting content.
5. Exit Play Mode, then SHIFT > Chapter 4 > Create Isolated Resonance Preview Scene. This copies the locked 40-level Prototype.unity and attaches all 65 references to **only** the new test scene. Preview save prefixes are isolated.
6. Press Play, SHIFT > Chapter 4 > Resonance Preview Controls; load 61–65. Check a small phone, TR/EN tutorial, Reduced Motion, first signal clarity, chained pulse, Undo, Restart, blocked receivers, performance and Console exceptions.
7. Run existing regression tests. Compare each candidate's comprehension, shortest solution, normal longer route, visual reaction payoff and real solve time. Reject or rework dull or ambiguous boards. The five introductory levels are not a complete Chapter 4.
8. Keep experimental levels out of VerifiedOptimality and all production Build Profiles until final fingerprints/certificates are approved. No speculative Perfect Shift awards.

## Future roadmap (not implemented)
- 66–70: two-signal staging, blocked target and switch/gate interactions with 5–8 meaningful decisions.
- 71–75: combine *approved* Resonance with existing Push/Direction/Rotator; consider using the separate Relay concept as a supporting mechanic only after playtests.
- 76–80: a chapter finale that visibly activates the valley through chained reactions, while keeping deterministic puzzle gameplay central.

Review gate: after testing, player enjoyment decides whether Resonance stays or is rejected. No changes to released SHIFT are implied.
