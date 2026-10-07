# Combined resource validation — FIX02

The current resource/Host combination needs one bounded compile followed by the fixed89-case EditMode run. The supervisor uses the existing P01 projection, recoverable file operations and a fresh external Bee cache. Native execution has not started.

FIX01 reserved cleanup time inside the900-second cap. FIX02 addresses subsequent GitHub P1s4212896382/4212896391: filesystem iteration and bounded read/hash chunks now check deadlines internally, and final evidence/receipt/output has30 seconds reserved inside the same total. The prior90-second closure and60-second restoration ceilings remain. Preparation carries the old120 forward plus at most40 for this correction; shared1036/P01 proposed1037/89-case content is unchanged. inputs.json differs only in two preparation-limit fields and the new finalization reserve.

Receipts/stdout use AWAITING_PROCESS_EXIT for candidate success. Acceptance additionally requires validationStatus TEST_PASS, no failure, exact receipt identity, real OS exit0 and an external monotonic elapsed measurement plus preparation<=900. Evidence/receipt/output timing boundaries are explicit; serialization/hash/write/flush overrun cannot return success. Cooperative checks cannot interrupt a kernel I/O stall; that limitation is retained rather than claiming hard real-time guarantees.

Validation: targeted red reproduced both issues; the second/full round passed158/158, preserving138 previous expectations. Two rounds total10.451651417 seconds. The current preparation ceiling is160; all V00/V01 successes and failures remain unchanged. Author actual01a1189d-7536-7522-8802-9f1b5b8588e0 completed. Central verified30 fixed references, retained-function ASTs, exact diff and the three-field input amendment without rerunning tests or doing a duplicate code review.

preparation.json contains the exact V01-to-FIX02 diff and completion-observation contract. Original FIX05 reference files remain for context; the previous FIX01 candidate is preserved at parent0e83cf66e5167cbc54207b7efbc9525df5d35e99. No Unity, real ps/signals, cache/projection writes, downloads or activation occurred. The158 checks are offline script checks, not the Unity89-case run.

New head requires independent GitHub review before C activation. Native compile/tests, formal localization, real download/Host, Windows, Android/device and Demo acceptance remain pending. Draft only; no merge/release/deployment.
