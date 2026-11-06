# MORA Living Seed — Master Asset Intake and Quality Preflight
**Date:** 2026-11-06  
**Status:** Gate A / asset intake, not production art acceptance.

## What is now implemented
Unity Editor: `SHIFT → MORA → Validate Living Seed Sprite Production`.

This scanner never modifies files. It checks the new primary Living Seed character only, with stage-prefixed asset naming:
- `Assets/_Game/Resources/Mora/MoraSeed_S1_Idle.png`
- `MoraSeed_S1_Curious.png`, `MoraSeed_S1_Happy.png`
- Repeat S2/S3/S4 for the same three expression names (12 total).

The tool checks Sprite/Single import, actual source alpha + alpha transparency setting, dimensions (1024px+ and square recommended), compression/mipmaps for review, consistent pivots and consistent image canvas sizes. It reports details in Console. Stage I Idle is required to pass first master-art intake; missing remaining assets are marked pending, not fabricated.

The earlier preview runtime path `MoraSeed_Idle.png` is a temporary compatibility alias, **not the final stage-prefixed contract**. No runtime swap has been made. This check will not produce or approve artwork, alter the older teal collectible preview, or change puzzle gameplay.

## Production gate before Unity runtime integration
- One original Stage I Living Seed master, isolated with alpha, consistent with the user-supplied fifth concept image.
- Face spacing, rounded silhouette, green layered leaf cap, golden bud design, shading/light and material treatment validated at 128px and 200px.
- A consistent front 3/4 camera and aligned bottom-centre pivot that remain reproducible for all stages.
- Adjacent stage forms visibly differ in silhouette, not only effects or flowers.
- Separate layers are retained in an editable source file when eyes/leaves/buds require independent animation.
- No cropped/upscaled presentations are accepted as runtime sprites.

## Current limitation
Image generation attempts in this session returned illustrated *presentation boards* rather than standalone isolated master character assets. Those outputs are **rejected as production sprites**. No unverified generated art was committed to GitHub or installed into SHIFT.

The new validator is committed only to `feature/mora-visual-pilot-v01`; Editor compilation, scanning and device tests still need to be performed in Unity. Keep the PR unmerged.
