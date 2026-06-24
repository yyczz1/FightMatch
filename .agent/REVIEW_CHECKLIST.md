# Codex Review Checklist for External DeepSeek Patches

Codex must inspect the actual patch and repository state. Do not accept the worker's summary as evidence.

## Required inputs

- Approved TASK_PACKET.
- DeepSeek response.
- Unified diff or modified workspace.
- Verification output.
- Current repository state.

If any required input is missing, return `NEEDS_FIX` or `REJECT`; do not infer the missing patch.

## 1. Scope gate

- [ ] Every changed file is listed by the worker.
- [ ] Every changed file is allowed by the task packet.
- [ ] No forbidden file changed.
- [ ] No unapproved file was created.
- [ ] Changed-file and diff budgets were respected.
- [ ] The patch performs only one coherent task.

Any hidden or unexplained changed file is a review failure.

## 2. Requirement alignment

- [ ] Required behavior is implemented.
- [ ] Every acceptance criterion has evidence.
- [ ] Non-goals remain untouched.
- [ ] The patch does not silently reinterpret ambiguous requirements.
- [ ] No architecture or product decision was invented by the worker.

## 3. Simplicity and design

- [ ] No unnecessary abstraction, interface, helper layer, configuration, or extensibility was added.
- [ ] No speculative future feature was introduced.
- [ ] Existing project structure and boundaries are preserved.
- [ ] The implementation is the smallest maintainable solution for the task.
- [ ] No unrelated refactor or optimization is mixed in.

## 4. Diff hygiene

- [ ] No unrelated formatting changes.
- [ ] No unrelated comment rewrites.
- [ ] No unrelated renames or import reordering.
- [ ] Existing line endings and style are preserved.
- [ ] Only dead code created by this patch was removed.

## 5. Protected-change audit

- [ ] No unapproved dependency or package was added.
- [ ] No unapproved lockfile change.
- [ ] No unapproved configuration or ProjectSettings change.
- [ ] No unapproved CI or build-script change.
- [ ] No unapproved public API change.
- [ ] No unapproved serialized format, persistence, migration, asset, `.meta`, or generated-file change.

If a protected change was necessary but not authorized, reject the current patch and create a new task packet that explicitly scopes the decision.

## 6. Correctness and compatibility

- [ ] Existing behavior outside the goal is preserved.
- [ ] Edge cases required by the packet are handled.
- [ ] Error behavior is deterministic and actionable.
- [ ] Threading and lifecycle rules are respected.
- [ ] Public API and compatibility impact were checked.
- [ ] Serialization and persistent-data impact were checked.
- [ ] Unity runtime/editor assembly boundaries are respected when applicable.

## 7. Validation evidence

- [ ] Build/compile verification was run when applicable.
- [ ] Relevant tests were run.
- [ ] Test output contains no failures.
- [ ] The worker did not substitute an unrelated test.
- [ ] Manual validation was performed when required.
- [ ] Skipped validation has a concrete, truthful reason.
- [ ] Codex independently reran or inspected sufficient validation.

See [`VALIDATION.md`](VALIDATION.md).

## 8. Rollback decision

- [ ] The patch can be reverted as one task.
- [ ] Contaminating unrelated changes are identified.
- [ ] Any portions that must be discarded are listed by file/hunk.
- [ ] A failed patch will not be repaired through uncontrolled scope growth.

## Review verdict

Codex must return exactly one:

### ACCEPT

Use only when:

- scope is clean;
- acceptance criteria are met;
- verification is sufficient;
- no unresolved material issue remains.

Required output:

```text
VERDICT: ACCEPT
Scope: PASS
Acceptance criteria: PASS
Verification: <evidence>
Notes: <concise residual risk, or NONE>
```

### NEEDS_FIX

Use when the approach is usable but a bounded correction is required.

Required output:

```text
VERDICT: NEEDS_FIX
Issues:
1. <specific issue with file/line or hunk>
Required correction:
- <bounded change>
Do not change:
- <protected scope>
Verification required:
- <exact verification>
```

After this verdict, Codex creates a smaller corrective TASK_PACKET. Do not return the original broad packet unchanged.

### REJECT

Use when the patch is unsafe, substantially out of scope, architecturally wrong, unverifiable, or easier to replace than repair.

Required output:

```text
VERDICT: REJECT
Reason:
- <specific reason>
Discard:
- <files/hunks>
Preserve:
- <safe work, or NONE>
Next step:
- <new planning decision or replacement task>
```
