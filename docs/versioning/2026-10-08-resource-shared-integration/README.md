# Shared resource modules: source adoption

This checkpoint brings the already reviewed resource contracts, release-set codec, local activation store, YooAsset provider/lifecycle and runtime factory into the shared FightMatch source tree. Runtime JSON parsing and resource-admission rules remain the accepted module implementations. Runtime UI remains uGUI; no new gameplay, save or locale behavior is added here.

The package manifests pin YooAsset 3.0.6 to commit 3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804 and retain Scriptable Build Pipeline 1.21.25. Exact source and naturally generated Unity metadata come from the accepted packets listed in the two adoption plans. No dependency resolver, Unity process or download ran during byte adoption.

Three mechanical batches adopted codec prerequisites, G01 contracts/activation and G02 SDK/provider/factory. Final shared Assets/Packages/ProjectSettings inventory: 1,036 files, 32,657,794 bytes; canonical SHA256 61f1364be566b5266468ea38a19caba1d172428dc7e5fda06b97398c89c83ba8. Only the 39 resource-related product paths are included in this PR. The base retains its Host validation fixtures; this branch is not claimed to be byte-identical to the whole shared checkout or to be build-verified.

Validation is SOURCE_ADOPTED_COMPILE_PENDING. Historical module reviews and fixed-combination tests remain valid only for their original inputs. A new combined 89-case compile/test contract is being prepared; it has not run. The G02 preflight lacks exact timestamp and process-row count in its recorded evidence, retained as unknown rather than reconstructed.

Review the assembled source/dependency references and preserved metadata against the accepted packet versions. Formal localization materials, actual resource-pack creation/download, production Host initialization, Android/Windows/device and full Demo acceptance remain outstanding. This draft is not merged and does not authorize release, deployment or paid services.
