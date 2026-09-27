# 025-P2B-PREPARE-C2 独立审查

唯一正式结论：**ACCEPT**。

025-P2B-PREPARE（含 C1/C2）本阶段独立核对通过。原唯一 R1 已关闭，本阶段无剩余待修项；依 §309 生成真实内容审查凭证。此结论接收精确候选内容及准备阶段，不接收尚未执行的真实 Publish、activate 或首 Demo。

## 1. 本次准确回合与完成门

- 工程：D:/Unity/UnityProj/FightMatch。
- C task：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；标题「FightMatch 本机保存与应用接入实现」。
- 本次 C turn：01a0d3fd-4b73-7102-b92c-a59b0d45d96a；原生 context：2026-09-24T15:16:27.720Z，gpt-6-astra / max。
- C 非异步 formal final：2026-09-24T15:58:06.729Z，COMPLETED；原生 task_complete：2026-09-24T15:58:07.017Z，error=null，duration=2499321ms；两处正式文本一致。
- R task：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；标题「FightMatch Demo 独立代码审查」。
- 本次 R turn：01a0d3fe-25b0-72c0-8e44-8500ba78de4b；原生 context：2026-09-24T15:17:23.616Z，gpt-6-astra / max。
- R 在 C 自然完成门通过后开始正式接收核验；2026-09-24T15:58:29.4888613Z 独立重算两报告身份与 formal final 相符。未把文件提前出现或作者 PASS 作为独立结论。
- §311 的 SD00 收件通知指向上述准确 C/R；本报告不预填 R 的 completed 时间，R 的自然完成与正式交付由原生事件及 final 外部绑定。

| 本次作者正式输出 | 字节 / 行 | SHA256 |
| --- | --- | --- |
| docs/system-design/2026-09-17/demo-025-p2b-prepare-c2-delivery.md | 15036 / 127 | 2e89e1cb138d8adce04785a6e02eeb9399d1088e46690743fe1c1ddb992280e1 |
| docs/system-design/2026-09-17/demo-025-p2b-prepare-c2-scope.json | 1564823 / 35567 | da2a19b47161f0cc7d9f2a5cff5ce96b93d8dcc67ce6f1c102f6fb1493993269 |

§308～309 冻结摘要为 1f0e1639396e90d77851e092ee5f66a370b636c6cc01405803357934978fed9a，按范围首标题起、下一节前止，CRLF→LF、TrimEnd 后补一个 LF、UTF-8 重算相符。原 §292～294、296、298、301～302、304 冻结摘要亦与前次已核值相符；三协调稿的后续追加未改冻结范围。

## 2. R1 与受影响条件

PublishedContentCodec.cs:58～67 的唯一生产改动在 Parser.Parse 后、ConvertValue 前检查 node != null；失败使用既有 ContentFailure("InvalidSchema", "Root")。外层 PublicationResult.Run 形成原有拒绝结果，避免 source.SchemaVersion、review.OriginalBytes、binding 索引和 release-set 解引用空对象。原字节预算、UTF-8 错误处理、字段校验、规范编码和精确数学预算保持。

该检查只作用于 Decode<T> 根；ConvertValue:128 的内部可空转换未改，未增加 catch-all，也未修改 Catalog、Models、Authoring、公开 API 或格式。原 Codec 入口 18874 字节 / SHA256=1d529b7e20386f0508bc02c222b2859bfc8ebd59bc155b858e41da366c927dee；最终 18992 字节 / SHA256=c64558961a7958b6ab426ecf5da8c47f3e62a76c47fc4bf96eb5cf5813755726。

新增用例已逐条读实现并核 XML：
- SourceNullRootIsRejectedWithoutThrowing：普通 null、带合法空白 null 两例；无异常、IsAccepted=false、InvalidSchema / Root。
- ReviewNullRootIsRejectedWithoutThrowing：同上两例，覆盖真实 review 文件解码入口。
- NullPublishedIndexOrReleaseIsRejectedWithoutWritesOrFallback(False/True)：两例各先成功发布隔离 fixture，再加入另一个实际可解析 binding 与 release；只将目标索引或显式 release 变成 null，目标准确拒绝，另一个 binding/release 仍可准确读取；调用前后的写次数、全部键和全部 blob 字节不变。
- ApprovedNestedNullsRemainValidAndKeepCanonicalPayload：Growth/Inventory/Progression/Reward.Context、NewProfile.Recovery、首关 UnlockAfterLevelId 保持合法 null，并核重新编码与原规范 payload 完全相同。

| 条件 | 独立判断与继承依据 |
| --- | --- |
| B01 | 根 null 结构拒绝已补齐；内部 null、规范字节、预算及旧负例保持。新九件与原批准内容逐字节相同，原393字段与31参数审查有效。 |
| B02 | 前次已核真实精确数学、几何、隔离完整回放、冻结 Prepared、revision/cancel 与共用草稿锁保持；相应生产文件身份未变，新实际 prepare 已重新通过。 |
| B03 | review 根 null 拒绝已闭合；原七类写入、同 op 同意图优先、异意图拒绝、逐件复读、部分/未知写保留及草稿 gate 结论保持；本轮仅隔离 fixture 和获准既有 Windows probe。 |
| B04 | 坏 binding 索引及显式 release 根 null 已准确拒绝，无异常、写入、替换或 latest 回退；五字段 binding、收据完整性、能力门与冻结新档的原结论保持。 |
| B05 | 原多关 V2 fixture、保留 V1 精确解析及错版本/删除 V1 拒绝用例仍通过；共享规则核、实际仅 L1 的内容范围保持。 |
| B06 | 新编译、真实新 prepare、全量测试与过程/身份/范围审计均通过；所有失败和历史证据保留，详见下文。 |

继承的独立报告：demo-025-p2b-prepare-c1-code-review.md，22094 字节 / 140 行，SHA256=a7fcbfefc5cc5e6234495186c6a022312d63bb468c63f27f4b71a2d9b0396f75；其历史 NEEDS_FIX 与唯一 R1 原文保留，不改写为先前已经通过。

## 3. 实际红绿验证与新生产身份

五个运行的 run/result/before/after、日志及适用 XML 已独立核身份、顺序与内容；所有时间为 2026-09-24 UTC。均为原 C 串行调用 Unity 2022.3.18f1。

| 号 / 模式 | PID | 起止 UTC | actual exit | 实际结果 |
| --- | --- | --- | --- | --- |
| 001 / Tests | 22032 | 15:25:20.4606189～15:25:30.4007726 | 1 | 新正例曾错误引用 LevelInput.Context，CS1061；无 XML、无实际测试执行，完整保留失败。 |
| 002 / Tests 红测 | 3232 | 15:28:28.1221431～15:40:04.1006310 | 2 | 3563 总数、3557通过、6失败、0跳过；六个新负例均为预期 NullReferenceException。 |
| 003 / Compile | 24732 | 15:41:18.4444574～15:41:30.0377777 | 0 | 中央 guard 修复后的实际编译通过。 |
| 004 / Prepare | 27324 | 15:42:24.1619325～15:42:36.1257175 | 0 | 实际 Authoring.Run -fmMode prepare；新根输出九件，日志记录 PREPARED 与准确指纹。 |
| 005 / Tests | 18372 | 15:44:27.8296319～15:55:30.8748499 | 0 | 3563 / 3563通过，0失败、0跳过。 |

001 的 CS1061 是新增断言笔误，不能算 R1 红测。其新测试源 13253 字节 / SHA256=8020f4f2be0b1d0bf5330ed2a6f2166549c747868134607b05af8b33f39159d3；修正后 002 的两个测试文件已为最终身份，生产 Codec 仍为原入口 SHA。001 与 002 的 failure.json 分别与当次 result.json 同字节；未覆盖或伪绿化。

原 C1 的3556例在红测和绿测均完整保留且全通过；按 fullname 多重集核对，既有同名参数例的重数也未改变。红测额外通过内部 null 正例，六个新增负例失败；最终新增七例全通过。旧测试文件正文无删除，两个测试文件仅新增23、26行，未削弱旧断言。C1有3551个不同 fullname、3556个用例；C2有3558个不同 fullname、3563个用例，不能误以集合去重后的数量替代总数。

| XML | 字节 | SHA256 |
| --- | --- | --- |
| C1 runs/002/tests.xml | 2495834 | 38474a8f4b8125325ba93d4878ee54f6e0979af2d48be4725172d769250fb125 |
| C2 runs/002/tests.xml | 2514095 | 2bc051623688c3c1fd2b3df745538fd6b2077a2b8a177f87a305d3ce66d15766 |
| C2 runs/005/tests.xml | 2501518 | 23a835e60e4be9c12084c692231be9e7be7440cb5a842128cf6338702198f6d0 |

Tests 的实际 argv 为 -batchmode -nographics -runTests -testPlatform EditMode 与准确项目/results/log，无 -quit 或 filter；Compile 和 Prepare 使用获准 -quit。Prepare 明确执行 FightMatch.Content.PublishedContentAuthoring.Run、批准 source 和新 C2 authoring 根。每次前后均记录700实现、716 Assets、3内容文件、308保护输入、36 DLL/PDB及工具；源码/资产/内容/保护输入/工具在各运行内不变，同工程 Unity 过程边界为空，PID和起止对应且无重叠。

003 后的全部36项 DLL/PDB 与 004 前后、005 前后、最终 scope 和当前实物逐项一致。相对 C1 仅以下四项变化，其余32项相同；生产二进制已经合法改变，未套用旧“生产全部同版”免跑 prepare 条件。

| Library/ScriptAssemblies/ 文件 | 字节 | SHA256 |
| --- | --- | --- |
| FightMatch.Content.dll | 122368 | a52e3c19ae9ef6017ccc4a5fecce4613008157f95d1d5184f47c146a486a7a55 |
| FightMatch.Content.pdb | 43276 | e77907eb6a36eff6b5477ae35a1d2d7375d63f915b4670168c4c853368e461fe |
| FightMatch.Core.Tests.dll | 1209344 | 233371573ef9c75ba2857cd6a019e153d0d8106ffb6ec0d1c3c38ef807666324 |
| FightMatch.Core.Tests.pdb | 355460 | c54849e686ab77445b81be9492758ecf50a3b5019e6d27dab282256b2e0c6c4a |

新 prepare 的外部证据位于 TestArtifacts/FMDemo025P2/p2b-prepare-c2/runs/004：
- run.json：1297字节 / f2c5026f6940afa1670c1eb5270be073ee0b8e3471d1f0589672cae98db2c4d7。
- result.json：1145字节 / e13e434496931e65c40ff80fff57d7b2dbf5ea1035186a0d4b8b6872b53774ab。
- before.json：484744字节 / 56d0f1c731e4b58bdc81a3c17658ec75c113e9140f1e56ec818103f1ff10f2be。
- after.json：484744字节 / 0c13007f60e1042afd3f5db888ead39664aa41fd85c8817c318de192fd0ca067。
- prepare.log：58802字节 / d5dba93c4fb7dff69507aa1a720fb65bf7d7ffbccc0fa7fee7d9c96386aee47c。

## 4. 新九件与原内容批准

2026-09-24T16:08:40.5680251Z 独立读取新旧九件并逐字节比较，九项全部完全相同；当前 Assets/FightMatchContent/demo-r1.source.json 亦与新 source-copy 同字节。以下均为新 C2 authoring/ 下的实际身份。

| 文件 | 字节 | SHA256 |
| --- | --- | --- |
| source-copy.json | 20443 | fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a |
| prepared.fmpackage.bytes | 11486 | b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129 |
| prepared.fmvalidation.bytes | 664 | 512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d |
| prepared-descriptor.json | 2173 | a5a06494af58cb918f18d8f55ade689933b45ad00f2a5a1eec99b45458c862b7 |
| field-map.json | 17301 | 59db573a43926f8c36d072ef977b6484a602f08b497d27bbcfb54c6a16d9f717 |
| geometry.json | 656 | 8c1c4347bf196f05efc7b698a6978e6ae449dc55b4f0a59677d75b052cc8592e |
| replay.json | 2389832 | 86e9f3aa1708fcdd9b9f6eb080ab74cc81acba4709f8c0f12e203bb4366cbfc4 |
| review-template.json | 876 | 82e824924c21256ce885a4bc679bece2425701301aca0b6200476a4af9c66a9a |
| result.json | 506 | 1cd65293f6e6ced222da213bf3a7326b2d92f369a9b1610bdeedc8bf81c85a6b |

批准映射 TestArtifacts/FMDemo025P2/p2b-prepare/approved-content-map.json：117327字节 / SHA256=da286b0a01f2bd06c74bef915422f94d76e99b33d00683b33fb3389b5949c9f6。原 stage025p1/review-inputs 的四份批准输入实际身份保持并包含于本次760项不可变导出逐项核验：
- publication-proposal.md：7196字节 / fae6eabd82df49a01138a8e881fe1d9b99cef9712f9b0797e01dc298de5822a5。
- source-ledger.json：34768字节 / 049a70ddc1ad5314ab39f10e490010737ef806e73f3b04b4a263099bad3b492b。
- prd-candidates.json：96954字节 / 1469ae1833d7d9709264c10b29ca065b6bf53c0425bc42ae0ed64fb6c338d3f4。
- replay-evidence.json：12528619字节 / d5117507ca3bc8e44ce26d6708049e5198a886b90056af28bdeb042c8129aa08。

§271/292 的批准及旧候选 5db393d0441880fc387ce7db0421bdf855b3b38b20edbfe818a83144bcbb5a12 保持；旧候选指纹未混作新规范载荷指纹。依 §309，前次已独立核过的393字段映射、31精确 C/epsilon、完整成长定义、几何及隔离回放结论按上述原身份和新 prepare 继承，未重跑原 P1 求参矩阵。

准确新候选为 draft:fightmatch-demo-r1 / Revision="1"；Binding：PackageId=package:fightmatch-demo-r1、ContentFingerprint=b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129、RuleVersion=demo-r1、NumericContractVersion=RC01、RandomContractVersion=PC01+SC01；DefinitionBindings 仅 level:ch01-01 / LevelVersion="1"。

冻结新档仍为 new-profile:default / RecordVersion="1"，规范字节298 / SHA256=36f2d8935f626c4bdafb53c56ac288f5199e7a6dec29680c0fac02f354685646。实际只有已批准 L1、AssumedBottomLeft、非全板路径与原奖励；没有追加关卡、奖励、唯一解检测、真实随机种子或 PlayerSave。result.json 仍明确 PublicationInvoked=false，原 UNREVIEWED 模板未修改。

## 5. 完整范围、历史证据与预算

2026-09-24T16:05:47.9633884Z 独立核当前700实现、实物716 Assets、379个 meta/GUID、308保护输入及763导出。760项不可变导出全部与 scope 的长度/SHA一致；三协调稿允许追加且冻结范围相符。Assets 实物集合无多缺，所有旧 meta 身份相同、无重复 GUID，保护输入无变化。

相对 C1，700实现仅 Codec 和两个测试文件改变；工具是第四个获准既有改动。763导出由758项加原 C1 delivery/scope、R1报告、C1 root/manifest 五件构成，未丢前序输出。C2 entry-files/final-files 各4件分别与 C1入口/当前最终身份逐项相符；其余副本沿已核 C1 704项继承。

| 文件 | 增 / 删 / 总量 | 最终物理行 | 最终字节 / SHA256 |
| --- | --- | --- | --- |
| PublishedContentCodec.cs | 5 / 1 / 6，≤24 | 260 | 18992 / c64558961a7958b6ab426ecf5da8c47f3e62a76c47fc4bf96eb5cf5813755726 |
| PublishedContentCompilerTests.cs | 23 / 0 / 23 | 156 | 13405 / addbebdcf3c920dbecd42293c65fc280468d8eb289cced87b73e274915eff4cf |
| PublishedContentCatalogTests.cs | 26 / 0 / 26；两测试合计49≤100 | 237 | 19860 / 7d95a2e107770923a902d67e53ffa3760ceea70a60c0e3189fa43acb4cf61123 |
| Invoke-FM025P2Validation.ps1 | 13 / 5 / 18，≤40；原入口累计62+7=69≤160 | 187，≤360 | 13281 / 92de02e9e37c2629320ae0d461463bf820301e9bdd9996b5891e6e41f5a98a84 |

已逐项读四份实际 diff，独立 numstat 与物理行核符，PowerShell AST parser 0错误。工具仅增加固定 C2 stage/root/本次作者 turn/冻结摘要/C1基线和8个固定运行槽；Compile/Prepare/Tests 沿原流程，C1 Prepare 禁令仍保留，无任意命令/根或 Publish/activate 开放。

C2 root-identity.json：786字节 / 8b26bdfb385266d4d0d407559f0ec97d83686d5a29401faf74d8a33c1d743162；read-manifest.json：16346字节 / 4323c10a7a3a66f882f93d6724e0dd0386d0e6058f66ab95c8621bacd65ccd25。75项 manifest 与 scope.evidence.items 完全同项，实物76件包含 manifest 本身；无额外、缺失、重复、越根或 reparse。范围只含 §308.2 固定根文件、4+4副本、九件 authoring 及实际001～005适用叶；未把原 B 的旧角色词表误用为否认本包明确获准 prepare.log 的依据。

2026-09-24T16:08:44Z 重新核原 B、C1 各1442项有限证据：所有路径/角色/字节/SHA/实物集合与原 manifest、scope 一致，各根1443件含 manifest，无额外、缺失或 reparse。原失败、旧报告、入口副本及 UNREVIEWED 模板完整保留。
- B root：516字节 / 49f2e30e864cfcb8b5289b7f57b4ee103ea0bb05c6fb911f1b18cd3709cd46d6；manifest：363700字节 / 9ef1d23b548f11477971f0c1c37ec63f7a9108cb61f07f4a0e4e12ec0c4a8ea8。
- C1 root：739字节 / c2e8bee41daeccfe2c83c6d10adb8faae930aaaa01320ab729f85adae87d64fd；manifest：364997字节 / c5981de520324999eeb00e87ab2e093ce467671505f11f55fe0d713f78b3776c。
- C1 delivery：15987字节 / 972b0f92190e0f71fdc485b35538e1560fdc192084a07a95341b9bab895c3c78；scope：1633214字节 / e13fb09957fa081335a7c1b3fd9155c51f4f89078aa6b4cb93dc2d4f48b96e25。

Windows 实际测试输出为“retained original exact blob and checked idempotency; no overwrite”。既有 writer.lock 为0字节，64a.blob 为4字节 / SHA256=054edec1d0211f624fed0cbca9d4f9400b0e491c43742af2c5b0abebf0c990d8，.work 不存在；无新 probe 创建/Flush/提升/删除权或其新增证明。原 B 创建证明继续继承，此项仍不能推出强杀/断电/Android或完整持久化故障矩阵已验证。

## 6. 真实内容审查凭证与停止边界

依实际 PublishedContentModels.ContentReviewEvidence 的19个公开属性独立构造 docs/system-design/2026-09-17/demo-025-p2b-content-review.json。SchemaVersion=1，Verdict 使用本文唯一结论；Revision/LevelVersion 为规范 BigInteger 字符串；绑定新 C/R 准确 task/turn、§308～309摘要、原批准映射及准确 source/payload/validation/五字段 binding。ApprovalBasis 明确绑定新 Codec、生产 DLL/PDB、C2 root/manifest、实际004 prepare的 run/result/before/after及005全量 XML；不采用模板替换或伪造完成时间。

凭证按实际 Codec 和 Catalog.CheckReview 的属性集合、类型、独立身份、摘要格式、精确字节/长度/版本/DefinitionBindings 与65536字节/8192字符串限制核对。独立核查不执行 Unity 或产品 DLL；未把凭证静态核对表述为已实际 Publish。两份 R 输出的最终长度/SHA在 formal final 外部绑定，不放入自身摘要，避免自哈希环。

本次 R 只写获准报告与真实凭证，未改实现/工具/meta/旧报告，不写 Git，不新建任务/代理/跟进。旧258项语义读取许可保持；前次 R 额外读取 config.json 的偏差及其归属仍留在原报告，本次没有重复该语义读取，也不以它作为内容批准依据。

原 P2A、P1/027 与无关已接收成果保留；34功能/28已接收/余6/51正向交付不增。真实 Publish/activate、PlayerSave、P2C/页面、025整体/B17、CONT-A/B/C、028/029、首Demo，以及物理鼠标/触控、像素/交互PlayMode、合法AwaitLinks、Player/Android、CI/lint/perf与 §184 旧 NOT VERIFIED 均未由本阶段验证补齐。SD00 应在本次 R 自然 completed、formal 唯一结论与两份输出实际身份核符后另签真正 Publish；本任务交付后停改。
