# Resource activation source checkpoint

Adds an internal, bounded store for resource activation state under an explicitly supplied product root. Prepare and restart requests remain separate from entering code and marking business readiness; uncertain atomic promotion does not advance the in-memory state or authorize player-save rollback.

- Parent: PR #7 at `4e10b0aa1e4c691d5d4d72577959d6bf1c4a7754`.
- Two new source files: 404 production lines and 511 test lines, with eight ordinary AS cases covering state transitions, strict records, failure injection and real filesystem behavior.
- The original contract conflict is preserved. The approved clarification permits a consumed restart record to coexist with the next prepared update while retaining identity checks.
- FIX02 changes only the Mac real-symlink test helper (+31/-4 lines). The fixed Unity runtime lacks File.CreateSymbolicLink; the replacement uses the existing project test-only native pattern. Production and the other seven test bodies retain their bytes.
- FIX02 source checks: 60/60 in one run. Original receipts retain their historical uncompiled/untested statuses. M02 subsequently compiled successfully and passed AS8 as part of the exact 40-case composite; the two new metadata files were naturally generated. Selected native evidence is in `../2026-10-03-resource-as-factory-joint-m02/`. Independent QA and GitHub review remain separate gates.
- This does not connect the Host, expand first-profile root admission, load/delete resources, alter player saves, prove process-restart or power-loss durability, or establish Windows/Android acceptance.
