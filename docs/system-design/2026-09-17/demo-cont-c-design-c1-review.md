# CONT-C-DESIGN-C1 独立复核

VERDICT: ACCEPT

Scope: PASS。Acceptance criteria: PASS（§364 的静态设计纠正验收）。原报告唯一 R1 已闭合；本结论接收 C1 设计，不表示 CONT-C 代码、未来测试或完整 Demo 已完成。

## 1. 准确完成门与报告身份

- 授权及范围：system-task-packets.md §364～365，仅复核原 R1、两稿一致性、输入保护和必要派生索引；R 只新写本报告，≤200 物理行。
- 原 C 任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；准确 C1 turn：`01a0e36f-c41b-7dd2-8c61-bc702a903506`。
- 已核 C 原生上下文 `2026-09-27T15:15:48.784Z` 为 `gpt-6-astra / max`、工程 `D:/Unity/UnityProj/FightMatch`；原生 `task_complete` 为 `2026-09-27T15:24:12.983Z`，正式最终消息为 `DESIGN_COMPLETED`。任务快照同一 turn 为 `completed / idle / error=null`。
- C 正式消息 ID：`msg_033d5e20bc77ad65016ab93516434c87d096d51dbe7657626c`。本复核在该准确完成及正式交付门之后进行，没有把作者进度或自评当作独立接收。
- 原 R 任务：`01a0c1cd-dce1-7ac3-8780-06163cb0acfc`；本报告准确新 turn：`01a0e379-0f90-7e82-b6aa-7dfa10ca24be`。原生上下文 `2026-09-27T15:25:59.652Z` 已核为 `gpt-6-astra / max`、正确工程根。

两份正式 C1 实物均独立重算身份，与 C 最终消息及 §365 相符：

| 实物 | bytes / 物理行 | SHA256 |
| --- | ---: | --- |
| [demo-cont-c-design-c1.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-c-design-c1.md) | 51912 / 240 | `ca8d9d1a50ed2590cf09de69368642e7d900e6374b37324820dc02b4af30d182` |
| [demo-cont-c-design-c1-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-c-design-c1-scope.json) | 270091 / 4542 | `dd9a2557dff4db1bb652877f1f643201d1515c311f9f62ca106abf85f20d6cb7` |

## 2. R1 纠正核对

| 要求 | 独立证据与判断 |
| --- | --- |
| 查询角色与事务字段分开 | [C1 第 45 行及 §2.1](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-c-design-c1.md:45) 明确 `NavigationContext.SelectedCharacterId` 用于原 QueryPermanent、展示、返回锚和失效检查。Craft 实际 Draft.CharacterId=null，报价 CharacterId/ClassId 均为空；Equip/SetPreference 继续使用明确目标角色。JSON [operationCharacterMapping](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-c-design-c1-scope.json:4035) 给出相同映射。PASS。 |
| 与现有字段契约一致 | 原 PlayerPermanentDraft.Copy/PreviewPermanent 契约和文件身份沿前次审查保持。本轮按 §364 只核必要 [CandidatePermanentProtocol.Preview](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidatePermanentProtocol.cs:25) 的角色解析、Craft/Equip/Preference 分支、原报价生成，以及 [Shape](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidatePermanentProtocol.cs:231) 的 hasCharacter 判别：Craft 不含事务角色，Equip/SetPreference 要求角色；与 C1 一致。PASS。 |
| 实际生产 Draft 输入可追踪 | [C1 第 55 行](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-c-design-c1.md:55) 固定 view→controller→session.Preview→共用 BuildPermanentDraft→原 PreviewPermanent→同一 Draft.Copy→原协议的调用链。构造器仅在原拟议 PlayerNavigationSession.cs 的 350 行预算内，不增文件/公开 API，不要求旧 Copy/Core 擦除错误角色，不另写测试专用构造器。PASS。 |
| CC09/CC11 不再绕过输入 | [CC09](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-c-design-c1.md:195) 与 [CC11](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-c-design-c1.md:197) 均从非空查询/返回角色走同一生产构造路径，再将该实际 Draft.Copy 送原合法隔离核，核空角色、合法形状、原来源及固定产出，之后才测确认/结果转移。明确禁止只手填正确报价后测 AcceptPrepared/Receive。JSON 两项逐字一致。PASS。 |
| 真实首包与信任边界 | 原实际 PlayerSession 的 NoPublishedDefinition 单列；不向正式会话注入配方，不增加友元或信任后门。现有 ExpectedCommitId、RecipeId→DefinitionId、Quantity、SelectedInputs、偏好及来源/优先规则保留。PASS。 |

没有剩余设计阻断项。本轮检查的是可执行契约及未来验收要求；BuildPermanentDraft 和上述测试尚未实现或执行。

## 3. 范围、保护与派生索引

独立对比原稿与 C1 的 Markdown/JSON 增量：业务变化仅 R1 字段映射、对应生产输入路径及 CC09/CC11；其余变化为本次准确回合、交接身份、静态核对和文档保护索引。其他 28 项验收不变，既有状态机/恢复/业务设计沿前次独立审查保全。

- 49 项受保护输入为原 45 项＋原三份正式交接稿＋1 项必要 Core 字段契约；before=after=当前身份。原三稿仍为：design 46997 bytes／`d82a00ac61b8925be6569264bcd5f23086655d375cf9b2e45a4a94d42bad5ecb`；scope 255111 bytes／`a5fe61500e91c7ab9fef11fdd28f79c09a8b910e4f63cc4256ba2047f25af561`；[原 R 报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-c-design-review.md) 11161 bytes／`c70151dff4a527f248ea502c80de57f1a85ef2a973fd32351d04ea2577d850cf`。
- 必要字段契约文件仍为 20871 bytes／`98d7738bc8c2e5c8f34342741a006b622edcf803e91a176bcfd73d8071255033`。未修改或重新审查其他历史 Core 实现。
- 三协调稿沿原设计结束、C1 before/after 分列；当前 SD00 独立续接更新与 C1 after 的差异没有混入 immutable 输入或被当成 C 的改动。
- C1 两稿分别满足 ≤260 行和 ≤327680 bytes。未来 8 生产 cs＋5 测试 cs＋13 meta 的路径、理由及逐文件预算保持；生产 1860/测试 1670/meta 156 行、802 实现/832 Assets/438 GUID/36 DLL-PDB 保持。
- 原 create、modifyExisting、evidence、futureImplementationPaths、futureAssetPaths 与原 scope 逐项一致。旧工具 delta36/final318/cumulative230 仍是待另签的准确例外；8 槽、141 证据路径未变化，未来证据根及源码未预创建。
- 未来导出集合准确为原 918 项加 C1 design、C1 scope、C1 review 三份文档，共 921 个唯一项；原项无删除，没有额外生产文件。旧 acceptedBaseline 和 notVerified 完整不变。
- CC01～CC30 的编号/状态保持，全部为 `NOT_RUN_DESIGN_ONLY`；仅 CC09/CC11 的 evidence/acceptance 改动，全部 30 条验收在 Markdown 与 JSON 一致。

## 4. 验证与后续边界

RUN — 只读 PowerShell/内存 Python 静态差分、SHA256、集合和字段一致性核对，exit 0；主审计时间 `2026-09-27T15:28:16.104218+00:00`，49 输入、原三稿、921 派生集合、未变白名单/预算、30 项状态与两稿一致性无失败。原生准确 turn/model/cwd、task_complete、formal final 与实际文件身份均已独立核实。

NOT RUN — Unity、产品 DLL、Compile/EditMode 或 Player/Android。本包只授权设计纠正复核；旧 3872/3872 通过沿已接收 CONT-B-CODE-C1 证据复用，未重复运行，也不计为 CONT-C 的通过证据。

R 本轮仅新写本报告，没有改输入、源码、测试、工具、资产/meta、设置、内容或旧证据，没有新建任务/代理、Git 动作或自动跟进。无需再次纠正 R1；SD00 可按 §364.3 另签精确 CONT-C 实现和工具预算例外，本报告本身不授源码写权。

34 功能／31 已接收／余 3（CONT-C、028、029）／51 正向交付保持。真实已发布配方→正式 PlayerSave 制作整链、物理鼠标/触控/像素、交互式 PlayMode、Player/Android、公开 AwaitLinks、未覆盖强保存故障及 §184 原 NOT VERIFIED 边界不变；同一 Android 产品路线保持。
