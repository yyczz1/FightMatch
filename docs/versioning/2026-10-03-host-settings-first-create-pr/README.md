# First-profile creation with existing locale settings

Allows first-profile creation when the same persistent root already contains the locale store's own settings files. Their bytes are preserved. Unknown settings entries, links and pre-existing player data still block creation; settings are checked again before writing a profile.

Source scope: two existing C# files, 32 production lines and 267 test lines added. Seven new test methods cover first creation, owned preference residues, unknown data, links, recheck, interrupted creation and reopening. Original tests and assertions remain.

This is an uncompiled, untested source checkpoint. The author ran 55 mechanical Python assertions once; engineering separately checked exact patch replay and fixed inputs. This does not inherit the prior 17-test result as behavioral evidence for the changed HostSession.

Base: PR #5 at a8b9758ed2db1a43dac9328f162433a4523624c9. GitHub code review and a focused 18-case run remain required. No shared-source, Scene, package, metadata, player-save or device write occurred. The full Host resource-state directory contract and real Demo startup remain pending.

The scope document is docs/team/2026-09-30/engineering-host-settings-first-create-001.md. The two JSON files here are byte-identical source-stage receipts. Full before/after snapshots, patch and checker remain under TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/S locally. publication-manifest.json maps uploaded paths to exact bytes and Git blobs.
