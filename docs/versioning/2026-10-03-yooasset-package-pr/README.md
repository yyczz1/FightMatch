# Fixed YooAsset package candidate

Proposes the exact two UPM-generated package files from the isolated P02 probe: YooAsset 3.0.6 at Git revision 3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804 and official Scriptable Build Pipeline 1.21.25. All 51 previous node objects remain equal; only two nodes are added. Preserve the generated lock's key ordering rather than rewriting it manually.

The request succeeded once. P02 then ran a separate compile that exited zero but logged CS2001 for a removed transient installer, so its strict compile gate remains NEEDS_FIX. The log subsequently rebuilt the compiler graph and emitted four formal assemblies. Approved P03 will only validate a stable no-helper compile using the installed graph; it does not repeat installation. This draft does not claim stable compile, shared-project adoption, Android or resource loading acceptance.

The native input was the fixed PR3 base plus PR5 Host isolation and PR6 resource contracts, followed by these two package changes. The publication base is PR6 at d7775a1134556698a6489029b20402a0ce185939; the probe is a recorded composite, not a direct test of that Git head. Shared Packages remain unchanged locally.

The JSON and archived installer here are byte-identical P02 audit files. The installer is outside Assets, is not a product component, and must not be executed from this archive. Source/registry distribution hashes, the package-local Apache-2.0 license identity, formal assembly identities, the unimported Samples~ assemblies and the failed Git LFS call are retained. No LFS pointer was found among 2070 resolved package files; do not claim the LFS command succeeded. This records provenance, not release compliance approval.

The original RES-A scope is supplemented by the isolated P01 and P03 scopes included in this change. Their independent-projection boundaries supersede the old shared-adoption/rollback sequence for this probe. Full logs, package graph and prior P01 failure remain local; no license-client logs, caches or generated Library files are uploaded. The publication manifest binds every selected path and preimage.

Audit erratum: the archived import-assemblies.json wording that the runner attempted to load every asmdef is inaccurate. It enumerated metadata only; the strict compile-log gate stopped execution before the DLL gate. No Samples~ DLL check was actually executed. The archived JSON remains unchanged.
