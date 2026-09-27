# 025-P2B-PREPARE-C2 作者交付

作者状态：COMPLETED。独立验收：PENDING。唯一 R1 的实现与本轮规定验证已完成；此状态不代表原 R 已 ACCEPT、真实内容已发布或首 Demo 已验收。

原 C task：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本次准确 turn：01a0d3fd-4b73-7102-b92c-a59b0d45d96a。
模型／effort：gpt-6-astra／max；原生 turn_context 时间：2026-09-24T15:16:27.720Z。
工程根：D:/Unity/UnityProj/FightMatch。本报告形成时间：2026-09-24T15:56:48.4456059Z；实际任务 completed 时间以宿主完成回执为准，未提前编造。
授权：system-task-packets.md §308～309；规范化 SHA256：1f0e1639396e90d77851e092ee5f66a370b636c6cc01405803357934978fed9a。§310 登记了原 C/R 两个准确新回合。

## 入口与原证据

从 C1 合法最终 700 实现／716 Assets／379 GUID 续接。入口逐项核符 758 导出中的 755 个不可变项；三协调稿取最新并核冻结摘要。未退回 A，未撤销原源码或批准范围例外。
C1 的 1,442 项和原 B 的 1,442 项有限证据逐项核 SHA256／长度／无 reparse；所有原报告、失败证据、entry/final 副本、probe 和根／resume 链保留。其余 C1 704 份最终副本沿原 manifest 继承；本根只新增四个可改文件的 entry/final 各四份。

| 输入 | 字节 | SHA256 |
| --- | ---: | --- |
| docs/system-design/2026-09-17/demo-025-p2b-prepare-c1-delivery.md | 15987 | 972b0f92190e0f71fdc485b35538e1560fdc192084a07a95341b9bab895c3c78 |
| docs/system-design/2026-09-17/demo-025-p2b-prepare-c1-scope.json | 1633214 | e13fb09957fa081335a7c1b3fd9155c51f4f89078aa6b4cb93dc2d4f48b96e25 |
| docs/system-design/2026-09-17/demo-025-p2b-prepare-c1-code-review.md | 22094 | a7fcbfefc5cc5e6234495186c6a022312d63bb468c63f27f4b71a2d9b0396f75 |

上轮独立报告唯一 verdict 为 NEEDS_FIX，仅 R1（JSON 顶层 null）；原 R turn 为 01a0d3c9-6eaf-74f0-8929-955d0727f06d。其他已核结论按 §309 继承，仍由本轮原 R 复核继承条件。
未语义读取 docs/game-design/balance/config.json；旧 258 项语义许可未扩大。

## 精确修改

| 文件 | 增／删 | 总增删／预算 | 最终字节 | 最终 SHA256 |
| --- | ---: | ---: | ---: | --- |
| Assets/Scripts/FightMatch/Content/PublishedContentCodec.cs | 5 / 1 | 6 / 24 | 18992 | c64558961a7958b6ab426ecf5da8c47f3e62a76c47fc4bf96eb5cf5813755726 |
| Assets/Tests/EditMode/FightMatch/PublishedContentCompilerTests.cs | 23 / 0 | 两测试合计 49 / 100 | 13405 | addbebdcf3c920dbecd42293c65fc280468d8eb289cced87b73e274915eff4cf |
| Assets/Tests/EditMode/FightMatch/PublishedContentCatalogTests.cs | 26 / 0 | 两测试合计 49 / 100 | 19860 | 7d95a2e107770923a902d67e53ffa3760ceea70a60c0e3189fa43acb4cf61123 |
| Tools/Invoke-FM025P2Validation.ps1 | 13 / 5 | 18 / 40 | 13281 | 92de02e9e37c2629320ae0d461463bf820301e9bdd9996b5891e6e41f5a98a84 |

PublishedContentCodec.cs:63 在 Parse 得到根节点后、ConvertValue 之前执行 Need(node != null, "InvalidSchema", "Root")。现有 PublicationResult/ContentFailure 拒绝机制处理错误；未改 Parser、嵌套 null 转换、Catalog、公开 API、格式、预算或 catch 范围。生产只改此文件。
Compiler 测试新增 source/review 的原样 null 与合法空白包围 null，共四个负例；正例检查 Growth.Context、Inventory.Context、Progression.Context、Reward.Context、NewProfile.Recovery、UnlockAfterLevelId 仍为合法 null，并断言重编码规范 payload 完全相同。
Catalog 测试新增 binding 索引与显式 release-set 的两个根 null 负例。先发布有效隔离 fixture 并另备有效较新版，损坏准确目标后检查无异常、拒绝码 InvalidSchema、字段 Root；较新版只能显式读取，原坏目标不回退。前后写入计数、全部键及每个 blob 字节不变。fixture 使用内存存储。
两测试文件仅新增 49 行，零删除，全部旧测试体／断言原样保留。原 DemoContentCompilerTests.cs 及其上轮获准修正未再改动。
工具只增加固定 C2 Stage／根／准确 task-turn／摘要及 C1 最终输入清单绑定，并允许 C2 沿原 Prepare 流程输出到本根。C1 的 Prepare 禁令保持；没有 Publish 模式、任意命令或任意输出根。
工具累计相对原始入口总增删 69 / 160；最终 187 / 360 物理行；PowerShell parser 0 错误。完整逐行差异见 line-budgets.json。

## 实际运行与红绿证据

Unity：D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe。均由原 C 经正常工具审批串行执行，运行前检查同工程进程，未杀 Editor、删锁或要求再次激活。每号对应一个实际过程；所有失败及中间记录保留。

| run | 模式 | PID | UTC 起始 | UTC 结束 | Unity 实际 exit | 结果 |
| --- | --- | ---: | --- | --- | ---: | --- |
| 001 | tests | 22032 | 09/24/2026 15:25:20 | 09/24/2026 15:25:30 | 1 | CS1061，未执行测试 |
| 002 | tests | 3232 | 09/24/2026 15:28:28 | 09/24/2026 15:40:04 | 2 | 3,563：3,557 通过／6 失败／0 跳过 |
| 003 | compile | 24732 | 09/24/2026 15:41:18 | 09/24/2026 15:41:30 | 0 | PASS |
| 004 | prepare | 27324 | 09/24/2026 15:42:24 | 09/24/2026 15:42:36 | 0 | PASS |
| 005 | tests | 18372 | 09/24/2026 15:44:27 | 09/24/2026 15:55:30 | 0 | 3,563 全通过／0 失败／0 跳过 |

001 是新增正例中的作者笔误：误用 LevelInput.Context，编译返回 CS1061。仅修正新增断言至真实 DTO 字段后才启动 002；001 无测试 XML、不计缺陷红灯。原错误测试文件 13,253 字节／SHA256 8020f4f2be0b1d0bf5330ed2a6f2166549c747868134607b05af8b33f39159d3 已由 before/after 绑定，原日志／failure 保留。
002 执行时生产 Codec 仍是入口 18,874 字节／1d529b7e20386f0508bc02c222b2859bfc8ebd59bc155b858e41da366c927dee。原 3,556 项全部通过；实际新增七个 fullnames，其中六个负例均由公开边界抛出 NullReferenceException，内部合法 null 正例通过。Unity 实际 exit=2；验证脚本对失败返回 1，两者没有混写。
取得真实红灯后才加中央 guard，随后依次运行 003 Compile、004 Prepare、005 全量 Tests。最终原 3,556 个 fullname 多重集全部保留并通过，新增七项全部通过；没有删测、跳过、过滤或削弱旧断言。

| 新增用例 | red002 | green005 |
| --- | --- | --- |
| FightMatch.Core.Tests.PublishedContentCatalogTests.NullPublishedIndexOrReleaseIsRejectedWithoutWritesOrFallback(False) | Failed | Passed |
| FightMatch.Core.Tests.PublishedContentCatalogTests.NullPublishedIndexOrReleaseIsRejectedWithoutWritesOrFallback(True) | Failed | Passed |
| FightMatch.Core.Tests.PublishedContentCompilerTests.ApprovedNestedNullsRemainValidAndKeepCanonicalPayload | Passed | Passed |
| FightMatch.Core.Tests.PublishedContentCompilerTests.ReviewNullRootIsRejectedWithoutThrowing("null") | Failed | Passed |
| FightMatch.Core.Tests.PublishedContentCompilerTests.ReviewNullRootIsRejectedWithoutThrowing(" \r\n\t null \t\r\n ") | Failed | Passed |
| FightMatch.Core.Tests.PublishedContentCompilerTests.SourceNullRootIsRejectedWithoutThrowing("null") | Failed | Passed |
| FightMatch.Core.Tests.PublishedContentCompilerTests.SourceNullRootIsRejectedWithoutThrowing(" \r\n\t null \t\r\n ") | Failed | Passed |

| XML | 字节 | SHA256 |
| --- | ---: | --- |
| TestArtifacts/FMDemo025P2/p2b-prepare-c2/runs/002/tests.xml | 2514095 | 2bc051623688c3c1fd2b3df745538fd6b2077a2b8a177f87a305d3ce66d15766 |
| TestArtifacts/FMDemo025P2/p2b-prepare-c2/runs/005/tests.xml | 2501518 | 23a835e60e4be9c12084c692231be9e7be7440cb5a842128cf6338702198f6d0 |

工具调用为 Tools/Invoke-FM025P2Validation.ps1，固定 -Stage 025-P2B-PREPARE-C2、-ExpectedScriptSha256 92de02e9e37c2629320ae0d461463bf820301e9bdd9996b5891e6e41f5a98a84；按表逐次传 -Mode Tests/Tests/Compile/Prepare/Tests。真实 Unity argv、PID、起止与 process.ExitCode 均在各 run.json/result.json；stdout/stderr 即使空文件也保留。
Compile 使用 -batchmode -nographics -quit；Prepare 增加 -executeMethod FightMatch.Content.PublishedContentAuthoring.Run -fmMode prepare，原批准 source 和本 C2 authoring 根；Tests 使用 -batchmode -nographics -runTests -testPlatform EditMode 和本次准确 results/log，无 -quit、无 filter。
每次 before/after 绑定完整 700 实现、716 Assets、3 内容项、308 保护项、工具和 36 DLL/PDB。001 的新增错误正例、002 的旧 Codec 与最终源码分别按实际版本核对；各次二进制身份相衔接。003 编译后与 004/005 前后全部 36 项一致。

| 最终实际变化的二进制 | 字节 | SHA256 |
| --- | ---: | --- |
| Library/ScriptAssemblies/FightMatch.Content.dll | 122368 | a52e3c19ae9ef6017ccc4a5fecce4613008157f95d1d5184f47c146a486a7a55 |
| Library/ScriptAssemblies/FightMatch.Content.pdb | 43276 | e77907eb6a36eff6b5477ae35a1d2d7375d63f915b4670168c4c853368e461fe |
| Library/ScriptAssemblies/FightMatch.Core.Tests.dll | 1209344 | 233371573ef9c75ba2857cd6a019e153d0d8106ffb6ec0d1c3c38ef807666324 |
| Library/ScriptAssemblies/FightMatch.Core.Tests.pdb | 355460 | c54849e686ab77445b81be9492758ecf50a3b5019e6d27dab282256b2e0c6c4a |

Content DLL/PDB 与 Core.Tests DLL/PDB 合计四项变化，其余 32 项保持 C1 身份；不能沿用 C1 的“30 非测试二进制全部同版”表述来免跑 prepare。

## 同源内容重新 prepare

004 使用真实公开 prepare 流程和本轮最终生产 DLL，返回 PreparedPendingIndependentReview，PublicationInvoked=false；验证 1 个关卡、31 个精确参数行、几何与回放。以下九件均与原 B 对应文件长度及 SHA256 完全相同。

| authoring 文件 | 字节 | SHA256 | 与原 B |
| --- | ---: | --- | --- |
| authoring/result.json | 506 | 1cd65293f6e6ced222da213bf3a7326b2d92f369a9b1610bdeedc8bf81c85a6b | 字节相同 |
| authoring/review-template.json | 876 | 82e824924c21256ce885a4bc679bece2425701301aca0b6200476a4af9c66a9a | 字节相同 |
| authoring/replay.json | 2389832 | 86e9f3aa1708fcdd9b9f6eb080ab74cc81acba4709f8c0f12e203bb4366cbfc4 | 字节相同 |
| authoring/geometry.json | 656 | 8c1c4347bf196f05efc7b698a6978e6ae449dc55b4f0a59677d75b052cc8592e | 字节相同 |
| authoring/field-map.json | 17301 | 59db573a43926f8c36d072ef977b6484a602f08b497d27bbcfb54c6a16d9f717 | 字节相同 |
| authoring/prepared-descriptor.json | 2173 | a5a06494af58cb918f18d8f55ade689933b45ad00f2a5a1eec99b45458c862b7 | 字节相同 |
| authoring/prepared.fmvalidation.bytes | 664 | 512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d | 字节相同 |
| authoring/prepared.fmpackage.bytes | 11486 | b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129 | 字节相同 |
| authoring/source-copy.json | 20443 | fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a | 字节相同 |

源内容、规范 payload 和 validation 的批准身份均保持。原 393 字段批准映射、31 C/ε、映射／几何／回放结论在新 prepare 证据和相同输出下继承；未重复原 P1 求参矩阵。UNREVIEWED 模板仍是原字节，作者没有把模板改成真实 review。

## 保全、有限导出与完成边界

最终仍为 700 实现／716 Assets／379 GUID。三份 .cs 合法变化，其余 697 实现保持；480 个生产输入中仅 Codec 变化，另外 479 保持。所有 meta、asmdef、source、其他生产和旧测试、308 保护项均核符，Assets 路径集合无增删。
本次根的 75 个有限条目按 role／字节／SHA256 列于 read-manifest.json，manifest 不自哈希；含它实际共 76 件。固定根文件 20 件，entry/final 各 4 件，authoring 9 件，runs 001～005 共 39 件。没有额外根、探针或源码。
最终 scope 保留全部原 758 导出，更新获准四项及三协调稿，并追加 C1 两报告、原 R 报告、C1 根／manifest 五项，共 763。两份当前作者报告由 formal final 外部绑定，不形成自哈希环。
原 B/C1 的 1,442 项证据各自重新核符，scope 分别记录有限 binding；原 B failed run003、C1 completed run002 及原报告全部保留。
Windows probe 仅按 §304 打开既有 lease、复读及同字节幂等／异字节拒绝。red002、green005 均输出 retained original exact blob and checked idempotency; no overwrite。writer.lock 仍 0 字节，64a.blob 仍 4 字节，.work 不存在；创建／Flush／提升只继承原 B 证据。

| 本轮关键绑定 | 字节 | SHA256 |
| --- | ---: | --- |
| TestArtifacts/FMDemo025P2/p2b-prepare-c2/root-identity.json | 786 | 8b26bdfb385266d4d0d407559f0ec97d83686d5a29401faf74d8a33c1d743162 |
| TestArtifacts/FMDemo025P2/p2b-prepare-c2/entry.json | 10316 | 129a73628de7ccaf8aaa57fb4e13a679c787fad14aae754fae6095d251a99cdb |
| TestArtifacts/FMDemo025P2/p2b-prepare-c2/read-manifest.json | 16346 | 4323c10a7a3a66f882f93d6724e0dd0386d0e6058f66ab95c8621bacd65ccd25 |
| TestArtifacts/FMDemo025P2/p2b-prepare-c2/validation.json | 33560 | 3a6805485fba537eec43e1e0bb447fb916e56ddc0fb78354a3226f4da9cfe240 |
| TestArtifacts/FMDemo025P2/p2b-prepare-c2/inherited-evidence.json | 20582 | 1eb1f3418f0d4d5d21a85dd657616761d8c720ebbf3f6dda59f15c980bfe5a03 |

C 作者逐项结果：R1、B01、B02、B04、B05、B06 为 PASS；B03 为阶段限定 PASS（当前真实 probe 仅既有字节读取／lease／幂等边界）。完整依据见 scope.criteria；所有作者 PASS 不等于独立 ACCEPT。
未运行真实 Publish、activate、StreamingAssets 导出、PlayerSave、新档创建、P2C／页面或 Player/Android build；未执行 Git、创建任务／代理或恢复五分钟跟进。旧 §184 NOT VERIFIED、物理输入／交互像素、legal AwaitLinks 等未验证边界保持。

## 下一实施范围交接

本轮仅关闭唯一 R1，不扩新功能。下一实施建议保持 C1 已交付的六件不可变内容包方案：source、payload、validation、真实 review、原始 PublicationRecord、显式 release-set；将 source 作为最小额外资产。原始 record 三个准确存储键只有在导出器验证各实际字节完全相同后才能别名读取，storage 保持只读。
该建议仍未实施／未运行。真正实施门为：本次准确 C completed 与 formal final → 原 R 在准确 C2 回合独立整体 ACCEPT，并按真实格式生成 content-review → SD00 核符后另签 Publish 范围。原 R 本次 turn 为 01a0d3fe-25b0-72c0-8e44-8500ba78de4b。
原提案的修改 Authoring／验证工具、新增 FirstReleaseContentStorage／对应测试、六件 StreamingAssets 名称及技术参数仅随 scope.nextScope 原样继承。发布与激活仍分开，P2C/029 后续消费同一个 Catalog；本包没有这些执行权限。

交付文件：docs/system-design/2026-09-17/demo-025-p2b-prepare-c2-delivery.md；docs/system-design/2026-09-17/demo-025-p2b-prepare-c2-scope.json。当前报告实际长度和 SHA256 由本次非异步 formal final 给出。
