# LAYOUT-S1 source review candidate

This candidate adds the approved responsive uGUI layout, shared modal ownership, fixed actor slots and a guarded native resource migration entry. Decorative art remains obvious temporary shapes; player text remains diagnostic prefab placeholders with the existing English/Simplified Chinese bindings.

Status: **SOURCE_READY / UNCOMPILED / UNIMPORTED / OLD_PREFAB_NOT_MIGRATED / NOT_PLAYABLE_CANDIDATE**.

## Scope and evidence

- Base: PR #1 head `3541930877837e14b58020b910976b8b67b56e42`, tree `37326f759fc065f268c23f66a674be1a61f12268`. The local dirty master and PR #3 locale changes are excluded.
- 15 C# files, including two new files; 2,260 added/deleted lines. Existing builder lines remain unchanged. No Prefab, scene, meta, package or project setting was edited in this stage.
- Engineering mechanically verified the frozen patch against all 15 workspace files, 133 fixed inputs, per-file budgets and the protected scope. This is source intake, not code approval.
- [Source receipt](source-receipt.json), [static checks](static-checks.json) and [handoff](handoff.json) are byte-identical copies from `TestArtifacts/FightMatch/LAYOUT-S1-001/S/`. The receipt retains hashes of the complete local evidence, including before/after snapshots and patch. [Publication manifest](publication-manifest.json) maps every selected file.
- 141 declared TMP targets (137 bindings, 3 inputs, 1 license); 12 new test declarations. None was compiled, discovered or executed in this stage. Historical tests cover their original inputs only.

## Review context

Read the [source contract](../../team/2026-09-30/engineering-ugui-layout-source-activation-system-001.md) and its [fixed input manifest](../../team/2026-09-30/engineering-ugui-layout-source-activation-inputs-001.json). Approved layout precedence is [central adjudication](../../team/2026-09-30/central-layout-hierarchy-adjudication-001.md), [design correction](../../team/2026-09-30/engineering-ugui-layout-system-design-correction-001.md), then [base design](../../team/2026-09-30/engineering-ugui-layout-system-design-001.md). Source corrections [001](../../team/2026-09-30/engineering-ugui-layout-source-contract-correction-001.md) and [002](../../team/2026-09-30/engineering-ugui-layout-source-contract-correction-002.md) restore fixed slot paths/single scaling and two serialized title references.

Review the changed code for layout ownership, input/cancellation, safe-area scaling, stable slot identity, lifecycle cleanup, exact text binding coverage and guarded migration. The old resources intentionally remain unchanged until a separately activated native migration. Reviewers must distinguish this source-stage boundary from a completed UI migration.

After GitHub review, engineering signs the bounded import/migration/test activation against the actual candidate. Native save/reopen, visual and device playthrough evidence remain required. This PR grants no merge or release approval.
