# Fixed cloud resource-test projection

FIX01 includes the exact current Mac validation projection: 1036 shared source/configuration files plus the one existing P01 LocalePreferenceStoreTests.cs.meta sidecar. The complete 1037-file / 32658037-byte identity is a27ee9a2192ea7a4fecf1ca85e3dbbe6cb011dc08a0c9cb6983da70192a6578a. source-manifest.json separately preserves the original shared identity and identifies the 243-byte supplement, its source and original GUID.

This addresses GitHub PR21 P1 r4213654050. The first snapshot omitted that sidecar, which would cause Unity to generate a new GUID on import. FIX01 preserves the exact already-existing Mac projection GUID d85e93281a75342ac8d13378ff40cdba; no new GUID, game code, test assertion, local product file or dependency version is introduced. The prior 6359bf36 snapshot remains in Git history and was never imported on the cloud host.

PR19 is the adoption base, not an exact snapshot of this validation projection. Ten file identities differ and four historical Host-test leaves are absent relative to it. The complete delta is in the manifest. This is a test-input snapshot, not a proposal to remove product tests or merge over newer Host work. The exact 89-resource-case selection remains unchanged.

The cloud shell and existing Unity2022.3.18f1/.NET SDK8.0.425 entries are available. The first source-fetch attempt failed because the configured network proxy could not be reached; no projection was created, and the old repository remained unchanged. Fixed YooAsset/SBP dependencies and current licensing are still unverified. No cloud test/import/install occurred. A separate bounded synchronization/preflight remains necessary; this publication authorizes no native run.

Historical Mac V01/V03 supervisor failures and V02 preflight stop remain recorded. Linux EditMode evidence cannot establish Mac filesystem, Player, gameplay, visual or device acceptance. This draft is not a merge/release candidate.
