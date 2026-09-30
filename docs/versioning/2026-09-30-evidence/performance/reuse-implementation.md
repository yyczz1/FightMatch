# D3 实施与独立 Mono 验证交付

D3限定范围实施完成，六槽契约均通过，等待SD00正式收件及原R独立审查。原产品三处字节所有权失败已真实复现并修复；首包实例内复用的CA01诊断信号通过。无过滤全量≤600秒尚未验证，本交付不代表该目标或029 Demo验收完成。

作者：C thread `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / local，turn `01a0f02b-94ca-7313-a45f-d4f86976bc6e`。执行依据：[PLAN的D3签发及设计预审](PLAN.md)。最终只读审计：2026-09-30T03:07:46.706682+00:00。完整结构化身份、差分、断言多重集合、trace推导与复核代码见 [reuse-scope.json](reuse-scope.json)。

**变更与边界**

| 既有文件（仓库相对路径） | 新增/删除行 | 实际变化 |
| --- | ---: | --- |
| Assets/Scripts/FightMatch/Content/FirstReleaseContentStorage.cs | +15 / -2 | 成功Create返回前保存一次完整准入结果；全binding和能力值相同才命中 |
| Assets/Scripts/FightMatch/Content/PublishedContentCatalog.cs | +22 / -10 | 只读入口通过辅助函数复用；保留release读取及原私有完整Resolve |
| Assets/Scripts/FightMatch/Content/PublishedContentModels.cs | +2 / -2 | ReceiptBytes、CanonicalBytes使用内部List副本的只读视图 |
| Assets/Scripts/FightMatch/Content/DemoContentReplay.cs | +1 / -1 | SeedBytes同样隔离；null语义保留 |
| Assets/Tests/EditMode/FightMatch/FirstReleaseContentStorageTests.cs | +117 / -0 | 原34用例保留，新增20个具名实例 |

产品增删合计55≤180；测试新增117≤240且无删除。没有新增Assets文件；公开API、格式、依赖、配置和.meta均未改变。五份原字节保存在 `reuse-sources-before/<原仓库路径>`，准确before/after SHA及独立差分已绑定scope。

复用只属于sealed FirstReleaseContentStorage的单个实例。保存字段只在成功Create公开实例前赋值；后续未命中不替换、不扩充该结果。命中比较ContentBinding全部五字段，以及Capabilities完整序列、MaxSourceBytes、MaxCollectionEntries、MaxStringCodeUnits；相同值的新能力对象可命中。null、改变任一能力/限额或绑定均走原完整路径。两个不同首包实例不串用。

GetCurrentBinding每次仍按原顺序读取/解码release并验证schema、身份、绑定及receipt。普通可变IContentPublicationStorage仍每次完整解析。Publish的两处原私有Resolve调用及调用者budget原样保留；复用没有插入共享私有Resolve。EvaluateAction、ReplayRecorded、恢复Current/RetainedRoots解码与初始化证明代码未改。

Create外层六输入读取和原调用者budget保持；保存结果来自原GetCurrentBinding完整成功路径，其release前置解码及原固定16MiB Resolve预算已实际通过。独立ResolveExact/GetCurrentBinding继续创建原内部固定预算，接口没有可观察的剩余预算或自定义调用者预算；暖命中只复用同一固定输入的已完成工作。Publish任意较小预算仍走完整路径并拒绝。原GetCurrentBinding(null consumer)的NullReferenceException行为与通用路径一致保留，不在本包扩展修复。

**RED / GREEN 与原用例保全**

RED在四个产品文件仍为原字节时执行。54个用例中51通过，恰好 `PublishedBytesRemainUnchangedThroughSyncRoot(0/1/2)` 三条失败；它们分别通过ICollection.SyncRoot污染receipt、profile、seed的当前共享字节，并检查当前及后续查询。失败断言均为“Current publication bytes escaped through SyncRoot”。未出现其它失败或跳过。

修复后，同一54具名实例全部通过，525断言；原34实例的多重集合、方法、参数与断言均保留。新增覆盖：三出口当前/后续不污染、等值新能力对象、重排能力序列、逐项能力/限额/null、五个binding字段、缺level/null binding、两实例不同内容、可变存储暖后缺件/损坏/IOException、已发布内容在调用者零预算下仍拒绝。NUnit真实fixture构建与WorkItem生命周期执行OneTimeSetUp及全部子用例；没有手工跳过fixture生命周期或墙钟断言。

**六条实际命令与计时**

执行cwd为 `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`，严格按下列顺序，每槽只执行一次：

```sh
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_reuse_probe.py --label red01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_reuse_probe.py --label baseline01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_reuse_probe.py --label green01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_reuse_probe.py --label after-plain01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_reuse_probe.py --label after-trace01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_reuse_probe.py --label after-plain02
```

| 原始result | 包装/探针exit | NUnit结果与断言 | WorkItem秒 | NUnit Duration秒 | 进程秒 | 完整墙钟秒 |
| --- | --- | --- | ---: | ---: | ---: | ---: |
| [red01](reuse-runs/red01/result.json) | 3 / 2（预期RED） | 51通过、3失败；513 | 18.620484 | 18.616372 | 18.973047 | 27.075669 |
| [baseline01](reuse-runs/baseline01/result.json) | 3 / 3（性能RED） | CA01通过；148 | 12.084062 | 12.079719 | 12.406835 | 15.388172 |
| [green01](reuse-runs/green01/result.json) | 0 / 0 | 54/54；525 | 10.473931 | 10.470330 | 11.039638 | 19.584932 |
| [after-plain01](reuse-runs/after-plain01/result.json) | 0 / 0 | CA01通过；148 | 2.907199 | 2.901932 | 3.387498 | 6.265865 |
| [after-trace01](reuse-runs/after-trace01/result.json) | 0 / 0 | CA01通过；148 | 2.962666 | 2.957967 | 3.301796 | 6.205608 |
| [after-plain02](reuse-runs/after-plain02/result.json) | 0 / 0 | CA01通过；148 | 2.907329 | 2.901975 | 3.242778 | 6.268526 |

6秒诊断门计量原CA01的WorkItem整体时间，非包装墙钟。四次CA01均为原Mac测试DLL的准确原用例，全部148断言，无失败/跳过。全部编译exit0，无超时。red01预期失败按契约允许进入baseline；原始失败和退出码未覆盖。

独立Content编译固定现有十个cs、相同csc /nologo /optimize+及相同引用，仅四白名单产品源不同；归一化命令逐项相等。baseline复制red01的Content DLL，三个after复制green01的Content DLL，均逐SHA核对。其它DLL、六首包输入、作者源及实际Mono/csc/BCL/NUnit身份相同，真实加载路径已记录。回归槽用准确两份测试源编译同名测试DLL；CA01槽只用原Mac Core.Tests DLL，同进程没有混装。

比较采用本轮相同csc条件的12.0840622秒基线，两个无trace结果为2.9071990/2.9073291秒，分别4.156600×/4.156414×，耗时下降75.941873%/75.940797%。D2B旧Unity编译DLL的绝对时间不作为本轮提速分母。

WorkItem计时含真实NUnit执行；Result.Duration为框架自身区间；进程时间另含启动/加载/JIT与输出。完整墙钟从Python入口至最终检查完成，末尾result JSON序列化和写入在其终点之后。详细分段、命令、PID和UTC起止均保留于各槽commands/result。

**after-trace01 实际计数与时间**

单线程459 ENTER/459 LEAVE，459完整区间；八边界全部出现，配对错误及未知trace行均为0。三条SaveCodecFailure通知属于实际拒绝路径，原文保留，未当作时间事件。时间量化为10微秒。下表残差仅扣除直属已追踪子区间的并集，仍含未追踪工作，不能当作纯方法self time。

| 方法边界 | D2B次数 → 本轮次数 | 包含子调用秒 | 扣已追踪子区间残差秒 |
| --- | ---: | ---: | ---: |
| RealRig | 1 → 1 | 1.611870 | 0.359530 |
| Catalog.Resolve | 34 → 1 | 0.920710 | 0.920710 |
| Submit | 14 → 14 | 1.337080 | 0.745410 |
| Restore | 3 → 3 | 0.088040 | 0.026640 |
| CheckPlayerRoots | 29 → 29 | 0.606660 | 0.077360 |
| ResolveRoots | 29 → 29 | 0.010170 | 0.010170 |
| DecodeApplication | 191 → 191 | 0.457640 | 0.457640 |
| CheckSnapshot | 191 → 191 | 0.117300 | 0.117300 |

唯一计数变化为完整Resolve减少33次，剩余1次在RealRig冷准入。原业务Submit、Restore和逐根解码/快照检查次数全部保留。父子inclusive时间没有相加当总时间。

不重叠归属：RealRig准备1.611870秒（54.4061%）；正文12次Submit共1.014850秒（34.2546%）；显式3次Restore共0.088040秒（2.9716%）；其它/框架及未覆盖间隙0.2479056秒（8.3677%）。前三类区间并集2.714760秒，恰等于全部节点残差之和。

trace对两个plain的开销为1.907905%/1.903345%，未超过10%门；两个plain差异为0.004475%。配对及开销门通过，但一次trace与两个控制不提供统计确定性，也不能外推Unity全量份额。每个独立进程的GUID、时间、NUnit种子及临时MemorySave可自然不同，固定文件身份一致。

**范围、证据与安全收尾**

新增证据仅限四个D3根文件、五个原字节备份及六槽。runner 299≤520行；driver 104≤260行。red/green各29叶，四个CA槽各25叶，均≤36；当前最大槽内叶1,531,904bytes，全部≤32MiB。完整scope绑定各叶SHA、实际加载程序集、工具、源码和原始结果。

原全源指纹：`a651dff1a66efd22423885deb80fc0bdb180c7e7dba860a20ceefd77f62440fe`。
仅新增回归后的red/baseline：`11ef54a3c9a44ed0c221bfecbc48f64be3a8f960da06e35d3567afacdb72d350`。
green及全部after/交付：`cdb2751255ab26a14bdb7d18f322f5342a735b8e1da41ba73e6105570e47ff06`。

最终73项只读检查全部通过：五文件之外源/配置/资源未变；106个原Mac DLL/PDB、四冻结文件、Sept29及D1/D2/D2B完整历史证据、所有已完成D3原始叶与五备份均保持。Git HEAD仍为`9d416e6c9d3c794be355f903b3636c856783601e`，git diff --check通过。ps成功，16个本轮编译/诊断PID均已退出，无本工程Unity占用。只读进程审计遇沙箱ps限制后以明确权限重试成功，没有重跑产品或诊断槽。

本包未执行Unity；Library仍是已接受Mac基线，新增产品验证来自槽内独立csc。当前全量只有既有4106/4106、3267.031977083秒证据；本包新增20用例后预期全量4126实例，尚未执行确认。原4106全量多重集合、Unity编译/针对性比较和无过滤≤600秒必须由后续正式签发验证。SD00据本次收益安排原R审查与下一小包，C在报告交回后安全停写。
