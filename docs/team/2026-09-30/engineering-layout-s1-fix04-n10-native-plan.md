# N10 原生准备：差量合同

2026-10-06 · PREPARE_AND_REQUEST_NORMAL_COMMAND_APPROVAL。本单先准备新的具体命令；N09运行已消耗，不复用旧许可。

## 1. 权源、责任与固定输入

中央[准备单](central-layout-n10-native-preparation.json)8221B/SHA `b45d78814a49fadf11797e584a9f7df528249edd5b45acd8c0e97ea6262320fb` 定义本轮范围；[N10源码包](central-layout-n10-source-packet.json)8240B/`e8f1621a7bdca1e39dd813d8ac4b89aa8f3426d3bd13a7391a1af8887350fa94` 批准三项时点/分层变门。继承[N09合同](engineering-layout-s1-fix04-n09-native-plan.md)9767B/`4950fad06c6b7ef3b04af856f8d7b5f0ce2bae4e10e03d8582874f21e7b37077`及其实跑runner48051B/`0acf7a9620b2beb985a572a33ae44057aa278375367437324c99685b3e1b4e80`的未替换守卫；只以下述明确差量替代，不删除其它门。

主程issuer／唯一完成收件：`01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a10d59-e3ce-70a3-80fb-3ca012f4c975`。唯一执行者“FightMatch C-MAC-HISO 定向验证” `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh、派发后绑定fresh实际turn。C独占I/runner/preparation和获批后的执行证据；主程只写本文及engineering.md，中央只接主程。不另派SYS/作者/审查或新聊天。

仓库=`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`；L=`TestArtifacts/FightMatch/LAYOUT-S1-001`；E=`L/FIX04/N10`；投影=`L/projection`；B=`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`；T=`Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs`。SOURCE回执 `E/S/source-receipt.json`69758B/SHA `ccfa499f5dfc87acd7949149039c67f0a1e476e6c033548bd79e4f9f3a7a81a7`；作者actual `01a10d39-7657-7062-8b20-231f053baaf8`已completed，主程收件actual `01a10d51-5615-7f00-832b-6f1cf078de75`。S八叶739100B冻结；静态第二轮48项通过但实际tool尾部未能外核的既有边界保持，不复跑源码fixture。

| 输入 | bytes / SHA256 |
|---|---|
|共享及封存N10 B|204772 / `a64fbb31f03c2ec0a5fcd88da12ac4793e4d4c75d190965c6d7f148a4b4ebc27`|
|共享及封存N10 T|50694 / `c64edeac81aa6b22e1070b7e6919ba572016207777136f0d62b5cbe84ed256c2`|
|投影N09 B前像|195375 / `b7000d9d14c00c4bd0588afd734b7029b637048cee6485901f9581bbc4eddf45`|
|投影N09 T前像|40209 / `6e52750705ccdc18e26f2bf4591fa9d675bb1e641cb7bb396f9c6a7f804f6f52`|
|投影Host.Editor.dll前像|207360 / `7bc6007b78fc676171f0ebd8102164bbaff403b960d0ed61964c7bd4564b7599`|
|投影Core.Tests.dll前像|1823744 / `f807f9f23730ccf48a7b7b7bf34736f117b562669564f2449768bca125b26698`|

## 2. N09闭合基线与保护

N09 `PExecution/receipt.json`1407697B/`8f616262e138722715bd7e7317027beeeab31960125f67ec0f9d0222d5ec746c`；`after.json`1164892B/`ae34b03c78b111143b7050b03b917209e7fe6f358bc0ca412d44883ace1a18b2`；`before.json`628894B/`f912567027e5ee0b672d53165ac3961454f6bf3c0facf1f4bdefd749f0daca81`。投影以after.projectedAfter为前像，共享以before.sharedInputs仅覆盖N10 B/T；同步后的投影预期也只覆盖B/T。当前完整清单仍须比较，不能吸收其它漂移。

N09全部18叶4419164B、N10 S八叶及原冻结证据集合逐字保护；15源/37资源/7保护/2meta、513 GUID/51依赖、141文字/3输入/6 IDs不减。旧N09 TMP `/private/tmp/fm-ln09.lanj0jp5` 按after.temporaryRoot原身份、完整目录/三文件清单77503B保护。N05–08及更早已确认缺失TMP仍核缺席；历史损失原因UNKNOWN，不重建、不删除、不改写旧收尾。主程本次只读核上述关键身份及新I/PExecution/D/P/PExport缺席，C按原完整门独立封存。

## 3. 唯一运行差量

- B/T分别同步一次，固定§1双端前后像；其余13源、Prefab/meta/资源零写。不删DLL、清缓存或touch。Host.Editor及Core.Tests均须本次真实Csc记录和不同于前像的新DLL；N09的Core复用规则不适用于N10。不运行发现/测试。
- pre-save三个sample均在唯一Set前，首kind=`pre-update`、Set0/Save0、geometryTiming=`before-only-Set`；PostSave/Reopen沿原次序，geometryTiming分别`after-Save`、`after-readonly-reopen`。成功仍三阶段九sample、Force3/Queue3、六次update、decisive3。阶段/事件/token及调用计数必须按新时点准确匹配，不照抄post-set/Set1旧前缀。
- `observedStaleOverrides`与`preservedSequenceExact`如实保存，不强制旧false/true。完整D仍核original27/expected22冻结绑定、UTF-16四字段、null/空/0、序列/重复/count/delta/firstDifference/OrderedEqual/ContentEqual及全部摘要自洽。每份actual须完整保留22键及各自次数、相对顺序；其它项仅SOURCE回执exactMemoryEnvelope.allowed112Keys冻结精确集合且每项≤1。许可集合绑定回执身份及批准N09 D差量，不能从本轮actual反推、扩充或按属性前缀豁免。原22的完整逐序绑定保持，未知target/property/value/reference、重复、丢失、改值、换序都失败。
- 保存前三项新分层门全过才执行冻结源码的唯一Set→即时Get精确22→Save，无中间日志/Force/Queue/yield。save-returned及finished末端新增`immediateOverridesExact`、`immediateOverrideCount`；成功必须true/22、Set1/Save1。失败保留原始报告值；Set未发生时false/0为未观察，不是实测Get0；其它观察状态只在时序证据足够时确定，不加探针。
- stage解析从右端固定有序suffix分离`cleanupFailures`、`diagnosticsIncomplete`、全部overrideDiff元数据及上述两个immediate字段；`failure=`自由首错单独完整保留。不能沿用N09旧trailer判断；次级解析/文件错误不得覆盖Unity首错。保存stage只绑定此前D00–02，终态成功绑定全部D00–08；无直接计数的NewScene/CloseScene仍null/UNOBSERVED。
- D固定九名、拒绝links/路径遍历/额外叶、有界读取、严格UTF8无BOM、bytes/hash/complete/序号/总量绑定及原日志预算全继承。先留raw行/行号和原文件，再解析；部分失败文件、未绑定状态和可得身份不删不修，不称完整。
- Save后源码即时核固定instance/source/Canvas原五块精确删除，其余字节完全不变；PostSave及只读复开重复磁盘稳定、真实几何/身份/非dirty门。runner独立原精确差量门保持。Save后失败保留真实返回/计数/后像，export0，无回滚/补跑。全部三结果JSON、exit0、原保护、owned闭包、九D及几何/分层门通过，才一次导出共享`Assets/Scenes/FightMatchDemo.unity`；PExport仅before.json/receipt.json，不含Scene副本。

## 4. 准备、封签与正常命令审批

批准前仅新建E内 `I/activation.json`、`PExecution/runner.py`、`PExecution/preparation.json`；一次`mktemp -d /private/tmp/fm-ln10.XXXXXXXX`，canonical≤40 UTF8字节、同UID/0700/无link/初始空、冻结device/inode/birthtime，≤16MiB/512叶并保留。仅TMPDIR换新，UPM_CACHE_ROOT仍L/package-cache。此时不同步、不创建D/P/PExport、不启动Unity。

runner从N09实跑532非空行作必要差量，≤640非空行，不压行/删守卫/加旁路文件。准备≤3轮、整命令累计≤30秒、首过停止；仅AST/输入/封签和新差量纯内存fixture（字段/阶段计数、精确许可及损坏拒绝、geometryTiming、immediate字段、自由首错与新trailer）。不执行runner顶层/C#/Unity/信号，不创建D或重跑SOURCE语义fixture。失败保留原始输出与实际用量。

I绑定本主程issuer及fresh C实际owner；`P_ACTIVATED`只代表准备，并标commandApproval=REQUIRED_NOT_GRANTED_AT_PREPARATION。三文件封签停改后，由C当前正常`require_escalated`提交新的唯一命令：

`python3 -B '/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N10/PExecution/runner.py'`

说明准确列双源各同步1、Unity1≤180秒、允许的Save1/关闭/只读复开及全门后唯一Scene写回。无工具实际批准不执行；旧N09/N08 prefix、中央文件或SOURCE通过不等于命令批准。pending回准确C聊天/actual/命令/可得call ID和三封签；拒绝保留真实理由即停，不换权限、路径或执行者重试。

## 5. 获批执行与完成回路

继承N09/N08固定Intel 2022.3.18f1 Editor身份、argv次序、cwd=投影，只换N10 I/P/log路径；无`-quit`/Play/测试/新参数。preflight≤120秒，完整保护和无重叠Unity/UPM核验后才双源同步；Unity一次≤180秒含compile/import、retry0。监护≤2秒、cache≤1GiB、投影派生≤4GiB、单log≤8MiB、free≥2GiB；D九文件各≤1MiB、合≤9MiB，主log九sample各8KiB/八stage各2KiB/合96KiB。首错停止正常流程；自然收尾≤60秒、fresh匹配owned每个最多TERM1后≤30秒，无KILL/外来信号，真实退出码/未知量分列。

E精确最多31叶、总≤201326592B：原S八叶；I一叶；D九固定叶；P仅native-migration.json/native-reopen.json/serialized-targets.json；PExecution仅runner.py/preparation.json/before.json/after.json/process.json/process-events.jsonl/editor.log/receipt.json；成功PExport两JSON。其它路径不新增。禁止112候选发现/测试、Play/Host/设备/APK、Git/审查、LOC、依赖/引擎/配置及任何产品源码修改。

C在准备/审批pending/终态有实质事件时主动回主程；结束执行、封存并停写后报告actual/具体工具/首错/计数/两编译/保护/收尾/导出及证据身份，然后结束回合。主程用wait/read确认实际completed并收件回中央；一位收件者，不无变化轮询或自动重派。原生通过也不等于112测试、GitHub审查、设备或产品ACCEPT，下一门另签。

技能边界：unity-cli用于固定Editor/进程边界；本批准的2022.3隔离合同覆盖通用安装/升级/live Editor步骤，不增工具或包。writing-for-agents仅形成这一份短差量合同，不新增设计/复核报告。
