# FM-DEMO-024-C1 交付

状态：COMPLETED（作者交付，待独立R复审，不自判ACCEPT）。
准确C turn：01a0cddf-4836-7ec0-8d7c-720950abc88c；任务01a0c403-bfa1-7e90-b503-c0fcd61f23c1；local，gpt-6-astra／max。
依据§249～252；签发r125全文SHA256=9a6b3f58fdb3cb53bddedfa6cfe04f7902f7c8215c36d5a4b331848f219d650d；冻结段规范化SHA256=9d211baa68b0d09ca81f65fb073a136475b6711e606b33fdd3c5a81b4cc2f773。规范化为CRLF→LF、TrimEnd＋LF、UTF8；协调追加另列，不重绑签发全文。

## F01修正与实际范围

CandidateTimeSample原有HasMonotonic／HasScope仅从internal改public，private setter保持。CandidateLifecyclePreparation.Time在复制前按Monotonic、Scope顺序检查提供标志，缺项返回MissingField及TimeSample.MonotonicElapsedMilliseconds／TimeSample.MonotonicScopeId；显式双null继续合法。
未改两个单调setter、PrepareTime／Advance、时间算法、意图编码、存储格式、018／019或Architecture；没有反射、内部friend、伪造成功状态或新提交入口。

| 文件 | 实际增／删 | 上限与保全 |
| --- | --- | --- |
| Core/CandidateRecoveryClock.cs | +2/-2 | 合计4；仅两属性可见性。 |
| Application/CandidateLifecyclePreparation.cs | +2/-0 | 合计2≤24；仅Time复制前两检查。 |
| Tests/.../CandidateLifecyclePreparationTests.cs | +28/-0 | 恢复缺字段三回归与仅测试用样本构造。 |
| Tests/.../CandidateLifecycleRecoveryTests.cs | +45/-0 | 显式null／零／同域／换域，冻结与Retry／Resolve六回归。 |
| Tests/.../CandidateLifecycleEndTests.cs | +20/-0 | 退出／胜利缺字段六回归。 |

三测试合计新增93≤220行，原方法体／断言／注册逐字节保留；旧测试部分用原文件完整前缀及结尾校验，其他旧测试整文件SHA保持。
before611→after611；实际仅上述五项变化、其他606项不变，无新增／删除实现或meta。Scripts＋Tests597、Assets624、331个唯一GUID。
原024 canonical=e57a457441b5d0e3353ca0651f07bb569fa06d28bf7e7d68c77ebd034e11405d。
C1 canonical=41344696e520c8c0f379498d779831c48597fea696d059d3325298f6c9ec92ca；不将C1起点重写为原024未改。
相对原019589项，此时Architecture及CandidateRecoveryClock两个旧项变化、其他587项保持、新22项；C1的App与三测试属于024既有新增项。

## 真实红绿验证

固定Unity为D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe，SHA256=ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895；projectPath固定本项目。
每次启动前均无Unity进程；Start-Process -WindowStyle Hidden -PassThru，WaitForExit后首先保存实际Process.ExitCode。未杀进程，未用外壳退出码代替Unity退出。
完整argv／cwd／PID／UTC、源码／保护项／DLL-PDB副本、日志及XML见stage024-c1/runs各目录。

| 运行 | PID | 开始UTC | 结束UTC | Unity退出／实际结果 |
| --- | --- | --- | --- | --- |
| red-01 | 37844 | 2026-09-23T10:52:59.2221813Z | 2026-09-23T10:53:44.0271770Z | 2；9用例、9预期断言失败、0跳过、无编译错误。 |
| compile-01 | 35244 | 2026-09-23T10:55:21.3766754Z | 2026-09-23T10:55:32.6491746Z | 0；无编译错误。 |
| tests-01 | 37160 | 2026-09-23T10:59:14.3389312Z | 2026-09-23T11:03:13.2834400Z | 0；3198/3198 Passed，0 Failed／Skipped。 |

红阶段参数为-batchmode -nographics -runTests -testPlatform EditMode -testFilter F01_，输出Logs/FMDemoB16C1RedTests.log与FMDemoB16C1-Red-EditMode.xml；测试不带-quit。
红阶段Core原SHA=86d8920bb392df984fac0d55294fa93fee736a7f81f4a79c3ef4927b724c954a；App原SHA=0d2eae39698b077886e7ee40e8e4f36e473079a05c7e0ffbe6a0cc7ac75e1685。9个失败均为Expected MissingField／But was Prepared，详见red-assertion-proof.json，不是不可访问属性造成的编译失败。
红XML SHA256=a53f1b349a22a8dbb5aaaa3f1bdb1e3d5f4d02d7a7700390e7326308fba99033；before／after实现canonical相同，原生产SHA及新测试SHA与实际运行绑定。
取证脚本最初将NUnit聚合Failed(Child)只按Failed匹配，导致已保存真实结果后外壳报错；原wrapper-failure、runner-used及实际Unity退出2全部保留。已修正后续脚本匹配，没有重复红阶段。此前两次脚本参数／路径错误发生于Unity启动前，原生记录保留，不计Unity运行。
绿编译参数-batchmode -nographics -quit，日志Logs/FMDemoB16C1Compile.log；绿测试-batchmode -nographics -runTests -testPlatform EditMode，日志Logs/FMDemoB16C1Tests.log、XML FMDemoB16C1-EditMode.xml；无filter或筛选环境参数。
绿XML SHA256=048b07ab23921877ccd07c045ad719b10294a598cca7b81485aa1a51c2cc324b。编译与最终测试的源码／DLL-PDB及当前611项完全同版。
原3183具名Ordinal多重集合完整保留，另15新增全Passed；不另重跑旧六链、019矩阵、G1数值矩阵或旧强杀验证。

## 验收

| ID | 作者验证结果 |
| --- | --- |
| C1-01 | PASS：恢复Prepare、退出EndTimeSample、PrepareVictory各省略双字段／仅Monotonic／仅Scope，共9例；字段顺序和MissingField准确，请求为null、无触盘／发布／恢复推进。 |
| C1-02 | PASS：真实公开DownAndExit产生未完成恢复期，三缺项直接与CandidateRecoveryClock.Advance拒绝对照；显式双null、零、同域和换域四正例按原Core结果比较Outcome／Anomaly／Elapsed及最后样本。 |
| C1-03 | PASS：合法样本Prepare后改原wall／monotonic／scope／trust不影响提交；显式双null的SaveFailed→Retry及CommitUnknown→Resolve仍保持原OperationId、CommitId、canonical、样本及10ms elapsed，一次marker发布并可Restore。原024相关有效证据继承。 |
| C1-04 | PASS：五文件窄差异、93行新增测试、606项保持、3183原名称／方法／断言保留；3198全Passed零跳过，331GUID不变。 |
| C1-05 | PASS：原生产SHA下真实9断言红，再绿编译和无filter全量；真实退出与源码／DLL／XML同版，失败证据保留。 |

## 保全、归档与阶段冻结

证据根为TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage024-c1/。entry／audit完整列611 before／after、保护输入、GUID、DLL、changes、原具名集合和运行身份；sources与每次run的before／after包括完整实现及Tools实际副本。
239440项旧证据入口／出口元数据不变，变化时才重核字节；没有重拷旧13.6GB或宣称本轮全字节审计。测试自然新增B12／B13根及完整文件／空目录在audit与归档清单登记。
本组新增两个helper共157行≤320；原184行helper仅只读复用。脚本、原失败、自然新根和实际源副本均纳入归档。
归档9580文件／661141059字节／33空目录；ZIP 114764958字节，SHA256=0c2ea77d5dca8be9023fdb65a76202aaed1a3488b04acd26b7065007bc0a0549。archive/verification.json核完整源／payload／ZIP路径、长度、SHA及空目录集合一致、源打包后稳定；空目录清单使用显式JSON数组。archive／late／两新报告排除自引用并由late/bindings.json独立绑定。
准确turn的原生CommandExecution／FileChange、生命周期与所选context见late/native.json，不含推理；尚未发生的task_complete由SD00／R在全组formal final后核验。
原024、首次G2、RESUME1作者报告，两旧R报告和旧E16全部只读。无Git写入、新任务、代理或任务间通知。
本阶段源码、测试、两报告和证据在进入G2前冻结；后续G2只可覆盖Core.Tests.asmdef和QFrameworkIntegrationTests.cs两个明确旧项，另609项须保持C1字节。G2无权返修本阶段。

NOT RUN／NOT VERIFIED保持：正式发布／PlayerSave来源、合法AwaitLinks／全倒补线公开见证、全部保存故障交叉组合、物理鼠标触控、交互式Editor／PlayMode、025内容、027动画、028完整面板、029正式场景／完整Demo与§184历史缺证。C1只闭合F01，不将作者COMPLETED等同独立ACCEPT。

