# FM-DEMO-018B-C1 交付

STATUS: COMPLETED
独立复审：待 R；本文仅为实施验证，不给 ACCEPT。
准确原 C1 turn：`01a0c8c7-6576-7841-8e87-32b65a7541dd`。
实施任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`，gpt-6-astra／max，原项目 local。

## 起点与最小修正

依据冻结 r98 §196～198，保留 §188～193；派发全文 SHA256：
`6bf95baadb658c0d9f6690bc06ea6077fe5c857206dd36c2ca940c638417daa1`。
原 R1 报告 SHA256：`ee7a9fbccfffc60bf24d44db2e956be9a6dcfe3d9cb17714688a127616a5da3e`，唯一 NEEDS_FIX。
本次以原 540 项交付为返修基线，不把原 018B 视为已接收。
beforeImplementation 沿用原 capturedAtUtc=`2026-09-22T10:08:41.9021275Z`；本次实际起点另记为 `2026-09-22T11:04:13.9061899Z`。

仅修改三个白名单 C#：
- Core/CandidateApplicationReferences.cs：新增11、删除2行，最终492行。
- Tests/EditMode/FightMatch/CandidateApplicationProtocolTests.cs：新增125、删除0行，最终418行。
- Tests/EditMode/FightMatch/CandidateApplicationSaveCodecTests.cs：新增79、删除0行，最终337行。
三文件均位于 Assets/Scripts/FightMatch 或 Assets/Tests/EditMode/FightMatch；完整路径、逐字节起点和实际 diff 均在 scope／证据中。
合计增删217行，低于360；六生产1717行、十C#共2959行，低于2600／4600；仅收证帮助脚本121行。

IgnoredTimeRegression 现在使用原历史回执的 LastAcceptedSample 与 Period.Anomaly。
同域要求原域非null且 Ordinal 相等，比较精确单调值；否则比较有符号 WallUtcMilliseconds，并要求严格倒退。
ResultAnomaly 必须恰等于原Period异常 OR 本次Sample异常 OR 必需的DomainChanged OR ClockBackward。
Propose 与 Decode 共用该检查；拒绝保留 InconsistentBinding，不追加记录、不修改basis、不返回半份Snapshot。
没有调用Advance、系统钟或修补Elapsed，没有修改公开API、FMINT001、M02 schema、旧五片编码及其他领域算法。
Applied／Unchanged／AlreadyIncluded 原分支未改，已完成期的旧revision优先语义保持。

## C1-01～06 实施验证

| 项 | 结果与证据 |
| --- | --- |
| C1-01 | PASS。先只新增两测试，原全部生产源码SHA不变。真实Enter→Attack(down)→Exit链得到修订2、LastWall100；实际Clock99产生Ignored及原Next／Period，另用Clock101的真实调用核Applied、修订3、Elapsed1。红阶段Propose与有效外层Envelope的Decode两项均因“Expected False, But was True”失败，原69项全部通过；非编译、夹具或预算错误。 |
| C1-02 | PASS。原反例在Propose结构化拒绝，basis／原结果保持；另4例覆盖同样本、单调增大／相等而Wall倒退的交叉输入。2例用真实结果与另一合法Sample异常交叉，缺少或额外DomainChanged均拒绝；未构造内部成功结果。 |
| C1-03 | PASS。6例覆盖负Wall、同域精确有理单调倒退、Wall前进但单调倒退、跨域／无域、域名大小写不同及超过64位负Wall。真实旧Ignored继续经历变域、推进、完成后，仍保留原sample／elapsed／revision／双异常；四Outcome及AlreadyIncluded旧revision优先通过。 |
| C1-04 | PASS。从真实合法Ignored六片只替换M02的规范意图字节及其长度，旧结果和其余五片保持；原Envelope Prepare→Write→Read成功证明摘要有效，随后Decode因InconsistentBinding拒绝且Value为null。另核历史原Commit／Generation查询、五片独立bytes及共享Math预算耗尽拒绝。 |
| C1-05 | PASS。最后源码修改后的compile-01和tests-01实际exit均0；2959/2959 Passed，原2944个fullname的Ordinal多重集合全部保留，新增15项全Passed、零跳过。红阶段两个测试方法正文保留，原两测试文件全部原行保持。 |
| C1-06 | PASS。537项及原520完全不变，实施540／Assets553／295唯一GUID保持；197个保护输入及39271份旧Artifacts逐路径／长度／SHA不变。实际增删及总行数合规，独立源／payload／ZIP三方8692项一致。 |

新增15项＝Protocol13＋SaveCodec2；完整名称保存在 new-test-cases.json。
本次不重新实现已接收领域算法；原 B01～B04／B06 等仅由未改源码与全量回归保持，不把作者自验代替独立复审。

## 真实红绿运行

Unity：`D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe`。
cwd／projectPath：`D:/Unity/UnityProj/FightMatch`。下表全部时间为2026-09-22 UTC。
每次先核无Unity，再Hidden／PassThru启动；WaitForExit后先持久记录真实Process.ExitCode，之后收集日志／源码／DLL／PDB／XML。
测试命令没有-quit，未结束用户进程。

| 阶段 | PID | 起止UTC | 实际exit／结果 |
| --- | --- | --- | --- |
| red-01 | 12304 | 11:08:24.2997535～11:09:24.5826118 | 199，沙箱中License Client IPC连接拒绝／60秒超时，未进入编译或测试、无XML；不算R1复现。完整失败证据保留。 |
| red-02 | 32320 | 11:10:49.1274846～11:11:34.2221180 | 2，71项中69 Passed、指定2项断言失败、零跳过。沿原成功运行权限启动。 |
| compile-01 | 3988 | 11:14:05.7504573～11:14:14.9012868 | 0，最终同版编译通过。 |
| tests-01 | 11944 | 11:15:52.9364559～11:17:06.3625788 | 0，2959 Passed、零失败／跳过。 |

红阶段完整参数：
`-batchmode -nographics -runTests -testPlatform EditMode -testResults D:/Unity/UnityProj/FightMatch/FMDemoB14BC1-Red-EditMode.xml -testFilter FightMatch.Core.Tests.CandidateApplicationProtocolTests;FightMatch.Core.Tests.CandidateApplicationSaveCodecTests -projectPath D:/Unity/UnityProj/FightMatch -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB14BC1RedTests.log`

最终编译参数：
`-batchmode -nographics -quit -projectPath D:/Unity/UnityProj/FightMatch -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB14BC1Compile.log`

最终全量测试参数：
`-batchmode -nographics -runTests -testPlatform EditMode -testResults D:/Unity/UnityProj/FightMatch/FMDemoB14BC1-EditMode.xml -projectPath D:/Unity/UnityProj/FightMatch -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB14BC1Tests.log`

红XML SHA256：`c6cbbf91f29fba46f4dc2ba4c64ce4ab2c754cfd8069c00f0d18dd0c5fc358b8`。
绿XML SHA256：`b7fe984a389ff9686e71a92bb19235238ba9155a969d1f72b6b677ea44dead2a`。
原2944基线XML SHA256：`60a89e5366a23fd374d7da0df2d1e271578e3df0918df43014fc7ed4a41e5808`。

四次运行各自的before／after完整540项及197个保护输入均冻结，并逐个核副本字节；红阶段生产SHA等于原基线。
最终编译前／后、测试前／后与当前540项一致，最终28个相关DLL／PDB也一致。
准确原turn的原生CommandExecution导出保留cwd、命令、UTC、输出和实际退出；wrapper失败exit1与Unity红期exit199／2分开记录，不用chunk_id或事后填0替代。

## 清单与归档

范围清单：[demo-018b-c1-scope.json](demo-018b-c1-scope.json)。
证据根：[FMDemoB14B/84c638ca1bea4160b6df02ed63ebf5e4](../../../TestArtifacts/FMDemoB14B/84c638ca1bea4160b6df02ed63ebf5e4/)。
scope明列继承的完整540及最终完整540；大清单无损Brotli＋Base64保存，附解码长度／SHA，原始完整JSON亦在归档内。
本次全量测试仅在B12／B13自然新增2个Guid根、1055文件，完整纳入清单与归档；旧根没有替换。
原018B两报告、R1报告、原红／绿与失败证据、旧日志/XML均保持。
三个文件在Git中为既有未跟踪文件，普通git diff为空不作为无改动证明；实际变更以本次原字节快照的git diff --no-index和哈希复核为准，未执行Git写入。

原540 canonical：`466dda28a5316343cf90f0021c64ffaf3b61c0ab22c99ce9bb7f5a4f8a13096e`。
最终540 canonical：`fe791423a3b2422c08da18158b37556b747d7c1c19533afac8ba0e3440b592e0`。
最终28 DLL／PDB canonical：`c0c5eac0f7b94802ca71fd2b9ca32e38a02f865a6e6d851aacb76954a7d87362`。
canonical为Ordinal路径排序后的UTF-8 `path<TAB>sha256<LF>`。

实际source／payload／ZIP各8692项，分别独立枚举源树、真实副本目录和ZIP流，逐项路径／长度／SHA比较，缺失／额外／重复／字节差异均0。
三方canonical：`5cdac52a39bb20e9b47365ae35dfc3ac8a0ce0bb72a8d56f83d228b05e10d478`。
ZIP为88060855字节，SHA256：`49372fd43387bfa9719647145ad73fd9f936700bc8e7d7f7dccafcbd7bea3313`。
archive自身、late末端收证及最终两报告明确排除自引用；所有原运行失败、真实红与绿及相应冻结源码／程序集均在ZIP中。
末端工具执行与最终报告哈希另由late记录及本次formal final绑定。

## 停止与边界

C1-01～06实施验证完成，交两报告后停止修改，等待R对准确原C1 turn completed后的独立复审，不自判ACCEPT。
NOT RUN：018C、019、024、025、026-G2、018整包、PlayerSave／真实M12写门、Domain Reload、场景／触控／029体验与正式内容发布。
未重跑旧强杀矩阵，未创建分支／worktree／提交／额外验证工程／子代理，未调用外部模型。

