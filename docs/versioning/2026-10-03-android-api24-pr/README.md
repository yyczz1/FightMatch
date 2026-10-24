# Android 7 minimum: independent source candidate

The project setting and the existing shared QA/normal build gate now require Android API24. The build report gains an integer minSdk field populated from PlayerSettings. Target API32, version, ABI, graphics, signing, output and restore behavior retain their original bytes.

- Base: reviewed PR1 head `3541930877837e14b58020b910976b8b67b56e42`, with the 83,965-byte build script. The separate 160,371-byte shared working file is untouched and is not included.
- Two logical files, four exact substitutions/additions, +4/-3 lines. The final build script is 84,070 bytes and ProjectSettings is 23,595 bytes.
- FIX03 completed one bounded static invocation: 40/40 checks, all three in-memory negative cases rejected, exact patch replay and byte/budget guards passed. Checker time was 0.011674375s; subprocess time 0.035297042s. No Unity, C# compile, Android build or device test ran for this candidate.
- Previous checker failures remain recorded: an unrelated before digest, an incorrect prior-file digest plus extra final LF copies, then bytes/str mismatch in patch replay. FIX03 restores exact original-candidate bytes, generates identity records from bytes, and decodes the replay input as strict UTF-8. The four intended product changes did not expand.
- In source-receipt.json, source_inventory.git_blob_sha1 denotes the baseline blob. This publication manifest separately records the outgoing source blobs.
- SOURCE_READY is the current stage. Shared integration, Unity save/reopen at API24, compilation, two fresh QA/normal APKs and their manifest values remain required. This checkpoint is not full API24, Google Play or Demo acceptance.
- Selected evidence is archived here; the original complete local stages and their failures remain unchanged. Independent GitHub code review follows publication.

The separate resource-as-factory-acceptance bundle records completed PR13/14 evidence without changing those already-reviewed heads. Its 40 native tests are independent of these 40 static source checks.
