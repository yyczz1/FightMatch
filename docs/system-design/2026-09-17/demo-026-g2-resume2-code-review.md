# FM-DEMO-026-G2-RESUME2 独立复审

VERDICT: ACCEPT

Scope: PASS
Acceptance criteria: PASS（B16B-01～07，含任务包明确保留的运行缺口）
Verification: 最终编译 exit 0；3304/3304 Passed、零失败／跳过；实际 UI Toolkit panel 事件、原 83 个独立用例、当前范围、同版产物及完整归档独立审查通过。
Notes: 合法 AwaitLinks／全倒补线公开见证等仍按包保留 NOT RUN；程序化无图形 Editor panel 通过不等于物理显示或触控体验通过。

## 准确回合与正式接收

- C 任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1（FightMatch 本机保存与应用接入实现）；准确 turn：01a0cddf-4836-7ec0-8d7c-720950abc88c。
- C task_started=2026-09-23T10:45:57.458Z；context=10:45:57.509Z，gpt-6-astra／max、cwd=D:/Unity/UnityProj/FightMatch。11:34:45.168Z 是同 turn 的续接 context。
- 原生非异步 AgentMessage／final_answer 于 12:06:40.533Z（delivery=null、物理行 9881）；准确 task_complete 于 12:06:40.814Z（物理行 9885）。final 对 C1 与本包分别声明作者 COMPLETED，并给四份正式文件绝对路径、SHA 和停止修改声明。
- wait_threads 另核同 turn completed、error=null、任务 idle，cursor=5c8a73c1-d962-408b-880b-35f970baaab2:12；工具省略 final 正文时按 §252 只读准确原生正式消息补核。
- R 任务：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；准确 turn：01a0cde0-108c-71f1-b0e0-b501c8f4b87b；task_started=10:46:48.771Z、context=10:46:48.814Z，gpt-6-astra／max、同 cwd。R 完成事件须在本轮 final 后核，不能由报告先落盘代替。
- R 在原任务自行做 Standards／Spec 两轴审查，仅新增本报告和 C1 独立报告；未运行 Unity、项目测试、业务代码探针，未修实现、写 Git、开代理／外部模型或发协调通知。

依据：[§241～243、§246、§249～253](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md)。§250 精确补 FightMatch.Platform 引用；§246 精确授权两行旧测试谓词例外；其余 G2 目标不变。
r125 签发全文 SHA256=9a6b3f58fdb3cb53bddedfa6cfe04f7902f7c8215c36d5a4b331848f219d650d；§249～252 规范化 SHA256=9d211baa68b0d09ca81f65fb073a136475b6711e606b33fdd3c5a81b4cc2f773，最终复核一致。
当前 r126 全文为 942250 字节、SHA256=74bc710d73f56df8bce36ddd61ed8fe89b40c0d104cfac4089294aa75bb7b14f；旧 §240～243／§246 冻结保持，不将追加协调段后的全文冒称 r125。
原 [G2 独立报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-026-g2-code-review.md) 的 B01 与 NEEDS_FIX、原 G2／RESUME1 的 BLOCKED 历史均保留；旧 R 报告 SHA256=25ad70decc88154eec4bfa54ce64ed85da125046b1cc7f92b84e1a21df744c4e，未覆盖。

| 正式作者文件 | 实物长度 | SHA256 |
| --- | --- | --- |
| [G2 delivery](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-026-g2-resume2-delivery.md) | 96 行／12925 字节 | d1000babf82f25c1c63b2f81320c78881d51fdec1f82eb5ccbac4ff5bbe913d0 |
| [G2 scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-026-g2-resume2-scope.json) | 1239518 字节 | a0666c08cc7a7259f23fcd106544719bca03a1bc6bce4e315704ebacd50a75fb |
| [G2 late/bindings](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage026g2-resume2/late/bindings.json) | 5858 字节／19 项绑定 | 2cb9747826aae7bede11000c064b1919bf3d4e2ae7fd4ebb972b7455269d4d7d |

R 于 12:09:59Z 核当前实物及正式 final SHA 全部一致；报告／scope 低于 280 行／3MiB。C1 两正式文件、21 项晚绑定也在最终重核中保持。

## Standards：导入、边界和当前范围

G2 入口为 C1 于 11:08:10～16Z 冻结后的 611 项；G2 entry 捕获时间 11:09:12.1129376Z。顺序为先 C1 红绿与两报告，再 G2；没有拿旧 024 未修正基线冒充 C1。
G2 相对该入口仅修改下列两项、新增精确 26 项（12 文件＋14 个自然 meta），609 旧项保持，无删除。R 对全部 637 项、650 个 Assets、345 个唯一 GUID 及保护输入核当前实物。

| 项目 | 独立结论 |
| --- | --- |
| [Core.Tests.asmdef](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/FightMatch.Core.Tests.asmdef) | +4/-2；references 仅加 FightMatch.Input／FightMatch.Presentation，noEngineReferences true→false；Editor 限定、其他配置和旧 meta 保持。 |
| [QFrameworkIntegrationTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatchQFramework/QFrameworkIntegrationTests.cs) | +2/-2；只落实 §246 两行：Core.Tests 可用 UnityEngine／UnityEditor，原 Core／Platform 禁用约束及其他断言不变。 |
| 四份 G1 导入 | GestureModels.cs、RouteGesture.cs、Input.asmdef、RouteGestureCases.cs 的源／目标均逐字节一致；编码、换行、internal 构造和原断言未改。 |
| [AssemblyInfo.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Input/AssemblyInfo.cs) | 仅 using System.Runtime.CompilerServices 与精确 InternalsVisibleTo("FightMatch.Core.Tests")，共 2 行；未扩大 public 构造。 |
| [Presentation.asmdef](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Presentation/FightMatch.Presentation.asmdef) | runtime／noEngineReferences=false；引用 Input、Application、Core、Platform、FlowPuzzle.Core、QFramework。前五项在许可中；QFramework 对实际直接调用继承 AbstractSystem 的 019 系统是编译所需，compile-01 的 CS0012 作实证。未安装包或修改旧生产 asmdef。 |
| 编写预算 | 三 UI C#＋AssemblyInfo 共 485 行；三新增测试 C# 共 564 行，分别≤1800。四份原件不计编写预算，仍核原字节。两阶段新增 helper 仅 C1 的两份、157 行≤320，G2 未加 helper。 |
| 禁止项 | 未改 Core／Platform／Application 业务实现、Packages、ProjectSettings、旧 meta 或 Tools；没有 Scene／UXML／USS／内容资产、Input System 包、反射、dynamic、替代预算类型或另一业务写入口。 |

四原件 SHA256 分别为：
GestureModels=c18257cdecb0f993b27130380b071e2f092ad5c230311e30bbd559c5218e9464；
RouteGesture=d6a95368ba46a307bac2cf03120a1c5e7a90ed24b94fe7866d91cb3dd1459e30；
Input.asmdef=b12108ce004ea6364923ebb383a7d285846e31db2b1f6b6ccd60eb56f8899f2f；
RouteGestureCases=7e0b1739d7014e645d4456a1178425d6f9e2c988b4480812830fc158676c114c。

| 三阶段实物 | canonical SHA256 | 对当前的关系 |
| --- | --- | --- |
| 原 024 611 | e57a457441b5d0e3353ca0651f07bb569fa06d28bf7e7d68c77ebd034e11405d | 五项 C1 修正＋两项 G2 覆盖，604 项保持 |
| C1 611 | 41344696e520c8c0f379498d779831c48597fea696d059d3325298f6c9ec92ca | G2 仅两项覆盖，609 项保持 |
| G2 637 | 14e2906fb75fd6b46cc4b174214d892caac6efea2afbaad792b7e7f2cf71ab8d | 新增 26，Scripts＋Tests=623，Assets=650，GUID=345 |

R 独立核原 019 的 589 项至当前仅四旧项变化：FightMatchDemoArchitecture、CandidateRecoveryClock、Core.Tests.asmdef、QFrameworkIntegrationTests；其余 585 项不变、新 48。五个 C1 修正文件在 G2 内保持；C1 App／三测试属于 024 原新增范围。没有把获准 Core 两行变化错报为 G2 越界，也未允许 G2 借范围返修 C1。

## Spec：实现审查与验收映射

R 已阅读全文、全部新测试和失败后差异；以下判断来自真实源码、019 接口和对应运行证据，未仅采用作者 PASS 或全绿总数。

| ID | 独立结论与实现位置 |
| --- | --- |
| B16B-01 | PASS。[Unity wrapper](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/RouteGestureUnityTests.cs) 通过 TestCaseSource(CaseIds) 单独注册 OriginalG1Case；R 从原 BuildIds 构造读出 83 唯一 ID，与 XML 的 83 个独立测试逐个对应，均 Passed。四份原字节和精确 friend 保持。 |
| B16B-02 | PASS，保留约定运行缺口。[Controller](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Presentation/CandidateBoardInputController.cs:49) 从真实 019 view 映射成员、生命、阶段、端点、锁线及可用性；以值键比较，Query 新对象不会清掉手势。选择仅本地、活成员可选、新 Attempt 清选择；历史不要求 Actor。AwaitLinks 映射允许 actorless Link、提交省略 Actor／Preference；没有合法公开 AwaitLinks／全倒补线成功见证，运行仍 NOT RUN，仅静态核边界。 |
| B16B-03 | PASS。[Element](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs:58) 用真实布局、WorldToLocal、左下原点和半开格界，越界为空格；鼠标 6／触控 10 panel points，按 0.15～0.25 格裁剪，统一距离单位。20／40／80 格尺寸及均匀缩放、非均匀拒绝、geometry 变更、最后 Up 位移、非法／过时输入均有实际 panel 见证。 |
| B16B-04 | PASS（程序化真实 Editor panel）。Down／Move／Up／Cancel／CaptureOut／Blur／Detach／Close 与 geometry 注册完整；原 pointerId 捕获，第二指不抢占，Up 最后样本转发且只消费一次。两指、九种取消分支及重复 Up 经真实 UI Toolkit 事件 handlers；没有以私有控制器调用替代事件验证。 |
| B16B-05 | PASS。[Preview／ConfirmRollback](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Presentation/CandidateBoardInputController.cs:177) 只在 Up 后调用真实 019 PreviewRollback，端点／锁线 locator 精确。单条立即提交，多条保存不可变有序范围、显式确认；确认重新比 Attempt／Scene／目标／顺序，变化关闭。取消无写入、空地不查全局历史、未选人和全倒 AwaitRescue 仍能合法历史回退；实际生命／历史恢复由 019 执行。 |
| B16B-06 | PASS（本包覆盖）。[Up／Submit／Receive](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Presentation/CandidateBoardInputController.cs:149) 按 Down 冻结身份构造完整路线，只有原 019 Prepare→Submit 写门；Retry／Resolve／End／Query 保留 LastRequest。SaveFailed／CommitUnknown 实例保持 prepared 身份、canonical／保存字节及一次 marker。pending／播放忙无队列，旧结果显示最新头，原令牌不清新播放；PlayOriginal 原令牌只交一次，显式 RebuildLatest 不重演奖励，生产不调用 ReportPresentationCompleted 冒作动画完成。全部故障交叉组合未声称覆盖。 |
| B16B-07 | PASS。原 3198 名称多重集合完整保留，加 83 G1 与 23 本包新例，共 3304 Passed、零跳过；原断言仅 §246 两行例外。637／650／345、阶段继承、最终源码／DLL／XML、全量新归档及正式完成门均核实。 |

[InputView](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs) 显示已提交 view 的队员／生命／阶段／保存／不可用原因，预览只在临时手势层；100ms Refresh 与稳定成员按钮不反复重建正在交互的界面。
本包提供宿主交接与确认入口；027 动画、028 完整面板、025 正式内容和 029 场景未由这三个 UI 文件代替。未发现需要阻止本包接收的未解决实质问题。

## 真实验证、失败保留与宿主修正

C 是唯一 Unity 执行者。固定 2022.3.18f1，exe SHA256=ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895；每轮预检无同项目 Unity，Hidden／PassThru，保存实际 Process.ExitCode。
编译 -batchmode -nographics -quit；测试 -batchmode -nographics -runTests -testPlatform EditMode，不带 -quit／filter／用例筛选环境参数。全部 projectPath／cwd 指向 D:/Unity/UnityProj/FightMatch。
实际 argv、开始／结束 UTC、日志、stdout／stderr、源码及 DLL/PDB 副本均在 [runs](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage026g2-resume2/runs/compile-06/start.json) 各独立运行目录；固定输出覆盖前保留前轮。

| 运行 | PID | 开始→结束 UTC（2026-09-23） | 真实退出与结果 |
| --- | --- | --- | --- |
| compile-01 | 36392 | 11:35:34.4716452→11:35:42.3657809 | exit 1；QFramework CS0012 与 PointerType 歧义 |
| compile-02 | 30824 | 11:36:31.9796447→11:36:38.2877372 | exit 1；测试默认参数非编译期常量；日志也保留前次导入诊断 |
| compile-03 | 12144 | 11:37:29.3602999→11:37:35.1627716 | exit 1；既有 fixture 的 Math 方法与 System.Math 命名冲突 |
| compile-04 | 21768 | 11:38:14.3729317→11:38:26.0380182 | exit 0 |
| tests-01 | 34172 | 11:39:15.5879238→11:43:09.6979697 | exit 2；3281/3304 Passed、23 Failed、0 Skipped |
| compile-05 | 26628 | 11:44:45.3207063→11:44:55.0380914 | exit 0 |
| tests-02 | 35616 | 11:45:30.0334943→11:49:20.8814912 | exit 2；3284/3304 Passed、20 Failed、0 Skipped |
| compile-06 | 9896 | 11:50:13.7836126→11:50:23.7446399 | exit 0；最终编译无错误 |
| tests-03 | 34120 | 11:51:04.5746873→11:55:06.8698249 | exit 0；3304/3304 Passed、0 Failed／Skipped，231.2752168 秒 |

三次编译失败均只修新文件。compile-01 自然生成 14 个新 meta，compile-04 完成 importer 内容；GUID 前后一致，未修改旧 meta，也没有运行中编辑 C#。
tests-01 的无图形窗口日志中断及夹具泄漏导致 22 个新 UI 例和一个旧 C1 例失败；tests-02 经清理修正后旧 3198＋83 G1＋3 个非 panel 新例全过，20 个 panel 协程因布局 NaN／关闭日志失败。全部失败日志、XML、runner 与前后副本保留；未修改旧生命周期代码或断言。
最终 [测试宿主](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs) 仍创建真实 EditorWindow，断言 panel 非空且 ContextType.Editor，公开 Board.panel.Pick(Vector2.zero) 触发真实样式／布局校验，继续断言正尺寸并输出实际矩形。
仅 GraphicsDeviceType.Null 下逐条 LogAssert.Expect 已观测的 Show 三条和 Close 一条原生图形消息；没有全局 ignoreFailingMessages、Ignore／Skip、反射或假 panel。TearDown CloseAll 和 finally Dispose 防止中断协程泄漏。
20 个通过的 UI 协程经实际 panel 发送池化 UI Toolkit 事件，其输出含正尺寸与事件结果；另 3 个普通新例也通过。这个结果不证明像素绘制或人手输入。
每次重跑都有实际失败与后续修正；没有另行重复 C1 已有效绿色全量、旧六进程／强杀或 G1 历史数值探针。

最终 [XML](D:/Unity/UnityProj/FightMatch/FMDemoB16G2R2-EditMode.xml) 为 2313387 字节，SHA256=d4fff69885971c2185b54db80ad0d50d08781febfcd7ceba9ba0ec90586bf34f。
原 3198 名称集合 SHA=12d3219ce57d7b46fbfca87bc417d9dc735acaefd19da76851aa56e03b50ddcd；最终 3304 集合 SHA=a7a12b13102387f25bf8d7f49e22e578e84c9a7bb1b1262257390858040d8db8；R 独立解析原 CaseIds 和 XML，旧名缺失 0、新 G1 恰为 83、新本包恰为 23。
最终 compile-06 后／tests-03 前后／当前的 34 份 DLL/PDB canonical 均为 1a7253edd50039d4f9ef4e27d79a744d4c6f2b7c3f4a43aa718d536664b39190。
最终全部 Assets canonical=aeb045d19427ed823ad7a2426a7a220be613253fcc593bca2973b047fda8c65a；257 项保护输入 canonical=c40c2e36e5bb580ef75fda043780a21a98eebaf5c3e9168179bc578e893a2304。
R 逐项重算入口 915 个实际副本及九运行各 before／after 的实际副本集合，均与实现／Assets／保护／DLL 清单一致、无缺失或多项。一般每阶段 945 份；首次编译前 927、后 943，第二次编译前 943，差异来自新 meta 与新程序集自然出现。含 Tools；未虚称未复制的实现已复制。

## 归档、保全与未运行边界

R 于 12:07:34.6890538Z 完成 G2 新归档实物独立核验：25826 文件、1478989427 字节、123 个空目录；四清单 canonical、每项路径／长度／SHA、源与 payload／ZIP 的空目录集合全部一致，source-before／after 时间戳稳定，当前源也一致。
canonical SHA256=55eada611a9e7c5b4d0653da193a2595afb7f0d445545656d00369fe20e18654。
[ZIP](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage026g2-resume2/archive/FMDemoB16-026-g2-resume2.zip) 为 281937435 字节，SHA256=cae1b08fd5ecd4265e3c4d24cfcc3a552cb00d018c80ed9832cb539ab103ac80。
archive／late／两正式报告仅为自引用排除，19 项 late/bindings 逐项实物匹配，binding 自身 SHA 与 C final 相符；C1 的 21 项独立冻结绑定也继续匹配。
G2 自然新根：B12/7a773f34fde94ebca734823be765b8d4、B12/886d5981baae4921a9888dd38733a4fc、B12/fe2b364a86fd49cd9ab23205a1fc0bd6；B13/c4e000062ce740b3ae07737f326fb03b、B13/c967b6ab39e34064a21f60503f1c0009、B13/cad04ea720aa4fc7ae048aa4e27d601a，六根完整入档。
late/native.json 导出的 114 条 CommandExecution／FileChange、两个 context 和 task_started，R 按原生物理 ordinal／时间／完整事实内容逐项核对一致，准确回合归属正确；不包含推理条目。正式完成发生于该导出之后，另由原生 final／task_complete 核，不使用导出中的预期完成文字代替。
R 独立 58 个旧根／239440 文件／14103273130 字节的 10:49:28Z 入口与 12:09:06Z 最终路径／长度／时间摘要全等；487 项保护输入字节保持。G2 元数据口径含 C1 三个自然新根，因此为 241670 项；两个新 stage 子树明确排除，不把口径差异说成旧历史新增污染。R 于 12:13:35.8566820Z 独立重算这 241670 项入口／出口元数据，canonical 均为 5b1be9bbd87e9dd92e77793661f81f3a01ce0163436e85bd02081f572ddacb16；58 旧根与 R 自存入口相符，另外三根共 2230 项与当前实物元数据相符。
本轮按 §251 继承旧历史全字节结论，只独立做旧元数据保全和新证据完整字节核验，未重拷或重哈希历史 13.6GB。两旧 R 报告、旧作者报告和旧 G1 文件保留。git --no-optional-locks diff --check 退出 0；未因 LF/CRLF 提示改写文件。

NOT RUN／NOT VERIFIED：合法 AwaitLinks／全倒补线的公开成功见证；未覆盖的保存故障交叉组合；正式内容发布与 PlayerSave 来源；像素绘制、物理鼠标／触控、交互式 Editor／PlayMode；Player build；025／027／028／029 及完整 Demo；§184 历史时间／stderr 缺证。
本包与 [C1 独立报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-024-c1-code-review.md) 分别成立；后续批次仍由 SD00 核本轮 R completed 和正式文件后登记派发，本任务不自行实施下一包。本文件自身 SHA 由本轮 R 正式 final 给出，避免自引用。
