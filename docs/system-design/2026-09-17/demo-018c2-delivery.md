# FM-DEMO-018C-2 交付报告

状态：COMPLETED；等待 R 独立审查，不自判 ACCEPT。候选应用层接线完成，不代表 Demo 已可玩或后续包已放行。

## 身份、起点与明确授权

- 原任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；原实施 turn：`01a0c9f5-5dd0-77e0-b085-fcbf01b451ec`。
- 第 220 节续接的准确 turn：`01a0cc13-c239-7fb1-a143-8a1dfe6dd8c1`；原生 task_started：`2026-09-23T02:24:02.160Z`。
- 原 turn 未见 task_complete／turn_aborted；不推断具体停止原因。保留原源码、原冻结文件、原证据根和原始命令记录，没有把中途成果当成新基线。
- 继承 C1 最终完整 550 行，canonical SHA256：`af0036b6c2938694a6cf73cbcd171a59db978476860080a83e5372c6075e116a`。
- 原 capturedAtUtc 原字串保持 `2026-09-22T15:41:49.2313950Z`；§217 的六位小数字串表示同一时刻，不以新时间覆盖。
- C1 scope SHA256：`954d76d6d25216483487d0ac0d968f876d594138e4f5bc0bf761f18aa2475e73`。
- 冻结 §209–212 SHA256：`14157867553e4c18ca2933554e2bd4966b01fd1cdcdd2de7d5a02e7f22e71d8e`；§217：`584b272d8b7e270cdf1b141426d0aa33360f2837612b557597f939b4b34f27ac`；协调稿未修改。
- 证据根：`TestArtifacts/FMDemoB14C2/bf4b05e72b854bd9a5711ca42d609794/`。原 baseline.json SHA256：`2757b4294635e0bd4f66556c1dc5d3d21e374a8246db337257814daf17c98fa8`。

**用户明确批准的一行范围例外（[§224](system-task-packets.md)）：** 首次全量发现旧 `QFrameworkIntegrationTests.ExistingPureAssembliesKeepTheirCompiledReferenceBoundary` 禁止 Core.Tests 引用 QFramework，与 §211 的新引用及真实 Command／Query／Event 测试冲突。用户于 `2026-09-23T02:57:28.916Z` 明确回复“允许这一行修正并登记范围例外”。

仅在 `Assets/Tests/EditMode/FightMatchQFramework/QFrameworkIntegrationTests.cs:182` 将断言中的 `x == "QFramework"` 改为 `(x == "QFramework" && name != "FightMatch.Core.Tests")`。Core／Platform 对 QFramework 的禁止，以及三个程序集对 UnityEngine／UnityEditor 的禁止均保持；没有删除、跳过或重命名测试。因此实际范围是原 550 项中 **548 项逐字节保持，2 个获准旧文件变更**，不是原包未经例外时的 549。见 [原生用户授权](../../../TestArtifacts/FMDemoB14C2/bf4b05e72b854bd9a5711ca42d609794/native-user-approval.json)、[逐字节变更记录](../../../TestArtifacts/FMDemoB14C2/bf4b05e72b854bd9a5711ca42d609794/approved-boundary-exception.json)。

旧文件起始 SHA256：`a9feb087ec5a2486ab325a793b10d4afebc6d4bc6600c35a50159a2a21be1129`；最终 SHA256：`7d7a5712b1184500dcd8349b1542ace0c66c0e8d98c3699d1e622fa74ad2c580`。

## 交付与责任

- 新建 Application 的五生产 C#、纯 C# asmdef、三测试 C# 及 Unity 生成的十份 meta，共 19 新 Assets。
- 原测试 asmdef 仅追加 `FightMatch.Application`、`QFramework`；另有上述唯一获准旧测试断言变更。
- Runtime 独占 store、不可变完整 View 和单办理槽；Model／System 引用同一 Runtime，十个消息只代理入口。
- 原 Lookup 优先于新操作门、ExpectedCommit 和 builder；同 ID 异字节冲突，旧结果关联到明确的 LookupViewCommitId。
- builder 仅构造一次；原候选在 Prepare 前失败仍留槽。真实 M12 metadata 交给六片 Encode，Write 前即时核 current／backup／pending 能力。
- 已成事实立即保留，完整最新头经 Decode、Descriptor 核对及 WithVerifiedRecovery 后才发布；保存事实与 Ready 分开。
- Observed 原候选只经 C1 公开接口读取、Decode、原身份接回或明确结束；没有重构、重采样或预先公开未提交结果。
- S17 保留 H04 已发布胜利路由，H06 用原 ReservedOperationId 作下一独立 Submit。实例事件在 store 门外、应用重入保护内发布；监听异常不撤销提交。
- Deinit 先核线程／受理状态，再关闭自有租约；旧对象 Disposed、磁盘 pending 不被取消，新 Interface 从 Unconfigured 开始。上游实例订阅未自动清理的事实保持。

## A01–A09 见证

以下 49 个新增测试全部 Passed，完整名称、结果和持续时间见 [测试逐项记录](../../../TestArtifacts/FMDemoB14C2/bf4b05e72b854bd9a5711ca42d609794/test-verification.json)。

| 条件 | 实际证据 |
| --- | --- |
| A01 | 明确 CreateNew／Existing 缺失与空目录；合法六片首次发布；旧五片 MissingApplicationRecords、坏业务拒绝；current、单独 backup 能力、磁盘 pending 能力拒绝；租约所有权及输入／返回集合冻结。 |
| A02 | 后继头下旧意图＋null builder 返回原 Commit；同 ID 异字节冲突；真实 rollback 后 Superseded 关系绑定发布 View；未知头留旧事实，未命中为 ResolutionRequired。 |
| A03 | 一次 builder、Success 后修改原 mutable resultInput 无效；拒绝／抛错／null／缺 builder／Propose 拒绝／数学 Limit；Prepare 前 I/O 失败和 Encode Limit 保留无 ticket 候选，新预算 Retry 不重构；pending-ticket 缺能力零 Write。 |
| A04 | 真实 Snapshot.Flush.after 的 SaveFailed、Marker.Promote.after 的 CommitUnknown；原 Lookup／Resolve／Retry／End；同槽身份、原 bytes 和单次 marker 提交；未成确认不变新操作或 NoEffectFinal。 |
| A05 | marker 已确证后，观察、Load、坏业务 Decode、能力及共享预算失败均 CommittedRestoreRequired；无半发布，新预算 Resolve 不重写。独立目录生成真实更晚 M12 头并注入外部磁盘前进，Resolve 加载最新头再查原记录。重复 Restore、事件内只读／Busy／Deinit 拒绝、监听中断均有断言。 |
| A06 | 真实目录中完整 work／snapshot／一致双副本，换新 Architecture／store 后原 Commit、Generation、Parent、OperationIds、完整 bytes／SHA 保持且 builder=0；坏业务／残片／多组／观察后变化拒绝；指定组 End、删除中断后的新观察、已成不删和续办失败的原槽语义通过。 |
| A07 | 合法领域 fixture 的真实胜利 H04→完整 AwaitBaseSettlement 发布→H06 独立 Commit；错误 reservation 拒绝、查／重试 H04 不结算；H06 故障保留 H04，恢复后两原对象可查；跨实例恢复路由不自动调用结算负责者。 |
| A08 | 实际 SendCommand／SendQuery／实例 Event；Model 与 System 同一 View；十入口 WrongThread 零 I/O／builder；重入 Busy；正常关闭可重开且 pending 不删；旧 System 拒写、新 Interface 无旧 Model；lease close 异常原样传播并保留诊断。 |
| A09 | 最终编译及全量 EditMode 实际 exit0；3068 全 Passed，原 3019 fullname 的 Ordinal 多重集合完整保留，49 新增全 Passed，零失败／跳过；同版源码／30 DLL/PDB、范围与完整归档核验通过。包含上文明确批准的一行边界测试例外。 |

## 真实执行与失败保全

固定 cwd：`D:/Unity/UnityProj/FightMatch`；exe：`D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe`；exe SHA256：`ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895`。

每轮检查无 Unity 冲突，用 Hidden／PassThru 记录真实 PID，WaitForExit 后先落盘实际 Process.ExitCode，再收产物。标准 argv 如下，完整绝对 argv、UTC、stdout／stderr、源码／程序集前后副本在各 runs 子目录：

- 编译：`-batchmode -nographics -quit -projectPath D:/Unity/UnityProj/FightMatch -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB14C2Compile.log`。
- 测试：`-batchmode -nographics -runTests -testPlatform EditMode -testResults D:/Unity/UnityProj/FightMatch/FMDemoB14C2-EditMode.xml -projectPath D:/Unity/UnityProj/FightMatch -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB14C2Tests.log`；无 `-quit`、filter 或 `--b14c1-*` 参数，不依赖作者 baseline。

| 轮次 | PID | 实际 exit | 结果／处理 |
| --- | --- | --- | --- |
| compile-01 | 19436 | 199 | LicensingClient IPC 超时，尚未编译；日志共享冲突导致收证脚本末尾报错，真实退出码已先保存，原始日志及后续稳定副本均保留。 |
| compile-02 | 31568 | 1 | IPC 已连通，但缺有效许可证，尚未导入。用户随后恢复许可证并关闭 Editor。 |
| compile-03 | 26400 | 1 | 新测试夹具的 TrackingStream 命名空间和 System 字段遮蔽错误；仅修新文件。Unity 首次生成十个 GUID。 |
| compile-04 | 26848 | 0 | 编译通过；十份新 meta 的 Importer 字段在此轮补齐，GUID 不变，未声称 meta 全程字节不变。 |
| tests-01 | 28464 | 2 | 3065/3068 Passed；两处新增测试未准确到达 Encode／Load 预算注入边界，已按实际存储阶段修正；另一失败为获准修正的旧引用断言。失败 XML、日志及当时源码完整保留。 |
| compile-05 | 2488 | 0 | 最终有效编译；`2026-09-23T02:59:01.1584934Z` 至 `02:59:36.5899152Z`。 |
| tests-02 | 36484 | 0 | 最终全量 3068/3068 Passed；`2026-09-23T03:01:23.2819678Z` 至 `03:03:21.9704188Z`。 |

首次失败前的部分源码及后续每版均冻结。续接中还修正了 builder 数学异常保留 Limit，以及 QFramework Command 显式接口调用写法。所有修正均在最终同版验证之前；已有效且仍适用的 C1 六进程证据未重跑。

## 范围、同版及归档

- 原 550→最终 569 实施；Scripts＋Tests 536→555；完整 Assets 563→582；唯一 GUID 300→310。
- 五生产 C#：911 行；八 C#：2322 行；收证帮助：127 行，分别低于 2000／3800／280 行上限。
- 548 个非变更旧实施文件逐项保持；217 项受保护输入及原 300 份 meta 保持。没有改 Core／Platform／QFramework 上游实现、旧 artifact、依赖、设置、场景或协调稿。
- 最终实施 canonical SHA256：`e5eb7e40fb8beb38230af4c758c19c826f4671155756b02e9d4eb1fc578bbc69`。
- 30 个相关 DLL／PDB canonical SHA256：`68f3546cd438eaeecafc059741234d1226ac6974f2788843b8897fada61c2f86`。最终编译后、测试前后和当前均一致。
- 最终 XML SHA256：`f8706327b7ad8f125f23ebb38f512380d36e1f99d4aefa177dc27c5e13fb3955`；基线 XML SHA256：`ee1bbd2dff1532050d5e9b35a9ba3b5443a0d775b4f25149d398aff13d5e915a`。
- 130,077 个旧证据文件完整 SHA／长度审核无改变，canonical：`ff88c8acaef983ae748183530b49b1b2f79eb6e73f8832bc59cc8eb4717175be`。审核完成于 `02:55:34.9281657Z`；最终测试后复查全部旧路径、长度及 LastWriteTime 保持，复用该有效字节审核，未重复全树哈希。
- 两次全量自然产生四个新 B12／B13 Guid 根，共 2,846 文件，完整纳入；没有复制旧 artifact 树。
- 独立枚举实际源、payload、ZIP：**16,226 文件、912,627,033 字节、4 个空目录，零遗漏／额外／重复／字节差异**；拷贝前后源稳定。
- 三方文件 canonical：`bf833dcd83cd014548751f77a3052655f73b3a34b1e4bd3ec22cbc95d69c619f`。
- ZIP：`archive/evidence.zip`，188,478,528 字节；SHA256：`dd3c9136c1b585d3db272fb456f3c5b8b36fd36c7454bf311cc8d82c9e85e899`。
- [完整 scope](demo-018c2-scope.json)：743,521 字节；SHA256：`8b54e95a80d8d2f3008205048ed8fcfdd50735e5664caf413788a570751d5758`。原 550 与最终 569 行均直接保存。
- [无损外置注册表索引](../../../TestArtifacts/FMDemoB14C2/bf4b05e72b854bd9a5711ca42d609794/archive/registry-index.json) 保存完整旧证据前后、Assets／GUID／保护输入、全部测试和三方归档清单。Brotli 4,511,123 字节；解码 128,774,871 字节，逐字节往返相等；压缩 SHA256：`e724aa0824813cbcadcf6ff7d86df737096481251052fc194e52baa7417c7308`，解码 SHA256：`4df26af6affbf660f81074b10811eb154b2867f0efab395e8d96ada4c640894f`。
- archive／late 子树及两份正式报告为明确自引用排除；ZIP／注册表、后续原生事件和最终报告 SHA 由 [最终绑定](../../../TestArtifacts/FMDemoB14C2/bf4b05e72b854bd9a5711ca42d609794/late/formal-bindings.json) 独立绑定。原生 CommandExecution／FileChange 只读导出，不导出推理内容，不用 chunk_id 代替退出码。

## 限制及 NOT RUN

A06 是同一 Unity 测试进程内的真实文件系统与新 Architecture／store 实例；不称新的跨进程试验。物理跨进程能力沿已独立接收的 C1 证据，不重跑六进程或旧强杀矩阵。仍无合法 Link 成功 Run fixture，不伪造成功见证。

NOT RUN：交互式 Editor Test Runner、PlayMode Domain Reload 开关矩阵、真实触控、新强杀／断电矩阵、019／024／025／G2、场景、正式 PlayerSave／发布及 029 Demo 体验。未创建额外验证工程，未安装 SDK／依赖，未写 Git，未调用外部模型／代理，未向其他任务发送通知。

本 formal final 后停止源码及正式报告修改；SD00／R 按准确续接 turn 的 completed 与两报告核完成门，后续包不自动放行。

