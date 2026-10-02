# FIX01 — first-create snapshot expectations

The first real 18-case run passed 16 cases and failed two: the first locator read acquires a writer lease and leaves a normal zero-byte locator/writer.lock after disposal, but those two new tests expected no new file. The original failed run is retained at TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/M01 (receipt SHA-256 f799fa8ca9a369d82c9030d34db2334c871c623b05fc96287d57a4f5ca2b4d6e).

Changes one existing test file only (35 additions, 3 deletions): two failing methods and their required private helpers. The corrected expectations permit only the precise owned non-link zero-byte lock and its locator directory. Original file bytes, directory contents, settings-only preconditions and all 16 unknown-entry variants remain checked. Fourteen other test methods and production code are unchanged.

The author preserved an initial static patch-serialization failure; the corrected patch then passed 77 mechanical checks. Engineering verified the exact one-file patch and fixed inputs. C# compilation and the complete 18-case run remain pending for this correction, as does matching-head GitHub review. The separate HostRig generated-output declaration is handled by the scoped Mac verification packet; it is not a production change.

The JSON files here are byte-identical source-stage receipts. Scope: docs/team/2026-09-30/engineering-host-settings-first-create-fix01.md. Full source/patch/checker/before/after evidence remains in the FIX01/S local folder. Prior source-stage and failed test evidence are preserved.
