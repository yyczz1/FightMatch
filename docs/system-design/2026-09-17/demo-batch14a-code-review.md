# DEMO-B14A-R1 · FM-DEMO-018A 独立代码审查

VERDICT: ACCEPT

仅接收018A固定QFramework最小接入及A01～A05实证；018整包仍未接收，功能包计数保持23，不据此放行018B或019。
审查任务：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；准确原R turn：01a0c7e4-f26c-7843-8cd2-bc7170a52dbc。
作者任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；准确原C018A turn：01a0c7e4-4bee-7363-afad-4475020babce。
模型／环境：gpt-6-astra／max，原项目local。证据复核收口：2026-09-22T07:49:34.2247406Z。
作者完整正式final于07:27:59.353Z出现，准确原turn的task_complete于07:27:59.695Z出现；此前R只冻结基线并等待，此后才正式审交付。
R未启动Unity、项目测试或.NET工具，未修作者文件、Git写入、开任务或使用子代理；唯一新增文件为本报告。

## 1. 审查依据与范围

依据用户限定、AGENTS／.agent规则及冻结r91 §177～182，采用规范／规格两轴在本R本地独立审查。
派发整稿SHA256：6ac773ab5118c45ef02dcce7739f4f22e97b63ac99a7673f7b2faec567fc5b4a。
SD00之后登记了两行执行状态和准确原turn，另更新协调稿；逐行比对确认§177～182来源、白名单、验收及R权限未变。
只读允许的QFramework研究及相关公共接口；未全面重审960行上游库，未介入G1-C1专用审查任务。
本报告不援引017 §172的历史实际退出码例外；018A的三次真实Unity运行均有实际Process.ExitCode。

正式交付输入：
- [作者报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018a-delivery.md)：83行／10941字节，SHA256=71c83b87ab5c0658236e4acc51f52275b08b06570119b6254f3752f4ba51d219。
- [机器证据](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018a-scope.json)：1500547字节，低于1572864字节，SHA256=dad47f043b66f1f9d4e920ca2ee35e2ac7672f7de42a11463cea28fd2e601931。
- 作者唯一证据根：TestArtifacts/FMDemoB14A/fec577db4655430491f17a2ea2352b81/，以下“证据根”均指该路径。

## 2. 规范轴

| 检查 | 独立结果 |
| --- | --- |
| 改动最小化／白名单 | 新增恰15个Assets文件，与§180逐名一致；旧505实施文件及完整518个Assets文件逐文件SHA保持。没有提前创建应用类、持久协议或玩家档。 |
| 手写预算／程序集 | 测试258行，asmdef21行，合计279；完整配置与冻结值一致，Editor平台、仅引用QFramework、noEngineReferences=true。 |
| 源码与资源边界 | 七份上游原件保持；六份本地meta为Unity自然生成，原276份meta未变。未手写或替换GUID，最终285个GUID全部有效且唯一。 |
| 清理与隔离 | 私有FixtureGate在SetUp／TearDown串行；各行为用例持有自己的Architecture，事件句柄和Architecture在finally清理；工作线程仅在一个用例内顺序独占。 |
| 既有文件／证据 | 原15个证据根共8822文件与R开工时逐根全集规范哈希一致；另98个旧日志／XML／工具输出原路径字节保持。 |
| 受保护输入 | R冻结的440项中437项未变；另3个协调文档的变化由SD00原FileChange记录归属确认，非C改动。Packages、ProjectSettings、权限和旧实现不变。 |
| 验证真实性 | 原命令、cwd、原生输出、起止时间及实际退出可回溯；未以摘要、文件存在或旧测试总数替代实证。 |

R在06:56:36.6804142Z开始冻结现场，06:56:40.0466918Z结束；本包目标文件及新证据根当时均不存在。
原505实施规范SHA=94d317b8cc269d79f6d218c7c5e84b0e166bf7d06df3c5fefc6f8290820922fe。
原518 Assets规范SHA=77be78c1877ec0135a8e6d72055f3fdc4c1e0188bb1a8a35dccb4d6c447f64d9。
原276 meta规范SHA=599b751e695daf8e01bb15434a8629f435bfa9ccedd33ede3b2d463f1bf2db90。
规范哈希均按Ordinal路径排序，串联“路径＋TAB＋小写SHA256＋LF”，以UTF8计算SHA256。
继承C1的完整501项after及505项implementation与原文对象一致；after时间05:00:14.9731359Z，原finalVerifiedAt为05:00:18.085419Z，未给旧implementation补造capturedAtUtc。
本次startedSnapshot实录06:57:10.4209024Z；压缩解码长度和SHA通过。52个文件时间字符串只省略末尾零，UTC ticks相同，路径／长度／SHA及捕获时间保持。

## 3. 规格轴：A01～A05

| 验收 | 独立结论及证据 |
| --- | --- |
| A01 固定来源／程序集 | 七次固定raw响应记录均HTTP200；实际downloads文件、Assets文件和运行冻结副本均匹配§179字节与SHA。MIT完整，三个上游GUID原样保留。实际QFramework与独立测试DLL存在且来自有效编译。 |
| A02 初始化／重建 | [测试24行](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatchQFramework/QFrameworkIntegrationTests.cs:24)调用真实Interface／GetModel／GetSystem；重复取得相同实例且初始化各一次。故意先注册System仍观察Model先初始化；Deinit各一次后重建新Architecture／Model／System，旧Value=99未沿用。 |
| A03 同步调用 | [测试70行](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatchQFramework/QFrameworkIntegrationTests.cs:70)经真实Command修改测试Model、Event通知、Query读取。断言顺序及返回前结果可见，XML保留实际线程见证；工作线程未调用Unity对象API。 |
| A04 订阅／生命周期 | [测试133行](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatchQFramework/QFrameworkIntegrationTests.cs:133)同handler重复订阅，逐次发送回调2→1→0；逐个IUnRegister注销，Deinit再建后旧handler不收新事件。finally覆盖断言及调用异常后的自有资源清理。 |
| A05 回归／范围 | 有效编译及全量EditMode实际exit均0；2772／2772 Passed，旧2767的Ordinal fullname多重集合完整保留，新增5个全部通过、零跳过。520实施／506 Scripts＋Tests／533 Assets／285 GUID吻合。 |

七原件固定commit为64cb5397c3a4497d99f1c82a5730e7444ba3d992，tag为v1.0.246-Unity2018Compatible；来源为[官方固定Scripts](https://github.com/liangxiegame/QFramework/tree/64cb5397c3a4497d99f1c82a5730e7444ba3d992/QFramework.Unity2018%2B/Assets/QFramework/Framework/Scripts)和[根MIT许可](https://github.com/liangxiegame/QFramework/blob/64cb5397c3a4497d99f1c82a5730e7444ba3d992/LICENSE)。
QFramework.asmdef的UTF8 BOM保留；LICENSE仅映射文件名LICENSE.txt；未引入Toolkits／Examples／额外包。
三个固定GUID：7a1fba0c3bb80422c82f18ba7b548eda、9e97e42c28a41430880bee376c274c86、32878c59179e84e84ab2d2132693f8f4。
R仅核查与测试有关的Architecture初始化／Deinit、Command／Query／Event及订阅注销接缝；没有反射重置上游字段、使用Global或OnRegisterPatch替测试隔离。

R用PEReader只读实际DLL的AssemblyReferences，未加载执行作者程序集：
- FightMatch.Core：netstandard、FlowPuzzle.Core。
- FightMatch.Platform：netstandard、FightMatch.Core。
- FightMatch.Core.Tests：mscorlib、System、FightMatch.Platform、FightMatch.Core、System.Numerics、System.Core、FlowPuzzle.Core、nunit.framework、FlowPuzzle.Validation。
- FightMatch.QFramework.Tests：mscorlib、System、nunit.framework、QFramework、System.Core。
- QFramework：netstandard、UnityEngine.CoreModule、UnityEditor.CoreModule；这是框架Editor编译的依赖，未扩散到原纯核程序集。

新测试的实际顺序为caller.before→command→event→command.after-event→caller.after→query→query.returned。
XML见证：普通用例runner／caller=1，command／event／query=1,1,1；工作线程用例runner=1、caller=23、handler=23,23,23。
初始化用例1个、同步调用参数例2个、订阅用例1个、既有程序集引用用例1个，共5个，与源码和实际XML逐名对应。
原2767条包含2762个不同fullname；其多重集合规范SHA=c0ef72d869f6ee474e395c9b4b381232ef027caa474a0ac98b706f6d1a8fb310，旧名重数差异和非Passed均0。
原XML SHA=3d4f6baf02ac0ba2a8cc6bb5e0b6439733ee2167e9a6007279fa0ecd16bf73ec；新XML SHA=d5480b9a155a89e6fabd82199b91490426d13b746ccc0516ac4464965d1e0736。

## 4. 真实Unity运行与失败保全

| 运行 | 原CommandExecution | PID／StartTimeUTC | 实际退出／先落盘时间UTC |
| --- | --- | --- | --- |
| compile-01失败 | exec-a8e193e0-2645-4623-b1a0-b0774f3e4851 | 35400／07:02:21.7876518 | 1／07:02:31.0147521 |
| compile-03有效编译 | exec-919d5026-0037-4357-abb8-16ed95ddbc60 | 30980／07:06:17.3088861 | 0／07:06:45.7383000 |
| tests-01全量测试 | exec-14d935a5-580b-4c45-900a-c3d04f8db950 | 37144／07:07:59.3023538 | 0／07:09:01.2095000 |

日期均2026-09-22；exe为D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe，cwd和projectPath为D:\Unity\UnityProj\FightMatch。
编译实际参数包含-batchmode -nographics -quit -projectPath及本包Compile.log；测试包含-runTests -testPlatform EditMode -testResults及本包Tests.log，没有-quit。
三次原命令都先检查Get-Process，无既有Unity才Hidden／PassThru启动；WaitForExit后读取并保存Process.ExitCode，再采日志／源码／DLL。实际捕获时间均早于DLL采样时间。
未见强杀或第二Unity并发。compile-02保全guard在Start-Process之前失败，没有第四次Unity运行。

首次失败为NUnit缺NonParallelizable／NonParallelizableAttribute的CS0246，完整日志及当时源／配置／meta、stdout／stderr、exit1均保留。
修正仅把新fixture的特性改为私有Monitor串行；原FileChange差异与最终源码一致，未更改上游或旧测试。
初存失败日志43613字节，完整尾部版本43681字节；R逐字节确认旧前缀不变，68字节仅两条cache统计。完整版本在compile-02和compile-03重试前均有副本。
另保留CIM查询拒绝访问、只读Get-Process无匹配、compile-02 guard、首版scope超限等原始失败；全部28条命令中5条实际非零，未隐去。
首版scope为1606659字节，原字节按Brotli保存在scope-creation-tool-record.json，解码SHA=f8dd97ecd28e6cb721089cecfe5cb536097243271aa4df3f247649d5fb820c18；修正后的报告／scope符合预算。
XML output的早期错误投影另存，最终记录来自未改变XML的InnerText；这不是更改测试结果。

R实核全部7组实施冻结副本：各组实际枚举、清单、长度及哈希一致。
有效编译后的520项与测试前／后及当前现场相同，规范SHA=fc9c5b4e911327e625841070d26c70d49b740398bb818badcd0a20cdeab368e6。
compile-03与tests-01的28份DLL/PDB逐文件相同，且与当前Library/ScriptAssemblies对应文件相同。
QFramework.dll SHA=bf1866fec406768e1e5396392d51f0646092b711fa28f6e6a50a5439ce65a8ef；测试DLL SHA=eecdfd8ef2f62ecfaa075f6fba1851a4dbabe2dfe9b3400a50ae318fee3ae11d。

## 5. 非阻断的交付表述更正

[作者报告56行](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018a-delivery.md:56)“最终编译和测试前后均为同一520实施规范SHA”不准确。
compile-03前的规范SHA实际为79563776b0c452f4b7b73d3d2bf1139bf1b2c43aa45186e06ea1c4b37e90fee9；编译后才为fc9c5b4e911327e625841070d26c70d49b740398bb818badcd0a20cdeab368e6。
唯一差异是六份Unity新meta由59字节的原GUID前缀补齐Importer字段，GUID及原前缀均保留；所有C#／asmdef、七固定原件及旧505项未变。
§181明确允许新meta自然生成，完整前后差异已在原证据中保全；编译后与全量测试同版的必要证明成立，因此不构成阻断。
准确口径应为“源码与配置保持；新meta在有效编译导入时补全；有效编译后到测试结束的520项和28份DLL/PDB一致”。R仅在本报告更正口径，没有改作者交付。
除此未发现需修正的阻断问题；不创建纠正实施包或要求重跑已通过验证。

## 6. 原生命令与归档独立核验

R直接读取准确C018A turn的原CommandExecution；20条验证前导出、27条最终导出均逐行与原JSON原文完全一致。
第28条最终门核exec-e52dc196-d4aa-411a-adc2-6980645d4738，原起止毫秒时间1790061974069→1790061996894、cwd及exit0可回溯；其完整请求和两段工具输出与final-gate-tool-record.json精确对应。
首版scope失败和成功修正的记录同样匹配原命令、完整输出及真实退出；合并后28／28覆盖，缺失0。未用显示chunk_id代替原CommandExecution。
R审查原FileChange及PowerShell命令／写入表达式，未见越界写入；六份本地meta只在Unity导入期间产生或补全。
scope六个顶层压缩块及来源清单压缩块均通过解码长度／SHA验证；30项keyEvidenceFiles与实际文件逐项一致。

独立来源集合取实际目录无忽略枚举，未拿作者清单定义全集：
- 本次B14A根3790个文件，扣除逐名列明的9个自引用／后生成元数据，归档3781项。
- 新回归根B12/72727a12bb4743fc9a75d624218b4e3d有713项，B13/4e9194f675c44c09864576da69552491有342项，共1055项，全部保留。
- 另加实际520实施文件、两份最终日志、新XML和原基线XML，共524项。
- 3781＋1055＋524＝5360项；每项按payload/<原项目相对路径>映射，实际解压完整流核长度和SHA。

R于07:40:17.2242952Z独立得到来源／zip各5360项、48785978原字节；零缺项、额外项、差异、重复、大小写冲突、路径逃逸或reparse。
来源及zip规范SHA同为7ba99e8dd5ec728e5fd4340411869e87585ef396ec67c56e98346b47be381b80，随后与作者两份完整清单逐项匹配。
[完整归档](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB14A/fec577db4655430491f17a2ea2352b81/all-validation-products.zip)：12462684字节，SHA256=73c6b8b65efbe2bbeeca523cf3fd01cfe58c862ce58fd40cf0c4d2469d7f9ba0。
9个排除仅为archive-source-manifest.json、zip自身、archive-generation.json、actual-archive-entry-manifest.json、archive-verification.json、final-command-events.jsonl、scope-creation-tool-record.json、final-gate-tool-record.json、final-gate-evidence.json。
这些文件作为明确的后生成交付元数据另行保留并验证；尤其scope超限发生在归档生成后，其原始失败字节与真实结果在已列明的scope-creation记录中，没有伪称它已进入先前zip。
两份报告属于外部交付元数据；没有按扩展名、ignore或旧过滤规则遗漏自然回归产物。

## 7. 接收边界与停止条件

实际Domain Reload开关、GameObject自动注销、场景卸载、018B/C应用办理与持久协议、正式单写门／保存／发布恢复、UI／Player构建／Android／IL2CPP／HybridCLR均NOT RUN。
QFramework同步调用和显式注销不证明持久队列、事务、OperationId幂等或主线程切换；link.xml随包不证明Player裁剪可用。
本R未重跑旧存档强杀矩阵或.NET工具，也未把旧矩阵结果写成本包新验证。
只向SD00交本阶段正式结果；准确原R turn完成前不得视为C／R双回合门已满足。后续018B仍由SD00按阶段门准备和派发。
本报告及正式final后结束停改，不直接派发、实现或审查018B。
