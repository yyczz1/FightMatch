# Fixed RAW/PREF native evidence

M02 ran the exact 128-case composite once: B38, RAW FIX02 13 and PREF FIX01 77 all passed, with no skipped or inconclusive case. NUnit execution was 13.6220662 seconds; test supervision was 177.248871 seconds and the runner completed in 178.401252 seconds. The fixed reviewed source heads are RAW `1b4e091526444822e487240cd0d6aff736f0080c` and PREF `229fdc604841e5efbbf490cbcc47b7dfc39b8db9`.

M01 had compiled and discovered all 77 runnable PREF cases, but its launch failed in UTF 1.1.33 logging when the filter contained literal JSON braces. M02 reused those inputs and discovery, changing only the filter's escaped-brace representation to regex hex escapes. No source/meta or test case changed, and no import/discovery was repeated.

The M02 execution receipt remains BLOCKED_OR_FAILED because a pre-existing external ADB log appended 36 bytes. Independent QA verified the complete original 1396-byte prefix and the sole append, plus unchanged source/meta/DLL/result identities and closed task-owned processes. The actor is unknown. Central accepts these implementation slices and behavioral evidence with that exact log-only variance reconciled; this is not a passing overall execution/protection receipt or a broad TMP exemption.

All JSON/XML copies retain their original bytes and historical head/status fields. Source associations are explained by the central receipt. Real package/download/Host/Scene/Android/device and full Demo acceptance remain outside this result. No merge or shared-project adoption is implied.
