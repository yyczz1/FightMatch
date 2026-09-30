# FightMatch 全量测试五分钟目标／十分钟上限：第二轮

状态：**ACCEPTED_UNDER_600_FIVE_MINUTE_NOT_MET**。2026-09-30用户明确要求继续缩减约50分钟的全量时间，争取五分钟，做不到也须限制在十分钟内。沿用本会话此前已批准的测量后收窄最小修复、既有C实施及串行Unity、既有R独立审查的协调授权；普通只读调查和诊断准备不重复询问。本轮D16实测及D17补充独审已正式接收十分钟上限，准确结论见文末D17正式接收；五分钟未达，旧结果和失败原样保留。

## 用户汇报用的实测阶段对照（十分钟上限已按D17补充接收）

用户所说「约50分钟」对应9月29日54分27秒；「11分钟」对应D9优化后的11分24秒，是不同实施阶段的真实测量。后续D10/D14虽测得低于十分钟，原偏好保全门未通过，其失败不变。最新D16为8分13秒，原夹持条件仍失败；D17对同组恢复作单独只读补充，原R正式ACCEPT，支持接收本机Release这一次实测的十分钟上限，不修改原D16结果。

| 阶段 | 完整分钟 | 全量结果 | 对应变化及验收状态 |
| --- | ---: | --- | --- |
| 本轮起点（9月29日） | 54.45 | 4106/4106 Passed | 原4041＋此前新增65；尚未做本轮准入复用 |
| D7 | 14.14 | 4181/4181 Passed | FirstRelease成功准入复用、每目录保留已验证快照；仍超过十分钟 |
| D9 | 11.39 | 4181/4181 Passed | 复用不可变的首包测试准备数据；玩家/目录/存档实例仍独立；仍超过十分钟 |
| D10 | 6.71 | 4181/4181 Passed | 本次进程采用Release代码优化、保留四定义及原断言；退出偏好检查失败 |
| D14 | 7.74 | 4181/4181 Passed | 关闭托管调试监听没有实测提速；退出偏好检查仍失败 |
| D16＋D17补充（最新接收） | 8.21 | 4181/4181 Passed | 同源码，完整三视图观察；同组恢复经D17独立补充ACCEPT，原D16保护失败保留 |

54.45→11.39约少43.06分钟（79.1%）；54.45→D14的7.74约少46.72分钟（85.8%）；54.45→最近D16的8.21约少46.24分钟（84.9%）。基准没有单独Compile阶段，D7起包含Compile；当前还增加75项必要回归，因此这些百分比是已执行完整命令的墙钟对照，不能描述为完全相同运行协议的逐项因果归因。相同原4106项的case总时长从3231.265933秒降到D14的276.231021秒、最新D16的325.336760秒；全4181项D16合计399.650337秒，单靠缩短收尾尚不足五分钟。

本轮之前已接收的大整数乘法位数边界与序列化反射/字符串处理优化保留，属于54.45分钟基准之前的优化，不能再次计入本轮54.45→8.21的收益。所有原4041保留、零跳过；当前4181=4041＋65＋20＋55。最新时间与验收以本表后续正式收件为准。

## 目标与当前边界

- 本轮对象是**无过滤全量**。完整命令含启动、必要编译及工具检查，目标≤300秒，验收硬上限≤600秒。不得以306项日常子集替代，不因仍有收益而接收超过600秒的全量结果。
- 保留当前全部4106个具名出现及其原断言（含原4041及上轮新增65）；后续必要新增回归另计。不得删用例、降低随机/边界覆盖、绕过校验、换轻量内容、改预算/业务/存档语义或伪造计时。
- 本轮入口完整包装墙钟3267.031977083秒（54.4505分钟）；当时压至600秒需减少约81.635%，压至300秒约90.817%。这是入口目标差距；最新接收实测为492.633793秒，见文末。
- 全量case duration合计3231.265933秒，占完整墙钟98.9052%；141个单例≥10秒。只优化启动/包装器不足以达到目标。剩余内部成本须实测，不把套件聚合时间冒充内部函数分段。
- 当前执行范围以文末最新正式签发包为准；D1/D2诊断、D3已接收实施、D4已接收针对性Unity及D5已交回诊断各保持其历史范围。后续修改必须根据测量补签准确白名单、兼容性门和必要验证。若改变业务/API/序列化/依赖/UI等真实额外范围，先形成具体方案并明确取得所需批准。旧C3/C5的跨调用缓存禁令仅属其局部实施范围，不能自动外推为永久产品规则；保持已批准规格及可观察语义的内部复用方案可由SD00依据既有授权签新小包，无需仅因“缓存”一词追加用户审批。详见本轮R边界澄清收件；本句本身不授权实现。
- 不创建聊天、子代理、分支/worktree；不提交、推送；不操作BlueStacks、真实玩家数据。不得手改Library/Temp/Logs/UserSettings、ProjectSettings、Packages、资源或既有meta；签发包可精确授权Unity自然编译产物处理及新回归测试的对应meta，不构成其它配置/资源许可。

## 角色与上一轮交接

- SD00：本会话`01a0ec0e-6084-7be3-8af3-26aeeada73df`，只负责设计、诊断工具/证据分析、派发和收件，不写产品、不执行Unity、不替代R。
- C：`01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / local，现为空闲。上一轮回合`01a0ecc7-081b-77e2-98cb-3968f5f599fc`正式completed（1790682457），Unity自然结束且安全交付。
- R：`01a0e404-e8ee-7310-8388-9260babd53f1` / local，继续使用原聊天，保留原模型设置。上一轮回合`01a0ecfc-80c6-74a1-99ba-0325d7ad9c20`正式completed（1790682849），按原范围ACCEPT；本轮新硬上限另行验收。
- 主029目标仍暂停。当前uGUI纠正有效；本轮不改UI/场景或UI测试，不借性能任务恢复产品迁移。

## 只读基线与保护身份

上一轮目录为`TestArtifacts/FMTestPerformance/2026-09-29/`，其中所有运行槽、脚本、报告及PLAN本轮保留，不覆盖。

- `unity-runs/full01/tests.xml`：3050600bytes，SHA`d2f875296d23bab876f37ce27b3cd133dc5b5136803735f93686168bce324b27`，4106/4106，XML3245.9436806秒。
- `unity-runs/full01/result.json`：5800bytes，SHA`4677fbae606014a19575b5cc57a9ede2324da1dcc4a7021959adb68a5f9bf0d7`，正确性true，包装器exit3，完整墙钟3267.031977083秒。
- `unity-runs/daily01/compiled.json`：28106bytes，SHA`982177f1fb7126c33501b0bd64391f4cb9ba71bd6586b765f3b9e69bfb154ef1`。其中绑定真实Mac DLL/PDB；本轮必须fresh比较实际Library文件身份，只读，不更新它们。
- `unity-runs/full01/after.json`及上轮完整源指纹`a651dff1a66efd22423885deb80fc0bdb180c7e7dba860a20ceefd77f62440fe`是Assets/Packages/ProjectSettings基线。任何变化如实列明，不能悄悄替换基线。
- `Assets/Scripts/FightMatch/Core/ExactMathBudget.cs`：5922bytes/SHA`083e90d55d664768fe549d211a31815548c31da60347b6c86d03144dc883279f`。
- `Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs`：18590bytes/SHA`fa3110b8de21b050aa2c8727b4b91754ba1b548c90f286f60c652b8b3a6c49e0`。
- `Assets/Scripts/FightMatch/Content/PublishedContentCodec.cs`：19824bytes/SHA`9a2ee42c7a76156aaf66740b7af4e3976342fa6cc408655fb68522bfe53f27a8`。
- `Assets/Tests/EditMode/FightMatch/PublishedContentCompilerTests.cs`：22026bytes/SHA`224f3ed66dc4a087c4e0906c21a7cf34b6ffd8ecdc61192d9f96123c04a18b75`。
- Git HEAD此前为`9d416e6c9d3c794be355f903b3636c856783601e`；保留全部既有029 WIP和用户权限文件、主线/美术任务改动，不以git reset清理。

## D1：当前Mac基线的紧凑重现与调用图

### 准确写白名单

以下均仅位于新目录`TestArtifacts/FMTestPerformance/2026-09-30/`，原C独占诊断文件编辑：

1. 新`run_probe.py`（≤280行）：从上轮runner复用必要准备，移除不再需要的旧变体选择；固定下面两个label和实际Mac DLL来源。记录最早Python入口到全部检查结束的总墙钟与分段。
2. 新`content_probe.cs`（以原实物385行、28857bytes、SHA`98fd6f64390b06fa494bb6e9dbf9fc036dbf3a4f67cdae35f5faafba152ff3c1`复制为起点，诊断改动增删≤120行）：保留真实入口、原正确性检查和原0.250秒历史信号，新增本轮诊断信号及完整准入链计时；不复制/重写产品算法，不换假数据。原“374行”是基准文字错误，2026-09-30经R指出、SD00按实物复核后更正，不改变范围。
3. 新`diagnosis.md`（≤160行）和`diagnosis-scope.json`：准确命令、C回合、保护身份、真实结果、最小化、候选及下一测量方案。
4. 只创建`probe-runs/baseline01/`和`baseline02/`，CreateNew，不覆盖；每槽精确20叶：`FightMatch.Core.dll`、`FightMatch.Content.dll`、`FightMatch.Platform.dll`、`FlowPuzzle.Core.dll`、`FlowPuzzle.Validation.dll`；`inputs/first-release.{fmsource.json,fmpackage.bytes,fmvalidation.bytes,fmreview.json,fmpublish.json,fmrelease.json}`；`content_probe.cs`、`content_probe.exe`、`manifest.json`、`compile.stdout.txt`、`compile.stderr.txt`、`commands.json`、`stdout.txt`、`stderr.txt`、`result.json`。

SD00仅编辑本PLAN；没有其它新增文件许可。各JSON/文本证据≤32MiB，不复制全工程，不读取或写入真实玩家存档。

### 输入、执行及复核

- 固定DLL只读来自本工程当前`Library/ScriptAssemblies/`上述五个纯程序集，执行前逐项匹配已接受Mac compiled.json；不是旧Android或独立重编DLL。记录它们及现有Mono/csc/BCL身份；不安装工具或重编产品。
- 六个首包输入只读来自上轮`probe-runs/diag02/inputs/`，先用原manifest逐SHA核对；复制到新槽。原C#探针只读复制自上轮`content_probe.cs`，其实际身份写新manifest。
- C执行前fresh核当前源码/配置/资源和4冻结文件、上轮4106 XML、全部旧输入及DLL；进程盘点必须成功，工程若有Unity占用即停止，不把ps失败当空进程，不强杀。必要沙箱提升按已授权作用域处理。
- 用已安装Unity随附Mono和现有csc，沿用已验证编译参数；单次诊断编译/独立探针上限60秒，超时保留124和实际stdout/stderr，仅终止该独立探针子进程，不碰Unity或其它进程。
- 准确接口为`python3 TestArtifacts/FMTestPerformance/2026-09-30/run_probe.py --label baseline01 --mode minimal --closeout-confirmed`，第二次仅label改为baseline02。每次模式均驱动真实六文件→FirstReleaseContentStorage.Create→PublishedContentCatalog.GetCurrentBinding→ResolveExact，固定同一实际首包与能力/预算。
- 各阶段单独计时，并记录从Create开始至首次Resolve完成的完整准入链时间（读文件与正确性检查边界说明清楚）。原0.250秒微探针门保持独立字段，不覆盖历史RED。
- 新一轮诊断信号固定为Resolve≤0.040秒且完整准入链≤0.150秒；正确性失败exit2，正确但新信号未达exit3，否则exit0。这只是对当前剩余公共路径的敏感信号，**不能用它推断或替代全量≤600秒**。最终完整业务操作及Unity全量仍须实际验证；不能因微探针GREEN提前宣称目标完成。
- 完整保留原首包准入、binding、receipt及level核对；记录每次实际产品方法执行和结果，不跳过重放/证据/规范化来获得绿色。证明两轮输入和源/DLL身份一致且结束后保护文件未变。

### 返回与后续停止门

1. 先得到并展示当前秒级红色命令及至少两次数据，再进入候选归因。上轮已最小化的一次真实Resolve作为起点；若仍可进一步最小化，单变量提出下一准确探针，不擅加运行槽。
2. 基于实测和已读源码给3～5条有可证伪预测的候选，区分：单次底层成本、一次业务操作中的重复完整准入、测试准备与实际操作的贡献、其它业务/序列化成本。不把磁盘、Rosetta或测试框架名称直接当根因。
3. 只读检查`FirstReleaseContentStorage.cs`、`PublishedContentCatalog.cs`、`PublishedContentCompiler.cs`、`PlayerSessionSystem.cs`和代表性`PlayerRosterSessionTests.CA01`及其RealRig帮助方法，给准确调用图与下一最小测量范围。静态调用次数与运行计数必须分开。
4. 当前代码可读到Create内自做一次GetCurrentBinding，而外部GetCurrentBinding和ResolveExact又各自执行Resolve；这只是源码事实，**是否主导完整业务慢用例尚待测量**。不得据此先实现缓存、取消任一校验或调整接口。
5. 回报能否仅靠当前低层路径达到5.45倍以上提速，明确哪些是测量、哪些仍未知；提出准确D2测量入口、临时插桩方式和文件列表。需要Unity代表性用例或诊断副本编译时先交SD00补签；本D1不启动Unity。
6. D1正式交回后停止写入，SD00据数据推进下一小包。本轮不能用收益但仍超过600秒的结果宣布整项完成；若现有边界阻碍目标，必须提出具体可审查选择与代价。

## 最终验证约束（尚未授权执行）

先以真实代表性慢业务操作形成秒级回路和前后同条件对照；只有修复及回归完整、源码固定且有数据支持时，签必要Unity编译、针对性验证，再一次无过滤全量。最终4106原实例多重集合完整保留并全Passed，新增另计，源码/DLL/旧I/O保全和原始失败全保留；准确绑定编译/启动/执行/工具检查，≤300秒为目标达成、300～600秒为用户接受的上限范围，>600秒不可接收为本轮完成。原R须独立审实际补丁及新证据。本阶段不预授任何额外Unity槽或产品文件修改。

## D1边界预审收件

R回合`01a0eff2-9ce8-7352-96d5-927729530d89`已于1790732338正式completed，final message `msg_0d29bf8d726d8dc0016abc681c2e5487d0860f3d591de64874`。未发现阻断D1诊断的实质漏洞；指出并已复核上述旧探针实物行数。此次仅诊断合同预审，不是产品最终verdict，也未预准缓存、移除校验或API变化。后续必须保留通用存储读取新鲜度及失败顺序、调用者独立预算和可变对象所有权、完整重放/证据准入、Current及全部RetainedRoots的解码和初始化证明。三次准入只是静态调用事实；D2须实测完整业务操作内部计数和分段后才能归因。

## D2测量准备草案（尚未授权执行）

SD00已按原始输出、槽内及加载文件身份复核D1两次结果：Resolve为0.2518219/0.2573724秒，完整准入链为1.5521563/1.6410997秒；全部保全和正确性检查通过，均保留exit3。两槽各20叶，产品及测试未改。D1仍须C正式completed及报告收件，以下不构成提前增开槽许可。

- 优先直接调用现有Mac测试DLL内的`FightMatch.Core.Tests.PlayerRosterSessionTests.CA01_RealApprovedWarriorMovesThroughAllSlotsAndEmptyWithoutReplacingItsOriginalIdentity`，保留原方法全部业务步骤和断言；用真实六首包输入及MemorySave，不重写该业务用例。
- SD00只读执行现有Unity随附`mono --help-trace`，确认支持`--trace=M:Type:Method`及逗号分隔方法名单。优先使用此原生功能，避免产品或测试插桩、程序集改写和重新编译产品；是否可在独立Mono完成原CA01须实际验证，不能由纯程序集标记推定。
- 拟按无trace→少量边界trace→无trace三个独立进程执行，记录实际方法总时间、进程及完整命令时间。固定同DLL、运行参数及实际首包，比较追踪开销；若首槽断言/依赖失败，保留原始失败并停止后续依赖槽，不伪造Unity入口或删步骤。
- 关键边界拟覆盖RealRig准备、FirstRelease.Create、Catalog.GetCurrentBinding/Resolve、Compiler.Build/EvidenceBytes、Lifecycle.Submit、Runtime.Submit/Restore/CheckPlayerRoots、PlayerSession.ResolveRoots及ApplicationSaveCodec.DecodePublished/EncodePublished。禁止高频数学逐操作trace。最终准确方法、依赖DLL及文件白名单待C的最小提案签发。
- 分析须区分包含子调用的总时间与扣除已追踪子调用后的自身时间，防止重复相加；报告实际次数和残差，不以静态次数替代测量。若有异常退出/解析不完整/时间基准不明/显著追踪扰动，降低结论强度并保留原始输出。
- 不将一个用例、独立Mono计时或微探针GREEN替代无过滤全量≤600秒；用例用于确定下一最小修复及其收益上限。

## D2准确执行包（DIAGNOSIS_AUTHORIZED，D1已正式completed）

采用C的D1最小提案：仅使用原测试DLL、首包副本及Mono八个低频边界trace。D1返回的静态事实足以开始实际计时，不再以穷举调用图延迟测量。C回合`01a0eff2-11d9-7042-8566-3774a5eba6c6`已于1790733467正式completed，final message `msg_0d179092dafe07fa016abc6c93d03087d0b574a9801c2ebe4f`，声明安全停写。SD00已读实际runner、探针差分、报告/范围、原始输出及两槽文件身份并收件；本节随显式派发执行，不预授产品修改或Unity。

### C准确新增文件

均在本`2026-09-30/`目录下，不改D1四文件及两槽：

1. `run_business_probe.py`（≤320行）：可只读复用D1的保全辅助函数；禁写pycache，固定三个label与顺序，不接受任意程序集/方法/trace参数。编译和运行仍分别≤60秒。
2. `business_probe.cs`（≤180行）：只通过确切公开类型/方法直接或反射调用原CA01一次；不可枚举或执行整个测试程序集、改私有状态、替换断言、伪造Unity/NUnit上下文或复制业务实现。正常返回才记原断言全部走完；展开并保留原异常。
3. `business-diagnosis.md`（≤160行）和`business-scope.json`：准确调用、工具及输入身份、原始结果索引、计数/时间推导、开销与局限、下一最小范围。结果及报告不能声称全量≤600已验证。
4. 仅`business-runs/plain01/`→`business-runs/trace01/`→`business-runs/plain02/`三个CreateNew槽。正常完成各精确24叶：九DLL（下列名单）、六个`Assets/StreamingAssets/FightMatch/first-release.{fmsource.json,fmpackage.bytes,fmvalidation.bytes,fmreview.json,fmpublish.json,fmrelease.json}`、`business_probe.cs`、`business_probe.exe`、`manifest.json`、`commands.json`、`compile.stdout.txt`、`compile.stderr.txt`、`stdout.txt`、`stderr.txt`、`result.json`。失败槽允许相应未生成叶缺失，但必须保留原始stdout/stderr/退出及结果，不覆盖重试。

九DLL为`FightMatch.Core.dll`、`FightMatch.Content.dll`、`FightMatch.Platform.dll`、`FlowPuzzle.Core.dll`、`FlowPuzzle.Validation.dll`、`FightMatch.Application.dll`、`QFramework.dll`、`FightMatch.Core.Tests.dll`、`nunit.framework.dll`。前八来自当前实际`Library/ScriptAssemblies/`，逐项匹配已接收`daily01/compiled.json`；NUnit只来自`Library/PackageCache/com.unity.ext.nunit@1.0.6/net35/unity-custom/nunit.framework.dll`，329728bytes、SHA`e388e140954de12970384331d1730e95b4613ef53c3326b6f8fa0fffdbc9314f`。不复制额外Unity/Input/Presentation程序集；如实际需要，首槽记录缺失后交SD00据事实收窄补签，不能stub或盲目复制整目录。

### 执行与测量

- 入口固定为`FightMatch.Core.Tests.PlayerRosterSessionTests.CA01_RealApprovedWarriorMovesThroughAllSlotsAndEmptyWithoutReplacingItsOriginalIdentity`，原源码位于`Assets/Tests/EditMode/FightMatch/PlayerRosterSessionTests.cs:18`，没有本类SetUp/TearDown。实际可行性由首槽确认；保持NUnit所有原断言和完整RealRig/MemorySave流程，不声称这是Unity Test Runner验证。
- 六输入从D1 `baseline01/inputs/`逐SHA复制，仍匹配原diag02；以每槽为cwd，使原方法按原相对路径读取同一六首包副本。实际源码和原输入均保持只读，不读取真实玩家存档。
- 准确命令是`python3 TestArtifacts/FMTestPerformance/2026-09-30/run_business_probe.py --label plain01`，继而仅label改为`trace01`、`plain02`。每个独立进程只调用原CA01一次；每槽独立编译小驱动，不重编产品或测试。前槽正确性/依赖失败或超时，保留证据并停止后续依赖槽；单纯性能RED不阻断另外两槽。
- `trace01`仅允许以下八方法的`--trace=M:Type:Method`逗号组合；另两槽不传trace。`FightMatch.Core.Tests.PlayerRosterTestData:RealRig`、`FightMatch.Application.CandidateLifecycleApplicationSystem:Submit`、`FightMatch.Application.CandidateApplicationSystem:Restore`、`FightMatch.Application.CandidateApplicationRuntime:CheckPlayerRoots`、`FightMatch.Application.PlayerSessionSystem:ResolveRoots`、`FightMatch.Application.CandidateApplicationRuntime:DecodeApplication`、`FightMatch.Application.PlayerSessionSystem:CheckSnapshot`、`FightMatch.Content.PublishedContentCatalog:Resolve`。
- 记录原方法整体时间、进程时间及从最早Python入口到最终检查的完整墙钟。完整原方法≤6.0秒作为本轮代表性诊断信号，超过记RED/exit3，原方法失败或保全不通过exit2、超时124；此门约对应原Unity CA01的33.849215秒所需提速量级，但运行环境不同，绝不据此验收全量。
- trace原始输出保留，逐线程按完整方法签名配对进入/退出，记录实际调用数、包含子调用时间、扣除已追踪子区间后的残差，按RealRig/正文Submit/显式Restore归属。嵌套父子区间不得相加冒充总时间；未知或未匹配输出不能当作零调用。必要分析代码放获准runner或只读内联命令，结构化汇总写各result/business-scope，不额外落文件。
- 用前后两个无trace控制估计trace扰动，报告输入相同仍存在的启动/JIT/时钟/I/O差异；如果trace未覆盖任一实际边界、存在未配对区间，或耗时相比两个plain均增加>10%，标明精确份额不可靠，不直接用被扰动绝对时间预报Unity全量。保留有效计数和可说明的局限，再提出最小替代测量。

### 保全与交付

每次执行前后沿用D1保护：源/资源/配置完整指纹、106个Mac DLL/PDB、四冻结产品/测试、Sept29完整证据、D1四文件和两槽、已完成D2槽、实际Mono/csc/BCL/NUnit身份均须保持。guard的ps必须成功且无本工程Unity占用；仅独立诊断子进程组可按60秒上限终止，不操作Unity或用户其他程序。manifest绑定实际加载路径、源输入及工具身份。所有证据文件≤32MiB，不新建上述范围外文件。

三槽完成或遇到真实依赖失败后，C正式交回，SD00结合实际非重叠份额决定下一包。若单条路径的份额不足以支撑总时长减少81.635%，不得仅凭该路径局部收益启动耗时全量。没有产品修改时无需重复Unity；后续必要实现与全量验证仍逐小包签准确白名单，由原R独立审最终实际补丁。

## R边界澄清收件：局部禁令与产品契约

R回合`01a0f00a-6688-7d00-94d0-93324aa5eeb8`已于1790733982正式completed，final message `msg_0d29bf8d726d8dc0016abc6e84fc9487d0bbca0eed3128f274`。SD00已独立读取所引原文：

- Sept29 PLAN:107及157的“不加跨调用结果缓存”分别限定C3算术预检及C5编码器包，未发现用户永久禁止内部缓存的指令。
- `docs/system-design/2026-09-16/details/configuration.md:123`允许一次装载核验后查询只读视图、按指纹隔离缓存。`docs/system-design/2026-09-17/system-task-packets.md:5889`的正式025-P2读取契约要求准确身份、能力及内容/验证/review/收据核验，没有每次Resolve重做数学证明、整关回放和证据编码的文字。当前每次Build只是现有实现事实。
- `details/battle-history.md:185`和189对EvaluateAction／ReplayRecorded另有实际求值/重演比较要求；`system-task-packets.md:1987`要求在调用方Math下重验保留数值。不得把这些入口变成返回缓存的空操作。
- `demo-product-continuity-plan.md:61`要求Current及所有保留根涉及的准确包和完整恢复图核验；不得借内容复用删减存档根的物理读取、解码或初始化证明。

因此，若测量支持内容复用，下一技术包须明确复用对象、完整身份、生命周期、存储变化/缺件/损坏/I/O失败和能力拒绝的检测；证明冷暖路径预算可观察行为、失败顺序和对象所有权。保持这些语义属于本专项已授普通内部优化，由SD00签准确范围；真正改变产品契约才形成具体额外授权问题。此收件不是预准方案，也不是产品最终verdict，当前仍仅执行D2。

## D2B补充：真实NUnit执行上下文（DIAGNOSIS_AUTHORIZED）

SD00已实读D2 `business-runs/plain01/result.json`及原stderr：九指定DLL均加载，原方法在RealRig的`NUnit.Framework.TestContext.WriteLine`抛NullReferenceException，未完成CA01，包装exit2；另记录UnityEngine unresolved提示。保留此失败，不把部分执行时间当作完整业务成绩，D2原trace01/plain02不执行。C已报告停下依赖槽；本补充允许同一C继续完成D2简短失败报告及scope、冻结其四文件和失败槽，然后直接推进本D2B，无需为了普通测试工具适配重复用户批准或等待一次额外聊天轮转。最终仍须正式completed收件。

### 准确写范围与入口修正

- 仅新增本目录`run_nunit_business_probe.py`（≤360行）、`nunit_business_probe.cs`（≤200行）、`nunit-business-diagnosis.md`（≤160行）、`nunit-business-scope.json`及`nunit-business-runs/plain01`→`trace01`→`plain02`。正常每槽仍精确24叶：沿D2相同九DLL、六个槽内Assets首包输入；将驱动cs/exe文件名换成`nunit_business_probe.cs`/`.exe`；其余manifest/commands/compile.stdout/compile.stderr/stdout/stderr/result七文件同D2。不改D1/D2已执行源及槽，不覆盖失败、不新增其它程序集。
- 唯一诊断变量是由同一精确NUnit DLL的真实单测试构建和执行机制运行原CA01，而非直接MethodInfo.Invoke。SD00已只读核实该DLL存在`MethodWrapper(Type, MethodInfo)`、`NUnitTestCaseBuilder.BuildTestMethod`、`TestMethod`、`SimpleWorkItem`、`WorkItem.CreateWorkItem/InitializeContext/Execute`等方法。C用实际public API编译确认；不反射篡改私有字段，不伪造程序集/Unity类型，不替换日志、断言或原测试方法。
- 允许为真实WorkItem构造`TestExecutionContext`并提供实际原fixture实例、WorkDirectory以及所需现有框架dispatcher；CurrentTest、CurrentResult、完成状态和Passed结果必须由框架实际执行创建。可构造仅含确切CA01的真实fixture/work item，不枚举或启动整个测试程序集。若需检查本原类的框架属性/SetUp/TearDown，限该确切类型；现有源码无本类SetUp/TearDown，不删掉框架应有生命周期。
- 成功须框架实际只执行原CA01一项且正常完成/Passed、无失败/跳过/忽略，记录完整名称、状态、实际断言计数及原测试输出；断言计数只记录，不捏造阈值（原Unity XML此例asserts=0）。框架结果原文或XML可写入既定stdout/result，不增叶。不手工将失败结果改为Passed。
- 命令固定为`python3 TestArtifacts/FMTestPerformance/2026-09-30/run_nunit_business_probe.py --label plain01`，随后仅label改`trace01`、`plain02`。同一固定八边界trace、真实输入/源码/DLL、6秒诊断门、60秒独立子进程上限、性能RED继续及真实正确性/依赖失败停止均沿D2。首槽若仍发生真实原生/依赖问题，原样交回最小事实，不自动加载Unity或复制额外DLL。

### 保全和测量解释

沿D2全部保护，再冻结D2四文件与plain01完整失败槽的大小/SHA；D2B各后继槽保全已完成槽。尽量复用只读保全函数，不增加与目标无关的检查或文档。记录框架执行总时间、实际测试Result.Duration、驱动/编译/完整命令分段；三次均用完全相同真实执行入口，清楚说明与原MethodInfo计时边界不同。解析原trace配对与非重叠时间、正文/准备归属及控制开销；不能因程序exit0、日志出现或局部用例成功宣称全量≤600秒。C完成三槽及紧凑归因报告后正式返回，SD00据实际份额签最小产品修复或必要细分；仍不预授产品/测试修改或Unity。

## R返回图预审收件（未预准复用方案）

R回合`01a0f015-ff26-7c82-afa5-59cb78dadea2`已于1790734785正式completed，final message `msg_0d29bf8d726d8dc0016abc719e074887d09f8ae02d86312489`。仅只读源码/本机Mono IL，未运行复现或改文件；SD00已读取所指三个构造位置。

三条公开可达路径使用`Array.AsReadOnly`：`ResolvedPublication.ReceiptBytes`（PublishedContentModels.cs:226）、`NewProfile.CanonicalBytes`（同文件:68）、`DemoContentReplayResult.SeedBytes`（DemoContentReplay.cs:27）。非泛型`ICollection.SyncRoot`可转交底层数组，构造前clone不能阻止返回后的这个出口；直接共享该结果会有跨调用污染风险。后续若复用，必须先以实际运行时的负例复现并封闭/隔离这些出口，不能仅靠IList写入抛异常认定深只读。

R沿其余指定公开图检查Definitions/Parameters、Replay/Candidate/Run的定义、路线、条件、记录、快照、报告和随机状态，未发现其它公开写入口；FlowPos按值返回。Catalog内部新建的DemoContentDraft owner不外泄，默认取消令牌也没有外部取消源，因而该Job与一般外部持有draft/取消源的制作候选不同。FirstReleaseContentStorage的sealed/私有构造/输入clone/Read clone/拒写使成功后的存储在公开API下固定；通用IContentPublicationStorage不保证此性质。上述为范围明确的预审事实，不能替代未来补丁及真实回归的最终独立审查。

## D2B正式收件

C回合`01a0f009-f52e-7780-9c6e-4c7a493e2d9f`已于1790735575正式completed，final `msg_0d179092dafe07fa016abc74d01b7087d0ad76f0922bde75c8`，安全停写。SD00已读驱动、runner、报告及原trace，独立按完整签名/线程栈复算492对区间：Resolve34次／9.333120秒，占完整case11.3584959秒的82.1686%；两plain为11.3991281／11.3316140秒，均148断言Passed。Decode191次0.445840秒、CheckSnapshot191次0.113430秒。每槽正确性和16项保全均通过，未改产品或运行Unity。该比例仅属于此Mono用例，不能外推全量。保留首个Resolve时理想用例下界约2.918秒，故下包即使成功，也不预宣称全量十分钟已达。

## D3：不可变首包实例内复用与返回图所有权（IMPLEMENTATION_AUTHORIZED）

这是已批准性能计划下依据D2B实测收敛的小包；C实施，原R独立审。目的：同一已成功创建的不可变FirstReleaseContentStorage，在准确相同绑定和消费能力下重复只读查询时，复用其完整准入结果。冷加载继续执行全部验证；不改通用存储、业务、保存或公开协议。以下为本包准确许可，其它历史段落的“当前仅诊断”不限制本包。

### 产品与测试白名单

仅允许修改以下五个既有文件；不新增Assets文件或meta，产品增删合计约≤180行、测试新增≤240行（超出先交具体必要差分，不能顺带清理）：

- `Assets/Scripts/FightMatch/Content/FirstReleaseContentStorage.cs`
- `Assets/Scripts/FightMatch/Content/PublishedContentCatalog.cs`
- `Assets/Scripts/FightMatch/Content/PublishedContentModels.cs`
- `Assets/Scripts/FightMatch/Content/DemoContentReplay.cs`
- `Assets/Tests/EditMode/FightMatch/FirstReleaseContentStorageTests.cs`

保持该测试原全部方法、断言及参数；可新增直接相关回归。四个Sept29冻结文件（含Codec）仍禁止修改，Application/PlayerSession/恢复/战斗算法、其它测试、资产、配置、依赖、asmdef、公开API、序列化及Git写操作均不在范围。允许只读上述文件依赖与已有PublishedContentTestData.cs，不扩大产品重构。

### 实现边界与可观察标准

1. 先在当前实际Mono执行三条`ICollection.SyncRoot`数组逃逸负例，再修改。仅封闭ReceiptBytes、NewProfile.CanonicalBytes、默认引用SeedBytes三个可达出口；允许用内部拥有的List副本及AsReadOnly保存同样字节和IReadOnlyList接口，不广泛替换其它包装。公开返回图必须没有可修改共享状态，构造输入/Read副本隔离保持。
2. 复用仅属于sealed FirstReleaseContentStorage的**单个实例、一个已完整准入结果**，不使用静态、全局、跨实例或通用IContentPublicationStorage缓存。成功Create返回前应已建立完整准入和只读所有权；内部初始化不得在对象公开后竞争写入。失败Create不外泄半成品。
3. 命中须准确比较ContentBinding全部字段及消费能力全部值：Capabilities的完整序列内容、MaxSourceBytes、MaxCollectionEntries、MaxStringCodeUnits。`Current`每次创建新对象，不能只比较引用。不相同或null不得借旧成功跳过拒绝；按原完整路径和原先后顺序处理。不同binding、level/version、scope/release不回落当前或最新内容。
4. 独立只读入口可加私有/内部辅助；**Publish继续调用原完整Resolve并消耗原调用者budget**，不能把复用插入共享私有Resolve而影响发布、幂等发布的预算/错误路径。ResolveExact及GetCurrentBinding使用原内部固定预算，说明冷/暖计费不可观察性和Create的调用者budget不变；不能把固定预算成功套到任意更小/自定义预算。
5. GetCurrentBinding仍读取/解码原release记录、核全部身份和receipt；通用可变存储每次走原全部读取/哈希/规范化/数学/回放/证据准入，缺件、损坏、I/O错误不得被旧成功掩盖。EvaluateAction、ReplayRecorded、每次恢复全部Current/RetainedRoots读取解码和初始化证明保持实际执行。
6. 新回归须覆盖三种字节视图无法污染当前及后续查询、相同能力不同对象、逐项能力/限额改变时与通用完整路径相同拒绝、错误绑定/level/release不回落、不同存储实例内容不串用，以及可变MemoryStorage暖后缺件/篡改/IOException仍拒绝。保留原Create输入/Read复制、坏文件/来源/receipt与调用者预算测试，不仅检查引用相等。不要写易波动的NUnit墙钟断言；性能由真实CA01诊断验证。

### 独立Mono紧凑验证与证据白名单

不启动Unity。仅C可新增本目录`run_reuse_probe.py`（≤520行）、`reuse_probe.cs`（≤260行）、`reuse-implementation.md`（≤180行）、`reuse-scope.json`。允许在`reuse-sources-before/`按原相对路径保存上述五个修改文件的原字节，CreateNew、逐SHA绑定，用于独立差分；不复制全工程。只读复用D1/D2B辅助，不修改它们，不产生pycache。

固定六槽和顺序：`reuse-runs/red01/`（新回归对未改产品）、`baseline01/`（同编译条件原CA01）、`green01/`（修复后回归）、`after-plain01/`、`after-trace01/`、`after-plain02/`（同原CA01）。命令统一`python3 TestArtifacts/FMTestPerformance/2026-09-30/run_reuse_probe.py --label <上述固定值>`，每槽CreateNew不覆盖失败。先写回归并记录旧产品的预期所有权失败，然后才实施；red01的预期失败不阻断baseline01，其它非预期正确性/依赖失败须保留并停下依赖步骤。若需修复runner编译错误，只修获准脚本并交SD00签新槽，不能覆盖原失败。

每槽只允许下列叶名（未使用可省略，单槽≤36叶，各证据≤32MiB）：沿D2B九DLL与六输入路径、`Assets/FightMatchContent/demo-r1.source.json`、`reuse_probe.cs`、`reuse_probe.exe`、`manifest.json`、`commands.json`、`content.compile.stdout.txt`、`content.compile.stderr.txt`、`tests.compile.stdout.txt`、`tests.compile.stderr.txt`、`driver.compile.stdout.txt`、`driver.compile.stderr.txt`、`stdout.txt`、`stderr.txt`、`result.json`。额外作者源只从本工程该精确只读文件复制并绑定身份，不以修改后的伪首包替换真实六输入。

允许用现有csc在槽内独立编译Content程序集，源为当前Content目录确切十个既有cs（DemoContentCompiler、DemoContentModels、DemoContentParameters、DemoContentReplay、FirstReleaseContentStorage、PublishedContentAuthoring、PublishedContentCatalog、PublishedContentCodec、PublishedContentCompiler、PublishedContentModels），其它所需五/八DLL仍来自已验证Mac基线。baseline与after使用完全相同编译参数／引用，记录实际source清单SHA和加载DLL；不将旧Unity编译与新csc差异混成优化收益。只有四白名单产品源可不同；同状态已编DLL可从此前对应槽逐SHA复制，避免重复编译。

red01/green01允许只用上述既有FirstReleaseContentStorageTests.cs和只读PublishedContentTestData.cs编译槽内测试DLL；经原NUnit真实fixture构建/WorkItem生命周期执行该精确fixture（含OneTimeSetUp及全部原/新增测试）。如需相同程序集名，回归槽只加载此测试DLL，CA01槽只加载原Mac Core.Tests.dll；不能同进程混淆。不得手动略过fixture生命周期、重写业务、fake断言或枚举运行其它测试。原CA01仍由D2B已验证真实NUnit入口执行，全部148原断言须保持。

每次编译／独立诊断子进程上限60秒，若整个精确fixture超过60秒保留超时事实并交最小拆分方案，不擅扩时。复用D2B固定八边界trace，仅after-trace01开启；baseline01与两个after-plain不trace。保留原6秒性能门并报告实际秒数/计数；预期原私有完整Resolve由34降至1，同时RealRig1、Submit14、Restore3、ResolveRoots29、Decode191、CheckSnapshot191均保留，差异必须解释，不能悄然漏业务。

每槽前后保全所有非五白名单源码/资产/配置、全部106原Mac DLL/PDB、全部Sept29证据及D1/D2/D2B四文件/已完成槽，记录五文件准确差分与最终完整源码指纹。ps成功且无Unity占用；只允许终止本包独立诊断子进程组，无权终止Unity/用户其它程序。保留全部原始失败。报告真实RED/GREEN、正确性、计时对比、加载身份、调用计数、原断言/测试多重集合，以及剩余全量目标差距。

六槽及安全收尾完成后C正式completed交回。SD00核真实结果，R独立审实际补丁与回归；本包不预授Unity、不把CA01通过等同全量≤600秒。下一步按收益和剩余分布签必要Unity针对性及一次全量，超过十分钟仍不可接收整项完成。

### D3设计预审收件

R回合`01a0f02b-aa3b-7b42-b8da-41078e447e74`于1790735881正式completed，final `msg_0d29bf8d726d8dc0016abc75fca09c87d0b725a705c55b3e96`：未发现设计合同实质阻断，无需补签设计修正。明确成功Create后未命中不能替换或扩充保存结果；首次GetCurrentBinding成功已包含release前置预算，后续同输入内部固定预算复用可成立。仍须核实际三出口RED/GREEN、公开图所有权、Publish及通用存储完整路径和恢复计数。此次不是产品最终verdict。

## D4准备草案：真实Unity代表性验证（尚未派发，不得执行）

D3六槽的现场结果已由SD00读取：RED仅预期三失败、GREEN54/54；同编译CA01基线12.0840622秒，两个无trace修复后2.9071990／2.9073291秒（约75.94%减少），trace2.9626656秒。SD00独立配对459区间，仅完整Resolve减少33，其余七边界次数不变。产品四文件55行增删、原测试文件只插入117行，原34实例完整通过、新增20项。C尚需正式交付，原R尚需实际补丁审查；以下只是下一步准备，不能绕过这两个门。

下一Unity代表性集合拟固定为三个完整fixture：`FightMatch.Core.Tests.FirstReleaseContentStorageTests`、`FightMatch.Core.Tests.PublishedContentCatalogTests`、`FightMatch.Core.Tests.DemoContentReplayTests`；以及以下五个精确原实例：

- `FightMatch.Core.Tests.PlayerRosterSessionTests.CA01_RealApprovedWarriorMovesThroughAllSlotsAndEmptyWithoutReplacingItsOriginalIdentity`
- `FightMatch.Core.Tests.PlayerRosterMigrationTests.CA11_OldSchemaTwoFinishesItsOriginalRouteBeforeExplicitReversibleMigration("victory")`
- `FightMatch.Core.Tests.PlayerBattleFlowTests.B02_ActualMultiFaceHistoryListRetainsOriginalRangeAndCommitsItsConfirmationOnce`
- `FightMatch.Core.Tests.PlayerSaveRecoveryTests.SettlingOriginalS17AfterUnknownWriteAndReopenAwardsOnlyOnce("snapshot-promoted","SaveFailed")`
- `FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H04_RealBoardVictorySettlesOnceAndTheOriginalReceiptSurvivesWholeHostReconstruction`

原full01 XML对此集合有118项／合计220.008607秒case duration（仅原执行时间，不是完整命令），其中五业务例125.089953秒。加源码明确的20新增，预计138项。集合覆盖首包/通用存储/默认重演和迁移、保存失败恢复、多面战斗、Host生命周期；不把它称作新日常套件或全量。SD00须在任何新Unity结果产生前从旧4106 XML及新增测试源码冻结完整期望（全量4126、代表性138），并校对过滤正则精确选中同一多重集合。

D4应沿用现有Unity工具和上一轮已核实包装器的纯保全辅助，不安装工具、不改旧脚本/证据。计划单一新槽`unity-runs/targeted01/`串行必要Compile→上述代表性EditMode，完整计时含前后检查。纯准备和只读preflight不启动Unity；最终准确文件/命令/源码与脚本身份、证据叶名、I/O余量及执行门待D3正式交付与R实际审查后补签。未授权full槽；据真实Unity收益及剩余时长再决定必要全量。无论局部数据多好，全量十分钟硬上限仍须实测。

仅SD00现可CreateNew本目录`expected-tests.json`（≤32MiB）：从Sept29 full01精确XML和当前新增测试源码独立提取20实例，绑定来源大小/SHA、源方法位置、原/新增多重集合和代表性过滤器。不得从未来Unity输出倒推期望。若C后续修改该测试，旧期望保留且本文件不得被悄然重绑；先明示变化再签新的准确期望。本纯只读分析及期望落盘可与C报告收尾并行，不授权C提前开始D4或改变D3范围。

### D3正式交付收件及D4工具准备许可

C回合`01a0f02b-94ca-7313-a45f-d4f86976bc6e`已于1790738110正式completed，final `msg_0d179092dafe07fa016abc7eb7e00887d0bed7edb14986ac83`，安全停写。SD00已读取实际五文件差分、六槽和原始trace并独立复算，无覆盖旧失败。报告`reuse-implementation.md`10434bytes/SHA`884ad3f028ee3b48e9c7e3188ee769906733afde1c22c55d612b85816c77092e`、scope258295bytes/SHA`8e81beb79cb0e193a24fe70ddbc06267a5d6b9b51976b6d78c4abc547449f302`；最终源码指纹`cdb2751255ab26a14bdb7d18f322f5342a735b8e1da41ba73e6105570e47ff06`。原R已被派实际D3审查，尚未返回。此次收件不是最终接受。

SD00独立创建的`expected-tests.json`为22962bytes/SHA`1487750000f9817fd0ab534032a533f33e1fab94c5222e6b97f0f346d7fd9f24`，绑定测试源码24140bytes/SHA`831e513d087cf46c58c61cb84e7a2928b34e43fc0cc216bb25f0b3bf5ac12f40`。独立期望为原4106＋新增20，全量4126／代表性138；没有使用未来Unity结果。

为使工具准备与R审查并行，现仅授权SD00创建本目录`run_unity_validation.py`（≤420行）和`unity-preparation.json`（≤32MiB），复用Sept29同名已核实包装器的纯只读辅助，形成D4准确固定目标/预期/当前源码和计时门；仅做Python AST、差分、只读preflight，不执行Unity。角色边界保持：SD00不写产品、不运行Unity，C仍唯一串行执行者。新runner只接受targeted模式，不提供full执行入口；原R实际补丁通过及SD00核工具/白名单后再派C执行。不得修改D3已交付文件和源，原失败及证据全部冻结。

## D3独立接受与D4准确执行签发

原R回合`01a0f050-2fad-7872-8035-68872a460df5`于1790738586正式completed，final `msg_0d29bf8d726d8dc0016abc8085615887d08fb85b597141ccab`，唯一verdict **ACCEPT（仅D3）**。独立核实际差分、初始化与能力/预算/可变存储边界、三出口RED/GREEN、原34实例及断言、真实编译加载身份和459对trace，未发现阻断。SD00据此接收D3限定实施；**全量≤600秒尚未达成**。

D4现在只授权原C执行一次串行Compile→138项代表性Unity EditMode。准确命令：

```text
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_unity_validation.py --mode targeted --source-fingerprint cdb2751255ab26a14bdb7d18f322f5342a735b8e1da41ba73e6105570e47ff06
```

- runner实物326行／22319bytes／SHA`67642c5eb4657d90172af2384effcd7cf7eed1bb3f56db89ad250000f2c1cab1`；SD00已读差分、AST及退出路径，唯一可选模式targeted，preflight在建槽或Unity之前return。
- SD00只读准备证据`unity-preparation.json`4061bytes／SHA`f1e6e7c3233d8ce5c64f08b8aa6145f81b8795dda21d58763a90a5526ab67fc9`。原沙箱ps权限失败已保留，明确只读权限重试exit0；没有Unity启动。fresh源指纹、106 Mac DLL/PDB与D3/Sept29基线一致；CONT4834目录／22632叶、Host92／598，预留余量通过，SceneTemplate不存在。
- C先核准确runner、expected、preparation及D3报告身份；运行时脚本再次fresh校验源、原DLL、所有旧证据和ps，成功才建槽。若有其他Unity占用、源变化、保全失败或权限真实阻断即停，不绕过门、不强杀进程；普通只读进程盘点及本已授权Unity命令的必要权限可按现有作用域处理。
- 唯一新运行目录`unity-runs/targeted01/`，CreateNew，准确20叶：`run.json`、`before.json`、`after.json`、`io-before.json`、`io-after.json`、`host-io-before.json`、`host-io-after.json`、`protection-before.json`、`protection-after.json`、`compiled.json`、`natural-cleanup.json`、`named-tests.json`、`result.json`、`tests.xml`、`compile.log`、`compile.stdout.txt`、`compile.stderr.txt`、`unity.log`、`unity.stdout.txt`、`unity.stderr.txt`。仅本槽run及cleanup文件可由包装器在核本轮拥有SHA后追加更新，结束后冻结；失败不覆盖。
- C可另CreateNew本目录`unity-targeted-delivery.md`（≤80行），绑定正式作者回合、命令/结果、五代表性业务例前后时长、源码/编译/流程安全身份与下一步事实。不重写runner/expected/preparation/PLAN、D3源或其证据，不再新增scope或大报告。源码、资产、meta、Packages/ProjectSettings和Git写入仍不在本包范围。
- Unity路径/参数沿已验证Mac2022.3.18f1、`-batchmode -nographics -buildTarget osxuniversal`；Compile用`-quit`，测试不用`-quit`。测试过滤器来自已冻结独立期望，须精确138项全Passed，无失败/跳过/忽略；原118＋新增20多重集合完全相等。记录五个原业务例同Unity环境前后值；这不是无过滤全量。
- 授权Unity正常导入/编译自然更新Library/Temp及其后台产物，不允许C直接改这些文件。Compile后固定真实DLL/PDB和源码，测试结束逐项匹配。若自然生成原本不存在的`ProjectSettings/SceneTemplateSettings.json`，仅在Unity自然退出、fresh盘点成功且3534bytes／SHA`5baf593374ad67246277a435d8d0ded7167696eef6e05d2d245670e490625d8c`完全匹配时，可先保全原始内容至本槽cleanup再删该精确临时叶；未知变化保留并停止。
- 原CONT/Host I/O每叶保全，仅现有测试在原两根中新GUID目录新增；本槽CONT新增≤64 GUID／1024叶，Host≤4 GUID／128叶，总上限仍分别6144／40000和256／8192。前置预留另含后续尚未执行全量的768／8192与30／512，不自动扩大或清理旧目录。
- 从最早Python入口到最终检查计完整墙钟，另记录Compile/测试进程及XML时长；超过时间目标也不强杀Unity。正确性/范围失败exit2并保留，成功exit0，结果明确`full_goal_verified=false`。所有JSON/XML≤32MiB，单日志≤128MiB。
- Unity自然退出后C核进程、源及保护证据，正式completed交回并停写。不得开始full、APK、BlueStacks或主029；历史聊天预览不覆盖此当前派发。SD00收到本槽真实收益后签必要下一步；本专项完成仍需4126无过滤全量≤600秒和原R最终证据审查。

## D4正式收件；D5可变存储路径诊断准备

C回合`01a0f057-b56d-7b02-9ce4-a474263a0b59`于1790739278正式completed，final `msg_0d179092dafe07fa016abc834a3d5c87d0bf1505fe9e2c1aa0`，Unity自然退出并安全停写。SD00已读取正式报告和实际XML/result/named-tests：138/138全Passed，22检查true，完整墙钟245.983819458秒，Compile34.510966625秒，XML179.72765秒。CA01 33.849215→5.517911秒、迁移19.513505→3.581535、恢复19.161726→2.833356、Host18.227429→4.156669；B02多面34.338078→36.644384，未受益。旧118case220.008607→147.034477秒，新20另25.473864秒。尚不据此外推全量。

固定源仍`cdb2751255ab26a14bdb7d18f322f5342a735b8e1da41ba73e6105570e47ff06`；实际新Mac DLL/PDB基线为D4 `compiled.json`28108bytes／SHA`ae1865e8bc8c6e6136e5de63c3ce50d21172180262f89f32416dfa0a18bccfe8`，106项指纹`c28c084d3dd9a6702f42b6f47e4fb92aa07425acf8cf9411f493c2d14aac4bdf`。D4 result8522bytes／SHA`47209ac5f02da6ab5d37112bcfaeb4699234e4650bd61c8944206c12c2b06d13`，XML106143bytes／SHA`e87a45653eb4bd98c05d39498e4365c89b9e99e7fea3c4d02fb5a0ba97e30ff8`。CONT4835／22634、Host93／612；原叶全保留、SceneTemplate未产生。原R已被派D4实际证据核验及下一候选边界预判。

SD00已读B02及PlayerBattleTestFixture：TwoFaces→Isolated使用通用MemoryStorage，D3限定FirstRelease实例复用不适用。下一候选仍须先量出通用内容验证与真实业务/准备份额；不擅改可变存储读取新鲜度。为避免Unity UI依赖及额外长跑，选原纯业务方法`FightMatch.Core.Tests.PlayerSessionTests.MultiLevelV2UsesTheSameFacadeWhileExistingV1KeepsItsExactPackage`：原类无SetUp/TearDown，PublishedFixture在同一MemoryStorage/Catalog发布两准确包、切换release，再走原实际Create/Open/入场流程。它不是B02或全量替代品，但直接覆盖其共同通用内容入口。原断言和原方法不改。

三个可证伪候选已向用户说明：重复完整准入主导（Resolve内Build/Evidence时间/次数占主导）；冷制作/发布主导（Build/Evidence主要位于Resolve外或Publish准备）；恢复主导（Decode/CheckSnapshot占主导）。先用plain/trace/plain同条件原方法区分，再签最小实施。

现仅授权SD00准备本目录`run_generic_probe.py`（≤360行）、`generic_probe.cs`（≤140行），从D2B真实NUnit小驱动和D4纯保全辅助复用，静态核Python/C#调用结构；不运行Unity或产品，不改任何D1–D4源/证据。真实执行由C在准确工具身份及下节诊断范围签完后运行。当前不授权任何新的产品/测试修改或full槽。

## D5准确诊断执行（DIAGNOSIS_AUTHORIZED）

SD00已准备runner171行／12466bytes／SHA`877b79501992d52d44205799f29f294d0d9a83f4bbb0f580d9b55f7a6e305886`，driver92行／5122bytes／SHA`e00b274cb05ae059f94eb301780cb580b4a248fdb3342ff143ead81c94815007`。已核Python AST、真实NUnit输出9字段格式及驱动唯一原方法，尚未执行。C现在只获准依次运行：

```text
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_generic_probe.py --label plain01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_generic_probe.py --label trace01
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_generic_probe.py --label plain02
```

1. 只读使用D4实际Mac编译结果：前八DLL为当前Library的Core、Content、Platform、FlowPuzzle.Core、FlowPuzzle.Validation、Application、QFramework、Core.Tests，逐项对D4 compiled.json及106DLL/PDB核验；第九NUnit仍为既定329728bytes／SHA`e388e140954de12970384331d1730e95b4613ef53c3326b6f8fa0fffdbc9314f`。本包只编译小driver，不编译产品/测试，不运行Unity。
2. 每个CreateNew槽`generic-runs/plain01`→`trace01`→`plain02`正常精确19叶：上述九DLL、`Assets/FightMatchContent/demo-r1.source.json`、`generic_probe.cs`、`generic_probe.exe`、`manifest.json`、`commands.json`、`compile.stdout.txt`、`compile.stderr.txt`、`stdout.txt`、`stderr.txt`、`result.json`。唯一作者源从工程原字节复制及绑定；原方法实际建立其隔离fixture，不伪造业务/断言，不复制无关六首包或Unity UI程序集。各叶≤32MiB。
3. 原纯业务多包方法由真实NUnit SimpleWorkItem执行一次；必须唯一精确名称Passed，完整原断言正常返回，实际asserts如实记录。原类无fixture setup需补行。计整个WorkItem/Result.Duration/进程/完整Python墙钟，六秒仅为敏感诊断门；不宣称是B02 UI或全量验证。
4. 只有trace01启用固定九边界：Catalog.Resolve、PublishedContentCompiler.Build、PublishedContentCompiler.EvidenceBytes、Runtime.CheckPlayerRoots、PlayerSession.ResolveRoots、Runtime.DecodeApplication、PlayerSession.CheckSnapshot、Lifecycle.Submit、ApplicationSystem.Restore。不做逐数学操作trace。按完整签名/线程栈配对，父子不重复计时；某边界未被此原方法调用须如实说明，不造零成本结论。原始trace留存；SD00负责独立复算与下一决策。
5. 每次compile/独立Mono进程上限60秒；超时仅终止该新诊断进程组并保留124，绝不操作Unity。前槽真实正确性/依赖失败即停后继，性能RED允许继续。禁止覆盖失败重试、擅改runner或扩大依赖/输入。
6. 每槽前后保全固定CDB源码、D4的106DLL/PDB、D1–D4全部原始文件/槽及已完成D5槽和实际工具身份；ps必须成功且无Unity占用。普通必要权限按已授权只读/独立诊断作用域处理。任何源、DLL或证据变化均停止，不偷偷重绑基线。
7. C可CreateNew `generic-diagnosis-delivery.md`（≤40行），仅原始三槽链接、真实结果/时间、保护身份及正式安全交回；不再写大型scope或重做分析工具。现有完整manifest/result绑定全部原始身份，SD00分析原trace即可。产品/测试/配置/依赖/旧工具/旧证据/Git均不准修改。
8. 三槽及简短交付完成后C正式completed并停写。全量、B02 Unity诊断和可变存储结果复用均尚未授权；需据实测补签。当前仍继续用户全量争取五分钟、至多十分钟目标，不能以局部GREEN结束专项。

## D5建槽前失败收件与运行时别名检查最小修正

C回合`01a0f068-5cd8-7682-b431-e0db9f14dd56`于1790739992正式completed，final `msg_0d179092dafe07fa016abc8614b12c87d08c87f3d23e957ce1`。首命令在创建任何槽、编译或执行前因工具拒绝既有Mono符号链接而exit1；原始stderr及12项保全结果保存在`generic-diagnosis-delivery.md`（SHA`87685ee56454268db611d3354272341a654d3968926b60b294f6cf8fffc3c44e`），不得覆盖。三个槽从未创建，业务尚未运行。

SD00依据实物最小修正自己的诊断工具：CreateNew `run_generic_probe_initial.py`保留原12466bytes/SHA`877b79501992d52d44205799f29f294d0d9a83f4bbb0f580d9b55f7a6e305886`，只在现runner身份函数准许十项原运行时清单中的四个既有别名，且必须解析到源码固定的四个准确目标；每项仍逐字节大小/SHA匹配D1清单。其余文件仍须普通文件。将失败报告与原runner也加入前后保全，未更改任何产品或driver、输入、NUnit入口、计时或成功门。SD00纯AST和十项身份核验通过；未运行诊断/Unity。修后runner 178行/13118bytes/SHA`85be405b697bdc7cf0d16138e91ba6382b439bd1244aa7a89a64433dc93a3d0e`；driver身份不变。

现再次派原C执行D5原三条准确命令，仍按plain01→trace01→plain02顺序；这是对从未创建槽的首次执行，不覆盖/重试已有失败槽。所有D5旧约束不变，原失败完整保留。C交付新增改为`generic-diagnosis-delivery-r1.md`（≤40行），原报告不得改写；任何新异常仍停止依赖步骤，不自行改工具或产品。C正式completed后才接续实施决定。

同时收件原R D4回合`01a0f063-8388-7730-852a-c3597c46e436`于1790739941 completed，final `msg_0d29bf8d726d8dc0016abc85d1fc9487d0b061ca30f329c46a`：唯一verdict **ACCEPT（仅D4）**，独立90检查通过，138项与期望相等；全量4126及≤600秒尚未验证。通用复用仅给条件性候选约束，尚未签发产品实施。

## D5原始分段收件；D6单条通用准入复用草案（仅预审，不执行）

D5三槽原始结果已出现，C仍在安全收尾，尚不凭文件视为正式完成。实际同一原方法各40断言Passed；plain01 12.1426526秒、trace01 11.3851379秒、plain02 10.8483487秒，三个correctness及全部13保全检查true，性能门仍RED。SD00独立按线程/完整签名/深度配对143个ENTER/LEAVE，无错配或未闭合：Resolve21次8.808520秒（77.37%总用例）；其中Build4.650670秒、EvidenceBytes3.186880秒，Resolve剩余0.970970秒。Resolve外首次制作Build/Evidence另1.308500秒；Decode23次0.056970秒、CheckSnapshot23次0.022440秒。重复准入是实测主要成本，恢复解码不是当前主要瓶颈。Build/Evidence各23次，ResolveRoots13、CheckPlayerRoots13、Lifecycle.Submit4；ApplicationSystem.Restore本例未调用，不能凭此推断其成本为零。

据此拟D6：可变存储每次读取与哈希保持新鲜，仅相同完整内容复用已成功的后缀验证结果。D3仍有效；此次不是改发布/业务或删测试。下列范围待原R按实物预审及D5正式交回后，由SD00准确派发才生效。

### 拟产品/回归白名单

- 唯一修改产品：`Assets/Scripts/FightMatch/Content/PublishedContentCatalog.cs`，增删合计≤180行；保留风格，不移动/重排无关代码。
- 明确新增：`Assets/Tests/EditMode/FightMatch/PublishedContentCatalogReuseTests.cs`（≤450行），单独纯Memory NUnit fixture；以及同名`.cs.meta`（沿现有MonoImporter格式，唯一新GUID，≤12行）。独立fixture避免为了编译现有Catalog测试中的物理I/O帮助类而扩展诊断依赖；本包仅准这一个新测试meta，不允许改任何既有meta、asmdef或资源。
- 只读相关上下文：上述Catalog及Content目录十个既有源、Core/ExactMathBudget.cs、Platform内容存储接口、PublishedContentTestData.cs、D3所有权回归、原PlayerSession及PlayerSessionTestData。所有既有测试/断言保持原字节；Core/Codec及其Sept29回归四冻结文件继续冻结；D3其它三个产品文件继续冻结；Application、UI、算法、保存/恢复、API、配置/依赖/Git均不在写范围。

### 设计及可观察验收

1. 通用存储仅在**单个Catalog实例**保存至多一个成功准入条目；无全局/静态/无界字典、跨Catalog或跨存储共享。条目完全构造后原子替换，私有只读字段持有准确binding、能力全部值、完整原始字节、自有ResolvedPublication和后缀实际数学成本。读取先捕获一个条目本地引用，再对同一个条目比较和返回；可用volatile条目引用，不锁外部存储、不改变发布锁行为。
2. **Publish继续原完整Resolve及调用者budget，不读/写这个复用条目。** FirstRelease实例继续D3既有GetAdmitted；其未命中保持原完整路径，不扩张实例准入条目。只在通用存储的ResolveExact/GetCurrentBinding只读入口启用新复用，允许原Resolve内部私有布尔参数默认false，但发布两个调用及其可观察执行顺序不能改变。
3. 每次仍按原先后顺序读取/解析index、核binding，再短路核operation及receipt，再依次读取/哈希payload、source、validation、review。operation不等时**不得读receipt**；某个ReadHash拒绝时不能先读取后续记录。读到的数组所有权未知，启用复用路径立即取得私有副本；本次解码、哈希、比较、冷验证及新条目均用同一批副本。operation/receipt完整等于index后无需额外重复保存两份。不得用哈希相同替代完整字节比较。
4. 仅准确binding、全部consumer能力内容与三个限额、完整index及四内容字节、MaxRecordBytes、MaxIntegerBits等影响后缀结果的条件相同，且本次**剩余**MaxPrimitiveSteps−PrimitiveStepsUsed足够覆盖原成功后缀实际步数，才允许返回已准入只读图。成本只从本次读取/哈希前缀结束到原后缀完整成功的实际步数差取得，不猜成本或改Core API；剩余判断以long安全比较。MaxPrimitiveSteps数值本身不同不必禁止命中，但剩余步数必须足够，整数位约束不可变宽后借旧成功绕过较小上限。
5. 不满足条件时从**当前已读取同一批字节和当前budget**继续原Normalize/Build/重放/Evidence/Review后缀，不能重启Resolve、再次读记录或重置/重复扣费。失败不保存成功条目。证明启用只读入口的Math预算不向调用者暴露、命中后无后续Math消费；GetCurrentBinding外层release读取/解码/身份及receipt SHA、DefinitionBinding外层level/version核对仍实际执行。
6. 保存对象沿D3已封闭的只读图，不添加新的可修改出口。EvaluateAction、ReplayRecorded、全部Current/RetainedRoots读取解码与初始化证明仍实际执行；不以复用替代任何玩家动作/恢复。可变存储暖后缺件、篡改、读取异常、不同包切换及不同能力必须与新Catalog冷路径同结果/错误字段，原始字节未变才复用。
7. 新回归最低覆盖：七记录分别缺件/损坏/IOException；暖后正确release/错误receipt、错误level与不同binding；每项能力/限额改变；底层返回借用数组后来改变不能污染已保存条目；并发两不同包替换不能交叉返回；operation不匹配同时receipt抛异常时保持原RecoveryBlocked/Receipt.Identity且不读receipt；四ReadHash逐条失败不读取后续记录；所有错误不写存储。
8. 预算回归用实际冷路径测得完整成本，比较剩余−1／恰好／＋1及已消费前缀、整数位上限改变；需覆盖真命中和预算不足原后缀，不仅比较引用。允许测试通过反射调用既有私有ResolveReadOnly及不复用Resolve，以传入测试自有预算，捕获真实异常/结果；不反射修改私有状态、不加公开/内部测试专用API。可用既有Core friend能力给自有budget预消费，不写Core。Publish冷/暖继续相同原调用者预算和错误行为。
9. 不添加NUnit墙钟断言。性能RED已有D5真实业务；新正确性测试可在旧产品全部通过，不能伪造预期功能失败。新fixture先在未改产品建立行为基线，再改产品；采用同编译参数的旧/新原MultiPackage真实40断言对照及九边界计数，说明Build/Evidence降幅和保留的业务次数。D5是实际Mac基线，不能与独立csc直接比较称收益。

### 拟紧凑验证和证据范围

原C可在本2026-09-30目录创建`run_catalog_reuse_probe.py`（≤400行）、`catalog_reuse_probe.cs`（≤180行）、`catalog-reuse-delivery.md`（≤110行）、`catalog-reuse-scope.json`（≤32MiB）；CreateNew `catalog-reuse-sources-before/Assets/Scripts/FightMatch/Content/PublishedContentCatalog.cs`准确备份唯一产品原字节。工具可复用D3真实fixture驱动、D5修正后的保全辅助；所有D1–D5工具、失败/槽/报告冻结。不得产生pycache或复制整个工程。

固定六槽`catalog-reuse-runs/before-tests01`→`baseline01`→`after-tests01`→`after-plain01`→`after-trace01`→`after-plain02`，统一`python3 TestArtifacts/FMTestPerformance/2026-09-30/run_catalog_reuse_probe.py --label <固定值>`。前fixture是新回归+原PublishedContentTestData，只构建该精确新fixture并经真实NUnit完整OneTimeSetUp/WorkItem，不运行已有物理fixture。业务槽仍从D4真实Core.Tests调用原MultiPackage方法；两种测试DLL不得同进程混装。

每槽允许精确叶集合（最多23叶、各≤32MiB）：九既定DLL；`Assets/FightMatchContent/demo-r1.source.json`；`catalog_reuse_probe.cs`、`catalog_reuse_probe.exe`、`manifest.json`、`commands.json`、`content.compile.stdout.txt`、`content.compile.stderr.txt`、`tests.compile.stdout.txt`、`tests.compile.stderr.txt`、`driver.compile.stdout.txt`、`driver.compile.stderr.txt`、`stdout.txt`、`stderr.txt`、`result.json`。未发生编译时相应日志可省略，失败产物不得覆盖。独立Content只编译D3准许的确切十个现有源；除唯一Catalog变化，其余源逐SHA不变。所需其它八DLL均从D4原Mac/NUnit基线取得；csc/Mono仍为D5十项准确运行时。同状态已编译Content可从已完成对应fixture槽逐SHA复制；before/after必须相同编译参数。

每个编译/业务独立子进程仍60秒；新fixture可能较多，允许该精确fixture进程上限120秒并在60秒内向用户说明进度（exec非阻塞yield），仅可超时终止本包新诊断子进程组。该上限不授权Unity或其它用例。前槽正确性失败须保留并停止后继；性能RED允许继续。不得为减时删正确性断言，若范围/依赖或工具实际失败，保留失败再请SD00签最小更正及新槽。

每槽前后比较所有非三白名单源/配置/资源、D4实际106 DLL/PDB、D1–D5全部证据与前槽、运行时/驱动；记录原产品备份与精确差分、新测试用例清单及源身份、实际程序集加载。ps成功且无Unity占用；原CONT/Host I/O不得改写或新增（纯Memory）。报告完整原始XML、实际40断言、配对调用次数、各秒数/完整墙钟、下一残差。六槽与正式C交回后由原R审实际补丁；本D6不启动Unity、不预授全量。仍需依据测得收益固定最终源码并签必要Unity验证及无过滤全部原4126＋新增测试≤600秒。

### D5 r1正式收件

C回合`01a0f074-2028-7183-996f-8d853517246a`已于1790740836正式completed，final `msg_0d179092dafe07fa016abc896056a087d0b6dedbcf21f31ab0`。三槽19叶/槽、各40断言Passed，正确性与13保护检查全true；六个子进程自然退出，源CDB及D4实际DLL不变。SD00已读取原报告/三槽result/XML并独立20项身份及内容核验全过、143对原trace无错配。D6草案已派原R做边界预审，待其正式回复后补签准确执行，不改变D5已交回范围。

## D6正式执行签发（IMPLEMENTATION_AUTHORIZED）

D5已正式交回如上。原R回合`01a0f078-d571-7551-80ac-1c97ecf9649c`于1790741054 completed，final `msg_0d29bf8d726d8dc0016abc8a30387887d095b730f4c1a1423d`：合同从产品边界可执行，未发现必须补签的实质漏洞；这不是D6产品最终ACCEPT。SD00据用户既有授权，现将上节D6完整草案范围、三源文件白名单、精确六槽/命令及所有验收正式签发给原C实施。

实施明确按R对现合同的解释：MaxIntegerBits必须**精确相等**，更宽也可能改变24×位数临时空间检查结果；index在原null拒绝后/Decode前复制，其余记录在原Read异常和null拒绝后/比较或哈希前复制，不先哈希借用数组再复制，也不提前consumer大小检查抢占原失败顺序。只读预算命中省下步数不可对外观察；测试不要求命中计数等于冷路径，预算不足必须同读取批次同预算原后缀。完整成功后缀成本覆盖其内部对剩余量的预检。无需Core/API扩大。

D6开始源为CDB指纹及D4真实106 DLL/PDB，原所有权补丁和D1–D5证据逐项冻结。C先准确备份Catalog，建立新增fixture和验证工具，执行before-tests01及同编译baseline01后才改产品。真实功能失败如未在合同内预期，保留并停止，不能为了RED制造引用相等/计时断言失败；性能RED由原业务同编译baseline提供。新文件全部CreateNew；新meta只给本新测试一个唯一GUID。C不得改PLAN或旧报告。报告需绑定整个新fixture具名实例多重集合，便于SD00在未来Unity结果之前独立冻结全量期望。

固定原MultiPackage业务仍40断言；trace只after-trace01，Build/Evidence、ResolveRoots13、CheckPlayerRoots13、Decode23、CheckSnapshot23及Submit4的实际次数和差异均报告，Restore仍如实说明未调用。预期只读复用缩减Build/Evidence重复；不规定私有Resolve本身必须减少，以免把保留前缀读取误判失败。保持原六秒业务诊断信号，不把局部GREEN当全量验收。

全部六槽、差分与保全完成后C正式completed停写交回，SD00收件及原R实际独立审查通过后才签下一必要Unity/全量。主029、APK/BlueStacks及旧预览始终不属于当前任务。保留所有原始失败；不得因任何普通诊断异常扩scope或修改受保护文件。

## D7工具准备（SD00独立准备，尚未授权Unity）

在C实施D6期间，SD00可仅创建本目录`run_full_validation.py`（≤430行），以D4已接受runner为起点，形成单次Compile→无过滤EditMode的完整计时及保全包装器。现阶段SOURCE/EXPECTATION使用明确未绑定状态并拒绝执行，SD00仅AST及差分检查，既不运行Unity也不执行产品。后续D6准确C交回＋原R实际ACCEPT、源/新增测试固定后，SD00才可CreateNew `expected-full-tests.json`和`full-preparation.json`（各≤32MiB），据原4106 XML＋D3 20项＋D6源码/已核实NUnit具名实例冻结完整期望，并将准确源码/期望SHA签入新runner，做只读preflight。D1–D6工具/证据不改。C本回合不读写或执行这些SD00准备文件；其保护树应按已签固定D1–D5和自身前槽，不将仍在准备的D7新工具当产品变化。

拟直接一次完整验证，D6微回路及独立实际审查通过后不再重复D4的相同138项预跑；B02和其它原代表性时间从完整XML取。若实测正确但>600秒，保留真实性能RED并根据新分布继续，不视为整体完成。尚未授权创建或执行full槽；准确身份、证据范围、运行命令和I/O余量须稍后签发。计时含必要Compile、Unity启动/全测试和包装检查，五分钟为目标、十分钟为硬上限；Unity超时不强杀，结果如实记录。

D7未绑定准备版已由SD00创建：`run_full_validation.py`342行；仅允许full模式，不含任何testFilter，Compile有-quit、测试无-quit；当前未绑定即在进程盘点前拒绝，SD00已静态及纯参数检查证实。继承D4所有源/历史证据/I/O/自然SceneTemplate处理，增加D4当前Unity可执行文件身份对照、完整20叶范围检查和600秒真实结果门；范围检查包含在墙钟终点前。该工具准备不授权任何进程执行。

D7工具静态预审收件：R回合`01a0f082-ff5f-7671-8c5b-1cc3dd9ca703`于1790741642 completed，final `msg_0d29bf8d726d8dc0016abc8c7f716887d081e3e265b158ec1a`。指出成功门仅限制≤19叶/白名单而未要求齐备；SD00按唯一最小修正将成功门改为准确集合`allowed_leaves - {'result.json'}`，仍逐项普通文件/非链接/大小核验，再CreateNew result成为20叶。失败可不完整但不会通过。其它无过滤、计时、600秒退出/不杀Unity、保全控制流未见缺口。本次仅静态预审和工具修正，尚未绑定或执行，不等于D6/全量接受；R实际审查时复核此准确差分。

### 后备候选只读意见（未授权实施，D6不变）

原R回合`01a0f087-8817-7580-8795-f854470a8d3a`于1790742002 completed，final `msg_0d29bf8d726d8dc0016abc8de4c44887d09af73203bf20bc55`。检查PlayerSessionTestData.RealCatalog及直接调用者后，条件性认为可在未来单独测量“测试帮助类共享成功准入的不可变FirstReleaseStorage、每次仍new Catalog”：未见业务测试要求每fixture重做cold Create或不同内容对象引用；CA13及Navigation重建已复用Catalog，B10只要求只读引用/内容。不可共享可变RealSource、玩家/存档/定位器、架构、预算或随机状态；初始化须安全，失败不能成为可用缓存。当前Content(Create(...))实际有成功断言，不能随初始化缓存而减少，也不能补无意义断言抵数；专门FirstRelease54项冷输入/篡改/预算/实例隔离回归保留，每个新全量进程仍真实六文件首次准入并计时。Host已独立缓存Catalog，B02/多包/自定义MemoryStorage不受此候选影响。

若D6后仍需此候选，先实测RealCatalog→Create累计准备、Memory准入及实际业务的非重复计时，区分首个cold和后续shared准备、不同用例顺序的玩家状态/原断言/调用次数；收益只能表述为测试准备复用，不能冒充产品冷启动改善。当前没有对此文件或任何额外诊断/Unity签写范围；优先完成D6实测及实际审查，再据必要性决定。

## D6正式交付收件（等待实际独立审查）

原C回合`01a0f07d-01f9-7da2-8cb5-d5ba9075710e`已于1790743237正式completed，final `msg_0d179092dafe07fa016abc92bee2c487d0a4b6c05cbde704b9`，停写交回。报告`catalog-reuse-delivery.md`10648bytes/SHA`52eacf8fe03b1e843ef312b73b09189204bea71b043b9cfa71a4d15be5f685a8`；scope211763bytes/SHA`28abd7a461627bb5c49451c608fb50e33e60eed946bd3b68804a1e4f65a6fa68`。

SD00已读实际产品差分、全部新增288行回归及真实NUnit驱动/runner，独立核源差异精确三文件、65行产品增删、meta唯一GUID、六槽每叶SHA、预算原始行及109对trace。源码提取13方法55实例与前后NUnit多重集合精确相等，均55Passed/788断言；四业务槽原40断言一致。同csc基线10.2446726秒；after-plain01 3.9755535、trace3.8140858、plain02 3.7659255，分别61.1939%/63.2402%减少。Build/Evidence各23→6，其余Resolve21、Root13、Decode23、CheckSnapshot23、Submit4不变；D5和D6 trace绝对时间不跨编译比较收益。

当前准确源码指纹`dfe22e0133e3af2b3eaa2061804cd8cc1fac83f43aff2210d9172cc0d3df984a`；Catalog20125bytes/SHA`766b198ba1e6547c89a5dcf2155fd2c2c651002303f618972288779122b0a25f`；新测试21340bytes/SHA`b33e6e2807d997999cc51a65bb65b51c7b8e64b5eb7646cc17befe708998b1e9`；meta243bytes/SHA`2a07cc79944a3110ab266fe4529e4253598d189b98e9b891dd624f0819b84e5c`。D4实际106DLL仍C28指纹。C最后80项保全检查通过，16子进程退出，原I/O4835/22634与93/612不变，无Unity/新I/O。本轮D6实施尚待原R实际唯一verdict；当前没有Unity执行授权，预计全量4181仍待验证。

D7期望准备前移补充：D6 C已正式completed且源固定，现允许SD00在原R只读审查期间仅CreateNew `expected-full-tests.json`，依据已冻结D3源码/原4106 XML及D6源码独立提取55实例，并与D6已完成NUnit交叉核对；这不会执行任何产品或Unity。runner的执行身份绑定、完整只读preflight及实际Unity许可仍等待D6原R ACCEPT。如R要求源码修正，则保留本期望，不悄然重绑；须另签准确新文件。此补充仅前移独立数据准备，不弱化原门。

## D6独立接收与D7身份绑定（只读准备）

原R回合`01a0f09e-16a2-7ba1-bac6-533ee10ba1d1`于1790743724正式completed，final `msg_0d29bf8d726d8dc0016abc94a0125087d08168fc6a341564c2`，唯一verdict **ACCEPT（仅D6）**。270独立证据检查全通过，55实例/788断言（778用例＋10初始化）、40原业务断言及109对trace独立相符，源/106DLL/历史证据/I/O完整；无需纠正包。SD00接收此限定优化，4181全量≤600秒仍未验证。

独立期望`expected-full-tests.json`36961bytes/SHA`4d52e9f5dd24e832f1c37db7065e5ba3a4f6a9186cd0501419cb3766ae72046b`，绑定D3 20＋D6 55及原4106，未来完整应4181。其execution_authorized=false记录的是期望创建时尚未授权，实际权限仍由后续签发决定；不改写此历史字段。

SD00将已静态审查的D7工具从未绑定态，仅修改说明及SOURCE/EXPECTED两个常量，准确绑定D6源dfe22e…984a和上述期望SHA；此前精确19叶修正已由R复核。当前runner 342行/24058bytes/SHA`8ffe06976ab5c5b55cef1dd7039f806c38d0cd03877c400528972ca51529c521`，仅full模式、无过滤，600秒门保持。现在仅做只读preflight，不启动Unity；执行仍待下面准确签发。

## D7准确Unity全量执行签发（FULL_VALIDATION_AUTHORIZED）

D6已获原R实际ACCEPT。SD00只读preflight先因沙箱禁止`/bin/ps`而失败，已保留原始stderr；针对同一`--preflight`只读检查的必要权限执行后exit0，无Unity启动、无full槽。准备证据`full-preparation.json`8980bytes/SHA`a52dfb69e5981d279d59272ae8a98dc2c076744fa66fd640b8e0165334d7d7a2`，准确runner342行/24058bytes/SHA`8ffe06976ab5c5b55cef1dd7039f806c38d0cd03877c400528972ca51529c521`；期望36961bytes/SHA`4d52e9f5dd24e832f1c37db7065e5ba3a4f6a9186cd0501419cb3766ae72046b`。源dfe22e…984a、D4实际106DLL、全部历史证据、Unity86759760bytes/SHA`71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f`及预留I/O通过fresh核验；SceneTemplate不存在。此只读权限通过不是Unity实际运行结果。

现在只授权原C执行下面一次准确命令，脚本内串行Compile→**无任何testFilter的全部EditMode**：

```text
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_full_validation.py --mode full --source-fingerprint dfe22e0133e3af2b3eaa2061804cd8cc1fac83f43aff2210d9172cc0d3df984a
```

1. C先核runner、expected-full-tests、full-preparation及D6交付身份；入口再次fresh核源/D4 DLL/全部历史证据/ps/I/O。源码、已有测试、meta、配置、依赖、资产和Git均不可修改；任何不符即保留并停，不偷偷重绑、不重跑或启主029。
2. 仅新CreateNew槽`unity-runs/full01/`，精确20叶：`run.json`、`before.json`、`after.json`、`io-before.json`、`io-after.json`、`host-io-before.json`、`host-io-after.json`、`protection-before.json`、`protection-after.json`、`compiled.json`、`natural-cleanup.json`、`named-tests.json`、`result.json`、`tests.xml`、`compile.log`、`compile.stdout.txt`、`compile.stderr.txt`、`unity.log`、`unity.stdout.txt`、`unity.stderr.txt`。只run/cleanup在运行中可经包装器owned SHA核对更新，完成后冻结；不得覆盖任何历史失败或槽。
3. 沿已验证Mac Unity2022.3.18f1与`-batchmode -nographics -buildTarget osxuniversal`；Compile有-quit，测试无-quit，无筛选。预期4181＝原4106＋D3 20＋D6 55，具名多重集合须精确相等并全Passed，零失败/跳过/忽略/额外。D6 Mono通过不替代Unity发现和执行。
4. 从最早Python入口开始，完整计入preflight、Compile、测试启动/所有用例及最终范围检查；另记录Compile/测试进程/XML。最终JSON序列化与工具返回在计时终点后如实注明。正确性/保全失败exit2；正确但>600秒exit3且full_goal_verified=false；≤600正确才满足上限，≤300另记五分钟目标。**超过阈值不杀Unity、不截断测试**，保留真实完整数据；R最终审之前不得称专项已达成。
5. 仅Unity正常导入/编译可自然更新Library/Temp及后台生成文件；C不能手工改。编译后绑定真实DLL/PDB，测试后完全相等。若自然生成原本不存在的SceneTemplateSettings，仅进程自然退出、fresh盘点成功且原规定3534bytes/SHA`5baf593374ad67246277a435d8d0ded7167696eef6e05d2d245670e490625d8c`相符，先保全base64原字节再删除该准确临时叶；未知变化保留并停止。
6. 原CONT4835 GUID/22634叶、Host93/612全部保全；本次仅原测试在原根内新增独立GUID目录，新增分别≤768/8192和≤30/512，总上限仍6144/40000和256/8192。不清理旧I/O、不扩大配额。所有JSON/XML≤32MiB、各日志≤128MiB，成功20叶必须齐备且非符号链接。
7. 新建本目录`unity-full-delivery.md`（≤90行）可由C交付：真实总数、退出/时长及五/十分钟状态、六代表原实例和主要耗时类的实际前后值、新增75项耗时另计、源/编译/保全/安全收尾。用原始XML/result链接支撑，不新增scope或分析工具，不复写PLAN/runner/独立期望/准备证据/D1–D6交付。
8. 程序自然退出后C核所有本轮进程、源、准确20叶/旧证据/I/O，正式completed安全停写交回。若>600秒如实以性能未达标返回，SD00据新残差继续最小诊断/优化；不得以“已有明显收益”终结本目标。无额外Unity槽、APK、BlueStacks、主029、提交/推送许可。普通必要进程盘点和本准确授权验证的权限按现有作用域处理，无须再向用户重问已批准计划。

### D7授权可见性补证与实际启动

原C执行回合`01a0f0a7-d12c-7f50-8335-1490ce56a615`的第一次命令申请被自动审批在启动前拒绝：执行聊天当时仅识别旧BlueStacks授权，未识别本性能专项。C没有改命令或绕行执行，先只读本专项原聊天`01a0ec0e-6084-7be3-8af3-26aeeada73df`，查到实际用户对“按计划协调现有C实施及串行Unity验证、现有R独立审查”的答复“批准计划并沿用现有C/R”，再以原授权证据提交同一D7命令。SD00也独立读取并确认该原始答复。拒绝及补证保留在C原回合，非新增用户许可或扩大范围。

随后原命令实际进入唯一`full01`槽：入口2026-09-30T04:54:49.310006Z；Compile PID48046自然exit0，用时25.24956370899963秒；Tests PID48100于04:55:18.583701Z启动，命令无筛选。此登记时测试仍运行，未生成最终XML/result；不得据部分状态宣称4181通过或五/十分钟达标。

## D7正式收件：4181全过，14分09秒，性能仍未达标

原C回合`01a0f0a7-d12c-7f50-8335-1490ce56a615`于1790745284正式completed，final `msg_0d179092dafe07fa016abc9ac0cdc487d0aeb4e946ce5d21b0`。报告`unity-full-delivery.md`72行/8046bytes/SHA`67d83950e18f9c2331e8425471b1043ab6e72318a7de57563bfbbc375fed8271`，C停写、两个Unity进程自然退出，等待R。

实际无过滤4181/4181 Passed，零失败/跳过/不确定，原4106具名多重集合完整保留，新增精确75。完整墙钟848.5345519589991秒，XML799.8467854秒；Compile25.24956370899963秒、测试进程815.2157566250007秒。包装器23检查全过但按600秒门exit3；full_goal_verified和five_minute_goal_verified均false，距上限还需减少248.534552秒。本次含Compile，上轮全量槽仅测试；完整墙钟3267.031977→848.534552仅表实际等待差，严格同边界用XML3245.943681→799.846785，或同原4106用例和3231.265933→686.784615秒（减少78.74565%）。新增75用例另102.137409秒，D6新增55占80.262785秒，不能省略。

SD00独立解析原始两份XML与预先冻结期望，核具名Counter精确相等、全部Passed、23检查及20叶，result所绑定12项证据字节/SHA全部相等。原B02多面战斗34.338078→6.567140秒，CA01 33.849215→5.063129秒，其余六代表及按类分布见C报告；下一诊断不能以类总时间代替内部计时。

本次result7580bytes/SHA`0e26e97f3610aebc5710ccf7bb546522bd98b8aaf4721d95ba8f286a868bed20`；XML3102505bytes/SHA`b7b13ea83c85ad90a4722e09c34f5c4b587ee73d13344616e07a3a342a01f070`；compiled28104bytes/SHA`24528f386b2decfd4e13c3a36d2a16ecbca436413c9093fc2370753e9677886a`。准确源仍dfe22e…984a，实际106DLL/PDB已变为`8f5dbbf75473e4abb6849547d3c90fafe274e821a9749c0301dbf70f666343ca`；以后新诊断须绑定本次实物，不能沿用旧C28。CONT新增557GUID/2592叶，现5392/25226；Host新增23/150，现116/762。旧字节保全，SceneTemplate前后不存在，无清理事件。

专项继续。原R当前仅做D8帮助类候选的只读依赖准备，尚未执行D7实际独立审查；全量性能不得ACCEPT为目标完成。全部D7原始工具、槽及报告冻结，不因性能RED覆盖或删改。下一包将依据剩余重复准备的真实分段测量收窄，只复用成功且不可变的测试内容，不减原覆盖或业务步骤。

## D8最小测试准备复用草案（仅R预审，尚未执行）

R只读准备回合`01a0f0b4-ae7a-7ea2-b7bb-4e5a91e84c36`于1790745331 completed，final `msg_0d29bf8d726d8dc0016abc9ac06f5487d0b37a406ce6285634`。核得CA01/CA09完整源依赖17文件5857行，未见Unity原生API依赖，但未编译/执行。确认每序列保留同一已加载程序集、每次全新NUnit WorkItem/执行上下文/fixture，才能观察真实冷/暖；原断言及witness须逐次保留。以下是等待D7实际R审查和本合同检查后才签发的最小纠正候选，不授权C现在修改。

### 单一帮助类与量化前置门

唯一现有源码写白名单`Assets/Tests/EditMode/FightMatch/PlayerSessionTestData.cs`，起点276行/23124bytes/SHA`7db9770560a13f17476eb59c47f7e9c983c1518a4520b59949883e6638dc12d4`，仅RealCatalog及其私有字段，增删合计≤50行。原所有测试方法/断言、其余源及meta一律不变，不新增测试case或改变4181全量期望。此优化仅复用测试准备，不宣称产品冷启动收益。产品、预算、存档、UI、程序集配置、依赖、资源、Git及旧工具均冻结。

先完成下述三个before槽，真实分段证实每三用例序列都反复三次Create同一首包，且三次Create在before-trace01总用例时间中累计≥20%，方可继续该唯一帮助类修改。达不到或正确性/依赖失败则保留原始证据、停止，不猜测收益或另改慢类。D7已证实全量仍超248.535秒，但本门不承诺这一帮助类足够达成600秒。

实现只共享成功的`PublicationResult<FirstReleaseContentStorage>`及其D3已封闭的不可变Storage；六文件惰性读取与成功缓存发布在同一私有锁保护。初始化每次尝试使用原新预算；异常及不成功结果不发布到可用缓存。每次RealCatalog仍执行原一次`Content(result)`成功断言，并new独立PublishedContentCatalog。不得将该断言移到一次性初始化或补空断言抵数。RealSource可变对象、玩家、存档、Locator、预算、架构及运行态不共享；其它帮助方法和专门54项FirstRelease输入/失败/所有权回归全部不变。

### 准确编译与诊断范围

新目录仍为本2026-09-30：C可CreateNew `run_fixture_reuse_probe.py`（≤430行）、`fixture_reuse_probe.cs`（≤200行）、`fixture-reuse-delivery.md`（≤100行）、`fixture-reuse-scope.json`（≤32MiB），以及唯一备份`fixture-reuse-sources-before/Assets/Tests/EditMode/FightMatch/PlayerSessionTestData.cs`。不改PLAN，不安装工具。源指纹从D7 dfe22e…984a开始，实际106DLL/PDB绑定D7 compiled SHA24528f…886a及8f5dbb…43ca，而非旧D4。

独立测试程序集名仍`FightMatch.Core.Tests.dll`，仅编译下面17个**完整原文件**，均在`Assets/Tests/EditMode/FightMatch/`：PlayerRosterSessionTests.cs、PlayerRosterTestData.cs、PlayerSessionTestData.cs、PublishedContentTestData.cs、CandidateLifecycleTestData.cs、SharedRuleContinuityTests.cs、CandidateBusinessSaveCodecTests.cs、CandidateApplicationTestData.cs、CandidateApplicationRuntimeTestData.cs、CandidateBattleApplicationTestData.cs、LocalSaveStoreTests.cs、DemoContentFixture.cs、SaveCommitMarkerTests.cs、SavePendingRecoveryTests.cs、SaveRecoveryTests.cs、SavePendingRecoveryProcessCases.cs、SaveRecoveryProcessCases.cs。前后编译参数相同、其余16源逐SHA不变；不复制/删减方法、不stub、不引用或加载原Core.Tests DLL来重定义同名helper。只运行指定CA01/CA09，不因整文件编译而执行其它fixture或物理存档场景。

产品只复制D7实际七DLL：FightMatch.Core、FightMatch.Content、FightMatch.Platform、FightMatch.Application、FlowPuzzle.Core、FlowPuzzle.Validation、QFramework，加既有NUnit和准确BCL/System.Numerics/netstandard引用；使用D5已处理四准确别名的原十项Mono/csc/BCL身份。不重编产品，不写Library。原真实六`Assets/StreamingAssets/FightMatch/first-release.*`文件及`Assets/FightMatchContent/demo-r1.source.json`按原路径复制到独立槽，逐SHA相等；只读使用，不预热、不反射改静态状态。

准确六槽及命令为`python3 TestArtifacts/FMTestPerformance/2026-09-30/run_fixture_reuse_probe.py --label <固定label>`，顺序before-forward01→before-trace01→before-reverse01→（量化门通过后改唯一helper）→after-forward01→after-trace01→after-reverse01。forward/trace顺序为CA01→CA09→CA01；reverse为CA09→CA01→CA09。完整原方法名分别`FightMatch.Core.Tests.PlayerRosterSessionTests.CA01_RealApprovedWarriorMovesThroughAllSlotsAndEmptyWithoutReplacingItsOriginalIdentity`和`FightMatch.Core.Tests.PlayerRosterSessionTests.CA09_PrepareFreezesRevisionAndSlotsAndOriginalOperationWinsBeforeCurrentHeadAdmission`。每槽全新Mono进程，同序列逐次新建真实NUnit WorkItem/上下文/fixture；保留逐次原witness、XML、具名顺序、asserts、秒数及从最早入口开始的整命令墙钟。首个cold完整计时，不以暖态独占报告。

仅两个trace槽启用固定12边界：PlayerSessionTestData.RealCatalog、FirstReleaseContentStorage.Create、PlayerRosterTestData.RealRig、PublishedContentCatalog.Resolve、PublishedContentCompiler.Build、PublishedContentCompiler.EvidenceBytes、CandidateApplicationRuntime.CheckPlayerRoots、PlayerSessionSystem.ResolveRoots、CandidateApplicationRuntime.DecodeApplication、PlayerSessionSystem.CheckSnapshot、CandidateLifecycleApplicationSystem.Submit、CandidateApplicationSystem.Restore。按完整签名/线程栈配对并避免父子双计；Create预期3→1，RealCatalog/RealRig及玩家业务次数和每用例asserts保持before对应值。返回独立Catalog由最小源差分及实际trace对象身份佐证；若trace无法可靠提供对象身份，不伪造该字段。

每槽准确允许叶集合（最多27叶、各≤32MiB）：八复制DLL及新编Core.Tests DLL；原六首包与author共七输入；fixture_reuse_probe.cs/exe；manifest.json、commands.json、tests.compile.stdout.txt、tests.compile.stderr.txt、driver.compile.stdout.txt、driver.compile.stderr.txt、stdout.txt、stderr.txt、result.json。17源直接从工程准确读入编译并绑定身份，不额外复制17源。相同before/after状态的已编测试DLL可从首对应槽逐SHA复制，其未发生的测试编译日志可省略（首编成功准确27叶、复用成功准确25叶）；准确叶规则须据命令是否实际编译判断。所有失败保存，不覆盖槽。

每编译及独立序列进程60秒上限；只可超时停止本新诊断进程组并保留124，不操作Unity。前槽真实功能/依赖错误停后继；不得擅增依赖、改保护逻辑或扩展源列表。每槽前后ps成功且无Unity、源码除唯一准许helper外精确相等、D7实际106DLL/PDB、D1–D7全部历史根文件/槽/备份、原CONT5392/25226和Host116/762全部I/O、运行时/当前工具及此前D8槽保全。D7全槽/报告也须冻结；不得以旧保护helper遗漏新阶段。

正式交付逐项报告量化门、前后冷/暖及两顺序、原断言/具名XML/witness、计数/分段、源差分及各身份；性能改善必须使用同编译参数的before/after，不混D7 Unity绝对值。六槽18次原用例均通过、业务计数/断言保持，暖态减少重复Create且首冷仍真实完整准入。C正式completed后原R审实际唯一helper差分和原始证据，再决定必要全量；本D8不授权Unity或新的无过滤槽，不凭局部收益宣称600秒达成。

### 后续完整验证工具的独立准备（不执行）

SD00可在等待期间仅CreateNew `run_full_fixture_validation.py`（≤430行），沿用已核D7包装器和4181无过滤规则，使用明确未绑定源码/新期望常量，在任何ps或进程操作前拒绝运行。计划未来唯一full02槽，仍同时计入Compile和测试、完整20叶及300/600秒实际门，不以本准备签发Unity。未来expected-fixture-full-tests.json及full-fixture-preparation.json暂不创建；只在D8实际完成/源固定且原R审通过后据保留的4181期望及原始源身份绑定。

工具将旧D7 full01/报告/期望/准备及已完成D8六槽、工具/报告/唯一备份纳入原证据保护；D8执行保护仅签定D1–D7和其自身此前槽，不把SD00仍准备中的D9文件当产品变化或读写它们。未来I/O仍保持总6144/40000和256/8192上限；依据D7实际新增557/2592，拟把单槽CONT预留和新增门收窄为600/4096（当前5392/25226仍有余量），Host维持30/512，不扩总量、不删旧文件。准确源码、期望、工具SHA和只读preflight仍须未来另签；若D8测量不支持值得做此全量，保留未绑定工具而不执行。

## D7独立结论与D8准确条件执行签发

原R实际D7回合`01a0f0c0-2942-7801-a11b-5e6d9d3f2599`于1790746022正式completed，final `msg_0d29bf8d726d8dc0016abc9d93603487d085bbeb84a106eff2`，唯一verdict **NEEDS_FIX**（适用用户≤600秒全量目标）。独立123检查全过，4181具名/源/106实物DLL/旧证据/I/O/两个PID及直接子进程退出均核实，D7执行证据无新增缺口；性能仍超248.534552秒，不能接收为目标完成。

R仅要求D8补齐准确槽目录。现补签：六个CreateNew写白名单目录统一为`TestArtifacts/FMTestPerformance/2026-09-30/fixture-reuse-runs/`下`before-forward01/`、`before-trace01/`、`before-reverse01/`、`after-forward01/`、`after-trace01/`、`after-reverse01/`；完整绝对根为`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FMTestPerformance/2026-09-30/fixture-reuse-runs/`。每槽前后将此前已完成槽纳入保全。原始NUnit XML沿stdout/result内嵌，不额外新建XML叶；首编精确27叶、同状态复用精确25叶。

SD00据用户持续授权及R合同核对，现正式把上节D8完整草案（仅本节目录澄清）签发给原C。C先保护/备份原helper并完成三个before槽；真实Create三次且累计≥20%门通过后，才准修改唯一helper≤50行并执行三个after槽。门失败则不改源，保留结果并正式交回；依赖/正确性失败停止，不自行修工具、扩列表或改名重试。六槽和正式交付后C completed停写，原R实际审查后才决定下一全量。原4181覆盖、全部历史证据及D7性能RED原记录冻结。

SD00已准备D9未绑定工具345行/24388bytes/SHA`66e285d1661088cb8efb018af6c0590e0a5952557450c8e6cbb862329936f247`，当前不得执行。D8 C不修改/执行/纳入其自身动态保护此准备文件或未来新期望；D1–D7及自身槽的保护仍完整。此次D8仍不运行Unity、不改产品、meta、其它测试/配置、APK/BlueStacks或Git。

D9静态补审：原R回合`01a0f0c8-93fb-7f50-aa7a-05603be5e050`于1790746145 completed，final `msg_0d29bf8d726d8dc0016abc9e1a8eb887d0b9e73b6144c2b989`，准确345行/24388bytes/SHA相符、未发现必须纠正项。确认UNBOUND拒绝在ps前、full02尚不存在、无筛选/20叶/300与600门和全部保全沿用；该root准备文件不属C越界。仅源码和AST检查，没有执行工具，也没有给未来Unity许可。D8已派原C新回合`01a0f0cb-08ae-7682-b782-7388a3672569`（起1790746233）实施当前条件包。

## D8正式收件（待实际独立审查）

C回合`01a0f0cb-08ae-7682-b782-7388a3672569`于1790747739正式completed，final `msg_0d179092dafe07fa016abca4551c7887d0833ecf717f087e32`，安全停写。`fixture-reuse-delivery.md`73行/9748bytes/SHA`9ffd9ed6d301de468f9a6c4cbae4df475a91edfa918739bf306d134740c84dd4`；scope127246bytes/SHA`002d8a42e0d8ced6b19186f022cc2c4d57eda6d585805d58232a684317de1d82`。runner338行/26431bytes/SHA`53c0bfaa930103fc9b3170ec7f306108a6b879df4120de8f19260a6a0f3a4ec6`，driver85行/5123bytes/SHA`6e413d7a31cd134da6170696ba34e7ab2aba9143da706c150136f9cb2b48de7c`。

唯一helper +16/-5共21行，新23625bytes/SHA`1ee623417a1a488c6e8b0f0789868baae8115540c33c9a754615fc61f8a1d05f`；全源新指纹`d28ac0623c6eddb3374dcecf32fb9aed8f8b26f0d311ec5d9c9477c45294ce90`。每次Content断言与new Catalog保留，只有成功的不可变Storage结果由同锁发布。14诊断进程自然退出，原D7实际106DLL/PDB、D1–D7证据及全部CONT5392/25226、Host116/762未变，无Unity运行。

SD00读取整个工具/driver和21行实差，独立核六槽每叶身份、27/25准确范围、18次原执行及1952断言（前后各976）、14进程退出；原trace独立1142/1134对均无错配或未闭合。before三Create0.97823/0.25804/0.25216秒，总1.48843占4.8560334秒30.65115%，门通过；after保留首次0.97236秒而后两次省去。正序4.9156503→4.4208368（减少10.0661%）；反序4.2222094→3.7786855（减少10.5045%）。这包含首次JIT/cold；首cold没有改善，不能按总Create占比外推全量收益。

RealCatalog/RealRig各3不变，Create/Resolve/Build/Evidence各3→1；Roots76、Decode/Snapshot464、Submit38、Restore6均保持。六槽18检查及C最后122检查全过；本轮4181无变化，下一全量尚未运行，≤600秒仍未验证。原R只审D8实际差分/证据，正式ACCEPT后才可绑定D9期望/源码并评估一次必要全量；如仍>600依原目标继续，不把局部10%视为完成。

## D8独立接收与D9身份绑定（只读准备）

原R回合`01a0f0e3-4bdb-7fc0-ad25-0a9a4aca0f09`于1790748343正式completed，final `msg_0d29bf8d726d8dc0016abca6aa28ec87d097703fc6caa5949e`，唯一verdict **ACCEPT（仅D8）**。实际21行、同锁/成功发布/每次原断言及new Catalog、六槽18原执行/前后各976断言、1142/1134 trace、同17源/编译/实际加载、27/25叶、全部旧证据/I/O/106DLL及14PID/子进程退出均独立复核。D9未见新增实质缺口，但全量及600秒仍待实测。

现在按既定准备范围由SD00 CreateNew `expected-fixture-full-tests.json`，沿用D7在运行前冻结的原4106＋D3 20＋D6 55具名期望，额外记录D7已完成XML/结果与D8唯一helper变化来源；独立核原测试源码身份及当前新源d28ac…ce90。未读未来full02结果，计数仍4181。可将D9未绑定工具仅改说明及SOURCE/EXPECTED常量，精确绑定后做只读preflight；其余已审代码不变。CreateNew `full-fixture-preparation.json`保存准备结果及如有的初次权限错误，不启动Unity、不建full02。实际执行仍需后续准确命令/工具SHA/期望及准备SHA正式签发。

## D9准确无过滤全量签发（FULL_VALIDATION_AUTHORIZED）

原R实际D8 ACCEPT已满足。SD00核全源仅D8 helper变更，原测试源码和D7全量4181 Counter与旧独立期望精确相符；新期望39095bytes/SHA`64a37b744e18b3347cbc977311975e4215574cf480f3595be45893b56f3b77d6`。未读取未来full02结果；execution_authorized=false仅描述该期望创建时点，实际授权以本节为准。

D9工具只改了已审未绑定版的说明和两个身份常量；当前345行/24450bytes/SHA`8b87064917fca809beff02e353feacfc654622fa207e2d7e706b11953785aa0c`。只读preflight初次因沙箱禁止/bin/ps在建槽/Unity前失败；同一--preflight必要只读权限重试exit0，全部源、D7实际106DLL/PDB、D1–D8历史证据、Unity身份和I/O余量通过，wrapper2.227273791秒，SceneTemplate不存在。两次原始记录保留在`full-fixture-preparation.json`8980bytes/SHA`a1f71aeec00f09f79cc0f84391c61f9eae089e7092b8a78a8e03eb7b006f8268`。没有启动Unity或创建full02；此只读权限通过不等于全量运行通过。

现在仅授权原C核上述三文件和D8交付身份后，执行一次准确命令：

```text
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_full_fixture_validation.py --mode full --source-fingerprint d28ac0623c6eddb3374dcecf32fb9aed8f8b26f0d311ec5d9c9477c45294ce90
```

1. 固定源d28ac…ce90及当前D7实际106DLL指纹8f5dbb…43ca；入口fresh核全部源/配置/资源、4冻结文件、运行时、D1–D8证据、ps和I/O。发现不符保留并停，C不改工具、期望、PLAN、已接受源、测试/meta、依赖或任何旧证据。
2. 只CreateNew `unity-runs/full02/`，与D7相同准确20叶：run.json、before.json、after.json、io-before.json、io-after.json、host-io-before.json、host-io-after.json、protection-before.json、protection-after.json、compiled.json、natural-cleanup.json、named-tests.json、result.json、tests.xml、compile.log/stdout.txt/stderr.txt、unity.log/stdout.txt/stderr.txt。所有旧槽及D8六槽完全保全；成功必须20叶齐备、普通非链接、大小原限额内。
3. 由同一已核Mac Unity2022.3.18f1串行Compile→无任何testFilter的全部EditMode，Compile带-quit、测试不带。4181具名多重集合须精确原4106＋75，全部Passed且零失败/跳过/不确定/额外。D8 Mono结果不能代替本次Unity。
4. 墙钟包含入口、必要Compile、测试启动/执行/退出和最终范围检查；最终result序列化及工具返回后计如实披露。300秒目标、600秒上限保持；正确但超600返回3而非完成，绝不强杀Unity或删减用例。普通错误返回2并保留，不能自行增槽重试。
5. 编译产生的新106DLL/PDB自然绑定并与测试后实物相等，禁止手改缓存。SceneTemplate仅沿原D7精确3534bytes/SHA条件和自然退出/先保全规则处理，未知变化保留并停。主029、APK、BlueStacks、设备、UI迁移和Git写操作不在范围。
6. 原CONT5392GUID/25226叶、Host116/762全部保全；本槽CONT新增≤600GUID/4096叶、Host≤30/512，总上限仍6144/40000和256/8192。D7实际557/2592提供预留依据，本次收窄单槽限额而未扩总量，不清理任何旧I/O。
7. C只另CreateNew `unity-fixture-full-delivery.md`（≤90行），报告真实总数、完整/XML/编译/进程时间、300/600门、D7→D9及9/29→D9同批原用例和六代表/主要慢类、新增75单列、源/DLL/旧证据/I/O及安全收尾。不同计时边界分开，不以C8正序10%推断全量。无需新scope/分析工具或复写旧报告。
8. 自然结束且安全核验后C正式completed停写交回，再由原R实际审证据。≤600并独立接受后才可宣称上限达成；若仍超时则用新分布继续最小诊断/修复，不以已有收益结束用户目标。本签发沿用户既有持续授权，无须重问相同计划批准。

## D10会话编译优化候选（只读准备，未签发Unity）

D9原始完整结果已出现：4181全部Passed，墙钟683.685038375秒，XML643.1212897秒；编译17.327049秒，测试进程658.32066325秒，23项检查全过，仍超600秒83.685038375秒。C仍在正式交付和安全收尾；此处不是提前判C完成或替代R。D7到D9完整墙钟少164.849513584秒；新方案不凭此差值预测收益。

SD00只读核D9两个Unity日志确实使用debugger-agent及活动`Library/Bee/200b0aEDbg.dag`。该活动目录53份编译rsp与53个实际ScriptAssemblies DLL一一对应，全部`/optimize-`、`/debug:portable`，全部定义DEBUG、TRACE、UNITY_ASSERTIONS、UNITY_INCLUDE_TESTS。只读查全部产品及测试条件编译，不以旧缓存目录名推定实际模式。

[Unity 2022.3命令行官方文档](https://docs.unity3d.com/2022.3/Documentation/Manual/EditorCommandLineArguments.html)明确`-releaseCodeOptimization`仅覆盖本次会话；[调试官方文档](https://docs.unity3d.com/2022.3/Documentation/Manual/ManagedCodeDebugging.html)说明Release优化更快、不能附加C#调试器。本候选是保持同源/同覆盖的自动测试命令优化，尚未运行或证实任何收益；不改变开发者默认偏好、不改产品或永久项目配置。

1. 先等原C正式D9交回及原R实际审查。原R已获只读候选准备，不启动Unity、不改任何文件。SD00可仅准备CreateNew `run_release_full_validation.py`（≤520行）的未绑定版本；任何调用必须在ps及建槽前因未绑定身份拒绝。未来准确签发仍待工具、模式保留规则和实物身份审查。
2. 拟沿D9完整包装器，只在Compile与完整EditMode两条Unity命令分别加一个`-releaseCodeOptimization`。仍使用相同Unity2022.3.18f1 Intel、目标osxuniversal、同源d28ac…ce90和原4181完整具名多重集合。零过滤、零失败/跳过、不变原断言；Compile及所有检查和启动均计时；超时不杀Unity。
3. 拟新增独立冻结`expected-release-full-tests.json`（≤32MiB）、`release-mode-baseline.json`（≤32MiB）、`release-full-preparation.json`（≤32MiB）。4181期望沿旧独立期望、不读取未来结果倒推；mode基线保留当前活动53rsp原参数及定义、源、引用和NUnit身份。当前实际DLL须绑定D9 full02 compiled，不再用D7实物。
4. 模式检查须由本次实际compile.log及unity.log提取活动Bee DAG，不猜目录名、不写任何生成rsp。拟保留入口、编译后、测试后原rsp参数，比较各程序集定义/源/引用；只允许优化选项和活动Bee输出路径变化。DEBUG、TRACE、UNITY_ASSERTIONS、UNITY_INCLUDE_TESTS及NUnit实现身份均须保留。若缺少可核证模式、覆盖削弱或其它编译参数差异则停止并交回，不擅自放宽规则。
5. 拟唯一CreateNew `unity-runs/full03/`，D9准确20叶另加`compilation-before.json`、`compilation-compiled.json`、`compilation-after.json`，共23叶；各json/xml≤32MiB，日志≤128MiB。C仅另CreateNew `unity-release-full-delivery.md`≤100行。SD00工具/期望/模式基线/只读准备由SD00准备，C不改工具、源或PLAN。
6. D9后实测CONT5949GUID/27818叶，Host139/912，旧总6144GUID容不下一次已知约557GUID的完整运行。仅本候选拟将CONT总GUID上限明确补签为6656，叶总上限仍40000；本槽新增仍≤600GUID/4096叶，Host≤30/512且总256/8192不变。所有旧I/O原字节及D1–D9全部工具/报告/槽/备份保留，不清理、不无限扩容、不因此自动授权更多全量。
7. 首次Release编译成本必须完整计入，本次模式在进程退出后结束；开发者默认Debug偏好保持。自然更新的Library DLL记录身份，不手工回写旧DLL或改EditorPrefs。600与300门仍按全部步骤实际墙钟判断。正式C交回且R实际ACCEPT前不宣称目标达成；若仍超600继续依据证据签最小下一包。

本节仅使候选和准备范围具体可审，不授权C立即运行。源、Packages、ProjectSettings、UserSettings、EditorPrefs、资源、meta、主029、APK/设备、Git仍全部禁止写；SceneTemplate仅未来沿已有准确自然生成处理合同，不增加设置许可。

## D9正式收件与D10静态合同补充

C原回合`01a0f0ef-f284-7071-aebd-2cb71238525e`于1790749850正式completed，final `msg_0d179092dafe07fa016abcac95bfac87d0a406e77c922fe7a5`，安全停写。`unity-fixture-full-delivery.md`84行/8794bytes/SHA`a8d535d67d78553db071c29d0309eb571bf4b01f15b1e1a843fddb4e87d699aa`。full02 result7592bytes/SHA`36bd9d51d9e7ef206eff00cd29a9818f42a3dd87dd9fae7f8f4691e16a57f3c7`；XML3102525bytes/SHA`80b241184aeaa2a702aba68f4b45ec3f26dd4fd2cd4ee08db49273c53ee4e601`；compiled28105bytes/SHA`7b52d96f0076ff9953417edc1eb89d57f01340c3f5b49f07cead0bc35f16309e`，实际106DLL/PDB新指纹`88cf78396f55707a1e4df9e534b6e128ad8a96c4f354efdac3384c1ce0b928cb`。

SD00已读报告和完整结果，核4181准确Counter与D7完全相等、23检查和20叶、全部Passed；墙钟683.685038375秒。C另40项收尾全过，两个Unity PID50264/50299自然exit0且子进程退出；源d28ac…ce90、旧证据和I/O保全，SceneTemplate不存在且events=[]。原4106 case总和528.691280秒，新增75另104.300497秒，4181总和632.991777秒。保留同名多重出现，不以fullname字典覆盖重复项计算总时长。仍待原R实际审D9，性能不能接收为目标完成。

R候选只读回合`01a0f0fc-17d4-7210-8d23-8a46701246b7`于1790749795 completed，final `msg_0d29bf8d726d8dc0016abcac4a278487d08c859ee69444ce7b`。未见候选硬性语义冲突，但指出官方快慢说明针对PlayMode，EditMode须实测；`/debug:portable`可与优化并存；文档不能替代偏好前后证据；所有条件符号及编译源/外部引用身份都须保存。此回合未审D9、未授权执行。

D10候选据此收窄为完整编译输入比对：当前全部53份rsp（21项目及其余包程序集）共2433140bytes；原参数逐项保存。外部引用353个约48.8MB、全部源3767个约21.6MB，只读逐SHA封存；编译器analyzer及additionalfile也绑定，活动DAG中的项目ref DLL仅允许自然重编并另由实际DLL指纹绑定。完整参数仅归一化准确活动DAG前缀及`/optimize±`一项，其余顺序、定义、源、引用和选项全部相同，否则停止。原NUnit文件身份保持。

EditorPrefs仅只读固定`/Users/elliotyip/Library/Preferences/com.unity3d.UnityEditor5.x.plist`，保留文件身份及排序规范化plist的语义SHA；不记录原偏好值、不写偏好、不涉及其它plist或凭据。入口、编译后及测试后语义SHA必须相等；序列化字节如有重序须报告，不能用文档推定状态未变。任一实质差异停止并保留证据，不自动恢复或放宽合同。

SD00当前未绑定工具`run_release_full_validation.py`424行/30706bytes/SHA`8f86ed3fd33109b1093459fe209e966eebb392a651c1bdbf32d5717c88811c5b`。所有身份常量仍UNBOUND，入口在ps前拒绝；full03、期望、模式基线和准备文件尚未创建。原R现可实际审D9，并静态核本候选与准确未绑定工具；完成且无必要纠正后，SD00才绑定独立期望/模式基线、做只读preflight并另签唯一Unity命令。

## D9独立结论与D10工具最小纠正

原R实际回合`01a0f106-2372-7920-8911-5cf7a0644372`于1790750487 completed，final `msg_0d29bf8d726d8dc0016abcaf053a8487d0bbce4565e1939a52`，唯一verdict **NEEDS_FIX（D9全量600秒目标）**。完整4181多重集合、683.685038秒、源/106实物DLL、历史证据/I/O/20叶、两个PID及子进程安全退出均独立核实，未见其它D9执行缺口。D9资料全部冻结，性能RED保持。

R要求D10唯一最小纠正为：活动DAG与ScriptAssemblies的106项DLL/PDB必须一一字节关联，编译后通过才运行测试，测试后保持编译后实物并复核。SD00仅修改尚未绑定的新工具，将普通非链接身份、准确集合、bytes及SHA相等结果加入现有三份compilation证据，未新增叶文件、未试跑、未改配置或缓存。已于当前D9实际Debug产物只读证实106/106相等。其余D10静态合同R未见必要修改，6656总GUID、四符号/完整参数/输入/NUnit/偏好保全、23叶与UNBOUND前置拒绝均保留。

纠正后工具433行/31576bytes/SHA`c7bb18c1064b5e04d98b00e26dfc634fcbf8b17125a0837ee11c8a481e935ecf`，仍UNBOUND；AST通过。原R仅补审该最小纠正，不重复D9全审、不运行工具或Unity。静态纠正接受后SD00才CreateNew所列期望/模式基线/准备文件，绑定三个身份常量，做只读preflight，然后以准确命令和SHA签发C；完整源和4181覆盖维持D9同一状态。

## D10静态纠正接受与只读身份准备

原R回合`01a0f10d-96af-7071-ab8f-279dc811cf5e`于1790750665 completed，final `msg_0d29bf8d726d8dc0016abcafc224fc87d08ee032b949f8cb9d`，唯一verdict **ACCEPT（仅D10工具静态纠正）**。准确433行/31576bytes/SHA核实；R内存逆向还原原424行准确SHA，确认除产物关联外旧门不变。106同集/普通非链接/逐bytes与SHA、编译后先记录再拦测试、测试后保持均通过静态审查。未启动任何测试。

SD00现在按已审合同CreateNew冻结期望和模式基线；可将已审工具作为非main模块加载，仅调用不启动进程的只读编译快照帮助函数，以原D9 unity.log为输入保留实际Debug编译来源、所有输入、偏好语义SHA及106对应实物。不调用launch/main、不执行Unity、不建full03。独立4181期望由旧运行前冻结清单沿用，再核D9准确Counter和源身份；不读取未来Release结果。随后只改说明及三个绑定常量，运行准确`--mode full --preflight`只读核进程/源/配额/历史/实物，首次权限错误与必要同命令只读重试都保留到新准备JSON。此步骤不是Unity执行授权，后续另签唯一命令。

## D10准确Release无过滤全量签发

静态纠正已由原R ACCEPT。SD00完成独立期望、当前D9模式/实物及新工具绑定；内存逆向还原准确c7bb18…5ecf，证明只改说明和三个身份常量。只读preflight初次因沙箱禁止/bin/ps失败于任何建槽/Unity之前；同一--preflight在必要只读进程权限下exit0，wrapper5.565781792秒、preflight5.544655292秒。源d28ac…ce90、106D9实物、全部历史、53rsp/4175输入/106对应产物、EditorPrefs语义SHA、I/O余量和Unity身份均通过，没有启动Unity或创建full03。

准确冻结文件：

| 文件（均在本2026-09-30目录） | bytes | SHA-256 |
|---|---:|---|
| run_release_full_validation.py（433行） | 31756 | `0ff474abdf5d9e6a3db7fdb5e71a62fc69bd45c7779e9797215b9ad263f060f9` |
| expected-release-full-tests.json | 40857 | `baa292149cbd259c6d096718b28e14158a2abe05c0810544d106c7db8d91162b` |
| release-mode-baseline.json | 3825130 | `c151b99c42213349fb5bdb11882ebada656a0f95004462dbd5fe0b9e5d2fcfb9` |
| release-full-preparation.json | 9919 | `a75a7f2e295336b2bd5009086c14c26f74df3ec24fa61b28fec067dcbfac5cb2` |

期望与准备中的execution_authorized=false只描述其创建时点；实际许可来自本节。SD00现按用户既有性能专项授权，**只授权原C一次**准确命令：

```text
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_release_full_validation.py --mode full --source-fingerprint d28ac0623c6eddb3374dcecf32fb9aed8f8b26f0d311ec5d9c9477c45294ce90
```

1. 固定原同源d28ac…ce90，D9实际106DLL/PDB指纹88cf7839…928cb和上述四签发文件；开始时fresh核所有身份/进程/编译输入/偏好/I/O。只用现有2022.3.18f1 Intel编辑器、osxuniversal测试目标；不安装/升级2022.3.62f3、不改安卓交付方向。
2. Compile和全部EditMode两个启动各仅增加一个`-releaseCodeOptimization`。Compile带-quit，tests无-quit、无筛选；所有4181原具名出现/参数实例/断言保留。唯一变量为本会话编译优化模式，不改源码、测试、配置、包、资源、meta、EditorPrefs、预算或存档。先前D9 Debug结果完整保留。
3. 编译完成后，原始53rsp完整参数除准确活动DAG前缀和optimize±外须相同；4175编译输入、NUnit、四种定义及偏好语义SHA须保持，全部53 optimize+。活动DAG与ScriptAssemblies准确106DLL/PDB身份逐bytes/SHA相等，普通非链接。先保存编译证据再核门，失败不启动测试；测试后重查模式/输入及同一编译产物。
4. 只CreateNew `unity-runs/full03/`，准确23叶：run.json、before.json、after.json、io-before.json、io-after.json、host-io-before.json、host-io-after.json、protection-before.json、protection-after.json、compiled.json、natural-cleanup.json、named-tests.json、result.json、tests.xml、compile.log、compile.stdout.txt、compile.stderr.txt、unity.log、unity.stdout.txt、unity.stderr.txt、compilation-before.json、compilation-compiled.json、compilation-after.json。各json/xml≤32MiB，各日志≤128MiB；历史槽/D1–D9全部工具/报告/备份均冻结。
5. C只另CreateNew `unity-release-full-delivery.md`≤100行，不改当前工具、四身份文件、PLAN或任何旧证据。报告准确4181 Counter/各Passed、完整/XML/compile/tests分项和300/600门、D9→D10同4181及9/29→D10原4106多重出现总和、新75单列与慢类/六实例。不得用fullname字典丢重复项，不把XML asserts=0解释为零断言。
6. 完整墙钟包括入口检查、第一次Release必要重编、启动/执行/自然退出及所有模式/保护检查，结束点仍在result最终序列化及工具返回之前。≤300目标、≤600硬门不变；正确但超600仍exit3且未完成。绝不为时间强杀Unity，失败保留槽并正式交回，不能自行改工具/放宽门/另增试跑槽。
7. 明确授权本唯一全量的CONT总GUID上限**6656**，叶总40000；当前5949/27818，新增仍≤600/4096。Host当前139/912，新增≤30/512、总256/8192不变。全部旧I/O字节保全，不删除清理旧目录；本补签不授权下一全量或无限扩容。
8. 仅沿已审SceneTemplate准确3534bytes/SHA及先保全/自然退出规则；未知设置变化保留并失败，不手改或恢复缓存。模式仅本次进程生效，退出后不写开发者默认偏好；自然生成的新Library DLL保留实际身份，不手工覆回Debug缓存。
9. 主029、APK、BlueStacks、设备、UI/uGUI/本地化/配置选型实施和所有Git写操作仍不在范围。自然结束安全核验、C准确正式completed停写之后才交原R实际独立审查。只有实际≤600且R ACCEPT才能确认硬上限；5分钟是否达到由真实≤300决定，不依据官方泛化说明外推。

本轮用户再次询问“之前不是五十分钟么，现在变成十一分钟了？”，SD00已说明54分27秒→14分09秒→11分24秒的实测阶段和旧轮不含独立编译、新轮包含的边界差别；该询问不取消持续优化目标。按现有授权继续，无需重复请求同一计划批准。

原C已派发D10回合`01a0f11e-9165-7532-8eec-4dd48a70bb54`（起1790751707），执行上述唯一合同；本条只记录在途身份，不预判编译/全量/300或600秒门。新会话规则增加的引擎、双语言与配置选型约束已读，本轮继续同2022.3.18f1 Intel，不扩展实施。

## D10正式收件：4181通过、402.589秒、偏好保全失败

原C回合`01a0f11e-9165-7532-8eec-4dd48a70bb54`于1790752568正式completed，final `msg_0d179092dafe07fa016abcb7332d1487d0b257fbd372478226`，自然退出并停写。`unity-release-full-delivery.md`96行/10910bytes/SHA`3d27165b8dc7fabd1d6449ca7a3affeb425d4d4ce5a0dba7fcff5eec49adbfb2`。result8810bytes/SHA`00c6ab99f69b7f3f98b98df9b207c73f7bd00a88d2dc3ba1de527da89d8cdba7`；XML3102507bytes/SHA`36392928daf73336e7b1b1d6206049a40a8f7cd3770f166d1f761a18324d1fe2`；compiled28422bytes/SHA`12ce10bcfeadab95febb3d198d69659b9d19f694623e2756993d207fcf82171c`。

真实4181/4181全部Passed，准确多重集合不变。墙钟402.589015125秒、首次Release Compile43.24935875秒、tests348.110293625秒、XML332.805538秒、case总和326.527266秒。原4106 case264.512711秒，新增75另62.014555秒。虽然实际耗时<600，包装器25/26检查且exit2，**不接收为目标完成**。原failure=null只表示未抛异常，不覆盖false门；300秒未达成。旧54分钟到当前6分43秒是实测进展，当前仍有验证缺口。

唯一false是`release_mode_and_assertions_verified`内全局EditorPrefs语义SHA比较。53rsp准确EDbg→E活动DAG、optimize+、四符号、4175源/外部依赖、NUnit、106对应实物全部保全；源仍d28ac…ce90，106实际新指纹`27254b7c6b70e38bb4bf615f3e27cd9d0f11d0608b2a3eed9cd2394ce3664581`。自然编译/测试PID58554/58833及直接子进程已退出。23叶齐全；D1–D9原证据/I/O保全，SceneTemplate未生成。CONT新6506GUID/30410叶，Host162/1062。

偏好原始事实不可覆盖：入口与编译后114093bytes/raw SHA988c10…c3d9、语义f636ee…7995；测试后快照为42bytes空字典/raw及语义SHA9261ec…13f1；后续只读观测自然又为114093bytes/763键、raw SHA200f08…d935、语义988ab0…6d6，仍不匹配原值。C补核07:12:04Z与SD0007:12:27Z相符。仅原两个模式相关键CodeOptimizationOnStartup/AllowAttachedDebuggingOfEditor未存在；当前ScriptDebugInfoEnabled为int 1。不能以文件大小相同或后续非空宣称偏好恢复，也没有证据宣称所有偏好永久丢失。

SD00只读扫描全部实际3767个编译cs，未发现EditorPrefs.DeleteAll或项目/测试EditorPrefs写调用；包中仅定点DeleteKey及VisualScripting配置重置方法中的PlayerPrefs.DeleteAll，未证明执行，不据此归因。原基线只留全文件/规范化语义SHA，未留逐键摘要，当前无法直接指出改了哪一键。所有偏好值未被备份/输出；没有手工偏好写入、恢复或清理。本轮失败保留，待R实际审查并形成足够小的观测/验证器纠正合同，不静默排除差异或覆盖exit2。

另只读看到D10两个Release日志仍含`Using monoOptions --debugger-agent`。官方2022.3另有`-disableManagedDebugger`会话参数；尚未测其性能影响，不能据此承诺5分钟，也未授权新的Unity命令。首要工作是厘清偏好检测差异。原600秒目标未正式完成，无进一步Unity槽许可。

## D10独立结论与D11单次Compile偏好观测签发

原R实际回合`01a0f12f-2f49-7023-8a71-ecf50b603423`于1790753362 completed，final `msg_0d29bf8d726d8dc0016abcba3d292887d0b498fb964a49a718`，唯一verdict **NEEDS_FIX（D10实际交付）**。准确4181、402.589015秒、源/4175输入/53rsp/106对应实物、历史/I/O、23叶及进程退出均独立成立；偏好门仍失败，不能追认原exit2通过。R额外20秒5次只读观测均为763键/语义988ab0…6d6，未知哪些键变化。SD00在07:25:03Z用`/usr/bin/defaults export com.unity3d.UnityEditor5.x -`仅捕获到内存计算摘要，API与磁盘当前字典一致、同763键/语义988ab0…6d6；未输出原值、未写设置。

据R要求，现在签发原C一个更小的**观测包D11**。这是原性能专项内定位验证缺口的受控实验，不是新全量或产品实施。C可编写下列唯一新观察器并执行准确一次既有Release Compile；原R在C正式交回后审实际工具与证据。本轮不改变任何产品或偏好，不自动豁免会话统计。

### 准确读写范围与执行

- C可CreateNew `TestArtifacts/FMTestPerformance/2026-09-30/run_preference_observation.py`≤360行、`preference-observation-delivery.md`≤90行；观察器可复用已冻结D10模块的**只读**身份、inventory、source_snapshot、dll_snapshot函数，不调用D10 main/preflight/launch或变更其全局输出目录，不修改旧模块。不复用D10 compilation_snapshot内两次读偏好的旧实现；模式证据改用已冻结full03/compilation-compiled.json中的53rsp/4175输入/106输出清单，fresh逐身份核实实际文件，编译后再从本次compile.log核活动DAG仍准确对应这些实物；原偏好字段只作为历史保留，不能充当本次新观测。
- 唯一CreateNew槽`TestArtifacts/FMTestPerformance/2026-09-30/preference-runs/compile01/`。准确13叶：run.json、before.json、after.json、observations.jsonl、result.json、compile.log、compile.stdout.txt、compile.stderr.txt、protection-before.json、protection-after.json、compilation-before.json、compilation-after.json、natural-cleanup.json。各json/jsonl≤32MiB、日志≤128MiB，不另建原值备份、scope或其它运行槽。原D10四签发文件、完整full03及交付报告、D1–D9所有历史和备份全部逐身份保护。
- 命令固定为`python3 TestArtifacts/FMTestPerformance/2026-09-30/run_preference_observation.py --source-fingerprint d28ac0623c6eddb3374dcecf32fb9aed8f8b26f0d311ec5d9c9477c45294ce90`。唯一Unity子命令逐参数沿D10 `run.json` 的Compile记录，仅把-logFile换到本新槽compile.log；仍-batchmode/-nographics/-releaseCodeOptimization、同项目与osxuniversal、-quit。不加-runTests、-executeMethod、-disableManagedDebugger或其它选项，不启动第二个Unity，不杀进程。
- 启动前盘点**全部Unity编辑器**，不是只过滤本项目；任一现存编辑器则保留现场并停止，不人为关闭。编译运行中及退出后定期盘点，出现额外编辑器须标明干扰，不归因或接收观测；不得操作其它进程。自身PID自然退出，核直接子进程/编辑器无残留后交回。

### 同字节逐键观测及固定窗口

1. 只读固定`/Users/elliotyip/Library/Preferences/com.unity3d.UnityEditor5.x.plist`。每次仅read_bytes一次，用这一份字节计算bytes/raw SHA、解析类型、排序规范化plist语义SHA、键数、**键名/类型/逐键SHA**；记录读取前后stat（inode/size/mtime_ns等）及读期间是否变化，不能把两次独立读拼成一个快照。逐键SHA对规范化`{key:value}`计算，保留类型差异。不得保存或打印偏好原值、原始plist、身份令牌或其它凭据。
2. 入口及固定结束点各可调用一次只读原生导出`/usr/bin/defaults export com.unity3d.UnityEditor5.x -`，stdout只进内存解析、产出相同摘要/逐键SHA，stderr只保留长度/摘要及必要无值错误原因；不落盘或输出导出原文，不调用defaults write/import/delete/synchronize，不写或恢复偏好。当前763键和语义988ab0…6d6作为签发观察起点；fresh不符则保留样本并停止，不能悄悄换基线。
3. 启动前以2秒间隔保留至少5个样本（≥8秒），须同一非空语义SHA、同一键集合、读取期间无变化，且入口原生导出与磁盘相等，才启动唯一Compile。运行期间继续约2秒采样；进程自然退出后从退出时刻起**固定再观察180秒**，不因先出现正常/相同样本提前结束，保留最终≥10秒的稳定性结果。
4. 每个空字典、解析/读取失败、stat变化、语义变化和逐键差异均写入append-only observations.jsonl，携带UTC/monotonic、阶段、进程状态；不能只取最终正常样本。≤256个样本、每条≤512KiB且整体≤32MiB；若过程异常太长或数据将超限，记录INCOMPLETE/观测停止原因、保留已得数据，仍等待本Unity自然退出，不强杀或再试。
5. 稳定性、模式输入、偏好语义/逐键变化是分立结论。发现差异后仅列键名、类型和前后SHA，不预设unity.editor_session_count/sessionid或其它键可忽略。一次Compile未复现也不能追认D10、证明全量退出无问题或宣布目标完成；下一修正/必要全量须据实际结果另签。

### 保全、验收与正式交付

源仍d28ac…ce90，当前Release实际106DLL/PDB指纹27254b7c…4581；53rsp均optimize+，四符号/NUnit/4175输入及准确106活动DAG对应全部绑定。Compile前后源/配置/资源、完整输入与原实际DLL/PDB须逐字节保持（本轮无任何源码或模式变化）；自然编译异常变化保留并失败，不手改缓存。Unity可自然访问Library/Temp；不手改任何生成缓存。SceneTemplate仅沿既有精确3534bytes/SHA、初始不存在、先base64保全至natural-cleanup.json及自然退出后删除的合同；未知变化保留并停。

CONT6506GUID/30410叶和Host162/1062必须精确原样、**零新增测试I/O**；原6656/40000与256/8192总上限不扩。本轮无全量，无新增case/断言、无测试filter，没有5/10分钟目标验收。成功是完整受控观测和保全证据，不是偏好差异已解释。报告逐项说明唯一argv/PID/退出、观测时段/样本总数/异常/末尾稳定性、每个改变/增加/删除键的类型和SHA、原生与磁盘对照、模式/输入/实物、源与历史/I/O、安全收尾。全部错误保留；观察器缺陷不得擅自覆盖槽、放宽门或新增重试，正式交回后SD00签更小纠正。

主029、APK/设备、UI/本地化/配置实施、引擎升级、外部安装及Git写均不在范围。C完成唯一观测后正式completed停写，原R实际独立审查；根因未测清前不授权任何偏好写入、恢复、永久配置变化或新全量。沿既有用户授权执行本诊断，不重复请求原计划批准。

原C已派发D11回合`01a0f13b-e068-7d91-a45d-209ca34942ac`（起1790753628）按上述合同实施观察器及唯一受控观测；当前未有正式结果，不预判差异来源或观测通过。

SD00另向原R派发只读候选核对：在D11 C准备期间，以D10实际日志/编译证据及Unity2022.3官方说明评估未来单独增加`-disableManagedDebugger`的语义保全条件及可核证边界。只返回短结论，不写文件、不运行Unity、不审尚未完成的D11、不授权新槽或Prefs写入/豁免。它仍是未测性能候选，不能据此承诺300秒；D11准确命令继续禁止该参数。原R完成后停写，待原C正式交回再实际审D11。

原R候选核对回合`01a0f148-1c02-7df3-84a1-325a64aebf2e`于1790754573 completed，final `msg_0d29bf8d726d8dc0016abcbf02680c87d09951cdaceb521d73`。结论为合理待测候选而无收益证据；官方仅保证关闭监听套接字，不能推出额外JIT优化或300秒。D10虽有debugger-agent/server=y，却也有Unable to listen，不能证明连接或开销量级；383份实际项目源码未见调试器状态依赖。未来如签发，除现有全部源/4181/53rsp/4175输入/四符号/NUnit/106产物门外，须新增两次准确argv和运行期间对应PID监听关闭的独立证据；仅退出后无端口或日志缺字符串不足以证明生效。此次仍未运行Unity、不修改文件，不能解释D10偏好差异或替代D11实际审查。

## D12最小测试退出观测（仅工具准备，尚未授权Unity）

D11原始受控观测已经结束，104次磁盘样本及两次原生导出无逐键变化，Compile后固定180秒完整；原C尚在正式交付，本段不提前接收D11或替代R。单次Compile未复现，下一合理变量是进入并退出Test Runner。SD00只准备CreateNew `run_test_preference_observation.py`≤440行，沿D11同字节逐键观察器；源身份和D11交付身份保持UNBOUND，在任何ps、建槽或Unity前必须拒绝调用。原C正式completed且原R实际D11及新工具静态核对后，才可绑定准确身份和另签执行。当前不创建运行槽，不运行工具或Unity。

拟定诊断仅运行现有原测试 `FightMatch.Core.Tests.ExactRationalTests.Arithmetic_AboveBinary64IntegerPrecision_PreservesUnitDifference` 一次。该原方法只涉及精确数学及原断言，无业务文件I/O；D10 XML准确一个Passed，原源码属于四冻结文件，方法和断言完全不改。这个单例只定位测试框架退出时的偏好现象，绝不替代4181无过滤全量或作5/10分钟性能验收。

未来拟用D10 `run.json` 的tests argv，只把-logFile和-testResults换到唯一新槽，并增加一个-testFilter及上述完整原名；同Release/编辑器/目标，无-quit，不加-disableManagedDebugger、-executeMethod、其它参数或额外Compile。源d28ac…ce90、当前106产物27254b…4581及53rsp/4175输入/四符号/NUnit保持；所有编辑器/直接子进程、SceneTemplate准确保全规则沿D11。

拟唯一槽 `preference-runs/test01/`，准确14叶：run.json、before.json、after.json、observations.jsonl、result.json、unity.log、unity.stdout.txt、unity.stderr.txt、tests.xml、protection-before.json、protection-after.json、compilation-before.json、compilation-after.json、natural-cleanup.json。JSON/JSONL/XML≤32MiB、日志≤128MiB；单字节缓冲逐键摘要、入口和末尾各一次原生只读导出、5次/至少8秒入口、运行中约2秒采样、退出后固定180秒、≤256条/512KiB单条及32MiB总量全部沿D11。不得输出偏好值或写/恢复偏好；仍没有任何键的自动豁免。初始763键/语义988ab0…6d6必须fresh相符。

新工具完整保护D1–D10及D11观察器、正式报告和compile01全部13叶。CONT6506/30410、Host162/1062全部精确保全，零新增测试I/O，原配额不扩。准确XML必须仅1个上述fullname、Passed、无失败/跳过/不确定/额外，并保留原XML；编译输入及实物不得改变。仅完成受控观测可算本诊断成功，偏好有变化与观测完整性分开报告，不能追认D10原失败。当前只授权SD00准备未绑定新诊断工具，不授权C执行、另建期望/准备文件或修改旧观察器/证据；其它禁止范围沿D11。

## D11正式收件与D12未绑定工具

原C回合`01a0f13b-e068-7d91-a45d-209ca34942ac`于1790755116 completed，final `msg_0d179092dafe07fa016abcc1271d9887d0865cd62d634b49b9`，正式停写交回。D11报告58行/8152bytes/SHA`ee14b3fbca57c7b7e79a685f09eb45c72ceaa3eee8ff3582fb63e02064e39dd7`；观察器348行/22913bytes/SHA`a417d6141687c31d39f3b9fc86c9ac3ae91f60a87ae981a2f43c8b3ebf4ae06a`。

唯一Compile PID64297自然exit0，14.561784秒；整诊断214.205645秒。入口5次8.020525秒，104磁盘＋2原生共106样本，全763键/语义988ab0…6d6，零键变化、零空/无效读取；退出后180.065486秒，末尾磁盘跨度180.004504秒。11运行检查全真、C147只读核验全过；准确13叶、源/53rsp/4175输入/106产物/历史/I/O保持，零新增I/O、SceneTemplate未出现、全部编辑器及子进程退出。观察器result6519bytes/SHA`788f9c8c8bfb5e696fc04f582556ee534a492c9c101e7d5072e5e65b73d84e44`；observations10672485bytes/SHA`a4680e1243a2ef1927c4679deafd8e85b71528e2cd603935d7d9ab5ce2f82931`。此为未复现的受控观测，不能排除2秒间隔内的更短变化，也不能接受D10或目标。

SD00已阅读D11完整观察器、result与交付，并准备D12新工具375行/25022bytes/SHA`51910031802f2a58f8cc166477dba5e54fc986882bef9f7c5fb229512d24e38d`。AST通过、未调用，test01不存在。与D11仅差准确单测试argv/输出槽/原XML检查、新D11历史保护、UNBOUND前置阻断和对应阶段标签；偏好同字节读取/逐键摘要/180秒/进程/全部保全逻辑沿原实现。源与D11交付SHA两个常量仍UNBOUND。原R现实际审D11，并静态审D12小差分与拟定合同；若通过才绑定已知源和正式报告身份，另签一次准确命令。C本时点不再运行Unity。

## D11独立接收与D12准确单例观测签发

原R实际回合`01a0f155-d926-7ef1-989a-57c52b99b82f`于1790755692 completed，final `msg_0d29bf8d726d8dc0016abcc3631ee487d0adfcb21d4e4989b5`，唯一verdict **ACCEPT（仅D11受控观测交付）**。104磁盘＋2原生、763逐键、完整窗口、准确13叶/源/53rsp/4175输入/106产物/历史/I/O及Unity和4个记录子进程退出均独立核实。没有复现，不能追认D10或目标。R静态D12无必要纠正，认为单个原数学测试是合理下一观测；其结论未自行授权执行。

SD00已只修改D12说明和SOURCE/D11_DELIVERY_SHA两个常量，内存逆向还原准确519100…38d，AST通过，没有执行或建test01。当前工具 `run_test_preference_observation.py` **375行/25128bytes/SHA`ad2f2f405210c9e66c2b76a3ab368af6d6a37ead4067620d0dcce1d60f13d456`**；源绑定d28ac…ce90，D11报告绑定ee14b3…9dd7。全部D11及D1–D10原文件冻结。

沿用户持续批准的专项计划，SD00现在只授权原C执行一次准确命令：

```text
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_test_preference_observation.py --source-fingerprint d28ac0623c6eddb3374dcecf32fb9aed8f8b26f0d311ec5d9c9477c45294ce90
```

本签发使上节D12拟定的单例、argv、14叶/大小、逐键观察、180秒窗口、零测试I/O新增及全部保全合同正式生效。仅CreateNew `preference-runs/test01/`上述14叶；C只另CreateNew `test-preference-observation-delivery.md`≤90行。唯一原例仍为Arithmetic_AboveBinary64IntegerPrecision_PreservesUnitDifference，不改源码/断言或任何原测试。当前源d28ac…ce90、106实物27254b…4581、763键/语义988ab0…6d6及CONT6506/30410、Host162/1062必须fresh核实相符才启动。

仅一个tests进程，原D10 tests argv仅改两输出路径并加准确-testFilter；无额外Compile、无-quit、无-disableManagedDebugger、无第二Unity或重试。程序返回仅代表本次受控单例观测是否完整；偏好键变化独立如实列名/类型/SHA，任何值不打印、不落盘、不写/恢复/自动豁免。XML必须精确1个原名Passed、零失败/跳过/不确定/额外；不得用此单例宣称4181或300/600秒已验收。

C核工具身份后执行并完成自然退出/安全收尾，再正式completed停写交回，原R实际审查。工具缺陷、任何范围/源/产物/I/O或输入起点不符均保留并停止，不自行修工具/放宽门/覆盖槽；后续仅SD00据实签更小纠正。主029、设备、UI、配置/包/引擎、偏好写入及全部Git写操作仍禁止；本签发不授权新的全量槽。无需重复请求已有专项计划与C/R协调批准。

原C D12在途回合`01a0f15d-8f16-7893-9938-8aa1ed3b43b7`（起1790755835）。本条仅记录派发身份，不提前判断单例或偏好观测结果；R须等待此准确回合正式completed及交付文件再实际审查。

### D12在途只读线索（不接收、不豁免）

C采样已报告仅字符串键`UnityConnectUrlConfiguration`变化，其余762键未变。SD00只读实际Unity2022.3.18f1二进制找到该精确键的UTF-8文字，项目/PackageCache的cs及UnityEditor托管DLL未找到；附近静态文字属于UnityConnect URL配置及creation/expires字段。原生符号查询没有进一步结果，不以邻近字符串单独证明写入调用链。

SD00只读当前该键并仅输出类型、摘要和结构：外层JSON准确`value`字符串、`creation`整数、`expires`整数，value自身为URL/服务配置JSON。没有输出或保存任何偏好原值、URL值或令牌；当前creation落在D12测试进程的已记录UTC区间内，expires大于creation。这提示缓存刷新时间，但还不能断言D10或D12只变了时间字段。

准许SD00做一次进一步的纯内存身份核对：仅在已有D10/D9 tests进程日志的有限UTC秒区间内，为当前已知这个缓存键替换creation及同寿命expires两个整数，其它字符串内容逐字不动，比较D12入口逐键SHA及D10旧规范化整字典SHA。仅输出候选数/匹配与否及字段保全结论，不输出候选时间值、不保存重建偏好、不写配置、不运行Unity；若不匹配即停止，不扩大字段或猜测其它键。本核对属于哈希身份验证，任何匹配仍须R独立复核；不能覆盖旧失败或直接授权豁免/新的全量。

SD00上述有限核对已完成：D10 tests区间350个整数秒候选中**唯一1个**准确匹配D12入口该键SHA`d84c8e65f7e22551cb1fefdc9f5e21309dbcfe39afc8be4e2d2e767f003e84b5`，重建整字典语义SHA也准确等于D11/D12入口`988ab02e…6d6`。D9 tests区间660个候选中**唯一1个**准确重建D10入口旧整字典语义SHA`f636ee575df7db545ee81102736e3d4909b65a1d1da8d7c7ba27eb2024ff7995`。只替换缓存的creation/expires整数，保持同一寿命、value原字符串逐字及其余762键不变；没有尝试其它键/字段，没有输出或保存候选值/偏好原文。现值该键SHA`202a9b8209a5198a1547cbd15aee70950f329b63eb4458084682d1ce911da2c3`。

这些匹配提供了可重复的身份依据：两个稳定历史状态均可仅由该缓存的两个时间字段变化解释，用户偏好及缓存正文未变；这是受限重建与既存SHA完全相等的证据，并非保存到了旧原值或直接观测到了D10全部写入过程。D10的42bytes空字典中间样本/exit2继续保留，尚未由R复核或接收。后续规则如要区分缓存时间，必须同时保留缓存正文、寿命、其它762键、非空稳定期及原生对照检查；不能整键无条件排除。

## D12正式收件（待R实际审查）

原C回合`01a0f15d-8f16-7893-9938-8aa1ed3b43b7`于1790756498 completed，final `msg_0d179092dafe07fa016abcc68e0b3487d09222c56a8bc95426`，正式停写交回。报告66行/9353bytes/SHA`8da59a2fa0a108fbced5bf03e0cd874113966c4f564a57e45ca829fb4c7a77fe`。原指定数学用例1/1Passed，XML3366bytes/SHA`1d80136f8e4f7296f2ab169961f481c21a37ec0141ba92c3e7304977ab629995`；PID67393自然exit0，22.399954秒。整诊断221.048914秒，不作全量性能计时。

110条观测（108磁盘＋2原生）、完整退出后180.072392秒；仅UnityConnectUrlConfiguration一次str→str SHA变化，首次新样本seq21，退出后6.014149秒，末个旧样本退出后4.011456秒。其它762键相同，零空/无效样本，末尾稳定173.989832秒，末尾原生与磁盘精确一致。当前整语义SHA`ce6fd4a5f69c370f032f876d89bd05f3785e9b559cd88059c9bb242ffd5c6462`。12项运行门及C151只读核验全过；源/模式/106产物/所有历史及CONT6506/30410、Host162/1062完全保全，准确14叶，无编辑器/子进程残留。

result9062bytes/SHA`f3c28d6dd4a7d9e12c140ffb52dcb92d66ad93bf1049b0d5790a0a936a4a7fb4`；observations11077876bytes/SHA`ce35c376a8d44ebec937e352c95390abb720aef580135ae393bb9eeeb67d2339`。C报告没有自行解释/豁免键，与SD00随后独立提出的纯内存两时间字段重建证据分开保留。现在可由原R一并实际审D12及复核该重建，只有R正式结论后才决定新的验证器最小纠正/必要全量；D10的原失败、402.589秒和全部原证据不改。

## D13稳定偏好检查与关闭监听候选（仅准备，未授权Unity）

原R已在当前回合独立复现两组有限重建，D12实际审查仍在进行。本节仅授权SD00 CreateNew两个未绑定诊断/验证工具：`preference_stability_guard.py`≤300行及`run_no_debugger_full_validation.py`≤720行。保持原4181及全部产品不变；入口UNBOUND在ps、建槽或任何Unity前阻断，准备时只允许AST/纯内存检查，不运行新工具或创建未来full04。必须等D12正式R结论及这两个新工具静态核对后，才创建/绑定准确期望、模式基线和只读准备记录，另签执行。

未来拟定唯一`unity-runs/full04/`：沿D10准确23叶，新增`preferences.jsonl`，共24叶。该逐样本JSONL≤64MiB、每条≤512KiB且≤512条，其余JSON/XML≤32MiB、日志≤128MiB。拟分别CreateNew `expected-no-debugger-full-tests.json`、`no-debugger-mode-baseline.json`、`no-debugger-full-preparation.json`（各≤32MiB），C唯一报告拟`unity-no-debugger-full-delivery.md`≤100行。当前不创建这些未来证据/报告或槽。

拟仍串行一次必要Compile及一次4181无过滤EditMode，全墙钟包括入口、编译、启动、执行、退出和全部稳定/模式/历史/I/O检查，不排除新增等待。仍同源d28ac…ce90、当前Release106实物27254b…4581、Unity2022.3.18f1 Intel/osxuniversal及53rsp/4175输入/四定义/NUnit。只在D10两条Release命令分别增加一个`-disableManagedDebugger`，不改任何源码/测试/断言/持久设置，性能收益未知；300秒目标和600秒硬上限不变，超时不杀Unity。

拟修正偏好门：单缓冲读/前后stat、完整763键类型/逐键SHA/整语义SHA、原生入口及末尾一致性均保留。只对`UnityConnectUrlConfiguration`精确三字段JSON的creation/expires整数作区分，拒绝重复JSON字段，要求value原字符串及其它序列化内容逐字保持、expires-creation寿命保持且为正、其它762键精确保全。若该键改变，新creation须属于本次实际Compile或tests进程UTC区间，采用预先固定的Unix整秒边界`floor(start) ≤ creation ≤ floor(end)`（运行中end取读取当时），不能把包装器准备/退出后等待当作进程授权区间；时间不倒退，包括刷新后又退回旧入口值也失败。不接受其它字段/类型/键集/值变化，不整键无条件忽略，既有两个历史状态的重建不替代新运行的前后实测。

拟入口至少5次/8秒稳定且符合已绑定初始状态；运行期间约2秒采样，保留全部空/异常/读竞争与变化。测试自然退出后至少观察20秒，并要求末尾连续至少10秒非空/同语义/读取一致，再作一次原生导出与磁盘完全比较；最长60秒仍不满足则保留失败，不无限等待或人为恢复。已观察到的6秒延迟提供20秒下限依据，新增等待计入完整墙钟。出现任何非空可解析但不获准的内容变化、额外Editor、完整采样缺口/越限或收尾不完整均失败；空字典、失败或读取不一致也单独失败，不能因末尾恢复自动豁免。末尾原生导出之后的模式/收尾采样仍须与它完整相符，否则取消稳定成功。完整观测、受限内容比较、是否出现无效读取和300/600秒分别报告。

关闭监听另立门：两次实际argv各只新增一次准确参数；准备使用`/usr/sbin/lsof -nP -a -p <本次Unity PID> -iTCP -sTCP:LISTEN -Fpcn`只读检查（-nP保持数字地址/端口，避免名称解析）。只在同一PID仍活跃且已初始化Mono时将采样用于证据，须至少3个有效样本、跨度≥4秒，且全部无TCP监听；保留原始进程号/命令/返回码/输出，不把进程退出后无端口当作生效。如存在未解释监听、检查权限失败或不足以证明则失败，不能仅凭日志缺字符串放宽。进程自然退出后，已记录的直接子进程允许最多30秒自然收尾等待，计入墙钟；额外Editor直接失败，不杀进程。该额外只读检查的准确触发/充分性须R静态确认，当前不执行Unity测试。

拟完整保护D1–D12全部工具/报告/备份/槽及既有I/O。当前CONT6506/30410，下一必要全量按既有实测新增557/2592，拟仅该槽预留≤600GUID/4096叶并把总GUID上限补签至7168，叶总仍40000；Host新增≤30/512且总256/8192不变。没有删除旧I/O，也未因本草案提前扩容或授权运行。新全量若任一正确性/保护门失败完整保留，不改D10原失败或自动重试。所有旧禁止范围沿D12；原C仍须等待后续准确签发。

## D12独立接收与D13未绑定静态交付

原R回合`01a0f169-e51a-7d23-b942-f185c45b4374`于1790757575 completed，final `msg_0d29bf8d726d8dc0016abccab40de487d086211a97838b8cba`，唯一verdict **ACCEPT（仅D12受控观测交付）**。1原例/110样本/180秒、唯一键变化、源/模式/全部实物/历史/I/O及7子进程退出均独立成立。两组350/660候选的唯一SHA重建也独立成立，限于稳定语义快照；不证明完整写入者/过程或旧plist原始布局，不解释D10空中间样本，不追认D10通过。

SD00依R具体边界补入D13：重复字段拒绝；固定整秒进程区间而非整个包装窗口；缓存更新后回退也失败；空/失败/读取竞争单列失败；末尾native后额外采样不能静默改成功状态。新helper239行，12项纯内存正反例检查全过，覆盖合法不变/有限刷新、正文/寿命/其它键/键集/额外或重复字段/类型/窗口/回退/空字典；没有读磁盘偏好、运行defaults/Unity或创建运行槽。新完整包装器保持所有四身份常量UNBOUND，只准备静态代码。R现在只静态审两新工具及草案，不运行工具/Unity；精确文件身份随派发给出，当前full04及未来期望/模式/准备文件均不存在。

## D13静态NEEDS_FIX与最小时序纠正

原R静态回合`01a0f17f-c4bc-7f72-afd3-2e37cc313dfa`于1790758413 completed，final `msg_0d29bf8d726d8dc0016abccdfcb39087d0bb9541194e4a96bf`，唯一verdict **NEEDS_FIX（仅D13静态）**。三项必要纠正：稳定/运行采样缺口未永久拦截；监听查询返回后PID已退出会过滤掉已捕获的反证；首次确认退出后偏好窗口仍未及时关闭。其余偏好内容/身份前置门/历史/计时未见必要纠正，未运行工具或Unity。

现只允许SD00修改上述两个尚未绑定的新工具，限三项：同阶段连续磁盘采样间隔最多3秒、阶段切换最多10秒（切换包含已测约5秒的静态I/O/编译清单检查，不能计作稳定窗口），任何超限永久记录并使观测不完整；保留所有监听查询的错误/非空/不正常返回作为反证，不因事后退出排除，只有“仍活”的有效空查询能贡献3样本/4秒正证；所有poll/wait首次确认退出时立即冻结UTC/monotonic并关闭唯一授权窗口，再允许任何后续偏好采样，后续查询不得延长该窗口。保持UNBOUND、原全部其它门和禁止范围不变。

必须用纯内存覆盖三个对应反例：相同摘要跨20秒再恢复也不能抹去采样缺口；三个正常存活样本之后，退出竞态中的监听/权限错误必须失败，而退出后的空样本不能增加正证；首次确认退出后再出现的缓存creation须被固定窗口拒绝。无需新增磁盘测试文件，不读取实际偏好、不调用ps/lsof/defaults/Unity。准确新身份及差分交原R补审后，才可继续绑定准备。

三项纠正已按上述范围完成，仍四常量UNBOUND。helper **248行/12949bytes/SHA`1248a7ef77fc3619c64966955d06616b11240efb4e0deed72ad23ef4492ce681`**；wrapper **544行/39187bytes/SHA`4af248b87193d44facf1afbd33fa0cee04bcc090462d7678cade9262b76b4c80`**。AST及10项针对性纯内存检查通过：20秒缺口永久保留；后续正常样本不清零；退出窗口只关闭一次且不能后延；窗外新creation被拒；3个存活空样本可作正证；退出竞态的监听与权限错误各自失败；死后空样本不能补足正证，也不抹掉此前有效样本。使用合成数据和模拟进程/时钟，未读实际偏好、未运行ps/lsof/defaults/Unity，未建新证据槽。现只交R补审三个纠正，未来full04/期望/基线/准备仍不存在。

## D13静态纠正接收与只读身份准备

原R补审回合`01a0f18b-0555-7381-b197-d1534204b990`于1790759013 completed，final `msg_0d29bf8d726d8dc0016abcd05af45087d0abeb91d827c7e82e`，唯一verdict **ACCEPT（仅D13三项纠正静态补审）**，无剩余必要纠正。独立AST通过，并在内存逆向撤销差分准确恢复上一轮两文件SHA，确认仅三项时序纠正；10个离线反例代码与原始通过输出已审。未运行新工具/Unity，未自行授权全量。

SD00现可按已审草案CreateNew三份准备文件：期望沿既有独立4181清单并核D10实际Counter，绝不从未来full04倒推；模式基线以当前实际Release输入/产物及D12已完成日志fresh只读捕获，763键/语义ce6fd4…6462初始状态必须相符。为此只在非main命名空间加载两新工具，允许只读identity/inventory/source_snapshot/dll_snapshot/compilation_snapshot及单缓冲read_disk，不调用launch/main或实例化写入观察器，不运行Unity/新槽。随后仅修改wrapper说明和四身份常量，逆向验证恢复已审4af248…c80。

绑定后允许准确`python3 TestArtifacts/FMTestPerformance/2026-09-30/run_no_debugger_full_validation.py --mode full --preflight`只读核对；如/bin/ps被沙箱禁止，保留错误并仅按同一只读命令申请必要权限重试，不改变命令或绕过控制。所有尝试输出保存在新`no-debugger-full-preparation.json`。准备本身不授权C启动Unity，也不建full04；完整签发仍须准确五份文件SHA、源和新配额。本轮600/300目标以及所有原失败状态维持。

## D13准确4181全量签发（full04唯一执行）

D13三项时序纠正已有原R正式静态ACCEPT；SD00仅绑定说明及四身份常量后，只读preflight通过。首次因沙箱拒绝/bin/ps在建槽及Unity之前退出，获必要权限后原样只读重试exit0，两个完整输出均保存在准备记录；没有启动Unity或创建full04。当前763键初始语义ce6fd4…6462、源d28ac…ce90、106 DLL/PDB 27254b…4581、53rsp/4175输入及四定义/NUnit均符合已审合同。期望沿独立4106＋20＋55具名出现，并与D10 XML Counter核实4181/4176个不同fullname；不读取未来结果。继承期望中的provenance描述D10原准备历史，D13用途和D10佐证另由intent/prior_release字段及准备记录明确。

现依据用户已批准专项及沿用原C/R的持续授权，签发原C **唯一一次** 以下命令，不需再请求同一计划许可：

```text
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_no_debugger_full_validation.py --mode full --source-fingerprint d28ac0623c6eddb3374dcecf32fb9aed8f8b26f0d311ec5d9c9477c45294ce90
```

签发五文件身份（均位于本目录，准备完成后冻结；各JSON≤32MiB）：

| 文件 | bytes | SHA256 |
| --- | ---: | --- |
| run_no_debugger_full_validation.py | 39407 | da6baee05effa4fac69db9aaa365bb147c5e29797211f2ef9951e402e37e0f45 |
| preference_stability_guard.py | 12949 | 1248a7ef77fc3619c64966955d06616b11240efb4e0deed72ad23ef4492ce681 |
| expected-no-debugger-full-tests.json | 42391 | 610c5989c90a14fda5e1a7456573e590ac9d7ecaef50c5eccfca9ce93a3f90cb |
| no-debugger-mode-baseline.json | 3990206 | dee19e80ce0d004741198c27bf97a04262b855a176d9e17e6eaeae9f678c4f1b |
| no-debugger-full-preparation.json | 11538 | 28fd53ab99e67f3dc43fec24f466512449daf236cbb02dc0957b23644b1a157f |

1. C先核上述身份与本次入口；调用中fresh preflight也须通过后才CreateNew full04。唯一槽为`unity-runs/full04/`，准确24叶：run.json、before.json、after.json、io-before.json、io-after.json、host-io-before.json、host-io-after.json、protection-before.json、protection-after.json、compiled.json、natural-cleanup.json、named-tests.json、result.json、tests.xml、compile.log、compile.stdout.txt、compile.stderr.txt、unity.log、unity.stdout.txt、unity.stderr.txt、compilation-before.json、compilation-compiled.json、compilation-after.json、preferences.jsonl。JSONL≤64MiB/512条/每条512KiB；其它JSON/XML≤32MiB、日志≤128MiB。
2. 串行一次Compile及一次无筛选4181 EditMode；两实际argv仅相对D10各新增一次-disableManagedDebugger并使用新输出路径。同Unity2022.3.18f1 Intel、osxuniversal、Release；Compile带-quit，tests不带-quit/filter。源码/测试/断言/配置/持久偏好完全不改；本次runtime选项不重写默认设置。实际106 DLL/PDB与入口须保持逐字节相同。
3. 入口5次≥8秒、全过程采样、退出后≥20秒且尾部≥10秒稳定、最多60秒和原生末次对照全部计入墙钟。缓存仅准确两个整数时间字段按已审实际进程窗口/不倒退/寿命与正文保全规则允许刷新；其它762键、所有类型和键集合保持。任何空/失败/读取竞争、采样间断、未授权内容变化分别失败，不以最后恢复豁免。D10原42bytes样本和exit2保持失败历史。
4. 两本次PID须在Mono初始化后仍存活期间各取得≥3个跨≥4秒有效无监听查询，所有查询反证均保留。首次确认进程退出立即冻结窗口；额外Editor或残留失败。只等自身进程及已记录子进程自然退出，全部时间计入，不杀Unity或操纵其它进程。
5. 本唯一全量CONT准确起点6506GUID/30410叶，新≤600/4096，**仅本次总GUID上限补签为7168**、总叶仍40000。Host162/1062，新≤30/512、总256/8192保持。所有旧I/O原字节保全，不清理、不授权下一全量或扩容。D1–D12工具/报告/备份/全部槽及本五文件冻结并由保护清单核验。
6. SceneTemplate只沿原精确3534bytes/SHA5baf593374ad67246277a435d8d0ded7167696eef6e05d2d245670e490625d8c、入口不存在、先保全base64至natural-cleanup.json且自然退出后处理的合同；其它未知设置变化保留并失败。不手改Library/Temp/Logs/UserSettings或任何旧证据。
7. C只另CreateNew `unity-no-debugger-full-delivery.md`≤100行，列准确命令/五身份/回合、4181 Counter、全部门、完整墙钟及分项、前次D9/D10和9/29的同集合耗时、原4106与新增75分别统计、缓存/采样/监听/模式/源/全部历史/I/O/自然退出证据及限制。不得用fullname字典丢掉5个重复出现；不把XML asserts=0解释为零断言。
8. 300秒目标和600秒硬上限不变；时钟从包装器最早入口至全部核验结束，止点在最终result序列化和工具返回前，如实披露。正确但超600仍未达目标；任一保护/正确性失败均不能用好看的时间宣称完成。失败完整保留并正式交回，不改工具、不覆槽、不自增full05或再试。
9. 主029、产品/UI/本地化/配置/包/引擎升级、APK/设备、安装及全部Git写操作均不在范围。原C准确回合正式completed且报告到齐后，原R才审实际交付；原R正式completed＋唯一ACCEPT之后才能接收本完整成绩。只读准备文件中的execution_authorized=false描述其创建时点；本节是后续唯一实际执行授权。

原C已派发D13回合`01a0f199-aa1b-75a2-8689-66597b08a57a`（起1790759774），按本节唯一合同执行；本条只记录在途身份，不预判full04/300或600秒门。没有创建新聊天或改变现有C/R模型设置。

### D13在途只读定位：全TCP与托管调试端口须区分

full04原始result已记录Compile PID71884自然exit0，但四次lsof均有5个TCP监听端点，因此旧「全部TCP为空」门拒绝，tests没有启动；43.551715秒是失败尝试，不是全量成绩。C仍在正式保全交付，当前不接收、不更改工具或失败记录、不授权再跑。

SD00只读现有官方PackageCache `com.unity.ide.visualstudio@2.0.22/Editor/VisualStudioIntegration.cs`，7617bytes/SHA`7d50c7b932585a2bd68b6d5d228bac7281ab2bf900802e4ac8b1914914b65c60`，第114–117行的DebuggingPort明确为`56000 + (Process.GetCurrentProcess().Id % 1000)`，MessagingPort另加2。D7/D9/D10六次实际Unity日志独立校准该规则：48046→56046、48100→56100、50264→56264、50299→56299、58554→56554、58833→56833，全部与原Using monoOptions中的address端口完全一致；是当前同一编辑器实物与现有包的证据，不仅靠跨版本网上示例。

D13 PID71884的预期托管端口为56884，四个存活样本实际均为55504、34999、38000、38443、53159，没有56884。compile.log明确把55504标作Player connection；未识别另外四个子系统，不臆测归属。D13已记录-disableManagedDebugger且没有原Using monoOptions --debugger-agent行，但缺日志文字本身不能单独作生效证明。官方2022.3命令行文档只承诺关闭debugger listen socket，没有承诺关闭所有Unity TCP服务（https://docs.unity3d.com/2022.3/Documentation/Manual/EditorCommandLineArguments.html）。

候选最小纠正应是按本Unity PID的准确托管端口识别，同时保留完整原始lsof输出、格式/权限/退出竞态反证及至少3个跨4秒存活样本；其它端口不能当作托管调试器，也不据本定位追认D13成功。待C正式completed及原R实际独立判定后再签新工具；当前没有新Unity授权，五冻结文件/full04和全部历史保持。

## D13正式失败收件与D14未绑定候选准备

原C回合`01a0f199-aa1b-75a2-8689-66597b08a57a`于1790760546正式completed，final `msg_0d179092dafe07fa016abcd65e789c87d09b0a1a626a940782`。唯一新报告`unity-no-debugger-full-delivery.md`87行/11848bytes/SHA`8f1f12dbf730b0442d30b7220e95f56a7a11eee7faf76592f07040849e502b15`。原result291789bytes/SHA`ed9d2385c0a1f1913b2a9996aa2a09f9a23fc688c9a9042d21868301f64047d0`保持exit2；仅Compile21.373098秒、完整失败43.551715秒，无测试XML或全量成绩。32门20true/12false，18叶及6项未执行证据的缺失如实保留；23偏好样本均ce6fd4…6462，无空/竞争/间断，但没有完整退出稳定收尾，不能补造。

C89项只读收尾核实际源d28ac…ce90、53rsp/4175输入/NUnit/106实物27254b…4581及全部历史原样，CONT6506/30410、Host162/1062零新增；唯一PID及4个记录子进程自然退出，SceneTemplate未出现。C正式停写，现在交原R实际审查本失败及上述端口依据；SD00不替代独立结论，不授权C重试。

在等候R期间，只允许SD00 CreateNew未绑定候选`run_debugger_endpoint_full_validation.py`≤640行。以冻结D13 wrapper为基础，改动仅限：按准确包源码身份与每次实际PID计算托管端口并严查lsof p/c/f/n数字输出；保留每一原始查询、权限/解析/退场竞态反证，至少3个跨4秒初始化后存活样本；结合新实际argv准确-disableManagedDebugger和初始化日志无debugger-agent选项作交叉检查；其余端口只记录、不擅称所属子系统。新增保护D13五文件、full04所有实际18叶及正式报告。拟未来槽为full05；四身份常量保持UNBOUND，在ps/建槽/Unity前拦截，现只能AST及纯内存合成输入检查，不调用新main/preflight/launch、ps/lsof/defaults/Unity或建新运行槽。

`preference_stability_guard.py`及所有D13文件均冻结，不修改偏好门、源/106/53rsp/4175输入、测试/断言、墙钟、300/600门、进程安全和I/O保全。候选可复用D13独立4181期望及模式基线，仅在fresh身份/起点相符时；不从未来结果倒推。未来拟CreateNew只读`debugger-endpoint-full-preparation.json`≤32MiB及C报告`unity-debugger-endpoint-full-delivery.md`≤100行，当前不创建。CONT此次尚零新增，不扩大7168/40000及Host256/8192上限；如后续获准一次必要全量仍须准确另签。正式D13 R结论及新工具静态核对完成后才决定绑定/只读准备/执行，当前准备不提前授权运行。

SD00已完成上述未绑定候选：588行/42178bytes/SHA`ea50d84f8eb01c6b12df807a13c557b677a69bba94bb75cea1105bf398e2e734`。AST和25项离线检查全过，覆盖3个存活其它端口样本、完整5端点记录、IPv4/IPv6/通配托管端口拒绝、其它IPv6、合法空返回、错误PID/进程名/截断/额外字段/地址/端口/退出码/权限、退出竞态反证、死后空样本不补足、未初始化与不足跨度不作正证，以及4常量UNBOUND。端口定义源已确认属于冻结4175编译输入之一。没有新的ps/lsof/defaults/偏好读取或Unity，没有full05。helper及全部D13原字节保持；新工具尚待R静态核对和后续准确签发。

## D13独立NEEDS_FIX与D14最小纠正静态交付

原R回合`01a0f1a7-6620-7602-8e0f-6f2346bfa8f5`于1790761235正式completed，final `msg_0d29bf8d726d8dc0016abcd900244887d087b9835f6f1f08c7`，唯一verdict **NEEDS_FIX（D13实际交付）**。失败记录/18叶/23样本/缺失六证据、实际源码/模式/106产物/历史/I/O及退出均独立成立；原D10/D13仍不接受为目标。R复核官方包公式、六历史日志和D13四存活样本后，认定上述按固定实物及PID的准确托管端口检查边界足够，不需要扩大产品或另加Unity诊断。

现正式收窄纠正为上节D14新wrapper，范围仅端口识别、对应交叉日志及D13历史保护；25项纯内存检查与已准备实物保持不变。SD00独立AST差分确认：仅protected_evidence、preflight、listener_proof、launch、main五个既有函数变化及新增listener_observation，+63/-19；其它所有函数（含argv、closeout、首次退出冻结）AST完全相等，helper原SHA不变。当前588行/42178bytes/SHA仍ea50d8…2e734、四身份常量UNBOUND。交原R只静态核对这一最小差分；通过后才绑定只读准备及另签唯一full05，当前没有执行许可或新槽。

## D14静态接收与只读准备授权

原R回合`01a0f1b0-cb99-77a0-bf24-ac472b58ae27`于1790761521正式completed，final `msg_0d29bf8d726d8dc0016abcda2ac1f487d0ad93c53327a93aaa`，唯一verdict **ACCEPT（仅D14未绑定工具静态）**，无剩余必要纠正。R核SD00 25例并独立完成40项纯内存/静态检查全过；身份、准确端口/反证/正证门、D13保护及其它AST/helper保持成立。未执行入口或查询实际进程/偏好、未写文件，也未授权Unity。

SD00现只可绑定候选说明及原四身份常量为既有已冻结源d28ac…ce90、期望610c59…90cb、模式基线dee19e…c4f1b、helper1248a7…ce681，逆向恢复准确ea50d8…2e734以核除绑定外无变化。复用D13期望和模式基线的历史字段/creation时点保持，D14含义由本正式合同及新准备记录指定。

之后允许准确只读命令`python3 TestArtifacts/FMTestPerformance/2026-09-30/run_debugger_endpoint_full_validation.py --mode full --preflight`。既有D13已证沙箱阻止/bin/ps，可直接为这个准确只读命令申请必要权限；保存所有实际调用输出到CreateNew `debugger-endpoint-full-preparation.json`≤32MiB。fresh源/106/完整输入/763键初始语义及全历史/I/O必须通过，不静默换起点。仍不运行Unity、不建full05；成功准备后另签五文件身份及原配额内唯一执行。

## D14准确4181全量签发（full05唯一执行）

SD00仅绑定说明/四常量，内存逆向精确恢复R已审ea50d8…2e734，AST通过。获必要权限的唯一只读preflight exit0，5.838401秒，未运行Unity/建槽；输出原文保存在新准备记录。源d28ac…ce90、106产物27254b…4581、初始763键ce6fd4…6462、全部模式/输入、历史及CONT6506/30410、Host162/1062仍准确原样。R已确认无需额外Unity诊断，现沿原用户授权签发原C唯一必要全量：

```text
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_debugger_endpoint_full_validation.py --mode full --source-fingerprint d28ac0623c6eddb3374dcecf32fb9aed8f8b26f0d311ec5d9c9477c45294ce90
```

准确五文件身份（本目录，签发后冻结）：

| 文件 | bytes | SHA256 |
| --- | ---: | --- |
| run_debugger_endpoint_full_validation.py | 42427 | 9a37091b40c0a3efe6c45b5bae5d5327a461f60ecd15b1ef45c80bace2323197 |
| preference_stability_guard.py | 12949 | 1248a7ef77fc3619c64966955d06616b11240efb4e0deed72ad23ef4492ce681 |
| expected-no-debugger-full-tests.json | 42391 | 610c5989c90a14fda5e1a7456573e590ac9d7ecaef50c5eccfca9ce93a3f90cb |
| no-debugger-mode-baseline.json | 3990206 | dee19e80ce0d004741198c27bf97a04262b855a176d9e17e6eaeae9f678c4f1b |
| debugger-endpoint-full-preparation.json | 10076 | 59eba1509f9b056c55829ff499f1afa91bd3a384b52d7c836b7339ab6cb16472 |

1. 仅CreateNew `unity-runs/full05/`准确24叶：run.json、before.json、after.json、io-before.json、io-after.json、host-io-before.json、host-io-after.json、protection-before.json、protection-after.json、compiled.json、natural-cleanup.json、named-tests.json、result.json、tests.xml、compile.log、compile.stdout.txt、compile.stderr.txt、unity.log、unity.stdout.txt、unity.stderr.txt、compilation-before.json、compilation-compiled.json、compilation-after.json、preferences.jsonl。JSONL≤64MiB/512条/每条512KiB，其它JSON/XML≤32MiB，日志≤128MiB。C只另CreateNew `unity-debugger-endpoint-full-delivery.md`≤100行。
2. 先核五身份，唯一调用自身fresh preflight后串行Compile→无筛选4181；沿D13准确argv（只换full05输出路径），同Unity2022.3.18f1 Intel/osxuniversal、Release及一次-disableManagedDebugger；Compile带-quit、tests无-quit/filter。全部源、测试、断言、106实际DLL/PDB、53rsp/4175输入/四符号/NUnit保持。本次没有产品/配置/持久偏好改动权限。
3. 托管监听门仅按固定包源SHA及每次owned PID精确计算端口；初始化后查询完成时仍存活的有效样本≥3且跨度≥4秒。所有原始输出保留，目标端口/错误PID/命令/格式/权限/异常返回均失败，包括退出竞态反证；死后空结果不能补正证。其它端口记录而不分类，no_tcp_listen=false本身不是新门失败；准确argv及初始化日志交叉检查均须通过。
4. 原偏好helper完全不变：入口5次≥8秒，全部同字节/类型/763键检查，缓存仅两个有依据的时间整数有限刷新、正文/寿命/其它762键保持，真实进程窗口及不倒退，任何空/异常/竞争/间断单独失败；测试自然退出后≥20秒且末尾≥10秒稳定、最多60秒、末尾原生和后续模式采样一致。全部时间计入墙钟，不以最终恢复豁免，也不追认D10/D13。
5. CONT准确6506/30410，新≤600GUID/4096叶，总仍7168/40000；Host162/1062，新≤30/512，总256/8192。没有配额扩张、旧目录删除或下一轮授权。全部D1–D13实物（包括full04失败18叶及报告）和本五文件完整保护。SceneTemplate仍仅沿原准确3534bytes/SHA5baf…25d8c及先保全/自然退出合同，其它未知设置变化保持并失败；不手改Unity生成缓存。
6. 完整墙钟从最早Python入口到全部检查结束（最终result序列化和工具服务返回前），包括fresh入口、必要Compile、tests、退出与稳定/历史/I/O检查；300目标、600硬门不变。只有真实4181全Passed且所有保全/观察/时钟门通过并≤600，才可在R独立ACCEPT后接收；≤300才是五分钟。任何失败或超时均自然收尾、如实正式交回，不杀Unity、不改工具/覆槽/自增full06或重试。
7. 报告列准确命令/回合/五身份、4181 Counter与零失败/跳过、完整及分项、32门与实际偏好/监听证据、原4106与新增75分别统计、D9/D10同4181和9/29边界有别的对比、六原实例及保全/进程收尾。不能丢重复出现，不能把XML asserts=0当零断言，不能把43.552失败尝试或旧6分43秒失败当已达标。已由本唯一包装器fresh通过的整套只读检查无需无因重复；只补完成报告所需统计、证据身份和退出核验。
8. C沿用已完成D13上下文，完成后准确回合正式completed停写，再交原R实际独立审查。只有R正式completed且唯一ACCEPT才能接收。所有准备文件execution_authorized=false及D13 intent是不可改历史，实际权限由本最新D14节给出。主029、APK/设备、UI/本地化、配置/包/引擎升级、安装、Git写及新聊天/代理均不在范围；无需重复询问用户已批准计划。

原C D14已派发回合`01a0f1b7-cc36-7e00-9b72-0d80c3227ec7`（起1790761749），执行本唯一合同；本条仅记录在途身份，不预判测试或目标达成。

### D14在途新偏好差异的只读定位

本次Compile17.528071秒、准确托管端口门通过，tests PID73714已启动并持续采样。观察到仍仅UnityConnectUrlConfiguration改变，其余762键/类型/集合不变；新完整偏好114016bytes/763键，语义7a6ab1…be232。缓存寿命SHA保持、creation属于实际进程窗口且不倒退，但value字符串SHA由b5e8db…ad394929变为b65f9b…7789a1，故原逐字保全门如实记false。当前尚未正式收件，不改运行、不追认通过。

为分辨真实配置变化和JSON文本格式变化，允许SD00一次只读当前固定plist并只在内存解析该已知value字符串，最多16个标准JSON序列化候选（保留/排序字段、四组逗号/冒号空格、ASCII开关）对比原value SHA。仅输出长度/字段数量/摘要及匹配格式，绝不输出或保存URL/偏好原值、修改任何键、扩大字段猜测或运行Unity。若无匹配就报告未解，不能自动豁免；若匹配仍交R独立复核，原全部失败状态和样本保持。

上述首个只读脚本在生成任何候选前assert停止：它误以UTF-8直接SHA比较，而冻结helper实际对canonical二进制plist字符串计算SHA。该诊断错误没有运行候选、写数据或影响Unity；原工具输出保留。仅允许修正为已冻结helper的准确编码后执行原16候选一次，不扩大字段或范围。

按准确canonical编码完成16候选，**0匹配**，停止格式等价假设。当前payload为2196字符、44字段，字段值类型bool/int/str；原已观测2215字符。不能把真实内容差异称作纯空格变化。下一只读线索仅在已绑定Unity实物、已知cache-key文字偏移附近±128KiB内检查已有完整JSON常量是否准确匹配旧payload SHA；不猜其它字段、不输出常量或原值、不写文件、不启动Unity。若没有完整常量则如实报告，不能据邻近字符串豁免差异。

上述二进制只读检查在固定偏移66973222附近找到15个完整JSON常量（最大18字段），**0个**旧payload SHA匹配；没有原值输出/写入，停止该线索，未猜更多键或字段。

## D14正式失败收件：4181通过，464.110秒，偏好仍未通过

原C回合`01a0f1b7-cc36-7e00-9b72-0d80c3227ec7`于1790762584正式completed，final `msg_0d179092dafe07fa016abcde542dc887d08fc8490403b5487a`。报告`unity-debugger-endpoint-full-delivery.md`91行/13399bytes/SHA`99d38ff7d1033d5cc8b88b7a99e8c8effa8bee3ae3c2efbb92b57c1f4eb41d45`。result549158bytes/SHA`1c5e093f1032e12b26520bdd8fe1326f2f87becc75068045ac9e6a0e6335b9bf`；XML3102506bytes/SHA`39342d269c02f898e8602ca2755059a58e4a303c48651337125ee7ab4bf5d93d`。原exit2、passed/full_goal_verified/five_minute_goal_verified=false保持。

全部4181/4181Passed、零失败/跳过/不确定，准确Counter保留。完整464.110414秒、Compile17.528071、tests361.178676、XML344.428682、case总337.898581；原4106为276.231021、新75为61.667560秒。相对D10同4181反而多11.371315秒，关闭托管监听没有实测收益，不继续以此承诺五分钟。总墙钟包含60.150054秒偏好超时窗口；虽然<600，四个偏好关联门仍失败，不能接受为目标。实际两个PID73608/73714、12个记录子进程已自然退出。

准确托管端口56608/56714各4/178个有效存活样本，全182查询通过；两argv及初始化日志交叉核对通过。源码d28ac…ce90、53rsp/4175输入/四符号/NUnit/106实物27254b…4581与全部旧证据均保持；24叶/范围/I/O/时钟门通过。CONT现7063GUID/33002叶（新557/2592），Host185/1212（新23/150），旧条目保全，原配额未扩。没有额外执行/重试/新槽或产品变更。

偏好236条（235磁盘＋入口native），无采样缺口，append-only完整。非空快照只改UnityConnectUrlConfiguration，其余762键/类型/集合完全相同；缓存正文第一次改变在tests期间seq35（09:50:57.614518Z），不是只有时间字段。退出后seq204–210连续7次真实42bytes空字典，首09:56:34.765432Z、末09:56:46.809309Z，跨度12.043877秒；都是一致读取、可解析、0键。seq211于09:56:48.809348Z恢复763键但保持新缓存正文；其后稳定46.070624秒，因内容仍不兼容而观察超过60秒后失败，未执行末尾native。没有把短暂空字典当永久丢失，也不能把后来恢复自动豁免。

原R现在独立审D14实际及最小后续诊断边界；不直接重跑全量、不删除I/O、不扩大配额、不把未解释缓存正文或空中间态加入豁免、不改旧失败。尚需厘清平台缓存刷新和退出落盘过程，当前没有新Unity/偏好写入/恢复或新全量授权。

SD00可补一项现有证据的只读来源查询：仅macOS已有统一日志，限制D14的2026-09-30 09:50:00–09:58:00 UTC、Unity/cfprefsd进程且消息含准确com.unity3d.UnityEditor5.x或UnityConnectUrlConfiguration。先读log show帮助确定时间语法；已遇沙箱禁止log，即便help也拒绝，可为准确只读查询申请必要权限。不得启用/变更日志配置或擦除日志、不得开实时跟踪或新的Unity。只输出事件数、时间/进程/分类及消息摘要，不输出偏好值/URL/原始事件正文；无记录不证明没有写入者。原命令与结果保留在工具回合，当前不新增磁盘证据文件。

该准确日志查询已完成exit0：6个匹配事件，均为两本次Unity在启动时的coreaudio/cameracapture事件，没有命中写入/删除/同步元数据或cfprefsd事件；不能据此归因或排除写入者。stdout9991bytes/SHA44e655…c09d2只进内存，未输出原文或写事件文件。没有扩大日志时段/进程。

Apple官方UserDefaults文档说明内存值先更新、磁盘异步写入，并建议用系统API/defaults访问偏好；这只提供观测方向，不证明本次空字典正常。SD00可补一个纯只读当前状态观察，唯一CreateNew `post-full-preference-observation.json`≤4MiB：5组disk→native defaults export→disk，间隔5秒、首末≥20秒，开始/末尾只读确认无Unity编辑器，不启动任何Unity。用冻结helper的read_disk/native/摘要函数，不实例化写入Guard；所有原始偏好和native stdout仅留内存，不输出/保存原文。每组三份摘要/763键须匹配full05末次非空状态；另只记录当前缓存value内部44个字段的名称/类型/SHA和语义SHA，以便未来若变化可定位，不猜旧字段值或补造旧摘要。任何偏差如实保存并停止，不改基线或恢复设置。

本补充只证明当前观察窗口的API/磁盘及原762键是否保持，不能回填D14缺失的末次native、不能说明7个历史空样本时API的值、不能解释新旧缓存正文语义或追认原exit2/目标。开始已知ps在沙箱受限，可仅为准确只读观察申请必要权限；没有新全量、配额或产品/偏好写权限。来源：https://developer.apple.com/documentation/foundation/userdefaults 。

SD00上述现态补充已完成：`post-full-preference-observation.json`2204583bytes/SHA`78ea9c2fc5c928b00722d883af4d246113dcf29d7e80c15d9cb34ce3ad67039a`，5组/15份摘要，首末20.004400秒、完整20.114908秒。全部磁盘/native均763键，精确等于D14末次非空7a6ab1…be232，原其它762键/类型保全；始末无Unity。当前44字段名称/类型/SHA和缓存语义摘要已保存，没有原值或写偏好，没有回填原D14或改变goal=false。仅现态补充成功，历史空档API值/正文变化来源仍待定位。

## D14独立NEEDS_FIX与D15单例同期观察准备

原R回合`01a0f1c6-c97a-7271-b257-955489a6e1af`于1790763769正式completed，final `msg_0d29bf8d726d8dc0016abce2ec219087d084f284d36fb968ce`，唯一verdict **NEEDS_FIX（D14实际）**。R独立核实4181通过、464.110秒及全部失败；补充五组现态身份/15样本/44字段成立，但不能追认D14。建议仅一次D12原无I/O数学用例、同步磁盘—原生—磁盘及字段摘要；即使复现仍不能仅凭配对推定写入者，若未复现须如实结束。

SD00现可CreateNew未绑定诊断`run_paired_preference_observation.py`≤480行，以冻结D12单例观察器为基础，复用其源/实物/编译模式/自然退出/精确XML/历史/I/O保护。只作下列诊断改动：

1. 新槽候选`preference-runs/paired01/`，仍仅准确14叶：run.json、before.json、after.json、observations.jsonl、result.json、unity.log、unity.stdout.txt、unity.stderr.txt、tests.xml、protection-before.json、protection-after.json、compilation-before.json、compilation-after.json、natural-cleanup.json。唯一新C报告候选`paired-preference-observation-delivery.md`≤100行。当前不创建这些运行证据。JSONL≤64MiB/512条/每条512KiB，其它JSON/XML≤32MiB、日志≤128MiB。
2. 入口5组跨≥8秒、测试运行中及自然退出后的固定60秒，每组按磁盘→native defaults export→磁盘记录，逐次开始/结束UTC与单调时钟。每2秒发起一组；最长组间隔及原生调用时长全部披露，异常后也保留最终native尝试，不能因内容不兼容而跳过。首次实际观察到进程退出就冻结退出时间，后续采样不能延长进程窗口。只等唯一owned Unity及记录子进程自然结束，不杀进程；观察容量耗尽永久记不完整，继续安全自然收尾。
3. 用当前补充摘要7a6ab1…be232/763键/44字段为显式本次观察起点，仅用于诊断，绝不替换D14历史基线或放宽原完整验收。记录全键类型/SHA、缓存原字符串/正文字符串/规范化正文/各字段类型SHA；所有原偏好、native stdout及缓存值只留内存，不保存或打印原值。空字典、解析/竞争/原生失败及差异均作为实际观察保存，不自动判为无害。
4. 唯一原测试仍为`FightMatch.Core.Tests.ExactRationalTests.Arithmetic_AboveBinary64IntegerPrecision_PreservesUnitDifference`。拟argv精确继承D14 tests，除新输出路径及唯一-testFilter外不变；同Unity2022.3.18f1 Intel、Release、-disableManagedDebugger，不另Compile、不改测试/断言。源码d28ac…ce90、106产物27254b…4581、53rsp/4175输入/NUnit全部保持。
5. 完整保护D14的protection-after所列全部历史，加full05准确24叶、D14正式报告和新现态补充。I/O起点为full05后7063/33002及185/1212，单数学用例须零新增并精确保持；不扩大7168/40000及256/8192，不清理旧I/O。SceneTemplate仍仅原准确身份、先记录备份且已自然退出的既有合同，其它未知变化保留并失败。
6. 诊断passed只表示准确单例、观察完成及保护合同通过；明确full_goal_verified=false、five_minute_goal_verified=false，不将60秒观察或单例耗时称全量。即使配对重现也不直接认定写入者，未复现就如实正式交回，不自增槽或重复。D10/D14原失败永久保持。
7. 新source/D14报告/补充身份常量暂UNBOUND。仅AST与合成内存检查，不执行main、不读实际偏好或调用ps/defaults/Unity。原R静态通过后才绑定准确常量并正式签一次原C执行；当前无新Unity授权。所有原工具/helper/报告/运行槽冻结，产品、设置、包、引擎、主029、Git及新聊天/代理不在范围。

SD00未绑定候选已完成：457行/30895bytes/SHA`4c84244987419391f0bb24ea008fe796e5e451005ea1e8d274c4ac85ff0d6bc2`。AST及36项纯内存检查通过，覆盖正文格式/语义/字段差异、空字典与错误、重复JSON字段/布尔时间拒绝、无原值输出、首次退出冻结、容量不完整及专留末次native、三种disk/API状态/读取竞争/原生失败、配对序列和采样缺口、UNBOUND前置阻断及原D12不变。没有实际ps/defaults/偏好读取/Unity或paired01建槽。现只交原R静态审最小诊断合同及候选；尚不绑定或执行。

原R的D15静态审回合为`01a0f1dd-618a-7ff3-b44d-e8d2b3f71134`（起1790764212），当前仅记录在途身份，不预判其结论。

该原R回合于1790764619正式completed，final `msg_0d29bf8d726d8dc0016abce645161c87d093cccfab9a6e3213`，唯一verdict **NEEDS_FIX（D15未绑定静态）**。R纯内存重现两点：ps异常使末次defaults实际调用为0；Editor第4秒退出而子进程第80秒退出时，工具第64.03秒提前返回。SD00仅可修正候选native将清单与原生导出独立try、保留清单错误且仍实际导出；固定60秒观察后另等已记录子进程自然退出，另计等待耗时，不改首次退出时点或固定窗口，之后才最终native与保护证据。仍三常量UNBOUND，原实物/范围冻结，不添加实际查询或Unity。更正后只静态交原R复核。

两项最小修正完成，候选现475行/31647bytes/SHA`6b591bb462bb5057784da706e24243acaf4d5a74ce94cfc0beb18b69e9ea5ef4`。新增14项纯内存回归通过：ps失败后实际导出桩调用1次且错误/结果/区间均保存；Editor第4秒、子进程第80秒反例现在等到≥80秒才末次native，固定post仍约60秒、第一次exit仍4秒，单独等待耗时及run记录保存；子进程清单失败也记录错误并继续末次native尝试。没有实际查询、文件运行证据或Unity；仅候选/PLAN改变，三常量UNBOUND。交原R只审这18行新增的两项修正。

原R纠正复核回合`01a0f1e5-b292-7362-80f6-50a19fa4d4e3`于1790764923正式completed，final `msg_0d29bf8d726d8dc0016abce774250c87d08d9fa4f06aabf32c`，唯一verdict **ACCEPT（仅D15未绑定两项静态修正）**，无剩余必要修正。R独立44项静态/纯内存检查通过，逆向18行精确恢复前版、AST仅native/monitor变化；其4/80秒反例现80.03秒末次导出，first exit仍4秒、post约60秒。容量/清单/最终记录失败保持失败留证，没有实际查询、文件修改或新槽。

SD00现仅可将新候选说明及三常量绑定：source为d28ac…ce90，D14报告99d38f…41d45，现态补充78ea9c…039a，逆向须精确恢复已审6b591b…a5ef4。绑定后记录实物身份，另签原C一次实际执行；本条尚不启动Unity。

## D15唯一实际签发：paired01单例同期观察

上述绑定仅改说明及三常量，逆向精确恢复R已审版本，AST通过。新工具现475行/31817bytes/SHA`cabce25db84df11567ca35dbd0453a63f30a1331fdd44842543fb39017cb103c`，签发后冻结。原C获准唯一命令：

```text
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_paired_preference_observation.py --source-fingerprint d28ac0623c6eddb3374dcecf32fb9aed8f8b26f0d311ec5d9c9477c45294ce90
```

1. 沿本D15准备范围，仅CreateNew `preference-runs/paired01/`准确14叶及`paired-preference-observation-delivery.md`≤100行，不另开槽/改工具/运行全量。先核新工具SHA及D14报告13399bytes/SHA99d38f…41d45、现态补充2204583bytes/SHA78ea9c…039a；工具内所有PINNED身份、D14失败及全部历史保护、fresh源/106产物/53rsp/4175输入/763键/44字段/零I/O入口须成立，否则保全失败即停，不悄换起点。
2. 同一固定Unity实物，准确继承D14 tests argv，只换新槽输出与追加唯一D12原数学-testFilter；一次原测试、无额外Compile。保留-releaseCodeOptimization和-disableManagedDebugger、osxuniversal、无-quit。C负责所有实际Unity且必须串行；已有Editor或清单失败不可启动，不杀进程。
3. 入口5组≥8秒、运行中及Editor首次自然退出后固定60秒每2秒磁盘→系统导出→磁盘，记录每次读取起止/分组间隔/所有原始摘要与异常；若有已记录子进程未退，固定观察结束后单独等自然退出，再实际尝试末次系统导出并取得最终保护。首次exit不重写，额外等待另计；即使ps异常仍实际尝试末次导出、错误永久保留，不能把有phase名称当执行成功。容量永久不完整、预留最终导出位置；保留所有失败，不恢复/写偏好。
4. JSONL≤64MiB/512条/每条512KiB、其它JSON/XML≤32MiB、日志≤128MiB；无任何原偏好/URL/密钥/缓存值写文件或打印。只保存名称/类型/SHA、规范化摘要、时间和结果。准确原数学用例1/1及原断言保持，不新增/删减测试；源d28ac…ce90、106产物27254b…4581、现有模式和全部历史原字节保持。
5. CONT7063GUID/33002叶及Host185/1212须全程零新增/零改动，原总上限7168/40000及256/8192不扩大；SceneTemplate只沿原精确身份、先保全且所有owned进程自然退出后的既有合同，其它未知变化保留失败。工具fresh检查已通过的全套内容无需报告前无因重跑，仅补准确证据SHA/统计和必要退出核对。
6. 报告列命令/工具身份/准确C回合、1/1、所有检查、全过程及测试/固定观察/子进程额外等待时间、样本/配对/空/错误/间断/磁盘与API是否同时异常、字符串格式与语义及逐字段变化、末次实际导出、源/产物/历史/零I/O/自然退出证据。明确这是最小诊断，不是全量或300/600秒验收；若未复现就如实结束，不以稳定现态追认D14，不推定写入者，不自行重试。
7. 失败也须安全自然收尾并正式交回；原C回合completed停写且正式报告到齐，再交原R实际独立审查。原R静态ACCEPT不代替实际接受。主029/产品/UI/配置/包/引擎/安装/其它进程/全部Git写/新聊天或代理均不在范围。已有用户协调批准持续有效，无需重复询问。

原C已派发D15回合`01a0f1ea-c656-7321-a0bd-68aef74be7bc`（起1790765090），执行上述唯一合同；本条只记录在途身份，不预判诊断、复现或目标验收。

## D15正式收件：未复现空字典，捕获正文回到原47字段状态

原C该回合于1790765585正式completed，final `msg_0d179092dafe07fa016abcea0d735087d0bc7102b90c454349`，已停写。正式报告`paired-preference-observation-delivery.md`87行/11607bytes/SHA`80e1fa931009d2eaf871038c1c5ae176eeb512781912ef1a6fed057ca1741136`；result25780bytes/SHA`8ff14795716d6ff21d0b53fcb533828fe8dc226d56e9eb9cacbf82b1fbe8af60`；observations15242530bytes/SHA`0eb927485c46d74cebc42d8256b844cc7d28cedc1e27f8dfebd556584c3cabd5`；XML3365bytes/SHA`44387746c513445bb88c3767870f5166daf679d53d2e18be222cf10eda207899`。

唯一原单例1/1 Passed、PID79481自然exit0；完整诊断103.903829秒、测试进程20.806316、case0.041852、固定post60.141431、额外子进程检查/等待0.028023秒。14运行门全true，143样本=94disk+49实际native，47组=pre5/tests11/post31；所有读取763键，空/解析失败/竞争/原生失败/清单错误/采样缺口均0。末次native实际成功，与末尾disk全键完全同值。全源d28ac…ce90、106产物27254b…4581、模式/输入/所有历史和CONT7063/33002、Host185/1212精确保全零新增；14叶及上限成立，SceneTemplate未出现。C已确认唯一本次PID和5个记录子进程全自然退出，无其它Unity。

46组完全一致；组14在10:47:04.613210–.613364Z读取44字段旧disk，.647107–.658455Z native读到47字段新内容，.693106–.693265Z disk同新内容。仅UnityConnectUrlConfiguration str→str变化，其它762键/类型/集合全部保持。新增5、删除2、修改11、另31字段完全相同，明细及类型/SHA在报告。正文从b65f9b…7789a1变回**原D12及D14入口已记录的同一正文SHA b5e8db…ad394929**；对应规范化正文从9f8b86…bfc2c到1bd714…22af。新完整偏好语义4e6bdf…12e3，时间字段和整键SHA不同，不能称整个偏好恢复到历史状态。寿命SHA001316…be0保持。

SD00复核该正文SHA历史等同及XML时间：所选数学case从10:47:06Z开始，44→47转换在其开始前已完成。它是初始化/测试准备期间出现的已知正文切换证据；仍不根据配对自行推定Unity或cfprefsd是哪一个写入者。D15没有重现D14空档，不能回填旧API，也不追认D14或目标。现交原R实际独立审查诊断；暂不再授权Unity、全量、I/O扩容或任何缓存豁免。

原R D15实际审回合为`01a0f1f3-b843-7172-9b9b-501c11a76183`（起1790765676），只记录在途身份。

## D15实际接收与D16必要全量的条件保全设计

原R回合于1790766252正式completed，final `msg_0d29bf8d726d8dc0016abcec9885e887d0a1d84037b5b1e253`，唯一verdict **ACCEPT（仅D15单例诊断）**。143样本/47组、指定1/1、14门/103.903829秒、历史/模式/源/产物/零I/O及退出成立；未重现空字典。R确认正文47字段SHA等于D12/D14入口且切换早于数学case，支持下一合同有限双正文准入，不能混搭字段或豁免整个cache；时间数值仍须未来实际读取检查，D15时间哈希不能替代数值。旧D10/D14失败不变。

R建议把缺失的空文件/API同期观察整合进下一必要全量，不再重复无判别力的同一短单例。未来可预设条件分支，但只能称「已采原生视图保持、磁盘随后恢复」，不能声称证明异步落盘机制或每一瞬间API都完整。必须有磁盘空→原生完整→磁盘仍空的配对、完整前后样本、所有已采native合法无缺口、所有owned进程自然退出后的磁盘/native稳定一致；原生空/失败、其它键变化、第三种正文、缺口或最终不一致仍失败。

据此SD00签以下**未绑定准备范围**，只静态/纯内存，不提前执行Unity或建full06：

1. 唯一新`known-unity-cache-bodies.json`≤32KiB，从冻结D15 observations第一份44字段及最后47字段摘要逐字摘取两项完整正文字符串SHA、各自规范化正文SHA、完整字段名称/类型/SHA和共同寿命SHA，绑定D15与D14原始证据身份；不读取实际偏好、不反推原值或混搭。正文仅b65f9b…7789a1与b5e8d…ad394929两项，没有任意第三项或整键通配。
2. 唯一新`paired_preference_guard.py`≤440行，复用冻结旧guard的canonical/严格decode等纯方法，以同一缓冲区扩充正文元数据。只读磁盘和defaults，所有原值只留内存。每组磁盘→native→磁盘，逐次起止、单次组≤1秒；每2秒采样、同阶段≤3秒/跨阶段≤10秒，否则永久失败。native全程必须763键、其它762键/类型/集合精确不变；唯一cache外层仍严格value/creation/expires及类型、正寿命、共同寿命SHA，完整正文和字段状态成套匹配两项。时间使用实际数值、真实owned进程窗口、每个视图各自不倒退，不能因API先于磁盘而用跨视图时间制造倒退或放宽真实倒退。
3. 一致可解析的空磁盘字典必须原样留证。只可按同一轮连续磁盘空态episode分类：有前后非空合法边界、episode内至少一组确切「空→合法native→仍空」、全轮所有native合法/无间断、最终磁盘/native稳定一致，才可条件接收为「已采原生视图保持、磁盘随后恢复」。只见空后恢复但缺该夹持配对、开放episode、读竞争/解析失败、native空/失败、任何未知更改或采样缺口都失败。保存所有空样本、episode边界、夹持配对和所有反证，不改D10/D14旧门或旧结果。有限取样不外推每一瞬间或写入者。
4. 入口5组≥8秒且三视图逐键等于绑定新起点；运行全过程配对。退出后总观察≥20秒、末尾磁盘与native精确一致且稳定≥10秒，最多60秒；退出时间首次观察即冻结。所有owned子进程须自然退出，超出观察窗口的必要等待单列且计总墙钟；不杀进程。观察异常或清单失败也实际尝试最终native，异常永久保留，不能只留下名为native的未执行行。达到观察容量后永久标不完整，预留最终native位置并自然收尾。
5. 唯一新`run_paired_full_validation.py`≤650行，以冻结D14 wrapper为基础，只改新helper/绑定与证据保护、偏好门/异常末次取样、full06槽及本次容量合同；保留串行Compile→无过滤4181、原D14 argv仅新输出路径、准确managed endpoint门、所有实际源码/53rsp/4175输入/四定义/NUnit/106产物、完整Counter/断言、墙钟及300/600门。源/期望/模式基线/helper身份暂UNBOUND，实际查询和建槽前拦截。
6. 观察容量拟1024条/128MiB/单条512KiB，含入口/Compile/全测试/退出/终核及最终native储备，覆盖600秒窗口按2秒三视图的预算；超过即不完整，不能删除旧样本。未来full06仍准确24叶，字段名同full05；其它JSON/XML≤32MiB、日志≤128MiB。拟新`paired-mode-baseline.json`≤8MiB、`paired-full-preparation.json`≤32MiB及原C`unity-paired-full-delivery.md`≤120行，当前不创建、不读取fresh偏好或运行preflight；静态审通过后另签只读绑定准备。
7. 下一次必要全量的准确I/O起点仍CONT7063/33002、Host185/1212，拟新≤600GUID/4096叶及Host≤30/512。为这一次运行明确拟将CONT总GUID由7168增至8192，总叶40000保持；Host总256/8192保持。此处仅准备预算，未授权实际运行/写I/O；最终签发须fresh核起点，所有旧I/O原样保全，不删除、不授下一轮扩容或重试。
8. 完整保护D1–D15所有工具/报告/失败/运行槽、现态补充和新manifest；只有新候选与PLAN可改，未来baseline/preparation另签。两新工具静态通过后才绑定、只读准备和签原C唯一必要全量，实际C completed后原R再审。五分钟目标/十分钟硬上限、主029暂停与全部产品/配置/引擎/Git/聊天边界不变。

SD00已完成未绑定静态候选：known-unity-cache-bodies.json15112bytes/SHA`6c1a3edf4727dbfa866421a400249c942a4158e323830782c8c7e753a6ac72aa`；paired_preference_guard.py298行/18349bytes/SHA`d4880c94e5d79732788daabcb74b3cb23bee1cb875c2e6c0de793081ba287ba1`；run_paired_full_validation.py588行/41596bytes/SHA`d56aca955187d9481e60765e8fa42e736f00fd0b0fcc39232d864b5bc46b2956`。manifest逐字段取自R已审D15两状态，无原值；wrapper四常量UNBOUND，baseline/preparation/full06均未创建。

AST及86项离线检查通过（51函数/静态、30生命周期、5跨视图/episode附加），只在内存使用虚拟时钟/进程/文件流和合成偏好值。覆盖成套双正文/拒绝第三种或字段混搭、其它键/时间窗口/寿命/倒退/类型/重复字段、磁盘与native分别的单调时间、确切空字典夹持配对与每段闭合边界、缺口/开放episode/原生空和失败永久拒绝、正常及条件恢复生命周期、ps失败仍末次导出、晚退子进程等待与原deadline失败、容量标记并预留最后导出、append-only和无原值输出。wrapper AST仅protected_evidence/preflight/closeout/main改变；argv/launch/首次退出冻结/端口解析与证明/编译核验等其余函数AST原样。没有任何实际偏好、ps/defaults、Unity或运行证据写入。现交原R只静态审核候选/条件规则与容量预算，不绑定或执行。

原R D16静态回合`01a0f20e-07a8-7990-83a5-4004bf4273ac`（起1790767400）在途，尚无执行权限。

该原R回合于1790768057正式completed，final `msg_0d29bf8d726d8dc0016abcf3a8dd6c87d09201d2bf1afcdf54`，唯一verdict **NEEDS_FIX（D16未绑定静态）**。R完成8项静态/32项纯内存核验，三项必要纠正：稳定尾段须在确认所有owned进程退出后才累计（反例仅退出0.04秒就以20.03秒稳定通过）；owned_child_exit记录异常不能中断自然等待或跳过最终native；0.5秒子进程盘点带来的高频三视图采样使保守预算1043条超过1024。

SD00只修两新工具及本说明：稳定状态候选必须同时有正证确认所有owned进程已退，任何未退出/盘点失败重置尾段；用首组最后读取完成到末组最早读取开始的共同区间保守计稳定≥10秒，first exit和60秒deadline不变。隔离子进程等待中的记录异常，永久记不完整并继续等待；单独finally保证实际最终native尝试。所有子进程等待仍每0.5秒盘点，但三视图按前组开始时刻至多每2秒采一组，wrapper/helper两处统一；维持1024/128MiB预算，不静默加容量。manifest及所有旧证据冻结、四常量UNBOUND，不作任何实际查询/Unity。回归R三反例及既有空态/容量/最终导出边界后交R只审修正。

三项修正现已完成。paired_preference_guard.py为310行/19296bytes/SHA`9dd13981b40af2d07ac4651d08913fce1a688f73f218cb653e68a0f31b9ec14b`；run_paired_full_validation.py为588行/41672bytes/SHA`e04d17117c534499e79db43933df3ae7a38018f590ae215e903a1e00c98a71a3`。manifest仍15112bytes/SHA`6c1a3edf4727dbfa866421a400249c942a4158e323830782c8c7e753a6ac72aa`，四身份常量仍UNBOUND，baseline/preparation/full06未创建。

新增24项纯内存回归全部通过：正常生命周期及合格的磁盘空态episode仍通过；子进程晚退反例现在从首次正证确认全部退出后的共同读取区间计算≥10秒稳定；owned_child_exit样本写入OSError反例永久不完整但继续至少30次盘点、等到子进程自然退出后实际执行最终native，首次exit不改；append-only保持。实际wrapper.guard在虚拟30秒中作61次0.5秒进程盘点、只作15组间隔≥2秒三视图采样；据R原保守模型，容量从1043降至908条（1043−(60−15)×3），小于1024且仍预留最终导出。所有时钟/进程/文件流均为内存桩，实际偏好/ps/defaults/Unity查询及运行证据写入均0。原86项离线检查为前版记录，不冒称全部重新运行；本次24项覆盖新增纠正及相邻回归。现交原R仅复审三项修正，不绑定、不启动Unity。

## D16静态接收与只读绑定准备授权

原R复审回合`01a0f225-7860-7d31-97cf-9033248eca58`于1790769414正式completed，final `msg_0d29bf8d726d8dc0016abcf8fe4a5087d0ba4b154bc26a9174`，唯一verdict **ACCEPT（仅D16未绑定三项静态修正）**，无剩余必修项。R独立84项纯内存检查全过：晚退反例共同稳定11.97秒，超过期限仍失败；记录失败永久不完整但继续自然等待与实际最终native；0.5秒盘点/2秒三联及908/1024保守容量成立，身份与范围完全匹配。未读实际偏好或执行Unity，不能替代全量接收。

SD00现仅获准以下只读绑定准备；仍不执行Unity或建full06：

1. 保持manifest和helper身份冻结；从冻结D13 mode baseline复用全部非偏好编译元数据，fresh只读核本机无Unity，读取一次磁盘→native→磁盘。全部须763键、已知成套正文、三视图完整语义相同且精确等于D15末次`4e6bdf18cb128dc43d591ff2049862cdf9bfe7e81faf07a4a11c9b328f9012e3`；显式对照D15所有763键类型/SHA，不悄换基线。实际时间由新helper严格解析，原始值仅内存。
2. CreateNew `paired-mode-baseline.json`≤8MiB，只更新该新文件的偏好起点和准确来源/审查记录，原mode baseline不改；记录三视图摘要、旧编译来源身份、新helper/manifest及D15依据。四身份常量绑定source d28ac…ce90、原expected 610c…90cb、新baseline实物SHA、helper 9dd139…c14b，只另改说明行；逆向替换须逐字恢复R已审未绑定wrapper e04d17…71a3，AST成立。
3. 仅执行绑定后`python3 TestArtifacts/FMTestPerformance/2026-09-30/run_paired_full_validation.py --mode full --preflight`，只读fresh source/106产物/53rsp/4175输入/完整期望/全部历史/I/O及模式。所需ps/defaults只读沙箱提升沿本会话已授权范围处理。保存准确命令/退出码/原输出摘要和全部绑定身份到CreateNew `paired-full-preparation.json`≤32MiB；明确Unity未启动、full06未建、execution_authorized=false。
4. 任何不一致保持失败，不替换起点或自动追加观察；全部准备通过后另记准确身份并签原C唯一full06。只有该正式签发才授本次8192GUID总量及必要全量；主029/产品/设置/引擎/Git与旧证据边界不变。

上述只读准备已完成：3视图全部763键、完整语义精确等于D15末尾4e6bdf…12e3，全部键类型/SHA保持；无Unity。新mode baseline为4447266bytes/SHA`4f1b2affefe50110e02ab97cb296d783824c6124870e0bc0423ea179fe445b36`。仅说明及四常量绑定，逆向精确恢复R已审e04d17…71a3；绑定wrapper为588行/41922bytes/SHA`9f35fa5662046618b55f46c62f9fbf639d68a3b4f177c51073450fe8660cb553`。只读preflight exit0，内部6.635470秒、完整包装6.664964秒、外部命令6.708744秒；源d28ac…ce90、全部旧证据/I/O/编译身份成立，full06未建。新preparation为8981bytes/SHA`e6a813fe3f3a17ceaf78f916e41bf732578709c844a94cce4a52730412b5b789`。两次必要只读沙箱提升获准，无自动审查拒绝。两新JSON及工具即刻冻结。

## D16唯一实际签发：full06必要全量

原C现获准唯一命令，由C串行执行Unity，SD00不执行：

```text
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_paired_full_validation.py --mode full --source-fingerprint d28ac0623c6eddb3374dcecf32fb9aed8f8b26f0d311ec5d9c9477c45294ce90
```

1. 先核以上wrapper/baseline/preparation准确身份及helper310行/19296bytes/SHA`9dd13981b40af2d07ac4651d08913fce1a688f73f218cb653e68a0f31b9ec14b`、manifest15112bytes/SHA`6c1a3edf4727dbfa866421a400249c942a4158e323830782c8c7e753a6ac72aa`、原expected42391bytes/SHA`610c5989c90a14fda5e1a7456573e590ac9d7ecaef50c5eccfca9ce93a3f90cb`。准备文件中的execution_authorized=false是创建时状态，本条是唯一后续执行许可；不改原字段。
2. fresh入口必须保持source d28ac…ce90、106产物fingerprint27254b…4581、53rsp/4175输入/NUnit/DEBUG/TRACE/UNITY_ASSERTIONS/UNITY_INCLUDE_TESTS和冻结Unity2022.3.18f1 Intel实物；偏好起点精确4e6bdf…12e3/763键，CONT7063GUID/33002叶、Host185/1212与D15after逐条一致，所有历史证据原字节保持。任何不一致或已有Unity/盘点失败即停，不悄换基线，不额外启动。
3. 一次Compile自然exit0后一次无过滤EditMode全量，原4041和此前140新增合计4181具名出现全部保留，按Counter包含重复fullname，零删改断言/随机轮数/跳过。argv精确沿D14，仅输出改full06；保留-releaseCodeOptimization、-disableManagedDebugger、osxuniversal，测试无-quit。端口仅准确owned PID托管端口的既有证明；其它TCP保留为观察。工具失败不授重试、追加槽或参数变更。
4. 本次CreateNew `unity-runs/full06/`准确24叶：run.json、before.json、after.json、io-before.json、io-after.json、host-io-before.json、host-io-after.json、protection-before.json、protection-after.json、compiled.json、natural-cleanup.json、named-tests.json、result.json、tests.xml、compile.log、compile.stdout.txt、compile.stderr.txt、unity.log、unity.stdout.txt、unity.stderr.txt、compilation-before.json、compilation-compiled.json、compilation-after.json、preferences.jsonl。JSONL≤1024条/128MiB/每条512KiB；其它JSON/XML≤32MiB、日志≤128MiB。只新增正式报告`unity-paired-full-delivery.md`≤120行，不增其它文件。
5. 精确授权本次测试自然新增CONT≤600GUID/4096叶、总≤8192GUID/40000叶；Host≤30GUID/512叶、总≤256GUID/8192叶。旧I/O一项不删不改，此扩容仅full06，不授下一次运行。SceneTemplate只沿原工具的准确身份、先保存且全部owned自然退出后的既有清理合同，未知变化一律保留失败。不手改Library/设置/偏好，不清理其它进程或玩家数据。
6. 入口5组≥8秒；运行中每2秒磁盘→native→磁盘；退出后≥20秒且所有owned退出后的共同稳定区间≥10秒、最长60秒；必要子进程额外自然等待单列并计总墙钟，首次exit固定，不杀进程。正文只允许manifest中成套44/47两状态，其它762键精确保持，实际时间/寿命/进程窗口/各视图非倒退严格。只有证据完整闭合且有确切空→合法native→仍空夹持的每一段磁盘空态才按已签条件接收；native空/失败、第三正文、其它键变化、缺口、容量耗尽或最终不稳定均失败，保留全部样本/反证。记录故障也安全收尾并实际尝试最终native，不推断写入者或每一瞬间API状态。
7. 总墙钟包括入口、必要Compile、全部4181、观察和终核；≤300秒为五分钟目标，300～600秒为用户接受上限，>600秒不接收。产品测试全过但任一保护门失败，也不能称全目标通过。D10/D14所有历史失败原样保持。报告列命令/版本/工具和证据身份、准确C回合、4181计数/原多重集合、分段和总时间、各门、三视图计数/空态episodes/缺口/最终稳定/实际native、source/编译/历史/I/O/退出及300/600结果；已成功核验的全套内容不为写报告无因重跑。
8. 失败也须自然安全收尾并正式交回；C completed且报告到齐才交原R实际独立审查，R completed唯一ACCEPT后才正式接收目标。当前只授以上一轮；不修改任何产品/测试/UI/配置/引擎/包、不恢复主029、不执行Git写或创建聊天/代理。沿用用户既有C/R授权，无需再次询问。

原C已开始D16回合`01a0f231-745b-74a1-a3a4-35fff3e511a9`（起1790769722）。本条只记录正式派发身份，所有Unity与报告仍由C按唯一合同执行；尚未接收计时或目标。

## D16正式收件：8分13秒全测通过，单空样本缺旧合同夹持

原C回合于1790770917正式completed，final `msg_0d179092dafe07fa016abcfee1e8a487d0a2be7a2e8d87c3ad`，已停写。报告`unity-paired-full-delivery.md`103行/16235bytes/SHA`dd73a82b08804f7e5451333cc1c8936eb022454367952d1db177af824d338c9d`。full06 result868440bytes/SHA`b051319cc115e6f5ddcc9d35a51b050a918f79c4233587cca5251d5e94df9c1a`；XML3102594bytes/SHA`6fedc5e867bb752b92ef285c5e5f8431936a59cca6b82ab5021f182e162da9ac`；731条preferences为78098582bytes/SHA`33c58410ffd05ebc4d52b434887b0cb791d01d4f384d70b1dff7ee313e6bd0af`。全部实物即刻冻结，不追加Unity。

完整492.633793秒（8分12.634秒）、Compile20.159693、tests425.466448、post20.261728秒；UTC12:03:33.405631→12:11:46.040847。4181/4181 Passed、0 fail/skip/inconclusive、4176种fullname及重复重数正确；原4041＋140新增完整，原4106＋75新增Counter精确。XML407.6121687秒，case总399.650337、原4106为325.336760、新75为74.313577。≤600计时成立、≤300不成立；33门31真，两假均因单空样本缺夹持，wrapper exit2及full_goal_verified=false原样保持。

243组三视图/731样本=486disk＋245native，730个非空样本全部763键、完整语义精确等于D15起点4e6bdf…12e3；没有任一cache/时间/其它键变化，正文全部已知47字段。native均实际成功/无stderr；没有采样缺口、进程错误、记录错误或容量不足。唯一空seq694是post组231的disk_before，读取12:11:18.049239–.049454Z、一致42bytes/0键/SHA9261…13f1；同组native seq695在.049876–.066290Z读到完整原起点，disk_after seq696在.080273–.080466Z也已完整恢复。前一disk seq693为12:11:17.203337–.203553Z完整。实际是「空→native完整→disk恢复」，没有「空→native完整→仍空」；因此原条件正确返回qualified=false，transient_sequences=[694]。31毫秒为相邻读取恢复观察间隔，不等同实际空态持续时间，不据此推定写入者/异步机制/每一瞬间API状态。

全部owned退出后的共同稳定17.983924秒，post实际20.209763，额外自然等待0.044682秒；native_end seq727实际成功，终核组242另有seq729实际native及最后disk730，全部三视图仍同原起点。2个Unity和10个记录子进程自然退出，C补充只读ps成功；源d28ac…ce90、106产物27254b…4581、53rsp/4175输入/NUnit/四定义/Release及D1–D15历史保持。CONT7620GUID/35594叶、Host208/1362，新增557/2592及23/150，全部旧I/O保持；SceneTemplate未出现，准确24叶。protection前后同326199bytes/SHA`ed356d1e1149c172199415753c102d59b2837034e15f470907997731b9f8d741`。

SD00已独立核上述XML Counter、计时、原样本和核心身份；本条不是R接收。现交原R按原D16合同实际独审并给唯一verdict，同时指出下一最小必要步骤：区分原夹持条件是否满足与现有数据能支持的有限结论，评估是否可用单独签发的只读重放/快速恢复分支补足验收，或具体还缺什么证据。任何后续规则修正须另签并独审，不反写D16原失败、不预判接受，也不为碰到某种偶发磁盘窗口自行重跑全量。

原R D16实际审查回合为`01a0f245-935d-79c0-a0a8-d22aefc066f9`（起1790771041），当前只记录在途身份；没有授权新的工具改动或Unity。

## D16实际R收件与D17最小只读补充签发

原R回合于1790771586正式completed，final `msg_0d29bf8d726d8dc0016abd016e70c087d08388588e7052c157`，唯一verdict **NEEDS_FIX（D16 full06实际）**。独立核报告/六文件/24叶、4181全部通过/原4041＋140及重复Counter、492.633793秒、731逐样本、原分类器纯内存重放、末尾17.983924秒共同稳定、最后实际native/disk、源/106产物/53rsp/4175输入/NUnit/四定义/历史/I/O及2 Editor＋10子进程退出全部成立。唯一缺口仍是旧合同夹持未满足，不是检测到其它状态变更。原exit2、两假及full_goal_verified=false必须保留。

R明确建议另行签发、独审且**仅限冻结full06**的「同组恢复」只读重放，无需为捕捉更长空窗口循环全量。新分类可支持「所有已采native原样、磁盘同组恢复、最终稳定、全量耗时合格」；它不补造旧夹持，也不声称写入者/异步机制/每一瞬间状态。SD00按已批准性能调查及最小纠正授权签D17：

1. SD00只新建诊断工具`reassess_full06_recovery.py`≤340行及CreateNew输出`full06-recovery-reassessment.json`≤4MiB，并更新本PLAN。所有产品/测试/配置/引擎/偏好/旧工具/全部旧证据和full06逐字冻结。读取范围仅本次准确绑定的full06 24叶、C报告、六冻结签发文件、旧4106与原4041 XML及必要已冻结decoder/manifest。无实际prefs/defaults/ps、Unity、编译、I/O新增或新运行槽；无Git写/新聊天/代理。
2. 新工具固定full06，不接任意路径或自增槽。先核所有输入字节身份、普通非链接文件、24叶和容量、原失败内容；独立复算XML Counter/原4041/新增140和计时，保留R已独立成立的其它31门及全部保护身份作为范围明确的复用依据。原执行492.633793秒为历史实测，不把重放称新全量，也不将非连续时间拼成新的墙钟；只读重放耗时另列。
3. 新分类仅允许**唯一**一致规范空字典样本，必须是一个完整三视图组的disk_before，前一个合法非空disk为明确前界，同组实际native成功且无stderr、disk_after已恢复；三组内读取顺序和起止完整，单组≤1秒，全部采样的native及非空disk（包括cache与时间）须逐键类型/SHA及完整语义**精确等于D15绑定起点**，而非仅落在双正文宽集合。缺前界/恢复边界、native异常、其它变化、第二空样本或任何缺口都拒绝本分支；原较长空态分支不在本次重新分类范围。
4. 重算逐行序号/分组/读取一致性/配对≤1秒/原同阶段3秒与跨阶段10秒上限/1024条及128MiB、无记录和清单错误；核原process窗口/自然退出记录、所有owned退出后共同稳定≥10秒、post20～60秒、native_end及之后真正末次native和disk仍同起点。不能靠放宽容量、跳过失败、删样本或换时间口径完成。
5. 先进行针对新分类的纯内存正反例：实际组顺序、缺前界、仍空或多空、native失败/空/变化、非空disk变化、时序/间隔/容量/退出/尾段不足等。负例仅内存修改，不写历史。全部必要检查通过后仅执行一次新工具离线生成补充证据；结果记录旧失败、分类依据与限制、原完整实测、重放耗时、输入/工具身份和保护前后一致。任何失败保留，不自动调用Unity补跑。
6. SD00把工具、纯内存检查、实际补充结果和精确身份交原R一次独立审查。R正式completed唯一ACCEPT后，才可据原实测及此补充接收本机Release全量≤600秒；≤300秒仍未达。该接收不覆盖旧D16合同或保证未来每次运行/其它机器相同；不更改原结果，不扩展正文、其它偏好或产品范围。

D17工具现为289行/24611bytes/SHA`581a68c98a23fc9237d5f87e16f9f9822aec6377103d432d516145f0f4bd516c`，AST通过。固定34份输入的字节身份（含full06准确24叶），没有任何实际偏好/进程/Unity入口。`--self-test`已执行，28项纯内存检查全部通过：冻结原记录正例；缺前界/错误位置/不一致或非规范空字典/第二空态；native退出/错误输出/空/变化；disk和cache时间/键摘要变化；未执行合法读取/时序反转/缺序列/间隔或容量不足；清单/记录故障、子进程存活、尾段不足或超期限、末次视图变化。未写文件、未新增测试运行。现只按已签范围执行一次`python3 TestArtifacts/FMTestPerformance/2026-09-30/reassess_full06_recovery.py --write`；其输出仍待原R独立接收。

D17唯一离线执行已完成，exit0；新输出`full06-recovery-reassessment.json`9650bytes/SHA`1320ff52c6df7d5995d27073cc83c606bab524ed91503d175f126b9ba5182dca`，即刻冻结。24项检查全部通过（10项测试/计时/原失败/身份＋14项偏好证据），34份输入前后字节身份完全保持。独立按原始起止重算post20.207556秒，与原包含判断开销的20.209763秒分别保留；共同稳定17.983924秒，真正末native729/末disk730。单空694→native695→同组disk696恢复，前界693；实际系统读取245次与非空disk485次全部完整等于起点。重放自身0.527994秒单列，不拼入或冒称新的全量墙钟；原492.633793秒、旧exit2和全部历史false保持。输出eligible_for_target_acceptance=true且final_acceptance_requires_R=true，目前尚未正式接收目标。原R现只复核新分类工具、反例及这一份补充证据，不再做实际查询或Unity。

## D17正式接收：十分钟上限达成，五分钟未达

原R回合`01a0f257-7778-7bf2-98a7-25e3489217dd`于1790772489正式completed，final `msg_0d29bf8d726d8dc0016abd04fd77b487d0a156507ba9784b0e`，唯一verdict **ACCEPT（D17最小只读补充）**，无剩余必修项。R核工具/输出/34输入及full06准确24叶，实际重跑获准28项纯内存检查，另补7个针对性反例均拒绝；独立复算10项非偏好、重现14项偏好检查全部通过。同组恢复、完整起点一致、最终真正native/disk和共同稳定均成立，没有任何额外查询/Unity或文件修改。R明确确认，补充与已审full06原始证据足以支持本机Release这一次完整实测≤600秒的目标接收。

**SD00最终接收：ACCEPT（用户允许的十分钟上限；五分钟目标未达）。** 4181/4181 Passed，原4041全部保留、另140新增，零失败/跳过。原始完整墙钟492.633792500秒=8分12.634秒=8.210563分钟，含入口/必要Compile/全部测试/观察/自然等待/终核；低于600秒107.366207秒，高于300秒192.633793秒。54分27.032秒→8分12.634秒，已执行命令对照少46分14.398秒，约84.9211%。54分钟基准未单列Compile且少75项；不把这项墙钟对照当逐项独立因果测量。

接收依据是D16原真实运行加D17针对同组恢复的独立补充，不是重写旧合同。D16 exit2、两个失败门和full_goal_verified=false，D10/D14失败，以及D17输出创建时的final_acceptance_requires_R=true全部保持；本正式收件提供后续审批事实。离线重放0.527994秒独立记录，不拼接新的墙钟；不推断写入者/异步机制、实际空态持续时间或每一瞬间API状态，不保证未来每轮/其它机器同耗时。

主要有效改变为：FirstRelease成功准入结果复用；每Catalog精确存储快照与完整绑定一致时复用已验证结果（仍读取七份内容并核变化）；测试准备共享不可变首包、保留各玩家/目录/存档隔离；本次进程采用Release代码优化且保留全部定义/NUnit/原断言。此前大整数位数边界和序列化反射/字符串优化已包含在54分钟起点中，不重复计入本轮收益。关闭托管调试监听无实测提速证明，不列为有效收益来源。

本性能专项按十分钟上限收尾，C/R均正式完成并停写；不追加测试、I/O、产品改动或运行槽。仅授权SD00在`START_HERE.md`新增一段本专项的接收索引和证据链接，保留同时进行的设计访谈、艺术任务和029暂停状态。Unity/工程版本、产品配置和Git历史保持；本次无提交/推送。进一步争取五分钟应另以实际计算瓶颈测量为依据，本接收不授未来运行或扩大实现范围。
