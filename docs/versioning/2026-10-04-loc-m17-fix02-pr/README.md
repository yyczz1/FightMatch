# LOC M17 FIX02: command timer, cached-input preflight, and central host template

This is the current PR #2 source candidate, based on `837dd4e39683548ee23dce5f5228074ec87231e5`. Earlier candidates remain unchanged historical evidence. It does not declare materials or game localization ready.

- `fix02/` contains the eight exact delivered source/model/evidence files (9,374,532 bytes). Driver SHA-256: `e2da00b70058835c2e268c42451f27125f1ba2f7eb6deb9a5ab4d01547699b70`; source seal: `3eebabf254cb2e3fd39e45757551e396d8db2613ee03b9eebc100a1652892d7b`; model: `318a02fc4930c4d896aebbe8a2db910300009289e5596fe8ebee7f8980d82a38`.
- `host-template/` contains the exact default-disabled 56-line central startup template and its delivery receipt (29,199 bytes). Template SHA-256: `b60dcc0521d0f3465c39a012a5502f05ece36aa06614781ee32ef1746055ebea`. It is separately reviewed, outside the eight-file source seal.
- Scope: [timer correction](../../team/2026-09-30/engineering-loc-m17-timer-shadow-fix02.md) and its overriding [P2/host amendment](../../team/2026-09-30/engineering-loc-m17-fix02-review-host-amendment.md).

## Problem and resulting behavior

A writer-rule loop overwrote the MStage timer with a string, causing the first real command to fail. Renaming that loop field preserves the original timer. Separately, GitHub review 5406086277 raised P2 r4177549312: a corrupt cached archive could be discovered after an earlier missing archive had already been downloaded. Every present official-get archive is now checked through the original safe length/hash reader before creating the material writer or issuing the first GET. Post-download checks remain.

The driver delta is 22 added and 15 deleted lines, including necessary fixed-input metadata. Approved tools, versions, licenses, 900-second M-only budget, 18 fixed archive bodies (25,172,764 bytes), three restores and separate B/L gates are unchanged.

The new host template keeps all execution logic fixed and `CONFIG = None`. Future central configuration may change only its single literal data block. The signed data, original logic bytes and normalized AST must be checked against this reviewed template before a separately authorized run. It obtains no pin/config from CLI, environment or stdin. No production activation or configured host is included here.

## Validation and limits

- Two old failures reproduced with in-memory substitutes, total 0.069461625 seconds. No real GET, process or material action occurred.
- Two targeted green runs each passed 18/18; cumulative 0.205527167 seconds. The second strengthened fixture call evidence without changing the source. These cover the original production wait/receipt functions, cache preflight ordering and host gates. Old 73-check evidence was preserved and not rerun.
- An initial metadata normalization assertion failed; correcting the scope-check script passed. The failure remains in the evidence.
- One source generation/seal/disk-consumption operation completed in 4.564795 seconds, rebuilding the model from actual disk without an expected-model shortcut. Final identity check: 0.055049416 seconds.
- Engineering matched the ten files, seven seal identities, full patch, model digest and six original tool outputs. This was mechanical receipt, not another code review or test run.
- Author turn `01a106e6-15b0-75a3-a1dd-b5b3108744d8` ended and stopped writing. Its 1,384.540 seconds include implementation and reasoning; they are not test runtime.

Production pin is null; material/work roots and real activation/host are absent. No download, unpacking, restore, dotnet, Unity or material execution occurred. Independent GitHub review of the new head is required before real M preparation. This remains a draft source checkpoint, with no merge or release.
