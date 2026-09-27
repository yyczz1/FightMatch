# CONT-A 独立代码审查

VERDICT: ACCEPT

Scope: PASS  
Acceptance criteria: PASS（CA01～CA18）  
Verification: 原 C 最终同源编译 exit 0；全量 EditMode 3733/3733 通过、0 失败、0 跳过；R 独立核实源码差异、原始 XML、身份与保护集合。  
Notes: 未发现需要纠正的实质问题；物理崩溃、Player/Android、公开 AwaitLinks 及交互验收边界见末节。

## 1. 准确回合与完成门

审查依据是 [任务包 §333、§336](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md:6564)，并核对 §334/337 冻结、§335 派发及最新 §338 收件。设计审查结论未代替本次代码审查。

| 身份 | 原任务 | 本次准确 turn | 原生 context（UTC） |
| --- | --- | --- | --- |
| C 实现 | 01a0c403-bfa1-7e90-b503-c0fcd61f23c1 | 01a0d75f-7c26-7d13-9eff-0f2a1c757f17 | 2026-09-25T07:02:34.349Z |
| R 独立审查 | 01a0c1cd-dce1-7ac3-8780-06163cb0acfc | 01a0d760-62e9-7162-892e-2ede600cb582 | 2026-09-25T07:03:33.425Z |

两个原生 context 均为 gpt-6-astra / max，工程根 D:/Unity/UnityProj/FightMatch。C 于 **2026-09-25T12:24:20.425Z** 自然 task_complete，非异步 formal 为 COMPLETED；准确 turn 为 completed/error=null，任务 idle。R 先核该原生事件、正式答复和以下两报告/root/manifest 实物身份，随后才审最终实现；落盘前再次核 C 仍为同一 completed 回合且四项身份不变。

C 正常执行期间 R 仅准备和等待；本轮 R 没有启动 Unity、执行产品 DLL、修改实现或测试、写 Git、创建任务/代理或自动跟进。本次唯一写入是本文，本文身份在正式答复外部绑定；不预填 R 尚未发生的原生完成时刻。

## 2. 绑定输入与实物身份

§333 按 CRLF→LF、从标题到下一节前 TrimEnd+单 LF、UTF-8 规范化为 23359 字节，SHA256=4be90b50aac817e8949a96fe4aaeb30d82dda9ef2e042d07545e3da2d9c25917。§336 同法为 3257 字节，SHA256=be50c5a34e14964f5a5e57b1ed2657a92f71c04a1857f7a0c07ed8ba8ca1839a。两冻结值经 R 重算一致，scope-audit、scope 和 delivery 均绑定补签。

| 实物 | 字节 | SHA256 |
| --- | ---: | --- |
| [原设计](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-a-design.md) | 36200 | 0f4ac05443ef7218f6ef2b892095a82f2abcd6e72455372c653572240204d5fe |
| [原设计 scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-a-design-scope.json) | 2968593 | 2c9068d6e105ec789d2e6fb6476e9a000cb8652aa1fcc80b275f69e6aad67652 |
| [原设计审查](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-a-design-review.md) | 15738 | 7e251090bbe1ed5aacf8a74bc54c61d0dd0185edf96322024d29d2b49283c474 |
| [C 正式交付，167 行](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-a-delivery.md) | 17454 | e5ecaf01c94266891c98a1fdcf753793a9ce33e213ea62999583464e2ea6ae66 |
| [C 正式 scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-a-scope.json) | 3168159 | 8a36329870259bceaa76432c543e5dbc5a1d745d4308e02abb144bff5eb07543 |
| [本次 root-identity](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoCONT/cont-a/root-identity.json) | 701 | a8d319e2dda4bc878f7a7f9c3949c02e7b1741d6868d212776273173233e30db |
| [本次 read-manifest](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoCONT/cont-a/read-manifest.json) | 46021 | e8d36e282adf58ad19f53d2b1386b7733eef1364f481b85ffc51d67df52e76a2 |
| [scope-audit](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoCONT/cont-a/scope-audit.json) | 552802 | a7a03a918a0e31778197fd1629d732eab1ecf0e45a115f6de03381021c8b1020 |
| [CA 实例映射](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoCONT/cont-a/case-evidence.json) | 48054 | de38623721ea3997707069b169a649b1192329042bef1d35bfdc2d8c2c687632 |
| [最终原始 XML](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoCONT/cont-a/runs/011/tests.xml) | 2746564 | 497b9048da65fae79bb1c550e70f4e6e580c1b697d8a9882327047e5610ea240 |

SD00 可更新的三协调稿独立列示，2026-09-25T12:45:04Z 读取到 §338 收件，不把它们与不可变保护项混算：

| 协调稿 | 字节 | SHA256 |
| --- | ---: | --- |
| [session-plan](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-16/session-plan.md) | 274554 | caf25709a270d0005f43ebe30424ae8f1efc6c09a9763a315ce67012f18e3187 |
| [integration-review](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/integration-review.md) | 365715 | 2c8415ee01b8e56f305388de226114841ab99d0d97a12424f1760a426f201f33 |
| [system-task-packets](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md) | 1262723 | 474d9563312c9f79c8822a3f1cf6a0546d35d5d12b7f77b7543cb8d387d194e0 |

## 3. Standards：范围、预算与证据保护

R 以已签设计 scope 的精确数组为边界；322 条旧语义路径中原缺席的 demo-025-p2b-prepare-code-review.md 仍按缺席处理，未替换搜索未知内容。逐项读取了 37 旧生产和工具的真实差异、6 新生产及 5 新测试，核对 38 before-text 与原入口、60 after-text 与当前实物。2026-09-25T12:27:08Z 独立核得 744 实现、774 Assets、409 唯一 GUID、838 导出路径集合准确；685 项旧受保护实现与其他受保护导出逐字节保持。原 P2C 三报告及设计三报告保持。

| 预算项 | 独立结果 | 上限 |
| --- | ---: | ---: |
| 37 旧生产增删 | Git no-index 2024；作者差异算法 2028 | 2600 |
| 6 新生产物理行 | 655 | 1000 |
| 新旧生产合计 | Git 2679；采用作者较大计数 2683 | 3600 |
| 5 新测试物理行 | 1147 | 2200 |
| 工具本次增删 / 含旧累计 / 最终物理行 | 21 / 143 / 253 | 60 / 200 / 360 |
| 新证据实物 / manifest items | 203 / 202 | 260 / 512 |

两种差异算法仅在 CandidateApplicationReferences.cs 的重复行配对上相差 4 行：Git 132增/60删，作者 134增/62删；相同原文与最终实物均已核实，较大值仍在预算内，无遗漏文件。新生产按白名单顺序为 208/37/185/136/88/1 行，新测试为 238/159/250/307/193 行，逐文件上限全部满足。未发现删除校验、无关格式改写或压行绕预算。

工具只扩展固定 CONT-A Compile/Tests 及准确源集合，旧六 Stage、ExpectedScriptSha256 与进程准入保持；实际 20544 字节，SHA256=aa3ad8bc3f052608622c4cb4b7cb8e5f96ede3c67eb498f78a6766e9fbaadae1。Presentation 仅两处查询显式使用 SelectedCharacterId；ApplicationTestAccess 仅现有 FightMatch.Core.Tests 友元属性。Content/Platform、旧测试与断言、旧 meta、asmdef、配置、首包及配方未改。

11 个新 meta 在 run001 前后及 run002 前缺席、run002 后随 Unity 导入出现；run004 导入形成最终 MonoImporter 元数据。中间无运行间改写证据，原 398 GUID 保持、新 11 GUID 唯一。新证据根全部实际项属于已签 259+manifest 路径，202 items 的角色、长度和 SHA 均与实物一致，无额外叶、缺项或自哈希环。

R 于 2026-09-25T12:26:05Z 沿绑定 manifests 对六根 4695 items 重新核验，身份、长度及有限集合全符；原失败、原候选、首包与历史报告未覆盖。C 的 entry 仅记录 13 项根级检查，未被误写为入口已核全部 4695；后续完整 audit 与 R 独立检查补足实际证据。

| 历史根（TestArtifacts/FMDemo025P2 下） | items | root-identity SHA256 | read-manifest SHA256 |
| --- | ---: | --- | --- |
| p2c | 166 | c2aed95b9aabd2dae897623cd26af93158d505b5e0648cf673684708d4684869 | 6697c91d2864a944360d11aa8697b9c4f9c3fa489203011d80ac9e2efb5958de |
| p2b-publish | 116 | a65fb7d567267636ff9fcacf9bb235105f296670e1e3d356f855ff746cf20891 | a68653f38151031488cf800a73fd529557f2c0cf8e9e889d514ebe3d5776ff26 |
| p2b-prepare-c2 | 75 | 8b26bdfb385266d4d0d407559f0ec97d83686d5a29401faf74d8a33c1d743162 | 4323c10a7a3a66f882f93d6724e0dd0386d0e6058f66ab95c8621bacd65ccd25 |
| p2b-prepare-c1 | 1442 | c2e8bee41daeccfe2c83c6d10adb8faae930aaaa01320ab729f85adae87d64fd | c5981de520324999eeb00e87ab2e093ce467671505f11f55fe0d713f78b3776c |
| p2b-prepare | 1442 | 49f2e30e864cfcb8b5289b7f57b4ee103ea0bb05c6fb911f1b18cd3709cd46d6 | 9ef1d23b548f11477971f0c1c37ec63f7a9108cb61f07f4a0e4e12ec0c4a8ea8 |
| p2a | 1454 | 985c8c9159f4778ce36b656448fa2ce050b70d7f4dc8d8aa4acbeedef1b74f4e | 2cb78d0a45526e60a4435e7becf5f1c03a9ad8aafe04fec5cf392b3ae8ea197a |

## 4. Spec：实现核对与 CA01～18

角色与阵容只有 M03 一份权威，实例初始槽与当前三槽、历史入场槽分离；M04 全局 T−ΣL−ΣR 与逐角色负载联动。联合 H02 先在本地候选推进所选恢复并检查全部成员，所有准入通过后才取得 ID/48 字节熵；任何失败均不发布半份恢复。原运行时先查询原操作，再调用 builder，pending 保存冻结候选；Retry/Resolve/ResumeObserved 续办原票据，避免重采时间、ID、随机数。

战斗和敌阶段按 CharacterId/ClassId/槽及完整 PRD、贡献引用核验，未以数组首元素冒充操作者。逐人 XP 复用原评分与入场等级，材料/首通仅一次；未参战者不获本轮 XP/End/恢复。Restart 使用原参与者、位置、Stats/HP 与三域 initState/initSequence，后来恢复者只可参加正常新入场。

六切片严格保留 schema1 和 all-2；新正式只允许 [3,3,3,3,2,2]，FMBIZ001/M06/M07 保持，精确十合同与两 feature 明确。FMINT003 仅 kind1/10/11/12，格式与 tag 显式核验，旧操作保留自己的字节格式；集合长度预算、规范排序、Unicode、深冻结、重编码和引用校验仍在。M12、FMAP01 与物理存储实现不变。

显式迁移要求已确认 Active、验证 Ready 的 all-2 空闲头，无活动/S17/内存或磁盘未决；冻结 head/generation/源 descriptor 长度与 SHA。可逆单元素投影核五业务 owner 原事实与领域修订，只有 M02 generation 和迁移记录递进；旧物理前代仍按自身版本与精确包验证。F2 record1/intent2、record2/intent3 严格分派，原 generation1/index0/record0 初始化锚独立于最新阵容头；两个格式均绑定同一 298 字节原配方。

以下逐项为 R 核对结果。源码引用指向断言起点；完整参数实例、testId/fullname 对应见已绑定 case-evidence。R 将每条映射与最终 XML 实际 Passed 节点和测试方法位置核对，覆盖全部 78 个新增实例，无未映射新增项；同一实例可覆盖多项 CA。

| CA | 结果 | 独立核对要点与断言位置 |
| --- | --- | --- |
| CA01 | PASS | [真实 W 会话:18](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterSessionTests.cs:18) 经槽0/1/2、回读与入场保留实例；全空合法。重复 ID 在 Prepare 拒绝；未知 ID 在可信 Submit 拒绝，无 pending/head/字节变化。[Roster:97](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRosterTests.cs:97) 核错 Class/重复/未知引用与不可变性。 |
| CA02 | PASS | [Roster:218](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRosterTests.cs:218) 与 [Migration:75](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterMigrationTests.cs:75) 核恢复者移槽/取消、恢复期及负载不变；查询不写、不自动补人。 |
| CA03 | PASS | [Roster:56](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRosterTests.cs:56) 与 [合法隔离构造](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterTestData.cs:46) 从不同角色/职业且均为 Warrior 的合法定义初始化、v3 规范往返、调用同一内部 builder；无反射伪造成功。 |
| CA04 | PASS | [Roster:160](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRosterTests.cs:160) 核 T=120、分别冻结99后 F=21、逐角色偏好与所有 End 类型，未参与者负载保持；[Battle:142](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRosterBattleTests.cs:142) 核非空 carry 整体拒绝。 |
| CA05 | PASS | [Battle:15](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRosterBattleTests.cs:15) 核实际三成员的操作者与前排分离、伤害/倒下/跨行动 PRD/贡献；[Battle:121](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRosterBattleTests.cs:121) 六类缺失/重复/乱序/外来引用拒绝。 |
| CA06 | PASS | [Battle:41](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRosterBattleTests.cs:41) 实际整关至逐人成绩/XP/结束收据；同 EndId 按角色索引，材料及首通一次，未参与 D 无收益，重复 H06 新增为零。 |
| CA07 | PASS | [Battle:83](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRosterBattleTests.cs:83)、Roster:218、Migration:75 核同时间样本的恢复与入场联合候选；未到期者留槽、Ready 者可进，全不 Ready 不发布恢复。 |
| CA08 | PASS | [Session:103](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterSessionTests.cs:103)、[131](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterSessionTests.cs:131)、[162](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterSessionTests.cs:162) 共9例覆盖 Prepare/Write/Complete、未知和对象重建；冻结 intent/candidate/op/head、修改调用方时间不影响重试，原候选一次生效。 |
| CA09 | PASS | [Session:57](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterSessionTests.cs:57) 核冻结修订、stale head/库存、原操作优先、同 ID 异意图冲突、Unchanged 收据及活动/S17门；CA08/13 与原旧例覆盖 pending/未确认 F2，新阵容不回改冻结历史。 |
| CA10 | PASS | Battle:83 核原参与者/槽/Stats/HP 和三域初态全等；后来恢复 A 不加入重开，正常新入场重新取熵。 |
| CA11 | PASS | [Migration:19](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterMigrationTests.cs:19)、[75](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterMigrationTests.cs:75)、[173](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterMigrationTests.cs:173) 核旧 idle/活动及退出/重开/回退/胜利路线、恢复与旧未知 H02 先 all-2 续办，再显式迁移；[119](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterMigrationTests.cs:119) 可逆投影核原事实/索引/修订、旧包根与 Lookup。 |
| CA12 | PASS | [Migration:132](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterMigrationTests.cs:132) 四故障点核原技术操作/commit/候选 bytes，Retry、重建 Resume 与 Resolve 仅一次迁移，不清档/回退。 |
| CA13 | PASS | [Migration:197](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterMigrationTests.cs:197) 两格式×七 F2 故障点共14例，原 Initialize 字节及 generation1/index0 锚保持；Active 后最新 v3 阵容与建档锚分离。 [257](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterMigrationTests.cs:257) 两例拒绝 magic/schema/intent 混配；原配方身份不变。 |
| CA14 | PASS | [Roster:111](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRosterTests.cs:111)、[127](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRosterTests.cs:127)、[Migration:279](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterMigrationTests.cs:279) 及身份负例覆盖六切片往返、低预算/非法 Unicode/未知版本/schema/feature、漏根/合同/包、错 Player/绑定与集合引用；拒绝前后实物保持。 |
| CA15 | PASS | [真实首包装配](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterTestData.cs:24) 与 [隔离多角色装配](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterTestData.cs:46) 分开核实；隔离闭包 CommitEligible=false，只用同一内部纯 builder，不冒充 M10 正式发表或赠角色。 |
| CA16 | PASS | 原3655具名实例多重集合及原断言字节保持；[Roster:15](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRosterTests.cs:15) 两例核 §336 三方法、旧公开属性/字段形状、只读数据、legacy null 和多元素 AmbiguousCharacter；旧规范字节/拒绝用例全通过。 |
| CA17 | PASS | R 核串行11次运行及最后010/011原始日志/XML、同版744实现与36DLL/PDB；37旧差异、所有新文件预算、11自然meta及失败保留见第3/5节。 |
| CA18 | PASS | Battle:142 核缺系数/非空 carry 在取 ID/熵与发布恢复前拒绝；[Session:195](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerRosterSessionTests.cs:195) 核任意公开 builder 无正式写权限且回调零执行、十精确合同与两 feature。无第二份业务权威。 |

§336 因果已独立交叉核实：旧 [DemoContentTestData 的反射序列化](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentTestData.cs:153) 枚举公开属性/字段；run005 原始回放失败在字符2129545出现新增 FormationRevision，长串长度12529215与12528595不等。最终改为 Begin.GetParticipants/GetFormationRevision 和 EntryCheck.GetParticipants，内部返回原只读数据，旧公开形状不变。六根原证据和旧断言未改，新增形状断言及原回放最终通过；没有通过重写首包或断言消除差异。

run007 的旧 RecoveryResults 空结果解码 null 兼容已恢复；其余新用例修正与 run009 最后一个未知 ID 拒绝阶段断言均已审实际实现。未知角色允许准备冻结意图、在可信 Submit 以 InconsistentBinding 拒绝且无提交，符合 CA01 “拒绝且无提交”，未放宽成功条件。

## 5. 原始运行证据与同源性

RUN（原 C 已运行，R 只读检查）— 在工程根执行的固定工具命令如下；每次 run.json 保留实际 Unity 完整参数、PID 和时刻。Tests 无 filter、无 -quit，Unity 路径为 D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe。

```powershell
& 'D:/Unity/UnityProj/FightMatch/Tools/Invoke-FM025P2Validation.ps1' -Stage CONT-A -Mode Compile -ExpectedScriptSha256 aa3ad8bc3f052608622c4cb4b7cb8e5f96ede3c67eb498f78a6766e9fbaadae1
& 'D:/Unity/UnityProj/FightMatch/Tools/Invoke-FM025P2Validation.ps1' -Stage CONT-A -Mode Tests -ExpectedScriptSha256 aa3ad8bc3f052608622c4cb4b7cb8e5f96ede3c67eb498f78a6766e9fbaadae1
```

| Run | PID | 类型 | UTC 开始→结束（2026-09-25） | exit | R 核实结果 |
| --- | ---: | --- | --- | ---: | --- |
| 001 | 6880 | Compile | 08:21:20.3735220→08:22:20.6896093 | 199 | 许可 IPC 失败保留 |
| 002 | 32196 | Compile | 08:23:17.3108774→08:23:30.1260434 | 1 | 导入/新增编译错误保留 |
| 003 | 37288 | Compile | 08:24:42.7568106→08:24:46.7141882 | 1 | 新增编译错误保留 |
| 004 | 26140 | Compile | 08:26:45.2331512→08:27:13.9797447 | 0 | 编译通过 |
| 005 | 30664 | Tests | 08:27:57.0022424→08:46:36.1402447 | 2 | 3724例，3618通过/106失败（56旧、50新）/0其他 |
| 006 | 32160 | Compile | 09:04:08.4278947→09:04:22.7536952 | 0 | 编译通过 |
| 007 | 21544 | Tests | 09:04:58.8350132→10:07:06.6655777 | 2 | 3733例，3730通过/3失败（1旧、2新）/0其他 |
| 008 | 19832 | Compile | 10:09:54.9384102→10:10:06.3332672 | 0 | 编译通过 |
| 009 | 42444 | Tests | 10:10:58.9451264→11:12:03.3096390 | 2 | 3733例，3732通过/1新增失败/0其他，原3655全通过 |
| 010 | 4428 | Compile | 11:15:19.4721474→11:15:29.9279126 | 0 | 最终编译通过，无 compiler error |
| 011 | 42760 | Tests | 11:16:20.2112777→12:16:20.2984701 | 0 | 最终3733/3733通过，0失败/0跳过 |

R 直接解析各次原 XML，不仅采信 result.json；全部 run 的 before/after 进程记录与时序无并发冲突，工具身份一致，所有失败均保留。最终编译日志 62555 字节、SHA256=5b8864fa4574f24570dbd1ec468d8ff0c3d4e2dcda87d3516ffe5a9825237e5e；另行检索 error CS/Compilation failed/Unhandled Exception 无命中。

2026-09-25T12:29:24Z 独立核实 run010 after = run011 before = run011 after = 交付实物的 744 实现和 36 DLL/PDB 长度/SHA；36 路径精确等于批准数组，仅取元数据、未执行 DLL。全量通过后未重跑 Unity。R 检查最终3733 test-case节点全部 Passed，其中3728不同 fullname；原3655实例/3650不同 fullname 按重名次数保留，多重集合 SHA256=6ab79d7846638672b60b2913c9c1ed3540228436cdf683e3401847b31e303d24，新增78例。

## 6. 审查边界与交付

本结论覆盖 §333 与 §336 的 CONT-A 源码、接口、格式、迁移、F2、范围和规定验证；未发现待纠正的实质问题。修改可按本包38份原文及22个新资产的准确集合回滚，本轮未执行回滚或其他 Git 写入。

多角色能力由合法纯定义和同一内部 builder 隔离证明；正式 PlayerSession 仍只消费批准首包 W。故障注入与对象重建是内存协议证据，没有新增物理 Player probe/persistentDataPath 写入，也不作为 Windows 崩溃/断电/Android 证明。合法 AwaitLinks 公开见证、物理鼠标触控/像素、交互 PlayMode、Player/Android 构建、未覆盖故障和 §184 原未验证边界保持。

025整体/B17及027已接收成果不重开。本文不更新三协调稿或接收计数；收件前仍为34功能/29已接收/余5/51计划交付，SD00在本轮正式答复及准确自然完成后登记CONT-A接收。CONT-B、CONT-C、028、029仍需另包，不据本次结论宣告完整Demo。
