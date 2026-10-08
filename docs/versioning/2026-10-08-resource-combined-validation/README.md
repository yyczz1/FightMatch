# Resource combined validation V02

Current candidate corrects two issues observed in the first native V01 attempt: CoreCLR diagnostic IPC was rejected by the existing TMP guard, and SDK CacheWrite telemetry was counted as a second Csc action. V02 sets DOTNET_EnableDiagnostics=0 only for the I/T child environment and distinguishes exact CacheWrite telemetry. Unknown IPC protections stay unchanged.

- V01: one Unity compile process exited0, supervisor exited1; T89 never ran. External elapsed87.088998s plus160 preparation=247.088998s. Original FAILED, recovered1035 projection inputs/43 compiler leaves, and original evidence remain immutable.
- V02: targeted red followed by177/177 offline checks, retaining all158 previous cases;12.600547s total. No new Unity/native execution or activation.
- Current source is runner.py/replay-check.py; inputs, replay-results and preparation contain exact seals, input mapping and delta from v01-reviewed sources. v02-design/source-packet govern this correction. Earlier design/fix documents and FIX05 baseline files remain historical context.
- New V02 run/cache/AS paths are independent. Shared1036/projection1035→1037,53 packages,43 compiler leaves and89 cases are unchanged. Overall900s includes160 preparation, reserved cleanup/restoration/finalization; no retries.
- Candidate success remains provisional until exact receipt hash, TEST_PASS/no failure, real OS exit0 and external monotonic total verification.

GitHub review of the new head is required before fresh C activation. No game-source changes, PR merge, deployment, Android/device or Demo acceptance.
