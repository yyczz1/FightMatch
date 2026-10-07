# Exact shared-project compile supervisor

The shared project has adopted three accepted Host sources and two meta files. One future bounded Mac Editor compile must cover all 999 shared files plus one fixed existing test meta, using the existing projection/cache. The 16 overwritten, four added and 12 parked paths are restored to the original 1,008 projection inputs afterward.

Current FIX02 addresses concurrent overwrite and partial process ownership findings. Existing destinations use atomic swap: the displaced actual value is preserved and checked. A conflict gets at most one return attempt; a second drift preserves both values and blocks subsequent restoration of that path. New destinations remain exclusive. Process details are validated before an owned record is published, so failures leave no partial record and a later fresh snapshot can recover. The previous partial-write and same-snapshot exception corrections remain.

Validation: 48 actual-function cases passed on the first FIX02 round in 9.856 seconds. Cases inject concurrent values at the native-operation boundary, drift again during the bounded return, preserve foreign values even when their bytes equal a planned postimage, and recover from details failure during real closure functions with simulated signals. Darwin wrappers run against a fake ctypes boundary; real native syscalls and Unity remain unexecuted.

The original I01 and FIX01 source roots (ten files), original inputs and all old failures remain unchanged. Older commits preserve their published state. FIX01's first fixture failure and second-round pass are historical evidence, not relabeled as FIX02.

Future execution uses separate I01-run with exact reviewed files and original inputs.json, a fresh owner/activation and normal tool approval. Read execution-plan.md followed by correction-plan.md and correction-fix02-plan.md; they define the output root and finite atomic evidence slots. No run directory exists yet. Only one compile of at most 360 seconds is planned; no Play, tests, build, download or retry.

Historical cache bindings remain incomplete. Actual compilation or a complete exact source/reference/define/response/DLL reuse chain is required, otherwise INCOMPLETE without forced recompilation. This draft establishes preparation only, not native compilation, Android, localization, gameplay or Demo acceptance.
