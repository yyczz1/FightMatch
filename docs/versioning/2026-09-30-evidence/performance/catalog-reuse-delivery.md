# D6 单条通用准入复用交付

D6限定实施及六槽验证完成，等待SD00收件与原R实际补丁审查。新fixture在原/新产品均55/55、788断言通过；原MultiPackage各40断言保留。同csc条件基线10.244673秒，修复后两个plain为3.975554/3.765926秒，均通过6秒诊断门。无过滤全量≤600秒尚未验证。

作者C thread `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / local，turn `01a0f07d-01f9-7da2-8cb5-d5ba9075710e`。依据 [PLAN D6正式执行签发](PLAN.md)。正式身份、三文件范围、精确55实例多重集合、原始叶SHA、编译/加载证据、预算与trace复算代码见 [catalog-reuse-scope.json](catalog-reuse-scope.json)。

**改动与可观察边界**

唯一产品 `Assets/Scripts/FightMatch/Content/PublishedContentCatalog.cs` 为+51/-14，合计65≤180行；原字节备份在 `catalog-reuse-sources-before/Assets/Scripts/FightMatch/Content/PublishedContentCatalog.cs`（原SHA `f243fa04a377e78928ceac87862c93e2ba8a9fa8e38e95a50800e4ec2b0fd822`，新SHA `766b198ba1e6547c89a5dcf2155fd2c2c651002303f618972288779122b0a25f`）。
新增 `Assets/Tests/EditMode/FightMatch/PublishedContentCatalogReuseTests.cs` 为288≤450行，55具名实例；仅其新meta为11≤12行，唯一GUID `0587736612b340408891065c7cca5791`。既有测试、meta、其它产品、公开API、序列化、Application/恢复/UI、配置/依赖保持原字节。

单Catalog持有至多一条完整成功ReadAdmission；记录字段只读，构造完成后通过volatile引用替换。Resolve先捕获条目本地引用，比较和返回均使用同一条目；不同包并发替换不交叉返回。没有全局缓存或对外部存储加锁。

仅通用存储只读入口启用复用。FirstRelease继续D3的GetAdmitted，未命中仍原完整Resolve；不向其扩充条目。Publish两处调用均保持原完整Resolve、原调用者budget及锁/顺序，不读写该条目，完成发布也不预热它。

每次按原顺序读index→operation→receipt→payload→source→validation→review。index原null拒绝后、解码前复制；其它记录原Read/null拒绝后、比较/哈希前复制。operation不等仍短路receipt，任一ReadHash失败不读取后续记录。index及四内容数组是本次独占副本，完整字节比较，不以相同hash代替；operation/receipt每次与index完整比较后无需另存副本。

命中须binding全部字段、消费能力完整序列及三个限额、五份原始字节、MaxRecordBytes、MaxIntegerBits精确相等，并以long计算本次剩余步数足以覆盖记录的实际完整后缀成本。MaxPrimitiveSteps总值可不同。未命中/预算不足沿本次已读字节和当前budget继续原后缀，不重启读取、重置或重复扣费；失败不保存条目。已准入返回图沿D3所有权封闭。

公开ResolveExact/GetCurrentBinding的Math预算在内部创建，不暴露剩余计数；命中后没有进一步Math消费。GetCurrentBinding外层release解码/身份/receipt SHA及DefinitionBinding外层level/version仍实际执行。EvaluateAction、ReplayRecorded和全部Current/RetainedRoots解码、初始化证明不受替代。

**55项行为与预算证据**

按合同先在未改产品完成新fixture行为基线；该基线全部通过，性能RED由同编译原业务基线提供，未虚构功能失败。新/旧fixture具名多重集合及源完全相同，均788断言，含真实OneTimeSetUp和全WorkItem生命周期。
回归覆盖七记录各缺件/损坏/IOException、operation不匹配且receipt会抛异常的短路、四ReadHash失败停止后续读取、release/receipt/level/version/binding、能力/三个限额/null/序列变化、MaxRecordBytes及更窄/更宽MaxIntegerBits、借用数组后改review和三份index不能污染已保存结果、两个真实包并发交替、不同Catalog不共享、Publish冷暖budget与不预热、错误不写存储。预算测试只反射调用合同准许的既有私有入口，不改私有状态。

真实冷路径总成本1,697,757步，读取/解码前缀10步，后缀1,697,747步。下表来自原始stdout的 `CATALOG-REUSE-BUDGET`，不是按实现猜数：

| 可用总量相对冷成本 | 预消费 | 原产品只读已用 | 新产品只读已用 | 冷完整路径已用 | 结果 |
| --- | ---: | ---: | ---: | ---: | --- |
| -1 | 0 | 1697756 | 1697756 | 1697756 | 同原拒绝，七记录各读一次 |
| 恰好 / +1 | 0 | 1697757 | 10 | 1697757 | 内容相同，真实命中 |
| -1 | 37 | 1697793 | 1697793 | 1697793 | 同原拒绝，同当前budget |
| 恰好 / +1 | 37 | 1697794 | 47 | 1697794 | 内容相同，真实命中 |

这些案例同时核错误/结果、内容字节、读取顺序与次数，证明预算不足走完整后缀，充足时节省实际步数；未仅以引用相等验证。所有不同整数位/记录限额案例与完整冷路径的结果和实际计步一致。

**实际六命令与同条件计时**

cwd `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`；下列各一次，严格顺序：
```sh
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_catalog_reuse_probe.py --label before-tests01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_catalog_reuse_probe.py --label baseline01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_catalog_reuse_probe.py --label after-tests01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_catalog_reuse_probe.py --label after-plain01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_catalog_reuse_probe.py --label after-trace01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_catalog_reuse_probe.py --label after-plain02
```

| 原始result | 通过/总数；断言 | 包装/探针exit | WorkItem秒 | NUnit秒 | 进程秒 | 完整墙钟秒 |
| --- | --- | --- | ---: | ---: | ---: | ---: |
| [before-tests01](catalog-reuse-runs/before-tests01/result.json) | 55/55；788 | 0/0 | 23.841131 | 23.837632 | 24.296632 | 39.716795 |
| [baseline01](catalog-reuse-runs/baseline01/result.json) | 1/1；40 | 3/3 | 10.244673 | 10.240208 | 10.580287 | 16.615465 |
| [after-tests01](catalog-reuse-runs/after-tests01/result.json) | 55/55；788 | 0/0 | 21.286155 | 21.283538 | 21.742423 | 34.220475 |
| [after-plain01](catalog-reuse-runs/after-plain01/result.json) | 1/1；40 | 0/0 | 3.975553 | 3.969346 | 4.340742 | 9.414495 |
| [after-trace01](catalog-reuse-runs/after-trace01/result.json) | 1/1；40 | 0/0 | 3.814086 | 3.810294 | 4.178759 | 9.598208 |
| [after-plain02](catalog-reuse-runs/after-plain02/result.json) | 1/1；40 | 0/0 | 3.765925 | 3.761828 | 4.125102 | 9.846271 |

各次编译exit0；两fixture限120秒，其余编译/业务限60秒，均自然完成、无超时。baseline的exit3只表示6秒性能RED，正确性通过。Content前后均独立csc固定十源、相同/nologo /optimize+与相同引用，唯Catalog源不同；同状态Content逐SHA由对应fixture槽复制。回归槽仅加载新fixture＋原PublishedContentTestData生成的测试DLL；四业务槽只加载D4实际Mac Core.Tests，未同进程混装。真实加载身份逐项绑定manifest/result。
完整墙钟含准备、编译、执行及最终检查；末尾result序列化和工具返回在终点后。提速以本D6同csc基线计算为2.5769×/2.7204×，耗时减少61.1939%/63.2402%；D5旧Unity编译绝对时间不作为分母。

**trace计数、时间与残差**

[after-trace01原始stdout](catalog-reuse-runs/after-trace01/stdout.txt)：71693bytes。单线程109 ENTER/109 LEAVE，109完整区间，无配对错误、未知行或异常通知，时钟量化10微秒。D5原trace143对亦用同解析器按完整签名/线程深度复核；两轮只作调用数比较。
| 边界 | D5 → D6次数 | D6包含子调用秒 | 扣已追踪子区间残差秒 |
| --- | ---: | ---: | ---: |
| PublishedContentCatalog:Resolve | 21 → 21 | 1.521770 | 0.149920 |
| PublishedContentCompiler:Build | 23 → 6 | 1.696870 | 1.696870 |
| PublishedContentCompiler:EvidenceBytes | 23 → 6 | 0.879830 | 0.879830 |
| CandidateApplicationRuntime:CheckPlayerRoots | 13 → 13 | 0.072740 | 0.006480 |
| PlayerSessionSystem:ResolveRoots | 13 → 13 | 0.015450 | 0.009660 |
| CandidateApplicationRuntime:DecodeApplication | 23 → 23 | 0.046600 | 0.046600 |
| PlayerSessionSystem:CheckSnapshot | 23 → 23 | 0.017100 | 0.017100 |
| CandidateLifecycleApplicationSystem:Submit | 4 → 4 | 0.533190 | 0.468260 |
| CandidateApplicationSystem:Restore | 0 → 0 | 0.000000 | 0.000000 |

Build/Evidence各23→6，均减少17次：各4次位于Resolve内、各2次在外部真实制作中；Resolve仍21次，原读取前缀保持。CheckPlayerRoots/ResolveRoots均13，Decode/CheckSnapshot均23，Submit4，完全保留。Restore本例未调用，不能推断其成本为零。
Resolve总1.521770秒，其中Build0.831280、Evidence0.540570，残差0.149920；嵌套父子时间未重复相加。追踪根区间并集3.274720秒等于全残差之和，用例其余0.5393658秒含未追踪工作/框架/间隙。当前最大追踪成本仍为6次Build共1.696870秒、6次Evidence共0.879830秒；本包不据此擅改冷制作/发布语义。
trace为两个plain的0.959385/1.012788倍，未同时高出10%；plain自身相差5.5664%。一次trace和两个控制不足以提供统计确定性，残差含未追踪子工作，也不能直接外推Unity全量份额。

**范围与安全交回**

最终只读审计UTC 2026-09-30T04:33:44.405356+00:00：80项检查全通过。源码/资产/配置差异仅三白名单；原D4的106 DLL/PDB、全部D1–D5证据（含原失败）、所有已完成本轮叶及原Catalog备份保持。CONT4835目录/22634叶、Host93/612均原字节保持，本包纯Memory未新增I/O。
原D6源指纹 `cdb2751255ab26a14bdb7d18f322f5342a735b8e1da41ba73e6105570e47ff06`；仅新测试/meta后 `f1901991cf8ceffb96134f8cb91fdb8b11c972342dc9aa7e67f5be38a3b4e8a4`；最终固定源 `dfe22e0133e3af2b3eaa2061804cd8cc1fac83f43aff2210d9172cc0d3df984a`。
runner236≤400行、driver104≤180行；fixture槽各23叶、四业务槽各19叶，最大叶9,296,261bytes，全部≤32MiB。ps成功，16个本轮子进程均退出，无本工程Unity占用；HEAD仍 `9d416e6c9d3c794be355f903b3636c856783601e`，git diff --check通过。
本包未执行Unity，Library保留D4实物。新增55实例已完整列入scope，供后续结果产生前独立冻结期望；原4126＋本包55预计4181实例，尚未完成Unity发现/无过滤执行。等待SD00及原R接收后另签下一阶段，主029保持暂停。C交付后停止写入；并行D7准备文件未读写、未运行，不纳入本包固定旧证据集。
