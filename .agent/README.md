# Codex + External DeepSeek Workflow

This directory defines the repository's AI collaboration workflow.

DeepSeek is used manually in another environment. Codex does not call it automatically.

## Documents

| File | Purpose |
|---|---|
| [`../AGENTS.md`](../AGENTS.md) | Authoritative repository-wide AI rules |
| [`../CLAUDE.md`](../CLAUDE.md) | Existing Claude-specific architecture guidance |
| [`PROJECT_CONTEXT.md`](PROJECT_CONTEXT.md) | Verified project facts, approved plans, and unknowns |
| [`CODING_RULES.md`](CODING_RULES.md) | Engineering and Unity-specific change rules |
| [`TASK_PACKET_TEMPLATE.md`](TASK_PACKET_TEMPLATE.md) | Template Codex fills for one external-worker task |
| [`TASK_GROUP_TEMPLATE.md`](TASK_GROUP_TEMPLATE.md) | Template for sending several ordered task packets in one worker session |
| [`DEEPSEEK_WORKER_PROMPT.md`](DEEPSEEK_WORKER_PROMPT.md) | Fixed prompt copied to the external DeepSeek environment |
| [`REVIEW_CHECKLIST.md`](REVIEW_CHECKLIST.md) | Mandatory Codex review procedure |
| [`VALIDATION.md`](VALIDATION.md) | Known validation commands and evidence rules |
| [`PLANS.md`](PLANS.md) | General planning, task splitting, and status lifecycle |

## Normal workflow

1. The user gives Codex a requirement.
2. Codex reads `AGENTS.md`, `PROJECT_CONTEXT.md`, and relevant project files.
3. Codex decides whether the requirement needs a plan.
4. Codex fills one `TASK_PACKET_TEMPLATE.md` instance.
5. The user copies:
   - `DEEPSEEK_WORKER_PROMPT.md`;
   - the completed task packet;
   - only the referenced context needed by the task.
6. External DeepSeek edits its workspace or returns a unified diff.
7. The user returns the patch, diff, changed files, and verification output to Codex.
8. Codex applies `REVIEW_CHECKLIST.md` and independently validates where possible.
9. Codex returns:
   - `ACCEPT`;
   - `NEEDS_FIX` plus a smaller corrective task packet; or
   - `REJECT` with the reason and rollback scope.

## Grouped delivery workflow

Use grouped delivery when several already-designed tasks share one dependency
chain and can be implemented in one external-worker session.

1. Codex creates one task-group manifest plus two or more complete task packets.
2. The user sends the fixed worker prompt, the group manifest, and all packets
   to external DeepSeek.
3. DeepSeek executes packets in order.
4. Each packet keeps its own whitelist, tests, output, and optional local
   checkpoint commit.
5. DeepSeek stops dependent work when a packet is `BLOCKED` or its verification
   fails.
6. The user returns the group report, commit hashes or diffs, and test evidence.
7. Codex reviews every packet separately, then issues one group integration
   verdict.

Grouped delivery reduces handoff latency. It does not authorize a broad
unreviewable patch.

## Token-saving rules

- Do not send the full conversation to DeepSeek.
- Do not send all project documentation when a short current-context summary is enough.
- Prefer exact file paths and narrow excerpts.
- Group low-risk data-contract tasks more aggressively; keep generator, solver,
  threading, persistence, and Editor lifecycle packets smaller.
- DeepSeek output should be patch-first and concise.
- Store stable project facts in `PROJECT_CONTEXT.md`; repeat only task-specific facts.
- After a rejected patch, create a smaller correction packet instead of discussing the entire project again.

## Rule maintenance

- Codex owns these collaboration rules.
- External workers may not edit `.agent/` or `AGENTS.md` unless a task packet explicitly assigns that exact work.
- Do not duplicate or overwrite `CLAUDE.md`; conflicts are resolved by `AGENTS.md` and approved project specs.
- Update `PROJECT_CONTEXT.md` only after verifying a repository fact.
- Do not copy the same rule into many files. `AGENTS.md` is authoritative; supporting files provide procedures and templates.
