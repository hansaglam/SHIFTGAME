# SHIFT Sprint 4 delivery

## A) Created files
- Tests/Editor/ChapterTwoPresentationTests.cs
- Tests/Editor/ChapterTwoTests.cs
- Tests/Editor/SwitchGateTests.cs
- Data/Levels/Level21_FirstSwitch.asset
- Data/Levels/Level22_OpenForRed.asset
- Data/Levels/Level23_ToggleTwice.asset
- Data/Levels/Level24_ClosedLane.asset
- Data/Levels/Level25_BeforeYouGo.asset
- Data/Levels/Level26_ThreeEntries.asset
- Data/Levels/Level27_BoxDoesNotPress.asset
- Data/Levels/Level28_TurnThrough.asset
- Data/Levels/Level29_ClockworkGate.asset
- Data/Levels/Level30_ClearThenOpen.asset
- Data/Levels/Level31_OnePadTwoGates.asset
- Data/Levels/Level32_OppositeStates.asset
- Data/Levels/Level33_SharedDeliveryGate.asset
- Data/Levels/Level34_ClearTheCorridor.asset
- Data/Levels/Level35_TwoChannels.asset
- Data/Levels/Level36_CrossBeforeClosing.asset
- Data/Levels/Level37_OpenTheSecondPad.asset
- Data/Levels/Level38_LiftAndTurn.asset
- Data/Levels/Level39_PrepareToggleRedirect.asset
- Data/Levels/Level40_TheStateOfShift.asset

New asset/script files include Unity metadata. Offline authoring data: Validation/author_chapter2.py and chapter2-design.json. Validation reports and portrait captures are retained in Validation.

## B) Modified files
- README.md
- Editor/ChapterLevelWiring.cs
- Editor/LevelValidation.cs
- Editor/PrototypeGameEditor.cs
- Scenes/Prototype.unity
- Scripts/Board/BoardAction.cs
- Scripts/Board/BoardManager.cs
- Scripts/Board/BoardView.cs
- Scripts/Core/GameEnums.cs
- Scripts/Levels/LevelData.cs
- Scripts/Levels/VerifiedOptimality.cs
- Scripts/Pieces/Piece.cs
- Scripts/Systems/AnalyticsService.cs
- Scripts/Systems/AudioManager.cs
- Scripts/Systems/LevelProgression.cs
- Scripts/Systems/PrototypeGame.cs
- Scripts/Systems/SaveService.cs
- Scripts/UI/ChapterSelect.cs
- Scripts/UI/GameHud.cs
- Tests/Editor/ChapterLevelTests.cs
- Tests/Editor/LevelWiringTests.cs
- Tests/Editor/ProgressionPresentationTests.cs

## C) Switch/Gate rules

Normal pieces activate switches on entry (including pushed normals), never by starting there. Boxes do not activate. One entry toggles every gate on its channel. Channels 1/2 use single/double diamonds. Closed gates block entry; open gates pass both movable types. A gate closing around an occupant does not remove it; it can leave. A push that closes the destination gate underneath the pusher cancels and rolls back the complete reaction with no move cost. Switches do not automatically propel pieces.

## D) Simulation

BoardManager was extended, not rewritten: authoritative GateOpen, snapshot restoration, closed-gate checks, deterministic channel toggles and three appended action kinds. No analytics/UI/audio dependency in simulation. Meaningful SwitchActivated/GateOpened/GateClosed records count toward reaction depth. Validation rejects orphan/invalid channels, duplicate terrain, unsupported metadata and initially closed-gate occupancy. Old assets remain compatible.

## E) Levels 21-40

| Level | Name | Recorded taps |
|---|---|---:|
| 21 | FirstSwitch | 3 |
| 22 | OpenForRed | 3 |
| 23 | ToggleTwice | 4 |
| 24 | ClosedLane | 4 |
| 25 | BeforeYouGo | 5 |
| 26 | ThreeEntries | 5 |
| 27 | BoxDoesNotPress | 5 |
| 28 | TurnThrough | 3 |
| 29 | ClockworkGate | 3 |
| 30 | ClearThenOpen | 5 |
| 31 | OnePadTwoGates | 4 |
| 32 | OppositeStates | 7 |
| 33 | SharedDeliveryGate | 5 |
| 34 | ClearTheCorridor | 5 |
| 35 | TwoChannels | 5 |
| 36 | CrossBeforeClosing | 6 |
| 37 | OpenTheSecondPad | 6 |
| 38 | LiftAndTurn | 5 |
| 39 | PrepareToggleRedirect | 6 |
| 40 | TheStateOfShift | 7 |

21-26 introduce activation, separate-piece control, parity and route preparation. 27-30 mix boxes, direction/rotator and lane clearing. 31-34 use linked gates and shared corridors. 35-37 introduce two-channel ordering; Level36 wrong-order dead end has a regression. 38-40 combine preparation, toggles and redirection. Level40 is 6x6, seven verified-minimum taps, two activations, three gates, one box, Direction and Rotator, with a reaction depth >=6. All 40 known solutions win.

Verified minima: Level30=5, Level34=5, Level37=6, Level40=7. Certificates are bound to exact puzzle data including channels/open state. Other recorded solutions are not claimed optimal. No runtime solver.

## F) Progression / migration

Two compact 20-level chapter pages; Chapter2 locks until Level21 is unlocked. Level20 -> Level21 works; Level40 shows CHAPTER 2 COMPLETE with no Next. Existing PlayerPrefs keys are retained. Old completed 19/19/19 frontier migrates to unlocked/current20, completed19; earlier replay selections survive. Incomplete saves do not unlock Chapter2. Migration is persisted/idempotent and no player reset is performed. Scene saved with exactly 40 non-null unique ordered references; repair assigns every slot explicitly and Inspector resizing remains disabled.

## G) Telemetry

Schema version2 adds switchActivations/gateOpens/gateCloses. Only committed actions counted; local and no-op providers tested. Existing attempt/session/Perfect Shift semantics remain. No SDK or online transfer.

## H) Visual / audio

Gold pads, channel diamonds and distinct closed/open gate surfaces use runtime-built UI. Switch badges remain visible above movable occupants and never intercept taps. Existing short pulses provide activation/toggle feedback without added waits or polling. Optional null-safe SwitchActivate/GateOpen/GateClose slots, Impact haptic on switch. No production sounds/native plugins. Chapter tabs retain the existing visual family.

## I) Verification

Final Unity 6000.3.23f1 full suite: **117 total / 117 passed / 0 failed**. No C# compiler errors or warnings. All 75 prior cases retained, with wiring size and Chapter1 finale expectations adapted to the requested expansion; 42 new cases added. Coverage includes switches, gates, unsafe rollback, occupancy, restart, malformed data, deterministic solutions, bounded minimum searches, wrong-order consequence, migration, chapter tabs, Level20->21, Level40 finale, Perfect Shift, telemetry and null audio.

All original level asset/meta hashes match the pre-sprint baseline. Delivered C# source text (newline-normalized) and all level assets match the tested copy. Independently resolved all 40 scene GUIDs against numbered asset metadata. No external SDK/package installation or platform build occurred. Final screenshots inspected: Level40 start with visible occupied-switch badge; chapter selection and finale also inspected in first run. Tests render real Unity UI offscreen, not physical-device screenshots.

## J) Manual Unity steps

Open SHIFT > Open Prototype, use portrait Game view, Play. Old completed Chapter1 saves continue at Level21. If an already-open scene retains its old array, exit Play and use SHIFT > Repair Prototype Level References. Chapter2 tab unlocks through progression; Editor Unlock All Levels is available for QA. SHIFT > Validate Chapter 2 Levels checks solutions. No other manual wiring required.

## K) Limitations

Subjective difficulty, switch/gate readability and tactile comfort still need player/device testing. Occupied gates permit exit and block entry; there is no crushing or automatic displacement. No automatic deadlock detector, undo or new hint system. Production audio remains unassigned; native haptic intensities remain unsupported. No SDK, monetization, online system or Android/iOS build/signing work.

## L) Recommended Sprint 5

Focused Chapter2 playtest and local telemetry review, especially Levels32,36,38-40, before tuning or adding mechanics. Sprint5 has not started.
