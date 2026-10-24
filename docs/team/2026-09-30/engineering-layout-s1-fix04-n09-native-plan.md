# N09 原生准备：差量合同

2026-10-06 · PREPARE_AND_REQUEST_NORMAL_COMMAND_APPROVAL。只先准备并申请新命令；N08一次已消耗，旧审批不可复用。

## 1. 权源、输入、责任

中央[准备单](central-layout-n09-native-preparation.json)6637B/SHA `b6672edabf3744ceebaa4d8a2b1c961e32ec6ce0c8f1329d17017c046a574925` 是本轮范围。继承[N08计划](engineering-layout-s1-fix04-n08-native-plan.md)15308B/`bb88211923c4cfb06701b149a4368f0f812f96782d05eb9e7345256948e1de9b` §2–6，以及 N08 `PExecution/runner.py`41088B/`30ea3a36f8cf43687bbedd7226607ed63c94665f3e0e1c9040c17af3c122bb8b` 的未替换守卫；仅本合同明确的身份、恢复前像、N09路径和诊断文件差量生效。

主程issuer／唯一收件：`01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a10cfe-1840-7340-9464-10480d50ebd4`。唯一执行者仍“FightMatch C-MAC-HISO 定向验证” `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`；Astra/xhigh派发后绑定fresh实际turn，不能沿用interrupted N08。主程只写本文及engineering.md、等C；中央只接主程。无新SYS/作者/本地审查。

仓库=`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`；L=`TestArtifacts/FightMatch/LAYOUT-S1-001`；E=`L/FIX04/N09`；投影=`L/projection`；B=`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`。固定 N09 `S/source-receipt.json`34298B/SHA `92818223d1dd0546135ff254a31157d9586f5b9a7b317eae1b8243e595967a00`，作者actual `01a10ced-ba1f-7611-b15f-3842623fa8f6`已completed；S六叶485568B、28静态通过只代表SOURCE_READY。主程SOURCE收件actual `01a10cec-0d7b-7852-baab-b0e62bad6105`；不复跑源码语义检查。

## 2. 恢复基线，不伪造旧收尾

[N08恢复回执](engineering-layout-n08-recovery-receipt.json)29670B/`71f8bf16628319483d3fc11537f26cc8b87569526da38cc1c78b54a26a91d056` 及 N08 `PExecution/before.json`625647B/`365fa497e2d901b1bfa771b07343aa0db296debf79d4a39e3c5b2fd246cc1d0c` 为可用前像来源。后者的projectionAfterSyncExpected作为当前投影产品预期，sharedInputs作为共享预期，再**仅覆盖共享B为N09**；同步后的投影预期也只替换B。C当前全清单比对仍必需，不能把新漂移静默收为基线；不读取或制造不存在的N08 after/receipt。原旧保护集合/叶集合、15源/37资源/7保护/2meta、513 GUID/51依赖、141文字/3输入/6 IDs不减。

主程2f3a5c已核以下身份，C准备/执行仍按原门核实：

| 项目 | bytes / SHA256 |
|---|---|
|共享／封存N09 B|195375 / `b7000d9d14c00c4bd0588afd734b7029b637048cee6485901f9581bbc4eddf45`|
|投影N08 B前像|189888 / `e471f4553341e9306633a054c35e58743e103b419e3314e64e1dd6b495a6b2bd`|
|投影Host.Editor.dll|203264 / `047542a56edfce007a1da7347caa8e0adf84fcb4b83f7ea67f3daa1560d48fa7`|
|投影Core.Tests.dll|1823744 / `f807f9f23730ccf48a7b7b7bf34736f117b562669564f2449768bca125b26698`|

T、Scene、Prefab沿恢复回执双端身份；当前锁状态由C只读实测封存，不能复用已中断进程租约。冻结现存N08全部13叶1717788B、恢复回执、N09 S六叶、中央准备单、本文及原旧证据集合。N05 `/private/tmp/fm-ln05.bxLSmcT6`、N06 `/private/tmp/fm-ln06.fKXEaN8N`、N07 `/private/tmp/fm-ln07.NHjkoP2j`、N08 `/private/tmp/fm-ln08.C3j0dwRD`改为**当前必须缺席**的历史损失条目；原因UNKNOWN，不重建、不删除、不写旧证据，不称旧收尾通过。更早六个已缺席TMP仍核缺席，任何其它漂移停报。

## 3. D文件与日志：先留证，再判定

全文按N09 source receipt的futureDiagnosticContract消费；原JSON字段/UTF-16键/完整序列及重复计数、firstDifference/contentEqual/orderedEqual解析保持。D文件替代旧overrideDiff=后缀，不能拿短摘要替代原数据或SequenceEqual。

- 原始sample/stage整行及行号、D现存叶与可得身份先登记；再做解析，次级文件/解析异常不得覆盖首个Unity错误。失败时保留部分文件原字节及实际大小、缺失/未绑定状态，不修复、删掉或宣称完整。
- 仅允许相对E根 `D/override-diff-00.json`…`08.json`，按零起sample序号一一对应；固定根/祖先/文件拒link、目录、绝对路径、遍历或额外叶。每文件≤1048576B、九文件合计≤9437184B；有界读取、严格UTF-8无BOM。主log仍≤9×8192B sample、8×2048B stage、合计98304B；D另计且纳入总证据预算/监护。
- 成功sample必须overrideDiffComplete=True；先核实际文件长度/SHA256与该条绑定，再严格解析完整JSON。原first-occurrence字典、整数而非bool索引、UTF-16四字段、null/空/字符串0、完整target/reference、顺序/重复/count/delta/基线一致、几何基数和旧真假门全部继承。另核Original/Expected/Actual长度及FirstDifference/OrderedEqual/ContentEqual短摘要与JSON一致。
- `failure=`自由文本不能再用全行空白regex截取；从右端完整、固定次序的 `cleanupFailures`、`diagnosticsIncomplete` 与新增 `overrideDiffPath/ExpectedBytes/Bytes/Sha256/Created/Complete/FileAttempts/TotalBytes` 元数据边界分离，并将首错原文独立保留。已保存stage和finished均按各自字段核；cleanup-error沿旧firstFailure/secondary规则，不能当成功stage。无文件时的空值/-1与失败的部分文件按source contract记录，未知不填0。
- 成功须原九sample/两stage/三阶段门完整，D恰好九个经绑定核实的完整文件、无额外/重复引用、attempts9、累计字节一致；末端last-file为08。save-returned反映先前三个sample文件，不错误要求它已达到九个。文件成功但后续log/产品门失败仍失败；失败中出现未被sample引用的文件，只记未绑定诊断，不能当成功样本。原始JSON字段内容不因摘要或文件迁移改变。

## 4. C准备与批准门

审批前仅新建E下 `I/activation.json`、`PExecution/runner.py`、`PExecution/preparation.json`，及一次 `mktemp -d /private/tmp/fm-ln09.XXXXXXXX` 自有空TMP（canonical≤40 UTF-8字节、0700、同UID、无link，冻结device/inode/birthtime，≤16MiB/512叶且保留）。只替换本次TMPDIR；UPM_CACHE_ROOT仍固定现有L/package-cache。不创建D/P/PExport、不同步、不启动Unity。

I使用fresh C实际owner、本主程issuer和固定输入；协议status=P_ACTIVATED仅准备状态，另记commandApproval=REQUIRED_NOT_GRANTED_AT_PREPARATION。唯一新runner从442行旧版做上述必要差量，≤540非空行；不压行、删门或另加runner/helper文件。准备检查≤3轮累计30秒，整命令外测，首轮通过即停止迭代；保留失败，不增加语义检查轮。仅AST/绑定/差量纯fixture，不执行runner顶层、C#、Unity、文件写或信号。fixture用内存字节/注入数据，覆盖>8KiB完整JSON、Unicode/null/空/0/重复/顺序、路径/长度/hash错误、缺失/部分文件不能通过、末端failure与新增元数据分离；不为fixture创建D。

I/runner/preparation最终封签后停改，C当前正常 `require_escalated` 请求唯一 `python3 -B '/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N09/PExecution/runner.py'`。审批说明要列B同步1、Unity1≤180秒、全门后save/关闭/只读复开/唯一共享Scene写回。pending给准确会话/actual/可见call ID/命令/封签身份；无实际批准不执行，不称中央文件或旧prefix已批准。拒绝保留真实原因并停，不换权限/路径/执行者重试。

## 5. 获批后的运行、唯一写域与收件

继承N08计划§5固定Intel 2022.3.18f1 Editor路径/身份、argv次序、cwd=投影；只把I/P/log任务路径改N09，不加-quit/Play/测试。preflight≤120秒按完整原保护和无重叠Unity/UPM核验；只同步B一次N08→N09，T/其余14源不写，不删DLL或清缓存。Host.Editor必须本轮实际Csc＋不同于§2的新DLL；Core.Tests无Csc且原身份保持记复用，有Csc如实记重编，无Csc却漂移失败。

Unity仅一次≤180秒含compile/import、retry0；监护≤2秒、cache≤1GiB、投影派生≤4GiB、单log≤8MiB、free≥2GiB。首次失败停止正常流程；自然收尾≤60秒，fresh PID/start/argv/后代身份仍匹配的owned每个至多一次TERM，再≤30秒，无KILL/外来信号/自动回滚。首错/次级错误、真实保存返回/磁盘与退出码分别记录；未知退出码/网络量保留null。中断只读恢复，不自动补跑。

所有原A门过才Save1；保存后关闭→Additive只读复开、非dirty/字节/保护/三JSON/exit0/owned闭包全部通过，且磁盘恰好只删五覆盖其余字节保持，才一次写回共享 `Assets/Scenes/FightMatchDemo.unity`。失败export0；若保存已发生如实保留隔离后像，不能回滚或跳门研究。Prefab/meta零写。NewScene/CloseScene无直接计数仍null/UNOBSERVED，路径推断单列。

E最多29叶/201326592B，精确白名单：S既有六叶；I一叶；D上述九叶；P仅native-migration.json/native-reopen.json/serialized-targets.json；PExecution仅runner.py/preparation.json/before.json/after.json/process.json/process-events.jsonl/editor.log/receipt.json；成功PExport仅before.json/receipt.json，无Scene副本。失败也封存已有raw/D元数据和原件，解析失败不得丢掉已读诊断。C实际完成并停写后交主程wait/read；编译/原生与未跑99测试、GitHub review、设备门分开，不自签产品ACCEPT。

技能适用说明：固定2022.3隔离执行合同覆盖Unity CLI通用安装/升级/连接live Editor步骤，不安装工具或包；主程不运行Unity。writing-for-agents仅用于此单一差量合同与可观察门，不新增重复设计报告。
