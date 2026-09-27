# FM-DEMO-018C-2 独立审查（DEMO-B14C2-R1）

VERDICT: NEEDS_FIX

仅裁决 FM-DEMO-018C-2。范围与有效验证证据通过；存在 1 项 P2 生命周期问题 F1，需要窄纠正。R 未修改实现、作者报告或协调稿，未运行 Unity、项目测试或额外探针，未创建代理、任务或 Git 写入。
审查日：2026-09-23；时间均为 UTC。项目根为 D:/Unity/UnityProj/FightMatch。
R 任务：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；准确续接 turn：01a0cc13-eeff-7ba3-97b0-ea7a0a4dbc86（02:24:13.611Z task_started）。沿原任务、gpt-6-astra／max、local。
原证据根 E：TestArtifacts/FMDemoB14C2/bf4b05e72b854bd9a5711ca42d609794。下文 E/ 均指此根，未重建起点或另换根。

## 1. 完成门与冻结依据

按 [system-task-packets.md](system-task-packets.md) 冻结 §209～212、§217，以及续接 §220、用户单行例外 §224 审查；§225仅登记收件，不改变本次责任。已阅读仓库审查清单、验证规则及实际八份 C#，沿 code-review 的规范／规格两轴独立核对，未启动该技能的并行代理流程。
原 C turn=01a0c9f5-5dd0-77e0-b085-fcbf01b451ec、原 R turn=01a0c9f6-2656-71d1-8fd8-3afa30092791 均保留历史中断状态；没有把日志静止或报告出现当完成。
C 任务 01a0c403-bfa1-7e90-b503-c0fcd61f23c1 的实际续接 turn=01a0cc13-c239-7fb1-a143-8a1dfe6dd8c1，02:24:02.160Z task_started（ordinal 6733）。
R 直接读取该原生会话：03:29:54.277Z final_answer AgentMessage 完成（ordinal 7365，id=msg_01289151d0393480016ab3479e0ac087d0b9cba71e36c20218），03:29:54.340Z task_complete（ordinal 7369）。该 final 非 async 问答，明确停止源码／报告修改并交付下表。
完成门核实于03:36:14Z，随后才进入最终裁决；原生会话路径为 C:/Users/YYC/.codex/sessions/2026/09/21/rollout-2026-09-21T20-49-35-01a0c403-bfa1-7e90-b503-c0fcd61f23c1.jsonl。当前无 wait/read 任务工具，依 §220 用只读原生记录替代。

| 正式实物 | 长度／行数 | 当前 SHA256，与 C final 一致 |
| --- | --- | --- |
| [demo-018c2-delivery.md](demo-018c2-delivery.md) | 12853 bytes；95行 | 8d4b38a634e61dace2eefffe60d63a16e957d4e478e913f0c67c3e262b0763c6 |
| [demo-018c2-scope.json](demo-018c2-scope.json) | 743521 bytes | 8b54e95a80d8d2f3008205048ed8fcfdd50735e5664caf413788a570751d5758 |
| E/late/formal-bindings.json | 20387 bytes | dcda8abc555736bc4568057e8e9a7fcadab58061dc607cf2b73634d8dec013f0 |

03:42:38Z再次按 LF／TrimEnd／单末尾 LF 核冻结条款：

| 范围 | SHA256 |
| --- | --- |
| §209～212 | 14157867553e4c18ca2933554e2bd4966b01fd1cdcdd2de7d5a02e7f22e71d8e |
| §217 | 584b272d8b7e270cdf1b141426d0aa33360f2837612b557597f939b4b34f27ac |
| §220 | e24a0965d3b8d38f463478be83965ad676958aa6375864bdd2f5d2471354f843 |
| §224 | 8aac59482fe5e72393c5dcb8acceaffb0c60ec83b66a87bfb1b7d0004c0b4a8a |

§224与 E/late/section-224.md 的4316字节完全匹配。原派发 r107 全文 SHA=18963d49460d1d315994fa10b13c326de075fe8bf0872cdb41ed8254512bb650；协调稿后续追加不冒作冻结全文未变。

## 2. 唯一待修项 F1（P2）：旧实例再次 Deinit 会注销新实例

定位：[FightMatchDemoArchitecture.cs:20](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/FightMatchDemoArchitecture.cs:20) 的 OnDeinit 无已完成销毁保护，直接调用 runtime.Close。
关联：[CandidateApplicationRecovery.cs:212](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateApplicationRecovery.cs:212) 对 disposed 直接返回；只读上游 [QFramework.cs:117](/D:/Unity/UnityProj/FightMatch/Assets/ThirdParty/QFramework/QFramework.cs:117) 随后执行实例清理，并在123行无条件将该 Architecture 类型的静态 mArchitecture 置 null。

具体输入（同一 owner thread、没有进行中的调用；均为现有公开入口）：

1. 保存 A = FightMatchDemoArchitecture.Interface，正常执行 A.Deinit()。
2. 取得新 Interface B，用合法 storage/profile 成功 Open；B 持有其 store 租约。
3. 再调用仍被保留的 A.Deinit()，随后读取 FightMatchDemoArchitecture.Interface。

预期：已完成销毁的 A 在上游清理前明确拒绝，或至少不改变 B 的当前实例身份；B 的 Model／System／View、待办理槽及租约仍属于可达的当前应用。
实际控制流：A.OnDeinit → A.runtime.Close → disposed 分支直接返回 → 上游 Deinit 清空已空的 A 容器并执行 mArchitecture=null；下次 Interface 因而创建第三个 Unconfigured 实例 C。B 没有收到关闭，仍持有租约。
影响：正常全局入口丢失正在工作的 B 及其 View／原办理槽；通过 C 打开同一 profile 会受 B 的租约阻挡。若上层只保留全局入口，B 的租约也失去正常关闭入口。此处不声称磁盘数据损坏。
这是实际源码的确定性调用链反例，R 依 §212 未执行运行探针；不把静态推导写成已执行失败测试。当前 A08_DeinitLeavesDiskCandidateAndNewInterfaceHasNoOldStateOrGlobalEvents（615～658行）测试正常 A→B 及旧 System 拒写，未再调用旧 A.Deinit。
依据为 §210.13 和 §212 要求独立核生命周期／所有权。当前四项 A08 测试全绿不能覆盖此顺序；没有其他待修项。
修复应落在新应用层的 Architecture 销毁边界，阻止已销毁旧实例进入上游清理；保持 WrongThread／Busy 的先行拒绝及首次租约关闭异常后的原有收尾能力，不修改只读 QFramework。

## 3. 规范与规格两轴

| 审查轴 | 独立结果 |
| --- | --- |
| 规范（Standards） | 白名单、原基线、行数、程序集边界、meta来源、同版证据和归档均核符；§224例外有准确用户授权。无越权源修改、依赖、反射、全局事件注册、外部模型或 Git 写入证据。 |
| 规格（Spec） | A01～A07的实现／有效见证成立；A09有效验证与保全通过。A08正常路径有实测见证，但F1说明重建后旧实例的销毁边界仍有缺口。 |

| 验收项／新增 Passed 用例数 | 代码与证据判断 |
| --- | --- |
| A01／9 | Runtime独占store；CreateNew经NoSave核证才开初始化门；Existing缺失、旧五片、坏业务及current／backup／pending能力拒绝不发布半份；View与结果输入冻结。 |
| A02／1 | 原完整Snapshot Lookup在ExpectedCommit、阶段及builder检查前；同ID异字节Conflict，旧结果带原LookupViewCommitId且不覆盖更晚头；未知头未命中不报NotFound。 |
| A03／9 | 唯一槽先保存原candidate再Prepare；真实metadata由回调传Encode，ticket前失败保留candidate；Retry不重跑builder，八字段resultInput已复制；同budget贯穿，无补建Math预算。 |
| A04／3 | 真实M12故障及原ticket查／续／结束；未确证时拒绝新办理，能力核证后才Write；ConfirmedNotCommitted未被当作新操作或权益最终无效果。 |
| A05／7 | 确证原Commit先保留提交事实，再恢复最新完整头并查原结果；加载／解码／预算失败不安装候选半份；发布后才通知、同头去重、监听异常不撤提交，事件内写入Busy。 |
| A06／14 | 真实目录、新Architecture／store；Read原候选→完整Decode和原身份核对→Resume原ticket；无builder／Propose／Encode重演，坏组／变化／多组及EndInterrupted有见证。 |
| A07／2 | 合法领域fixture真实攻击形成Win；H04发布AwaitBaseSettlement后H06另次Submit／Commit，保留ReservedOperationId与两原对象；H06失败不回滚H04，查询不自动结算。 |
| A08／4 | 实际Command／Query／实例Event、同一View、WrongThread／Busy零I/O、正常释放、旧System Disposed及CloseFailed传播均有见证；遗漏顺序见F1。 |
| A09 | C实际编译／全量测试exit0；3019原fullname多重集合与49新增全部Passed，零跳过；有效源码、30DLL/PDB、XML与最终交付同版。 |

已读取最终八C#全部内容及各次变更；初始生产文件不是重建基线。恢复期间 Encode／恢复预算见证的失败及修正均保留：A03预算改为真实业务Prepare之后的应用Envelope边界；A05在真实Load边界消耗同一调用方Math预算并断言故障已触发，未放宽期望结果。

## 4. 范围、原始起点与授权例外

起点引用已独立接收的 demo-018c1-c1-scope.json（573301 bytes，SHA=954d76d6d25216483487d0ac0d968f876d594138e4f5bc0bf761f18aa2475e73），没有重新审C-1。
原 capturedAtUtc=2026-09-22T15:41:49.2313950Z；550项 canonical=af0036b6c2938694a6cf73cbcd171a59db978476860080a83e5372c6075e116a。R原入口16:34:24～16:34:33Z已独立核550全符。
原536 Scripts＋Tests、563 Assets、300唯一GUID；原Assets canonical=83f89418e74ff7205b0d42a3b5009f382ebb2b76cb5b9e9d5e7ffced166b4d74。C恢复后保存完整550行和原字节，未把中途成果冻结成旧起点。

§224授权原生用户消息：2026-09-23T02:57:28.916Z，ordinal7096，msg_01a0cc32-6154-79e2-ab8e-554a880fea6e；答复“允许这一行修正并登记范围例外”。该问题仅指下述断言条件，R已直接核对问答及实际一行差异：

| 原有获准修改项 | 精确差异及原／终 SHA256 |
| --- | --- |
| Assets/Tests/EditMode/FightMatch/FightMatch.Core.Tests.asmdef | 只在references追加FightMatch.Application、QFramework；584→640 bytes，其余字段／格式不变。beb9f82d8c69a992bf21803a04357694eadb12dac69626e74e9db3adebb9865c → 154205439e7d707c4772b9891f3bd85ebff1179bbe73cfe45cce69983a3a189a。 |
| Assets/Tests/EditMode/FightMatchQFramework/QFrameworkIntegrationTests.cs:182 | 仅 x == "QFramework" → (x == "QFramework" && name != "FightMatch.Core.Tests")；11029→11066 bytes。a9feb087ec5a2486ab325a793b10d4afebc6d4bc6600c35a50159a2a21be1129 → 7d7a5712b1184500dcd8349b1542ace0c66c0e8d98c3699d1e622fa74ad2c580。 |

该旧测试方法／fullname／数量未变；Core／Platform仍禁QFramework，三个程序集仍禁UnityEngine／UnityEditor。其.meta保持48e0d036ab63476d047b6ed373d2708ed265d4f9a066dedcc7acbe5755c292f3。此精确例外不计为越权，也未抹去首轮失败。
另外548旧项逐项不变，canonical=d4dcfa00207019acee1d514e3cbe8103f28d7b6f64b336b9d1fece6d596472a1。19新增Assets准确为五生产C#、三测试C#、应用asmdef及10meta，无其他新增Assets。
10meta首次由Unity compile-03导入生成GUID，compile-04补齐Importer内容；原GUID保持，最终310全部唯一，无手造／改写旧meta。
应用asmdef refs恰为FightMatch.Core、FightMatch.Platform、QFramework，noEngineReferences=true、autoReferenced=true；没有Engine／Editor依赖。五生产C#911行，八C#2322行，证据帮助127行；均低于2000／3800／280限制。
03:40:26Z交付后重新实算：569实施、555 Scripts＋Tests、582 Assets、310唯一GUID；所有长度／SHA与最终scope及实际枚举相符。
最终实施 canonical=e5eb7e40fb8beb38230af4c758c19c826f4671155756b02e9d4eb1fc578bbc69；完整Assets canonical=4915831dfef25a2b4deab054b22b814a0cd969030d22faf7d5e90ae796deccdf。
F1涉及最终 Architecture SHA=a7ee449c612e73f343e6345e88389b686600fe83270ef699e704d7909a05215f；Recovery SHA=561f3b4af575e51ffd1b25c653e09023dabb1ba35385baa1358cf020e188bf16；RuntimeTests SHA=cd2fe460417f862f089ffd76e7a36506b5f9555ce8c0dc6610f10eeccff32546。

## 5. 实际验证、失败历史与同版性

C是唯一Unity执行者。固定exe=D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe，SHA=ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895；cwd／projectPath均为项目根。
R读取每轮真实CommandExecution、进程结果及收证帮助：先核无同项目Unity，以Start-Process Hidden／PassThru启动，WaitForExit后先保存真实Process.ExitCode，再收输出；未以包装器exit／chunk_id代替Unity进程退出。

| 轮次 | PID | 实际exit | 核实结果 |
| --- | --- | --- | --- |
| compile-01 | 19436 | 199 | 许可证IPC超时；包装器后续日志共享读取失败另有保留，非假成功。 |
| compile-02 | 31568 | 1 | No ULF／Token，无有效许可，未完成导入。 |
| compile-03 | 26400 | 1 | 新测试TrackingStream的System字段遮蔽System.IO；错误日志保留。 |
| compile-04 | 26848 | 0 | 当轮源码编译通过，后续修正后不冒作最终同版验证。 |
| tests-01 | 28464 | 2 | 3065／3068 Passed，3失败、零跳过；两个新增预算注入点与旧QFramework断言问题均有原XML。 |
| compile-05 | 2488 | 0 | 最终同版编译。 |
| tests-02 | 36484 | 0 | 标准完整EditMode，3068／3068 Passed，零失败／跳过。 |

compile-05：02:59:01.1584934Z～02:59:36.5899152Z，exit于02:59:36.6570052Z捕获；原生命令exec-b737011c-3554-4170-adb8-47c70b7f385d，ordinal7119。
tests-02：03:01:23.2819678Z～03:03:21.9704188Z，exit于03:03:22.0337305Z捕获；原生命令exec-91d3a93c-a28b-414d-99c3-71d0cee40382，ordinal7163。
编译argv为-batchmode -nographics -quit -projectPath指定项目 -logFile本包Compile；测试argv为-batchmode -nographics -runTests -testPlatform EditMode -testResults本包XML -projectPath指定项目 -logFile本包Tests；实际顺序／绝对参数保存在原生命令和runs记录。测试没有-quit、testFilter或--b14c1-*专用参数。
最终XML：FMDemoB14C2-EditMode.xml，2120102 bytes，SHA=f8706327b7ad8f125f23ebb38f512380d36e1f99d4aefa177dc27c5e13fb3955，与runs/tests-02/EditMode.xml相同。
R直接解析XML：原3019 cases／3014 distinct fullname的Ordinal多重集合完整保留，全部Passed；最终3068 cases／3063 distinct，新增49全部Passed。原XML SHA=ee1bbd2dff1532050d5e9b35a9ba3b5443a0d775b4f25149d398aff13d5e915a；首轮失败XML SHA=8ad40fbb6f3b70db89382579248b60a299f7115e75c08a079591efa7e7ed28c6。
最终compile log：63068 bytes，SHA=b49051d715340bb2c01be529c2020d74cca6054f5998eb4326fbab2125ee5b63；tests log：90474 bytes，SHA=f93d2db7840ec61049cc2836539afc9e0acb2f7a10320fdb09f80477d3d00f10。均与各轮Unity-final.log原字节相同。
compile-05前／后、tests-02前／后四份实际source副本各833项，经独立集合枚举及逐字节SHA比较无漏项、额外或差异；四份569实施均为最终canonical。
compile-05后、tests-02前／后及当前30DLL/PDB canonical均为68f3546cd438eaeecafc059741234d1226ac6974f2788843b8897fada61c2f86；编译前的旧DLL摘要另存，未混为最终同版。

## 6. 保全与归档独立复核

R原入口与续接均保留只读核证。39旧证据根原始130077文件、6722701075 bytes，全局canonical=ff88c8acaef983ae748183530b49b1b2f79eb6e73f8832bc59cc8eb4717175be。
交付后完整重读每个旧文件的SHA，窗口 2026-09-23T03:36:51.1433546Z～2026-09-23T03:48:01.8875671Z；39根的路径集合、计数、字节及各根canonical全部与原入口一致，不仅依赖作者mtime。
旧日志／XML及Tools bin／obj共120项、43432461 bytes，03:39:11Z重新逐项核符；canonical=d7d0a91c55da66c19622f5ed43d10b2475c5eb258c78058811d48e324217f7e2。
原466保护输入由原始完整canonical约束；原生显示截断的清单通过保留原行与三个原协调稿摘要重建，结果仍精确等于f9f42127986a9144a85815999ad4effbc340dc92ecd2b42b7b119a5fc871aea9。03:36:56Z重新核对，其余463全部不变。
三协调稿差异属于SD00：最新FileChange为03:35:37.489Z、ordinal17220、exec-203abe89-efef-412c-88b6-4a2888b53100、SD00 turn=01a0cc52-1d5e-7c52-9cef-f77a68af3bb8，只追加§225／正式收件状态。不是C或R范围污染。
自然新增完整回归根为B12/127177a58068445fa909cf1e121947aa、B12/80699eeda65c44b6a6815b77271805ad、B13/94ed7257fbe64b5d9f0b903abd463175、B13/f8f8fc16cc134329a1b2b1c256bde99a；均在TestArtifacts下并纳入归档，无覆盖旧根或根外散落文件。

R独立枚举实际选源、payload、ZIP并读取所有ZIP条目流：16226文件、912627033 bytes、4空目录，路径／长度／SHA／空目录集合全等，无重复／遗漏／额外项；三方canonical=bf833dcd83cd014548751f77a3052655f73b3a34b1e4bd3ec22cbc95d69c619f。
ZIP为E/archive/evidence.zip，188478528 bytes，SHA=dd3c9136c1b585d3db272fb456f3c5b8b36fd36c7454bf311cc8d82c9e85e899。四个Tools源／配置在完整baseline与逐轮source内保存且与原C1字节相同，没有额外平铺到ZIP根；这不是清单遗漏。
外置完整注册表E/archive/registry.json.br为4511123 bytes，SHA=e724aa0824813cbcadcf6ff7d86df737096481251052fc194e52baa7417c7308；R解压128774871 bytes，SHA=4df26af6affbf660f81074b10811eb154b2867f0efab395e8d96ada4c640894f。
解压后的18个完整JSON对象与各原件逐项语义规范化SHA相同；原130077文件前后清单、完整Assets／GUID、测试及归档清单未裁剪。原550与终569实施列表另直接列在scope。
E/native-events.jsonl的89条记录（74命令／15文件变更）及E/late/final-native/native-events.jsonl的106条（90命令／16文件变更）均逐原始JSON行与正式原生会话匹配；最终导出截止ordinal7346，无遗漏／不同，未导出推理。
最终导出4321773 bytes，SHA=efe2c4350bf2f9a33f29f0bf7c64a64f813b004dc9f894ad48560b10694275f0。其后导出、绑定自身命令及formal final／完成事件另由原生会话直接核实，自引用排除已声明。
E/archive、E/late及两作者报告不递归收入自身ZIP；03:40:26Z重新核最终binding引用的32个不同文件及binding本体，共33个实物全部匹配，13个late文件没有未绑定项。
§224登记后仅补两报告元数据：scope增加三项approvedException登记属性，其余结构不变；之前两报告保留于late/pre-section224-reports。旧final-gate中的旧报告SHA由section224-report-amendment.json及最终binding明确接续，未用旧SHA冒充最终报告。

## 7. 窄纠正任务草案（待SD00签发，R未派发）

Task ID建议：FM-DEMO-018C-2-C1；状态DRAFT_FOR_SD00；单包，无任务组。目标仅关闭F1，保留本次已核通过的行为与证据。
实施仍复用原C任务，独立复审仍复用原R任务，gpt-6-astra／max、local；不创建新任务／代理、不调用外部模型。此草案不是当前实施授权，不在R回合执行。

起点：本报告绑定的最终569项，capturedAtUtc沿作者03:05:13.7509389Z，实施canonical=e5eb7e40fb8beb38230af4c758c19c826f4671155756b02e9d4eb1fc578bbc69；原550历史和§224继续保留。
只读输入：本报告F1、冻结§209～212／§217／§224、两份C2作者报告、八C#及其现有依赖公开接口、QFramework Architecture.Deinit、现有合法测试fixture和C2有效XML／取证方式。
只允许修改两个文件：
- Assets/Scripts/FightMatch/Application/FightMatchDemoArchitecture.cs。
- Assets/Tests/EditMode/FightMatch/CandidateApplicationRuntimeTests.cs。

只允许新建交付：docs/system-design/2026-09-17/demo-018c2-c1-delivery.md、docs/system-design/2026-09-17/demo-018c2-c1-scope.json；Logs/FMDemoB14C2C1Compile.log、Logs/FMDemoB14C2C1Tests.log、FMDemoB14C2C1-EditMode.xml；TestArtifacts/FMDemoB14C2C1/<一次全新GuidN>/下本包完整证据。SD00签发时绑定实际根E1；标准回归自然生成的B12／B13新Guid根按原规则登记并完整归档。
禁止修改其余全部文件，尤其Runtime／Recovery／Messages／Models、QFramework／Core／Platform、旧QFramework测试、全部asmdef／meta、Packages／ProjectSettings／权限／协调稿、原报告和E及全部旧证据。没有新业务源码文件／GUID／接口／序列化／依赖／构建配置权限。
最大源码变化：1生产＋1测试文件，合计约100增删行内；新增1个回归[Test]，不删除／改名／跳过原3068项；报告≤240行、scope≤2048KiB，证据帮助≤280行。超界先报BLOCKED，不自行扩白名单。
行为：已完成正常销毁的A再次Deinit，在上游清理前明确拒绝，B始终是当前Interface；首次CloseFailed后的原有再次收尾仍可完成，WrongThread／Busy先行拒绝及原事件／租约／pending规则保持。只需实例生命周期保护，不引入第二store、静态业务状态或通用框架改造。
回归命名建议：A08_StaleArchitectureDeinitCannotDetachCurrentInstance。保留A，正常Deinit，创建并Open B，保存B的Model／System／View；对A再次Deinit后断言Interface仍为B且这些引用不变、B租约未被关闭、B仍可使用；B自身Deinit正常释放后，同profile可重开。使用现有ApplicationRuntimeRig及真实M12路径，不替换成伪Architecture。
验收：上述新测试修复前实际失败、修复后通过；原WrongThread／Busy／CloseFailed／跨实例候选测试继续通过。除两个目标外567旧实施项逐字节不变，582 Assets／310唯一GUID不变，原3068 fullname多重集合全Passed，加1项后3069全部Passed、零跳过。
C为唯一Unity执行者：每轮确认无同项目Unity；固定2022.3.18f1 exe、cwd和projectPath，Hidden／PassThru、真实PID／起止UTC／Process.ExitCode先保存。复用取证方法，不复用旧运行冒作新源码验证。
验证argv（由批准后的收证帮助传给上述exe；所有输出在本包允许路径，R不执行）：
- 红阶段：-batchmode -nographics -runTests -testPlatform EditMode -testFilter FightMatch.Core.Tests.CandidateApplicationRuntimeTests.A08_StaleArchitectureDeinitCannotDetachCurrentInstance -testResults E1/red/EditMode.xml -projectPath D:/Unity/UnityProj/FightMatch -logFile E1/red/Unity.log；先只加回归测试，保留实际失败XML及源码。
- 修复后编译：-batchmode -nographics -quit -projectPath D:/Unity/UnityProj/FightMatch -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB14C2C1Compile.log。
- 修复后全量：-batchmode -nographics -runTests -testPlatform EditMode -testResults D:/Unity/UnityProj/FightMatch/FMDemoB14C2C1-EditMode.xml -projectPath D:/Unity/UnityProj/FightMatch -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB14C2C1Tests.log；无filter／-quit／专用历史参数。
后两轮应实际exit0、同源码／DLL／XML；保留失败，不重跑旧强杀或C1六进程矩阵。按原完整保全／源-payload-ZIP／注册表／原生事件和报告最终绑定要求交付，不能只交截图或摘要。
交付格式：COMPLETED或BLOCKED、准确原任务turn、两报告绝对路径／SHA、两个文件差异、逐条验收／真实命令和退出、NOT RUN；停止修改，不自判独立结论、不通知其他任务。
回滚边界仅两个目标文件回到本报告SHA所指字节，必须另获实际执行授权；保留全部原证据。未授权任何commit／push／branch／worktree或历史重写。

## 8. 限制与交接

R对Unity、项目测试、运行时反例探针均NOT RUN，依据§212只读要求；本报告的3068通过来自独立核实C的真实运行，而F1为静态控制流证明。
A06是同一Unity进程的真实文件系统及新实例；跨进程物理能力沿已独立接收C1，不称新跨进程试验。交互式Editor、PlayMode Domain Reload矩阵、真实触控、新强杀／断电均NOT RUN；合法Link成功Run fixture仍无新覆盖。
019／024／025／G2、场景、正式PlayerSave／发布、029 Demo体验不在本次验证；不据此宣布018整体完成或后续包放行。
SD00在本R formal final及准确task_complete后接收本报告并签F1窄纠正；本报告为R唯一新增文件。无任务间完成通知要求，R交付后停止修改。
