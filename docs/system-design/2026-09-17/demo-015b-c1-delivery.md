# FM-DEMO-015B-C1 / DEMO-B11-C1 实施交付

状态：IMPLEMENTATION_VERIFIED_PENDING_INDEPENDENT_REVIEW。F1 已完成实际红灯复现和最小修正；最终编译及全部 2525 条 EditMode 通过。独立结论由 R 给出。

- 执行者：原 C，任务 01a0c403-bfa1-7e90-b503-c0fcd61f23c1，原实施 turn 01a0c4e0-0c6c-7f32-ad78-2fa6593bf819。
- 依据：system-task-packets.md r79 §§138–141；派发整稿 SHA256 为 7ec729d263ba06526a3943eee7b0db409e24715253e2f3c7e78024418357d171；原 r77 §§127–131 七项验收继续适用。
- F1 来源：[原 R 报告](demo-batch11-code-review.md)，SHA256 为 785c1dda700594e679d80f8bd690bddf71fcff1721e2046f4c324a3123679ab8，唯一结论 NEEDS_FIX。
- 固定起点：[原 scope](demo-015b-scope.json).after，原时间 2026-09-21T16:04:18.2498787Z、471 项规范 SHA256 fb38a53bb72479907648a9a2a713bea025ec477972705ab80768de5ccd7e9bc4；原 scope SHA256 15babb238beeff611d851a9b82f87ff96ae2017befef1be66a1a9e322885f0f3。该起点是 R 已核范围但业务尚未接收的版本。
- 本次真实起点另存 startedSnapshot；原 before 的时间和完整清单没有替换。

## 修改与范围

| 文件 | 实际修改 |
| --- | --- |
| Assets/Scripts/FightMatch/Core/CandidateBusinessRestoreChecks.cs | Run 中新增 1 行 ReferenceEquals(CurrentSnapshot.Baseline, run.Baseline)，失败沿既有 InconsistentBinding，FieldPath 为 M06.Run.CurrentSnapshot.Baseline。 |
| Assets/Tests/EditMode/FightMatch/CandidateBattleSaveCodecTests.cs | 新增 81 行：合法控制组、Prepare 混合对象负例、独立字节 Decode 负例及窄辅助；旧测试字节内容完整保留。 |
| docs/system-design/2026-09-17/demo-015b-c1-delivery.md | 本交付文件。 |
| docs/system-design/2026-09-17/demo-015b-c1-scope.json | 原字节、完整清单、冻结、红绿运行与原始证据。 |

两份 C# 合计新增 82 行、删除 0 行，生产文件增删 1 行；满足 ≤260／≤20 行预算。移除新增片段后，两文件分别与保存的原始字节完全一致。
最终仍 471 项，仅上述两份 C# 的 SHA 改变；其余 469 项 SHA 不变。全部 260 份 meta 的路径、字节、GUID 不变，260 GUID 唯一。
未修改公共 API、015A 格式／入口、012／013／014、CandidateProgressionState、旧测试、旧报告／scope、原验证产物、依赖、设置或权限；没有新增 C#／meta、Git 写入、016 实现或其他任务。

| 文件 | 原 SHA256 | 最终 SHA256 |
| --- | --- | --- |
| CandidateBusinessRestoreChecks.cs | ea42030433b3f9eedbf7b4474d855aa800c4dd736abd42a7deef49411cd923f7 | 642b18fd7097329726f3dac6160fd0f63eeff8c2a8ec0532b8727de25abb0c4c |
| CandidateBattleSaveCodecTests.cs | 382d34639de52804ce7092e5d53c0eab31099b6fbf16766fb4f877bf23de34c1 | b972d036b57db892143e6dfee9e9c6ec69a2d320f164e5d55a7782530001f79d |

## F1 实际复现及修正验证

1. RetainedTwinRuns 经原公开入口创建 A，再以相同 PreparedEntry／种子创建 B；Baseline、Binding、初态 Snapshot 各自是不同对象且值相同。公开普通退出后 ActiveHistory、ActiveAttempt、ActiveCarry 为空，显式 RetainedRuns 保留 A/B。
2. SameValueDistinctRetainedBaselinesRoundTripAfterNormalExit：合法图 Prepare→Encode→015A Write／Read→Decode 全程成功；往返后 A/B 仍是独立对象，各 CurrentSnapshot 仍引用自身 Baseline。红、绿阶段均 Passed。
3. PrepareRejectsRetainedCurrentSnapshotFromAnotherEqualBaseline：仅重建 A，将 CurrentSnapshot 换成 B.CurrentSnapshot，保留 A 其他原字段和 B。断言输入根、Baseline、初态、混合 Current 均未被替换或归并。原生产代码错误接受并保留该混合根；修正后拒绝、Value=null、InconsistentBinding，字段准确为 M06.Run.CurrentSnapshot.Baseline。
4. DecodeRejectsValidSnapshotReferenceToAnotherEqualBaseline：从合法图编码的 M06 正文，以独立固定 69 字节尾部见证定位第一个 Run.CurrentSnapshotRef；只将有效表序号 0 改为 B 的 1，断言整个 M06 只变 1 字节、另四片完全不变。使用原 015A Prepare 重建摘要，再经 Write／Read 验证技术完整性；没有使用业务 writer 生成错误图。
5. 原 Decode 接受改后正文，恢复结果明确显示 A.CurrentSnapshot 与 B.InitialSnapshot 同一对象、其 Baseline 与 A 不同对象；失败发生在“应拒绝”的断言。修正后准确拒绝上述引用字段、Value=null，未依赖越界、摘要破坏或 CanonicalLayout 先拒绝。
6. 2026-09-21T17:02:25.5272578Z 完成红证据核验时，生产 SHA 仍为原 ea420304…；之后才加入单行生产检查。检查不修改数据，不按值合并 Baseline，原 SceneRevision 间隔和回退规则保持。

红阶段：总计 2525、Passed=2523、Failed=2、Skipped=0；原 2522 条和合法控制组均通过。两个失败信息分别为：

- F1: Prepare accepted a retained Run whose CurrentSnapshot belongs to another equal Baseline.
- F1: Decode accepted a valid table reference to another equal Baseline's Snapshot.

最终：总计 2525、Passed=2525、Failed=0、Skipped=0。按 StringComparer.Ordinal 逐名比较原 2522 条的 fullname 出现重数和 Passed 重数（2517 个不同名字）；全部保持，3 条新增全过。没有用总数代替逐名回归核对。

## 冻结与实际 Unity 运行

每阶段冻结原 015B 同一十份 C#；红阶段仅测试变化，绿阶段测试 SHA 与红阶段相同。每次运行前后十文件及完整 471 项 SHA 均一致。
red-1：2026-09-21T16:59:03.0012236Z，规范 SHA256 876de548dcb5c7e558f5d916b7ec1ed39d920841253b1c8477fbca092dfba4ff。
green-1：2026-09-21T17:03:05.3794814Z，规范 SHA256 99c01476b8fc44c37cb6c82f1c15e8b985e6a6a9d463f00a5459f9a078e23232。
固定程序 D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe，ProductVersion=2022.3.18f1_d29bea25151d；项目路径 D:\Unity\UnityProj\FightMatch。
所有启动均先核 Get-Process Unity 为空，使用 Start-Process -WindowStyle Hidden -PassThru -Wait，随后 WaitForExit 并记录真实 Process.ExitCode；C 是唯一运行者，没有终止进程或并发实例。最终无残留 Unity。

| 运行 | Freeze | UTC 开始→结束（2026-09-21） | PID | Unity ExitCode／结果 | 原 CommandExecution |
| --- | --- | --- | --- | --- | --- |
| red-compile-1 | red-1 | 17:00:02.8354201→17:00:11.6452295 | 1568 | 0，编译通过 | exec-9689132a-db4b-44a6-8c2c-42c253c3d29d |
| red-tests-1 | red-1 | 17:00:30.7427108→17:01:08.0319864 | 1684 | 2，恰好两个预期 F1 失败 | exec-670e5231-5e24-4886-a888-337b9548ebba |
| green-compile-1 | green-1 | 17:03:25.4974294→17:03:33.1225574 | 28040 | 0，编译通过 | exec-79d84cfc-6f08-4042-9345-ce14d02cf9f8 |
| green-tests-1 | green-1 | 17:03:52.1679310→17:04:29.9273737 | 55952 | 0，2525/2525 Passed | exec-0dac8a7d-5aee-42d1-bc8b-9f2e11a5e18d |

编译参数为 -batchmode -nographics -quit -projectPath <上述绝对项目路径> -logFile <下表日志的绝对路径>。
测试参数为 -batchmode -nographics -projectPath <上述绝对项目路径> -runTests -testPlatform EditMode -testResults <下表 XML 的绝对路径> -logFile <下表日志的绝对路径>，不带 -quit。
scope.validationRuns 保存逐次完整参数、PID／UTC／真实退出码、前后 SHA；rawCommandExecutions 保存从本原 turn 的 read_thread 取得的四条完整原 CommandExecution，输出未截断。四条 shell ExitCode 均为 0，与红测试 Unity 的 2 分开记录。

| 自然产物 | 字节数 | SHA256 |
| --- | --- | --- |
| Logs/FMDemoB11C1RedCompile.log | 61132 | a0fa06ebfd845bdcab835691979523936ff78efadcf7fca85fc1b592b17c2f1d |
| Logs/FMDemoB11C1RedTests.log | 89040 | a170ecb4bad7befd4ba88f7d6e31540bc9dcdd3b0895d845fe2abc93cca81810 |
| FMDemoB11C1Red-EditMode.xml | 1668037 | 273e4beb998fd111d0dbfa8c8936a508332220ce1c5a05a96ee3abd45ce73cff |
| Logs/FMDemoB11C1Compile.log | 61085 | 6666e89be270c323ba8aa81a31183510b639e938f9ee94e1bed732399d55ff5e |
| Logs/FMDemoB11C1Tests.log | 89058 | dcc44ddb976ee392b65a2e9d228a99bf09141896d7dc016ac82c7c1065308378 |
| FMDemoB11C1-EditMode.xml | 1666182 | 6df01fa60096bc32c1dd2691a623637d00e119921933ca3f7192022f0e5aeb64 |

红失败日志和 XML 除保留原文件外，完整字节以 brotli+base64 收入 failedAttempts，已解压核对长度和 SHA；本次没有覆盖任何产物。原 B11 失败档案只引用原 scope SHA，未重写。29 项受保护输入（含原 B11 报告、scope、R 报告及最终三产物）SHA 均保持。

## 原七项验收的回归证据

| 原项 | 本次全量绿测试保持的证据 |
| --- | --- |
| ①基础五片 | FreshPublicOwnersRoundTripThroughTheActualEnvelope；输入列表复制、只读发布集合及重复根共享用例均 Passed。 |
| ②历史和保留根 | 两代回退、原 Range／首次 Superseded、退出后保留 Run／Rollback、跨面／救援均 Passed；新增控制组及两个 F1 负例补上同值异对象错连拒绝。 |
| ③后续与随机 | LoadedAndOriginalBranchesHaveIdenticalNextActionRandomPrdAndFinalReport、014 RecordedReplay、历史 Participant／偏好修订、HugeSceneRevisionAndMaterialCounts 均 Passed。 |
| ④两次提交界限 | WpsIsASeparateRecoverableSubmissionBeforeAnyEndOrReward、普通联合结束、恢复后重复增量 0、收讫／金额／身份冲突拒绝用例均 Passed。 |
| ⑤定义／时间／数值 | 旧定义与时钟域、精确分数和大整数、原奖励上下界／TermsUsed、声明缺失及版本错误拒绝用例均 Passed。 |
| ⑥腐坏与资源 | 原固定字节、独立腐坏、历史／条件／贡献／终态、总字节／集合／字符串／Math 限额及同源重试均 Passed；新增 Decode 负例经 015A 完整性验证后在指定业务字段拒绝。 |
| ⑦范围与回归 | 原 2522 条 fullname 多重集合及逐名 Passed 全保留；3 新增通过，469 非目标项及 260 meta/GUID 保持，所有最终执行对应 green-1。 |

原七项的完整场景说明及先前运行历史沿 [原实施交付](demo-015b-delivery.md) 和冻结原 R 报告；本次以未改旧测试的实际全量结果确认回归。未扩大成功 Link 支持域。

最终 471 项规范 SHA256：2e447f19aabed033939d88d90c3b712c8cf0505fb19f4ec9d089db45d7bd1b99。
[本次 scope](demo-015b-c1-scope.json)：573057 字节（≤600 KiB），SHA256 a09e689364639e9d69c50572092dde32bdc4857f459c8b55caeb0666d036c808。
规范清单算法：Ordinal 路径 + TAB + 小写 SHA256 + LF，以 UTF-8 无 BOM 求 SHA256。原两文件完整字节和旧 2522 案例数组均保存在 scope，并已核对可解码性。
本交付回传原 R 与 SD00 后，C 结束本原实施回合并停改；R 须等该 turn completed 后独立复审。无用户待答或重授权事项。
