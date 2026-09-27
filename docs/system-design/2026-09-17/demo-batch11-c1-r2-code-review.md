# DEMO-B11-C1-R2 · FM-DEMO-015B 独立代码复审

审查日期：2026-09-22（Asia/Shanghai）；以下执行、冻结和核查时间均为 UTC。

```text
Task: FM-DEMO-015B（C1复审）
VERDICT: ACCEPT
Scope: PASS
Standards: PASS
Spec: PASS（F1关闭，原r77整包七项验收满足）
Acceptance criteria: PASS（7/7）
Verification: 原实现红阶段2523 Passed／2个预期失败；修正后编译exit0、2525/2525 Passed
Required user action: NONE
```

## 1. 授权与恢复审查门槛

- 原业务契约：r77 §§127–131，派发整稿SHA `02627b89e614e7ab271ab4b8ddec2d558eaa140b60dd59288930e88de65a6244`。
- C1修正契约：r79 §§139–141，派发整稿SHA `7ec729d263ba06526a3943eee7b0db409e24715253e2f3c7e78024418357d171`。
- 本次续接：r80 §142，派发整稿SHA `89fd6a1738fe441fce03917985e24c6c76ef2302a6ad38a21508e4753d8a24a0`；只更换收尾／审查门槛，不改业务、白名单、红绿及原七项验收。
- 原C任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；R任务：`01a0c1cd-dce1-7ac3-8780-06163cb0acfc`。

| 回合 | 准确turn ID | 本次使用方式 |
| --- | --- | --- |
| C原C1执行 | `01a0c4e0-0c6c-7f32-ad78-2fa6593bf819` | 实际interrupted，永久保留该状态；全部实质改动和四次Unity来自此回合。 |
| C只读收尾 | `01a0c501-07f8-7462-a36d-5cefb945acc0` | final正式提交原冻结交付；wait_threads和read_thread确认17:27:54 completed。 |
| 本次R2复审 | `01a0c502-94eb-7ab2-a9c8-a4e95e396a8f` | 在上述门槛满足后独立审查，唯一写入本报告。 |

C收尾只有三条只读CommandExecution及状态读取，没有源码修改或Unity执行；本报告不将其核对称作新测试。R于17:28:59开始终态范围核对，随后独立计算两文件diff。

原C1-R1门槛报告保持58行／4806字节／SHA `331f61b09dbf22d57d6961edb37ea37aee812f2d5cf0f66e6c946fd903c6971c`，其原turn `01a0c4e0-aa73-76e1-961d-13900042f5f2` 已completed。它当时未进行代码复审；本次明确按§142恢复，并未把原C的interrupted改称completed。

## 2. Standards：独立范围、字节与过程核对

| 检查 | 独立结果 |
| --- | --- |
| 原始来源 | 原demo-015b-scope.json SHA `15babb238beeff611d851a9b82f87ff96ae2017befef1be66a1a9e322885f0f3`；完整471项before仍继承原时间2026-09-21T16:04:18.2498787Z及规范SHA `fb38a53bb72479907648a9a2a713bea025ec477972705ab80768de5ccd7e9bc4`。 |
| 独立原字节 | R于16:52:34、C首次改源码前保存两份完整原字节。最终scope中的原字节与R副本逐字节相同，SHA和行数准确。 |
| 真实起点 | startedSnapshot另记16:53:19.3435749；其471项与before逐项相同，没有把原时间改成本次时间或标成已接收。 |
| 生产diff | CandidateBusinessRestoreChecks.cs仅第317行新增引用检查，+1/-0，增加138字节；原523行变524行。移除新增行可逐字节恢复原文件。 |
| 测试diff | CandidateBattleSaveCodecTests.cs第289–369行新增三测试和一个场景辅助，+81/-0，增加5968字节；原315行变396行。原测试、辅助、换行及其他字节全部保留。 |
| 修改预算 | 两C#合计+82/-0≤260；生产增删1≤20。没有新增C#或meta。交付92≤150行；scope 573057≤614400字节。 |
| 471终态 | 仅两允许文件SHA不同，其余469项完全保持；实际文件与scope.after逐项一致，规范SHA `2e447f19aabed033939d88d90c3b712c8cf0505fb19f4ec9d089db45d7bd1b99`。 |
| 全Assets及GUID | 完整Assets仍488文件、无增删；全部260份meta的路径／字节／GUID相同，260 GUID无重复。 |
| 保护范围 | R预存209项中206项保持；仅SD00三份协调稿有状态／回合／续接记录更新。原报告、scope、旧产物、权限、Packages和配置保持；C写入记录不含协调稿。 |
| 契约复核 | 当前原§§127–131仅四处状态说明变化；§§138–141和§142仅状态、派发及准确回合登记变化。业务／范围／验收与对应冻结文本一致。 |
| Git及回滚边界 | 只读检查使用--no-optional-locks，diff --check exit0。工作区存在先前累积修改，本包范围按冻结字节清单确认；未提交、推送、重置或修改Git。两段新增可独立移除，无需回滚其他批次。 |
| 原执行审计 | 直接读取原C1会话35条CommandExecution、3条FileChange：测试、单行生产修正、交付文档各一条patch；其余写入仅新scope。实际Start-Process恰四次，没有额外Unity或终止进程。 |

所有规范清单均以Ordinal路径＋TAB＋小写SHA＋LF、UTF-8无BOM独立复算。没有用文件总数代替逐项比较。

## 3. Spec：F1已关闭

生产修正在 [CandidateBusinessRestoreChecks.cs:317](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBusinessRestoreChecks.cs:317) 增加 `ReferenceEquals(run.CurrentSnapshot.Baseline, run.Baseline)`，不修改输入，不按相同值合并对象。

该检查通过Check第44行遍历全部收集的Run；Collect第28–29行收集显式RetainedRuns及Rollback根。Prepare和Decode都调用Verify／Check，所以无活动History的保留Run也被覆盖。Decode在CanonicalLayout比较之前执行此业务检查；失败沿既有 `InconsistentBinding`，FieldPath精确为 `M06.Run.CurrentSnapshot.Baseline`，Value为空。原SceneRevision≥previous、回退修订间隔、012／013／014规则及其他校验未变。

新测试均位于 [CandidateBattleSaveCodecTests.cs:290](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleSaveCodecTests.cs:290)：

| C1要求 | 独立源码及原执行证据 |
| --- | --- |
| 合法控制 | 第354–368行经原公开Begin／008 Prepare／012 CreateCandidate构建同值而Baseline、Binding、InitialSnapshot各不相同的A/B；相同种子和PreparedEntry保持。EndExit调用原022／020／021后清除History、Attempt和Carry，显式保留A/B。第290–304行Prepare、Encode、015A Write／Read、Decode完整往返，恢复后仍是两个独立对象且各自引用自身Baseline；红、绿均Passed。 |
| Prepare负例 | 第307–320行仅将A.CurrentSnapshot换成B.CurrentSnapshot，保留其他原字段；断言根、Baseline、Initial和混合Current未被替换。红阶段先确认被接受的Value仍持有该混合Run，再在“应拒绝”断言失败；绿阶段准确字段／错误码／无Value全部通过。 |
| 独立字节Decode负例 | 第323–352行先编码合法图。独立69字节尾部见证断言两空Run、无History及两保留根；首Run.CurrentSnapshotRef位置为body.Length−69+4+12，即body.Length−53，与R在见新测试前的格式推导一致。仅把有效Snapshot索引0改成1，完整比较证实M06仅该字节变化、其他四片不变。 |
| 技术封套与真实业务缺口 | Repack直接调用原015A Prepare重建外层摘要，随后015A Write／Read成功。红Decode的返回对象断言确认A.Current就是B.Initial且Baseline不同，才在应拒绝断言失败；不是越界、摘要坏、无效场景或CanonicalLayout先拒绝。绿Decode在指定Baseline引用字段拒绝。 |
| 不归并／兼容 | 合法控制保留两个同值异对象Baseline；生产代码只有Need检查，没有重写或归并。原回退、高修订、继续行动、014重演、WPS及联合结束用例源码字节不变且最终全部Passed。 |

未发现需要进一步修正的问题。

## 4. 红绿时序、真实Unity执行与归档

直接追溯原C1会话事件，并将scope四份压缩CommandExecution解压：逐份长度／SHA、ID、状态、未截断stdout和shell退出码全部一致。解开工具展示命令的shell引号后，四条归档命令与实际PowerShell命令逐字相同。

- 测试patch实际时间16:57:44.307；red-1冻结16:59:03.0012236；红阶段生产仍为原SHA。
- 红测试17:01:08.0319864结束，完整失败归档17:02:27.9976827；生产唯一patch直到17:02:39.877才出现。
- green-1冻结17:03:05.3794814。红绿测试文件SHA完全相同，两冻结之间只有生产的单行检查变化。
- 每次启动前核固定2022.3.18f1_d29bea25151d、无其他Unity、十源码冻结、471项及保护清单。Hidden／PassThru／Wait之后调用WaitForExit，读取真实Process.Id／ExitCode。
- 编译带-batchmode -nographics -quit；测试带-runTests -testPlatform EditMode且无-quit，项目／日志／XML均为固定绝对路径。
- 每次十源码前后SHA和471项前后规范SHA均与对应阶段独立重建结果一致。四次顺序运行，17:32核查没有Unity进程；R没有运行或终止Unity。

| 运行 | UTC开始→结束（2026-09-21） | PID | Unity ExitCode／结果 | 原CommandExecution ID |
| --- | --- | --- | --- | --- |
| red-compile-1 | 17:00:02.8354201→17:00:11.6452295 | 1568 | 0 | exec-9689132a-db4b-44a6-8c2c-42c253c3d29d |
| red-tests-1 | 17:00:30.7427108→17:01:08.0319864 | 1684 | 2；2523 Passed／2 Failed | exec-670e5231-5e24-4886-a888-337b9548ebba |
| green-compile-1 | 17:03:25.4974294→17:03:33.1225574 | 28040 | 0 | exec-79d84cfc-6f08-4042-9345-ce14d02cf9f8 |
| green-tests-1 | 17:03:52.1679310→17:04:29.9273737 | 55952 | 0；2525/2525 Passed | exec-0dac8a7d-5aee-42d1-bc8b-9f2e11a5e18d |

四个外层shell退出码均为0；本报告明确使用Unity进程的0／2／0／0，未把红阶段shell成功当作测试成功。

独立XML检查显示红阶段只有Prepare测试第317行、Decode测试第349行失败，信息分别为“F1: Prepare accepted a retained Run whose CurrentSnapshot belongs to another equal Baseline.”及“F1: Decode accepted a valid table reference to another equal Baseline's Snapshot.”，均为Expected False／But was True。合法控制及原2522条全部Passed。

绿阶段2525项全部Passed，Failed／Skipped／Inconclusive均0。原XML为2522项／2517个Ordinal fullname；红、绿均逐名保持其出现次数及Passed次数，差异0，新增恰三项。原案例压缩数组371838字节解压后长度／SHA及完整多重集合也与原XML一致；scope新增列表与实际XML一致。

失败日志89040字节和XML1668037字节的Brotli归档逐份解压，长度、SHA、完整字节均与保留的原文件一致。四次启动均拒绝覆盖已有产物，本轮六份自然产物未被替换；原B11档案仍以原scope SHA保留。

日志无C#编译错误／警告、Compilation failed或Unhandled Exception；仍有许可签名／token消息及编译日志Curl error 42，故不宣称全日志零Error。编译真实exit0与最终XML全通过已分别核实。

## 5. 原r77整包七项验收

原B11全量源码审查及七次执行的已核证据引用冻结的 [demo-batch11-code-review.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-batch11-code-review.md)，SHA `785c1dda700594e679d80f8bd690bddf71fcff1721e2046f4c324a3123679ab8`。本次重新确认旧实现／测试字节保持及全部2522回归，并独立补审F1，不仅依据三个新增测试接收。

| 原项 | 本次结论及有效证据 |
| --- | --- |
| ①基础五片 | PASS。FreshPublicOwnersRoundTripThroughTheActualEnvelope、显式空根、owner/schema、CandidateValidation、CommitEligible=false、输入列表复制／只读集合／重复根共享全部保留并通过。原020／021／022／023至015A真实内存往返路径保持。 |
| ②历史与保留根 | PASS。TwoRollbackGenerationsKeepOriginalRangesReferencesAndRecordedReplay、旧Run／Rollback根、跨面与救援通过；两代回退、首次Superseded、原Range与正确引用保持。F1补齐无活动History下同值异Baseline错连拒绝。 |
| ③后续与随机 | PASS。LoadedAndOriginalBranchesHaveIdenticalNextActionRandomPrdAndFinalReport及014 RecordedReplay通过；历史条件／Participant修订、大SceneRevision和回退间隔保持。加载未新增随机初始化、抽样、重演或推进时间。 |
| ④两次提交边界 | PASS。WpsIsASeparateRecoverableSubmissionBeforeAnyEndOrReward、NormalEndingRetainsTheOriginalScoreAndEveryReceiptAndRetriesAddZero及16个部分／冲突结束负例保持；原固定经验26／14／31、020／021／022重复增量0、023复用固定结果成立。 |
| ⑤定义、时间与精确值 | PASS。旧成长／奖励定义、原Clock域、1/3毫秒Elapsed、大整数、原Score上下界／Amount／TermsUsed、Requirements提取／核对及绑定／版本错误拒绝均保持。 |
| ⑥腐坏与资源边界 | PASS。原独立固定字节、腐坏正文重封套、历史／条件／贡献／终态、总字节／集合／字符串／数字token／Math预算、无Value及同源重试通过；F1新增有效表序号的同值错连在技术完整性通过后准确拒绝。 |
| ⑦范围与回归 | PASS。C1仅两源码共82新增行，其他469项及260 meta／GUID保持；原B11范围证据保持；红阶段真实暴露F1，最终同版编译及2525全过，原2522逐名重数／Passed无损，失败与原命令完整可追溯。 |

## 6. 冻结成果标识与交回

| 证据 | SHA256 |
| --- | --- |
| C1交付，92行／10799字节 | `d7df4c79934f6700036df8cf322d8053d3b27532b1c242600885ff901c280fd2` |
| C1 scope，573057字节 | `a09e689364639e9d69c50572092dde32bdc4857f459c8b55caeb0666d036c808` |
| RestoreChecks最终56518字节 | `642b18fd7097329726f3dac6160fd0f63eeff8c2a8ec0532b8727de25abb0c4c` |
| BattleSaveCodecTests最终30347字节 | `b972d036b57db892143e6dfee9e9c6ec69a2d320f164e5d55a7782530001f79d` |
| red-1十源码 | `876de548dcb5c7e558f5d916b7ec1ed39d920841253b1c8477fbca092dfba4ff` |
| green-1十源码 | `99c01476b8fc44c37cb6c82f1c15e8b985e6a6a9d463f00a5459f9a078e23232` |
| FMDemoB11C1RedCompile.log | `a0fa06ebfd845bdcab835691979523936ff78efadcf7fca85fc1b592b17c2f1d` |
| FMDemoB11C1RedTests.log | `a170ecb4bad7befd4ba88f7d6e31540bc9dcdd3b0895d845fe2abc93cca81810` |
| FMDemoB11C1Red-EditMode.xml | `273e4beb998fd111d0dbfa8c8936a508332220ce1c5a05a96ee3abd45ce73cff` |
| FMDemoB11C1Compile.log | `6666e89be270c323ba8aa81a31183510b639e938f9ee94e1bed732399d55ff5e` |
| FMDemoB11C1Tests.log | `dcc44ddb976ee392b65a2e9d228a99bf09141896d7dc016ac82c7c1065308378` |
| FMDemoB11C1-EditMode.xml | `6df01fa60096bc32c1dd2691a623637d00e119921933ca3f7192022f0e5aeb64` |

原2522的fullname＋TAB＋Count＋TAB＋Passed＋LF规范SHA为 `f7e92a7b754c1bac7158d8732308c1d39436e0d431f643a98883a522e8573d01`；最终2525同规则SHA为 `ea7559fcacc4dade4d5049045a6636b4e70e770f04db04327d74a82c50653fb3`。

无未关闭的实质问题，无需新的修正包。成功Link扩展、真实文件保存、016～018、M02、Model安装及PlayerSave／H10仍按原授权留在后续范围，未纳入本次接收。

本轮唯一写入为本报告；未改原报告、代码、测试、meta、scope、产物、协调稿或配置，未运行Unity／项目测试、创建任务／子代理或作Git写入。正式交回后结束本原R2回合并停改；由SD00确认本回合completed，再执行接收登记及后续派发。用户无需操作。

