# Chapter 3 — Safe Unity Playtest

1. Check out branch feature/chapter3-levels-41-50 in a separate Unity worktree/project.
2. Open Unity 6.3 LTS and verify the Console has no compiler errors.
3. Run SHIFT > Chapter 3 > Validate Draft Levels 41-50 and the Editor test Chapter3DraftTests.
4. Run SHIFT > Chapter 3 > Create Isolated Preview Scene. This duplicates Prototype.unity as Chapter3_DraftPreview.unity, and only the new copy gets draft Levels 41–50 and isolated save prefixes.
5. In that copy, enter Play Mode, then SHIFT > Chapter 3 > Preview Controls. Select 41–50 and click Load Draft Level.
6. Compare solutions, restart/undo, readability, hint and chain payout. Do not publish preview scene.

Important: the original Prototype.unity is not overwritten. The main game still exposes only the original 40 levels. The preview tool is only compiled in Unity Editor and uses PrototypeGame's debug LoadLevel route to avoid updating player progression. Review before merging into a shipping build.
