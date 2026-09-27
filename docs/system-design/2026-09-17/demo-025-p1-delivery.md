# FM-DEMO-025-P1 · 内容闭包与发布前验证准备

作者状态：COMPLETED（技术准备；等待独立审查，不自判 ACCEPT）。本次 turn：01a0ce47-0fb4-7c43-9717-1085de8fac27；local / gpt-6-astra / max。

授权：system-task-packets.md r128 §256–259，冻结片段 SHA256 aa1219c85e2ebc655dedc7fdf6f4479d74b92f2318cf58a0d72c1a9a2ede6c21。B16 已独立接收并只读保留；没有 Git 写入、代理、外部模型或协调通知。

## 范围与入口

B16 完整637项起点 canonical=14e2906fb75fd6b46cc4b174214d892caac6efea2afbaad792b7e7f2cf71ab8d，逐路径长度/SHA匹配后冻结；旧3304具名多重集合及源码保持。

新增19项：Content目录自然meta、四个生产C#、Content.asmdef及自然meta；四个新测试C#及自然meta。唯一旧修改为 Core.Tests.asmdef 的 references 追加 FightMatch.Content，2增1删，其余字段及meta未改。阶段656项、355唯一GUID；636旧项不变。生产582行／2400上限；新测试578行／2600上限。两个E17 helper共159行／360上限。

Content为noEngineReferences=true，项目引用仅Core、FlowPuzzle.Core、FlowPuzzle.Validation。没有Unity、QFramework、Platform或Application依赖；没有玩家写入或发布能力。

新增主要公开签名：

- DemoContentCompiler.PrepareCandidate(DemoContentInput, DemoContentJob, ExactMathBudget) → DemoContentPreparationResult。
- DemoContentParameters.EvaluateParameterEvidence(ExactRational p, ExactRational c, ExactRational epsilon, DemoContentJob, ExactMathBudget) → DemoParameterResult。
- DemoContentReplay.ReplayCandidate(DemoPreparedContent, CandidateSeedMaterial, CandidateBattleConditions, Func<BattleSnapshot,CandidateReplayStep> selectNext, int maxSteps, DemoContentJob, ExactEvaluationScope, int maxRandomWords=4096) → DemoContentReplayResult。
- DemoContentDraft(string draftId)、Revise()、BeginJob(CancellationToken=default)；单一草稿owner下修订单调递增，作业绑定开始/结束同一修订和取消令牌；可读结果的IsCurrent供迟到结果采用门检查。调用者须在调用期间保持输入稳定，编辑草稿须Revise。
- DemoContentInput / SourceInput / RouteInput / PrdParameterInput为输入壳；PreparedContent / Source / Route / ParameterEvidence / ShortTermEvidence及三种Result只暴露只读结果。Core既有类型直接复用。

候选指纹由完整有序实际输入计算，不能传入任意fingerprint。规范为固定schema的UTF-8长度前缀字段；精确整数与约分分子/分母，来源以实际字节SHA或核对后的SHA+locator归一化、按path/locator排序；游戏语义列表保留顺序。输入已有Growth/Reward.Context必须为null，由编译器生成共同上下文。完整编码细节和覆盖字段见scope。

## 验收证据

| 条目 | 实际结果 |
| --- | --- |
| P1-01 | L1/L3 × BottomLeft/TopLeft 四候选独立指纹；007A/007B入场/成长和FlowSolutionValidator逐面完整校验，6/16格有效。缺方向、错端点/映射、缺参数、未支持主动及既有DTO缺字段均明确拒绝。源定位、实际SHA、身份与坐标未审项进入source-ledger。 |
| P1-02 | C=1、1/2、1/4分别验证E=1、3/2、71/32，rho=1、2/3、32/71。半率前2次至少成功一次=1，平均成功次数/机会=5/8。31档相邻10^12网格夹根与独立整数生存乘积核对；2/3/10次指标由独立2^m路径枚举核对。零容差、缺项、共享已消耗预算、极小C与超限无半份证明。 |
| P1-03 | 原生SC01材料00..2f在执行前声明；三域初态、f=0、实际PCG字/拒绝组/前后状态保存。四候选全关WonPendingSettlement，014逐步重新求值匹配；023实际报告奖励。L1两步终HP95/XP26；L3首暴击杀A，跳过第二WA，两步终HP95/XP31。64固定来源完整矩阵另覆盖4暴击跳步、60未暴击补击。 |
| P1-04 | 输入/来源字节/路线/奖励/参数深复制，取消、r8→r9→r10同字节和执行中更改令牌拒收。非法首步不耗随机字；没有PlayerSave、玩家Model、存档路径、批准凭证或CommitEligible=true。 |
| P1-05 | review-inputs内source-ledger.json、prd-candidates.json、replay-evidence.json、publication-proposal.md齐备；额外replay-cohort.json记录完整固定矩阵。提案列实际双坐标、31C/候选ε、建议仅L1及合法重打、L3隔离、新档W Lv1/xp0/空库存/无主动/仅L1开放。 |
| P1-06 | 无过滤EditMode 3402/3402 Passed，失败0、跳过0；原3304名称多重集合和旧测试断言保持，新增98。完整范围、GUID、源码/DLL/XML和历史证据元数据审计通过。 |

## 实际验证

- compile-01：PID38052，实际退出1，仅新测试的Input名称与FightMatch.Input命名空间冲突及BattleRandomSnapshot属性名错误；失败输出与前后源/DLL均保留。修正限定新测试。
- compile-02：PID9524，2026-09-23T13:11:18.6793550Z→13:11:55.0818209Z，实际退出0；编译错误0。新meta由Unity自然补全，GUID稳定。
- tests-01：PID21612，2026-09-23T13:13:02.9353502Z→13:20:52.8627915Z，实际退出0；3402/3402；无filter、无-quit、无用例选择环境参数。
- 固定Unity 2022.3.18f1，EXE SHA ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895；每轮核无Unity后Hidden启动。原始argv/cwd/PID/时间/ExitCode/stdout/stderr、稳定日志及实际复制集合见runs。
- 最终实施canonical：9c34b62661ae2fb8b8f5bf12ae4a51ea675c74b860f443a8957523e061463280；36 DLL/PDB canonical：74322e85e0a5081ac6b3ec5afa9967d9355bb1ad299f2a066d78af6ab486dfc7。
- XML：FMDemoB17P1-EditMode.xml，SHA 8c135a35d44b282dfc11f87463660764861d693d365c3fd4793bd0739b0caffe。有效验证不另重复；未单独重跑B16历史矩阵。

## 冻结与归档

E17=TestArtifacts/FMDemoB17/62aa18a2eb9443d195296216f0184526；本阶段TestArtifacts/FMDemoB17/62aa18a2eb9443d195296216f0184526/stage025p1。起点、全部实际源/Assets/保护输入/DLL副本、每轮前后快照、失败运行及审定资料完整保留。旧308409个证据文件入口/出口元数据一致，未重复复制或全字节复核历史多GB树。

完整新源/payload/ZIP为10365文件、806953592字节、34空目录，路径/长度/SHA及空目录集合逐项一致，归档后源稳定。两处自然B12/B13新Guid根完整纳入。ZIP=archive/FMDemoB17-025-p1.zip，129074477字节，SHA 3ddd1711fb41ebf00ab8604783543bb877c744a4523dd52c44191b3ece148f70。archive、late和本两报告排除自引用，late/bindings.json晚绑定。

本阶段656项冻结后交027继承；027只允许原BoardElement窄改，其余655项应保持。本报告和scope在027开始前完成冻结。

## 待审与未运行

publication-proposal.md为具体未审提案：双坐标仍无正式源方向；身份/版本/行为/奖励映射、31C与ε=1/10^9、新档来源等待内容审定。技术证明不替代内容批准；固定来源全关通过不代表任意种子保证或PCG长期频率。

正式Publish、ResolveExact、PlayerSave新档桥接及持久恢复：NOT IMPLEMENTED；025-P1完成不等于025整体功能接收。L3不参与正式开放，旧W1+53、CSV XP25/31不是新评分自然进度来源。

Player构建、真实鼠标/触控/真机、交互式PlayMode、完整Demo、OS低内存与正式内容发布：NOT RUN。无新依赖、配置、Scene/UXML/USS、美术或历史Tools修改。独立审查由R执行，准确原生task_complete须在本次正式final后核实。
