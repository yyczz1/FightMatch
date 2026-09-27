# FM-DEMO-018B 交付

STATUS: COMPLETED
独立审查：待 R；本文是实施交付，不给 ACCEPT。
准确原实施 turn：`01a0c861-71eb-7be1-9675-bce27f7a8645`
实施任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`，原项目 local，gpt-6-astra／max。

## 范围与结果

依据冻结任务包 r95 §188～193，派发 SHA256：
`597f84b8c03ef58caffa6b5e6eb3e1f52a4b93dc0d05b2d876a0872d5c2dfe38`。
当前协调稿的后续状态／018C设计不扩大本包。

新增六份 Core：CandidateApplicationIntent、Records、Protocol、IntentCodec、SaveCodec、References。
新增四份 EditMode：CandidateApplicationIntentTests、ProtocolTests、SaveCodecTests、TestData。
十份 C# 与十份 Unity 生成 meta 均为包内精确白名单；旧源码、旧测试、旧 meta、QFramework 七原件和旧五片 API／字节不改。

五个公开业务入口已实现：PrepareIntent、Propose、Lookup、Encode、Decode。
九种意图采用封闭字段和规范原始字节；全字节判定同操作，返回冻结的原结果。
新档为 CandidateValidation 六片；M02 记录与提交索引一一对应，Snapshot 持有完整六片 Descriptor。
旧操作优先于当前 ExpectedCommit 检查；新办理验证同一 basis，保留回退和结束所需原根。
原 H04 固定一个结算 OperationId，独立 H06 成功候选消费续办路由；本包不执行物理提交或发布。
恢复历史保留原 sample／elapsed／revision／outcome，并分别保存 Period.Anomaly 与 Result.Anomaly。

## B01～B08 实施验证

| 项 | 结果与实际证据 |
| --- | --- |
| B01 | PASS。103 项 Intent 测试；九 Kind 合法形状、关键字段／次序／Context／Preference／Time、漏填／多 payload／错误 Actor／时间半组、输入冻结和只读输出。100 字节完整手算常量及独立 SHA256 固定预期通过。 |
| B02 | PASS。初始化及第 2／3 代、未绑定候选、正确 Parent、历史摘要仅补 basis 的六片摘要；缺失／重复／重排索引、错误代际／ID／摘要和一行多操作拒绝。 |
| B03 | PASS。原操作在升级及新 Attempt 后仍返回原 Commit／Generation／入场对象；冲突、未知及新意图过时分别处理。原查询不重新发奖或安装旧头。 |
| B04 | PASS。真实攻击→回退→新分支→再次回退→退出→新 Attempt→重来链；首次 Superseded、有效旧操作、RollbackRecorded、原结束／新起点可查。缺历史根、缺记录、伪 Anchor、额外行动／所有者变化拒绝。 |
| B05 | PASS。真实 RecoveryClock 产生 Applied、Unchanged、IgnoredTimeRegression、AlreadyIncluded；负 UTC、有理单调时间、显式无单调域、超过 64 位的旧 ExpectedCharacterRevision 及完成期优先语义通过。后续推进不覆盖旧回执；未登记的真实恢复推进不能混入存档。 |
| B06 | PASS。真实 WonPendingSettlement 保存原终局和固定路由；加载后原预留 ID 独立结算。缺路由、多路由、报告／Attempt／终局 ID 错误、ID 碰撞、擅换结算 ID、无收讫消路由均拒绝，basis 未变。 |
| B07 | PASS。34 项 SaveCodec 测试；六片往返、与独立旧五片逐字节相同、完整摘要区别于投影；合法旧五片返回 MissingApplicationRecords。Purpose／contract／owner／schema／requirements／词法／截断／尾随、集合／文本／数字／元数据／总量／共享 Math 预算拒绝。 |
| B08 | PASS。最终 Unity 编译及全量 EditMode 实际 exit 0；原 2772 个 fullname 的 Ordinal 多重集合全部 Passed，新增 172 项全部 Passed，零跳过。范围、GUID、源／程序集绑定及独立归档核验通过。 |

协议测试共 35 项；新增测试总计 103＋35＋34＝172。
B04 的真实成功链使用 Attack。现候选领域为单战士，不能经公开入口产生 AOE 留下的合法 Link 成功链；Link 已覆盖意图形状、字段布局及旧领域拒绝回归，没有伪造成功 Run 或收讫。
Context 完整字段采用既有 PreparedCandidateContext／BusinessFields 的七个具名字段，含 SourceNotes；§189 的“八字段”计数与其引用类型不符，未发明第八个字段或修改旧布局。

固定意图 oracle SHA256：
`dc9d0a7e42a08310d68fd546199dd0f484f818da5deee3f4d5568e799a95383d`。

## 实际 Unity 证据

引擎：`D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe`。
项目及 cwd：`D:/Unity/UnityProj/FightMatch`。
每次运行前确认无 Unity；Hidden／PassThru，保存 PID、完整参数、实际 UTC，WaitForExit 后先持久记录真实 ExitCode。
测试命令没有 -quit。未结束用户进程。

| 轮次 | 编译 | 全量测试 | 保留结果 |
| --- | --- | --- | --- |
| 01 | PID 28560，exit 0 | PID 36232，exit 2；2936／2943 Passed，7 Failed | 结束时关卡定义对象经旧 Codec 恢复后可能不同实例，原检查过严；已改为完整定义字段比对。原输出和当时源码完整保留。 |
| 02 | PID 35432，exit 0 | PID 31824，exit 2；2943／2944 Passed，1 Failed | 新增恢复历史检查已通过；剩余是 NUnit 比较 BigInteger 与 int 的断言类型异常，已修正。原证据保留。 |
| 03 | PID 37624，exit 0 | PID 18176，exit 0；2944／2944 Passed | 最终有效同版。零失败、零跳过。 |

最终编译 UTC：2026-09-22T10:03:48.2425131Z ～ 10:03:58.4285357Z。
最终测试 UTC：2026-09-22T10:04:52.5297326Z ～ 10:06:03.7377621Z。

最终命令：

```powershell
& 'D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe' -batchmode -nographics -quit -projectPath 'D:/Unity/UnityProj/FightMatch' -logFile 'D:/Unity/UnityProj/FightMatch/Logs/FMDemoB14BCompile.log'
& 'D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe' -batchmode -nographics -runTests -testPlatform EditMode -testResults 'D:/Unity/UnityProj/FightMatch/FMDemoB14B-EditMode.xml' -projectPath 'D:/Unity/UnityProj/FightMatch' -logFile 'D:/Unity/UnityProj/FightMatch/Logs/FMDemoB14BTests.log'
```

实际启动由证据根 evidence.ps1 执行；每轮 start.json、process-result.json、end.json、stdout／stderr、Unity 日志、XML、源码／配置／meta 和 DLL／PDB 均归档。准确原 turn 的 native-command-events*.jsonl 保留原 CommandExecution 事件，不以 chunk_id 或推断值代替退出码。

最终 XML SHA256：`60a89e5366a23fd374d7da0df2d1e271578e3df0918df43014fc7ed4a41e5808`。
原 2772 基线 XML SHA256：`d5480b9a155a89e6fabd82199b91490426d13b746ccc0516ac4464965d1e0736`。

## 范围、清单与归档

完整实施：520→540；Scripts＋Tests：506→526；完整 Assets：533→553；meta：285→295，全部 GUID 唯一。
原 520 文件、533 个旧 Assets、191 个保护输入及 14047 份既有 Artifacts 的路径／长度／SHA 不变。
六个新自然回归 Guid 根位于允许的 FMDemoB12／FMDemoB13，共 3165 文件；归档均覆盖，未替换旧测试输出根。
生产 C# 为 1708 行，全部十份 C# 为 2746 行，分别低于 2600／4600 上限。
首轮编译前已有十个新 C#、没有其 meta；首轮 Unity 生成十个 meta。最终编译前／后及测试前／后完整 540 项一致，不声称新 meta 在生成前已存在。

原 520 canonical SHA256：
`fc9c5b4e911327e625841070d26c70d49b740398bb818badcd0a20cdeab368e6`。
最终 540 canonical SHA256：
`466dda28a5316343cf90f0021c64ffaf3b61c0ab22c99ce9bb7f5a4f8a13096e`。
最终 28 个相关 DLL／PDB canonical SHA256：
`e4e7aa3de366610a549c9c1b3f28759d00069685b6ab34a069dc001c1f8ea653`。
canonical 定义为按 Ordinal 路径排序的 UTF-8 `path<TAB>sha256<LF>`。

结构化清单：[demo-018b-scope.json](demo-018b-scope.json)。
证据根：[FMDemoB14B/522f3781d7064ad6bbe024cfc41bc983](../../../TestArtifacts/FMDemoB14B/522f3781d7064ad6bbe024cfc41bc983/)。
scope 明列完整 beforeImplementation／implementation，继承 018A 原 capturedAtUtc；大清单以 Brotli＋Base64 无损保存，附解码长度及 SHA。原始完整基线、最终复核和测试名单也各自存档。

源文件／payload／ZIP 三方均为 12993 项，独立枚举实际目录和 ZIP 条目，再逐项比较路径、长度、SHA；缺失／额外／重复／字节差异均为 0。
ZIP 大小 100792893 字节，SHA256：
`22da328d9fa1bf69a0aced35ad578826800404b8845b8aded80b09feee1d5550`。
三方 canonical SHA256：
`ceff065752ba5869dfed18144484e96e70f729f2fce5841716a15b3d9e9c7d50`。
archive 自身及末端收证 late 目录、最终两报告明确排除自引用；原运行／失败证据均在 ZIP 内。末端命令和最终哈希门另存 late 并由交付摘要绑定。

## 边界与交接

NOT RUN：018C、019、024、025、026-G2、018 整包、PlayerSave／真实 M12 写门、Domain Reload、场景／触控／029 体验与正式内容发布。
未重跑已接收的强杀矩阵；未创建额外验证工程、分支、worktree、提交或子代理，未调用外部模型。
本包只证明候选协议、六片恢复和已列测试，不把候选成功当玩家持久完成。
交两报告后停止修改，等待 R 对准确原 018B turn completed 后独立审查；不自动进入 018C。

