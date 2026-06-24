# Project Context

Last verified: 2026-06-24

This file separates observed repository facts from approved future design and unresolved information.
Do not convert assumptions into facts.

## Observed facts

### Project and tooling

- Repository path used during verification: `D:\Unity\UnityProj\FightMatch`.
- Project type: Unity project.
- Unity Editor version: `2022.3.18f1`.
- Product name in `ProjectSettings/ProjectSettings.asset`: `FightMatch`.
- Current Unity executable on this machine:
  `D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe`.
- VS Code is configured to attach to Unity.
- Unity Test Framework `1.1.33` is present through `Packages/packages-lock.json`.
- Code coverage package `1.2.5` is present through package dependencies.

### Current repository contents

- `Assets/` currently contains only `Scenes/SampleScene.unity` and metadata.
- There are currently no project C# source files.
- There are currently no project `.asmdef` files.
- There are currently no project tests.
- There is no verified CI configuration.
- There is no verified lint or formatting command.
- There is no verified Player build script.
- There is no root README or CONTRIBUTING guide.
- There was no pre-existing `AGENTS.md` before this workflow was added.

### Version control

- Git is initialized and currently valid.
- Current branch: `master`.
- Current remote: `origin` at `https://github.com/yyczz1/FightMatch.git`.
- At the time of verification, `master` tracked `origin/master`.
- Existing commits include an initial Unity baseline and a project-safety initialization.
- Do not commit, push, branch, merge, or create a worktree unless the user authorizes the action.

### Existing rules and workflow files

- `CLAUDE.md` is tracked and contains Claude-specific architecture guidance.
- Its simplicity, verification, rollback, SOLID, Runtime/Editor separation, and design-pattern rules are compatible with this workflow.
- Its generic mention of future unique-solution validation conflicts with the approved Flow Puzzle specification; the approved “no unique-solution detection” rule wins.
- Its generic `Assets/Editor/` location guidance is overridden where an approved project plan specifies an assembly-separated Editor path.
- `.gitignore` is tracked and ignores Unity-generated content, `.spec-workflow/`, and `.agents/`.
- `.spec-workflow/` contains generic ignored templates. They are not evidence of current project architecture or product requirements.
- `.agents/` remains an empty ignored directory; this workflow uses the explicitly requested tracked `.agent/` directory.

### Existing approved product documents

- `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`
- `docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md`

These documents describe approved future Flow Puzzle work. They are not evidence that the implementation already exists.

## Approved future technical direction

The Flow Puzzle documents currently approve:

- deterministic solution-first generation;
- pure C# domain/algorithm assemblies;
- Unity persistence and Editor layers separated from core algorithms;
- UI Toolkit Editor tooling;
- ScriptableObject primary storage with optional JSON export;
- EditMode tests;
- an exact local solver for manual layout completion;
- no unique-solution requirement.

External workers must read only the portions relevant to their assigned task.

## Unknown or unresolved

- Official target platform(s) and Player build procedure.
- CI provider and CI commands.
- Repository-wide code style beyond rules defined here.
- Whether a formatter or analyzer will be adopted.
- Whether generated assets will be committed.
- Whether external DeepSeek works on a copy, patch-only workspace, branch, or future worktree.

## Updating this file

Codex may update this file only when:

1. the repository provides fresh evidence;
2. the user makes an explicit project decision; or
3. an approved implementation establishes a new stable command or structure.

Record unverified claims under “Unknown or unresolved,” not under “Observed facts.”
