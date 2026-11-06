# SHIFT — Companion Identity Decision v1.0
## Approved direction from player feedback (2026-11-06)

The four-stage v0.7 preview was rejected: it showed nearly the exact same teal creature and tiny cosmetic glints at each stage. It did not communicate development, ownership, or a changing environment.

### PRIMARY — MORA: Living Seed
Source of truth: the **fifth image supplied by the player**, the previous "MORA — Yaşayan Tohum" concept board. MORA is a small rounded cream/pale seed creature with a broad green leaf canopy and warm luminous sprouts, with a welcoming and curious personality. This is the evolving companion and the face of the new Living World experience. Avoid replacing it with a teal triangular form.

Visual growth needs four **genuinely distinct, high-quality, consistent original assets**, not just four progress labels or floating circles:
1. Awakening: small seed with a first leaf, basic home in mossy ruins.
2. Sprout: larger leaves and more animation/expressions, moss and spring flowers spread.
3. Growth: visible canopy and first blossoms, living water/ruins brighten.
4. Full Form: warm luminous blossoms and a recognizable, mature leaf crown; habitat becomes more lush.

Keep identity consistent (eyes, leaf motif, silhouette family) even though the form genuinely changes. Final design naming and exact milestones need art and player validation before release.

### SPECIAL — existing teal/yellow companion prototype
Existing Unity teal triangular sprite with golden curled sprout and diamond core is **not MORA's growth stage**. Preserve art and tested tap/chain presentation as a *candidate collectible surprise companion* (codename optional). A future verified Perfect Shift milestone after Level 40 can grant a permanent collection unlock. The unlock must be earned only once, locally persisted without repeatedly farming replay rewards, and must not replace the main seed progression. No rewards are wired yet.

### Current branch behaviour after this correction
- The misleading v0.7 UI (four identical teal stages, TEST +, progress decoration) has been removed from the home panel by restoring the last tested v0.6.2a layout.
- Dormant `MoraGrowthPolicy` / `MoraGrowthVisuals` C# helper files remain on the branch but are **not wired to the UI or gameplay**; they are provisional and MUST NOT be treated as an approved progression design.
- MORA Home now chooses `Resources/Mora/MoraSeed_Idle.png` if present, plus optional `MoraSeed_Happy.png`, `MoraSeed_Curious.png`. This primary art is **not yet delivered**. Until then, the existing `Mora_Idle/Happy/Curious` previews continue working but their panel is explicitly labeled **Special Companion / Özel Karakter · Test**, not MORA.
- Chapter entry is no longer dependent on the teal cameo asset; a future primary sprite alone can enable the home.
- The `MoraCompanionPilot` puzzle cameo is still the teal test prototype and should not ship before the reward design is implemented.
- Feature flag `Enable Mora Pilot` remains OFF by default and no changes have been merged into `main`.

### Next implementation order
1. Build & review a **production-quality master sprite** for the leaf-seed MORA, based on user-supplied concept board; no cropping/upscaling concept artwork.
2. Approve four distinct silhouette-based growth stage sprites and appropriate habitat transformations.
3. Import seed Idle/Happy/Curious and link stage visuals; remove fallback special preview from primary home for release.
4. Validate editor and device presentation/animation, then build separately versioned growth persistence and special-character collectible unlock based on verified future Perfect Shift milestones.
5. Add chapter 41+ content and connect true campaign progress. Existing levels 1–40, their saved mastery and campaign finale remain unchanged.

No Unity compilation, Play Mode verification or mobile device check was possible in this GitHub change. Do not merge the draft PR.
