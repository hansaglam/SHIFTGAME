# Chapter 3 Unity certification report — review guide

Run SHIFT > Chapter 3 > Export Quality and Perfect Candidate Report.

This writes Validation/chapter3-unity-candidate-quality-report.json with the authoritative Unity BoardManager-based minimum, exact board fingerprint, reaction depth, meaningful decisions, efficiency decisions, solution count estimates and normal vs Perfect discovery probabilities.

**Critical:** This is evidence for review, not an automatic gameplay unlock. The exporter never changes VerifiedOptimality.cs, player saves, ChapterLevelWiring or any LevelData asset. Any validation failure blocks that level's certification. Only add the final fingerprints to the runtime Perfect registry after 20/20 proof pass and hands-on QA. Do not report a Perfect Shift when VerifiedOptimality.Get is null.

The numerical probability metrics come from enumeration under a uniform-choice model of committing taps, not real player retention or success rates; compare them directionally and validate with playtests. Do not treat a large reaction depth by itself as quality.
