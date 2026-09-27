# FM-DEMO-CONTINUITY-C1 独立复审

VERDICT: ACCEPT

Scope: PASS。Acceptance criteria: PASS。本结论仅接收 §275 的 F1/F2 报告纠正与后续交接质量。
没有未关闭的本次阻断项；不表示 025-P2 已实施、正式发布/PlayerSave 已放行或首 Demo 已验收。

## 1. 准确回合与正式完成门

- 工程根：D:/Unity/UnityProj/FightMatch；本报告所有工程相对路径均据此解析。
- R 任务：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；本次 R turn：01a0d1b2-c7f4-7971-9706-cab1c49776a6。
- R 原生 task_started：2026-09-24T04:35:49.929Z；turn_context：04:36:04.471Z，gpt-6-astra / max、上述 cwd；05:02:14.179Z 的续接上下文仍是同一 turn/model/effort。
- C 任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本次 C turn：01a0d1b2-2c5e-7241-ae35-1c5112307cbc。
- C 原生 task_started：2026-09-24T04:35:10.080Z；turn_context：04:35:10.893Z，gpt-6-astra / max、上述 cwd。
- 已读本次 C 的非异步 AgentMessage/phase=final_answer，时间 04:59:33.395Z，声明 COMPLETED 并绑定下表两报告；task_complete 时间 04:59:33.475Z，completed_at=1790225973、duration_ms=1463396。北京时间为 2026-09-24 12:59:33。
- 原生记录仅摘取本次生命周期/上下文/正式消息；C 来源为 C:/Users/YYC/.codex/sessions/2026/09/21/rollout-2026-09-21T20-49-35-01a0c403-bfa1-7e90-b503-c0fcd61f23c1.jsonl；R 来源为同目录 rollout-2026-09-21T10-31-29-01a0c1cd-dce1-7ac3-8780-06163cb0acfc.jsonl。
- 任务查询快照曾仍显示 inProgress，本次以准确原生 task_complete 与匹配的正式 final 为完成证据；未用文件提前出现或 §273 旧 turn 替代。本 R 在本次完成与两报告齐备后开始内容复核。
- 已独立复算 §275～276 范围摘要：b09047dd6725b4cdccb2c1d98e91bcd7e775c96f24719aadf98c8cb16bbc0ee4。算法为 CRLF→LF，从“## 275.”到“## 277.”前，TrimEnd 后加单 LF，UTF-8/SHA256；与 §277 派发一致。
- 本报告启动及落盘前均不存在；本次唯一新写文件为 D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-c1-review.md。原 review 保持只读。

| 本次正式 C 报告 | 字节 / 行 | 独立 SHA256 |
| --- | --- | --- |
| [C1 plan](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-c1-plan.md) | 22413 / 157 | 8a9ed80fbc038b7b75ee5d1a532d7b6ecfbb352bdde09ba32ee52b876bd2a9ca |
| [C1 scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-c1-scope.json) | 633186 | da3eabc8fc96834bdcf3f61805afc2e606e3aa741b82cf5c97982d4b582ec058 |

两文件与本次 formal final 一致，scope 内 plan 绑定一致；满足 ≤320 行、≤4 MiB。scope 自身摘要由正式 final 外部绑定，没有自哈希循环。

## 2. F1：前序读取、有限证据和入口重绑定已闭合

定位：[plan:26](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-c1-plan.md:26)、43、54；[scope:7579](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-c1-scope.json:7579)、8833、9385、9958、11708。

独立按原 234 输入＋原三报告＋本次 C1 两报告及其独立 review 组成共同 240 项，再逐阶段展开前序 create.production/tests/assets/tools/naturalMeta/reports、独立 review、证据根描述/manifest，以及 B-PUBLISH/C 的真实内容 review 路径，和实际 readPaths 比较：

| 阶段 | 去重后 readPaths | 前序 create 文件数 | 本阶段合法创建后可读数 | 差集 / 重复 |
| --- | --- | --- | --- | --- |
| 025-P2A | 240 | 0 | 15 | 均为 0 |
| 025-P2B-PREPARE | 258 | 15 | 25 | 均为 0 |
| 025-P2B-PUBLISH | 287 | 40 | 14 | 均为 0 |
| 025-P2C | 304 | 54 | 20 | 均为 0 |

四个 Core 上下文文件、demo-r1.source.json/meta、PublishedContentCatalog 与同批实现、五个 first-release 实物/meta、前序 C/R 报告、内容验证与真实 reviewEvidence 入口均有准确许可。所有本段 modifyExisting 和 readFutureGatePaths 也均包含在可读集合内；新建目录不授递归权限。

有限证据机制使用四个 TestArtifacts/FMDemo025P2/<stage> 专用根；准确 root-identity.json/read-manifest.json 由已接收前序 scope 按路径、字节、SHA 绑定，items 必须与该 scope 的有限清单一致。拒绝绝对/盘符/UNC/设备路径、点段/越根、通配符、ADS、NUL，并核 reparse/junction；不递归发现额外项、不执行清单指令。当前阶段自己的运行输出仍须后续源码包逐项签发，未来文件标 EXPECTED_NOT_CREATED_UNBOUND，没有编造身份。

Tools/Invoke-FM025P2Validation.ps1 的读取与执行分列：A 合法生成后读取，后继继承准确文件身份且无修改权；执行须 SD00 源码包、阶段参数和输出根明确后才生效。四阶段 enabledInCurrentReportTurn 均为 false。

各 entryGate 均要求准确 completed、非异步 final、C 报告身份、独立唯一 ACCEPT 和 SD00 接收/本段签发。入口取最近已接收前序 scope.stageExports.implementationFiles 的 fresh 路径/字节/SHA，且保留更早阶段继承项；报告/review 身份另由接收登记绑定，不形成循环。

独立核过各修改项 priorAuthorizedWriters 与前序逐文件修改表，全部一致；current 均明确为本轮观察值。预期差异映射到前序已接收改动，未知差异保全并停止受影响阶段；没有要求合法前序修改后仍匹配旧 SHA，也没有直接把磁盘最新值当获准基线。阶段继承没有扩大本段写入表或预算。

## 3. F2：完整原意图和四种持久状态已定义

定位：[plan:65](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-c1-plan.md:65)、90、116、136；[scope:7460](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-c1-scope.json:7460)、11100、11308、11403。

- Core 不可变记录含格式、原 PlayerId/OperationId、完整 ContentBinding、新档定义 ID/正整数版本、完整有界冻结定义、完整规范初始化意图、格式与摘要，以及所有已生成材料。字段重复部分必须一致，深复制及预算先于分配；记录不依赖当前启用集合，摘要不充当正式准入凭证。
- 准确接口已落入正文和 signatureAdaptations：RecordCreateIntent 接完整 Core 记录；Read 返回含已校验记录/物理观察的结果；RecoverCreateIntent/ContinueCreate 显式恢复和续办；ConfirmCommit 接准确 expected observation 和 Core 初始化提交证明。相同记录幂等，同 ID 不同载荷冲突且保全。
- Core 提供 Freeze/Write/Read/RestoreIntent/VerifyInitializationCommit，既有内部意图 codec 增加 ReadFrozenPublishedInitialize。Application ResolveExact 原包并核定义/规范字节，恢复原 prepared；Platform 仅持久化 Core 记录、物理观察和确认收据，不引用 Application DTO。
- 首次 PlayerSave 之前必须完成记录持久写和复读。写入失败/未知不能越门；坏记录、未知格式、缺原包、绑定不一致、超预算均区分返回且保全，先校验再用 PlayerId 选路径，不降级为无档。

| 持久状态 | 已明确的唯一继续行为 |
| --- | --- |
| 完整创建记录，但尚无候选/完整头 | M12 完整观察证明无保存后，以原 ID、原规范意图和 ResolveExact 原包首次提交；启用集合 V1→V2 仍使用原 V1，不重新准备或抽样。 |
| 原未决候选或物理结果未知 | 核原 OperationId/Commit、父链、字节、能力，沿 M12 查询/Resume/Resolve/Retry；不再调用初始化 builder 或生成另一候选。 |
| 完整头已提交，active 确认未成 | 在原写租约及同一 M02 排他办理内核原初始化记录、规范字节、CommitId/索引与创建记录，再补 ConfirmCommit；不重复初始化/发奖。 |
| active 已确认 | 打开同一 PlayerId 的当前完整头；相同确认返回原结果。active 只锚定创建身份，不把当前头固定到初始化提交；档案丢失/损坏不得回退到首次初始化。 |

首次 active 确认之前关闭普通业务写入；过时 owner/observation 与不同 profile/初始化 Commit 拒绝，已确认的同一收据可幂等返回。通用 End/EndObserved 不能终止未确认 Initialize，关闭应用只释放资源。本次遵循 §275 选定的续办方案，没有采用旧审查曾允许讨论的“终止创建”分支。

直接对照当前接口：CandidateLifecyclePreparation.cs:16–30 重新 Prepare 确实重生成两 ID；CandidateApplicationIntent.cs:114 的冻结类型及 CandidateApplicationIntentCodec.cs:21、50 含原规范字节/上下文/初始化字段；CandidateApplicationRecords.cs:62、117 提供原操作记录、头及 descriptor；CandidateApplicationRecovery.cs:74、81、154 和 CandidateApplicationRuntime.cs:159 显示原查询、无档准入与未决候选恢复路径。拟增记录能供 Core 还原原意图，续办仍由原 runtime/M12 承担；未另起第二套业务规则。

已核 Core、Platform、Application 当前 asmdef 方向；新增记录、材料、证明与 codec 均归 Core，Platform 使用 Core 类型，Application 调用 Platform/Content。该方案不要求 Platform→Application 反向引用。新档定义的准确 ID/版本/规范字节由原拟建 Content models/catalog 只读投影提供，未另加编译器路径。

未来 C1-V01～V05 覆盖三个中断窗口、第一窗口切换启用集合、原 ID/绑定/字节及单次初始化、缺包/坏记录/未知格式/超限/同 ID 不同载荷、重复/过时/错 owner 或 Commit 确认、active 档丢失与程序集方向。全部明确为未来 P2C 拟验收，本次未执行。

## 4. 写入范围、未变设计和 fresh 身份

新旧 scope 结构比较确认：七流程映射、approvedContent、actualDependencyAudit、additionalPackages、remainingPackets、countProposal、preservedLimits、原 V01～V10、原 invariants、publicationReceiptProtocol、禁止项/rollback 及四阶段预算保持。原 14 项 signatureAdaptations 仅 F2 的索引 8/12 修订，并追加两项 Core 契约；新增字段/职责和运行门都可追溯到 F1/F2。

未来写入清单仅 P2C 增加 Assets/Scripts/FightMatch/Core/PlayerProfileCreateRecord.cs 及自然 meta，并在 modifyExisting 增加 Core/CandidateApplicationIntentCodec.cs；该 codec 已在 A 的原表内，因此既有唯一修改路径仍为 58。C 的本段修改项由 20 到 21，其余逐文件写入项保持，四阶段原预算全部相同。本轮这些未来 production 文件均未生成。

2026-09-24T05:06:12.3408252Z，R 独立按 §268 已接收 scope 的 implementation668.files 逐项读取字节并复算 SHA：668/668 相同，总 3569979 字节；同时与 C1 fresh 清单比较为零差异。基线来源 demo-025-p1-c1-scope.json 为 833816 字节，SHA f8ba3e2e25fa3d7d87fe1fd2c75fbddbc92c253d131b198fccb82ae34edbb677。

基线顺序 path TAB SHA LF 摘要为 c11a8a8aef0e46e00d364836e8baab3baea01fa45c81adddefae7cd7b8c84ea0；path TAB bytes TAB SHA LF 摘要为 81db0fb2a13d3e8e1dc372b66e0bbe66be79ffae462e65b8f52220719a822c09。Scripts/Tests 实际 654 文件与相应基线集合相同，无增删；Assets 文件数仍 681。

原 234 输入只有以下三协调稿因 SD00 本次签发变化，其余 231 保持；C1 的 237 输入和当前实物全部相符。当前三稿身份与 C1 inputBaselineComparison 一致：

| 协调稿 | 当前字节 | 当前 SHA256 |
| --- | --- | --- |
| docs/system-design/2026-09-17/system-task-packets.md | 1037711 | 0b8a3c656e7a400d9d8def6b47073068cf57e6a7cf7c86650ce399e7f4c2b857 |
| docs/system-design/2026-09-16/session-plan.md | 242369 | 560371961d5e7023553dbf8033d7529d75deecc8712953588b6809f90f604d14 |
| docs/system-design/2026-09-17/integration-review.md | 333292 | dd59a6e310302be4f22dd88e4a854a72ddfd64e7fa57eba5859d8c228a6c9d17 |

原三报告逐项保持：plan 24285 字节 / 24bc8ae17f7c682fcf3bcbdc2a1ef6f020b2d7a5d51e0d839664db14672edfdc；scope 486838 字节 / 9d946be13720b10844e2f534f4c52bb5f1462c20f6a16f7aedfd804e0ff4acf5；review 13471 字节 / 61878ccdd33977909bb306d2d8fa1db35e43c7570a9fba1c684e713bfa17ab24。

§271 批准实物位于 TestArtifacts/FMDemoB17/62aa18a2eb9443d195296216f0184526/stage025p1/review-inputs/，独立复核全部保持：

| 实物 | 字节 | SHA256 |
| --- | --- | --- |
| publication-proposal.md | 7196 | fae6eabd82df49a01138a8e881fe1d9b99cef9712f9b0797e01dc298de5822a5 |
| source-ledger.json | 34768 | 049a70ddc1ad5314ab39f10e490010737ef806e73f3b04b4a263099bad3b492b |
| prd-candidates.json | 96954 | 1469ae1833d7d9709264c10b29ca065b6bf53c0425bc42ae0ed64fb6c338d3f4 |
| replay-evidence.json | 12528619 | d5117507ca3bc8e44ce26d6708049e5198a886b90056af28bdeb042c8129aa08 |

## 5. 实际验证与剩余边界

RUN — 只读 PowerShell 文档/JSON 检查：Get-Content -Raw | ConvertFrom-Json、ReadAllLines、SHA256.HashData；按实际清单逐路径字节/SHA、规范化范围摘要、新旧 JSON 结构、读取集合差集/去重、逐修改项入口映射核对。正式验证内联命令均 exit 0；证据为上述数值、空差集和逐项契约定位。05:08:49Z 再核 C1 两文件身份仍匹配正式 final，plan 无 TODO/TBD/FIXME。

一次按类型名猜测 CandidateApplicationSnapshot.cs 的辅助读取返回不存在；随后在许可源码清单中定位到 CandidateApplicationRecords.cs 并实际读取，未据此构造任何成功证据或遗留未核项。

NOT RUN — Unity 编译、EditMode/PlayMode、Player/Android、产品运行、参数求解、共同验证脚本和未来 C1-V01～V05；§275～276 仅授权报告/静态契约核对。本次没有新增测试通过数，不重跑历史 3424，不执行 Git、新任务或代理。

原已接收 28 项、025-P1/027、四实物内容批准及原 R 其他设计结论保持；共享上下文/唯一 runtime、候选隔离、发布/启用/PlayerSave 三门、原内容版本/31C/ε/奖励/身份/真实入场熵、旧候选兼容和精确旧根恢复不被本次更改。34 功能/51 交付仍是待 SD00 登记的建议。

合法 AwaitLinks/全倒补线、物理输入/像素/交互式 PlayMode、Player/Android 真机、额外保存故障及 §184 历史 NOT VERIFIED 保持；新定位器仍仅设计 Windows EditorProcessCrash，不声称断电或 Android 保证。

SD00 仍须核本次 C、R 各自实际完成门，登记设计接收后再签 025-P2 各源码阶段的精确文件、接口、验证和独立 R 门。本报告不自动授予任何源码实施/正式发布/玩家档案放行，不恢复五分钟跟进。正式交付后停止修改。

