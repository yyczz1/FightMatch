# Project Context

Latest user instruction (2026-09-30 overnight) supersedes the prior pause: resume development under the approved five-role team, targeting a reviewable first Demo tomorrow. See [the active team registry](../docs/team/2026-09-30/README.md) and its dispatch packet. The central chat coordinates; specialist leads delegate design/implementation/testing. Existing evidence and product scope remain binding. The Goal tool still reports paused and the user has been asked to resume it in the UI; authorized current work continues. Historical pause statements below describe the previous checkpoint.

Last context refresh: 2026-10-01. The active five-role team now sends code review to GitHub PR Code Review, per the user's request to reduce general Codex usage; no new local R code reviews. Automated tests still prefer dots, with scoped local Unity/device exceptions. See [the review procedure](TEAM_WORKFLOW.md#7-github-pr-code-review). FIX17 Host30 passed30/30; Core190 timed out at420s without XML and was safely terminated after its grace period. FIX15 exact2 remains valid, but222 coverage and native reopen are incomplete. Product/resource bytes are preserved while a review checkpoint and read-only timeout diagnosis are prepared. Layout implementation, the Luban material/toolchain, and a new APK remain unaccepted. Current dependencies are in the active team registry; older pause and pending-handoff statements below are historical.

This file separates observed repository facts from approved future design and unresolved information.

Version checkpoint467: the user requested committing and pushing current work with concise, detailed log entries.029 source/build WIP, accepted test-performance changes and art candidates have separate local commits; design/team rules and selected byte-identical evidence are included in the checkpoint. See [the version and push log](../docs/versioning/2026-09-30-checkpoint.md) for actual commit IDs and remote receipt. This is preservation, not029/Demo acceptance or a new Unity run; historical uncommitted-WIP statements describe the pre-checkpoint state.

Team organization decision (2026-09-30, packet466): one central coordinator with planning, engineering, art and testing leads, each in a separate chat; specialist design and execution are delegated below them. See [the organization design](../docs/architecture/2026-09-30-ai-team-organization.md) and [the execution agreement](TEAM_WORKFLOW.md) for role boundaries, task/test receipts and activation. The engineering path is architecture → system design → implementation. Independent R remains distinct from authors; the testing lead coordinates its receipt. The user subsequently confirmed “以后就这么执行”, making this the standing workflow in AGENTS section6 for future authorized FightMatch work, including necessary role-chat creation/reuse and in-team dispatch/receipts without repeated per-packet permission. Actual role handoffs remain pending; the policy does not resume the product goal or supersede C's serial Unity responsibility before handoff. The existing cloud chat “设置 FightMatch” reports Unity installation and compilation at9d416e6; its stopped test has no pass/fail result and is not current-WIP acceptance.

Current user boundary: the build and safe closeout required by section457 are complete; SD00 is pausing the main goal. C original turn01a0eaf3-31c0-7d83-9dfc-31c24012946f completed at1790669409 with BUILD_READY_DEVICE_PENDING. Final Compile014, Host01530/30 and Full0164041/4041 passed. QA018 and normal019 APKs are preserved, settings restored and build processes exited. Normal019 Unity succeeded but its raw wrapper false positive remains; section460 structural/signature supplemental checks passed, and the validator itself still needs correction/review after resumption. Device acceptance and independent product R are not complete. No further long tests, formal R or Android development before user resumption. The user separately requested BlueStacks installation in C turn01a0ec3a-3884-7963-8410-a40aec520b7a; that limited task is not authorization to resume this main goal. MonitorControl alone may be updated/restarted after safe stop; macOS/Codex may not be restarted by that instruction. Section461 records the receipt, hashes and exact continuation gates.
The user separately authorized a new art/resource creation chat, thread01a0ec79-5b64-7b62-8365-bee9229512b5/local. Section462 confines its first batch to ArtSource/FightMatch/2026-09-29; it does not run Unity or alter product assets/code. The main goal remains paused. Section463 records the current design/remaining-scope audit: all15 design responsibility groups have documents, but complete Android design and a refreshed exact remaining package count are not finished.
Do not convert assumptions into facts.

Post-Q34 audit (2026-09-30): Q35 is answered: prompt and obtain confirmation before resource downloads use mobile data, including Wi-Fi-to-cellular transitions; a per-download confirmation is not permanent permission. The user also asks to verify resumable downloads using the chosen YooAsset framework; package capability is not completed project integration. Prior Q34 confirmation covers the stated consensus, not completion of all detailed design. Q21 is now explicitly decided: use R2 Standard with Cloudflare caching/CDN for public game resources, downloaded through YooAsset, prioritizing lower service cost. The ad-backend server deployment and launch regions remain undecided, alongside concrete engineering and privacy-flow proposals. See packet465 and backend-self-hosting-research section15 for the R2 billing clarification; no implementation is resumed.

Latest release scope (2026-09-30, Q25-Q28 answered): prioritize Google Play launch with local guest gameplay and saves, rewarded ads plus the necessary minimal ad backend, and the original small bootstrap with initial remote content download/CDN. Formal Google/email accounts, cloud saves and human-reviewed account recovery move to a later release while their integration boundaries can be designed now. This is not a fully offline or backend-free release. Go/sqlc/MySQL Community is selected for the limited ad backend; ad-backend deployment remains unselected and needs a scope-specific estimate; Q21 selects R2 Standard with Cloudflare caching/CDN for resource delivery. Q27 keeps only the free default walkthrough in the first release and defers current-battle online analysis. Q28 keeps guest credentials only on the device: if all are lost, recovery is unavailable and this limitation must be disclosed beforehand; no recovery code or manual export/import is included. Q23 rejects merging two existing formal accounts; Q24 approves human-reviewed recovery for the later account release, which cannot recreate never-uploaded lost local data. Q34 explicitly confirms the consolidated design understanding for later detailed design and review. There are no pending interview cards; the resource provider is now R2, while ad-backend deployment and concrete launch regions remain undecided. These design decisions do not resume the paused product goal or authorize implementation.

Runtime UI decision corrected by the user on 2026-09-29: FightMatch player UI uses uGUI; UI Toolkit is for Editor tooling, as now explicit in AGENTS.md. Existing runtime views and the029 host still use UI Toolkit and require migration. See [the correction plan](../docs/system-design/2026-09-17/runtime-ui-ugui-correction.md) and packet464 before UI work, asset integration or product acceptance. Existing accepted business behavior and evidence remain valid for their original scope; they do not establish user approval of the runtime framework. The main goal remains paused and this documentation correction does not dispatch implementation or Unity validation.

Q29 sets Android 7 (API 24) as the first-release minimum OS, excluding Android 5.1/6; settings are not yet changed or validated. Q30 now selects one ad source initially, with later expansion based on actual availability. Q31 selects teenagers aged 13 and above and adults as the primary intended audience, not an adults-only or content-rating declaration. Q32 selects AdMob with Google as the single ad source for initial validation. Exact SDK versions, Unity builds and receipt/reward recovery remain unverified; After Q33 technology and career-fit clarification, the user explicitly selected MySQL Community, superseding their earlier PostgreSQL choice. Their career direction remains undecided; no database version is pinned and no installation or implementation has occurred.

For a new chat or device, start with [`START_HERE.md`](../START_HERE.md). It links
the portable system-design, architecture, and planning context and the current
handoff. The old description of an empty project no longer applies.

## Observed facts

### Project and tooling

- Historical Windows repository path: `D:\Unity\UnityProj\FightMatch`.
- Current Mac repository: `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`.
- Current Mac Unity executable: `/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity` (x86_64/Rosetta).
- Project type: Unity project.
- Unity Editor version: `2022.3.18f1`.
- Product name in `ProjectSettings/ProjectSettings.asset`: `FightMatch`.
- Historical Windows Unity executable (not a Mac path):
  `D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe`.
- Unity Test Framework `1.1.33` is present through `Packages/packages-lock.json`.
- Code coverage package `1.2.5` is present through package dependencies.

### Current repository contents

- FlowPuzzle and FightMatch source and assembly definitions exist under `Assets/Scripts/`.
- Project test assemblies exist under `Assets/Tests/EditMode/`, including FlowPuzzle, FightMatch.Core.Tests, and FightMatch.QFramework.Tests.
- QFramework is present under `Assets/ThirdParty/QFramework/`.
- Design, task, delivery, review, and selected migration evidence are versioned under `docs/`.
- CONT-C-MAC-R1 is accepted after C formal completion and independent R ACCEPT (§430–431). Final Compile013 and unfiltered EditMode015 passed: 3956/3956, no failures or skips. The original 3872 named occurrences remain.
- `4be170fd65051eeac5a523817f534e725526c4c6` is the historical pre-resumption WIP, not the current delivery. 028 is now independently accepted (§442), with master9d416e6 pushed.029 build WIP has formally returned (§461); APKs exist but device gameplay/cold-start acceptance and product R remain pending. The later user pause (§457) supersedes the continuous-next-stage instruction in§438.
- The accepted CONT-B-CODE-C1 Windows evidence reports 3872/3872 tests passing, with no failures or skips. It does not validate the later WIP or Mac.
- There is no verified CI configuration.
- There is no verified lint or formatting command.
- The029 validation/build entry produced QA018 and normal019 APKs. Its normal-APK raw-string exclusion predicate still falsely rejects friend-assembly attribute names; preserve019 failure and460 supplemental evidence, and correct/review this after resumption.
- `START_HERE.md` is the root continuation entry; `docs/migration/2026-09-28/README.md` documents Git retrieval and the evidence archive.

### Version control

- Git is initialized and currently valid.
- Current branch: `master`.
- Current remote: `origin` at `https://github.com/yyczz1/FightMatch.git`.
- At the time of verification, `master` tracked `origin/master`.
- The migration starting HEAD was `adaffdc4d7e8329fb4e8ee663ed8918c78ad8f0c`. Accepted CONT-C source commit `7464efd10e4d2ed5341bbc1e2d179364dd94489c` was pushed normally to origin/master and independently verified by ls-remote at 2026-09-28T11:15:07Z. A following documentation-only checkpoint does not change that accepted source. See task packet §432; read actual HEAD when resuming.
- Preserve `.claude/settings.local.json` as user-managed local permissions. Its current local modification is not part of the migration commits.
- `.gitattributes` disables text conversion to preserve hash-bound source/evidence bytes across platforms; do not normalize line endings as incidental cleanup.
- Do not commit, push, branch, merge, or create a worktree unless the user authorizes the action.

### Existing rules and workflow files

- `CLAUDE.md` is tracked and contains Claude-specific architecture guidance.
- Its simplicity, verification, rollback, SOLID, Runtime/Editor separation, and design-pattern rules are compatible with this workflow.
- Its generic mention of future unique-solution validation conflicts with the approved Flow Puzzle specification; the approved “no unique-solution detection” rule wins.
- Its generic `Assets/Editor/` location guidance is overridden where an approved project plan specifies an assembly-separated Editor path.
- `.gitignore` excludes Unity caches, local settings, raw TestArtifacts, ExternalWork, and migration output. The selected continuation evidence archive is explicitly versioned under `docs/migration/2026-09-28/`.
- `.spec-workflow/` contains generic ignored templates. They are not evidence of current project architecture or product requirements.
- Repository workflow documents are in the tracked `.agent/` directory.
- Current SD00/C/R responsibilities and the latest user-approved same-product Demo scope are recorded in the continuation entry and task packets. Historical manual external-worker rules do not require recreating all old chats.

### Existing approved product documents

- `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`
- `docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md`

These remain foundational FlowPuzzle specifications. Current implementation and
acceptance must be read from later delivery records, not inferred from their age.
For FightMatch product rules, architecture, and current tasks, use the three role
context links in `START_HERE.md` and their original sources.

## Approved technical direction

The Flow Puzzle documents currently approve:

- deterministic solution-first generation;
- pure C# domain/algorithm assemblies;
- Unity persistence and Editor layers separated from core algorithms;
- UI Toolkit Editor tooling;
- ScriptableObject primary storage with optional JSON export;
- EditMode tests;
- an exact local solver for manual layout completion;
- no unique-solution requirement.

FightMatch's Demo is an intermediate version of the same Android product, with
less published content and the same business rules, normal flow, and save
semantics. Platform adapters may differ. Do not create a second Demo business
implementation or reset player data merely to transition to a later version.

External workers must read only the portions relevant to their assigned task.

## Unknown or unresolved

- Mac compilation/EditMode for final CONT-C are established by this round's actual evidence. Android APK build/signature evidence is established with the019 wrapper limitation above; iQOO Neo5/BlueStacks normal flows and cold restarts remain unverified at original build receipt. Follow the separate user-requested C installation turn for later installation/first-screen facts; these do not substitute full acceptance.
- `Tools/Invoke-FM025P2Validation.ps1` now has an accepted fixed CONT-C-MAC-R1 stage with serial process, script SHA and same-version checks. The canonical external runtime directory is `Applications`, not the former `application`. 028 has its signed §434 packet, §440 focused supplement and §442 independent acceptance; 029 now has its explicit §443 packet and fixed demo-029-mac-r1-plan.json. Reuse valid evidence and preserve old failure/input records.
- CI provider and CI commands.
- Repository-wide code style beyond rules defined here.
- Whether a formatter or analyzer will be adopted.
- Current authorized local C thread: `01a0e404-d89d-7ab2-bece-3cd1df3fbc52`; R thread: `01a0e404-e8ee-7310-8388-9260babd53f1`, both gpt-6-astra/max. Actual completed author/review turns and formal messages are recorded in §430–431. Old Windows IDs remain historical provenance.
- Full historical raw test artifacts, local caches, credentials, real player saves, and native Codex chat databases are not included in the selected migration archive.

## Updating this file

Codex may update this file only when:

1. the repository provides fresh evidence;
2. the user makes an explicit project decision; or
3. an approved implementation establishes a new stable command or structure.

Record unverified claims under “Unknown or unresolved,” not under “Observed facts.”

Latest Android implementation authority: §443, after028 commit9d416e6c9d3c794be355f903b3636c856783601e normal push and remote equality. Keep actual UI/device gaps separate; complete independent code/build/QA work first. The first native CUA BlueStacks read blocked for30216.1843 seconds despite its default30-second timeout, delaying dispatch; no cause is inferred. Do not call that UI route in the029 C turn. Existing source/DLL/test evidence remains valid; this delay is not test execution.

029 execution supplements: sections445/446 bind QA exact output, natural UTF cleanup and temporary settings restoration; sections450/451/452/453 record actual compile/resource failures and bounded recovery. Current caps are Compile7 and ResourcePrepare5, other per-mode caps unchanged; all modes still share at most20 existing run slots even though their individual caps sum to23. Section452 preserves four unexpected Unity default-theme leaves by exact bytes and permits only their verified cleanup. Original plan/source/evidence identities remain immutable. C is still in the original active029 turn; resource method009 and its wrapper passed, but an actual saved scene check found UIDocument.panelSettings missing. Section453 authorizes only the existing scene binding correction with the same GUID and a real save/reopen verification; final compilation, tests, APK, device and independent acceptance remain pending. Existing BlueStacks was actually connected on2026-09-29T02:49:40Z after enabling only local ADB, but C later observed it exited/offline; recheck actual device state at testing time. iQOO remains unconnected. Section448 prepares a candidate tag and later same-product non-Development build/upgrade proof, without authorizing that later code yet.

Latest author progress: C reports011 passed the real scene save/reopen binding check. Section454 bounds only Unity-generated IL2CPP sibling backups (exact per-slot roots, no additional APKs or runs), with full inventory in existing build-report JSON. Existing R is concurrently checking the signing-cache execution contract under455, actual turn01a0eb64-2ec5-7901-9d38-4b8504129e76; this is not product review. A private generated-Gradle credential-cache clarification is pending exact local SDK evidence; no password or key is to be sent to the model or public artifacts. Formal code acceptance remains pending.

Signing-cache clarification is now approved in456 after the exact455 R turn completed at1790656328. It covers only required Unity/Bee/Gradle credential copies in the task project Library/Bee and the already approved external gradle-user-home, with private root permissions before credentials, inherited umask077, no reuse of an old-umask daemon, and same-UID0600 credential files. No generated cache body, secret or key is delivered/public; the original plan and tests remain unchanged. C is incorporating this with454 before final compilation and regression. This closes a tooling contract issue, not product acceptance.

Current029 validation: Compile012 passed. Host013 naturally failed with27/30 passing, three H04 routing/lifetime failures and zero skipped. Section458 authorizes the same C to make the smallest fix within the original host/test whitelist and raises only Compile7 to8; the overall20-slot ceiling and all other mode caps stay unchanged. Preserve the failure XML; finish scoped diagnosis before the one final full regression. Section457 still requires build closeout followed by pause, with device and product-review gates pending.

Latest029 progress (2026-09-29): SD00 verified the original014/015 result files and015 XML. Compile014 passed; Host015 passed30/30 with zero failed/skipped (455.7086277s). FullTests016 began at05:27:00.1776320Z with the same frozen tool/root identities and has no final result yet. Do not repeat the resolved013 failure or accepted earlier stages on resumption. The user's section457 pause-after-build condition remains effective and C has confirmed it.

Verified029 full regression:016 completed at2026-09-29T07:22:27.4019220Z with4041/4041 passed, zero failed/skipped/inconclusive. XML3006081bytes/SHAa03827dbb1b25a5727b51833d8bc8ac076a7f2803212f64d52f72d273c45dad4; duration6848.3711223s. SD00 independently compared the old028/006 named-instance multiset: all4011 preserved,30 new. C preserved the Mac DLL identities and entered017 QA build preflight. Do not rerun this unchanged full suite on continuation. Section459 permits only backed-up, exact-identity SceneTemplateSettings cleanup after relevant Unity processes end. Section457 still requires QA/normal APK closeout followed by pause; device and independent product review remain pending.

Design interview started2026-09-30 under packet465 at the user’s request. Q1 keeps the selected branch outcome for the same ad source. Q2 preserves B’s ordinary sword/crafting debit and restores the original3 ad iron that B never received, without importing A’s axe. Q5 now applies the same original-reward rule to an ad XP card never received by B: restore the50 XP card, not A’s converted50 XP. This supersedes the old converted-card rule for that case; direct XP rewards and services are separate. Q3 clears the fallen character’s own ongoing negative statuses, with existing cooldown rules unchanged. Q9 consumes ad-origin materials/battle items first, then ordinary units. Q10 still consumes a toggled-on poison arrow against a poison-immune target: normal damage applies, poison does not. Q11 preserves teammates’ existing shields until their original expiry after the caster dies. Q4 is overseas Google Play; Q6 offers both Google account and verified email after voluntary guest binding. Q8 now confirms both password and numeric-code login for the same email account, chosen by the player; normal launches remember login. Q22 permits changing and unlinking login methods after identity verification while retaining at least one valid login method; the game account, progress and reward-source records remain the same. Q7 makes Excel/CSV the primary configuration authoring source; JSON may remain generated output. Q12 selects English and Simplified Chinese; Q13 defaults from the system language (Chinese to Simplified Chinese, otherwise English), with a remembered manual setting. Q14 keeps only one level16 first-clear certificate when A's ad first-clear overlaps B's ordinary first-clear, and requires the reward UI to label it as a first-clear reward instead of a popup/toast. Q15 prompts for a downloaded update at a natural page boundary, with deferral allowed. Q17 selects Luban for authoring-time table generation feeding the existing publication flow. Q16 is decided: retain Unity2022.3 and validate public2022.3.62f3 as the release-preparation candidate. The user selected the Intel Mac editor and requires continued Windows development; platform-specific tooling still needs validation. Q19 selects Luban text tables with project-owned localization bindings and YooAsset resources. Q18 confirms one character per profession: direct ad XP maps to the selected progress character of that profession and is deduplicated by the original ad source. Q20 custom-backend effort, difficulty, overseas hosting and CDN assessment is complete in docs/system-design/2026-09-17/backend-self-hosting-research.md, with later Go/sqlc and Tencent low-cost addenda. Q20A now selects Go with sqlc for the backend. Q33 later explicitly selects MySQL Community after revisiting an initial PostgreSQL choice to consider project learning and future employment. Tencent overseas 2GB/4GB examples are CNY30/42 per month; earlier USD VM examples are not minimum prices. R2 low usage can remain free; Tencent general overseas CDN is also a candidate. Q20B hosting/maintenance for the full account scope is deferred to the later account release; the minimal ad deployment still needs its own plan. After deferring Q21 to understand Tencent and R2, the user explicitly chose R2 following the clarification that monthly usage billing has no fixed starting subscription fee. Use R2 Standard with Cloudflare caching/CDN; the earlier conditional Tencent-first preference is superseded. No cloud account or service has been provisioned by this decision. The report gives planning estimates, not measured capacity or a delivery promise. The user also requires conspicuous prefab text placeholders replaced by runtime localization; missing replacement must remain recognizable. AGENTS and the uGUI correction plan carry these constraints. No auth provider or installed engine upgrade is approved by this discussion; the localization direction is selected, while exact versions and adapters remain to be planned. This is design discussion, not resumption of the paused main goal or authorization to implement proposed choices.
