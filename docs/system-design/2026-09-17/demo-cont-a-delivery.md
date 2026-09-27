# CONT-A 作者交付

实现与本轮规定验证已完成，独立代码审查待原 R 执行。本文不代 R 判 ACCEPT，也不预写原生 completed；准确完成由本任务 formal final 和原生状态外部绑定。

- 作者任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；实施 turn：01a0d75f-7c26-7d13-9eff-0f2a1c757f17；gpt-6-astra / max；Unity 2022.3.18f1。
- 工程根：D:/Unity/UnityProj/FightMatch。
- §333 原包：4be90b50aac817e8949a96fe4aaeb30d82dda9ef2e042d07545e3da2d9c25917。
- §336 接口补签（§337 冻结）：be50c5a34e14964f5a5e57b1ed2657a92f71c04a1857f7a0c07ed8ba8ca1839a。
- 设计三份输入及原入口完全保留；CONT-B/C、028、029未实施，34功能/29已接收/余5/51正向交付保持。

## 实现结果

M03 Roster 唯一拥有 CharacterId/ClassId 各唯一的实例集合、恰好三个可空槽及 FormationRevision；保留原实例和 OriginalSlot。M04 保持全局库存及逐角色负载，M05/M06 冻结实际参与者和实际槽，战斗、贡献、PRD、经验与结束收据按完整角色 key 处理。材料和首通一次计入，未参与者不获本次收益。

PlayerSession 提供 QueryRoster、PrepareFormation、PrepareFormationEntry、PrepareRosterMigration、PrepareNewRosterProfile，继续通过原可信生命周期提交门。显式时间冻结后，在同一候选内办理选中角色恢复和 H02；无 Ready、携带/能力/级别等失败不发布恢复。重试使用原 pending/候选；Restart 保留原参与、位置、Stats/HP和三域初态。

正式 v2 只在已确认 Active、已验证空闲头且无未决事务时显式迁移。旧活动、回退、S17、未知写候选与未完成 F2 先按旧格式续办。迁移保留 Player/W/绑定、领域事实/修订、旧历史/索引/收据和原初始化锚，仅增加 M02 generation 与迁移记录。

旧候选 schema1、旧正式六 schema2、FMINT001/002、FMPROF01 保持；新正式向量仅 [3,3,3,3,2,2]，FMINT003 仅新集合 Initialize/Migrate/Formation/Entry，FMPROF02=record2/intent3。M12、FMAP01、真实首包六实物与298字节配方不变。旧 PrepareNewProfile 保持原 v2，新入口显式选择 v3。

§336 只将本轮新增 Begin.Participants、Begin.FormationRevision、EntryCheck.Participants 改为 GetParticipants()/GetFormationRevision()/GetParticipants()。内部共享原只读数据，旧公开属性集合、Participant 单元素/多元素歧义及无结果 null 语义保持；不改旧反射测试或首包。

## 源码与范围

722→744实现、752→774 Assets、398→409唯一GUID；838项导出输入/实现齐全。37旧生产实际变化，另外685原实现逐字节不变；6新生产、5新测试（含helper）、11 Unity自然meta。38原文副本和60最终副本均绑定，无删除、旧测试/旧断言/旧meta/asmdef/Content/Platform改动。

| 旧生产文件（Assets/Scripts/FightMatch/ 下） | 增 | 删 | 合计 |
| --- | ---: | ---: | ---: |
| Core/CandidateBusinessSnapshot.cs | 18 | 4 | 22 |
| Core/CandidateInventoryState.cs | 24 | 4 | 28 |
| Core/CandidateInventory.cs | 104 | 37 | 141 |
| Core/CandidateInventoryDefinition.cs | 2 | 1 | 3 |
| Core/CandidateProgressionState.cs | 20 | 5 | 25 |
| Core/CandidateProgression.cs | 75 | 1 | 76 |
| Core/CandidateProgressionRequests.cs | 13 | 0 | 13 |
| Core/CandidateProgressionDefinition.cs | 3 | 1 | 4 |
| Core/CandidateBusinessSaveCodec.cs | 37 | 15 | 52 |
| Core/CandidateBusinessSaveValues.cs | 3 | 1 | 4 |
| Core/CandidatePermanentSaveCodec.cs | 43 | 13 | 56 |
| Core/CandidateBusinessRestoreChecks.cs | 162 | 54 | 216 |
| Core/CandidateApplicationIntent.cs | 43 | 1 | 44 |
| Core/CandidateApplicationIntentCodec.cs | 106 | 19 | 125 |
| Core/CandidateApplicationProtocol.cs | 103 | 45 | 148 |
| Core/CandidateApplicationRecords.cs | 47 | 12 | 59 |
| Core/CandidateApplicationReferences.cs | 134 | 62 | 196 |
| Core/CandidateApplicationSaveCodec.cs | 83 | 38 | 121 |
| Core/PlayerProfileCreateRecord.cs | 14 | 12 | 26 |
| Core/PublishedSaveContext.cs | 28 | 6 | 34 |
| Core/BattleEntryPreparer.cs | 3 | 1 | 4 |
| Core/CandidateBattleOperations.cs | 43 | 24 | 67 |
| Core/CandidateEnemyPhase.cs | 67 | 33 | 100 |
| Core/CandidateHistoryOperations.cs | 7 | 2 | 9 |
| Core/CandidateBaseRewards.cs | 121 | 63 | 184 |
| Application/CandidateApplicationModels.cs | 3 | 1 | 4 |
| Application/CandidateBattleApplicationBuilders.cs | 7 | 5 | 12 |
| Application/CandidateDemoView.cs | 19 | 5 | 24 |
| Application/CandidateLifecycleApplicationSystem.cs | 21 | 9 | 30 |
| Application/CandidateLifecyclePreparation.cs | 66 | 12 | 78 |
| Application/CandidateLifecycleModels.cs | 11 | 3 | 14 |
| Application/CandidateLifecycleEntry.cs | 1 | 0 | 1 |
| Application/CandidateLifecycleEnd.cs | 25 | 20 | 45 |
| Application/CandidateLifecycleView.cs | 11 | 4 | 15 |
| Application/PlayerSessionSystem.cs | 6 | 4 | 10 |
| Application/PlayerSessionModels.cs | 34 | 0 | 34 |
| Presentation/CandidateBoardInputController.cs | 2 | 2 | 4 |

| 新文件 | 物理行 | 上限 |
| --- | ---: | ---: |
| Assets/Scripts/FightMatch/Core/CandidateRosterState.cs | 208 | 240 |
| Assets/Scripts/FightMatch/Core/CandidateRosterSaveCodec.cs | 37 | 150 |
| Assets/Scripts/FightMatch/Core/CandidateRosterProtocol.cs | 185 | 220 |
| Assets/Scripts/FightMatch/Application/CandidateLifecycleRoster.cs | 136 | 220 |
| Assets/Scripts/FightMatch/Application/PlayerRosterSession.cs | 88 | 169 |
| Assets/Scripts/FightMatch/Application/ApplicationTestAccess.cs | 1 | 1 |
| Assets/Tests/EditMode/FightMatch/CandidateRosterTests.cs | 238 | 340 |
| Assets/Tests/EditMode/FightMatch/CandidateRosterBattleTests.cs | 159 | 460 |
| Assets/Tests/EditMode/FightMatch/PlayerRosterSessionTests.cs | 250 | 440 |
| Assets/Tests/EditMode/FightMatch/PlayerRosterMigrationTests.cs | 307 | 700 |
| Assets/Tests/EditMode/FightMatch/PlayerRosterTestData.cs | 193 | 260 |

旧生产增删2028/2600，新生产655/1000，生产总计2683/3600；新测试1147/2200，逐文件上限全部满足。未用合并语句/删校验绕过预算。
固定工具本轮增删21/60，继承122后累计143/200，最终253/360行；只增加 CONT-A 固定 Stage，旧六 Stage 与 ExpectedScriptSha256 门保持。

## 实际验证与失败保留

命令均通过 Tools/Invoke-FM025P2Validation.ps1 固定 CONT-A Stage；各 run.json 保存完整 Unity 参数、PID、实际时刻。Tests 无 filter、无 -quit；串行准入且无其他本项目 Editor。

```powershell
& './Tools/Invoke-FM025P2Validation.ps1' -Stage CONT-A -Mode Compile -ExpectedScriptSha256 aa3ad8bc3f052608622c4cb4b7cb8e5f96ede3c67eb498f78a6766e9fbaadae1
& './Tools/Invoke-FM025P2Validation.ps1' -Stage CONT-A -Mode Tests -ExpectedScriptSha256 aa3ad8bc3f052608622c4cb4b7cb8e5f96ede3c67eb498f78a6766e9fbaadae1
```

| Run | 类型 | UTC开始→结束 | 实际退出 | 结果 |
| --- | --- | --- | ---: | --- |
| 001 | compile | 2026-09-25T08:21:20.3735220Z → 2026-09-25T08:22:20.6896093Z | 199 | 失败保留 |
| 002 | compile | 2026-09-25T08:23:17.3108774Z → 2026-09-25T08:23:30.1260434Z | 1 | 失败保留 |
| 003 | compile | 2026-09-25T08:24:42.7568106Z → 2026-09-25T08:24:46.7141882Z | 1 | 失败保留 |
| 004 | compile | 2026-09-25T08:26:45.2331512Z → 2026-09-25T08:27:13.9797447Z | 0 | 编译通过 |
| 005 | tests | 2026-09-25T08:27:57.0022424Z → 2026-09-25T08:46:36.1402447Z | 2 | 全量失败保留 |
| 006 | compile | 2026-09-25T09:04:08.4278947Z → 2026-09-25T09:04:22.7536952Z | 0 | 编译通过 |
| 007 | tests | 2026-09-25T09:04:58.8350132Z → 2026-09-25T10:07:06.6655777Z | 2 | 全量失败保留 |
| 008 | compile | 2026-09-25T10:09:54.9384102Z → 2026-09-25T10:10:06.3332672Z | 0 | 编译通过 |
| 009 | tests | 2026-09-25T10:10:58.9451264Z → 2026-09-25T11:12:03.3096390Z | 2 | 全量失败保留 |
| 010 | compile | 2026-09-25T11:15:19.4721474Z → 2026-09-25T11:15:29.9279126Z | 0 | 编译通过 |
| 011 | tests | 2026-09-25T11:16:20.2112777Z → 2026-09-25T12:16:20.2984701Z | 0 | 全量通过 |

首次 Windows PowerShell 5 预检因 UTF-8 历史 JSON 按 ANSI 解析失败；尚未启动 Unity 或分配 run，不改输入/工具，改用本机现有 PowerShell 7.6.5。run001 是沙箱内 LicenseClient IPC 超时；后续批准方式连接本机许可证。run002 修正新增解码分支局部变量重名，run003 修正新增 LINQ 导入与 void 调用错误。

run005：3724总数、3618通过、106失败（56旧+50新）、0跳过。新增 M05 公开属性改变旧反射回放/首包验证，按§336修正读取形态；同时保留 RecoveryResults 旧 null 语义，修正新测试的 BigInteger、非胜利 Remaining、旧隔离 PlayerId，补足格式负例和公开形状测试。原失败 XML/日志以及源码和 DLL 身份快照全部保留。

run007：3733总数、3730通过、3失败（1旧+2新）、0跳过。最后一项旧失败源于解码器仍给无结果记录传空恢复集合，已仅对 EnterFormation 保留显式空集合；新增初始化/旧入场/H02解码断言。新ClassId负例改为实际M03切片及UTF-16LE；换槽断言改为与每次换槽前修订比较，保留正常退出的既有增修订行为。原旧测试/断言全部不改。

run009：3733总数、3732通过、1新增测试失败、0跳过，原3655已全部通过。CA01原断言把结构非法与M03所有权核对都要求在Prepare拒绝；现按契约分开证明重复ID准备拒绝、未知ID提交时BuilderRejected/InconsistentBinding、原head/存储不变且无pending。生产源码未因此修改。

最终 run010 Compile exit0，无编译错误；run011 全量 EditMode exit0，3733通过、0失败、0跳过。
原3655具名实例（3650不同fullname）按多重集合逐一保全并通过，新增78项另列于 named-tests-after.json；原集合SHA256=6ab79d7846638672b60b2913c9c1ed3540228436cdf683e3401847b31e303d24。
run010后 = run011前 = run011后 = 交付实物：744项实现文件及36 DLL/PDB长度/SHA完全相同。通过后未再跑Unity。

## CA01～18 证据

每项具体完整 test fullname、参数实例、源码方法/行号与原始 XML 绑定在 case-evidence.json 和 scope.criteria。下表是作者验证结果，独立验收仍待原 R。

| 验收 | 结论 | 主要断言/边界 |
| --- | --- | --- |
| CA01 | PASS | 真实已批准 W 经 PlayerSession 移入 0/1/2、恢复回读及实际入场，原实例/初始槽不变，每次换槽保全当时角色修订（正常退出仍按原规则增修订）；空阵容入场拒绝且存储不写，重复ID在Prepare拒绝、未知ID在可信Submit返回BuilderRejected/InconsistentBinding且无pending，纯 Core 错误身份拒绝。 |
| CA02 | PASS | 恢复中的 C/A 换位、取消及旧 A 迁移保留 RecoveryId/起点/进度；查询与失败候选不发布恢复，未选 D 不自动入场。逐角色负载由 M04 集合往返及未参与负载断言补证。 |
| CA03 | PASS | 四个不同 CharacterId/ClassId 的纯 Core Warrior 已解析定义，经同一内部生命周期 builder 真初始化、Propose、v3 Encode/Decode；CommitEligible=false，无 M10 假凭证或额外正式建档。 |
| CA04 | PASS | T=120 的四种物品及各自 L，选 A/B 冻结 C=99 后核 T/ΣL/ΣR/F、独立偏好与未选 C/D 负载；胜利/退出/重开逐行守恒。非空 carry 在 H02 战斗 ID/熵前整体拒绝。 |
| CA05 | PASS | 实际 3 人战斗以 B 攻击、按槽 C→A 承伤并倒下，PRD/贡献按完整 key；乱序成员/PRD/贡献、漏/重 PRD 与外来成员的恢复负例拒绝。 |
| CA06 | PASS | 真实整关 FinalReport 对每位参与者用原 ExactScoreCalculator 独立复算 XP，M03 对应经验/结束收据、材料2与首通1；未选 D 无收益/End/新恢复，重放原操作与重复 Propose 不增收益。 |
| CA07 | PASS | 单一冻结时间在一个候选里推进 C/A 恢复并选择 Ready；A 未到期仍留槽但不入场，全不 Ready 时不发布；旧 A 迁移后到期恢复+H02 只新增一条 M02 记录。 |
| CA08 | PASS | 真实 W facade 的准备检查、snapshot-before/promoted、marker-before/after、已提交后应用回读注入；候选存在时逐字节比对、冻结 intent/time、原 op/commit 保全，Retry/Resolve/重建 OpenExisting+ResumeObserved 只生效一次。Prepare 尚无落盘字节时以原内存 pending 源码和一次记录断言证明续办。 |
| CA09 | PASS | 冻结阵容/head/revision、同操作优先、异意图冲突、Unchanged 不增阵容修订；活动战斗/S17、保存 pending、未确认 F2 及过期库存/head 门拒绝；历史冻结槽保持。 |
| CA10 | PASS | Restart 逐个比较原参与 C/B、槽0/2、等级/Stats/HP、基线与三随机域初态；A 在后续恢复后仍不加入旧重开，正常新入场才新增一次熵调用。 |
| CA11 | PASS | 真实 schema2 idle/exit/restart/rollback/victory 原路完成后显式迁移，五领域以旧格式/旧头逐字节可逆投影，旧操作/原绑定/收据/修订/保留物理祖先字节/初始化锚保持；纯 Core 旧恢复状态和真实旧未决 H02 分别补证。 |
| CA12 | PASS | 迁移 snapshot-before/promoted、marker-before/after 故障后沿原 intent/commit/候选字节 Retry 或重建后 ResumeObserved，迁移记录恰好1；完成后重复 Submit/Resolve 不重复迁移。 |
| CA13 | PASS | FMPROF01/FMINT002 与 FMPROF02/FMINT003 各在 record、snapshot-promoted、marker-before/after、确认前/后/坏回读七点重建；先以原字节完成 generation1/index0/record0，再迁移或换位，继续建档仍返回原锚；真实298字节配方、单W/lv1/xp0/HP100/空库存及混配负例。 |
| CA14 | PASS | 五领域和六切片规范往返，集合低预算/非法 Unicode/未知 intent 版本、schema99/混配/all3、未知或缺 feature/错误 Player/Class 引用，以及恢复所需旧根/旧新合同/精确包缺失均拒绝；原 envelope 或内存实物字节不变。 |
| CA15 | PASS | 多角色仅合法纯 Core 闭包与相同内部 builder 的隔离能力证据；真实 session 只消费首包已批准单 W，并使用相同 Query/Prepare/Submit/Restore 门。 |
| CA16 | PASS | 原3655具名实例按 fullname 多重集合逐一保全及全通过；旧测试和断言原字节保持。新增 §336 测试核旧公开属性/字段集合及三只读 Get 方法，legacy 单值/null 与 multi AmbiguousCharacter 保持。 |
| CA17 | PASS | 固定 CONT-A 工具串行 Unity2022.3.18f1 Compile 和无 filter 全量 EditMode；最终源码744/36DLL前后同一身份，实际退出码/XML、37旧差异、11自然meta、各次失败及预算纳入有限证据。 |
| CA18 | PASS | 非空 carry/缺暴击系数在候选发布及战斗 ID/熵前拒绝；公开任意 builder 对正式 PlayerSave 仍拒绝且回调不执行；M03 Roster/M04 Inventory 是唯一权威，十精确恢复合同和两 feature 明确。 |

## 证据身份与保护

唯一新根为 TestArtifacts/FMDemoCONT/cont-a。read-manifest 仅列实际文件并排除自身，root/manifest与两报告由 final 外部绑定，无自哈希循环。before-text/旧失败/原设计和六根历史证据不覆盖。
本根实际203/260文件，manifest 202/512项。六根4695唯一实物、123971385字节于2026-09-25T12:17:41.115746Z fresh核零差异。
entry原始字段只记了13项根/manifest级检查，未回写伪作4695；完整4695项 fresh结果在scope-audit的earlierInheritedFresh及inheritedFresh分别记录。

| 实物 | 字节 | SHA256 |
| --- | ---: | --- |
| TestArtifacts/FMDemoCONT/cont-a/root-identity.json | 701 | a8d319e2dda4bc878f7a7f9c3949c02e7b1741d6868d212776273173233e30db |
| TestArtifacts/FMDemoCONT/cont-a/read-manifest.json | 46021 | e8d36e282adf58ad19f53d2b1386b7733eef1364f481b85ffc51d67df52e76a2 |
| TestArtifacts/FMDemoCONT/cont-a/scope-audit.json | 552802 | a7a03a918a0e31778197fd1629d732eab1ecf0e45a115f6de03381021c8b1020 |
| TestArtifacts/FMDemoCONT/cont-a/case-evidence.json | 48054 | de38623721ea3997707069b169a649b1192329042bef1d35bfdc2d8c2c687632 |
| TestArtifacts/FMDemoCONT/cont-a/runs/011/tests.xml | 2746564 | 497b9048da65fae79bb1c550e70f4e6e580c1b697d8a9882327047e5610ea240 |
| docs/system-design/2026-09-17/demo-cont-a-scope.json | 3168159 | 8a36329870259bceaa76432c543e5dbc5a1d745d4308e02abb144bff5eb07543 |

三协调稿仅由 SD00 更新，按各自最新身份单列；首次技术请求发送曾被自动审批拒绝，用户随后明确授权本次 CONT-A 请求，SD00 已登记§336–337，无待答审批。

## 独立审查与剩余边界

原 R 任务01a0c1cd-dce1-7ac3-8780-06163cb0acfc，代码审查 turn=01a0d760-62e9-7162-892e-2ede600cb582。先核本次准确C原生completed、formal final及两报告身份，再审实际差异/全部CA/范围/真实验证，仅写demo-cont-a-code-review.md并给独立唯一verdict。作者final后停止修改。

多角色仅纯 Core 已解析定义及同一内部builder的隔离证明，未声称额外角色经M10发表或正式session赠送；真实session仍只用批准W。故障注入与对象重建使用隔离内存协议，不是新Windows崩溃/断电/Android物理证据。无新增physical probe、Player/Android构建、交互PlayMode、鼠标触控/像素验收；合法AwaitLinks公开见证及§184原未验证边界保持。

下一功能仍由 SD00 在准确独立 ACCEPT 后另签 CONT-B、CONT-C、028、029；025整体/B17及027已接收成果保持，本作者交付不增加已接收计数，也不宣告完整Demo。
