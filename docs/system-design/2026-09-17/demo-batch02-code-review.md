# DEMO-B02 独立代码审查

FM-DEMO-007A — VERDICT: ACCEPT

Scope: PASS；Acceptance criteria: PASS；Verification: 已独立核对实际文件、作者命令输出、日志及 XML。
发现：NONE。七项验收均满足，无需修正包；本结论只覆盖候选定义与入场资料校验。

审查日期：2026-09-21；审查任务 R：01a0c1cd-dce1-7ac3-8780-06163cb0acfc。
依据：system-task-packets.md §80～82、仓库审查/验证规则及 §81 获准设计上下文。
实现任务 A：01a0c1fb-248d-7321-8b69-9efd26b817f1；本次 turn：01a0c1fb-26b8-7fb1-abbb-809699906678。
已通过 wait_threads 确认该 turn 为 completed（revision 36），随后读取完整交付；未将中间文件或先前许可证阻塞当成交回。
R 全文审读五份新 C#、五份新 meta、交付报告，核对范围清单及直接调用的已接收能力；未重审 002～006 的旧算法。

## 七项验收与代码依据

下表 P 指 [BattleEntryPreparer.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleEntryPreparer.cs)，T 指 [BattleEntryPreparerTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleEntryPreparerTests.cs)。行号按本次 SHA 对应文件。

| 验收 | 独立核对结果 | 实际代码和通过的测试见证 |
| --- | --- | --- |
| ①固定资料接入 | PASS | T:21、657 直接使用 006 CaptureSource/CopyLevel，四例覆盖 L1/L3 × C↑/C↓，不重复坐标转换；保留 A/B、颜色、端点、E01/E02、槽和行动序。W1=100/20/10/6、range1；E01=15/10/0/0、Strike 3/5；E02=20/10/20/0、Charge→Strike 13/10；闪避0。p=1/5、C=1/4、倍率3/2 的合成条件及坐标未审定说明写入 SourceNotes。 |
| ②结构和映射拒绝 | PASS | P:104、127、170 按输入顺序核对面、Pair、颜色、敌实例、槽、行动序、端点及意图。T:151～438 覆盖缺项、显式赋值、重复、越界、同端、空集合、非法尺寸及行为形状，并断言原因/路径/无 Entry。T:90 的独立双面允许各有局部 A，保留次序和空槽；2×2 交叉端点只验结构。 |
| ③精确入场与职责 | PASS | P:65、211、222、230、240 使用原精确核和同一预算。T:117 保留 W4 的124、118/5、29/2、21/2；改变推荐等级不缩放敌人，2^80+1 及相邻等级/修订精确保留。四处 BigInteger、21处有理字段逐项检查缺值及分子/分母位数；域边界与错误均有见证。 |
| ④队伍和支持域 | PASS | P:38～71 校验角色/职业/原槽唯一、Warrior、已算属性来源、未学主动和最多一名战士，最后检查 Ready。T:478～520 覆盖重复成员、多战士、空队、不 Ready 和唯一 slot2；非空携带、未知枚举/功能/技能、非零闪避明确拒绝。 |
| ⑤候选与依据 | PASS | P:74～101 校验完整 Context，并以 Ordinal 逐字段、有序 SourceNotes、精确 DraftRevision 对比 StatsContext。T:449～472 覆盖每字段、修订、说明内容/数量/顺序/大小写差异。输入和输出只承载候选身份与定义，不读取玩家进度或授予发布/提交资格。 |
| ⑥深隔离和原子性 | PASS | P:11～18 先完成校验再构造输出，输入无写入，ExactMathLimitException 未捕获或改码。PreparedBattleEntry 及嵌套记录均只读，列表均新建后 AsReadOnly，FlowPos 按值复制；共享的 ExactRational/BigInteger/string 不可变。T:591、624、749、788 验证输入修改隔离、集合拒绝写入、失败无 Entry/输入不变、预算不足不返回部分结果及新预算重试一致。 |
| ⑦验证与范围 | PASS | 实际编译和全量 EditMode 均 exit0；XML 为851/851，新增178，原673按完整名称和出现次数全过。原309文件 SHA 不变，新增恰10份实施文件、1516行；五份新GUID唯一。详细证据见下。 |

标准与设计检查：纯求值器复用既有 Core/Tests asmdef；无 Unity/Editor API、依赖、配置、序列化或旧公共 API 修改，无单实现接口/注册框架或范围外功能。
根空 input/budget 先抛 ArgumentNullException；随后按根元数据→Context→Level/Face/Pair/Enemy→Members→Ready 返回首个拒绝，字符串空白视缺失且保留非空原文。
输入的可合法零值字段及 Charge 的可空字段记录是否显式赋值；缺项不会被默认零、false 或 null 静默接收。
Number/PositiveInteger 重验当前预算；EntryHp 上界复用 ExactRational.Compare，Context 修订复用预算整数比较，无 float/double/decimal、重新成长、校准 C 或随机取字。
PreparedBattleEntry 仅保存入场候选资料，未引入 SceneRevision、Phase、Random、EntryBaseline 或 BattleSnapshot；完整基线/快照仍属于未派发的 007B。

## 范围及被审版本

before 采集于 UTC 03:23:55.9479505，共309文件；A 的起始命令 exec-5be2432b-815a-4567-a63c-95f9ac6bb973 记录12个新路径均不存在及11份原 asmdef SHA。
R 重算 before 规范摘要为 ea20d11dd44a2486d3902e1225387c2acc623e9bf8b86404da36e1b10c341e4a，与起始工具输出一致；309项也与 R 原有基线逐项相同。
after 采集于 UTC 03:51:06.0641566，共319文件；R 实时重算摘要为 ba26d102e53096c71e95d0665d964371f7a5952829be58c1485601054a8b543c，与交回清单一致。
规范为 Ordinal 排序相对路径、TAB、小写SHA256、LF，UTF-8无BOM；旧309项无修改/删除，新增仅下表10项；全部既有 asmdef/meta 保持。

| 新增实施文件（仓库相对路径） | 行数 | SHA256 |
| --- | ---: | --- |
| Assets/Scripts/FightMatch/Core/BattleEntryInput.cs | 139 | 458d6266a155018c69e9a52ed40145c23335953f43e68dd9bf089eee35ee67c4 |
| Assets/Scripts/FightMatch/Core/BattleEntryInput.cs.meta | 11 | 4313bde049fd47db566ee84d49e1485ed1e94cb44c192c4de1e78fe6f625be76 |
| Assets/Scripts/FightMatch/Core/PreparedBattleEntry.cs | 221 | 5b7f482f98b8815bb0bbf23eacc45dd6154567dbeebb014d08548b4674c815b2 |
| Assets/Scripts/FightMatch/Core/PreparedBattleEntry.cs.meta | 11 | f4c0c235d2b06b54eb46766375c582cf99bff004f3f300f8d778b0d60cbb8014 |
| Assets/Scripts/FightMatch/Core/BattleEntryPreparer.cs | 267 | d641bb55773f3e4e93e0c15d98e24de464dd33a6e73e47683c3d57c7dbec21b2 |
| Assets/Scripts/FightMatch/Core/BattleEntryPreparer.cs.meta | 11 | ca86d300d580456446dc5c7446bf32b48d69d41b881cf28f212d5f1ae5ace57c |
| Assets/Scripts/FightMatch/Core/BattleEntryPreparationResult.cs | 27 | 7d1f497cbf7827cbcbc42a6afbd30ecffb63b80122fe691893370c0115aa29b1 |
| Assets/Scripts/FightMatch/Core/BattleEntryPreparationResult.cs.meta | 11 | 2e7c7dae2c1d7bbe1f384c11a4712a89565ea4c243f90655732231c081982064 |
| Assets/Tests/EditMode/FightMatch/BattleEntryPreparerTests.cs | 807 | 343d65177d362f1f94d25fbaf210133cbe3cf2f364b48c7b63773f13ecc43574 |
| Assets/Tests/EditMode/FightMatch/BattleEntryPreparerTests.cs.meta | 11 | bea156c01d50243cddf07175b88f77a701886cc30c8e616c1c477420191a17ef |

合计1516行≤1600。新meta由成功编译导入产生，编译日志226～230行的路径/GUID与当前文件一致；R 全 Assets meta 检索确认以下GUID各只出现一次：
0844180a3a26e9749b891967cad240a4；e0a7c085df6f99b4c8035dfd105b43c2；80534e4c30161b14f8e55147bb177980；139d32a461eff784b9565aa5c09ef1dc；5646c1dc66bb5024790a10b7b4373a21。
其余作者新增为 demo-007a-delivery.md（85行≤140）及 demo-007a-scope.json（267457字节≤300KiB）；包的回滚边界为这12份新文件。
R 核对 scope 所列17份规则/设计/依赖/基线XML摘要与磁盘一致；无包、设置、权限或其他旧资产的新增差异证据。
Git 起始/当前 tracked 路径一致，均为已有34文件、+2809/-597；普通 status 的新增差异仅本包 XML，折叠的未跟踪目录另由清单核对，不把旧改动归入007A。
RUN（R，只读）git --no-optional-locks -c core.safecrlf=false status --short、diff --stat、diff --name-only、diff --check，均 exit0；check 无输出。五份新 C# 另查行尾空白/冲突标记，0项。

## 实际运行及输入对应

以下为作者实际 Start-Process 命令；R 只读核验本次 turn 的命令、退出码和运行产物，未执行 Unity。
作者每次启动前核对无 Unity 进程，编译和测试串行，未杀进程。工具读取 WaitForExit、Refresh 后的 ExitCode，测试未带 -quit。

```powershell
$env:UNITY_EXE = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
$compileProcess = Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-quit','-projectPath','D:\Unity\UnityProj\FightMatch','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemo007ACompile.log') -WindowStyle Hidden -PassThru
$compileProcess.WaitForExit()
$compileProcess.Refresh()
$compileProcess.ExitCode
$testProcess = Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-projectPath','D:\Unity\UnityProj\FightMatch','-runTests','-testPlatform','EditMode','-testResults','D:\Unity\UnityProj\FightMatch\FMDemo007A-EditMode.xml','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemo007ATests.log') -WindowStyle Hidden -PassThru
$testProcess.WaitForExit()
$testProcess.Refresh()
$testProcess.ExitCode
```

RUN（作者）编译：UTC 03:46:32.3596039～03:46:44.6440909，PID40336，exit0；命令事件 exec-510b2bee-d3b9-430b-b7e0-2012b4714d51。
RUN（作者）EditMode：UTC 03:47:20.9961614～03:47:31.8646768，PID42408，exit0；命令事件 exec-42964936-e2b8-4f7f-a7c2-21398f66a05a。
日志确认 Unity 2022.3.18f1 (d29bea25151d)、正确项目/参数；编译日志214/217行为两份 FightMatch 程序集实际 Csc，220行 build success，521～522行成功退出；测试日志335行写入本包XML。
五份 C# 的编译前 SHA（exec-6bc11ca4-66f5-4880-86f7-ed77fd6163d5）、测试后核对（exec-9237a9ca-0272-4437-b056-3c33c48bb5b1）与当前上表完全一致，原309项也保持。
此前编译启动分别 exit199（许可 IPC 超时）和 exit1（无有效许可证）；用户回复恢复后本次编译/测试成功。失败过程的工具输出及SHA留在 scope，最终编译日志由成功运行覆盖。
两份最终日志的 error CS / Compilation failed / Unhandled Exception 均0项；仍含许可签名/令牌信息，编译日志另含 Curl error 42，未阻断上述实际完成的编译与测试。

| 交付/运行证据 | SHA256 |
| --- | --- |
| docs/system-design/2026-09-17/demo-007a-delivery.md | bb2cd05057c1c92cdf2f1a8aafd6fb9f3c7ed515dc3325f02e5cbf61b324caf5 |
| docs/system-design/2026-09-17/demo-007a-scope.json | da9770fd250339b02676eb881219923a3d91611abf04a02d942add378669c8e6 |
| Logs/FMDemo007ACompile.log | 009b1b1bdfe910e398e7e29b61120b033fe0886928a5e3da6edda7d8719e3c30 |
| Logs/FMDemo007ATests.log | bb5c113d891f7694bdafe69a53f490e8c140289685223ec301a9542f2b2175f2 |
| FMDemo007A-EditMode.xml | 631a94ee9347188c8e7bb1e645653121a08d4b54725dbaa2aee2285146591492 |
| FMDemo006-EditMode.xml（原673条基线） | c48ed5ac221c8effdcf14a29caa9f662137a341c653628e4305a181d379a9c49 |

## XML 独立核对与交回

R 实读 XML：执行区间 UTC 03:47:28～03:47:30，testcasecount/total/passed=851，failed/skipped/inconclusive=0；851个 test-case 全为 Passed。
新增178条均属于 BattleEntryPreparerTests，共35个方法；与源文件中的四候选、结构/数值/支持域、Context、隔离和预算见证对应。
原673条包含670个 Ordinal 完整名称。逐名核对 before次数=after次数=after通过次数，缺失、次数变化、非通过均0；scope内670条逐名记录也与XML一致。
重复名称 FightMatch.Core.Tests.BattleRouteValidatorTests.Validate_EndpointMismatch_PrecedesCellErrors(System.Collections.Generic.List`1[FlowPuzzle.Core.FlowPos]) 的4次出现全部保留并通过。
原 fullname+TAB+次数+LF 的两侧规范SHA均为21bfa132e4ea736cf2fbc291b5d3c02686eda3be99b7de3efc538e2c932829e1；没有只凭测试总数接收。
NOT RUN（R）：Unity、项目自动测试及实现复现；§82明确禁止，采用实际命令输出、静态代码和产物交叉审查。未运行 Player/PlayMode/真机、真实入场、正式随机回放或发布，均非007A验收范围。
剩余边界：调用期间输入不得并行修改；预算不覆盖输入词法、总堆内存或完整行动工作区；正式坐标/C审定、Published接入、真实进度和完整快照留给所属后续包。
本轮R唯一写入本文；B01报告SHA仍为086d4a6836131b167d6dc9361770238e6f17e4233c8933552e8774512f1fec24。无代码、测试、meta、运行产物、协调稿或Git写入。
修正包需求：无；当前用户动作：无。结果交SD00登记；后续批次仍由SD00按流程安排。
