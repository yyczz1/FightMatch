# M16 FIX01 source correction

This is the current source-only successor to the immutable M16 archive on PR2 at bb642ab3897fa587b1f41f8298533229874f9617. The current driver is source/m16_driver.py in this directory. Earlier drivers, receipts and failures remain historical evidence.

The correction addresses GitHub findings r4163732952, r4163732956 and r4163732959: compare actual control output bytes/semantics/canonical publication; require the complete frozen deployment and package closure; reject caller-invented execution authority.

The author reproduced all three old false positives, then passed 46 offline checks (18 output, 11 deployment, 14 authority, 3 regression) within 0.446052 seconds across two validation attempts. The six original files remain byte-identical. No Unity, dotnet, restore, network, child execution, production writes or Git writes occurred in that task. The archive copies all eight final leaves without rewriting their bytes.

This candidate is deliberately SOURCE_ONLY. The production execution path stays closed without an independently pinned central activation; the synthetic positive authority case tests only the verifier and does not launch a process. Actual materialization requires a separately reviewed trusted binding, intact material inputs and activation. A previously referenced temporary M09 material source directory is now absent; this source-only preparation did not re-establish all material availability. Passing fixtures is not material or runtime acceptance.

The preparation contract and original GitHub findings are included under docs/team/2026-09-30. Publication-manifest.json records exact outgoing blobs and preserves all existing base files. GitHub review of this successor remains required; no local duplicate code review was performed.
