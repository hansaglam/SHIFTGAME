# SHIFT: Chapter 3 — Canlanan Hatlar (41–60)
This is a design-authoring kickoff, not a claim of playable new levels. Levels 1–40 remain locked and unchanged.

**Goal:** address real feedback that 40 handcrafted levels finish very quickly. Add 20 designed levels in a coherent new region; certify the first 10 (41–50) before working on the next ten. Prioritize meaningful decisions and satisfying chains rather than filler or repeated levels. Don't stall level production waiting for fully rigged MORA art.

| Level | Working name | Design problem | Combined existing mechanics |
|---|---|---|---|
| 41 | İlk Kıvılcım | New-chapter chain prediction | rotator + direction + exit |
| 42 | Kapıyı Erken Açma | Gate timing and wrong order | switch + gate + wall |
| 43 | Dar Su Yolu | Clear a shared corridor | push block + rotator |
| 44 | İki Yön Bir Karar | Sequence alters outcome | direction + gates |
| 45 | Sonraki Hamle İçin | Setup instead of immediate exit | rotator + switch |
| 46 | Yansıyan Yol | Non-obvious path choice | two objectives + direction |
| 47 | Paylaşılan Eşik | Space and timing | push block + gate |
| 48 | Sessiz Zincir | One significant deep cascade | gates + rotator + color |
| 49 | İki Anahtarın Sırrı | Track two channels correctly | channels 1 & 2 |
| 50 | Suyu Uyandır | Chapter milestone synthesis | multicolor + gates + rotator |

51–60: deeper creative recombinations, a distinct new system only after deterministic-rule tests, an impactful chapter finale and a visible habitat milestone.

**Real authoring requirements:** create unique `Level41_Name.asset` etc in `Assets/_Game/Data/Levels/Chapter3_Drafts/`; record actual winning KnownSolution. New read-only menu `SHIFT > Chapter 3 > Audit Level 41-50 Drafts` runs playable validation, BoardManager replay and minimum-move certification. Keep `ChapterLevelWiring`, campaign ending, SaveService and all first 40 untouched until content is certified and migration/continue logic is tested. Real player test must measure completion time, retries, hints, fail rates and perceived novelty; no invented retention claims. Stage unlocks only after actual Level41+ content exists.

## MORA visual test
The accepted 4 forms were supplied as a single 2172×724 RGB checkerboard-printed image. An **art-only Editor prototype** package converts them to four grounded composite RGBA sprites `MoraScene_S1..S4`; masking artifacts and resolution mean **not production art**. With all four imported, the existing MORA home displays those composite sprites and hides old separated layers in Editor, fades stage images in 0.38s and provides 1.2% tap feedback; Reduced Motion removes movement/fade. A single composite cannot support proper independent facial/leaf motion. Real rig, S1–S4 face set, phone profiling, and binary sprite merge are pending. The main branch and published game are unchanged.
