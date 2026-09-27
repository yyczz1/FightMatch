# 025-P2B-PREPARE-C1 交付与下一范围交接

状态：**COMPLETED（作者验证通过，独立审查 PENDING）**。原 B01～B06 与本次最小纠正仍由原 R 整体独立审查；本报告不代签 ACCEPT。
任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1，local，gpt-6-astra / max。
本次准确 turn：01a0d3c8-8128-7510-a5b4-2a1a09a4f30c；原生 context：2026-09-24T14:18:48.073Z。
工程根：D:/Unity/UnityProj/FightMatch；下列工程相对路径均从该根解析。

## 授权、入口和保全

§301～302 规范化 SHA256：db77b27ee9ec96581b66ca07b99c6c35100952d209f1c2f7daa2b63ebe2983b7。
§304 括号勘误及旧 probe 复读补充 SHA256：c266b26cabb7a1b55547217841f5cfe9d13107464d2a91929093bb99b3c8ad1b。
先前同一技术例外问题已由SD00依§300～302关闭，不再待用户确认。
本次入口是 §300 收件的原 B BLOCKED 最终状态：700 实现、716 Assets、379 GUID、753 导出；不是退回 A。
入口核 750 不可变导出全部长度/SHA相符；3 份协调稿单列可更新，冻结段另核，不把 SD00 的合法追加当污染。
原 B delivery 12783 字节/2c56cd0ee4150ef2e430c2f5f95cc90eb5255b6486dd9aad27261df5fe716cb5。
原 B scope 1518833 字节/f616044fcded0082dd1610a673cb224b1633576723419a30799908a33a0386ae。
原根、resume-001、中断链、1442 项证据、authoring 九件、003 失败、probe、BLOCKED 两报告均保持原身份。
本次唯一新证据根 TestArtifacts/FMDemo025P2/p2b-prepare-c1；704 份入口副本和704 份最终副本均准确保留。
未使用 Git 命令、分支、提交、推送、工作树、代理或新任务；未恢复自动跟进。

## 精确修改

唯一旧测试修改为 Assets/Tests/EditMode/FightMatch/DemoContentCompilerTests.cs 原141行，按 §304 的合法括号写法：

    Assert.IsFalse(references.Any(n => n.StartsWith("Unity") || n == "QFramework" || n == "FightMatch.Application"));
    Assert.IsTrue(references.Contains("FightMatch.Platform"));

精确 2 行新增、1 行删除；保留 Unity/QFramework/Application 禁止及 Platform 必需契约。
空输入、小预算、旧 Compiler 没有 Publish/ResolveExact 方法、其余全部用例/断言/名称保持。
入口14552字节/437f883927ed8a4e7bea950d89e041985f90fe99342078f9abd05d943ef5e707。
最终14594字节/b7dc6beba1ea2084c0e859845ed8617f1358269d3bdfeaffdebb13189020d325。

Tools/Invoke-FM025P2Validation.ps1 仅适配固定 C1 Stage/根、当前task/turn/包摘要、原B清单及001～008运行号。
C1仅 Compile/Tests；Prepare 明确拒绝，Publish 不在模式集合中；原 Stage 行为保持，没有任意命令/目录入口。
入口10985字节/cf3f2c82d8fe6d2a5fd8bf2b17361c0ae4bfd07ab6358281410d1823c6abd285。
最终12382字节/8600bc0df196cb44773dfa5ac9ffc45cf6f9644f0cc7e4019707f485fa2f416e。
本段工具20增3删=23/80，最终179/360行；相对原B入口所继承A版132行工具，累计54增7删=61/160。
按原B的40加本次23保守相加亦仅63；预算未转移给业务代码。PowerShell解析0错误。

其余699项实现、全部480项生产输入（含meta/asmdef）、source、379份meta/GUID及308项保护输入未变。
实现700、Assets716、GUID379数量和路径集合保持，无新增源码、测试、资产或meta。
唯二合法变化为上述一个测试及一个工具；精确逐行差异见 line-budgets.json，完整身份见 scope-diff.json。

## 真实验证与同版继承

RUN：Tools/Invoke-FM025P2Validation.ps1 -Stage 025-P2B-PREPARE-C1 -Mode Compile/Tests -ExpectedScriptSha256 8600bc0df196cb44773dfa5ac9ffc45cf6f9644f0cc7e4019707f485fa2f416e。
两个 Mode 分别串行调用 Unity 2022.3.18f1；实际 argv/PID/时间/退出码/前后身份在各 run.json、result.json、before/after.json。
启动前无另一个 Unity，不杀 Editor、不删锁；Tests无-quit、无filter，没有跳过或删除用例。

| 运行 | 实际结果 |
| --- | --- |
| C1 001 compile | exit0、0编译错误，PID13544，29.6462148秒 |
| C1 002 tests | exit0，3556/3556通过、0失败、0跳过，PID26648，626.0154264秒 |
| 原 B 002 prepare | 按§301.2继承原exit0及九件输出；本次不重跑、不覆盖 |
| 原 B Windows创建probe | 创建/Flush/提升证据继承原003；本次全量只走§304批准的已存在文件复读 |

本次XML 2495834 字节/SHA256=38474a8f4b8125325ba93d4878ee54f6e0979af2d48be4725172d769250fb125。
原3464＋原新增92的 fullname 多重集完整相同，无新增/减少/过滤；原失败用例 NullAndTinyBudgetCannotCreateCandidate 本次真实执行并通过。
源码/source/工具/Assets在两运行前后保持；编译后36项DLL/PDB与最终测试前后同版。
只有 FightMatch.Core.Tests.dll/.pdb 因合法断言修改改变：DLL仍1206272字节，SHA=60c8a024a21e984844284d3ac7440c829ebd075a22da7373fbdd37f29637afcc；
PDB354444→354448字节，SHA=3047fbc22a2c28f1f1e7272acecce5199ef2e5223d95a7c057664a11099efeae。
30项非测试DLL/PDB（含Editor）与原prepare前后/原B最终/本次最终长度及SHA均相同；另4项其他测试DLL/PDB未变。未声称全部36项从原B起不变。

source20443字节/fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a；
prepared payload11486字节/b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129；
validation664字节/512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d。
原approved-content-map的393叶字段、31精确C/ε、仅L1、新档/奖励、能力映射、实际geometry/replay及九件authoring逐项原身份均保持。
原Windows writer.lock仍0字节、64个a.blob仍4字节00010203/SHA=054edec1d0211f624fed0cbca9d4f9400b0e491c43742af2c5b0abebf0c990d8。
该完整回归用例输出为 retained original exact blob and checked idempotency; no overwrite；.work仍不存在。
本轮仅排他lease打开/关闭、复读、同字节幂等及异字节拒绝，无创建/Flush/提升/删除，不能声称扩展进程强杀/断电/Android保证。

| 条件 | 作者核验及交给R的依据 |
| --- | --- |
| B01 | 原严格编解码、393字段映射及31C/ε源实物不变，本次相关正负用例全部通过 |
| B02 | 原实际数学/几何/完整回放、Prepare无发布副作用及revision/cancel门同版继承；本次用例通过 |
| B03 | 原Catalog及内存故障实现不变，本次用例通过；Windows原创建证据继承、本次复读单列 |
| B04 | 原全五字段/整关版本/能力/review/验证/收据检查及显式release-set读取同版，本次用例通过 |
| B05 | 原多关/V2 fixture及旧包精确解析用例通过；真实source仍仅L1 |
| B06 | 本次编译/全量全绿＋满足身份条件的原prepare继承；范围、预算、meta、有限证据均可核 |

read-manifest精确1442项，不含自身；scope.stageExports.evidence与manifest完全同项，根/manifest由scope外部绑定。
stageExports完整继承原B753导出、只更新合法变化及可更新协调稿，并加入原B两报告、根/manifest/resume等准确前序入口；共758项。
当前两报告从formal final外部绑定，避免自哈希循环；原B证据另保留其准确scope/manifest/同项items，不赋予执行权限。

## 下一实施范围建议（仅报告，须SD00在真实独立ACCEPT后另签）

实际入口仍是 PublishedContentAuthoring.Run 的 prepare CLI；公开 Publish(catalog, prepared, reviewPath, job, operationId, budget) API 已存在。
当前没有publish/activate/export CLI、首包loader或first-release正式资产；P2C/029不能假设已有这些能力。
后继仍复用 PublishedContentCompiler、PublishedContentCatalog、原共享规则核；不建立Demo专用战斗/成长/结算/保存代码。

建议最小文件范围与估算如下，均非本轮写入权；正式预算和精确物理输出须下包签定：

| 既有/新增路径 | 拟做技术适配 | 估算 |
| --- | --- | --- |
| 既有 Assets/Scripts/FightMatch/Content/PublishedContentAuthoring.cs | Run新增固定publish与activate-export分支；调用真实API、核reviewed字节、精确导出/读回 | 增删120～180行 |
| 新 Assets/Scripts/FightMatch/Content/FirstReleaseContentStorage.cs | 六份有界原字节构成只读IContentPublicationStorage；准确键映射/深复制，写/lease拒绝 | 100～160行 |
| 新 Assets/Tests/EditMode/FightMatch/FirstReleaseContentStorageTests.cs | fixture六件映射、缺/坏source、坏receipt/release、未知格式/预算、只读、冷启动精确解析 | 180～260行 |
| 既有 Tools/Invoke-FM025P2Validation.ps1 | 固定PUBLISH Stage、分离两操作、准确根/参数/进程与产物采集 | 增删40～70行 |
| 原五件first-release＋新增first-release.fmsource.json | 只从真实完成发布/显式启用读取原字节；六件各有自然meta，另两个目录meta | 原总资产32MiB上限内 |
| 后继P2C既定 PlayerSessionSystem.cs、FightMatchDemoArchitecture.cs | 接收同一只读Catalog与明确release-set；沿原P2C建档/打开协议 | 在P2C原包重核，不计本PUBLISH估算 |

对应自然meta精确路径也须在下包列出：
- Assets/Scripts/FightMatch/Content/FirstReleaseContentStorage.cs.meta
- Assets/Tests/EditMode/FightMatch/FirstReleaseContentStorageTests.cs.meta
- Assets/StreamingAssets.meta
- Assets/StreamingAssets/FightMatch.meta
- Assets/StreamingAssets/FightMatch/first-release.fmsource.json.meta
- Assets/StreamingAssets/FightMatch/first-release.fmpackage.bytes.meta
- Assets/StreamingAssets/FightMatch/first-release.fmvalidation.bytes.meta
- Assets/StreamingAssets/FightMatch/first-release.fmreview.json.meta
- Assets/StreamingAssets/FightMatch/first-release.fmpublish.json.meta
- Assets/StreamingAssets/FightMatch/first-release.fmrelease.json.meta

所有meta由Unity自然导入；不需改Catalog/Codec/Models/Core/Platform/asmdef才能完成上述发布和只读bundle适配。
运行宿主建议为已有Run；无第二启动器。内容工作根建议 TestArtifacts/FMDemo025P2/p2b-publish/publication-store，导出根为 Assets/StreamingAssets/FightMatch。
技术参数建议固定 operationId=publish:fightmatch-demo-r1:1、scope=player、releaseSetId=release-set:fightmatch-demo-r1；这是供SD00签定的建议值，当前尚未执行/启用。
reviewPath必须指向原R实际ACCEPT后生成且SD00绑定长度/SHA的 docs/system-design/2026-09-17/demo-025-p2b-content-review.json，不能用原UNREVIEWED模板。
sourcePath为本次不变的Assets/FightMatchContent/demo-r1.source.json，capabilities=ContentConsumerCapabilities.Current，Prepare/Store分别给新的ExactMathBudget(maxPrimitiveSteps:16000000)，Store最大记录16MiB。

建议真实调用顺序（拟新增CLI参数明确绑定，当前命令不支持）：

1. publish分支显式接收-fmSource/-fmReview/-fmStoreRoot/-fmOperationId及固定预期source/payload/validation身份；所有路径/值由下包固定，拒绝未知/重复参数。
2. DecodeSource后为draft:fightmatch-demo-r1的修订1建立同一个DemoContentDraft/Job；调用现有PublishedContentAuthoring.Prepare(sourcePath, job, caps, math)获得Prepared对象。
3. 这次构建是现有Publish要求prepared.Job与currentJob同对象的必要运行步骤；逐字节比较旧reviewed payload/validation及原source身份后才允许写。不得覆盖原authoring九件，差异须停下重新审查，不能换载荷沿用旧指纹。
4. 构造WindowsContentPublicationStorage(storeRoot)及PublishedContentCatalog(storage,caps)，调用PublishedContentAuthoring.Publish(catalog,p,reviewPath,job,operationId,storeBudget)；只有IsAccepted且Status=Completed成立才继续，发布本身不启用。
5. 用catalog.ResolveExact(p.Binding,caps)复核整包；DefinitionBinding.Prepare(p.Binding,"level:ch01-01",1,saveBudget)成功后再调用另一个ResolveExact重核整关。ContentBinding五字段原值为package:fightmatch-demo-r1、b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129、demo-r1、RC01、PC01+SC01。
6. 另一个activate-export分支须有SD00明确启用参数-fmScope/-fmReleaseSetId，先ResolveExact并取得Publication.ReceiptBytes；构造SchemaVersion=1的ContentReleaseSet，其Binding为原五字段、PublicationReceiptSha256为原收据原字节SHA。
7. 以PublishedContentCodec.EncodeReleaseSet和PublishedContentCatalog.ReleaseSetKey(scope,id)生成明确字节/键；在storage.AcquireWriter内WriteImmutable并复读，再以catalog.GetCurrentBinding(scope,id,caps)核准入。无SetCurrent/latest隐式回退。
8. 精确导出下表六件原字节，CreateNew或已有完全同字节的受控续办，不覆盖未知/不同字节；六件重新有界读入只读storage，再用同一个Catalog的GetCurrentBinding与两种ResolveExact验收冷启动加载。部分导出不得当完整首包。

原五件不足：Catalog.Resolve必读原source，而当前.fmpackage.bytes是规范载荷，不能逆造出20443字节原source及其SHA。最少新增一件first-release.fmsource.json即可；不改已审payload格式或指纹。
原Catalog把同一PublicationRecord规范原字节I写到operation、receipt、binding三键；导出必须先核三者实际均存在且逐字节等于I，才能采用下述只读完成态映射：

| 首包物理文件（均在Assets/StreamingAssets/FightMatch） | 只读storage精确映射 |
| --- | --- |
| first-release.fmsource.json（新增） | Key("source", SourceSha256) → 原source字节S |
| first-release.fmpackage.bytes | Key("payload", PayloadSha256) → 已审规范载荷P，原指纹不变 |
| first-release.fmvalidation.bytes | Key("validation", ValidationSha256) → 原验证字节V |
| first-release.fmreview.json | Key("review", ReviewSha256) → R实际review原字节R，不重新格式化 |
| first-release.fmpublish.json | 原完整I同时服务Key("operation",OperationId)、Key("receipt",OperationId)、BindingKey(ContentBinding) |
| first-release.fmrelease.json | ReleaseSetKey(scope,releaseSetId) → 已显式启用且绑定原I的release-set字节 |

七类发布记录加一类release记录由六个物理文件无损表达；operation/source都保留，没有丢弃操作事实或只导出operationId。
I必须是实际完整原收据字节，不是按字段重新拼出三个“成功”记录；只读adapter为三键返回I的防修改副本，并拒绝所有写/lease，不用于挂起发布恢复。
adapter有界解码I和release以建立准确键，所有长度/哈希/原source→payload规范关系/review/validation/完整回放仍交同一Catalog核验；缺任一件/能力/schema不支持则准确拒绝。
持久发布库仍保留七份.blob及独立release.blob，不删除或合并其中operation/binding实体；仅首包导出使用上面的只读多键共享字节契约。
Key规则仍SHA256(UTF8(kind+NUL+identity))；真实review SHA及最终操作/启用身份确定后才能列出writer.lock、每个准确64hex.work/.blob和六件输出的白名单，不能本轮提前写未知路径。

P2C/029宿主负责从已批准固定位置获取同样六份有界字节，再注入FirstReleaseContentStorage→PublishedContentCatalog；平台I/O不进入Core，不把源码路径或草稿放进PlayerSave。
新档沿P2C既定PrepareNewProfile(catalog,releaseSetId,SaveCodecBudget)和F2完整创建记录；旧档/未决创建必须按冻结ContentBinding调用ResolveExact，不能换到current或重新生成配方。
029通过同一PlayerSessionSystem/Architecture办理启动路由和展示；Android将来只替换获取这六份字节的平台适配，同一Catalog/规则/绑定协议保持。此处没有宣称Android读取/构建已经验证。

本轮未真实Publish/activate、未写StreamingAssets或PlayerSave、未实施P2C/页面。025整体/B17、CONT-A/B/C、028/029及首Demo仍未闭环。
物理鼠标/触控、像素/交互PlayMode、合法AwaitLinks及§184旧NOT VERIFIED保持；原R须完成原B全部B01～B06及本C1独立审查后，SD00才能签下一实施范围。

