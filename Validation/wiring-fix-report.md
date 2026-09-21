# SHIFT level wiring fix

Root cause: Unity serialized-array growth from 10 to 20 copies the last element into slots 10–19. The new regression reproduced all ten duplicate Level10 references. The previous setup assigned references only when creating a missing scene, so it could not repair an existing scene. The default Inspector allowed this resize without validation. No runtime population code was found that explicitly repeats Level10. The original saved scene was already correct at inspection; the historical in-memory edit cannot be established from disk.

ChapterLevelWiring.cs now resolves the exact ordered 20 asset paths, rejects missing/duplicate assets before assignment, and overwrites every array slot. PrototypeSetup.cs repairs and saves existing scenes as well as initializing new ones. PrototypeGameEditor.cs shows a read-only Levels array, validates it, and provides Repair and Save Chapter References. LevelValidation.cs uses the same explicit asset catalog.

Prototype.unity was saved with the reference block from the Unity-tested scene. All 20 references map sequentially to Level01 through Level20; 0=Level01_FirstTap, 9=Level10_SetTheChain, 10=Level11_OpenTheLane, 19=Level20_TheFinalShift. The block was already correct on disk, so there is no semantic scene diff. Unrelated Unity-generated scene settings were not copied.

LevelWiringTests.cs adds four tests covering saved scene count/non-null/uniqueness/exact order, fresh assignment, resize reproduction plus existing-scene repair/save/reload/idempotence, and actual completion of Level10 and Level11 followed by Next Level loading Level11 and Level12.

Full Unity EditMode suite (including tests entering Play Mode): 60 total, 60 passed, 0 failed. Unity 6000.3.23f1. Results: wiring-results.xml. Log: wiring-unity.log. No C# compiler errors or warnings. Ran in an isolated project copy.

BoardManager, PrototypeGame, all level layouts, and difficulty assets are unchanged.

If an already-open Editor retains stale unsaved references, exit Play Mode and run SHIFT > Repair Prototype Level References to repair and save that loaded instance.