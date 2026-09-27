# 025-P2B-PREPARE-C1 独立审查

VERDICT: NEEDS_FIX

C1 的旧依赖断言冲突已关闭；本报告完整核对原 B01～B06 与 C1。保留实际通过的内容、验证和范围核验，但严格解码存在一项 P2 缺陷：JSON 根值 null 可逃逸为 NullReferenceException，影响 source/review 与持久索引/release-set 的准确拒绝。未生成 demo-025-p2b-content-review.json，未批准真实 Publish 或启用。

**准确回合与完成门**

- R task：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；本次 turn：01a0d3c9-6eaf-74f0-8929-955d0727f06d；local，gpt-6-astra / max。
- R 原生 task_started：2026-09-24T14:19:48.811Z；turn_context：2026-09-24T14:25:00.881Z，模型、effort、工程根已核。本报告不预写尚未发生的 R completed。
- C task：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本次 turn：01a0d3c8-8128-7510-a5b4-2a1a09a4f30c；原生 context 2026-09-24T14:18:48.073Z，gpt-6-astra / max。
- C 非异步 formal final：2026-09-24T14:52:21.593Z；task_complete：2026-09-24T14:52:23.090Z；COMPLETED，idle，error=null。final 文本与 task_complete.last_agent_message 一致。
- 2026-09-24T14:52:48.7367100Z 实核下列两份文件与 final 长度/SHA 相符后，才进入本次完整实现审查；文件提前出现或旧回合完成均未代替此门。

| C1 正式文件（工程根 D:/Unity/UnityProj/FightMatch） | 字节 / 行数 | SHA256 |
| --- | --- | --- |
| docs/system-design/2026-09-17/demo-025-p2b-prepare-c1-delivery.md | 15987 / 155 | 972b0f92190e0f71fdc485b35538e1560fdc192084a07a95341b9bab895c3c78 |
| docs/system-design/2026-09-17/demo-025-p2b-prepare-c1-scope.json | 1633214 / 37428 | e13fb09957fa081335a7c1b3fd9155c51f4f89078aa6b4cb93dc2d4f48b96e25 |

原 B 的恢复 C turn=01a0d338-e586-7973-9050-a705cf6a4789 在 2026-09-24T12:38:13.681Z 正式 BLOCKED 完成；其原异步授权请求不算 formal final。原始 C turn=01a0d274-f73b-78f1-9055-f770107acc4b 是证据根创建者，resume-001 正确连接恢复回合；没有重写根身份。原 R turn=01a0d339-4a18-7a42-b871-cabc756b0012 因连接/压缩错误失败，没有独立 verdict；本报告只复用已核输入和只读审阅记录，不冒作此前已经完成审查。

原 B delivery 12783 字节 / SHA256=2c56cd0ee4150ef2e430c2f5f95cc90eb5255b6486dd9aad27261df5fe716cb5；scope 1518833 字节 / SHA256=f616044fcded0082dd1610a673cb224b1633576723419a30799908a33a0386ae。原 BLOCKED、003 失败、入口副本、authoring 九件及 probe 均保留。先前自动审批传递超时不是安全拒绝；旧测试例外已由 §301/304 技术批准，不再待用户回答。

冻结范围重新规范化核对如下（CRLF→LF，trimEnd+单LF，UTF-8）：

| 范围 | SHA256 |
| --- | --- |
| §292～294 | 687ef834f7b556fe1f6c8ed75e3fe406e58757292b45eda021382b707054b2de |
| §296 probe 补充 | 4941fa1fca9109a13ae38e66dbdb29979993eb936a3e5e15e0028a5de006e637 |
| §298 中断恢复 | e3d1987c0d3eb5a982fff8fd51aea6dca02bc87cda9fd9426a33011806394768 |
| §301～302 C1 | db77b27ee9ec96581b66ca07b99c6c35100952d209f1c2f7daa2b63ebe2983b7 |
| §304 括号勘误及既有 probe 复读 | c266b26cabb7a1b55547217841f5cfe9d13107464d2a91929093bb99b3c8ad1b |

**R1 — [P2] JSON null 根值没有转成拒绝结果（B01/B04，review 文件入口亦受影响）**

定位：[PublishedContentCodec.cs:124](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Content/PublishedContentCodec.cs:124)，关联该文件 25～38、58～63、226～227 行；[PublishedContentCatalog.cs:108](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Content/PublishedContentCatalog.cs:108) 及 81～82 行。

四个 UTF-8 字节 null 是可由外部 source/review 文件或损坏的索引/release-set 提供的输入。Parser 将其转成 null；ConvertValue 对引用类型允许 null；Decode<T> 不校验顶层非空，因而返回空引用。随后 DecodeSource 的 value.SchemaVersion、DecodeReview 的 r.OriginalBytes、Resolve 的 r.SchemaVersion、GetCurrentBinding 的 set.SchemaVersion 分别解引用。PublicationResult.Run 只捕获 ContentFailure/ExactMathLimitException，Catalog.Io 也不捕获 NullReferenceException，所以调用方拿不到 PublicationResult 的明确拒绝。

这不是“坏包被哈希检查提前挡住”的情况：binding 索引与显式 release-set 在此处先直接按固定键取出、解码，再检查内部身份/收据；仅将该记录替换为 null 就能到达空引用，无需改变其他记录或构造哈希碰撞。结果会打断后继精确加载/恢复分支，而不是按 B04 返回可处理的坏记录拒绝。Prepare 路径的 source 文件和 Authoring.Publish 的 review 文件亦可触发同一缺口。

静态复现条件（交原 C 补回归；R 没有运行 Unity 或执行产品 DLL）：

1. 对 PublishedContentCodec.DecodeSource(UTF8("null"), Current, 有效预算) 和 DecodeReview(UTF8("null"), 有效 StoreBudget) 调用，应返回拒绝；当前静态路径会抛空引用异常。
2. 在现有隔离 MemoryStorage fixture 已完成发布后，只把 BindingKey(one.Binding) 的字节改为 UTF8("null")，调用 ResolveExact(one.Binding, Caps())；当前在 Catalog 第109行抛异常。
3. 把指定 ReleaseSetKey(scope,id) 的字节设为 UTF8("null")，调用 GetCurrentBinding(scope,id,Caps())；当前在第82行抛异常。
4. 修正后应对这四条入口断言“不抛异常、IsAccepted=false、明确且稳定的拒绝码/字段”，并确认没有写入、替换内容或 latest 回退。有效对象中的 Context=null、Recovery=null 等批准空值必须仍合法。

现有 StrictDecoderRejectsInvalidShape 仅覆盖重复字段、空对象、浮点 schema 和尾随字节（PublishedContentCompilerTests.cs:100～104）；Catalog 损坏测试覆盖翻转内容字节、缺记录、schema99 等，没有覆盖上述 null 根。故现有全绿不消除此缺陷。最小修复是在 Decode<T> 顶层拒绝 null，再进入字段转换；不能全局禁止所有内部 null，也不建议用捕获全部异常掩盖缺失校验。

**完整条件核对**

| 条件 | 独立结论及实际依据 |
| --- | --- |
| B01 | 内容映射、规范字节、Unicode/预算/未知字段与 schema 的既有正负路径已核；null 根的结构拒绝不完整，见 R1。 |
| B02 | 已核真实旧精确数学、几何、完整隔离回放调用；Prepared 持有冻结副本，revision/cancel 最终复核，同草稿 Commit 与 Revise 共用锁；Prepare 不写发布库/current。 |
| B03 | 已核七类写入、同 op 同意图原结果优先、异意图冲突、逐件精确复读、部分/未知写保留、坏复读不 Completed、草稿 gate、缺/错 review 的内存正负例；review 文件 null 解码缺口仍关联 R1。Windows 创建与本轮复读范围分列如下。 |
| B04 | 五字段 binding、整关 ID/正版本、载荷/原 source/validation/review/operation/receipt 完整性、能力门、显式 release-set 与冻结新档字节均核；坏 binding 索引/release-set 的 null 根拒绝不完整，见 R1。无 latest 回退或隐式启用。 |
| B05 | 同一 Catalog 的多关 V2 fixture、V1 保留精确解析、错版本/删除 V1 拒绝均真实通过。真实 source 只有 L1；未发布真实 L3 或 fixture 内容；共享规则核未另起 Demo 分支。 |
| B06 | C1 编译/全量测试、原 prepare 继承、源码/资产/DLL/进程/XML、范围/预算/meta/清单已独立核符；旧断言冲突关闭。R1 仍需新增负例及修复后的相应验证，不能据全绿接收整体。 |

**验证、差异与范围**

- 原 B 001 compile：PID34020，2026-09-24T11:48:40.3887336Z～11:49:17.6568121Z，真实 exit0；002 prepare：PID6084，11:49:56.1150584Z～11:50:07.9272838Z，真实 exit0。prepare.log 第319行给出本次新指纹。
- 原 B 003 tests：PID3048，11:51:13.278910Z～12:01:03.7949209Z，真实 exit2；XML 3556 总数/3555通过/1失败/0跳过，唯一失败正是 DemoContentCompilerTests.NullAndTinyBudgetCannotCreateCandidate 第141行旧依赖断言。
- C1 001 compile：PID13544，14:29:08.400415Z～14:29:38.0466298Z，真实 exit0；002 tests：PID26648，14:30:57.6538531Z～14:41:23.6692795Z，真实 exit0。
- C1 XML 实际 3556/3556通过、0失败、0跳过；2495834 字节，SHA256=38474a8f4b8125325ba93d4878ee54f6e0979af2d48be4725172d769250fb125。原失败用例本次 Passed，duration=0.009119。原 A 3464 的 fullname 多重集无删除，原 B 新增92保留，B→C1 多重集完全相同。
- 检查实际 run.json/result.json、before/after 与日志：Unity 路径为 D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe，工程准确；Tests 无 -quit/filter。各运行前后没有另一个 Unity，串行时间无重叠。退出码来自 process.ExitCode，不以日志成功文字代替。
- 原 B 680→700 只修改 DemoContentModels.cs（3增）和 Content.asmdef（2增1删），合计6增删；DemoContentCompiler.cs 完全未动；其余678原实现字节不变。新增7生产共1053/2600行、3测试共430/2000行；20个实现项及3个额外 Assets 项符合清单。
- C1 相对 B 只有 DemoContentCompilerTests.cs 一项实现变化，精确2增1删；其他699实现不变。旧空输入、小预算、无 Publish/ResolveExact 方法断言和全部其余测试体保留；新增 Platform 必需断言，继续禁止 Unity/QFramework/Application。
- C1 工具适配20增3删=23/80，最终179/360行；相对原 B 入口的132行 A 版工具累计54增7删=61/160。实际只扩固定 Stage/根/作者/清单/运行号，C1 Prepare 提前拒绝，无 Publish 模式；只读 PowerShell 解析0错误。
- 当前实际700实现、716 Assets、379份唯一 GUID；Assets 路径集合无额外/缺失，308保护输入不变。旧367 meta 保全；原 B 001 编译前12份新增 meta 缺失，编译后自然出现并入清单，C1 全部379 meta 字节不变。
- 原 B 681入口副本与704最终副本，以及 C1 各704入口/最终副本均核；原 B entry 的680实现逐项对应 A 接收版本，C1 entry 对应 B 最终版本，C1 final 对应实际当前字节。
- C1 两运行源码/source/工具/Assets 前后相同；编译后36项 DLL/PDB 与测试前后和当前相同。与 B/原 prepare 相比只变 FightMatch.Core.Tests.dll/.pdb：SHA分别为60c8a024a21e984844284d3ac7440c829ebd075a22da7373fbdd37f29637afcc、3047fbc22a2c28f1f1e7272acecce5199ef2e5223d95a7c057664a11099efeae。
- 30项非测试 DLL/PDB（含 Editor）与原 prepare 前后保持；另4项其他测试 DLL/PDB不变。因此 §301.2 的本次 prepare 继承条件成立，未把已改变的测试二进制说成36项全部从 B 起不变。

原 B 与 C1 的有限 manifest 各1442项，均逐项核实际长度/SHA、路径/角色/重复项/越根/reparse；各根实物1443件含 manifest 本身，无额外或缺失。原 B root：516字节/49f2e30e864cfcb8b5289b7f57b4ee103ea0bb05c6fb911f1b18cd3709cd46d6，manifest：363700字节/9ef1d23b548f11477971f0c1c37ec63f7a9108cb61f07f4a0e4e12ec0c4a8ea8。C1 root：739字节/c2e8bee41daeccfe2c83c6d10adb8faae930aaaa01320ab729f85adae87d64fd，manifest：364997字节/c5981de520324999eeb00e87ab2e093ce467671505f11f55fe0d713f78b3776c。C1 scope 的 evidence.items 与 manifest 完全同项。

C1 stageExports 758项完整继承原753项，只更新测试/工具及3份可更新协调稿，新增原 B 两报告和 root/manifest/resume 共5项；755不可变项实际长度/SHA相符。协调稿按最新内容核冻结段，未作为污染。当前 C 两报告由 formal final 外部绑定，不自哈希；尚不存在 R ACCEPT JSON。

**批准内容与新实物**

四份批准输入保持准确原身份：publication-proposal.md 7196字节/fae6eabd82df49a01138a8e881fe1d9b99cef9712f9b0797e01dc298de5822a5；source-ledger.json 34768字节/049a70ddc1ad5314ab39f10e490010737ef806e73f3b04b4a263099bad3b492b；prd-candidates.json 96954字节/1469ae1833d7d9709264c10b29ca065b6bf53c0425bc42ae0ed64fb6c338d3f4；replay-evidence.json 12528619字节/d5117507ca3bc8e44ce26d6708049e5198a886b90056af28bdeb042c8129aa08，均位于原 stage025p1/review-inputs。旧候选选择5db393d0441880fc387ce7db0421bdf855b3b38b20edbfe818a83144bcbb5a12；历史 proposal 的“未批准”标题不撤销 §271/292 已有批准。

- 独立将冻结 source-ledger 的整关、敌人、奖励、原坐标路径，以及冻结 replay-evidence.Rows[0] 的完整成长定义按 §292 的正式 ID/版本映射，与 source 实值比较；无差异。未依赖作者的 matches 布尔作结论。
- 31个 Target/C/Epsilon 逐项与批准 prd-candidates.Rows[0..30].Evidence 比较，并与新回放 Parameters 比较；目标均精确为(40+i)/200，ε均1/1000000000，新证据各行容差成立；未重跑旧求参矩阵。
- source、实际 field-map.DecodedSource 共393叶逐项相符；approved-content-map 覆盖393项，无重复/遗漏。对 source 集合按已审规范排序后与 prepared payload 393叶比较相同；不是以原始索引误认排序后的 Sources/Capabilities。
- 独立按属性 ordinal 排序、精确 JSON 转义重建 payload 规范文本，11486字节/SHA与原载荷完全一致；新档298字节规范文本亦一致，SHA256=36f2d8935f626c4bdafb53c56ac288f5199e7a6dec29680c0fac02f354685646。
- Package=package:fightmatch-demo-r1、Draft=draft:fightmatch-demo-r1、revision=1；level:ch01-01/正版本1，class:warrior、enemy:clockwork-infantry、passive:warrior-crit、reward:ch01-01、item:tin/wood 均匹配。
- demo-r1、标签 RC01 与 PC01+SC01 对应实际 NC01.ExactRational、RC01.PCG32.PRD、PC01.ExactParameterEvidence、SC01.sc01-pcg32-le128-v1 能力；未混用旧候选指纹。
- AssumedBottomLeft 是批准解释，原始来源方向仍未知。F1为4×4；A/B颜色0/1、敌实例A/B及槽/稳定序0/1；零基路径分别(0,0)-(0,1)-(1,1)、(0,3)-(1,3)-(2,3)，原坐标保留；6/16格，没有唯一解或铺满棋盘要求。
- new-profile:default/RecordVersion1，W/class:warrior、Lv1、XP0、HP100、槽0，空槽1/2，库存/携带/已学技能为空、Recovery=null，仅L1初开，无L1→L3、额外首通、卡牌/凭证/配方；没有冻结真实 PlayerId/OperationId/熵。
- 奖励 BaseXP20、tin2/wood0及原精确评分定义保持；真实闭环计算XP26只存在回放奖励，不是 source 固定值。成长/暴击/恢复定义沿原共享数学和规则核。

| 原 prepare 实物 | 字节 | SHA256 |
| --- | --- | --- |
| Assets/FightMatchContent/demo-r1.source.json（与 source-copy 相同） | 20443 | fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a |
| authoring/prepared.fmpackage.bytes | 11486 | b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129 |
| authoring/prepared.fmvalidation.bytes | 664 | 512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d |
| authoring/replay.json | 2389832 | 86e9f3aa1708fcdd9b9f6eb080ab74cc81acba4709f8c0f12e203bb4366cbfc4 |
| approved-content-map.json | 117327 | da286b0a01f2bd06c74bef915422f94d76e99b33d00683b33fb3389b5949c9f6 |

表中 authoring/ 与 approved-content-map 均从 TestArtifacts/FMDemo025P2/p2b-prepare 解析。validation 的 EvidenceSha256 准确绑定本次 replay 原字节，descriptor/validation/新回放的 ContentBinding 一致。新回放2个有效动作、2次 recorded replay 均 Matched，最终 NormalVictory/WonPendingSettlement，CommitEligible=false；隔离 seed=00..2f、SourceCapabilityId=isolated-validation:bytes-00-through-2f，未进入新档配方。检查新 Compiler 确实调用既有 PrepareCandidate、PreparePublished、ReplayCandidate、共享 PublishedRuleDefinitions，没有把旧证据贴到新指纹。

Windows fixture 调用生产 WindowsContentPublicationStorage：原 B 测试实际输出 created work, flushed, promoted，C1 实际输出 retained original exact blob。原 writer.lock 0字节，64个a.blob 为00010203/4字节/SHA256=054edec1d0211f624fed0cbca9d4f9400b0e491c43742af2c5b0abebf0c990d8，.work不存在，io-probe-plan保持原 PROPOSED 字节。C1仅既有排他lease开关、复读、幂等和异字节拒绝；未新增创建/Flush/提升证据。此项不证明强杀/断电/Android或完整持久化故障矩阵。

**最小后续纠正包建议：025-P2B-PREPARE-C2（交 SD00 签定，不在本 R 回合执行）**

目的仅为关闭 R1；保留 C1 已核的依赖契约、全部内容批准、正例、原证据和共享架构。入口为本报告所绑定 C1 两报告、758导出及两段有限 manifest，不退回旧 A 或覆盖 B/C1。

精确建议修改范围：

| 既有文件 | 限定修改 / 建议增删上限 | 当前入口字节 / SHA256 |
| --- | --- | --- |
| Assets/Scripts/FightMatch/Content/PublishedContentCodec.cs | 顶层 null 解码拒绝；保持内部合法 null、规范格式与预算；≤24行 | 18874 / 1d529b7e20386f0508bc02c222b2859bfc8ebd59bc155b858e41da366c927dee |
| Assets/Tests/EditMode/FightMatch/PublishedContentCompilerTests.cs | source/review 根 null 及带空白 null 的明确拒绝、合法内部 null 正例 | 11786 / a872779d0ec83f2c2101b913801428d08b69c3bbe7e3dd75001c3fcb2ca7c7d9 |
| Assets/Tests/EditMode/FightMatch/PublishedContentCatalogTests.cs | binding 索引/release-set 根 null 的拒绝、无异常/写入/回退；两测试文件合计≤100行 | 17196 / b258c3521a9b89d536576f5c1a8a5e9b2a91f753f5feb2dd5b7629df5499bb4a |
| Tools/Invoke-FM025P2Validation.ps1 | 固定 C2 Stage/根/当前身份与既有 Compile/Prepare/Tests 采集；≤40行，原累计仍≤160，最终≤360 | 12382 / 8600bc0df196cb44773dfa5ac9ffc45cf6f9644f0cc7e4019707f485fa2f416e |

不需改 Catalog 生产行为、Core、Platform、Models、Authoring、asmdef、source、旧 DemoContentCompilerTests、任何 meta 或应用/页面；如中央 guard 无法覆盖，应先给 SD00 准确最小扩展。保持700/716/379，无新增生产/测试文件、公开API、依赖或格式；禁止“catch所有异常即成功”、放松断言或把空内容当缺包/latest。

建议唯一新证据根 TestArtifacts/FMDemo025P2/p2b-prepare-c2；须由 SD00 正式授予，不能借本报告提前写：
- 固定20件沿C1：root-identity.json、read-manifest.json、entry.json、input-manifest.json、source-before.json、source-after.json、assets-before.json、assets-after.json、protected-before.json、protected-after.json、meta-guid.json、scope-diff.json、line-budgets.json、named-cases-before.json、named-cases-after.json、named-cases-diff.json、compatibility.json、validation.json、delivery-bindings.json、inherited-evidence.json。
- entry-files/<p> 与 final-files/<p> 仅上表4个可改文件；其他准确源副本继承 C1 的704项，并以完整700/716清单核身份。禁止覆盖原 B/C1 副本。
- authoring/ 精确九件：source-copy.json、prepared.fmpackage.bytes、prepared.fmvalidation.bytes、prepared-descriptor.json、field-map.json、geometry.json、replay.json、review-template.json、result.json。
- runs/001～008按真实每号单过程；叶文件仅run.json、before.json、after.json、compile.log、compile.stdout.txt、compile.stderr.txt、prepare.log、prepare.stdout.txt、prepare.stderr.txt、tests.log、tests.stdout.txt、tests.stderr.txt、tests.xml、result.json、failure.json，保留失败/中间运行。
- 原 probe 仅沿§304复读既有0/4字节实物，不新建/重写/Flush/提升/删除；原创建证据继承。无新probe、发布库或StreamingAssets写入权。
- 建议 C 正式报告仅 docs/system-design/2026-09-17/demo-025-p2b-prepare-c2-delivery.md（≤280行）与 demo-025-p2b-prepare-c2-scope.json（≤4MiB）；R后续报告路径由SD00另签。本回合不创建这些文件。

验收：先补能暴露当前缺陷的四类根值负例及合法内部 null 正例，再最小修复；保持原3556 fullname 多重集及其断言，不固定虚构新增数量。串行新编译、真实新 prepare、全量 EditMode 均真实exit0、0失败、0跳过。因生产Codec/DLL改变，不能沿用C1“生产同版”免跑prepare；新九件输出写C2根，与原已审source/payload/validation逐字节比较，正常内容不应改变。若有差异保全并报告，不换指纹沿用批准。捕获新源码/DLL/PDB/进程/XML及完整导出，保留原两段各1442证据。原C自然completed＋非异步formal final＋两报告后，原R再核R1与受影响B01/B03/B04及新验证；整体通过前仍不得生成真实ACCEPT内容凭证。

**审查执行与保留边界**

R只写本报告；未运行Unity、执行产品DLL、修改实现/source/工具/meta、写Git、创建任务/代理、恢复自动跟进或生成发布凭证。Git仅用于获准文件副本的只读diff；JSON、XML、哈希、行数与PowerShell解析在内存独立核验，未落额外审查文件。

审查侧一次读取范围偏差如实登记：核成长参数时额外只读 docs/game-design/balance/config.json，随后确认该文件不在本段258项语义readPaths中；虽其SHA与已批准溯源所记一致，这不产生语义读取授权。未修改该文件，已停止额外读取；成长结论重新完全依据获准冻结 replay-evidence.Rows[0].Result.Candidate.Character.Definition 与§292核得，不以该额外读取作为接收依据。此偏差属于R执行，不归责C，不据此要求C改配置或重跑无关验证。

作者末段的发布适配/六件首包为后继设计输入：当前Run仍仅prepare；既有Catalog精确读取原source，因此后续只读bundle需保留原source，实际发布/启用/导出仍待另签。本文不接收或实施该后段。

原P2A、已接收P1/027及其他无关成果保留；同产品Demo/Android共用代码路线不变。真实首包Publish/activate、PlayerSave、P2C/页面、025整体/B17、CONT-A/B/C、028/029和首Demo均未在本次闭环；34功能/28已接收/余6/51正向交付不增。物理鼠标/触控、像素/交互PlayMode、合法AwaitLinks、Player/Android、CI/lint/perf及§184旧NOT VERIFIED仍保持。正式结论与本报告长度/SHA在本次R final外部绑定，随后停改。
