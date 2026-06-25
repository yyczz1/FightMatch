# Validation Rules

Validation evidence is required before a patch can be accepted.

## 1. Evidence standard

For each command report:

- exact command;
- whether it ran;
- exit code;
- test totals or relevant log lines;
- timestamp or current task context when useful.

Never report “should pass.”

Use:

```text
RUN — <command>
Result: exit <code>; <tests passed/failed or concise evidence>
```

or:

```text
NOT RUN — <command>
Reason: <specific blocker>
```

## 2. Current repository validation facts

- Unity version: `2022.3.18f1`.
- Unity Test Framework `1.1.33` is available through package dependencies.
- No project test assembly exists yet.
- No lint command is currently defined.
- No CI command is currently defined.
- No Player build method is currently defined.
- Git is initialized on `master` with remote `origin`.
- Git mutations still require user authorization.

Do not invent missing commands.

## 3. Unity executable

Prefer a configurable environment variable:

```powershell
$env:UNITY_EXE = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
```

Verify it:

```powershell
Test-Path -LiteralPath $env:UNITY_EXE
```

Expected: `True`.

The absolute path is machine-specific and may differ elsewhere.

## 4. Unity process safety

Before batch mode:

```powershell
Get-Process Unity -ErrorAction SilentlyContinue |
    Select-Object Id, Path, MainWindowTitle
```

Do not launch batch mode against this project while it is open in Unity.
Ask the user to close the relevant Editor instance; do not kill Unity without explicit permission.

## 5. Compile/import validation

Once source files exist:

```powershell
& $env:UNITY_EXE `
  -batchmode -nographics -quit `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\AICompile.log'
```

Expected:

- process exit code `0`;
- no compiler errors in the log.

Inspect:

```powershell
Select-String `
  -Path 'D:\Unity\UnityProj\FightMatch\Logs\AICompile.log' `
  -Pattern 'error CS|Compilation failed|Unhandled Exception'
```

Expected: no matches.

## 6. EditMode tests

After a project test assembly exists:

```powershell
& $env:UNITY_EXE `
  -batchmode -nographics `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\TestResults.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\AITests.log'
```

Expected:

- process exit code `0`;
- XML reports zero failed tests.

Do not claim tests pass merely because the log file exists.

Do not add `-quit` to a `-runTests` invocation. Unity Test Framework exits after
the run; `-quit` can terminate the Editor during an initial import or domain
reload before the test runner writes its result XML.

## 7. Static scope validation

For every returned patch:

```powershell
git status --short
git diff --stat
git diff --name-only
git diff --check
```

Always verify:

- no forbidden file appears;
- no unexpected binary or asset change appears;
- no package/config/lockfile change appears without permission;
- no unrelated whitespace-only diff dominates the patch.

## 8. Documentation-only validation

For Markdown rule/template changes:

- verify every requested file exists;
- search for unresolved `TODO`, `TBD`, placeholders, or contradictory rules;
- verify links use correct relative paths;
- verify the DeepSeek prompt and task template share the same output contract;
- verify review verdicts are exactly `ACCEPT`, `NEEDS_FIX`, or `REJECT`;
- verify no business, package, or ProjectSettings file changed.

## 9. Manual validation

Use manual validation only for behavior that automated checks cannot cover, such as:

- Editor window layout and resizing;
- pointer interaction;
- visual distinction;
- asset selection and undo behavior.

Record the exact actions and observed result. “Looks fine” is not enough.

## 10. Acceptance rule

A patch may be `ACCEPT` only when:

1. scope validation passes;
2. every acceptance criterion passes or has approved manual evidence;
3. required compile/tests pass;
4. no unapproved protected change remains;
5. remaining risk is explicitly documented.
