# Coding and Change Rules

These rules apply to Codex plans and external DeepSeek task packets.

## 1. Think before coding

Before editing:

1. Read the task packet completely.
2. Inspect every allowed-to-modify file.
3. Inspect directly related call sites, tests, and approved design sections.
4. State any assumption that affects behavior.
5. Stop with `BLOCKED` if the task requires an unapproved decision.

Do not infer permission from proximity. A nearby defect or style issue is outside scope unless the packet includes it.

## 2. Simplicity first

- Implement only the required behavior.
- Prefer direct, readable code.
- Do not add abstractions, extension points, settings, caching, retries, logging systems, or error frameworks unless required.
- Do not create an interface merely because one might be useful later.
- Do not generalize a one-use helper without evidence.
- If the requested implementation is overcomplicated, report the simpler option before coding.

## 3. Surgical changes

- Every changed line must trace to the task goal or its test.
- Do not reformat whole files.
- Do not reorder unrelated members or imports.
- Do not rewrite comments unless they became incorrect because of the task.
- Do not rename unrelated symbols.
- Do not fix unrelated warnings or dead code.
- Remove imports, fields, or helpers only when the current change made them unused.
- Preserve line endings and surrounding style.

## 4. Goal-driven execution

Every task must define:

- one goal;
- non-goals;
- allowed and forbidden files;
- acceptance criteria;
- verification steps;
- output format;
- blocking behavior.

No acceptance criterion may rely only on “looks good” or “should work.”

## 5. Protected changes

The following are forbidden by default and require explicit task-packet permission:

- public API changes;
- serialized field or data-format changes;
- asset moves, renames, deletions, or `.meta` changes;
- new packages or dependency-version changes;
- `Packages/manifest.json` or lockfile changes;
- `ProjectSettings/` changes;
- `.asmdef` changes;
- CI, build scripts, or release configuration;
- database/schema migrations;
- generated files;
- broad formatting;
- changes under `.agent/` or `AGENTS.md`.

If a protected change becomes necessary, stop with `BLOCKED`; do not expand the packet yourself.

## 6. Unity-specific rules

- Never edit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, generated `.csproj`, or generated `.sln`.
- Preserve `.meta` files when moving or renaming Unity assets.
- Do not manually invent `.meta` files unless the task explicitly requires deterministic asset creation and permits it.
- Do not run Unity batch mode while this project is open in Unity.
- Keep Editor-only code in Editor assemblies/folders.
- Do not introduce `UnityEditor` references into runtime assemblies.
- Keep pure algorithm code free of Unity APIs when the approved architecture requires it.
- Do not call Unity APIs from background threads.
- Treat serialized field renames and ScriptableObject layout changes as compatibility changes requiring explicit approval.

## 7. Approved Flow Puzzle constraints

When a task belongs to the approved Flow Puzzle plan:

- automatic generation must remain solution-first;
- do not add unique-solution detection;
- do not require every board cell to be used;
- do not treat the recommended solution as the only valid player route;
- keep generator and exact solver responsibilities separate;
- fixed seeds must not depend on unordered collection iteration;
- use UI Toolkit for the Editor main interface as specified;
- do not place business logic inside the EditorWindow or board view;
- save formal level assets only after validation.

## 8. Testing and evidence

- Add or update the smallest relevant test when behavior changes.
- Run the exact verification listed in the task packet.
- Never claim a command ran if it did not.
- Report skipped verification and the reason.
- A passing unrelated test does not prove the requested behavior.
- See [`VALIDATION.md`](VALIDATION.md).

## 9. Blocked behavior

Use this exact structure:

```text
BLOCKED
Reason: <why the task cannot be completed safely>
Missing information: <specific missing input>
Required decision: <what Codex/user must decide>
Files inspected: <paths>
No changes made: Yes/No
```

Do not guess, silently broaden scope, or produce a partial patch that violates acceptance criteria.
