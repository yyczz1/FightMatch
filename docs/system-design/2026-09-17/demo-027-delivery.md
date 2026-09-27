# FM-DEMO-027 · 已提交事实播放与中断恢复

作者状态：COMPLETED（等待独立审查，不自判 ACCEPT）。本次 turn：01a0ce47-0fb4-7c43-9717-1085de8fac27；local / gpt-6-astra / max。

授权：system-task-packets.md r128 §256–259；冻结片段 SHA256 aa1219c85e2ebc655dedc7fdf6f4479d74b92f2318cf58a0d72c1a9a2ede6c21。025-P1 已先完成、验证和冻结，本阶段继承其全部产物；不依赖候选内容批准。没有 Git 写入、代理、外部模型或协调通知。

## 范围与继承

入口为 P1 完整656项，canonical=9c34b62661ae2fb8b8f5bf12ae4a51ea675c74b860f443a8957523e061463280；未重捕获更换起点。

新增三生产C#：CandidateBattlePlaybackFrame、CandidateBattlePlaybackController、CandidateBattlePlaybackView；三测试C#：CandidateBattlePlaybackTests、CandidateBattlePlaybackPanelTests、CandidateBattlePlaybackTestData；各配Unity自然meta，共12项。

唯一旧修改 CandidateBoardElement.cs：13增4删，仅新增可清除的只读绘制覆盖和Draw读取。构造器、选择、指针、坐标命中、提交、捕获及几何处理原文保持，旧meta保持；17行低于90行预算。其余655项保持。最终668实施项、681全部Assets文件、361唯一GUID。生产348/1800行，测试498/2200行；既有两个E17 helper仍159/360行，未修改。

全组相对B16原637项：仅Core.Tests.asmdef追加Content引用、BoardElement绘制接缝两项获准改变；635旧项保持、新31项。P1源码、两报告、完整证据子树及helper已纳入027入口/出口保护。细目见scope及review-inputs/group-inheritance.json。

## 行为与公开入口

CandidateBattlePlaybackFrame / CandidatePlaybackActor只持可丢弃显示值和原事实引用：精确HP、意图游标、路线、PendingLink、阶段、原Index及反馈文本；不构造BattleSnapshot、不推进规则或随机。

CandidateBattlePlaybackController(input,system)订阅既有Input Controller的PresentationReady、ResultReceived、Changed；提供Advance(double)、SkipToFinal、ReportSchedulingFailure(Exception)、Dispose及只读Frame/LatestView/Original/诊断。只有PlayOriginal、当前核证Ready头及原AttemptId+SceneRevision+OperationId匹配才能开始，并核对OriginalLookup中的前后快照、事实引用。

CandidateBattlePlaybackView.Attach(system,budget)组合原InputView；提供Advance、SkipToFinal、Close、Dispose和公开等效取消入口NotifyLowMemory/ReportSchedulingFailure。实际Label显示HP、意图、动作与阶段；棋盘只读覆盖不参与输入权限。BoardElement新增SetPlaybackOverride/ ClearPlaybackOverride及只读PlaybackOverride。

时间线为一个180ms初始已提交节拍，再按原Index各保持一个180ms节拍；末事实也完整显示后才完成。UI Toolkit schedule每16ms推进，手动Advance只接受有限非负delta；180ms仅技术试调，不是手感验收。零事实分支直接完成，但没有伪造零事实成功DTO作运行见证。

DirectAttack取原HpBefore/HpAfter/Crit；EnemyIntent取原Charge/Strike、Damage和Cursor；Stage按原顺序处理消线、固化、PendingLink、翻面和阶段。固化路线只取原After.Board中匹配Pair的路线。跨面或不足以表示中间状态时保留事实反馈并显示已知终态，不补算规则。

正常完成及明确中断均先QueryView最新头、清草稿和覆盖、再原样回报自己持有的原token并刷新可用性。新Attempt/Scene或不同当前token使旧frame失效；只变Commit不作为完成条件。重复原结果的RebuildLatest可处理仍等待的当前令牌，即使没有本地播放进度；不是“播过集合”门禁。

Close/Detach/失焦/低内存等效入口/调度异常等效入口停止并失效自己的调度，释放订阅；旧A1收尾不无条件清A2门闩。唯一无条件019.RebuildLatest调用在明确的新页面Attach接管。调度异常保留类型与消息。未Ready显示实际保存阶段及不可用原因，不回装旧状态。

## 验收证据

| 条目 | 实际结果 |
| --- | --- |
| P27-01 | 真实019攻击、原快照/事实引用；逐Index推进。L1：敌HP15→0、W100→95、RouteLocked、PhaseSelected；L3：敌HP20→4、蓄力、W100→95、TemporaryRouteRemoved、PhaseSelected。首/末179+1ms边界、零delta和非法delta核对。纯播放不改变业务头、随机引用、存储调用数或磁盘字节。 |
| P27-01暴击 | 沿上游候选C=1/1000、f=0，在预声明SC01 seed [0,4096)／sequence=54范围搜索真实分支；首见seed=109、实际PCG字2800022000。合法退出/重新入场后由真实019提交产生暴击，再经真实panel播放“暴击”及原HP。无注入字/结果；这是分支见证，不是概率、正式参数或无条件成功声明。完整48字节源与实际词列留在XML/摘录。 |
| P27-02 | 当前令牌重复Query/Retry/Resolve不重播或双回报；本地零播放进度时，Query原结果仍能重建并回报等待的原令牌。真实Published回调中的Busy不启动，Ready原结果送达才播放一次。 |
| P27-03 | A1完成后A2挂闩、旧Advance/Close/Dispose/旧Input查询/原A1回报不解A2；旧A1仍播放时遇到真实A2或新Attempt也只重建最新头。Skip、Close、Detach、真实Blur、低内存等效、异常等效六分支均验证先清覆盖再回报，以及无重复伤害/奖励/写盘。重新Attach注销旧Controller。 |
| P27-04 | 实际WindowsEditorSaveStorage的Snapshot.Flush.after→SaveFailed、Marker.Promote.after且禁发布探测→CommitUnknown；旧头零播放，原Prepared请求/字节Retry或Resolve后一次播放、一次marker提升。Unknown中Close保留实际状态。真实Rollback、新页面接管、新实例既有存档恢复和观察到的pending续办均只重建。 |
| P27-05 | 真实EditorWindow/Editor panel正尺寸160×160、池化PointerDown/Move/Up通过既有Controller提交；实际Label和覆盖随事实变化，结束清覆盖并可再次画线。L1第二次攻击后WonPendingSettlement、W95/100、双敌0，基础奖励仍0。另有实际UI Toolkit调度推进见证。 |
| P27-06 | 无filter全量3420/3420 Passed，失败0、跳过0；原3402（B16 3304＋P1 98）具名多重集合及测试源码/断言保持，新增18（16个panel协程、2个新实例恢复用例）。668/361、同版源码/DLL/XML和旧证据保全审计通过。 |

新例的精确fullname、结果、时长和原输出从最终XML提取到review-inputs/playback-evidence.json，未另运行探针。无图形模式只沿旧夹具精确期待Show三条/Close一条固定原生消息，无全局吞日志或Skip/Ignore。

## 实际验证

- compile-01：PID32148，实际退出0，但全日志保留新文件导入前两条CS0246，证据门拒绝放行；后续编译和自然meta导入成功。该轮完整保留，未把最终exit 0替代日志检查。
- compile-02：PID37900，退出0、编译错误0，为自然导入后的干净确认；随后只在本包新文件收紧关闭顺序/结果归属并补测试。
- compile-03：PID21076，退出0、编译错误0，验证生命周期修正；之后新增真实SC01暴击见证并按真实pair0标识修正新显示断言。
- 最终compile-04：PID17532，2026-09-23T13:58:26.3886318Z→13:58:44.3681600Z，实际退出0、编译错误0。
- 唯一tests-01：PID18668，2026-09-23T13:59:53.7528398Z→14:09:28.3648985Z，实际退出0；3420/3420。无filter、测试无-quit、无选择用例环境参数。未另重复P1阶段验证或B16独立历史矩阵。
- 固定Unity 2022.3.18f1，EXE SHA ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895。每轮核无Unity后Hidden启动；真实argv/cwd/PID/开始结束UTC/Process.ExitCode/stdout/stderr及稳定日志见runs。
- 最终实施canonical：243121465a9724a87e3e93f570273a55a6f98e08085591b90974c5091389fab4；36 DLL/PDB canonical：bad0f4469095e00607da18d9408b3358865de0a0d463631d23b3a412a2f829b4。最终编译/测试/当前同版，测试前后源码一致。
- FMDemoB17P27-EditMode.xml SHA：c8c3f8e962600d669187ce9efb2d39ccd64e194ffaa158d1dcb54bc88a12a18d。

## 证据冻结与未运行边界

证据根：TestArtifacts/FMDemoB17/62aa18a2eb9443d195296216f0184526/stage027。原起点、每轮真实源码/Assets/保护输入/DLL副本及清单完整保存。328153个历史证据文件入口/出口长度和时间戳一致，包含冻结P1完整子树；历史多GB全字节结论继承，不重复全树复制或哈希。

新自然根B12/41ff896650e042f4a220c6499ae8ccc7、B13/92d2753b8cc2437789706350a2b5df7d完整入档。源/payload/ZIP共14907文件、1303905578字节、34空目录；路径/长度/SHA及空目录集合全等，归档后源稳定，canonical=0134c3639606370b1f5383444f94b1e5aadf74c8c991cdae7808d5cd101811cb。

ZIP=archive/FMDemoB17-027.zip，199471209字节，SHA 1727df06eb1241c03e413ce4a8cd8687bce9c51115a1c9068a5ef228f653a17b。archive/late/本两报告排除自引用并由late/bindings.json晚绑定。原生导出只含准确本次turn的context/生命周期/已完成命令与文件修改事实，不导出推理；task_complete由SD00/R在正式final后核实。

NOT RUN：合法AwaitLinks/全倒补线公开成功见证（沿上游缺口，仅静态核PendingLink映射）；同Scene永久提交（仅静态核不比较Commit）；跨面中间表现、零事实PlayOriginal；未列出的故障交叉组合；实际OS低内存、实际调度器内部抛错；像素绘制、人手鼠标/物理触控、交互式PlayMode、Player build、真机、完整Demo。公开等效取消测试不冒称OS事件或真实调度异常。

正式内容发布、PlayerSave来源与新档桥接：NOT IMPLEMENTED／NOT RUN；025-P1候选坐标/身份/31C/epsilon/新档提案仍待内容审定，详见stage025p1/review-inputs/publication-proposal.md。本包不修改P1提案、不批准内容、不把025整体或B17提前放行；后续025-P2、028、029需各自授权及独立接收。
