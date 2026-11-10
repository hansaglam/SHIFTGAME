# SHIFT — Resonance 2.0 / Chapter 4 experimental QA

**Branch:** `feature/chapter4-resonance-v2-pilot` — based on `feature/chapter4-resonance-pilot`. This is a test-only feature branch. Do not merge into main, ship, or change the first 60 levels until Unity and playtests pass. Echo and Relay were separate previous concepts and are not imported.

## Confirmed design rule for V2

- When a movable piece **enters** a Rezonans Düğümü tile, the matching remote node sends its normal-colored occupant along that occupant's own current facing direction (if movable). The triggering piece **then continues one normal move in its original direction**, with the usual chain rules applied. Subsequent direction/rotator/other Resonance tiles may prolong the chain.
- There is no teleport, no rotation and no extra player tap for the distant reaction.
- Each channel can fire **once per player tap** (max two channels). Crossing that link a second time that tap is traversable but does not emit another signal.
- No receiver or blocked receiver: the source can still continue. Pushed pieces can enter a Resonance tile and send signals, but only normal colored pieces are signal receivers.
- The pushed piece or remote chain is resolved **before** the source resumes. A cycle or overflow cancels the whole tap and restores all piece/gate state without spending a move. Undo and Restart follow original rules; all outcomes remain deterministic.

## Animation/UI

- Teal paired resonator nodes keep clearly marked matching channel badges. New single reusable link overlay draws a brief directional connection from the triggering node to its paired node before remote movement starts.
- The pulse is followed by receiver movement, then source continuation: action order is visible rather than simultaneous confusion.
- Reduced Motion: full static signal connection for two frames, without scale/translation interpolation. Existing audio/haptics are reused.
- No additional scene asset, package, runtime prefab or network dependency.

## New isolated 61–65 level drafts

| Level | Player insight | Independent-model shortest route | Normal longer winning route |
|--|--|--|--|
|61 First Signal|Two pieces finish on one tap|1|2|
|62 Quiet Shortcut|A remote pulse saves a tap|3|4|
|63 Distant Switch|Remote switch opens a gate|3|4|
|64 Chain of Signals|Two channels prepare three colors|4|5+|
|65 Remote Cascade|Push a blocked red piece into a resonator; open a distant gate|5|6+|

Note: short tutorial levels 61–63 are intentionally approachable. The interesting challenge is timing/order; Levels 66–80 will grow meaningfully harder **only after** the mechanic is accepted. The five recorded paths were checked in an independent Python BoardManager-style simulator, NOT a Unity build. Therefore the counts and paths are hypotheses until actual Unity validation.

### Validation gate — all must pass

1. Open isolated SHIFT-RESONANCE-V2 Unity worktree with Unity 6.3 LTS. Resolve compiler errors first; do not use Ignore in Safe Mode.
2. Run EditMode `ResonanceMechanicTests` (11 tests). Especially assert both pieces move after one tap, blocked receiver behavior, two channels, once-per-channel guard, Undo, canceled cycle and longer normal win.
3. Menu **SHIFT > Chapter 4 > Validate Resonance Levels 61-65**. Require `5/5 certified`.
4. Menu **SHIFT > Chapter 4 > Analyze Resonance Decision Quality 61-65**. Examine decision counts and potential trivial routes; low depth alone is not proof of quality.
5. Exit Play Mode and choose **Create Isolated Resonance Preview Scene** (recreate/Replace if an old 65-level scene exists). In Play Mode use **Resonance Preview Controls**, Levels 61–65.
6. Test moving into either paired node, empty remote node, blocked remote piece, source continuation after a pulse, 2-channel chained motion, a normal longer win vs shortest win, Undo, Restart, failed tap, Reduced Motion and EN/TR guide. Watch signal line for cropping or covering pieces in narrow aspect ratios.
7. Run all existing regressions for Levels 1–60. Profile on a real mid-range Android device and iPhone simulator/device, especially dropped frames and undesired UI clipping.
8. Review by player observation: first attempt route, number of restarts, time-to-understand Rezonans, time-to-solve, hint usage, and satisfaction of 65. Rewrite levels that feel obvious or are just visually noisy.

**Perfect Shift is not yet enabled** for these drafts. Only certify a Perfect minimum from Unity's authoritative bounded solver and bind it to the final level fingerprint after design freeze. No runtime registry writes are made by this branch.

## Post-approval direction

Level 66–70: introduce staged signals and decisions with competing orders. Level 71–75: combinations with push/gates and maybe a **secondary** Relay concept only if it survives its own playtest. Level 76–80: a demanding chapter finale. Do not add extra mechanics merely to increase level count.
