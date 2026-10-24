# LAYOUT-S1-FIX04-N08-NATIVE — 准备与具体命令审批

2026-10-05 · PREPARE_AND_REQUEST_NORMAL_COMMAND_APPROVAL。此合同允许准备新 N08 执行文件和提交正常人工命令审批；不是人工运行批准，不重置已消耗的 N07 次数。

## 1. 权源与唯一责任链

全文遵守[中央准备单](central-layout-n08-native-preparation.json)：6004B/SHA256 `bc545372f7ae8f5db9a5996944a818812fa6bdeabde6620f2c190c9419199df8`（已含 PExport 与调用次数证据澄清）。用户 AGENTS §6 同项目团队派发及已授权真实场景修复为准备权源；实际新运行须由 C 的正常 `require_escalated` 命令审批决定，历史人工授权仅作历史记录。

主程 issuer／唯一 C 收件：`01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a10bb8-ed01-79f1-a05d-83299ca3f36a`。唯一 C：“FightMatch C-MAC-HISO 定向验证” `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，派发 Astra/xhigh 后自读并绑定 fresh actual，不能沿用已结束的 `01a10b70-2924-7a62-8e60-d645db4c7078`。主程 wait/read 接实际准备／审批／完成，中央只接主程；不再创建会话、代理或双重接收。

## 2. 固定输入与只读继承

仓库根为 `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`；下文 L=`TestArtifacts/FightMatch/LAYOUT-S1-001`，E=`L/FIX04/N08`，投影=`L/projection`。旧文件只读：N07 `PExecution/runner-manual.py`（34356B，374非空行，SHA `eb8388b48242689fae092da17eba23b2057d654fcafd99db415bf746915197b2`）、`I/activation.json`（95693B/`cfee8f52034f327960e7f975aef4ed89614aef43440b2563434fa0ae9cdd36f6`）、`PExecution/receipt-manual.json`（52934B/`b4b66ec5b10867d48bbaf839dee6d9eb98a6eed86d0d4d2cf3c00da6fa4a2548`）及其真实 before/after/process/log。继承 N07 source-plan §7 的未被本包明确替换的原保护与原生门，不继承旧 owner、旧审批、两源同步或双程序集强制重编条件。

当前源权威是 N08 `S/source-receipt.json`，34011B/`a78e6b4e7b5f79a30e3e65636a087eb30da9d862d17f8a74237b3697e17f160d`，作者 actual `01a10ba6-1512-7583-8e1a-5603f6044c15` completed；主程 source 收件 actual `01a10b9e-1723-7402-bdfc-3e1d104adda5`。其六叶477917B及完整 sourceS15After/native4/固定输入保持；源计划12400B/`556c7b24619d76c0614ce19a9ea545fd2b0737e60d62776a332f0aaa0ce5ad5b`。源码准备非产品接受。

|当前固定项|bytes / SHA256|
|---|---|
|共享 B：`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`|189888 / `e471f4553341e9306633a054c35e58743e103b419e3314e64e1dd6b495a6b2bd`|
|投影 B 前像（N07）|183600 / `4c99967906fe9afdd4f53d58db750acc7e8bbc1827f61e73577fcbc9b3b33116`|
|两端 T：`Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs`|40209 / `6e52750705ccdc18e26f2bf4591fa9d675bb1e641cb7bb396f9c6a7f804f6f52`|
|两端 Scene：`Assets/Scenes/FightMatchDemo.unity`|12900 / `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd`|
|两端 Prefab：`Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab`|1322960 / `0064bbee5e0d84dfdfd627310a9abada5093252bb26cf363d0646b38586f19d2`|
|投影 Host.Editor.dll（N07真实编译后）|198656 / `84ca60f9d3cf40e9e810f5f7bf97f9cbefb0c91440cca3169bc94e21a917a4b9`|
|投影 Core.Tests.dll（N07真实编译后）|1823744 / `f807f9f23730ccf48a7b7b7bf34736f117b562669564f2449768bca125b26698`|

主程 f7e95a 已核 B/T/两DLL及 source receipt，未同步。C 以 N07 实际 after.projectedAfter、before.sharedInputs 和 sealedEvidence 为原保护基线，应用 N08 sourceS15After **只有 B 变化**的叠加；N07全23叶与N08 S六叶、本文及中央准备单纳入固定清单。旧 N06/更早清单沿既有引用，不重建另一套权威或跳过原旧证据集合检查。旧TMP `/private/tmp/fm-ln07.NHjkoP2j` 按 N07最终回执真实154310B/5文件清单保护，不再当作空目录；更早TMP缺席/存在规则沿 N07已执行合同。发现漂移即报告，不还原、不忽略。

## 3. 准备白名单、顺序与预算

主程仅写本文和 `engineering.md`。C审批前只可新建 `E/I/activation.json`、`E/PExecution/runner.py`、`E/PExecution/preparation.json`，以及一次 `mktemp -d /private/tmp/fm-ln08.XXXXXXXX` 创建的本次自有TMP目录；新建前核目标不存在，已存在则停，不覆盖旧文件。准备文件用 apply_patch，静态检查命令/fixture放内存，不加脚本叶。TMP canonical≤40 UTF-8字节、0700、同UID、祖先无link、初始空，冻结device/inode/birthtime等真实身份；限16MiB/512叶、结束保留。环境只替换 `TMPDIR` 为此新目录，`UPM_CACHE_ROOT` 仍固定 `L/package-cache`；不改用户设置或安装/升级 CLI、Pipeline、Editor、包。

1. 自读 fresh C actual，核 §2 新输入及现投影 B/T/两DLL/lock/TMP状态；保留完整 before 身份。准备前共享与投影源码同步次数均0。
2. 全文读取旧 runner，生成唯一新 runner：只做 N08路径/当前租约/源及保护前像/新TMP、B单源同步、程序集事实、完整日志解析及相应证据叶变化；旧守卫/错误监护/资源/导出分支不减。上限450非空行（旧374＋最多76），不压行、删检查或另加 helper 文件避限；超限先停说明。
3. 至多3轮/累计30秒新准备检查：AST语法、固定路径/argv/源/身份和新增解析器的无副作用fixture。按整条命令外层计时，解析失败也记录，所有失败及纠正保留在 preparation.json；仅原预算内修正准备层机械错误，不改冻结产品或执行条件。不得重复作者44项语义检查或旧全树长验证。最终准备失败/超限不提交运行请求。
4. 固定唯一 canonical I；`ownerThread/ownerTurn`=本fresh C，`issuerThread/issuerTurn`=本主程；15源/37资源/7protected/2meta、argv/cwd/固定Editor/合同/源码回执、B唯一同步前像、两DLL、旧证据/TMP、预算与全部精确叶齐全。B的协议字段需要 `status=P_ACTIVATED`，它仅表示代码入口租约准备态；另明确 `commandApproval=REQUIRED_NOT_GRANTED_AT_PREPARATION`，不复制旧 `directCurrentHuman=true` 或声称中央是本次人工批准。I身份由runner常量绑定；preparation封存I/runner最终身份与全部实际命令/结果，不制造相互引用hash循环。
5. 当前I/runner最终封签后停改，再按 §6 提交具体正常命令审批。新 I/P 不使用 activation-direct/manual 或备用路径；审批后也不改已封存I/runner。若 actual结束或输入改变，不沿用租约重启。

无副作用fixture仅测试从runner AST提取的纯解析函数，不执行runner顶层、文件写、进程、信号、Unity或产品C#：至少验证包含空格/引号/反斜杠/CRLF/中文及非BMP字符的键，null字段/空/字符串0/null modification，重复项/增删/改值/引用/纯顺序差异，坏长度/索引/计数/截断或JSON后缀多余数据应拒绝。这不是99测试或新产品测试。

## 4. 新日志解析合同（原通过条件保持）

读取 source-receipt.observationFormat 和冻结 B 的 `LayoutLogSceneCanvas`、`LayoutDescribeOverrides`、`LayoutRecordCleanupFailure`。仅替换旧 `phase_evidence` 的解析/证据适配及相关error pattern；不对新源码追加观测调用。

- 每个 `[LAYOUT-N08-SAMPLE]` 原始整行和行号先保留；在固定 ` overrideDiff=` 边界分离prefix与**全部后缀**。旧空白 `key=value` regex只能用于无空格的旧标量prefix，禁止用于JSON后缀或冒称完整解析自由文本。后缀必须严格JSON、无尾随垃圾且字段齐全，保留raw suffix和parsed object；每条self-contained。
- `keys` 唯一完整字符串；original/expected/actual三个索引序列保留索引、顺序和重复，不去重排序；核索引类型/范围、expectedCounts/actualCounts/delta长度和实际计数、firstDifference/orderedEqual/contentEqual与重建值一致。键按四个十进制长度前缀解码，长度单位是C# UTF-16 code unit（非Python Unicode code point），`-1:`是null、`0:`是空，字符串`0`与null不同；`null modification`单列。四字段后无尾料；保留完整稳定target/reference身份，不用hash替代内容。
- 解码后原始/期望基线在同run各sample一致，具体差量和移动以完整键/位置/计数呈现；重复项不假设唯一配对。新 orderedEqual 与旧 `preservedOverrides` 应一致，pure reorder仍不能通过原SequenceEqual门。parser真实性/完整性检查不能把原False改True。
- 所有原三阶段前缀、token/event/call计数、九sample、几何数值基数、source/parent/driver/结构/scaler、三 decisive时点的 finite/positive/0.5、stale/顺序/five为空门原样保留。成功仍只有save-returned＋finished两stage；末端须正确去除新增 `cleanupFailures`／`diagnosticsIncomplete` 字段后核首错为空，另须cleanupFailures0、diagnosticsIncomplete=False。
- 失败时接受并完整记录新 `kind=cleanup-error` 的run/stage/token/event/phase、firstFailure与secondary原文；此类行没有Set/Save/Force字段，不套正常stage字段要求，不把它当成功stage。首个Unity失败仍是首错；解析或cleanup失败作为次级记录。若终态日志自身失败、截断、数据缺失或超限，证据标不完整且失败，不能因parser提前抛错丢掉已取得raw。
- 最多9条×8192B sample、8条×2048B stage、合计≤98304B（均按最终raw UTF-8）；保持每个旧几何/身份字段。完整性不足即失败且不导出，不增sample/Force/Queue/Set/等待。不将“日志没报错”或文件出现当三阶段成功。

没有独立NewScene/CloseScene成功计数日志时，实测次数=`null/UNOBSERVED`；固定源码成功路径推断单列依据。保留Unity实际原始行、三阶段/真实save事实、helper失败/次级cleanup与最后场景警告；不新增探针或篡改B，不编造创建/关闭实测计数。首错、helper返回值及原保护门保持。

## 5. 真正获批后的唯一执行与输出

Editor绝对路径 `/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity`，x86_64，86759760B/SHA `71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f`。argv沿N07唯一原次序：`-batchmode -force-metal -buildTarget StandaloneOSX -projectPath <投影绝对路径> -executeMethod FightMatch.Host.Editor.FightMatchAndroidBuild.RepairLayoutSceneOverrides -fmLayoutContract LAYOUT-S1-SYS-001 -fmLayoutInputManifest <E/I/activation.json绝对路径> -fmLayoutEvidenceRoot <E/P绝对路径> -logFile <E/PExecution/editor.log绝对路径>`。cwd=投影；没有`-quit`、测试、Play或新驱动flag。

真实命令批准后，原preflight≤120秒，包括共享/投影完整保护清单、旧证据/513 GUID/51依赖/锁/无重叠Unity或UPM进程、资源与TMP核验；未知或外来进程即停，不能杀掉它们。**只同步B一次**，前像必须N07 B、后像必须N08 B；T及其余14源不写。记录SOURCE_SYNC_INTENT/DONE各一次及两端身份。不得删除DLL、清缓存或touch源码强迫编译。

唯一Editor≤180秒含compile/import，retry0；Host.Editor必须实际本轮Csc＋不同于N07的新DLL身份。Core.Tests若无Csc且前后DLL为固定N07字节、相关输入保持则记录REUSED；若有实际Csc则如实记录重编及前后身份（允许同字节），没有Csc却出现未知DLL变化即失败。不要求强制两个DLL变化，不把复用说成新编译；没有测试发现或执行。

继承监护≤2秒/cache总≤1GiB/投影派生≤4GiB/单log≤8MiB/free≥2GiB；首次错误/超时保留并进入唯一收尾。自然≤60秒，之后每个仍活且fresh PID/start/argv/后代身份匹配的owned最多一次SIGTERM，再≤30秒观察；无SIGKILL/外来信号/自动回滚。记录真实PID、退出码、保存返回与磁盘变化（失败也记录）；网络接收未知则null/UNKNOWN，不能猜0。外层工具yield/轮询≤60秒，不把180秒Editor预算混同准备/收尾或聊天耗时。

只由Unity对投影Scene操作：原全部A门过才Save1；任何A失败Save0，不允许为研究绕门保存。保存后关闭→Additive只读复开，所有既定几何/覆盖/顺序/源Prefab/身份/非dirty/字节/141文字、3输入、6 IDs门、三个JSON及exit0、保护和owned闭包全过，再检查Scene磁盘精确只删五项、其余字节逐字保持。仅此成功路径将投影Scene逐字节写回共享 `Assets/Scenes/FightMatchDemo.unity` 一次；Prefab/meta/其他资源零写、零导出。失败导出0，保存若已真实发生只如实保留隔离后像，不自动回滚或补跑。

N08精确叶：既有S六叶冻结；I一叶；P仅 `native-migration.json`、`native-reopen.json`、`serialized-targets.json`；PExecution仅 `runner.py`、`preparation.json`、`before.json`、`after.json`、`process.json`、`process-events.jsonl`、`editor.log`、`receipt.json`；成功时PExport仅 `before.json`、`receipt.json` 两证据，不保存Scene副本。总20个允许叶，实际可少，仍受中央28叶/201326592B上限；不把剩余8叶视为自由白名单。N07全部原件/新S冻结，所有源/产品修改均禁止，唯一共享Scene写回仅在上述获批成功路径。

## 6. 审批、停止与交回

准备语法/绑定/fixture通过后，C在本聊天以正常 `exec_command(sandbox_permissions="require_escalated")` 申请运行唯一已封存 runner，形式为 `python3 -B '/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N08/PExecution/runner.py'`；准备时核当前实际Python路径/命令，不安装。无需宽泛prefix授权。justification必须清楚说明是**新的N08**、B同步1次、Unity1次≤180秒、全部门过才一次save/关闭/只读复开及唯一共享Scene写回、失败停止/无额外重试；不把旧卡或中央文案当已获批。

命令只有经工具真正批准后才能执行。pending时向主程给准确C thread/actual、可见approval状态、工具call/item ID（能取得才给）、完整具体命令、I/runner/preparation身份及sync/launch/save/export仍0；主程把新卡位置交中央，等待用户，不代点、不改权限模式、不索取一条泛化旧授权。若被拒，保留工具原始理由、无PID/NOT_RUN事实，停，不换路径/执行者/间接启动重试。未返回可验证tool approval时不得称批准。

准备失败或拒绝可在批准的preparation.json及尚不存在的receipt.json记录NOT_RUN与真实预算；原错误不覆盖。进入执行后由runner封存before/after/process/raw/receipt；C确认actual completed与owned闭包，停写交主程wait/read。回执区分本次source/compile/native/parse/save/export与未运行的99测试、GitHub review、设备门；准确列未知/推断，不签Demo或产品ACCEPT。主程不复跑旧验证器或本地代码审查，机械核身份/实际raw/范围后交中央唯一结论。

Unity CLI技能仅用于边界核对；固定2022.3隔离合同覆盖其通用安装/升级/新Pipeline/连接live Editor步骤，主程不运行Unity。writing-for-agents用于本合同单一责任、固定输入、动作门和可观察停止条件；不再另写设计报告。
