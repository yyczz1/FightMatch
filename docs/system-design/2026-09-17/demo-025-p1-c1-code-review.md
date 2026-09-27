# FM-DEMO-025-P1-C1 独立复审

VERDICT: ACCEPT

Scope: PASS。Acceptance criteria: C1-01～C1-04 PASS。规范轴、规格轴均无未解决的阻断问题；原 F1 的有损编码碰撞已由实际红绿证据闭合。本结论只关闭 025-P1 技术准备阶段。

R task：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；本次准确 R turn：01a0cebd-0ad3-77c2-a58a-f3ee5abdd8f1。local / gpt-6-astra / max。审查日期：2026-09-23。

## 授权、准确续接与正式交付门槛

按 [system-task-packets.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md) §262～265执行。R 入口先核 r131 整稿 SHA256=efe585203d3beb21ec91236542aaf8ca641777619f26464c26a199b3cf10abac；§263～265 按 CRLF→LF、TrimEnd+LF、UTF-8 的冻结 SHA256=38d35d93c6f53c23e87333c2062ea5df93783ee28726976c62e5a2cf20c5edc5，最终仍一致。

SD00 后追加 §266登记准确派发身份；C 保存的入口整稿为 438a6ec8ce96d4a334e2421a14f3d60d5d3f84b826c5a5bd0874f15ef321a705。该整稿含追加登记，冻结包未改；本审查区分原签发整稿与追加后的实读整稿。

C task：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本次准确 C turn：01a0cebc-4911-7d02-9fb9-d1e5661a42d1。原生 task_started 为 2026-09-23T14:47:21.142Z，第11028行；turn_context 为第11029行，local 对应工程、gpt-6-astra / max。R 本次 task_started 为14:48:10.775Z，第18927行；context 第18928行。

R 实读 [C 原生会话记录](C:/Users/YYC/.codex/sessions/2026/09/21/rollout-2026-09-21T20-49-35-01a0c403-bfa1-7e90-b503-c0fcd61f23c1.jsonl)：第11599行、15:38:16.917Z 为本次非异步 AgentMessage formal final（phase=final_answer，delivery=null）；第11603行、15:38:17.788Z 为同一准确 turn 的 task_complete。随后 wait_threads revision 16 确认 completed、error=null。以下唯一结论在该门槛满足后签发。

| 正式交付实物 | 长度 / 行数 | R 实算 SHA256，与 C formal final 一致 |
| --- | --- | --- |
| [delivery](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-025-p1-c1-delivery.md) | 8870字节；60行 | 2d79dfc70d3bfd5b6c1f005f43a9a22f605b79dd753b2acb5974dbe972d01118 |
| [scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-025-p1-c1-scope.json) | 833816字节 | f8ba3e2e25fa3d7d87fe1fd2c75fbddbc92c253d131b198fccb82ae34edbb677 |
| [最终绑定](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB17/62aa18a2eb9443d195296216f0184526/stage025p1-c1/late/bindings.json) | 6511字节；22项 | 7c1de0a382927bc69b9c7d6ff15deb7b57d1422c48ff52f7403a4fba7972edae |

## 规范轴：范围与最小修改

依据 AGENTS.md、.agent 的审查/编码/验证规则和本冻结包，自行采用 code-review 的规范、规格两轴方法。遵循 §265 的单 R 约束，没有启用子代理。R 仅创建本报告；没有运行 Unity、项目测试或业务探针，没有修实现、改旧报告/三协调稿、Git 写入、新任务或协调通知。

入口完整668项与027正式 scope 的路径、长度、SHA逐项相同，canonical=243121465a9724a87e3e93f570273a55a6f98e08085591b90974c5091389fab4。最终仍668项，仅下列两旧文件变化；666项原字节保持，剩余集合 canonical=e4dbe3fc7506884ab12a616b0af2904f918d95ab30076d3d7be01ed3c27eb984。全部 Assets 路径仍681项，361个 meta 原字节及唯一GUID保持。

| 修改文件 | 实际修改量 | 入口 → 最终 SHA256 |
| --- | --- | --- |
| [DemoContentCompiler.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Content/DemoContentCompiler.cs:79) | 3增1删，合计4/80行 | 29b699aa0e264de686fe07c4135733dbe7de67e55e5f7df25600a90e568e2c15 → 506a3523b4383d4b6b6d29b0bc343ff0ec8744f28333d5b962cd2c8389042dfc |
| [DemoContentCompilerTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentCompilerTests.cs:144) | 只追加57/180行，0删除 | 0497377ae4200b3dba883d84d48c4bedb3fb653e4360cded320580ccdee0ed9d → 437f883927ed8a4e7bea950d89e041985f90fe99342078f9abd05d943ef5e707 |

R 对比实际 diff，并核原测试文本前缀和末尾原文完全保留；其他测试文件由666项字节比对覆盖。无新增实现/测试/meta文件，无旧断言、用例名称或选择设置修改。两份新证据 helper 共177/360行，旧 E17 helper 保持。未见依赖、公共API、序列化、持久格式、场景、UXML/USS、配置或无关重构变化；没有为此次单点修正引入抽象层。

## 规格轴：C1-01～C1-04 与原 F1

C1-01：实际生产第188、194行以私有静态 UTF8Encoding(false, true) 替换中央 Canonical.S 的默认替换式编码。原 Fingerprint 内所有规范字符串仍经 S；非法代理项在此抛 EncoderFallbackException，第79行在 PrepareCandidate 既有封套内返回 InvalidValue / Fingerprint.String。实际绿测确认不向调用者抛异常、Candidate=null、无可用指纹，原 job 仍有效。

C1-02：null 的 -1 前缀、空/缺字段/空白的原前置诊断、合法UTF-8字节、ASCII长度前缀、整数/分数、语义与来源顺序均未改；没有规范化、大小写折叠或全局编码设置。两个合法控制红绿均成功、Character/Entry 身份保留、重复准备稳定，指纹保持。取消、修订、预算的原用例源码未改且在两轮全部通过。

C1-03：四个具名回归都调用真实公开 PrepareCandidate。D800/D801 在方法体内构造不同码元，使用同一仍有效 job，其余输入相同；日志逐项输出转义UTF-16身份，避免显示替代符混淆。红轮生产仍为入口29b699…，两失败均落在第153行的期望 InvalidValue 断言，实际 RejectionCode=null；没有用编译/装配错误制造红。

| 具名用例 / 输入 | 原生产红轮 | 修正绿轮 |
| --- | --- | --- |
| F1-D800 / P\uD800 | 接纳，保留D800身份；旧指纹H1；拒绝断言失败 | InvalidValue；无候选/指纹；无异常外泄；通过 |
| F1-D801 / P\uD801 | 接纳，保留D801身份；同一旧指纹H1；拒绝断言失败 | InvalidValue；无候选/指纹；无异常外泄；通过 |
| F1-FFFD / P\uFFFD | 合法成功、原身份保留；H1；通过 | 合法成功、原身份保留；同一H1；通过 |
| F1-PAIR / P\uD83D\uDE00 | 合法成功、原身份保留；H2；通过 | 合法成功、原身份保留；同一H2；通过 |

H1=7c47b4801487d6ea1042f126f250e123ad183bdba22a284fef16a09a2b195ab4；H2=2a50ed6fef37733a2308e58fda31f4017ec1064d4c6e2cb8f06e4f4ddb36b9c7。辅助合法身份P的红绿共同指纹=d97476b542a3962959aeff3dff33381d507e4943eb7146ae3a793ffe720d71e1，三种合法身份互异。这是原规范编码有损替换造成的身份碰撞，严格拒绝非法输入后已闭合。

R 直接解析原027 XML和两份新XML，按 fullname 多重集合比较：原3420项全部保留并通过，恰新增上述4项。原3420名称按Ordinal排序、LF结尾的SHA256=99d85df383132013dc3389d6a731fb89fb6d6006e7c866f72b10bb1a93dbffe8；红绿3424名称摘要同为156a0d976ed5c782580556750415b13a79ac7de0d0e1b4ec9da0d99f98bac856。未仅以总数推断覆盖。

C1-04：668→668继承、两文件修改、666项保持、361GUID与旧027保护已核实。作者308项保护输入 canonical 始终为ad47038500eb10d39eb4fd6e270e57d06b1d65ccd200a38b77409dd6fb663723；R另以本轮505项独立入口在C完成后实算比对，全部无变化。原P1/027作者与R报告、候选资料及旧证据保全。

## 实际验证、版本绑定与失败记录

R 核实固定 Unity EXE 的真实 SHA256=ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895。四轮均先记录无Unity进程，再按授权工作目录 Hidden/PassThru 启动；启动记录与日志 argv 相符，完整起止UTC、真实Process.ExitCode及stdout/stderr保存。compile含-quit，tests无-quit/filter；实际全量具名集合也与该命令范围一致。

| 运行 | PID | 开始 → 退出 UTC（2026-09-23） | 实际Unity退出码 | 独立读取结果 |
| --- | --- | --- | --- | --- |
| red-compile-01 | 37468 | 15:00:36.7642056 → 15:01:36.1831336 | 0 | 无编译错误 |
| red-tests-01 | 26800 | 15:02:58.9243255 → 15:13:09.6724980 | 2 | 3424总数；3422通过、2预期断言失败、0跳过 |
| green-compile-01 | 20736 | 15:16:02.1827880 → 15:16:17.2555717 | 0 | 无编译错误 |
| green-tests-01 | 34696 | 15:17:04.6835929 → 15:27:22.2403851 | 0 | 3424通过、0失败、0跳过 |

R 逐字节核验入口1030份实际副本及四轮前后各1032份副本的路径集合、长度和SHA，无缺失、额外文件或清单冲突。每轮668项源码前后相同，各阶段编译后36份DLL/PDB与测试前后相同；红绿测试源码均为437f8839…。红源码canonical=e4cdc6c0b1e42b30a7689ffd43c3916d2c84c10fca200f241607c5d654378509，红测试DLL/PDB canonical=7c7af32353550920208524af46a6c381c6d3d4d0e8cc0ca06d69b647fd223fde。

最终源码canonical=c11a8a8aef0e46e00d364836e8baab3baea01fa45c81adddefae7cd7b8c84ea0；681 Assets canonical=af80d9c7798b7c8e1f384533283178a48260465a5c170cefa189329fcaf9001c；最终36 DLL/PDB canonical=6609b54e1e0e89628866e6d5f4a51315512710d10e05efd468650e1296378bdd。C完成后scope逐项与当前实物一致。

[红 XML](D:/Unity/UnityProj/FightMatch/FMDemoB17P1C1-Red-EditMode.xml)：2409762字节，SHA256=21b271198bcd45c692eca5fbd9ddf2b3b9c8c50fa8443bd7413b1fe11eda1bf4；[绿 XML](D:/Unity/UnityProj/FightMatch/FMDemoB17P1C1-EditMode.xml)：2407521字节，SHA256=b3331b5bcbc76794f487aa1401fe1c177be74fd0accf8df6c0c685e14134b385。根输出、实际运行副本、scope和late绑定一致。

取证过程保留三项脚本问题：红wrapper把Failed(Child)误判而自身退出1；Audit的phase循环变量与ValidateSet Phase参数同名；内联报告生成的run与点入库的有类型Run同名。原红verification的false、wrapper失败、XML/日志、运行时helper副本、后两次失败记录均保留；仅补充绑定原件SHA的红结果判读及局部脚本修正。R独立确认实际红结果有效、最终审计成立，原生第11556行也证实报告首次命令退出1。四次Unity运行未重复，源码/测试未因此改写。

## 新归档、历史保护与晚绑定

证据根：[stage025p1-c1](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB17/62aa18a2eb9443d195296216f0184526/stage025p1-c1)。R 独立重建新源路径并全字节读取源、payload和ZIP条目：15561文件、1218686444字节，三方路径/长度/SHA集合完全相同，canonical=829b875d92dcc8d9e75e55796f8802717a45f3a6f72aec1dd1b78507670f8e4c。实际源/payload/ZIP均68个空目录，未发现遗漏的新阶段空目录或未列源文件。R核验于15:36:33.5496056Z完成。

[新 ZIP](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB17/62aa18a2eb9443d195296216f0184526/stage025p1-c1/archive/FMDemoB17-025-p1-c1.zip)：178971919字节，SHA256=625d7315d3e3bc4d1bc1f954799925499ea9ba709c26cc11219dab72da69427a。

R独立按入口目录集合识别并核验下列四个自然新根全部入包：

- [B12 / a82dc5fbac7c488d8371e6b1faf70350](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB12/a82dc5fbac7c488d8371e6b1faf70350)。
- [B12 / dc9812a86b32464abd3bb127f1781328](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB12/dc9812a86b32464abd3bb127f1781328)。
- [B13 / 17312a984e0c481d9a3dc55ec1da0c4d](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB13/17312a984e0c481d9a3dc55ec1da0c4d)。
- [B13 / e6d412e2afe941098542e43dcc21200b](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB13/e6d412e2afe941098542e43dcc21200b)。

R分别在14:53:11.7481098Z、15:34:22.7851019Z完成73个旧根的实物元数据采集：356952文件、23125287844字节，逐根Ordinal路径/长度/mtime摘要全部相同；加两份E17原helper为356954文件、23125311317字节。作者入口/出口files数组也逐项相同，并与R独立73根摘要一致。旧多GB全字节结论按授权继承，没有宣称本轮重哈希旧23GB历史。

旧 [P1审查](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-025-p1-code-review.md) SHA256=0f2516e07cd4d5108aa30b463b29bf47489d6947c9d893c74a40a826531ab6e9；旧 [027审查](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-027-code-review.md) SHA256=1688fa9bbd5b5426c5cbd3bcea13a2b2fdaf2c1c87ca6cb674aa33b78d4072dc。两份历史报告保持，027已接收成果与原stage025p1/stage027完整树未重开或改写。

archive、late及两新正式报告排除归档自引用。R实算22项late绑定及scope引用的失败记录，全部路径/长度/SHA相符；绑定自身SHA由C正式final确认。late/native的53项已完成CommandExecution/FileChange、2项context和入口lifecycle逐条对照原生物理行一致；导出后报告/Bind/最终只读检查及formal final/task_complete另由R读原生记录核实。没有导出Reasoning；没有用报告提前出现或旧B17完成记录替代准确完成门槛。

## NOT RUN 与接收边界

R未重跑Unity、项目测试或业务探针；依据实际源码差异、完整C红绿运行、原生回合及独立实物核验作出结论。没有另跑原P1/027旧独立验证。

Player build、真实鼠标/触控/真机、交互式PlayMode、像素绘制、完整Demo、实际OS低内存、真实调度器内部异常及额外故障交叉：NOT RUN。027既有AwaitLinks/全倒补线、同Scene永久提交、零事实PlayOriginal、跨面中间表现的未验证边界保持。

坐标/方向、身份与版本/行为/奖励映射、31档C/ε及新档仍未获得内容批准；原候选提案不变。Publish、ResolveExact、PlayerSave新档桥接与025-P2未实施/未运行。本结论不接收025整体、不放行028，也不扩充029范围。回退若获另行授权，仅针对两份允许文件的本次差异，不重置工作区或旧证据。

本轮唯一写入为本报告；规范轴0项阻断、规格轴0项阻断。按本次准确R回合交付后停止修改，不发送协调通知。
