# FM-DEMO-026-G2-RESUME2 交付

状态：COMPLETED（作者交付，待独立 R 复审，不自判 ACCEPT）。
准确 C turn：01a0cddf-4836-7ec0-8d7c-720950abc88c；任务 01a0c403-bfa1-7e90-b503-c0fcd61f23c1；local，gpt-6-astra／max。
依据 §241～243、§246、§249～252；签发 r125 全文 SHA256=9a6b3f58fdb3cb53bddedfa6cfe04f7902f7c8215c36d5a4b331848f219d650d。
§249～252 规范化 SHA256=9d211baa68b0d09ca81f65fb073a136475b6711e606b33fdd3c5a81b4cc2f773；CRLF→LF、TrimEnd＋LF、UTF8。§253 协调追加不改签发段。

## 实现和依赖边界

先完成 024-C1 的红绿验证、611 项冻结及两报告，随后才导入 G2。C1 的 F01 修正、报告和证据未被 G2 返修；原 024、G2 BLOCKED、RESUME1 BLOCKED 及两旧 R 报告保持原事实。
原 R 的 B01 依赖缺口按 §250 在新 Presentation.asmdef 引用 FightMatch.Platform。首次真实编译又证实直接调用 019 系统需要其 AbstractSystem 所在 QFramework；仅在原许可的新 Presentation.asmdef 追加该引用，没有新增包或修改旧生产 asmdef。

- CandidateBoardInputController 持有实际 019 系统及预算，从不可变当前 view 映射 G1 Context；选择为本地状态，新 Attempt／倒下成员清选择，历史点击不要求 Actor。按真实身份及可用性比较重绑，重复只读 Query 不清除正在绘制的手势。
- CandidateBoardElement 使用实际布局和 WorldToLocal，左下原点／半开边界；距离及鼠标 6／触控 10 基准统一为 panel points，裁剪到 0.15～0.25 格。几何改变取消，非均匀缩放拒绝。真实 Down／Move／最后 Up、Cancel／CaptureOut／Blur／Detach 均经过注册 handlers，第二 pointerId 不抢占。
- Down 冻结当前身份且只预览；Up 最多消费一次。完整路线经原 019 Prepare→Submit；恢复保留同一个 prepared，失败不显示候选头，不自动排队或复投过时手势。
- 历史端点与锁线分别调用实际 PreviewRollback；单条在 Up 后提交，多条交接不可变范围并显式确认／取消。确认重新核原 Attempt／Scene／目标／有序范围，变化关闭，不自动改选。
- CandidateBoardInputView 组合真实队员／生命／阶段／保存与不可用原因。PlayOriginal 与原令牌只交宿主一次；Query 不重播，显式 RebuildLatest 委托 019。本包生产代码不报动画完成、不发 H06／奖励、不直调 Core 业务 builder。

Input 的三文件和原 RouteGestureCases 完全按原字节导入；只添加精确的 InternalsVisibleTo("FightMatch.Core.Tests")。Input 保持 noEngineReferences=true；Presentation 是 runtime，不引用 UnityEditor。EditorWindow 只存在于 Editor 测试程序集。

| 导入原件 | 字节 | SHA256（源／目标相同） |
| --- | --- | --- |
| GestureModels.cs | 12570 | c18257cdecb0f993b27130380b071e2f092ad5c230311e30bbd559c5218e9464 |
| RouteGesture.cs | 14019 | d6a95368ba46a307bac2cf03120a1c5e7a90ed24b94fe7866d91cb3dd1459e30 |
| FightMatch.Input.asmdef | 396 | b12108ce004ea6364923ebb383a7d285846e31db2b1f6b6ccd60eb56f8899f2f |
| RouteGestureCases.cs | 53011 | 7e0b1739d7014e645d4456a1178425d6f9e2c988b4480812830fc158676c114c |

## 实际范围与阶段覆盖

G2 相对 C1：611→637，新增 26 项（12 文件＋14 个 Unity 自然生成 meta），仅以下两个旧文件修改、其他 609 项逐项长度／SHA 保持，无删除。Scripts＋Tests=623，Assets=650，唯一 GUID=345；Packages、ProjectSettings、旧 meta、Tools 和受保护输入保持。

| 旧文件 | 实际增／删 | 精确范围 |
| --- | --- | --- |
| Assets/Tests/EditMode/FightMatch/FightMatch.Core.Tests.asmdef | +4/-2 | references 仅追加 Input／Presentation，noEngineReferences true→false，其他字段不变。 |
| Assets/Tests/EditMode/FightMatchQFramework/QFrameworkIntegrationTests.cs | +2/-2 | §246 精确两行 Unity 引用断言例外；既有 QFramework 条件不变，其他方法／断言不变。 |

新 UI 三 C#＋AssemblyInfo 共 485 行≤1800；三份新测试共 564 行≤1800；四份原字节导入不计编写预算，仍完整核 SHA。本组新增 helper 仍为 C1 冻结的两份、157 行≤320；原 184 行 helper 只读复用，G2 未新增 helper。
原 024 611 canonical=e57a457441b5d0e3353ca0651f07bb569fa06d28bf7e7d68c77ebd034e11405d。
C1 611 canonical=41344696e520c8c0f379498d779831c48597fea696d059d3325298f6c9ec92ca。
G2 637 canonical=14e2906fb75fd6b46cc4b174214d892caac6efea2afbaad792b7e7f2cf71ab8d。
原 024→当前有五项 C1 修正＋两项 G2 覆盖，其他 604 项保持；C1→当前仅两项覆盖、其他 609 项保持，stage-overlay.json 逐项给出原／现 SHA。不能把当前 SHA 冒充原阶段。
相对原 019 的 589 项，只有 FightMatchDemoArchitecture、CandidateRecoveryClock、Core.Tests.asmdef、QFrameworkIntegrationTests 四旧项变化，另外 585 项保持；新增 48。C1 的 App 与三测试属于 024 原新增项。

## 真实运行与失败保留

Unity 固定 D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe，SHA256=ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895。每次先核无 Unity，Start-Process -WindowStyle Hidden -PassThru；WaitForExit 后保存实际 Process.ExitCode，无杀进程。
projectPath／cwd 均为 D:/Unity/UnityProj/FightMatch。编译参数 -batchmode -nographics -quit；测试参数 -batchmode -nographics -runTests -testPlatform EditMode，不带 -quit、filter 或筛选环境参数。
固定输出为 Logs/FMDemoB16G2R2Compile.log、Logs/FMDemoB16G2R2Tests.log、FMDemoB16G2R2-EditMode.xml；每次覆盖前已保存上一份，独立 runs 目录保留真实 argv／cwd／PID／开始结束 UTC／退出、源码／保护输入／DLL-PDB、日志和 XML。

| 运行 | PID | 开始 UTC | 结束 UTC | Unity 退出与结果 |
| --- | --- | --- | --- | --- |
| compile-01 | 36392 | 2026-09-23T11:35:34.4716452Z | 2026-09-23T11:35:42.3657809Z | 1；编译失败，保留原诊断 |
| compile-02 | 30824 | 2026-09-23T11:36:31.9796447Z | 2026-09-23T11:36:38.2877372Z | 1；编译失败，保留原诊断 |
| compile-03 | 12144 | 2026-09-23T11:37:29.3602999Z | 2026-09-23T11:37:35.1627716Z | 1；编译失败，保留原诊断 |
| compile-04 | 21768 | 2026-09-23T11:38:14.3729317Z | 2026-09-23T11:38:26.0380182Z | 0；无编译错误 |
| tests-01 | 34172 | 2026-09-23T11:39:15.5879238Z | 2026-09-23T11:43:09.6979697Z | 2；3281/3304 Passed，23 Failed，0 Skipped |
| compile-05 | 26628 | 2026-09-23T11:44:45.3207063Z | 2026-09-23T11:44:55.0380914Z | 0；无编译错误 |
| tests-02 | 35616 | 2026-09-23T11:45:30.0334943Z | 2026-09-23T11:49:20.8814912Z | 2；3284/3304 Passed，20 Failed，0 Skipped |
| compile-06 | 9896 | 2026-09-23T11:50:13.7836126Z | 2026-09-23T11:50:23.7446399Z | 0；无编译错误 |
| tests-03 | 34120 | 2026-09-23T11:51:04.5746873Z | 2026-09-23T11:55:06.8698249Z | 0；3304/3304 Passed，0 Failed，0 Skipped |

compile-01：新 Presentation 缺 QFramework 引用及 PointerType 歧义；compile-02：新夹具默认参数用了非编译期常量（日志还保留导入前 QFramework 诊断）；compile-03：System.Math 与既有 fixture 的 Math 方法命名冲突。均只修新文件，失败副本保留。
tests-01：3304 项中 3281 Passed／23 Failed／0 Skipped。首个 UI 用例被无图形设备日志中断，未关闭的夹具造成 21 个 Unconfigured／Ready 失败及一个旧生命周期 InconsistentBinding；具体断言和版本见 run-history.json。未修改旧生命周期测试。
tests-02：3284 Passed／20 Failed／0 Skipped；原 3198、83 G1 和三个非 panel 新例全通过。20 个 panel 用例真实检测到无重绘时布局 NaN，关闭窗口另有原生图形诊断。
最终夹具用实际 EditorWindow panel 的公开 Pick 验证真实布局；仍断言 ContextType.Editor、非空 panel、正尺寸，事件继续经 handlers。仅 Null 图形设备下对 Show 三条及 Close 一条已观测原生图形消息逐条 LogAssert.Expect；没有 ignoreFailingMessages、Ignore、跳过或替换业务断言。TearDown 负责中断协程的清理，避免状态影响后续旧例。
tests-03：3304/3304 Passed，0 Failed／Skipped，实际 Unity exit=0。原 3198 名称 Ordinal 多重集合完整保留，新增 83 个独立 OriginalG1Case 加 23 个本包用例，其中 20 个使用真实 panel。
最终 XML SHA256=d4fff69885971c2185b54db80ad0d50d08781febfcd7ceba9ba0ec90586bf34f。
最终 compile-06／tests-03／当前实现 canonical 相同；对应 DLL-PDB canonical=1a7253edd50039d4f9ef4e27d79a744d4c6f2b7c3f4a43aa718d536664b39190。最终日志无编译错误或未处理异常；无图形诊断属于上述精确预期，不能将其描述为物理显示通过。
每次重跑均由实际失败与新修正触发；C1 绿全量、019 旧矩阵、旧六进程／强杀和 G1 历史数值探针未单独重复。

## B16B 验收

| ID | 作者验证与保留边界 |
| --- | --- |
| B16B-01 | PASS：四原件字节一致、精确 friend，83 原 CaseIds 独立注册且全 Passed；冻结断言未弱化。 |
| B16B-02 | PASS（现有公开链）／保留运行缺口：真实 view、只读选人、清选择、倒下灰显及 actorless 历史；FreeLink／全倒补线按 AwaitLinks 静态映射，无合法公开构造见证，未伪造成功快照。 |
| B16B-03 | PASS：左下／半开／越界、20／40／80 格尺寸和均匀缩放阈值、末 Up 样本、合法完整路线、非法／过时／pending 不提交。 |
| B16B-04 | PASS：实际 Editor panel 和事件，包括两指、Cancel／CaptureOut／Blur／Detach／关闭／geometry／选择／越界及重复 Up；非物理输入。 |
| B16B-05 | PASS：锁线单条 Up 才回退；多条显式确认，生命／随机／历史恢复来自 019；Scene 改变关闭原范围，取消无写入，空地不找全局历史。 |
| B16B-06 | PASS（本包覆盖）：SaveFailed→Retry、CommitUnknown→Resolve 复用原 prepared／canonical／保存字节且 marker 一次；忙无队列、旧结果保留最新头、原令牌不误清新播放、PlayOriginal 一次、显式重建不发奖励；预算拒绝与 End 取消有见证。未覆盖所有故障交叉组合。 |
| B16B-07 | PASS：3304 全 Passed、零跳过；原 3198 保留、83＋23 新例；637／650／345 范围和阶段覆盖、同版、归档均核对。§246／250 两个授权范围例外明确登记。 |

## 证据、归档和完成门

新证据根：D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage026g2-resume2/。
entry／audit 完整列阶段 611 before／637 after、保护输入、GUID、actual changes、namedCases 与运行；sources 和每个 run 的 before／after 留完整对应实现（含 Tools）及必要保护输入、DLL-PDB 实物。
241670 项旧证据入口／出口元数据不变；本轮两个新子树明确排除，不声称重新逐字节审计 13.6GB 历史。C1 冻结绑定另核 21 项实际长度／SHA 一致，包括两报告、原输出、ZIP 与清单；不重写 C1。6 个自然新增 B12／B13 Guid 根完整纳入新归档，旧根不改。
归档 25826 文件／1478989427 字节／123 空目录；ZIP 281937435 字节，SHA256=cae1b08fd5ecd4265e3c4d24cfcc3a552cb00d018c80ed9832cb539ab103ac80。
源／payload／ZIP 逐项路径、长度、SHA 和空目录集合相等，源打包后稳定；失败运行全部保留。archive／late／两新正式报告排除自引用，late/bindings.json 独立晚绑定，自己的 SHA 由 formal final 给出。
static-scope.json、stage-overlay.json、g1-ui-test-proof.json、run-history.json 给精确导入、配置例外、阶段映射、实际面板输出和失败原因；git diff --check exit 0。无 Git 写入、外部模型、新任务／代理、SD00 通知。
late/native.json 仅本准确 turn 的 CommandExecution／FileChange、生命周期与必要 context，不含推理；原生 task_complete 待 formal final 后由 SD00／R 核，不能由报告提前出现代替。
UI 宿主排错时只读核对 [Unity 2022.3 EditorWindow 源码](https://raw.githubusercontent.com/Unity-Technologies/UnityCsReference/2022.3/Editor/Mono/EditorWindow.cs)及 [IPanel.Pick API](https://docs.unity.cn/2022.3/Documentation/ScriptReference/UIElements.IPanel.Pick.html)；运行结论来自本机日志／XML。

NOT RUN／NOT VERIFIED：正式内容发布／PlayerSave 来源；合法 AwaitLinks 与全倒补线公开见证；未覆盖的保存故障交叉组合；真实绘制像素、物理鼠标／触控体验、交互式 Editor／PlayMode；025 正式内容、027 动画、028 完整面板、029 正式场景／完整 Demo；§184 历史时间／stderr 缺证。程序化无图形 panel 事件通过不消除上述边界。
本包源码与证据冻结，交付四个组报告后停止修改；024-C1 与本包分别等待独立 R 唯一裁决，作者 COMPLETED 不等同 ACCEPT。

