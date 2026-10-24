# Resource combined validation — V04 source candidate

Current runner/checker/inputs/preparation belong to RES-COMBINED-V04. V03's real import exited0, but the supervisor failed on a Bee IPC special entry and three natural-closure deadline errors; T89 never started. V03 remains FAILED with its independent QA receipt and full restoration evidence preserved. V01 FAILED and V02 NOT_RUN_BLOCKED are also retained.

V04 only adds precisely bound Bee socket handling and distinguishes normal natural-grace expiry from genuine in-flight timeout/I/O/global-budget failures. It preserves protected product, package, test, transfer, restore and process-exit contracts, while binding fresh roots and the newly observed preserved Tundra state. See v04-design.md and v04-source-packet.md; preparation.json contains the reconstructible data patch and exact source diff.

Fresh V04 validation: 63 affected/new offline cases passed in one5.420996-second round. The historical V02 177-case checker/results are retained as v02-replay-check.py and v02-replay-results.json; they are reused within the documented unchanged categories, not reported as a new V04 177-case pass. V03-reviewed-runner.py is the exact reviewed parent source. Central mechanical comparison confirms80 unchanged functions, six modified functions, seven added definitions and146 reconstructible input changes; no local duplicate code review or native run occurred.

V04 has no activation or Unity/test-stage execution. Exact-head GitHub review and a fresh, separately scoped C activation are required before one bounded I→T89. Source/replay review cannot establish gameplay, Player, device, visual or Demo acceptance. Historical plans and verdicts in this folder retain their original scopes; they are not current run authorization.
