# Fixed cloud resource-test input

This branch snapshots the exact 1036-file shared Assets/Packages/ProjectSettings candidate already used by the Mac combined resource validation. Its canonical identity is 61f1364be566b5266468ea38a19caba1d172428dc7e5fda06b97398c89c83ba8 (32657794 bytes). It does not change the local product checkout or introduce new implementation behavior.

PR19 is the source-adoption base, not an exact snapshot of the current shared candidate. Relative to that base, ten file identities differ and five historical Host-test leaves are absent. source-manifest.json lists every identity and the full comparison. These differences restore the exact already-sealed test input; they are not approval to remove tests from the product or merge this snapshot. The unrelated Host tests are outside the fixed 89-resource-case selection, and the production assertions in this snapshot are unchanged from the sealed local input.

The cloud workspace currently has the old clean 125b849 baseline. Its shell and installed Unity2022.3.18f1/.NET SDK8.0.425 are available, but YooAsset3.0.6 and SBP1.21.25 are missing and current Unity licensing is unverified. This publication alone authorizes no environment changes or test run. A separately scoped task must sync the exact commit, verify the manifest, prepare fixed dependencies and check current licensing before executing the exact test-cases.json selection.

Historical Mac V01/V03 supervisor failures and V02 preflight stop remain failures/stops. Linux EditMode evidence cannot establish Mac filesystem, Player, gameplay, visual or device acceptance. This is a draft test-input checkpoint, not a merge/release candidate.
