# 025-P2C 正式玩家接入交付

状态：COMPLETED（作者实现和规定验证完成，等待原 R 独立裁决）。本报告不作为独立 ACCEPT，也不将本阶段计作完整 Demo 或 Android Player。

- 原 C task：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本次准确 turn：01a0d666-8097-7780-b1a6-e70fe6d71e7a。
- 原生 context：2026-09-25T02:30:37.025Z；gpt-6-astra / max；D:/Unity/UnityProj/FightMatch。
- 冻结包 §322～323：SHA256 a0bfb5f579ccdc5d6d1c574af2e98179aa63b404210994db69dae9b3e3913655；§325 补已验证内容只读投影，§326 补两处明确目的分派。
- 前序 §321 P2B-PUBLISH 唯一 ACCEPT；保留原入口、787 项完整导出、六件真实首包、原失败及有限证据。
- entry.json 为 16943 字节 / 0c4a4844ab50ddfde841939b266a27a0b3fa6343fcfe523ee5dc7662905b7a3d，原 root/entry 未改。补签入口另存 entry-supplement-001/002.json，22+2+2 逻辑入口、44 最终副本。

## 实现与格式

原 CandidateApplicationRuntime/model/store/队列继续承担全部办理。PlayerSessionSystem 解析已发布内容、冻结或恢复原建档请求，接入原战斗和生命周期链。公开任意 builder Submit 仍只准候选；正式入口使用同程序集内部可信办理，先查原 operation 再运行 builder。

- 正式门面：OpenExisting、PrepareNewProfile、RecoverCreateIntent、ContinueCreate、CreateNew、PrepareLifecycle(PlayerLifecycleDraft, SaveCodecBudget)、PrepareVictory(CandidateTimeSample, SaveCodecBudget)。
- PlayerLifecycleDraft 仅业务目标及预期 head/revision，不提供 PlayerId、Stats、定义、奖励或随机材料注入；视图 purpose/绑定来自已核真实快照。
- Business/Application SaveCodec 新增 EncodePublished/DecodePublished；正式应用存档的六切片 schema2 和 fm.player.application.v1 真实编码/校验。旧候选入口及 schema1 原字节保持。
- 正式定义采用完整五字段及精确引用。growth/inventory/progression 为 schema1 发布载荷的固定单例槽 ID，recordVersion=1；关卡用 DefinitionBinding，奖励另核实际 ID/正版本。槽名不新增作者内容。EntryBaseline、实际 Stats/原槽、随机初/现态/耗字、报告/奖励/收据和 M02/S17 原样保存。
- 恢复先解析当前头、物理保留备份及未知候选的全部精确包/定义，再完整业务交叉核验，经原 WithVerifiedRecovery 发布；不完整旧根明确拒绝，不选 latest 或重建坏档。
- 旧 Prepare(input,budget) 和 M12 未决恢复三入口仍仅候选；显式 SavePurpose overload 校验正式用途，未知目的拒绝，目的分派不授写权限。
- §325 仅公开已核 31 项 Parameters 和新档六标量。Application.asmdef 仅追加 FightMatch.Content。

## F2 创建与恢复

Core 五入口为 Freeze/Write/Read/RestoreIntent/VerifyInitializationCommit。FMPROF01、recordFormat1 包含原两 ID、完整绑定、新档 ID/版本、有界配方副本（真实首包为 298 字节）、format2 规范 Initialize 及摘要、生成材料集合和 RecordSha。输入输出深复制、分配前预算检查；当前额外材料为空，H02 熵不在建档预造。

定位器使用宿主提供的独立 IContentPublicationStorage，完整记录持久并读回后才首次 PlayerSave。ConfirmCommit 接收 Core 从已完整解码快照生成的证明，在原 M02 办理/租约内核实际头，再写 105 字节 FMAP01。Active 锚定原 Initialize commit，当前进度头由 M12 选择。

| 状态 | 续办 |
| --- | --- |
| 仅完整记录 | RestoreIntent + ResolveExact 原 V1 后首次 Initialize，启用 V2 不改原配方 |
| 未决候选/未知 | 原 operation/commit/字节沿 Resume/Resolve/Retry，无第二次初始化 |
| 完整头已提交、确认未成/未知 | 核原初始化证明，只补确认；新业务及 End/EndObserved 关闭 |
| 已 Active | 打开同 PlayerId 最新完整头；重复确认不退头；缺档/坏档不退回建档 |

身份沿原 Guid，入场沿原 48 字节 RandomNumberGenerator。LocalPlayerClock 提供 DeviceUntrusted UTC 与进程域内精确 Stopwatch 毫秒；跨域恢复用 UTC。

## 实际验证

Compile run 008 和无 filter EditMode run 009 均实际 exit 0。EditMode 3655/3655，0 failed、0 skipped；原 3597 项具名多重集合全保留，新增 58 项通过。
XML：TestArtifacts/FMDemo025P2/p2c/runs/009/tests.xml；2630563 字节 / SHA256 5c6bdba060a7d4c7f2292bbe0b2ad39f59b3ef944a5d07184479c81bb6ea3be5。

| 准则 | 实际证据 |
| --- | --- |
| C01 | player-save-cases：六切片往返、原候选字节/入口、purpose/schema/能力/包/字段负例 |
| C02 | session-flow-cases：真实首包/Guid/参数、唯一 runtime、任意 builder 拒绝、真实视图 |
| C03 / C1-V01～V05 | creation-recovery-cases：四态、原 V1/身份/意图/初始化锚、一次初始化、确认幂等、坏/超限/未知/owner/proof/Active 缺档拒绝 |
| C04 | 正式 H02→攻击/报告→S17→H06→重开；奖励由本次 report 决定；重复增量0；SaveFailed/CommitUnknown 原候选字节续办 |
| C05 | 隔离 V2 多关正例、V1 原包恢复/缺包拒绝、跨域时钟；未变真实 L1 发布集 |
| C06 | scope-diff、line-budgets、meta-guid、validation、inherited-evidence；范围/导出/源码与 DLL 同版 |

| run | 模式 | PID | 实际 exit | 结果 |
| --- | --- | --- | --- | --- |
| 001 | compile | 19600 | 199 | 失败，原日志及 failure.json 保留 |
| 002 | compile | 18176 | 1 | 失败，原日志及 failure.json 保留 |
| 003 | compile | 16092 | 1 | 失败，原日志及 failure.json 保留 |
| 004 | compile | 27288 | 0 | 通过 |
| 005 | tests | 26584 | 2 | 失败，原日志及 failure.json 保留 |
| 006 | compile | 16596 | 0 | 通过 |
| 007 | tests | 2908 | 2 | 失败，原日志及 failure.json 保留 |
| 008 | compile | 4124 | 0 | 通过 |
| 009 | tests | 11348 | 0 | 通过 |

run001 许可证 IPC 失败（199）；run002 连接成功但无有效 Editor 许可证（1）；用户恢复后 run003 实际编译发现新测试 Core 辅助名冲突，已修正。全部测试失败的准确名字/诊断保留在相应原 XML；后续修正不覆盖旧运行。

最终 722 实现/752 Assets；原704中 23 项获准修改、681 项不变。9 自然 meta、旧389全部保持，最终398唯一GUID。最终源码和36DLL与最后全量测试同版。

## 范围预算与交接

- 旧源码增删 479/2400；其中 Content 两项 17/45，§326 调用点两项 4/4。
- 新生产5文件 651/1200 行；新测试4文件 857/2400 行。
- 工具本轮增删 21/80；相对原始累计 122/200；物理 236/360 行；P2C仅Compile/Tests。
- 逐行增删及所有前后 SHA 见 scope/line-budgets；旧测试、旧 meta 不改。准许但未改的 CandidateBattleSaveCodec / CandidateBusinessRestoreChecks 仍沿旧实现。
- 完整导出810项=原787+PUBLISH三报告/root/manifest五项+新18Assets；逐项路径/身份核对，不以计数代替。
- 本根 manifest 166 项，另自身1项；继承PUBLISH/C2/C1/B/P2A有限项数分别 116,75,1442,1442,1454，原失败和报告保留。

新增故障用例采用隔离内存存储及对象重建；没有新 Windows 物理 probe、断电耐久或 Android 设备验证。profile-storage-plan.json 明确无新增物理路径；029 宿主负责产品根。

真实首包仍仅 level:ch01-01/1、new-profile:default/1，W Lv1/xp0/HP100/槽0、其他空状态；未固定 XP26、未新增 L3/卡/配方，六件首包及启用集合保持。未修改 UI/场景、CONT-A/B/C 或 Git，未新建任务/代理/自动跟进。

后继范围沿已接收 C1 保持：CONT-A 补三槽队伍、联合恢复/入场及正式格式迁移；CONT-B 补 H03 成本/效果预览确认、合成/学习/装备办理；CONT-C 补共用冒险、队伍、背包页面和导航。028 接战斗确认/重开/退出、真实结算及失败/未知状态；029 接实际启动入口、宿主保存根与完整连续体验。各段均须 SD00 独立签准确包，本交付不提前实施。

原 R 仅在本次准确 C completed + formal COMPLETED 两报告身份核符后独立审查 C01～C06/F2/实际差异与验证，给唯一 ACCEPT/NEEDS_FIX/REJECT。SD00 据原 R 接收，再协调025/B17、CONT-A/B/C、028、029；本 C 不代写或预判 R 裁决。

当前两报告的精确长度/SHA 由作者 completed formal final 外部绑定，不写自身摘要或循环引用；R 预期报告为 demo-025-p2c-code-review.md。
