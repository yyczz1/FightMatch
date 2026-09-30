# D8 测试准备成功结果复用交付

状态：**限定实施与六槽验证完成，等待原R实际审查；4181全量及≤600秒目标仍未验证。**
C thread `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / local，turn `01a0f0cb-08ae-7682-b782-7388a3672569`。依据 [PLAN D8条件执行签发](PLAN.md)，原D7的848.534552秒性能未达标记录完整保留。
原/新实际18次用例全Passed，每次CA01为148断言、CA09为59；前后各976、合计1952断言。没有新增、删减或改写测试方法。正序总用例耗时4.915650→4.420837秒（减少10.0661%），反序4.222209→3.778686秒（减少10.5045%）；这仅是同编译参数的独立诊断收益。

**唯一源差分与前置门**

仅 `Assets/Tests/EditMode/FightMatch/PlayerSessionTestData.cs` 的RealCatalog改变，并新增两个私有字段，+16/-5共21≤50行，其余原字节原样保留。原23124bytes/SHA `7db9770560a13f17476eb59c47f7e9c983c1518a4520b59949883e6638dc12d4`，新23625bytes/SHA `1ee623417a1a488c6e8b0f0789868baae8115540c33c9a754615fc61f8a1d05f`；[准确备份](fixture-reuse-sources-before/Assets/Tests/EditMode/FightMatch/PlayerSessionTestData.cs)未变。
三个before均完成后才改helper。before-trace单线程1142 ENTER/1142 LEAVE完整配对，三次RealCatalog各真实调用一次同首包Create，依次0.978230/0.258040/0.252160秒，总1.488430秒÷完整用例和4.8560334秒＝**30.65115%≥20%**。七输入每槽均与原Assets逐SHA一致，首次cold没有预热。
新增共享内容仅为成功的PublicationResult及D3已封闭的不可变FirstReleaseContentStorage；私有同锁保护六文件惰性读取、结果检查和成功发布。首次/失败后尝试仍创建原新预算，失败结果或异常不发布成功缓存。锁外每次保留原Content(result)成功断言，再new独立Catalog。RealSource、玩家、存档、Locator、预算和架构状态均保持每次原路径；专门54项FirstRelease回归源未变。
新固定源指纹 `d28ac0623c6eddb3374dcecf32fb9aed8f8b26f0d311ec5d9c9477c45294ce90`；原起点dfe22e…984a。完整身份、差分、122项只读复核、各叶SHA及复算代码见 [scope](fixture-reuse-scope.json)。

**六次准确执行与冷/暖证据**

cwd为本FightMatch根；按下列顺序各执行一次，全部编译/探针/包装器exit0，无超时；中间仅在三个before结束且量化门通过后修改唯一helper。
```text
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_fixture_reuse_probe.py --label before-forward01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_fixture_reuse_probe.py --label before-trace01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_fixture_reuse_probe.py --label before-reverse01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_fixture_reuse_probe.py --label after-forward01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_fixture_reuse_probe.py --label after-trace01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_fixture_reuse_probe.py --label after-reverse01
```

CA01完整方法：`FightMatch.Core.Tests.PlayerRosterSessionTests.CA01_RealApprovedWarriorMovesThroughAllSlotsAndEmptyWithoutReplacingItsOriginalIdentity`。
CA09完整方法：`FightMatch.Core.Tests.PlayerRosterSessionTests.CA09_PrepareFreezesRevisionAndSlotsAndOriginalOperationWinsBeforeCurrentHeadAdmission`。
forward/trace按CA01→CA09→CA01，reverse按CA09→CA01→CA09。每槽全新Mono进程、仅加载一份本槽新编测试程序集；序列每次new真实NUnit fixture/执行上下文/WorkItem，未反射改私有静态状态。首项含真实cold及首次JIT，后两项为同程序集内顺序执行。

| 原始result | 三次WorkItem秒（首项cold） | 用例和秒 | NUnit和秒 | Mono进程秒 | 完整命令墙钟秒 |
| --- | --- | ---: | ---: | ---: | ---: |
| [before-forward01](fixture-reuse-runs/before-forward01/result.json) | 2.753302 / 0.848569 / 1.313778 | 4.915650 | 4.911695 | 5.359068 | 16.580472 |
| [before-trace01](fixture-reuse-runs/before-trace01/result.json) | 2.707887 / 0.835534 / 1.312613 | 4.856033 | 4.851068 | 5.164345 | 10.882285 |
| [before-reverse01](fixture-reuse-runs/before-reverse01/result.json) | 2.254623 / 1.302616 / 0.664970 | 4.222209 | 4.217686 | 4.557919 | 10.469680 |
| [after-forward01](fixture-reuse-runs/after-forward01/result.json) | 2.753739 / 0.600835 / 1.066263 | 4.420837 | 4.416594 | 4.751589 | 13.507520 |
| [after-trace01](fixture-reuse-runs/after-trace01/result.json) | 2.723038 / 0.624760 / 1.062908 | 4.410707 | 4.406341 | 4.708990 | 10.308630 |
| [after-reverse01](fixture-reuse-runs/after-reverse01/result.json) | 2.273894 / 1.074131 / 0.430661 | 3.778685 | 3.773640 | 4.205411 | 10.163643 |

正序首cold为2.7533025→2.7537388秒，反序首cold为2.254623→2.2738938秒，未声称cold变快。正序两暖项分别减少29.1944%/18.8399%，反序两暖项减少17.5405%/35.2361%。一次正/反序及一次trace不足以构成统计确定性或全量预测。
全部17个完整原测试源直接编译，既有16源未变；前后同csc /nologo /optimize+、同源列表/引用。七产品DLL仅复制D7实际实物，原Library Core.Tests未加载。每状态首次重新编译测试DLL，随后两槽逐SHA复制对应测试DLL；driver每槽独立编译。原六首包和author按原相对路径复制。编译与真实加载身份见各manifest/commands/result。
独立闭包编译保留8条原未赋值字段CS0649警告；每槽保留一次UnityEngine可选类型解析提示。所需9 DLL均从本槽实际加载，18次原用例全通过，未增加依赖或替代方法。每次原witness与XML完整内嵌stdout/result；18个玩家ID互异，固定包/定义/新档内容及相同阶段状态前后相等。动态GUID、时间/随机结果及其哈希保留原值。
计时自Python最早入口，完整包含保护、准备、实际编译、进程和最终核验；最终result序列化及工具返回在终点后。WorkItem和NUnit是内嵌边界，不能与进程/墙钟重复相加。

**12边界计数与不重复计时**

[before原trace](fixture-reuse-runs/before-trace01/stdout.txt)与[after原trace](fixture-reuse-runs/after-trace01/stdout.txt)分别1142/1134完整区间，全部12边界覆盖，单线程，零配对错误/未知行；各保留原拒绝路径11条已处理异常通知。残差只扣直接已追踪子区间并集，仍含未追踪工作。

| 边界 | 次数前→后 | 包含子调用秒前→后 | 残差秒前→后 |
| --- | ---: | ---: | ---: |
| PlayerSessionTestData:RealCatalog | 3→3 | 1.510520→0.994340 | 0.022090→0.021980 |
| FirstReleaseContentStorage:Create | 3→1 | 1.488430→0.972360 | 0.122370→0.123470 |
| PlayerRosterTestData:RealRig | 3→3 | 2.043460→1.533670 | 0.184570→0.183630 |
| PublishedContentCatalog:Resolve | 3→1 | 1.366060→0.848890 | 0.166140→0.117120 |
| PublishedContentCompiler:Build | 3→1 | 0.860960→0.579410 | 0.860960→0.579410 |
| PublishedContentCompiler:EvidenceBytes | 3→1 | 0.338960→0.152360 | 0.338960→0.152360 |
| CandidateApplicationRuntime:CheckPlayerRoots | 76→76 | 1.328730→1.396680 | 0.160130→0.154790 |
| PlayerSessionSystem:ResolveRoots | 76→76 | 0.013740→0.013240 | 0.013740→0.013240 |
| CandidateApplicationRuntime:DecodeApplication | 464→464 | 1.021620→1.088230 | 1.021620→1.088230 |
| PlayerSessionSystem:CheckSnapshot | 464→464 | 0.272910→0.297210 | 0.272910→0.297210 |
| CandidateLifecycleApplicationSystem:Submit | 38→38 | 2.317950→2.399510 | 1.085200→1.092790 |
| CandidateApplicationSystem:Restore | 6→6 | 0.162080→0.165930 | 0.046070→0.044650 |

Create由每例[1,1,1]变为[1,0,0]；首cold真实Create为0.978230→0.972360秒。Resolve/Build/Evidence同样从3→1；RealCatalog和RealRig仍[1,1,1]。根检查各[29,18,29]、Decode/Snapshot各[191,82,191]、Submit[14,10,14]、Restore[3,0,3]均原样保留。
两轮根区间并集4.294760/3.868880秒，分别等于所有残差和；用例内未追踪余量0.5612734/0.5418268秒。trace/对应forward比率为0.987872/0.997709，未观察到trace更慢，但不据此声称零开销。
每次无条件new Catalog的源差分保留；after实际返回地址0x10e558ae8、0x10e4c9b80、0x10e6c1a08各异，before亦三异，仅作本进程即时trace佐证，不能作跨运行身份。原始返回文本完整保留。
result.trace.gate_passed只描述修改前“三次Create且≥20%”准入谓词；after因仅一次Create而为false是预期，实际after合同检查全部true。全部原玩家业务保持；本序列after残差较大者为Submit1.092790秒和Decode1.088230秒，尚不能外推全量份额。

**范围与安全交回**

UTC 2026-09-30T05:49:52.387117+00:00最终只读122项复核全部通过；六槽各18项检查全部true。两个首编槽各27叶，其余四槽各25叶；最大10,376,169bytes，所有叶≤32MiB。runner338≤430行，driver85≤200行；未改任何已冻结工具。
D1–D7全部证据（含full01的20叶、报告/期望/准备）、当前工具和此前D8槽均保全。D7实际106 DLL/PDB仍为 `8f5dbbf75473e4abb6849547d3c90fafe274e821a9749c0301dbf70f666343ca`；本包未运行Unity，Library仍是D7编译实物，不冒称已编入新helper。
CONT仍5392 GUID/25226叶，Host仍116/762，全部原字节保持，无新增磁盘测试I/O。14个本轮诊断进程自然退出，ps核无其PID/直接子进程及本工程Unity；git diff --check通过，master/HEAD仍9d416e6c9d3c794be355f903b3636c856783601e。
C交付后停写，由原R实际审后决定下一次完整验证。D9动态准备文件未读写或纳入固定保护；主029仍暂停。本次局部收益不宣称产品冷启动收益或≤600秒全量已达标。
