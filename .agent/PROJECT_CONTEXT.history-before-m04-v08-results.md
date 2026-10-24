# FightMatch project context

Updated2026-10-08. Start with [START_HERE](../START_HERE.md), [current team snapshot](../docs/team/2026-09-30/README.md) and the latest explicitly assigned packet. The user requested continued work; their pause-status question did not cancel it. No active Goal object was observed. First Demo acceptance is incomplete.

## Observed facts

2026-10-08当前恢复：人类在中央actual01a11a60-d890-7660-b7ee-1accdb3ffe0c/item01a11a60-d8c2-71f0-b832-a4e0c8db237e明确批准LOC一次18官方归档/约25MB/三restore/外置盘/15分钟重试及中央读取。派发已成功，LOC actual01a11a62-0e7c-7553-aa47-c16cd6a276b9，M04中央新绑定就绪；旧拒绝和M03失效记录保留，尚无材料完成结论。PR20 V08 FIX01 head a360ad864171a3b49f7785ab7c797028d583d5c0 已收到GitHub6054341623 no-major、summaryCompleted；QA actual01a11a63-574e-7001-8edf-f4e963fa3204核收，中央resource-combined-v08-fix01-source-verdict.json限定ACCEPT。C已接一次V08 I→T89新包，实际结果待收。两条线分别写外置M/W和独立P/K/E，普通保护门不放宽。

- Unity project at `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`, containing FlowPuzzle/FightMatch source, assemblies, tests and delivery/design records.
- Current Editor: Unity2022.3.18f1 Intel on the external drive. Test Framework1.1.33 is present. No generally accepted CI/lint command; use only the scoped build/test entry and evidence contract.
- Shared resource adoption is complete at1036 inputs/32657794 bytes, canonical `61f1364be566b5266468ea38a19caba1d172428dc7e5fda06b97398c89c83ba8`. The first combined I ran with Unity exit0; the supervisor failed on .NET diagnostic socket/FIFO and T89 never started. Native evidence and restoration were independently received; central verdict is NEEDS_FIX. PR19 closed only its source-review gate.
- 资源组合验证V05真实FAILED：I1/Unity exit0，T89未启动，115.290570秒/含准备275.290570。新原始FD证据已定位TMP与Bee直接子目录之间多一条斜线，以及短命子PID身份查询退出竞态；无旧2.561ms超时。QA已核P1035/43compiler恢复、owned0。V06源码修正34/34回放通过并推PR20 head d8578a3，GitHub6051513575完成、源码门已接收；C actual01a1198b-3beb-7052-b04c-5cd441a5eee7实际FAILED：117.263279秒，T0，临时Bee端点消失及目录变化使闭合门未通过；独立恢复已实收RECOVERED（2.193398秒/OS0），P已回1035，43编译文件/62原前像/58移出档案核对通过，当前推进V07监护生命周期修正；不追认V05通过。见docs/team/2026-09-30/resource-combined-v05-native-verdict.json及central-resource-combined-v06-source.md。
- Cloud input PR21 corrected its missing LocalePreferenceStoreTests meta and head92fb75c2549f31923bdda07b35032666f8157fdd is source-only ACCEPT. Actual cloud sync failed on unavailable proxy port8080; projection was never created and no native/dependency work ran. Local V04 continues independently.
- LOC material dispatch was rejected by automatic tool review; a specific confirmation card is pending. Independent work continues without bypassing that rejection.
- Old I01 native failure and subsequent recovery remain separate records. N07/N10/N11/I01 old dispatches and historical questions are not current tasks or reusable allowances.
- Local master/WIP remain based at125b849; origin is `https://github.com/yyczz1/FightMatch.git`. Remote review branches are separate draft checkpoints, not merged/released acceptance. Never reset user changes or stage `.claude/settings.local.json` without exact authorization.

- 当前V07 FIX02已审源码head `6da2ad59c99fd88c732ff56a8ca415aafa4b2cf2` 的文档P1已在95e21ca修正并由QA机械接收；代码字节不变，中央源码限定ACCEPT。C actual01a119ff-3be9-75a0-bf48-2a6464c1ea93实际V07 FAILED：I1/Unity0、T0、95.126369秒，child82401身份不完整；QA已核P1035/43恢复及closuretrue。当前定位这项新异常，不原样重跑。GitHub连接器已恢复。

- V08原head ce6c0728独立审查review5452286080发现P1 4215462510（cwd名称缺描述符仍被接受）。FIX01作者actual01a11a22-678b-7b43-a80c-179df9eda21a仅修details；inputs字节不变、其余103函数不变，15相关案例一次2.037791秒通过。已推PR20 head a360ad864171a3b49f7785ab7c797028d583d5c0，210发布叶/15020622B远端blob/size匹配，trigger6053659601。主测试actual01a11a27-e872-7e23-a73a-eff4741da301有限收件结束为PENDING；中央续读至06:35:27Z，GitHub仍Running（06:16:03Z开始），准确head无review。C未激活/native0。主程actual01a11a30-f8c1-7a50-9a56-f8d655d1f939核当前冻结窗口独立产品小包为NONE，详见engineering-demo-next-ready-2026-10-08.md。LOC具体18归档/约25MB/三次restore/外置根/15分钟重试审批已再次明确呈给用户，未实施。等待外部review及具体审批，无活动Goal或新增自动化；V07 FAILED/T0及已接恢复不变。

## Approved constraints

[AGENTS](../AGENTS.md) governs scope and protected files. Follow the [five-role workflow](TEAM_WORKFLOW.md); C alone executes local Unity until an explicit responsibility transfer. Primary roles use requested Astra/xhigh, mechanical work may use Luna. Receive real completion, preserve historical evidence, and perform only necessary new validation. GitHub PR Code Review owns code review; dots is preferred for supported automation but current readiness cannot be assumed.

Runtime UI is uGUI; UI Toolkit is for Editor tooling. Before UI work read the [correction plan](../docs/system-design/2026-09-17/runtime-ui-ugui-correction.md). English/Simplified Chinese use diagnostic prefab placeholders, Luban authoring tables, project-owned localization and YooAsset; do not introduce Unity Localization/Addressables. Demo/Android share code, business and save semantics. Placeholder art is permitted; selected commercial assets are not yet approved.

Preserve Mac/Windows compatibility and current versions;2022.3.62f3 is only a candidate. Approved Android minimumAPI24 and overseas Google Play first-release choices still require their own scoped implementation/device evidence. First release retains guest/local saves, rewarded ads with a minimal Go/sqlc/MySQL backend, small initial package and R2 content distribution; formal accounts/cloud saves and live walkthrough analysis are later work. Approved design does not itself authorize service purchases, deployment or release.

FlowPuzzle follows the approved [design](../docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md) and [implementation plan](../docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md): solution-first generation, no unique-solution detection, no full-board requirement, core separated from Editor UI.

## Context references

Use the role-specific [system design](../docs/migration/2026-09-28/SYSTEM_DESIGN_CONTEXT.md), [architecture](../docs/migration/2026-09-28/ARCHITECTURE_CONTEXT.md) and [planning](../docs/migration/2026-09-28/PLANNING_CONTEXT.md) only as needed. Current readiness and actual identities come from the team snapshot and exact packet receipts.

The previous long context, including original decisions, Windows migration facts and historical paused states, is preserved byte-for-byte in [the2026-10-08 history](PROJECT_CONTEXT.history-2026-10-08.md). Its old “current” headings describe their original dates. This entry-point correction prevents stale dispatch; it changes no product decision or execution permission.
