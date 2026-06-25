# Planning and Task Lifecycle

This file defines general workflow. It is not a product roadmap.

## 1. When Codex must plan

Create an explicit plan before work that:

- affects multiple modules;
- changes architecture or public APIs;
- changes persistence, serialization, dependencies, packages, build, CI, or assets;
- requires more than a few related file changes;
- has ambiguous acceptance criteria;
- includes migration or compatibility risk.

A trivial, isolated edit may use a short inline plan, but still needs acceptance criteria and verification.

## 2. Requirement-to-plan process

Codex:

1. reads `AGENTS.md` and relevant context;
2. inspects current files and existing conventions;
3. identifies assumptions and unresolved choices;
4. states goal, non-goals, constraints, and risks;
5. defines observable success criteria;
6. maps affected files and protected boundaries;
7. sequences tasks by dependency;
8. gets user approval before complex implementation.

Do not plan from filenames alone. Do not claim existing behavior without reading it.

## 3. Splitting a plan for DeepSeek

Each external-worker task must:

- produce one coherent result;
- have a narrow file whitelist;
- include its directly related test;
- be independently reviewable;
- avoid requiring architectural judgment;
- fit within a small diff budget;
- leave the repository in a coherent state.

Prefer:

```text
one behavior + its tests
one validator rule + its tests
one data object + serialization test
one bounded UI component
```

Avoid:

```text
implement the whole feature
refactor a subsystem and add behavior
fix all failing tests
clean up the architecture
```

High-risk generator, solver, threading, serialization, and UI lifecycle tasks should be split more aggressively than simple DTO work.

### Grouped delivery

Codex may place multiple approved packets into one task group when:

- packets have an explicit execution order;
- dependencies between packets are documented;
- each packet remains independently reviewable and revertible;
- the group has an integration verification step;
- a failure policy identifies which later packets must stop.

Recommended group size:

- low-risk DTO and pure utility work: `2-4` packets;
- ordinary deterministic algorithm work: `2-3` packets;
- persistence, Editor UI, threading, or solver work: `1-2` packets.

Do not group unrelated modules merely to reduce handoffs.

When local worker commits are authorized:

- one packet equals one commit;
- use the exact commit message from the packet;
- never amend or squash;
- never push;
- report the start commit and every resulting commit hash.
- stop changing the repository after returning the group report; late commits
  require a new corrective packet.

The tracked `.claude/settings.local.json` file is a special user-managed local
permission state. User-approved permission additions may remain as an
uncommitted change throughout a group. Workers must exclude it from commits,
and Codex ignores only this exact file when evaluating group cleanliness.

## 4. Allowed and forbidden files

Codex derives file scope from:

- current call graph;
- assembly/module boundary;
- approved design;
- tests required to prove behavior.

Rules:

- list exact paths;
- separate read, modify, create, and forbidden lists;
- everything not allowed is forbidden;
- permit new files explicitly;
- name protected files even when obviously forbidden;
- include a maximum changed-file and approximate diff budget.

## 5. Acceptance criteria

Criteria must be observable and binary where practical.

Good:

```text
Given seed 123 and the same config, two calls return deeply equal paths.
Validator rejects a diagonal step with error code InvalidAdjacency.
No file outside the whitelist changes.
```

Bad:

```text
Generation works well.
Code is clean.
UI feels good.
```

Include negative criteria for prohibited behavior when risk is high.

## 6. Verification steps

Verification must identify:

- exact command or manual procedure;
- expected result;
- required environment;
- known reasons it may not run.

Do not ask DeepSeek to run broad expensive validation when a focused test proves the task.
Codex runs broader integration verification at checkpoints.

## 7. External worker handoff

Codex produces:

1. the fixed `DEEPSEEK_WORKER_PROMPT.md`;
2. either one completed task packet or one task-group manifest plus its packets;
3. only necessary file excerpts or repository access instructions.

The user manually sends them to external DeepSeek.
Codex does not invoke or monitor the external model.

## 8. Patch review

When the user returns a patch:

1. compare changed files to the whitelist;
2. inspect the full diff;
3. check protected changes;
4. evaluate correctness and simplicity;
5. run focused validation;
6. run integration validation when warranted;
7. use `REVIEW_CHECKLIST.md`;
8. return `ACCEPT`, `NEEDS_FIX`, or `REJECT`.

For a group, Codex first returns a verdict for every packet, then a group
integration verdict. One failed packet does not invalidate earlier independent
accepted commits.

## 9. Failed patches

### NEEDS_FIX

Use for a bounded repair.

Codex creates a smaller corrective TASK_PACKET containing:

- exact defective file/hunk;
- expected correction;
- files that must not change;
- focused regression test;
- exact verification.

Do not ask DeepSeek to “try again” with the original broad packet.

### REJECT

Use when the approach is unsafe or scope contamination is extensive.

- discard the unsafe patch or specified hunks;
- preserve only independently verified work;
- revisit the plan if architecture was misunderstood;
- issue a replacement task only after scope is clear.

## 10. Task records

When task packets begin to be used, store approved packets under:

```text
.agent/tasks/<TASK-ID>.md
```

The task file should record:

- task packet;
- worker status;
- Codex verdict;
- verification evidence;
- accepted commit hash when Git becomes available.

Do not create task records for speculative work.

Suggested statuses:

```text
DRAFT
APPROVED_FOR_WORKER
WORKER_RETURNED
NEEDS_FIX
REJECTED
ACCEPTED
```

## 11. Context and token control

- DeepSeek receives one task, not the full roadmap.
- Link stable repository rules instead of repeating them.
- Paste only relevant source excerpts when repository access is unavailable.
- Keep worker responses patch-first.
- Summarize verification rather than pasting full logs unless failure details matter.
- Reuse verified context from `PROJECT_CONTEXT.md`.
- Split a task when its packet requires several unrelated context sections.
- After acceptance, retain the task record and discard conversational repetition.

## 12. Rollback and integration

Preferred workflow:

1. clean baseline;
2. one branch/worktree per task;
3. external worker produces an uncommitted patch;
4. Codex reviews and validates;
5. one focused commit after `ACCEPT`;
6. revert the single commit if regression appears.

Grouped variant:

1. clean baseline and recorded start commit;
2. packets executed in dependency order;
3. one local commit per packet;
4. Codex reviews commit-by-commit;
5. Codex runs group integration verification;
6. only the user or Codex may push after the group is accepted.

Git is currently valid, but branch, worktree, commit, merge, and push operations still require user authorization. Until such authorization is given:

- require unified diffs;
- do not stack unreviewed patches;
- apply one accepted task at a time;
- keep worker changes uncommitted;
- use `git diff` and `git status` as review evidence.
