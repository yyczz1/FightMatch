# FM-DEMO-014 delivery

Status: implementation and same-version joint validation complete; submitted for independent R review. This is not a review verdict.

- Batch: DEMO-B10, r75 §121 / §124; implementation owner and sole Unity executor: B.
- Task: `01a0c3c3-e163-7f01-b639-ca1f16e1a99d`.
- Original implementation turn: `01a0c404-71c6-7003-a4ed-241dcde05bd6`.
- Scope evidence: `demo-014-scope.json`; no branch, worktree, commit, push, external model, new task, or subagent.

## Delivered behavior

`CandidateReplayOperations` exposes the three approved operations: `BuildIsolationInput`, `ReplayRecorded`, and `EvaluatePlan`.
Current plans retain the current immutable run and explicitly frozen preferences. Recorded operations retain their original conditions.
Effective records use the original effective prefix. Superseded records use the `BeforeRun` of the exact first superseding rollback, including revision gaps and old random states.
Every replay actually invokes 012 with the original operation identity, timestamp, actor, pair, route, revision, and conditions.
Every plan freezes its whole step sequence, derives each revision from the real preceding result, and invokes 012 in order with the same caller budget.
All steps must succeed. Rejections preserve the failure index, original stage/code/path, and route diagnostics; no successful prefix, report, or plan evidence escapes a failed plan.
Recorded and rollback IDs, as well as duplicate IDs within a plan, are rejected for new plan operations. Extra steps after victory are evaluated and rejected.

The explicit context key includes purpose, player/attempt, source revision, complete original binding/baseline/definitions, initial/current snapshots, effective prefix, preferences, Empty rights, target, and used operation IDs.
Key comparison is structural and exact, with Ordinal string identity. It is not a hash or a cache.
Plan evidence additionally contains the complete ordered actual operation records, including timestamps, routes, identities, revisions, and actual frozen conditions.
Known immutable graphs are shared; caller-owned request shells, conditions, route lists, and step lists are not retained.
Every public isolation/evidence/result object is read-only and `CommitEligible=false`.

The closed, hand-written comparison covers accepted fixed types only; it does not use reflection, generic business-object discovery, or serialized byte offsets.
Its order is inputs/associations, crit opportunities and full rejected/accepted raw words, random streams/counters, direct damage, enemy intents, stage/board, contributions, full after-snapshot, and full terminal report.
Collections compare count first and then preserve semantic order. Divergences expose the step, operation, business field path, and immutable expected/actual values.
The report comparison includes all operations and contributions, terminal identity/time, whole-level HP, and fingerprint.
Malformed source graphs are rejected; complete recorded claims contradicted by real computation produce `Diverged`, distinct from `Rejected`.
Math and random limits propagate without a success object or partial proof; retry requires the caller to supply a fresh budget.

## Acceptance evidence mapping

| Criterion | Test methods and inspected implementation |
| --- | --- |
| ① real replay | `RecordedOperationRuns012AgainWithOriginalIdentityConditionsAndImmutableGraph`: public entry preparation/random binding → 012 → 013, then a newly computed record/direct/crit/enemy/stage graph; original history remains unchanged. |
| ② old branches | `TwoRollbackGenerationsReplayTheFirstSupersedingBeforeRun` covers a/b/c across two rollback generations; `RecordedCrossFaceAndRescueRestoreTheExactOriginalPrestate` covers face changes and unexecuted intent cursors after all participants fall. |
| ③ randomness/differences | `RejectionSamplingPreservesAllRawWordsAndSharedWordBudget` uses a legal SC01 seed whose accepted PCG state starts at zero and produces two rejected words before acceptance. `CertainCritAndRejectedLinkUseNoSamplingWords` covers q=1 and the real Link rejection path. `CompleteButChangedRecordedEvidenceReportsFirstBusinessField`, `RandomEvidenceWinsOverDamageAndLaterSnapshotDifferences`, and `TerminalReplayChecksCompleteReportAndFingerprintAfterLastOperation` locate business fields rather than only fingerprints. |
| ④ conditions/keys | `ContextKeyBindsCompleteValuesWithoutRelyingOnBaselineIdOrHash` and `ConditionsRequestsRoutesAndOrderedStepsAreFrozenAndBindDistinctEvidence` cover preference changes, same baseline ID with different definitions/random states, old versus new preferences, and retained immutable routes/steps. |
| ⑤ plans | `CompleteRealSourcePlanProducesReportIncludingLastStep` covers genuine L1/L3 source routes in both declared coordinate conventions; `FullPlanCrossesFacesWithoutChangingOriginalHistory`, `PlanFailurePreservesOriginal012RejectionAndNeverReturnsSuccessfulPrefix`, `RescueIsAnAcceptedPartialPlanOutcomeWithoutClaimingVictoryOrNoSolution`, and `PlanRejectsEveryKindOfPreviouslyUsedOperationId` cover all scoped outcomes. |
| ⑥ malformed input/budgets | `IsolationRequestsRejectMissingConflictingOrUnsupportedFields`, `InvalidPlanStepReturnsItsExactIndexWithoutLeakingPrefix`, `MissingOriginalRelationCannotBeGuessedFromCurrentSnapshotOrArchiveOrder`, `IncompleteOrForeignGraphIsRejectedBeforeTakingAnyRandomWord`, `CallerMathLimitPropagatesWithoutAnySuccessObjectAndFreshBudgetRetries`, `MathLimitAtFinalComparisonOrPlanStepCannotReturnPartialProof`, and `NullRootsThrowArgumentNull`. |
| ⑦ joint regression/scope | Final B10: 2426/2426 Passed, original 2145 full Ordinal names/multiplicities and every occurrence Passed retained; 014 adds 110, 015A adds 171. Exact 453-file after list, all old SHA values, frozen ten sources, protected inputs, and GUID checks are in scope. |

## Scope and limitations

Only the approved Empty, single-Warrior, E01/E02 candidate domain is implemented. No search, online strategy, revive, publication, permanent write, inventory consumption, or cache-based substitute for evaluation was added.
The accepted 012 domain cannot naturally create a pending-link history: a supported Warrior kill locks its own pair, and there is no supported secondary death source.
Link dispatch calls the real 012 `EvaluateLink` with the caller's math budget and takes no random words. The new Link test verifies its authentic rejection behavior; this delivery does not claim a publicly reachable successful Link replay fixture.
Internal changed-evidence constructors are used only to test precise divergences; they do not introduce an import/commit/recovery bypass.
L1/L3 victories prove the supplied conditional test binding and its fixed random trajectory only. They do not prove all seeds, global solvability, calibrated production crit parameters, or published playable content.

## Exact new implementation files

- `Assets/Scripts/FightMatch/Core/CandidateBattleIsolation.cs` and its Unity-generated `.meta`.
- `Assets/Scripts/FightMatch/Core/CandidateReplayRequests.cs` and its Unity-generated `.meta`.
- `Assets/Scripts/FightMatch/Core/CandidateReplayOperations.cs` and its Unity-generated `.meta`.
- `Assets/Scripts/FightMatch/Core/CandidateReplayComparison.cs` and its Unity-generated `.meta`.
- `Assets/Tests/EditMode/FightMatch/CandidateReplayOperationsTests.cs` and its Unity-generated `.meta`.
- This delivery and `docs/system-design/2026-09-17/demo-014-scope.json`.

B additionally generated only the five authorized 015A meta files through Unity. C's five source files were not used as a dependency, reviewed, or edited by B.
The original 433-file before snapshot and its `2026-09-21T12:15:10.9719792Z` timestamp are inherited unchanged from the independently accepted B09 scope.
The actual B start snapshot is separately retained at `2026-09-21T12:52:08.7683696Z`.

## Joint validation

The final ten-source freeze is `b4e5b8b8df5d0f41a4f6225bbb29301fe9d1978094a796a98be2907afc958750`.
B's final five-source freeze is `026ff10e139e82a44eff96424a6c6c4a0b1ba1cf1c67cf566faf74ba1a941836` (1695 C# lines).
C's final five-source freeze is `c07edcd5db2767d0aed63ed0d681f9a5bd634fe213261907a0dc149f6b3f96be` (1801 C# lines); C explicitly confirmed CODE_READY in its original turn.
Both authors remained frozen for the final compile and complete EditMode. B's ten implementation files total **1750 lines**, C's total **1856 lines**; each is below 2600.

| Actual Unity run | PID | UTC start → end | Actual exit |
| --- | --- | --- | --- |
| Compile attempt 1 | 43064 | 2026-09-21 13:22:14.1464285 → 13:32:23.9448724 | 1 |
| Compile attempt 2 | 48828 | 2026-09-21 13:34:55.4894257 → 13:35:14.3894830 | 0 |
| EditMode attempt 1 | 39380 | 2026-09-21 13:38:13.5900897 → 13:39:01.2747934 | 2 |
| Final compile | 29320 | 2026-09-21 13:45:09.1791764 → 13:45:21.2384561 | 0 |
| Final EditMode | 51844 | 2026-09-21 13:45:53.6779293 → 13:46:38.8239555 | 0 |

Final compile CommandExecution: `exec-a1a9c864-40ba-40d8-81db-95262bd55629`; final EditMode: `exec-4abcc726-fcd8-4e6d-ac3d-4115b67fa877`.
Both logs contain zero compiler errors and zero compiler warnings.
Every launch checked that no Unity process was running, used `Start-Process -WindowStyle Hidden -PassThru -Wait`, and captured the actual Process.ExitCode.
Executable: `D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe`; project: `D:/Unity/UnityProj/FightMatch`.
Compile arguments: `-batchmode -nographics -quit -projectPath D:/Unity/UnityProj/FightMatch -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB10Compile.log`.
EditMode arguments: `-batchmode -nographics -projectPath D:/Unity/UnityProj/FightMatch -runTests -testPlatform EditMode -testResults D:/Unity/UnityProj/FightMatch/FMDemoB10-EditMode.xml -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB10Tests.log`; no `-quit`.

| Final artifact | SHA256 |
| --- | --- |
| `Logs/FMDemoB10Compile.log` | `e9810674d27cc3441f5a711bdff0fcfe0230d8728c492a1b3ee46faa855b1ba5` |
| `Logs/FMDemoB10Tests.log` | `ff5f36f62c330d74acd10975404d065a6586568a5e1e9469bc6b7933a5fa4095` |
| `FMDemoB10-EditMode.xml` | `dfb08fb27958836e521021293faf502aba76a18d56873977ba1b83941497dc5a` |

Final XML: **2426 total / 2426 Passed / 0 Failed / 0 Skipped / 0 Inconclusive**. Original 2145 cases retain their full Ordinal names, exact multiplicities, and Passed state; 014 contributes 110 actual cases, 015A contributes 171.
Original XML byte SHA remains `d0aea4d0a22a3de60cf7f6bb560a64ccf98e5b0cf8e0bbeeb97b7e2d7ffabd22`.
The original name/multiplicity canonical SHA is `beb17a2c289c85f9fd21c1c7e8bf5113d23bfd8e3553a3ad89437a0b71897df5`; its exact canonicalization is in `xmlSummary`.
The final 453-file canonical SHA is `7dec93b9baef37c7196372087df1089ea49a6b27bcd37a145732bfae652202ae`: 433 original files unchanged, ten new 014 files, ten new 015A files.
All 251 Assets GUIDs are unique; all original 241 path/GUID pairs remain unchanged. The ten new GUIDs are individually recorded in scope.
Protected input hashes remain unchanged. No old C#/test/meta/asmdef, dependency, project setting, permission, or accepted design document was edited.

## Preserved failures and evidence decoding

Compile attempt 1 found CS1729 in B's test-only crit constructor. B corrected only that new fixture to the accepted seven-argument API.
Its original 42394-byte log SHA is `e0df3543a9687864367b3b6825f7289c5d20084b0d3a9efd2c69c52e3ce03c20`; `failedAttempts[0]` retains full original bytes and the original source freeze.
An EditMode preflight command then stopped before launching Unity because successful import expanded the new meta placeholders from two lines to eleven-line MonoImporter records. The ten GUIDs were verified by reconstructing the exact original two-line bytes and their original hashes. No Unity process was started by that preflight failure.
EditMode attempt 1 had five B failures from the same L3 fixture's omitted explicit-null intent fields, plus one C allocation-observation failure. B added the required explicit nulls only in its new test; C corrected only its own new test after receiving the exact diagnostic.
C reported replacing Mono's zero-valued allocation counter with direct Stream.Read buffer identity, size, and reuse observations, with no skipped test or timing substitute. B did not inspect or edit C's implementation.
The original failed XML is 1605772 bytes, SHA `a1d5cda16db11a8cefedcda07607da589417af298b8b61067e27b9a35a898ad5`; its complete bytes, the complete failed test log, and the preceding successful compile log are retained in `failedAttempts[1].artifacts`.
Every reused fixed artifact path was checked against its preserved byte SHA before rerunning. Both original turns explicitly refroze after their own corrections.

The scope retains the complete uncompressed 433-file before and 453-file after lists. To stay below 400 KiB, original failure bytes and full raw command evidence use explicitly labeled lossless `gzip+base64` or `brotli+base64`; actual new test names/counts use `brotli+base64` UTF-8 JSON.
Decode with `Convert.FromBase64String`, then `System.IO.Compression.GZipStream` or `BrotliStream` in Decompress mode. Verify the decoded byte length and SHA256 fields before parsing JSON or XML. Per-method actual case/Passed counts remain directly readable in `xmlSummary.newTestMethods`.
`rawValidationEvidence` contains complete original PowerShell commands, initial/final exec results, and all six CommandExecution records, including the preflight command that launched no Unity process.
The evidence distinguishes PowerShell command output from Unity output retained in `-logFile`; it does not claim separate capture of Unity child stdout/stderr.
