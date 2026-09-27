# 025-P2B-PREPARE 交付记录

状态：**BLOCKED_SCOPE_EXCEPTION_REQUIRED**。B01～B05 实现与对应新测试通过；B06 的全量零失败门未通过，不能提交为已完成或独立 ACCEPT。
任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1，local，gpt-6-astra / max。
本次准确 turn：01a0d338-e586-7973-9050-a705cf6a4789；原生 turn_context：2026-09-24T11:41:59.004Z。
中断前 turn：01a0d274-f73b-78f1-9055-f770107acc4b。根描述、入口及 681 份入口副本保持，resume-001.json 绑定真实续接链；不把旧回合当本次完成。
工程根：D:/Unity/UnityProj/FightMatch。以下工程相对路径均从该根解析。

## 唯一阻塞与精确修正

tests003 唯一失败：`FightMatch.Core.Tests.DemoContentCompilerTests.NullAndTinyBudgetCannotCreateCandidate`。
原文件 `Assets/Tests/EditMode/FightMatch/DemoContentCompilerTests.cs:141` 仍禁止 Content 程序集引用 FightMatch.Platform；§293.1 已批准新增该引用，而同节另规定旧测试只读。
这是实际程序集引用触发的旧断言冲突；空输入、小预算及“旧 DemoContentCompiler 没有 Publish/ResolveExact”断言均已执行通过。
原旧测试 14552 字节、SHA256=437f883927ed8a4e7bea950d89e041985f90fe99342078f9abd05d943ef5e707，仍保持原字节。
请求的准确例外是替换该行并紧接增加一行（共 2 行新增、1 行删除）：

```csharp
Assert.IsFalse(references.Any(n => n.StartsWith("Unity") || n == "QFramework" || n == "FightMatch.Application")));
Assert.IsTrue(references.Contains("FightMatch.Platform"));
```

保留全部原用例、其余断言和候选算法；不跳过、删除或过滤测试。该例外尚未取得授权，**未实施**。
按 §293 向 SD00 发出收束请求及唯一重试，自动审批均超时，无成功回执；只读核 SD00 仍为原已结束协调回合，未取得新补充。
已向用户提出同一精确授权问题，尚待答复。这里报告审批超时，不将它解释为行为不安全或已被拒绝。
后续获准后需保存本次失败及报告，在准确补充的修正/证据版本范围内完成最终编译与全量 EditMode；不得覆盖本次 authoring、final-files 或失败记录。
有效 prepare002 可由新补充明确按生产源码/DLL保持来继承；如果需要新 prepare，须先追加准确新输出名，不能覆盖原九件。

## 授权与保留

| 范围 | 规范化 SHA256 |
| --- | --- |
| 原 §292～294 | 687ef834f7b556fe1f6c8ed75e3fe406e58757292b45eda021382b707054b2de |
| §296 Windows 最小 probe | 4941fa1fca9109a13ae38e66dbdb29979993eb936a3e5e15e0028a5de006e637 |
| §298 恢复 | e3d1987c0d3eb5a982fff8fd51aea6dca02bc87cda9fd9426a33011806394768 |

P2A 已按 §291 独立 ACCEPT。入口核 725 导出：722 不可变项匹配；3 份协调稿单列允许更新，不以旧观察 SHA 报污染。
本次另核 P2A manifest 1454 项全部保持长度/SHA，仅做身份保全检查，没有重跑 A 验证。
原实现 680、资产 693、GUID 367，3464 个 fullname 多重集及其所含前 3424 项均继承。
§296 澄清 27 为 C25+R2；本 C 只写原明表 25 项，未代写 R 报告或真实 content-review.json。
Git 命令、分支、提交、推送和工作树均未执行；原源码/报告/证据及许可失败保留。

## 实现与边界

新增 7 个生产文件，均位于 Assets/Scripts/FightMatch/：

- Content/PublishedContentModels.cs：有界 source、完整不可变新档定义、待审产物、review/release 数据契约。
- Content/PublishedContentCodec.cs：严格 JSON 类型图、重复/未知/缺字段拒绝、严格 Unicode、精确整数/分数、规范字节和预算。
- Content/PublishedContentCompiler.cs：规范化集合，复用原精确参数、几何及完整回放；调用 P2A 正式上下文入口和原共享成长/背包/进度/奖励核。
- Content/PublishedContentCatalog.cs：草稿串行提交、同 op 意图防冲突、完整持久与复读、准确绑定解析、显式 release-set 读取。
- Platform/ContentPublicationStorage.cs：不可变 blob 与单 writer 的平台契约、明确未知/损坏诊断。
- Platform/WindowsContentPublicationStorage.cs：本地规范路径/拒绝 reparse、排他 lease、create-new、Flush(true)、不覆盖提升及准确读回。
- Content/PublishedContentAuthoring.cs：Run/Prepare/Publish 作者 API；本轮只执行 prepare，实际输出待独立审查材料。

原 DemoContentModels 只增内部同草稿串行门；原 Content.asmdef 只追加 FightMatch.Platform；DemoContentCompiler 原字节完全保持。
Tools/Invoke-FM025P2Validation.ps1 仅增加本段 Stage/根/入口清单、恢复身份及 prepare 参数采集，不提供任意命令/目录入口。
新测试为 PublishedContentCompilerTests.cs、PublishedContentCatalogTests.cs、PublishedContentTestData.cs，共新增 92 个实际具名用例。
测试 review 的构造器先要求 `fixture:` 包身份；真实 source 从未通过测试凭证调用 Publish。
Prepare 只返回待审规范载荷；纯 DTO/正式上下文不是已发布事实，也不授权 PlayerSave。

## 内容与实物

正式 source：Assets/FightMatchContent/demo-r1.source.json。仅 L1 可玩并合法重打；无 L3 或 fixture V2 混入。
逐项复制批准的 31 档 C 与 ε=1/1000000000；Lv31 以上沿第31档。没有重新搜索或选择 C。
采用 AssumedBottomLeft，原源坐标原点仍未知；F1、A/B、颜色0/1、敌实例键与原槽/稳定顺序均保留。
正式 ID/版本、W Lv1/xp0/HP100/槽0、另两空槽、空库存/携带/技能/恢复、仅 L1 开放，与批准映射一致。
新档冻结完整规范字节及 Id/RecordVersion；未写入玩家 PlayerId、OperationId 或真实随机初始化材料。
L1 source 保留 baseXP20、锡片2、木材0、无首通额外和解锁；本次真实关闭报告计算 XP26，不是固定正式奖励。
正式 Numeric=RC01、Random=PC01+SC01 标签保持；capability-map.json 解释其对 NC01 精确数值、RC01 PCG32/PRD、PC01 证明、SC01 实际映射的对应。
393 个 source 叶字段在 approved-content-map.json 中记录原值/映射/实际解码值；geometry/replay 为本次真实 API 结果。

| 实物 | 字节 | SHA256 |
| --- | --- | --- |
| source | 20443 | fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a |
| authoring/prepared.fmpackage.bytes | 11486 | b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129 |
| authoring/prepared.fmvalidation.bytes | 664 | 512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d |
| authoring/replay.json | 2389832 | 86e9f3aa1708fcdd9b9f6eb080ab74cc81acba4709f8c0f12e203bb4366cbfc4 |
| approved-content-map.json | 117327 | da286b0a01f2bd06c74bef915422f94d76e99b33d00683b33fb3389b5949c9f6 |

后三类位于 TestArtifacts/FMDemo025P2/p2b-prepare。新内容指纹是规范载荷 SHA，不使用旧 5db393d0…12 的候选指纹。
authoring/review-template.json 为 UNREVIEWED，独立身份字段留空，不能用于真实 Publish。

## 实际验证

RUN：Tools/Invoke-FM025P2Validation.ps1 -Stage 025-P2B-PREPARE -Mode Compile/Prepare/Tests -ExpectedScriptSha256 cf3f2c82d8fe6d2a5fd8bf2b17361c0ae4bfd07ab6358281410d1823c6abd285。
三个 Mode 分别运行，实际 executable、argv、PID、起止时间、输入/程序集身份和退出码均保存在对应 run.json/result.json；没有合并为一个过程。
Unity 2022.3.18f1 全程串行，启动前无 Unity 进程，没有杀 Editor 或删锁；调用正常工具审批通道，许可证无需用户再次激活。

| 运行 | 实际结果 |
| --- | --- |
| 001 compile | exit 0；无编译错误；自然生成12份新 meta |
| 002 prepare | exit 0；真实新入口；31 参数证明、1关几何及完整正式绑定隔离回放；9件 authoring 输出 |
| 003 tests | exit 2；3556 总数、3555通过、1失败、0跳过；新增92全通过、旧3463通过，唯一失败见首节 |

全量 tests 无 -quit、无 filter。XML 2497081 字节，SHA256=3c65aece6c7cd2d5a1b2b21c858d92492bb5df9cfb2cb7734ff5d69b0949b073。
所有运行源码、source、工具和36个程序集/PDB身份一致；最终源副本仍是这次实际运行版本。
Windows probe 通过真实生产适配执行：writer.lock 0字节、64个a.blob 4字节，读回 00 01 02 03，.work 已不覆盖提升、无删除。
blob SHA256=054edec1d0211f624fed0cbca9d4f9400b0e491c43742af2c5b0abebf0c990d8；保留 lock/blob。排他、复读、幂等和不同字节禁止覆盖均有测试结果。
该四字节 probe 只验证本次 Windows 路径，不代表正式发布、进程强杀、断电或 Android 保障。
内存故障覆盖每个写入边界的未知结果、部分写、坏复读；已有 op 不覆盖，同 op 异意图冲突，已完成事实不被后续取消/revision倒改。
多关 V2 fixture、准确 V1 保留/删除拒绝、五字段/关版本、review/验证/收据损坏、缺能力、未知 schema、release-set精确读取均通过。

| 条件 | 当前依据与状态 |
| --- | --- |
| B01 | PASS：严格 source、393字段映射、31 C/ε、规范新指纹及完整新档；对应正负测试通过 |
| B02 | PASS：原共享数学/几何/完整回放真实产物；旧revision/cancel拒绝，Prepare无发布副作用 |
| B03 | PASS（阶段限定）：fixture真实 Catalog/内存故障 + 获准 Windows最小probe；无真实首包发布 |
| B04 | PASS：精确绑定、证据/收据/能力准入、只读新档字节与显式启用集合查询 |
| B05 | PASS：同程序集多关/V2 fixture及旧包准确解析，真实 source仍仅L1 |
| B06 | BLOCKED：编译/prepare/范围已通过；旧断言冲突导致全量仍1失败，尚不可接受 |

## 范围和预算

完整实核为实现700、Assets716、GUID379且唯一；367份旧 meta/GUID、308项保护输入、全部旧测试源码/断言保持。
680旧实现中仅2文件获准修改，另678保持；源增删6/160，工具增删40/160且最终162/360行。
新生产1053/2600行，新测试430/2000行，source20443/262144字节；正式文档行/字节预算由 scope记录。
681份入口与704份最终源码/工具/source副本均为准确实物，不覆盖原副本。
唯一证据根、固定文件、运行号、authoring和probe均按有限清单进入read-manifest；manifest不自哈希。
scope.stageExports完整继承最近A导出，更新获准项及新增项，另绑定A三报告与根/manifest；本段报告从formal final外部绑定，避免自循环。
本轮没有整体接收或未来读取/执行授权；后继仍需准确 C/R 完成与 SD00 独立签发。

## 下一实施范围交接

1. 首先只处理上述旧断言例外。原3464名称和其余断言保留，新增92及已有效prepare保全；最终编译/全量测试须按新授权范围完成，再由原R独立审查。
2. R只有整体ACCEPT后才生成真实 content-review.json，严格可被 ContentReviewEvidence 解码，绑定本次实际交付及本人准确task/turn，不仅把模板布尔改为通过。
3. 当前 Run CLI只支持 `-fmMode prepare -fmSource <绝对source> -fmEvidenceRoot <绝对输出根>`；**不存在可宣称已验证的 publish CLI命令**。Tools Stage也只接受 P2A/P2B-PREPARE。
4. 已有可调用的 Publish 作者API签名为 `Publish(PublishedContentCatalog, PreparedPublication, string reviewPath, DemoContentJob, string operationId, ContentStoreBudget)`，读取有界真实review并调用Catalog；Prepared必须和当前Job同一对象来源。后继需明确签发实际调用宿主/工具适配，不能假设现Run已有publish能力。
5. Windows通用命名为同一显式绝对根下 `writer.lock` 以及每个64位小写十六进制key的 `.work`→`.blob`。Catalog共有7类准确键：operation、source、payload、validation、review、receipt、binding；键为 SHA256(UTF8(kind+NUL+identity))，binding identity为完整五字段规范JSON的SHA。source/payload/validation/review identity分别为其实际字节SHA，operation/receipt identity为原operationId。
6. 当前真实source的source/payload/validation身份已定，review字节和operationId需待真实R/SD00绑定，才能列出后继全部精确物理路径。release-set另用显式scope/id规范JSON求键；Publish不创建/启用release-set，GetCurrentBinding只读。first-release五件与通用存储7记录的导出/读取安排，须在下段准确签发，不能把4字节probe当该映射已完成。
7. 不执行真实 Publish/activate、PlayerSave、P2C或页面。025整体/B17、CONT-A/B/C、028/029及首Demo仍未闭环；物理鼠标/触控、交互PlayMode、合法AwaitLinks、§184旧NOT VERIFIED保持。

两报告为本次真实阻塞交付，不是 C COMPLETED 或 R ACCEPT；唯一待决修正具体见首节。

