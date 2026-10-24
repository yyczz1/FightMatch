# LAYOUT-S1-FIX04-N08-SOURCE — 关闭修正与覆盖差量

2026-10-05 · APPROVED_FOR_WORKER / SOURCE_ONLY。本包不激活原生运行，不改变 N07 NEEDS_FIX。

## 1. 权源、责任和固定输入

[中央 SOURCE 激活](central-layout-n08-source-activation.json)为唯一上级范围：4319 B / SHA256 `f0f2b1a19bb1b517609b0892f9d613023eb7e42e249d1aa9a29c14c6a8bd0de3`。承接用户继续真实场景修复、普通技术问题自主处理及 AGENTS §6 的同项目派发授权，不扩展已用尽的 N07 单次 Unity 许可。

交付负责人／唯一完成接收：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，本 issuer actual `01a10b9e-1723-7402-bdfc-3e1d104adda5`。实施复用“FightMatch 首建设置兼容源码实施” `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，显式 Astra/xhigh；原 N07 actual 已 completed，本次自读并记录 fresh actual，不沿用旧身份。作者仅回主程，主程接实际完成后回中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。不唤醒 SYS/C/R/LOC，不创建代理。

先全文读中央激活、本文及[已接诊断](engineering-layout-s1-fix04-n07-readonly-diagnosis.md)（12275 B / `2d980f8cea1029febd1b57dbcc3124d033568ef2f93e28ee480ed77b41f00d3c`）；沿 START_HERE、PROJECT_CONTEXT、uGUI纠正及团队现行规则。诊断技能的复现/探针步骤被本 SOURCE-only 包覆盖；复用真实红证据，不再索取第二份 SYS 报告。

|输入|字节|SHA256|
|---|---:|---|
|B：`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`|183600|`4c99967906fe9afdd4f53d58db750acc7e8bbc1827f61e73577fcbc9b3b33116`|
|T：`Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs`|40209|`6e52750705ccdc18e26f2bf4591fa9d675bb1e641cb7bb396f9c6a7f804f6f52`|
|`Assets/Scenes/FightMatchDemo.unity`|12900|`0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd`|
|`Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab`|1322960|`0064bbee5e0d84dfdfd627310a9abada5093252bb26cf363d0646b38586f19d2`|
|N07 source-plan（同目录 `engineering-layout-s1-fix04-n07-source-plan.md`）|16368|`12c0e7bb6fca1842b1e3f258de8c7cc8a6f66066af30486154cd01aa119e0984`|
|N07 `PExecution/receipt-manual.json`|52934|`b4b66ec5b10867d48bbaf839dee6d9eb98a6eed86d0d4d2cf3c00da6fa4a2548`|
|N07 `PExecution/editor.log`|109057|`3efdfa45acbaf6150a53dc12abb8b0dc4f3b43f283ac2159d8cc5896fb427e5e`|

N07/N08 前缀均为 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/`。主程已核 B/T/Scene/Prefab 与上表一致；B、Scene 是旧 tracked WIP，T、Prefab 是旧 untracked WIP，均不可按 HEAD 重置。对 B 只比较本表前像。N07 全部证据冻结，旧 scope/事实沿[集成回执](layout-n07-integration-receipt.json)，不重新扫历史全树。

## 2. 精确写范围与预算

作者只改 B；只新建 `N08/S/` 六叶：`before.json`、`FightMatchAndroidBuild.before.cs`、`FightMatchAndroidBuild.cs`、`source.patch`、`static.json`、`source-receipt.json`。所有本地写入使用 apply_patch；二进制身份须核，保留现有换行。前像不可事后改写；候选与共享 B 必须逐字节相同。主程独占本文和 `engineering.md`，作者不写其他报告。

B 相对冻结前像的增行＋删行合计 ≤320，不通过压行、去检查或大规模重排凑预算。正式静态核对最多 3 轮、累计 ≤30 秒，计时覆盖整条检查命令，保留每轮命令、原始输出、退出码和耗时；修正检查脚本也消耗下一轮。仅最终文件大小/哈希封签不另算源码检查。需要超限先停交主程说明最小缩窄，不自行续跑。

T／全部测试、Scene／Prefab／meta、其余产品、Packages、Settings、Library/Temp/Logs、投影项目、N07及更早证据均禁止写。禁止 Unity/编译/测试/探针/保存/复开/导出、创建 I 或执行 runner、同步源码到投影、Git写入、外部服务与本地代码审查。

## 3. 具体 B 修改合同

行号以 §1 B 为准。允许 `LayoutSceneRepairRun` 增加自有空场景句柄、创建标记、原始覆盖键和次级错误字段；新增一个私有诊断 DTO（如 `LayoutOverrideLog`）和必要私有编码/关闭/清理错误 helper（`LayoutDescribeOverrides`、`LayoutCloseOwnedScene`、`LayoutRecordCleanupFailure`）。不新增公开 API 或通用框架。

**A．仅自有场景关闭。** `LayoutCloseOwnedScene(run)` 只关 `run.scene`，先核 `run == layoutSceneRepairOwner`、句柄有效且 loaded；不接受任意外来 Scene 参数。只读枚举 loaded scenes；若待关自有 Demo 是唯一 loaded scene，且本 run 未创建空场景，则调用一次 `EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive)`，立即保留返回句柄和创建事实，核 valid/loaded/不同于 Demo/path 为空/rootCount 0。已创建却丢失、变脏或含对象时明确失败，不创建第二个、不开 Single 兜底、不清空或关闭外来场景。已有自有空场景时核其原句柄和空白状态。只在另一 loaded scene 确实存在后 `CloseScene(run.scene, true)`，返回 true 后才清空 `run.scene`；失败不冒称关闭成功。

`LayoutContinueSceneRepair` 1570 的正式关闭和 `LayoutFinishSceneRepair` 1605–1610 的失败清理均走该 helper；Finish 仍只在自有 scene valid/loaded 时调用。`LayoutOpenRepairReopen` 1592 唯一 `OpenSceneMode.Single` 改为 `Additive`，保留自有空场景；首开 1635 的 Single 及其 clean-start guard 不变。校验仍仅限 `run.scene`，不把空场景计作产品。空场景从不 MarkDirty/Save/Export/Close，随本自有 batch Exit 释放；不改场景资产或退出策略。

保留第一失败 `run.failure ?? error`，每个 cleanup catch 另记阶段、异常类型与消息，再锁存（已有首错不被替换）；记录关闭、UnloadPrefabContents、观察磁盘哈希、终态日志各自次级错。不得因一个清理失败跳过后续清理、退订或 owner 清除。次级记录受原 stage 8条×2048B 及总预算约束，不增加 sample；若某条日志自身超限/失败，保留首错和明确“不完整”事实，使用有界错误摘要，不能递归调用同一个失败日志器或静默截断称完整。异常摘要截断必须显式标记；若无法安全记录，以失败返回，不放行。

**B．完整但有界的覆盖观测。** `LayoutOverrideKeys` 1412–1422 完整保留：四字段长度前缀编码（target 稳定 GlobalObjectId／path／value／reference），null 与空字符串、`0`、null modification 各自不同。入口从原有 `modifications` 冻结 `run.originalOverrides = LayoutOverrideKeys(modifications)`；从原 `preserved` 取得原 `run.preserved`，均在唯一 Set 前。禁止再读 GetPropertyModifications 或重新访问 Unity 对象来生成日志。

`LayoutDescribeOverrides` 只消费这两个字符串数组及当前已有 `sample.overrides`，输出 JSON 可逆载荷，附到原 `LayoutLogSceneCanvas` 的同一 sample 记录。用按首次出现顺序的唯一完整键字典 + 原始/保留期望/当前三个整数序列避免重复长字符串；每个序列位置就是原索引，不排序/去重这些序列。字典必须保留全键，JSON 转义无损，不能只记哈希。每字典项的期望/当前重复计数及其差额，以及首个不同索引、orderedEqual、contentEqual 只作诊断；完整序列可确定每项移动/重复位置，计数差额可确定全部增加/删除，完整键可定位改值/引用。null/空/0/引号/反斜杠/换行必须可区分并复原。编码字段名与解码说明写入 source-receipt 即可，不另造文档。

每条 sample 自包含原始基线＋保留基线＋实际序列，不依赖另一次采样或额外基线日志。共三阶段每阶段原三个时点，仍最多 9 条；即使第一阶段失败也保留已到达的完整记录。`LayoutSceneCanvasSnapshot` 和 `LayoutCaptureSceneCanvas` 数据读取/时机保持，仅 N07 消息字面量变更。不得更改原 `sample.overrides.SequenceEqual(run.preserved)` 或以新诊断比较取代它；所有几何、结构、stale、首错谓词原封不动。原几何和五项字段继续输出。

UTF-8 字节数按最终转义日志计：sample ≤8192B×9，stage ≤2048B×8（合计上限 90112B，自然低于96KiB；如新增任何日志须仍计入原限额）。超限在输出不完整记录/后续保存前明确失败，终态标示诊断不足；不能分裂额外sample、去掉字段或盲增预算。当前磁盘 27 条记录只是静态输入事实，不保证运行时条数或日志必能放下。

**C．N08 固定路径与字面量。** `LayoutGuard` 725 的 `sceneRepair ? "/FIX04/N07" : ""` 只换为 `/FIX04/N08`；726 异步消息中的 N07→N08。仍严格比较 `executionRoot + "/I/activation.json"`、`executionRoot + "/P"`、`executionRoot + "/S/source-receipt.json"`，不改变 contract/mode、issuer/owner线程、fresh turn、精确argv、资源及输入身份、create-once和 clean-start 守卫；不用 activation-direct/manual、路径参数化或回退。实际 I 不在本包生成。

仅 1378–1651 异步 block 内消息的 `LAYOUT N07`→`LAYOUT N08`；日志标签固定 `[LAYOUT-N08-SAMPLE]` 与 `[LAYOUT-N08-STAGE]`。`LayoutArmSceneRepair`／`LayoutUpdateSceneRepair` 只改这些消息文本，其调用、两update、5秒期限、token和 owner 检查保持。入口名、命令 flags、LayoutResult 格式和三 JSON 叶不变。不得全文件替换或改旧同步入口。

## 4. 保留门与验收

`LayoutStaleCanvasOverride`、严格源 Prefab 几何、`LayoutUsableSceneCanvasGeometry` 的0.5四角/有限正数、两个 `LayoutVerifySceneCanvas`、`LayoutOverrideKeys`、旧同步 repair/reopen/migrate/verify、全部 T 断言逐字保留。除以上限定替换外，所有原 Require／顺序和调用次数不变：Set1、Save最多1、Force3、Queue3、两事件×3、decisive3、sample9；失败阶段未通过时 Save0/导出0。Scene磁盘只删既定五项及其余字节、Prefab/meta身份、141/3/6 IDs、只读复开非dirty/字节保持、三JSON/唯一Scene导出、99测试及 GitHub PR/设备后置门均不降级。不得接受仅顺序漂移或删除 pivot 覆盖。

静态验收：①实际 diff 只落 §3 明示范围且 ≤320；②原地 B 与候选、before与冻结B、patch正反重建一致；③保护输入/其meta及旧失败原件身份不变；④上述不可改方法、T、调用位置/计数、N08固定 guard 机械比较；⑤编码覆盖完整、次级错与首错分离及自有关闭路径在源码中可追溯，标明尚未编译/运行。机械检查不是替代 GitHub 代码审查，也不证明 Unity 行为。

作者先写 before 身份/源码，再用 apply_patch 实施，生成统一补丁与候选；静态范围/格式使用冻结前像差量（可 `git diff --no-index --numstat`／`--check`，正确区分正常diff exit1），身份用 SHA256/bytes，只在批准六叶内保存证据。其余静态脚本如需使用，只可命令内存形式，完整命令/输出写 static.json；不创建脚本叶、不执行 C#／模拟探针，不复跑旧全树验证器。终核只限本次输入和六叶。

## 5. 完成回执与下一步

`before.json` 写实际前像、保护输入身份和fresh author/issuer；`static.json` 保留每轮真实结果和完整计时，包含失败；`source-receipt.json` 列六叶身份（本回执自身身份在最终消息给出）、B增删数、逐项范围验收、观测格式、技能如何约束范围、真实未执行项及未知。不得生成自我引用哈希或因封签循环反复重写旧证据。

作者完成停写，等主程 wait/read 接实际 completed 结果；简短返回路径／SHA／预算／actual，状态仅 `SOURCE_READY / UNCOMPILED / UNTESTED / NATIVE_PENDING / REVIEW_PENDING`，不签产品 ACCEPT。失败按 BLOCKED 模板保留现场和原因，不自行降门或启动 N08 验证。

主程接收机械事实后回中央：源码只修关闭及增可观测性，五条重现根因仍未知；只有新独立执行授权才可交唯一 C。未来必要运行要用新 actual、N08 canonical I、固定源码和新专用执行器，最多一次≤180秒/retry0；仍在原门失败时 Save0/导出0。当前既没有此运行许可，也没有“为诊断跳过前置失败而保存”的许可。
