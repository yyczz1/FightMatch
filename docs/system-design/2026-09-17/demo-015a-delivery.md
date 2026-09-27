# FM-DEMO-015A 技术封套与严格标量编解码

状态：WORKER_RETURNED；同版联合编译及2426条EditMode全部通过，C已独立核验证据，正式交回R审查。本文不作独立审查结论。
C task：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本次原 turn：01a0c403-c2a4-7c50-851a-3335c3080a47。
授权：system-task-packets.md r75 §120、§122～125；派发稿 SHA256=06425a9ae84a74ebaaab2ed71d59c6b83b66d49ce80242750dc5c46808cdeb2a。
仅本包五份 C#、本报告及 demo-015a-scope.json 由 C 新建；五份 meta 由 B 唯一启动 Unity 导入生成。
未启动 Unity、创建任务／子代理、分支／worktree、提交／推送；未修改旧源码、测试、配置、权限、依赖或同批 014。

## 实现及边界

SaveEnvelopeCodec 提供 Prepare、Write、Read、ReadRequirements 四入口，统一 IsAccepted／Value／RejectionCode／FieldPath。
Prepare 冻结切片数组及完整技术元数据；正文逐片保留，不拼接第二份整档大数组。
Write 从调用方流当前位置写 FMSAVE01 版本1：60字节头、严格元数据、连续切片正文；返回全 snapshot 长度及 SHA256。
Read 校验完整 expected 身份／长度／摘要、目录及每片／正文摘要后才返回只读切片；ReadRequirements 流式散列正文，只返回技术元数据及 descriptor。
所有流由调用方拥有；不打开路径，不 Seek 回填，不 Close／Dispose／Flush；I/O 异常原样传播，中途写出的临时字节留给016处置。
根 null 抛 ArgumentNullException；结构、格式、摘要及资源拒绝不给半份 Value。资源 Limit 带具体原因、所需／允许量；Math 原限制信息保留。
SaveCodecBudget 共享原 ExactMathBudget，分别限制整份字节、元数据、各集合、UTF-16代码单元和数字 token；字节预算不声称等于托管内存峰值。
技术字符串逐 u16 小端保留 Ordinal，包括非ASCII、孤立代理单元和 NUL；身份非空，不 Trim、不归一化；可空字段用0／1标志，未知标志拒绝。
ExactSaveValueCodec 严格接受唯一整数及约分 n/d，保留 >2^53 整数和分数；先核 token 及整数位预算，通过既有受限原语和 ExactRational.Create 还原。
目录与 RequiredSliceContracts 同序精确覆盖，SliceId 唯一、Owner／schema一致；切片 offset 连续，允许显式空列表及零长切片，不推定业务有效。
CommitIndex 从1连续至本代；完整父链、CommitId／OperationId唯一；只有当前行不带旧 snapshot 长度／摘要，自足查询不依赖祖先文件。
SaveRequirements 按完整绑定组件比较，能力列表覆盖绑定版本；顶层恢复要求按目录及各列表顺序取首次出现并集，读取拒绝少报、多报、重复及错序。
SourceNotes 按有序来源列表保留原顺序及重数；五个恢复要求集合仍逐项判重，不把来源列表静默转集合。
CandidateContent／CandidateDefinition 只允许 CandidateValidation；PlayerSave接受发布绑定的技术字段不构成Published授权。
未知业务 schema 可读技术摘要；无战斗／成长业务实例化、014 API依赖、015B切片恢复、真实文件提交、发布或 H10 Completed 声明。

## 独立字节依据

最小完整固定向量为128字节：60字节头＋68字节元数据＋0字节正文。
Purpose=CandidateValidation，PlayerId=p，Generation=1，CommitId=c，Parent=null；Required／目录显式空，索引唯一行(1,c,null,无旧摘要,空操作)，恢复要求五列表显式空。
正文摘要为 SHA256(empty)=e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855。
完整固定字节按字段字面量拼接，独立 PowerShell SHA256 得 3ad60355090274a1067a5480dd1bc8bf89161c97c5951941de012970ae42e0ea；测试固定读入与写出分别对照，不以自产自读代替。
源码中的黄金常量已独立重核128字节及上述SHA；标量字节另核0、-1/2、A中、U+D800以及null／空字符串标记。

## 七项验收用例

以下31组方法展开171条，全部在最终同版XML中Passed；scope.xmlSummary保存逐完整名称、结果、方法展开数与实测输出。

| 验收 | SaveEnvelopeCodecTests 见证 |
| --- | --- |
| ①确定字节与精度 | FixedCompleteGoldenBytesAndShaAreIndependentOfTheProductionWriter；FixedScalarBytesPreserveOrdinalCodeUnitsAndNullableStringMarkers；CanonicalIntegers／CanonicalRationals；EveryNoncanonicalInteger／Rational；ScalarLimits。 |
| ②技术完整性 | CorruptionIsDetectedAtHeaderMetadataBodyAndSnapshotBoundaries；EveryTruncationOfTheIndependentSnapshotReturnsNoPartialValue；ExpectedIdentityLengthAndFullDigestAreRequiredEvidence；ThreeGenerationsAndUnknownBusinessSchema。 |
| ③目录与声明 | MissingDeclarationsNeverBecomeAnImplicitEmptyList；DirectoryMustExactlyCoverTheRequiredContractsInOrder；IndependentlySealedInvalidDirectoryOffsetsAndDigestsAreRejected；StoredRecoveryUnion；SliceRequirements。 |
| ④自足索引 | ThreeGenerationsAndUnknownBusinessSchemaReadWithoutAnAncestorFile；SelfSufficientIndexRejectsBrokenChainsAndCurrentSelfReference，覆盖断代、分叉、重ID、错父、旧摘要缺失／零长、本代自引用及封套身份错配。 |
| ⑤用途与隔离 | BindingKindsRemainDistinctAndPlayerPurposeRejectsCandidateReferences；FrozenInputHasNoWritableOutputAndStartupSummaryHasNoBodyOrBusinessObjects；RequirementsUnionPreservesFirstOccurrenceAndCompleteComponentIdentity；CandidateRevisionAndOrderedSourceNotes。 |
| ⑥流与失败 | NonSeekShortReadStreams；EveryReadPhase／EveryWritePhase；UnreadableUnwritableAndNullRoots；LimitsAreCheckedAgainByEveryEntryPoint；HostileDeclaredLengths；OverflowingHeaderTotals。 |
| ⑦回归与规模 | IncreasingDirectoriesAndBodiesRespectExactByteBoundariesWithoutAGameplayCap；StartupStreamingAllocationsDoNotGrowByASecondBodyArray；完整旧2145多重集合、453项范围和meta核验均通过。 |

## 基线、冻结及静态检查

before 原样继承已接收 demo-013-scope.json.after.files 433项，原 UTC=2026-09-21T12:15:10.9719792Z。
原 scope SHA=d110349421486511971eb1a6c49b6adce38b9685f62e03db097643d4c9a136fb；433项规范SHA=2f695856ae72d8d7a8582d6b51e3480ded3913dbd006970c052180bf8c2c851e。
真实 startedSnapshot UTC=2026-09-21T12:54:49.1684440Z，433项与基线完全一致，C十条新路径全部不存在。
规范均为 Ordinal 路径＋TAB＋小写SHA＋LF，UTF-8无BOM；同批许可新增单列，不改原 before。
原全Assets241个meta路径／GUID保持；最终251个meta中的十新GUID全部唯一，五份本包meta均为完整11行MonoImporter。
旧 FMDemoB09-EditMode.xml SHA=d0aea4d0a22a3de60cf7f6bb560a64ccf98e5b0cf8e0bbeeb97b7e2d7ffabd22。
已实读原2145条全部Passed、2140不同fullname；完整Ordinal fullname＋TAB＋重数＋TAB＋Passed重数＋LF规范SHA=d13c918425980cdac786df6a24d366d6d4c4c87ace615fb9cddf33d07792595c。
CODE_READY UTC=2026-09-21T13:15:54.1682252Z；五源码规范SHA=d83d26cb58547bc121d8328563b0129c0188bed17c66e2111bd022da9f489ff0。
五源码1778行；此时旧433项保持，现场443项＝原433＋B5＋C5，规范SHA=de16f2a6b7230c633a24dd78f529b0adda00f8519107dc1c1ba1c237b24bb96d。
逐源码SHA／行数、完整before、实际startedSnapshot及冻结记录见scope；五C#实读无行尾空白／冲突标记，git diff --check退出0。
RUN by C：只读字节、源码、范围、最终／旧XML及原始B回执检查；NOT RUN by C：Unity编译／EditMode／PlayMode／Player构建。

## 修正、最终冻结与范围

首轮编译因014测试构造参数错误失败，由B只改自有测试；C四Core及本包测试没有编译错误。
首轮EditMode共2426条，2420 Passed／6 Failed：014五条L3夹具缺显式null由B修；015A的170条通过，唯一失败为分配计数观测。
本机GC.GetAllocatedBytesForCurrentThread对已分配并保留的2MiB正文给出差值0；C仅修正测试观测及本测试未用import，四Core SHA始终不变。
替代断言直接记录真实Stream.Read所接数组的引用、长度及复用次数；检查摘要只用一个8KiB正文缓冲，完整读取保留同一读入数组，不复制第二份正文。
没有跳过该测试、放宽正文缓冲目标、使用计时替代内存证据；该检查不声称测得整个托管堆峰值。
Unity曾将十个新meta由2行占位扩成11行MonoImporter；测前守卫在启动前拦截一次，核原占位精确SHA／GUID后重新冻结，未额外启动Unity。
全部失败日志／XML、当次通过编译日志、原始命令／回执及历史冻结保留在scope；C逐项解码复核完整字节数与SHA后接收。
第二次C CODE_READY UTC=2026-09-21T13:43:21.3619272Z；五源码规范SHA=c07edcd5db2767d0aed63ed0d681f9a5bd634fe213261907a0dc149f6b3f96be。
最终共同十源码规范SHA=b4e5b8b8df5d0f41a4f6225bbb29301fe9d1978094a796a98be2907afc958750；此后全部源码保持。
本包五C#共1801行＋五meta共55行＝1856≤2600；全部保护输入保持，最终新增路径恰014十项＋015A十项。
最终453项规范SHA=7dec93b9baef37c7196372087df1089ea49a6b27bcd37a145732bfae652202ae，原433项逐项完全保持。

| 本包源码 | 行数 | 最终SHA256 |
| --- | ---: | --- |
| Core/SaveEnvelope.cs | 152 | f8582cf3e9f059e3fc5e30635de1073650ff4639b662605e61a667ff3a79bdb5 |
| Core/SaveEnvelopeRequests.cs | 93 | 9f5286125fa7ac9286fb36c9f57f90ce64e3fccc596d22c77e11e775260ade06 |
| Core/SaveEnvelopeCodec.cs | 567 | c617a4f568594504159ab6a5ad4e047830da2307885949dae3cdc05604f24a14 |
| Core/ExactSaveValueCodec.cs | 98 | 2baa45d3d0d5524fd9f209861282c4e13dba609971eb16cd3b71e5e3d86bb125 |
| EditMode/FightMatch/SaveEnvelopeCodecTests.cs | 891 | 5daea0301e4076a22c5fc5abf908600e8389509c8c06bf6cb618c296e29efc58 |

Core前缀为Assets/Scripts/FightMatch/；EditMode前缀为Assets/Tests/。五meta最终SHA／GUID／行数均完整保留于scope.metadataVerification。

## 最终验证及失败记录

B唯一执行者：task=01a0c3c3-e163-7f01-b639-ca1f16e1a99d；原turn=01a0c404-71c6-7003-a4ed-241dcde05bd6。
各次均以Get-Process核排他，Start-Process Hidden／PassThru／Wait，WaitForExit后读取实际Process.ExitCode；未杀Unity或用即时LASTEXITCODE推测子进程。
以下均为2026-09-21 UTC；scope保存完整实际PowerShell命令、初始／完成exec回执、六条CommandExecution以及各次冻结SHA。

| 运行 | PID | UTC起止 | 真实退出 | 原始CommandExecution |
| --- | ---: | --- | ---: | --- |
| 编译1：014夹具失败 | 43064 | 13:22:14.1464285→13:32:23.9448724 | 1 | exec-59291004-f8fe-4fe1-ac1e-45ba53f4044b |
| 编译2：通过 | 48828 | 13:34:55.4894257→13:35:14.3894830 | 0 | exec-263ea57c-766c-4e7b-adf9-621daabdc9e6 |
| EditMode1：6条失败 | 39380 | 13:38:13.5900897→13:39:01.2747934 | 2 | exec-3b7c3d8e-916c-48ff-8710-5e80549842b2 |
| 最终编译3：通过 | 29320 | 13:45:09.1791764→13:45:21.2384561 | 0 | exec-a1a9c864-40ba-40d8-81db-95262bd55629 |
| 最终EditMode2：通过 | 51844 | 13:45:53.6779293→13:46:38.8239555 | 0 | exec-4abcc726-fcd8-4e6d-ac3d-4115b67fa877 |

另一次exec-7171c8a8-b60c-4c83-91a4-38ffd61ebc1f仅meta预检失败，launchedUnity=false，不计作Unity运行。
C通过read_thread实读B原turn，六条marker的ID／状态／退出匹配，最终两次完整命令和输出逐字匹配scope压缩原记录。
最终日志C#错误／警告均0；验证后Unity进程0。首轮日志同一CS1729出现3处，不误称三个独立编译缺陷。
命令参数如下；实际执行由B的Start-Process包装，C未执行这些Unity命令。

```powershell
& 'D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe' -batchmode -nographics -quit -projectPath 'D:/Unity/UnityProj/FightMatch' -logFile 'D:/Unity/UnityProj/FightMatch/Logs/FMDemoB10Compile.log'
& 'D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe' -batchmode -nographics -projectPath 'D:/Unity/UnityProj/FightMatch' -runTests -testPlatform EditMode -testResults 'D:/Unity/UnityProj/FightMatch/FMDemoB10-EditMode.xml' -logFile 'D:/Unity/UnityProj/FightMatch/Logs/FMDemoB10Tests.log'
```

最终2426/2426 Passed＝原2145＋014新增110＋015A新增171；Failed／Skipped／Inconclusive均0。
C逐Ordinal fullname核原2145条／2140个不同名称的次数及每次Passed，差异0，旧子集规范SHA仍为d13c918425980cdac786df6a24d366d6d4c4c87ace615fb9cddf33d07792595c。

| 最终原始产物 | 字节数 | SHA256 |
| --- | ---: | --- |
| Logs/FMDemoB10Compile.log | 61385 | e9810674d27cc3441f5a711bdff0fcfe0230d8728c492a1b3ee46faa855b1ba5 |
| Logs/FMDemoB10Tests.log | 89006 | ff5f36f62c330d74acd10975404d065a6586568a5e1e9469bc6b7933a5fa4095 |
| FMDemoB10-EditMode.xml | 1600224 | dfb08fb27958836e521021293faf502aba76a18d56873977ba1b83941497dc5a |

## 字节与分配边界实测

| 切片数 | 正文字节 | 元数据字节 | 整份字节 |
| ---: | ---: | ---: | ---: |
| 1 | 0 | 204 | 264 |
| 10 | 10240 | 1428 | 11728 |
| 100 | 409600 | 14028 | 423688 |
| 1000 | 1000 | 143628 | 144688 |

每例以恰好整份／元数据预算成功，整份预算少1字节即Limit；这是递增技术样本，不是玩法切片或回合上限。
2MiB主切片＋1字节次切片：摘要正文缓冲1个、8192字节、实际257次读取；完整Read保留2个实际读入数组、合计2097153字节，并核原数组身份及切片SHA。

## 证据读取与交回

scope保留完整433项before／原时间、真实startedSnapshot、自身原／新冻结、共同历次冻结、453项after、逐名用例和保护输入；重复快照可由已保存清单及newMetaFiles重建。
rawValidationEvidence采用brotli+base64，解码为UTF-8 JSON，49883字节，SHA=e29d0df6eeba1b8b31c748ae075c7f8858594cf385054ec1066e6d4eb8b740ee。
failedAttempts中的日志为gzip+base64、首轮XML为brotli+base64；按各项encoding使用System.IO.Compression.GZipStream或BrotliStream解压Base64字节，核原bytes／SHA后读取。
首轮编译日志42394字节／e0df3543a9687864367b3b6825f7289c5d20084b0d3a9efd2c69c52e3ce03c20；首轮XML1605772字节／a1d5cda16db11a8cefedcda07607da589417af298b8b61067e27b9a35a898ad5，均已无损复核。
范围文件≤400KiB、报告≤210行；仅以自身七项验收和同版证据交回，不宣称015B、真实保存提交、发布可信或H10已完成。
正式报告及scope同时交R与SD00后结束本原turn并停改；R等双方原实施turn completed后独立审查。
