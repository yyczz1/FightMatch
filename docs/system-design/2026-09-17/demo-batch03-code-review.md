# FM-DEMO-007B 独立代码审查（DEMO-B03 / B03-R1）

裁决：**ACCEPT**。七项验收满足，未发现需要修正的问题；无需修正包或用户操作。
结论仅覆盖当前 W 无主动／空携带／E01/E02 支持域的完整候选入场初态。

审查依据为 [system-task-packets.md r64 §83～85](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md:1244)，SHA-256 `b35b7cecc63bca0e8141878dba64b480c536e9118627455a47368b28d7e00f82`。
已读取 AGENTS、.agent 审查／验证规则及 §84 许可设计输入；对规范与实现契约分别核对。
作者任务 `01a0c1fb-248d-7321-8b69-9efd26b817f1` 的本次 turn `01a0c238-7a84-7ad2-bbf4-9a3d15c15ed0` 于 2026-09-21 04:54:06 UTC 明确 completed，wait revision=63。
正式审查始于上述完成状态及完整交付报告之后，覆盖全部 12 份实施文件、直接依赖、范围清单与原始运行证据。
R 仅作静态阅读、SHA／GUID／XML核验及只读 Git 检查；本轮唯一写入为本报告。

逐项验收：

| 项 | 代码及行为证据 | 测试见证与结论 |
| --- | --- | --- |
| ①完整初态 | [快照构造](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleSnapshot.cs:31) 同一 Baseline，首面、空 LockedRoutes/PendingLinks、修订1、动作/敌阶段/面索引0、AwaitAction、Empty、W原EntryHp、首面敌MaxHp及意图游标0、贡献0/1；[基线封存](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleEntryBaseline.cs:13) 三域初值。没有画线、攻击或采样。 | [四组来源用例](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:21) 覆盖006 C↑/C↓经007A生成的L1/L3、E01 Strike/E02 Charge及所有初值，通过。 |
| ②身份／原槽／多面 | [Combatant/Pair键](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleCombatantKey.cs:25) 按种类及原始组件Ordinal等值，成员/敌字段分离、相等键同Hash；[成员及敌状态](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleSnapshot.cs:90) 保留原槽、StableOrder及Pair顺序，基线保留后续面。 | [原槽与多面](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:112)、[键等值](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:148) 覆盖成员slot2、敌slot7/2与order17/3、跨Face/Attempt局部A、分隔符、大小写/空格及缺组件，通过。 |
| ③随机／PRD齐全 | [流核验](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleStartAssembler.cs:72) 逐域Validate并要求w=0及当前核心等于初始核心；[PRD核验](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleStartAssembler.cs:83) 按输入顺序检查必填、重复、准确Ready/Crit绑定和显式f=0，最后查缺行。非零w/f直接拒绝，无清零、补流或补行。 | [随机缺项与消费](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:221)、[PRD错误](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:240)、[合法相同域/零状态](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:308)、[错误顺序](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:326) 覆盖准确拒绝码/路径和Start=null，通过。 |
| ④预算与不二次求值 | [入口与数值重验](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleStartAssembler.cs:10) 先检查根参数，再用传入预算遍历Prepared全部面/成员/Context数值，后查三域与PRD；所有检查完成后才构造。无新预算，异常原样传播；同预算生成精确零，保留原属性及EntryHp。 | [巨大整数/有理数](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:345)、[12种流数值路径](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:406)、[共享步骤](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:431)、[W4保值](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:458) 覆盖更小预算、f幅值、精确步骤边界、预消耗/重试及124、118/5、247/2；不成长、不回满、不缩放敌人，通过。 |
| ⑤隔离／重复 | [基线封套](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleEntryBaseline.cs:13) 新建只读三槽及PRD列表，不保留可变输入壳；[路线](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleSnapshot.cs:78) 深复制FlowPos列表；输出属性只读、构造器internal，既有不可变Prepared/数学值可安全共享。 | [重复组装及输入变更](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:487)、[只读图检查](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:704) 覆盖换流、改/清空PRD行与列表，旧结果和Prepared不变；输出列表拒绝写入，通过。 |
| ⑥所有权／范围 | [战斗随机当前态](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleSnapshot.cs:129) 只有Battle流/PRD；BaseReward/Bonus仅留在Baseline初值槽。保留候选Context，不提供恢复、快照安装、WithHp、Published、M12/SDK/Use句柄。未纳入能力没有虚构运行状态。 | [构造面与所有权](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs:517) 核公开入口、只读构造、三域初值与Battle当前态、五阶段、原始HP贡献字段；非零w/f拒绝由③覆盖，通过。 |
| ⑦实际验证 | 两次真实Unity进程exit0，当前源码与运行前指纹一致；319份旧文件不变，恰12份新实施文件、6个GUID唯一。 | R独立解析XML：961/961通过，原851条Ordinal fullname多重集合及各条Passed保留，新增110条；详细证据见下，通过。 |

测试通过007A公开入口取得Prepared，没有用反射伪造不可变数学状态，也没有改旧测试公开帮助方法。
当前007A支持域不产生额外非Ready队友；实现仍只遍历ReadyParticipants生成局内成员，并在Baseline保留原Members，不据此要求超域队伍构造。
Standards检查满足白名单、Core纯C#分离、既有数学核复用、只读模型和公开构造边界；未引入依赖、资源、配置或旧API变化。

被审文件与SHA-256：

| 文件（绝对路径链接） | 行数 | SHA-256 |
| --- | ---: | --- |
| [BattleCombatantKey.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleCombatantKey.cs) | 108 | `a1269c134e2564b7323c39ced88e7a9787819f1d37f5b498191723767efe1748` |
| [BattleCombatantKey.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleCombatantKey.cs.meta) | 11 | `55f4600174cd54d7b115b7f8e34776b6fe4ad8cf4148d8e3986477265016faae` |
| [BattleEntryBaseline.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleEntryBaseline.cs) | 76 | `3cdede0006e090eb63d8b15e2c999787ba62454b791571c84c6f07e351c500b8` |
| [BattleEntryBaseline.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleEntryBaseline.cs.meta) | 11 | `44732fdfb2ba38be3dce2f4e49524f126c62a691f17d9cb362415d4db5e8df53` |
| [BattleSnapshot.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleSnapshot.cs) | 154 | `f4583c3c6509800557bce1b96b74cbcb6b21da85dfe3ffe506994be98ef825bc` |
| [BattleSnapshot.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleSnapshot.cs.meta) | 11 | `cf917965123eb036fd00c8b266d565e19e0478e0a8f28d63093dbeec920b98b8` |
| [BattleStartAssembler.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleStartAssembler.cs) | 118 | `803bff5f1e3f9fde1f788ddfb932f97349db7df9f13aac756ef420d2f295d805` |
| [BattleStartAssembler.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleStartAssembler.cs.meta) | 11 | `581b97d690c31cb585561c907e6fbefdb5571e5fc054b4bdf821635896db768d` |
| [CandidateRandomInitials.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRandomInitials.cs) | 21 | `f7a769c2c8b8c9647278ff1f513762689817e463e6e70b8b279853f4ee8eeb12` |
| [CandidateRandomInitials.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRandomInitials.cs.meta) | 11 | `07a50983e25b7e6b21cfe007efcdb92aef9d0256a7e857a461ff491a424453b4` |
| [BattleStartAssemblerTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs) | 723 | `5c87422b014a25f22e68d24efef9305940943be1ba83f0ee5dee4a5fd2c39578` |
| [BattleStartAssemblerTests.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs.meta) | 11 | `40b2315119c84fc8a5d88cc6235645d9ae31f626ba2728b735140e681c71fbe7` |

合计1266行 ≤1600；6份C#及6份Unity生成meta均全文检查。新增C#无尾空白或冲突标记；meta保留标准MonoImporter空字段格式。
逐一读取GUID并全Assets查重，结果如下；没有改旧meta：

| meta | GUID | 全Assets出现次数 |
| --- | --- | ---: |
| CandidateRandomInitials.cs.meta | `6d96b07df06fe3245b5a6ab617462e39` | 1 |
| BattleEntryBaseline.cs.meta | `ffd12acb949a4564aaa94396f4199141` | 1 |
| BattleSnapshot.cs.meta | `a593ae8d18fa07340aa3f78cd67e92f8` | 1 |
| BattleCombatantKey.cs.meta | `aee15e9349555284ea7d11633f563f58` | 1 |
| BattleStartAssembler.cs.meta | `a57de963df368644fa345194c043bc38` | 1 |
| BattleStartAssemblerTests.cs.meta | `11f09d0beaaf60a4fbffe1c8918d7429` | 1 |

范围与输入完整性：

- [交付报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-007b-delivery.md)：112行 ≤140，SHA `cbccfecb259c49f13d97580e5e6ee5466679617bbc75b42339c19674c7ac0a22`。
- [范围清单](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-007b-scope.json)：118999 bytes ≤300KiB，SHA `277bb5e05b48bc334e8d2f2a8a93d323a07ed5bd6d2ecddeface7cbd1a5ac177`。
- before：2026-09-21T04:33:18.2287838Z，319项，14个新路径原先不存在；规范摘要 `ba26d102e53096c71e95d0665d964371f7a5952829be58c1485601054a8b543c`，与已接收007A after及R事先独立基线一致。
- 从最终JSON移除追加after并还原原结尾，原before文件SHA为 `c4fc27e83b391a4c7a39e475391717127524ccc55f60d303632575a266098331`，与作者修改前工具记录一致，before未被改写。
- after：2026-09-21T04:50:12.6047894Z，331项；重新计算的清单摘要与当前文件摘要均为 `24f0e94da6104daa0565ea00a46d541d65ab886d4d43676bd7a458f557f3496b`；旧319项SHA逐一保持，新增恰为上述12项。
- R事先Assets路径集合336项，交付后348项，仅上述12项新增，无删除；22项规则/协调稿/设计/前批报告/依赖配置/XML保护指纹保持。
- 只读Git status、diff --stat、--name-only、--check均exit0；既有tracked差异仍34文件、+2809/-597，未归因本包。diff --check不覆盖的新增文件已另行全文检查。

实际验证由A串行执行，R核对当前turn原始工具记录与落盘日志，未启动Unity。
A先确认固定Unity路径存在、没有活动Unity进程；通过Start-Process Hidden/PassThru启动，每次WaitForExit、Refresh后读取实际ExitCode，测试命令没有-quit。
实际启动参数如下（摘录自工具记录）：

```powershell
$env:UNITY_EXE = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-quit','-projectPath','D:\Unity\UnityProj\FightMatch','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemo007BCompile.log') -WindowStyle Hidden -PassThru
Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-projectPath','D:\Unity\UnityProj\FightMatch','-runTests','-testPlatform','EditMode','-testResults','D:\Unity\UnityProj\FightMatch\FMDemo007B-EditMode.xml','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemo007BTests.log') -WindowStyle Hidden -PassThru
```

| 运行 | 工具事件 | PID | 2026-09-21 UTC起止 | 实际退出码 |
| --- | --- | ---: | --- | ---: |
| Compile/import | `exec-1e031585-a8e4-4c3d-8056-130055f0fc2c` | 53244 | 04:46:20.4699934 → 04:46:32.6973396 | 0 |
| EditMode | `exec-f68c8bbd-d415-45d9-ac16-1d6bf410fb5c` | 16784 | 04:47:17.1677122 → 04:47:29.1343549 | 0 |

作者运行前2026-09-21T04:45:49.2753686Z的6份C# SHA与上表、after及R当前读取完全一致；当前12份实施文件也与after逐项相符。
[编译日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemo007BCompile.log:214) 记录FightMatch.Core及Tests实际Csc编译，220行build success，523行Unity最终return code 0；[测试日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemo007BTests.log:335) 记录XML保存路径。
两日志均标明Unity 2022.3.18f1。存在许可客户端Code 10/token更新诊断，编译日志另有Curl 42；其后授权解析及本轮运行成功，无C#编译error/warning。
编译日志202行Bee中间ExitCode 4对应随后DAG重建，最终构建及Unity进程成功；不把中间构建子进程状态当成最终退出码。

| 原始运行产物 | SHA-256 |
| --- | --- |
| [FMDemo007BCompile.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo007BCompile.log) | `c2f844c521cb5c69f3bc335be11bd709cf659d5f4d8a84b23da32be4a194efb7` |
| [FMDemo007BTests.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo007BTests.log) | `2dc47d5f1dbbd61fcef7a2b0f07ee297b502cbbd527081dc22355283750edd28` |
| [FMDemo007B-EditMode.xml](D:/Unity/UnityProj/FightMatch/FMDemo007B-EditMode.xml) | `9b0d12667df5a7bf3061377874efeb12ba1c694454446976ea50c763a8baf6e9` |
| [原007A XML](D:/Unity/UnityProj/FightMatch/FMDemo007A-EditMode.xml) | `631a94ee9347188c8e7bb1e645653121a08d4b54725dbaa2aee2285146591492` |

R独立解析XML根及全部test-case：007B testcasecount/total/passed均961，failed/skipped/inconclusive均0，961条结果全Passed；测试时间04:47:25Z至04:47:27Z。
用StringComparer.Ordinal字典核旧851条、848个不同fullname的出现次数，并单独核这些条目的Passed次数，缺失/次数变化/非Passed均0。
其中既有EndpointMismatch测试fullname原本出现4次，本次仍4次；未以去重集合或测试总数替代多重集合比较。
旧fullname按Ordinal排序，以fullname+TAB+出现次数+LF（UTF-8无BOM）计算两边摘要，均为 `5a8cc1a4d4bd42de84a06df600d2abf54a53deb765f89d8ef6abcce073c4c437`。
新增恰110条、21个方法组，全部属于BattleStartAssemblerTests；已逐条阅读其行为断言，对应上面七项验收。

实际未运行范围：R没有重跑编译/EditMode，也没有运行PlayMode、Player构建或玩家端集成；依据是A本次真实运行及R对同版输入/产物的独立核验。
结论不扩展到Published/H10接入、真实冻结/奖励提交、恢复/回退、战斗推进、额外职业/状态能力、序列化或总堆内存保护；这些不在007B验收范围。
发现问题清单为空，无需最小反例或修正建议。用户当前无需操作。报告及SHA交SD00登记，R不修改协调稿、不派后批。
