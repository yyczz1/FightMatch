# Adapter FIX01

Fixes the two GitHub findings on original head `5e70957df7db1caab56e3908621a295bcb58a1ce`: a later explicit retry must restart failed initialization or manifest selection, and invalid release-set diagnostics must return the canonical bounded reason. Only provider, lifecycle and provider-test code changes; existing contracts, assemblies and remote-services source are preserved. Three focused regressions are added and all original 31 test names remain.

The fixed candidate passed 111 mechanical checks. Its 34 test names are a static candidate list; corrected compilation and behavior are still pending at publication. The old candidate compiled and passed 31/31 in M01, but those tests did not cover the two review defects. The original source receipt, review findings and M01 receipt are preserved rather than relabeled as fixed-source validation.

This correction also preserves twelve naturally generated Unity metadata files: six from M01 for the adapter, and six previously accepted asset-contract metadata files. Exact identities and original evidence are in metadata-receipt.json. No author-generated GUIDs, Host/locale metadata, shared checkout changes, package upgrades or runtime-resource acceptance are introduced.

Original-author scope is `docs/team/2026-09-30/engineering-res-b-adapter-fix01.md`. New-head GitHub review and one corrected native compile/focused run remain independent gates. Real built-in object loading, raw resources, Host/Android/device acceptance and shared adoption remain later work.
