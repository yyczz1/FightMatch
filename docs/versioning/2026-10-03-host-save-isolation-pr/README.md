# Host save isolation source checkpoint

The Host can select an explicitly activated, Editor-only acceptance save root without touching the normal player root. Invalid markers, activation records, overlapping or linked paths, and concurrent leases fail closed. Normal startup with no marker keeps the existing production path and I/O order.

This draft contains two product source files. It is based on locale-preference PR #3 at `6818405b859d71bf8c0f116299e7e9605a3d769a`, because one test uses its existing explicit-root preference store. No locale-preference dependency is copied as a new change. The shared checkout and frozen layout candidate remain unchanged.

C delivered 261 changed Host lines and 261 test lines. Engineering checked the exact two-path patch replay, eight staged leaves, 1089 shared leaves, 103 protected records, and the ten FIX04 leaves. The original mechanical checker recorded one directory-location failure, then passed 194 static assertions after its own path correction. These assertions are Python source/evidence checks, not 194 executed game tests.

`source-receipt.json` and `static-checks.json` are byte-identical selected originals. The complete eight-leaf delivery remains in the external-disk workspace at `TestArtifacts/FightMatch/HOST-SAVE-ISOLATION-001/S/`; before/after inventories, patch, and checker are deliberately not duplicated in this small review checkpoint. `publication-manifest.json` maps staged source bytes to canonical PR paths.

Not run: C# compilation/import, NUnit discovery or the six HISO tests, real Host/G3, devices, and Demo acceptance. The new test meta is intentionally absent until a scoped natural Unity import. GitHub Code Review is a separate pending gate at publication. Real localization composition and the settings/EmptyRoot compatibility issue remain separate work.
