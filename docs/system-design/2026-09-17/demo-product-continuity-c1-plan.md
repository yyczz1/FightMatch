# FM-DEMO-CONTINUITY-C1：阶段继承与原建档意图恢复

状态：COMPLETED（§275 报告纠正）；只补 F1/F2，不是源码实施或设计 ACCEPT。
任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本次 turn：01a0d1b2-2c5e-7241-ae35-1c5112307cbc。
原生 task_started：2026-09-24T04:35:10.08Z；turn_context：04:35:10.893Z，local、gpt-6-astra / max。
工程根：D:/Unity/UnityProj/FightMatch。下述工程相对路径均由此根解析；新接口和文件仍仅为未来建议。
§275～276规范化SHA256：b09047dd6725b4cdccb2c1d98e91bcd7e775c96f24719aadf98c8cb16bbc0ee4（CRLF→LF，§275起至§277前，trimEnd＋单LF，UTF-8）。
已读取最新§274～277、session-plan r153第130节、integration-review r136第124节；本次准确派发沿§277，不沿用§273旧完成身份。
两C1报告启动时均不存在；原三报告只读。范围遵循§275而非旧review第97行所建议的就地修改方式。

## 1. 原交付绑定与最小差异

| 原正式实物（docs/system-design/2026-09-17/） | 字节 | SHA256 |
| --- | --- | --- |
| demo-product-continuity-plan.md | 24285 | 24bc8ae17f7c682fcf3bcbdc2a1ef6f020b2d7a5d51e0d839664db14672edfdc |
| demo-product-continuity-scope.json | 486838 | 9d946be13720b10844e2f534f4c52bb5f1462c20f6a16f7aedfd804e0ff4acf5 |
| demo-product-continuity-review.md | 13471 | 61878ccdd33977909bb306d2d8fa1db35e43c7570a9fba1c684e713bfa17ab24 |

原C准确completed/formal final、原R唯一NEEDS_FIX已由SD00在§274收件。本次读取该完成登记及原review，不重新执行原宽核对。
F1依据：原scope四阶段readPaths均只有同一234项；后继没有A新增Core类型、B源码/source、五个发布实物、工具与实际证据读取继承，原current也未定义后继重绑定。
F2依据：原RecordCreateIntent只接两个ID。Application/CandidateLifecyclePreparation.cs:16～30重新准备会生成新PlayerId/OperationId；Core/CandidateApplicationIntentCodec.cs:50～75还编码完整上下文及初始化字段；Application/CandidateApplicationRecovery.cs:81～91的NoSave仅恢复InitializationReady，无法找回丢失意图。
本报告替换原plan第5节的阶段读取/入口约束及第6节建档定位器约束；其他原plan决定继续适用。新scope以原scope为可追溯底本，differenceIndex列具体JSON路径与理由。
本次重新核：668/668长度/SHA仍符，总3569979字节；原234输入中231保持，仅SD00获准更新三协调稿，当前身份逐项记录。原三报告及四批准实物全部保持。
原668集合摘要c11a8a…与含bytes格式摘要81db0f…分别记录；本次实际逐文件读取清单另列，不用旧scope代替实际核对。

## 2. F1：逐阶段读取继承

阶段顺序仍为A→B-PREPARE→B-PUBLISH→C，每段原独立ACCEPT门和本段修改/新建白名单不变。
机器清单见新scope.proposedImplementation.stages[*]：readPaths已按下表展开、并集去重；readOwnCreatedPaths仅允许本阶段已合法生成文件的后续读取；directories项不授目录泛读。
共同既有输入为原234路径、原三报告；另列本C1两报告及未来C1独立review作为设计门文件。未来文件只列预期路径和来源门，没有虚构SHA。

| 阶段 | 除共同输入外的必要读取 |
| --- | --- |
| P2A | 本次C1准确完成及唯一独立ACCEPT/SD00签发登记；本段合法生成后可读四Core新文件、两新测试、工具、meta及本段报告/精确证据。 |
| P2B-PREPARE | A的create.production/tests/tools/naturalMeta/reports逐项、A独立review、A有限证据；明确包含ContentBindings.cs、RuleContext.cs、RuleContextChecks.cs、PublishedSaveContext.cs。 |
| P2B-PUBLISH | A与B-PREPARE上述新增项逐项、各独立review/有限证据；包含demo-r1.source.json及meta、PublishedContentCatalog等七份同批源码、真实新候选与验证材料；另读demo-025-p2b-content-review.json及准确SD00批准绑定。 |
| P2C | A、B-PREPARE、B-PUBLISH的全部上述必要项及独立review/有限证据；明确包含五个first-release文件及meta、原reviewEvidence与发布/启用分离依据；自身合法新建的Core创建记录及meta另列可读。 |

前序报告精确名为demo-025-p2a、demo-025-p2b-prepare、demo-025-p2b-publish对应的-delivery.md、-scope.json、-code-review.md；均位于原2026-09-17目录。C的对应报告名同理。review是R/SD00输入，不在C的create中。
first-release五实物为Assets/StreamingAssets/FightMatch/下.fmpackage.bytes、.fmvalidation.bytes、.fmreview.json、.fmpublish.json、.fmrelease.json；其完整路径和逐一meta已展开。读取不授修改/重发布权限。
任何直接前序独立ACCEPT缺失、formal final和报告哈希不一致、前序尚在运行，后继不能启动；“文件已出现”不替代完成门。

### 2.1 有限证据展开

每段专用根分别为TestArtifacts/FMDemo025P2/p2a/、p2b-prepare/、p2b-publish/、p2c/。它们是未来建议位置，本次未创建。
每根有精确root-identity.json与read-manifest.json；前序正式scope的stageExports.evidence同时绑定根描述文件与manifest的路径/字节/SHA，并列出相同的items记录。SD00接收登记绑定该scope、formal final及R的ACCEPT。
root-identity记录根的规范工程相对/绝对位置、StageId、作者task/turn及范围身份；目录本身不伪称有文件SHA。manifest逐项为根内相对路径、用途、长度、SHA；不含执行指令、不自包含自身哈希。
仅允许manifest与已接收scope.items完全一致的有限项目：命令/进程结果、编译/测试日志/XML、源/二进制身份清单、具体差异/保存故障见证、内容映射/验证与发布证据、完成元数据。完整包/日志不自动扩大到未列归档内部条目。
先核根描述和manifest身份，再核每个item；拒绝绝对/盘符/UNC/设备路径、空段/点段/..、通配符、ADS冒号、NUL及规范化后越根路径；检查沿途reparse/junction，不能只用字符串前缀。
新增证据只按已接收scope的精确items展开；不递归枚举根“发现可读文件”，不从未批准scope增权。缺项/越根/身份不同保全并报告，不能回退到整目录权限。
后继若实际需要尚未导出的证据，由SD00补其准确项；未来身份都标EXPECTED_NOT_CREATED_UNBOUND，待真实交付后填值，不编造长度/SHA。
当前阶段自己的证据生成/读取须由后续源码包列明具体运行输出；本C1只定义前序证据接收机制，不授任意目录读写。

### 2.2 工具读取、执行及入口重绑定

Tools/Invoke-FM025P2Validation.ps1的读取已逐阶段列明：A合法创建后读取，B-PREPARE/B-PUBLISH/C从前序已接收导出继承。
executionPermissions另列同一精确脚本及阶段参数/输出根；只有SD00签发源码包、该脚本实际存在且身份核对完成才可执行。后继只有读/执行权，无修改权；本C1任何阶段执行权均未生效，也没有运行该脚本。
阶段启动依序核：前序准确completed→非异步formal final→C报告字节/SHA→R唯一ACCEPT→SD00准确接收/本段签发。A以前置C1设计接收和源码签发代替源码前序。
随后由最近已接收前序scope.stageExports.implementationFiles取得本阶段每个必要入口的期望身份，实际fresh读取路径/长度/SHA；该表须覆盖继承源码/meta/工具/内容及保留的既有入口。当前前序报告及review身份从接收登记取得，不形成自哈希循环。
A用本C1观察基线及签发时实际状态；B-PREPARE用A导出，B-PUBLISH用B-PREPARE导出，C用B-PUBLISH导出。最新导出须保留更早阶段的完整继承项，不能丢A的Core类型。
原modifyExisting.current仅为本轮观察值，新scope已标observationOnly；不是要求后继经过A/B合法修改后仍等于旧SHA。每项附stageEntryBinding和可能的前序写入者；真正期望值在门通过后取自最近已接收fresh导出。
预期已变项须能映射到前序准确modify/create及R接收；未批准差异原样保留，标UnexpectedStageInputChange并停止受影响阶段，不回滚、不重设原基线、不静默采用磁盘最新。
本段modifyExisting/create/budget独立执行。增加读取或执行权限不增加写入项，不让同一脚本替后继改A源码；后继如确有必要修改仍只按其本段表。

## 3. F2：完整Core创建记录

采用SD00选定的“冻结完整原意图，重开续办同一意图”，不增加终止创建/清档重建API。
未来仅在P2C新增Core/PlayerProfileCreateRecord.cs及自然meta；该文件容纳不可变记录、有限编解码、生成材料和初始化提交证明类型。所有路径以Assets/Scripts/FightMatch/为前缀。
P2C另允许精确修改Core/CandidateApplicationIntentCodec.cs，使新记录可调用同一正式规范意图读取/校验核；不新增泛用反射入口，原候选编解码不放宽。
原P2C预算不增；其他阶段和本段原文件写入表保持。独立观察的既有修改路径仍58个（该codec本已在A中），C本段modify项20→21、新增文件仅上述.cs与.meta。

| Core记录字段 | 精确含义/核验 |
| --- | --- |
| RecordFormatVersion | 显式创建记录格式，首版1；未知版本拒绝，不能按空档处理。 |
| PlayerId / OperationId | 原一次生成值，须与规范初始化意图内同字段逐字一致；意图Kind=InitializeProfile、ExpectedCommitId=null、用途PlayerSave。 |
| ContentBinding | PackageId/ContentFingerprint/RuleVersion/NumericContractVersion/RandomContractVersion完整原值；与意图正式Context一致，不保存latest别名。 |
| NewProfileDefinitionId / RecordVersion | 原新档定义身份和正整数版本，建议技术名new-profile:default/1；仍是原批准W/空槽/L1配方，不新增玩法。 |
| FrozenNewProfileDefinitionBytes | 有界不可变规范新档配方副本：原角色/等级/经验/原槽、其他空槽、库存/携带/技能/恢复/开放关卡等初始化事实；从原准确包取得并在恢复时核原字节/版本。 |
| CanonicalInitializeIntentBytes / IntentFormatVersion | 完整原规范初始化意图载荷，含ID、正式Context和所有初始化字段；不是仅两个ID或一次临时内存引用。 |
| IntentSha256 / DefinitionSha256 | 各冻结字节的长度/摘要与规范再编码一致；重复字段不一致拒绝。SHA只作完整性/身份校验，不替代Published准入。 |
| GeneratedMaterials | 对未已包含在规范载荷中的一次生成身份/熵，保存有序Kind/Name/SourceCapabilityId/准确字节；必须完整引用并受预算约束，不保存“以后重新生成”的配方。当前建档只生成两ID且已入意图，其他材料明确空；H02熵仍在后续入场时产生。 |
| RecordSha256 | 对格式、绑定、定义版本、长度、冻结载荷和生成材料的规范编码求摘要（排除自身摘要）；任何载荷改变不能保持原记录身份。 |

所有输入深复制；字节/列表先检查SaveCodecBudget及其Math/字符串/集合限制再分配，整份记录≤MaxEnvelopeBytes，不以压缩/引用可变源绕过冻结。整数和精确值沿既有编码，不经float。
这里冻结初始化请求形成时已产生的身份/材料；随后M12分配的CommitId仍由原提交协议及未决候选负责，不提前伪造到创建记录。
Core核格式、规范字节、摘要、初始化种类及ID/绑定一致性；Application使用M10 ResolveExact原包，核冻结新档定义与原目录/版本/字节和能力一致，才恢复PreparedPlayerProfile。
Platform只持久化/读回Core记录和定位器物理状态。它不引用Application DTO，不执行初始化/奖励，不从current release-set补字段。
PrepareNewProfile是显式首次准备入口，成功prepared附带完整CreateRecord；RecordCreateIntent必须先持久完成并复读核验，才能开始第一份PlayerSave提交。记录写入失败/未知时不越过该门。

### 3.1 准确建议签名与责任

以下签名是待源码包实现的契约，不声称当前存在；完整对应关系同步到scope.signatureAdaptations。
Core（同一新文件中的PlayerProfileCreateRecordCodec）：
- Freeze(PreparedCandidateApplicationIntent intent, string newProfileDefinitionId, BigInteger newProfileDefinitionVersion, byte[] canonicalNewProfileDefinition, IReadOnlyList<PlayerProfileCreateMaterial> generatedMaterials, SaveCodecBudget budget) : SaveCodecResult<PlayerProfileCreateRecord>
- Write(PlayerProfileCreateRecord record, SaveCodecBudget budget) : SaveCodecResult<byte[]>
- Read(byte[] encoded, SaveCodecBudget budget) : SaveCodecResult<PlayerProfileCreateRecord>
- RestoreIntent(PlayerProfileCreateRecord record, SaveCodecBudget budget) : SaveCodecResult<PreparedCandidateApplicationIntent>
- VerifyInitializationCommit(PlayerProfileCreateRecord record, CandidateApplicationSnapshot verifiedSnapshot, SaveCodecBudget budget) : SaveCodecResult<PlayerProfileInitializationCommit>
Core既有internal codec新增/适配ReadFrozenPublishedInitialize(byte[] canonicalBytes, SaveCodecBudget budget) : PreparedCandidateApplicationIntent，供上述Core入口内部调用；只接准确正式初始化，复用规范Visit/Shape/字节比对，不公开任意业务提交。

Platform（LocalPlayerProfileLocator）：
- Read(SaveStoreBudget budget) : LocalPlayerProfileResult
- RecordCreateIntent(PlayerProfileCreateRecord record, SaveStoreBudget budget) : LocalPlayerProfileResult
- ConfirmCommit(LocalPlayerProfileObservation expected, PlayerProfileInitializationCommit proof, SaveStoreBudget budget) : LocalPlayerProfileResult
LocalPlayerProfileResult含明确Code/Diagnostic及不可变Observation；Observation含已校验Core记录、定位器owner/物理观察身份、CreateIntentRecorded或Active状态及可选active绑定。不存在/坏记录/未知分别返回，不能用record=null统称无档。
PlayerProfileInitializationCommit由Core校验已解码/已核完整快照后构造，包含RecordSha256、PlayerId、OperationId、原ContentBinding/新档定义ID及版本、原IntentSha、原初始化CommitId和ObservedHeadDescriptor；无Application类型，也不是调用方自填“成功”布尔。

Application（PlayerSessionSystem）：
- PrepareNewProfile(PublishedContentCatalog catalog, string releaseSetId, SaveCodecBudget budget) : PreparedPlayerProfileResult（原签名保持；结果新增不可变CreateRecord）
- RecoverCreateIntent(LocalPlayerProfileObservation observed, PublishedContentCatalog catalog, SaveCodecBudget budget) : PreparedPlayerProfileResult
- ContinueCreate(PreparedPlayerProfile request, LocalPlayerProfileObservation expected, ILocalSaveStorage storage, LocalPlayerProfileLocator locator, SaveRecoveryCapabilities capabilities, SaveStoreBudget budget) : CandidateApplicationCallResult
- CreateNew(PreparedPlayerProfile request, ILocalSaveStorage storage, LocalPlayerProfileLocator locator, SaveRecoveryCapabilities capabilities, SaveStoreBudget budget) : CandidateApplicationCallResult（原签名保持；先Record完整记录，再委派ContinueCreate）
OpenExisting原签名保持；active资料继续打开当前完整头。RecoverCreateIntent不调用PrepareNewProfile、不调用GetCurrentBinding、不生成任何ID/熵；从PlayerProfileCreateRecordCodec.RestoreIntent及原精确包恢复同一prepared。
新档定义规范字节由P2B既有拟建PublishedContentModels/Catalog公开只读结果提供；只补F2所需的准确定义ID/版本/规范字节投影，不另加源码路径或第二套编译器。

### 3.2 持久状态、幂等与路由

RecordCreateIntent同记录身份/规范字节重试返回Recorded/AlreadyRecorded；已Active且同记录返回AlreadyActive。相同PlayerId或OperationId而载荷不同为CreateIntentConflict；另一未完成建档为CreationPending/Busy。均保全原记录，不覆盖或生成替代ID。
定位器写采用自身排他/观察校验和完整记录/确认收据的持久复读；保存承诺仍仅Windows EditorProcessCrash。写结果不明返回CreateIntentWriteUnknown/ConfirmationUnknown并重读原记录，不能宣称Absent或自行重建。
Read先核已知定位器根和完整物理观察，再对有界字节做Core格式/校验；确认记录有效前不把其中PlayerId用于选存档目录。恢复后仍需M10/正式会话核验，记录存在不授业务写权。

| 磁盘实际状态 | Application唯一续办 |
| --- | --- |
| 只有完整创建记录，M12经完整观察确认没有候选/完整头 | ResolveExact原ContentBinding并核原新档定义；恢复原prepared，以原ID/规范意图沿唯一runtime首次Initialize。使用CreateNew模式的依据是已持久原显式创建意图＋确认无头，不是坏档自动新建；启用集合变V2仍用原V1。 |
| 已有未决保存候选，或物理结果尚不确定 | 先按M12查询原OperationId/Commit、原父链/字节/能力；沿原Resume/Resolve/Retry续办。不得再调用初始化builder或生成第二候选；不足以证明状态则Pending/CommitUnknown/RecoveryBlocked。 |
| 完整头已提交，active确认未成 | 在原租约/同一M02排他办理下核完整头、原初始化记录与CreateRecord完全对应，生成Core提交证明并幂等ConfirmCommit；不再次Initialize/发奖。 |
| active已确认 | 读同一PlayerId最新完整头，按原精确绑定恢复；同确认重复返回AlreadyActive，不重跑创建。active但档案丢失/损坏为明确恢复失败，不能退回第一行。 |

ConfirmCommit先核原记录hash及Player/Operation/Intent/ContentBinding/定义版本与proof一致；原初始化记录须在已核完整头中，Kind、规范字节、原CommitId及M12索引关联一致。
同一M02办理保持原写租约并核该证明所用头；首次active确认完成前关闭新业务写入，仅允许原创建续办/确认。确认失败不能让UI绕过门进入可写主页。
expected包含定位器owner与准确物理观察，过时返回StaleProfileObservation；但已存在同一RecordSha/原初始化Commit的active收据先返回AlreadyActive。不同记录/初始化Commit的确认拒绝，不覆盖别的profile。
active引用锚定PlayerId、CreateRecordSha和原初始化Commit，不能将它当当前存档头。后续进度头由M12选择；重复确认不倒退最新头，也不要求当前角色仍是Lv1。
坏记录=RecoveryBlocked/CorruptCreateRecord；未知记录/意图格式=UnsupportedCreateRecord/UnsupportedSchema；缺原包或能力=UnsupportedBinding；定义/意图不一致=InconsistentCreateIntent；保存/记录未知分别保持未决；超预算=Limit。每项保留文件及具体FieldPath，均不改绑latest、不终止创建或清档。
关闭应用仅释放资源；下次Read→上述同一状态机。现有通用End/EndObserved也不作为未确认创建的出口，M02拒绝以它们终止Initialize；已确认资料的原战斗结束协议保持。原创建记录/active依据保留供防重核对，本C1不设计清理/终止功能。

## 4. 针对性验证设计及未变范围

以下均为未来P2C拟验收，本次没有执行：
| ID | 中断/负例 | 必须观察到的结果 |
| --- | --- | --- |
| C1-V01 | Record已持久、首次候选/完整头之前中断；重开前启用集合V1→V2 | 原PlayerId/OperationId、ContentBinding、新档定义版本、规范意图及生成材料字节一致；ResolveExact V1、仅一个初始化结果；PrepareNewProfile/GetCurrentBinding/新ID/新熵调用次数为0。 |
| C1-V02 | 原未决候选已落盘，marker前/结果未知时中断 | 匹配原记录与候选，按M12原字节续办/查询，不再跑初始化builder、不生成第二候选/操作。 |
| C1-V03 | 首完整头已提交、active确认前中断（含确认结果未知） | 核原初始化Commit和完整头后只补确认；多次重开/确认同一profile，初始化次数1，奖励增量0，不回退后续头。 |
| C1-V04 | 缺原包、坏/截断/超限记录、未知schema、改ID或意图/定义/材料字节、同ID不同载荷 | 返回上述具体错误并保全，不能当Absent或用新启用包补齐；验证记录后才使用ID选路径。 |
| C1-V05 | 记录重复写、确认重复/过时/错owner/错profile/错初始化Commit、active档丢失 | 同记录幂等；冲突/过时拒绝；已确认后不回第一状态初始化；Core/Platform没有Application依赖。 |

F1静态验收：四阶段read集合与前序create各分类/独立review/证据入口逐项闭合；root/manifest有限展开、工具读执行分离、modify入口重绑定可追踪；未来缺文件保持预期状态。
F2静态验收：字段足以从磁盘恢复原规范初始化；各签名接收/返回同一Core记录；四状态互斥且原意图优先；新增.cs/meta及codec窄修改在C表内，预算保持。
完整原决定保持：七流程映射、共用上下文/规则/唯一runtime、候选隔离、发布/启用/恢复三门、原内容与版本/31C/ε/身份/奖励、真实入场熵、PlayerSave schema与旧候选保留、精确旧根恢复、CONT-A/B/C/028/029分工与预算/计数建议、平台和回退边界。
原V01～V10验收意图不删除，仅给V04/V05/V06关联上述创建窗口；未变决策逐组以及JSON差异边界在scope列明。34功能/51交付仍待SD00登记，不增加已接收28项。
P1/027原接受及3424等有效验证保持；合法AwaitLinks、物理输入/像素/交互式PlayMode、Player/Android、额外保存故障及§184历史NOT VERIFIED保持。Windows仍仅EditorProcessCrash，不宣称断电/Android保证。

## 5. 本次交付与停止门

只新写demo-product-continuity-c1-plan.md及demo-product-continuity-c1-scope.json；文档/JSON、路径继承、接口/数据责任、当前文件身份已静态核对。scope绑定本plan，scope自身SHA由formal final外部绑定。
没有源码/测试/配置/资产/meta/旧报告/三协调稿写入，没有Unity/测试/产品/求参器运行、Git写入、新任务或代理；没有生成任何未来源码、reviewEvidence或发布物。
准确本次formal final和completed后，原R按§276独立核对；设计ACCEPT也不自动批准P2执行。最终给本次turn、两文件绝对路径/字节/SHA、F1/F2位置后停改。
