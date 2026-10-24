# Warrior Pixel Battle Idle — Candidate 01

Status: `CANDIDATE / USER_SELECTION_PENDING / NOT_IMPORTED`

This is the first fully pixel-art battle actor candidate derived from the approved visual direction in the user's reference image. It intentionally avoids smooth skeletal deformation. The four idle poses are complete frame-by-frame drawings with stepped playback, following the useful constraints described in Spine's *Using Spine with Pixelart in Dan The Man*: limit separated parts, avoid mesh/weight deformation, remove interpolation, and draw important held poses directly.

## Art direction

- Identity: brown-haired young warrior, teal tunic, cream trousers, red scarf, round brown/gold shield, short sword.
- View: three-quarter battle stance facing right.
- Canvas: 128 × 128 px per frame; feet share the same baseline.
- Animation: 4 frames, 220 ms per frame, forward loop (`idle-breathe`).
- Palette: one project-owned 16-color palette; opaque pixels are binary-alpha after cleanup.
- Motion: a `0 / -1 / -2 / -1 px` vertical breathing arc moves the complete character, shield, and sword as one coherent master silhouette. The original bottom sole rows are restored in the raised frames so the feet retain ground contact. Five to nine explicitly drawn edge pixels add delayed scarf motion. No internal rectangular region is moved or cleared.
- Technique: frame-by-frame pixel poses. No paid Spine dependency is required for this candidate.

## Primary deliverables

- `warrior-idle.aseprite` — editable source, 4 frames, `Body-FBF` layer, `idle-breathe` tag.
- `warrior-idle-preview-6x.gif` — nearest-neighbor review preview.
- `warrior-idle-strip.png` — 4-frame horizontal transparent sprite strip.
- `warrior-idle-strip.json` — Aseprite frame/tag metadata.
- `character-scale-profile.json` — scale/anchor baseline for later player actions.
- `prompt-used.txt` and `raw-sheet.png` — generation provenance.
- `processed-clean/pipeline-meta.json` — extraction and QC evidence.

## QC receipt

- 4/4 valid frames.
- Empty frames: 0.
- Source/output edge touches: 0.
- Paste clamps: 0.
- Extraction body-scale coefficient of variation: `0.00763` (limit `0.08`).
- Extraction feet-anchor Y standard deviation: `0.00704` (limit `0.05`).
- Final authored source uses one locked body master frame translated through the `0 / -1 / -2 / -1 px` breathing arc; no AI-generated anatomy changes occur between frames.
- Breathing-integrity regression check: pass; every opaque master-frame body pixel exists at the expected shifted coordinate in frames 1–4.
- Ground-contact check: pass; opaque sole pixels remain on the master ground line in all four frames.
- Alpha discipline check: pass; exported frames contain only alpha `0` or `255`, with no semi-transparent edge pixels.
- The earlier scarf-only version is retained as `warrior-idle-scarf-only.*` for comparison.
- Final palette use: 16 colors per frame.

## Scope boundary

This candidate has not been copied into `Assets/`, imported into Unity, wired to Animator/uGUI, or declared final. User visual approval comes before gameplay integration or production of additional action sets.

