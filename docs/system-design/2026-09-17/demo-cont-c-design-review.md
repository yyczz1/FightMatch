# CONT-C 独立设计审查

VERDICT: NEEDS_FIX

本轮设计整体可沿用，须先闭合一处 Craft Draft 与页面角色上下文的接口契约。此结论只针对 §360 的设计交付；不是 CONT-C 实现或完整 Demo 的接收。

## 1. 准确回合与正式交付门

- 原 C 任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；准确设计 turn：`01a0e2b4-870f-7f21-ac5e-48b253638bea`。
- 已独立核原生 `turn_context` 为 `gpt-6-astra / max`、工程 `D:/Unity/UnityProj/FightMatch`；原生 `task_complete` 为 `2026-09-27T12:16:59.981Z`，正式最终消息为 `DESIGN_COMPLETED`。紧凑任务快照同样给出该 turn `completed / error=null`；任务容器 `notLoaded` 不改变已完成事实。
- C 正式最终消息 ID：`msg_0205740344f2cc52016ab90935574c87d0bcb64b9f1f5f4c1a`。本审查没有把异步进度、旧等待回合或 C 的自评当作独立结论。
- 原 R 任务：`01a0c1cd-dce1-7ac3-8780-06163cb0acfc`；本报告准确 turn：`01a0e35d-3ef5-7f82-8899-854646c131ba`。原生上下文 `2026-09-27T14:55:39.941Z` 已核为 `gpt-6-astra / max`、正确工程根。设计稿第 6 行记载的是先前等待 turn，本报告以本准确续接 turn 为身份。
- 写权限依据 §360 及 §362 的报告输出澄清：R 仅新写本文件，设计输入、实现、测试、工具和既有证据保持只读。

正式输入已对实际文件重算长度及 SHA256，与 C 最终交付及 §361 相同：

| 实物 | bytes | SHA256 |
| --- | ---: | --- |
| [demo-cont-c-design.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-c-design.md) | 46997 | `d82a00ac61b8925be6569264bcd5f23086655d375cf9b2e45a4a94d42bad5ecb` |
| [demo-cont-c-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-c-scope.json) | 255111 | `a5fe61500e91c7ab9fef11fdd28f79c09a8b910e4f63cc4256ba2047f25af561` |

## 2. 唯一阻断项 R1：分开 Craft 的查询角色与事务字段

优先级：P2。定位：[设计第 43 行](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-c-design.md:43)，结合第 27、39、167、179、181 行；[scope 的 previewKinds](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-c-scope.json:3830) 及 CC09/CC11。

第 43 行要求“制作沿既有 Draft 也保留明确的上下文角色”，并以未选择角色时 `ActorSelectionRequired` 约束制作。第 27、39 行又规定直接消费既有 `PlayerPermanentDraft`。两份稿件均未明确：页面选中角色只用于查询与返回上下文，还是也写入该 Draft 的 `CharacterId`。后一解释与已接收的永久事务契约冲突，不能留给实现者自行决定。

实际接口证据：

1. [QueryPermanent](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/PlayerPermanentSession.cs:9) 确实要求明确、持有的查询角色；因此页面保留角色用于该查询是成立的。
2. [PlayerPermanentDraft.Copy](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/PlayerPermanentModels.cs:21) 无条件复制 `CharacterId`；[PreviewPermanent](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/PlayerPermanentSession.cs:43) 直接把其 Copy 传给原永久协议，不会替 Craft 清空角色。
3. [已接收 CONT-B C1 的 Draft 契约](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-b-design-c1.md:24) 将 Craft 定义为 `Craft(RecipeId,PositiveBatches,SelectedInputs)`，角色属于 Equip/SetPreference 等操作。沿用此前已审的 `CandidatePermanentProtocol.Shape`：仅 UseExperienceCards/LearnSkill/Equip/SetPreference 的报价具有 `CharacterId/ClassId`；Craft 的这两个字段必须为空。该约束所在文件当前身份仍与已接收 C1 相同：20871 bytes，SHA256 `98d7738bc8c2e5c8f34342741a006b622edcf803e91a176bcfd73d8071255033`。本轮只核该文件身份，未重做 Core 语义审查。

具体影响：页面从准备流程选中 W，再将 W 一并复制到 Craft Draft 时，即便日后存在合法配方与材料，该报价仍不满足原 `Permanent.Character` 形状约束，不能完成所设计的正常制作链。当前真实首包先返回 `NoPublishedDefinition`；CC09/CC11 又允许将已由隔离核构造好的报价/请求直接送入纯转移 Seam，两类证据都可能绕过这段实际 Draft 构造而仍然通过。本项是设计契约缺口；本轮没有运行尚未存在的 CONT-C 代码。

必须作以下有限纠正：

- 明确页面的 `SelectedCharacterId` 留在 NavigationContext，用于原 `QueryPermanent(characterId)`、查询展示、返回锚及其失效检查；Craft 的实际 `PlayerPermanentDraft.CharacterId=null`，不得从该上下文填入角色或制造受益人。Equip/SetPreference 则仍须传入明确的目标 `CharacterId`。其他已接收字段、数量和来源语义不变。
- 在 Markdown 与 scope 同步固定这一按操作种类区分的字段映射，消除“沿 Draft 保留角色”的两种解释；无须改变现有 Core、PlayerPermanentDraft 或 PlayerSession API。
- 将 CC09/CC11 的制作验收补到实际输入边界：使用与页面相同的生产 Draft 构造路径，保留非空查询/返回角色，同时断言 Craft 的实际 Draft/报价角色为空；将该实际 Draft 送入原隔离永久核验证合法制作形状与原来源/固定输出。不能只手工构造正确报价后验证 Receive/AcceptPrepared。真实 PlayerSession 的缺配方拒绝继续单独保留，不能注入正式配方或另开信任后门。

## 3. 其余审查结果

以下结论仅适用于设计，不宣称未来实现或验收已经通过：

| 审查项 | 独立核对结果 |
| --- | --- |
| 状态与上下文 | N0～N10、三个 ReturnAnchor、路由最多 5 层、一个确认槽，以及 owner/head/完整绑定/角色阵容/来源/活动历史的失效规则相互对应。确认后先查原请求与结果，不因新头或余额变化重新确认。 |
| 一次确认与恢复 | 先锁 token、保留成功 Prepare 的原 request 再 Submit；Back/Cancel 不等于 End；无票据 PendingPreparation 走原 Retry；Unknown 沿原 Resolve；Observed 精确选 commit；Ended 后继续检查恢复门。未发现将通知失败当保存失败的设计路径。 |
| 对象重建 | 页面重建保留同一导航 session、请求和返回锚，旧 UI epoch 拒绝回调；整个 Application 重建沿原 F2/Open/Restore/Observed，丢失内存锚时不猜原操作。无持久候选窗口及真实崩溃边界明确保留。 |
| 业务及 Presentation 边界 | 现有 PlayerSession/可信 Lifecycle/M02/M12 继续拥有真值。活动、S17、创建未确认、pending、恢复未知及明确 2→3/3→4 迁移的门保持；菜单采用兄弟层，避免旧 playback 的 Detach/Close/Dispose 触发完成上报。 |
| 同一产品连续性 | 精确发布关卡目录、单 W/L1 首包及真实空配方保持；没有 Demo 平行存档、假余额、平台业务分支或新增内容。028/029 的 HostRequest 与真实宿主、设备证据分开。 |
| 范围与预算 | 仅拟新增 8 个生产 cs、5 个测试 cs 及其 13 个自然 meta，合计 26 个新 Assets；生产 1860 行、测试 1670 行、meta 156 行的逐文件上限一致。现有工具的 delta≤36/final≤318/cumulative≤230 提案明确要求 SD00 另签例外，没有自授实现权限。 |
| 证据提案 | 未来 802 实现/832 Assets/438 GUID/36 DLL-PDB、918 导出输入与实现路径的集合关系一致。141 个有限证据路径与 8×12 槽叶、16 根叶、1 before-text、27 after-text、1 manifest 一致；文本快照使用 `.txt` 后缀。不会要求制造未产生的日志或覆盖旧失败。 |
| 验收与未验证标记 | CC01～CC30 均为 `NOT_RUN_DESIGN_ONLY`。除 R1 的输入映射缺口外，状态/失败/恢复/幂等用例和位置可追踪；真实无配方、隔离同核组合、实际回调与物理设备证据分别标识。 |

## 4. 本轮验证与证据边界

- RUN — 只读 PowerShell/内存 Python 静态核对，exit 0；审计快照 `2026-09-27T15:09:04.459648+00:00`。对两份正式实物、scope 的 45 项 immutableBefore/immutableAfter/当前身份、25 项当前 Application/Presentation/程序集输入与 C1 身份、逐文件白名单/行预算、未来精确集合、141 路径及 30 项状态逐项校验；这些机械检查无失败。
- 三协调稿由 SD00 独立更新，当前身份与 C 设计结束时不同，已单列；没有将其变化误归 C 越界。未重新遍历或声称重审全部历史源码、DLL 字节和 4897 项旧证据。
- 复用 §359 已接收证据：C1 run002 Compile exit 0，run003 全量无 filter EditMode 3872/3872、0 fail、0 skip；原 3867 个不同 fullname/3872 个多重集合条目保持为未来回归基准。run005 的 8 个红测及 run001/004 IPC 失败不抹除、不重标。
- NOT RUN — Unity Compile、EditMode、产品 DLL 或 Player/Android；§360 仅授权设计审查。旧 3872 通过不能充作 CONT-C 通过，未来 30 项验收没有在本轮执行。
- R 只新写本报告；未修改设计输入、源码、测试、工具、资产/meta、配置、旧报告或证据，未创建任务/代理、自动化，未做 Git 动作。

## 5. 最小纠正包提案：CONT-C-DESIGN-C1

此处供 SD00 按 §362 签发，不自行授权实现或启动其他任务。由原 C 修正、原 R 独立复核，保持 gpt-6-astra/max。

- 目标：仅闭合 R1 的查询角色/事务角色映射与相应未来验收，沿用其余设计。
- C 拟议新文件：`docs/system-design/2026-09-17/demo-cont-c-design-c1.md`（≤260 物理行）和 `docs/system-design/2026-09-17/demo-cont-c-design-c1-scope.json`（≤320 KiB）。保全本次原稿、scope 与本 R 报告；两份新稿除 R1、相应测试边界和准确版本/身份/保护索引外，不改设计内容。
- 未来源码仍限原 8+5 文件、13 meta 及原工具例外提案，不增 Core/PlayerSession 旧文件写权，不新增正式内容、序列格式、测试友元或公开测试注入入口。若增加 C1 交接文档的保护索引，只更新相应文档集合与计数，不扩生产范围。
- 验证：只读核实际 Copy/Preview 接口及沿用的永久种类字段约束；C1 的 Markdown/JSON/CC09/CC11 映射必须一致；明确未来测试覆盖真实生产 Draft 输入而非仅后置纯转移；原 45 输入、本次三份正式交接文件身份保持。只做静态设计核对，不运行 Unity，不重复已有绿测。
- 输出：原 C 新准确回合自然完成，正式 DESIGN_COMPLETED 并给两份 C1 实物的字节数/SHA256；原 R 再核准确完成门和最小增量。R 的 C1 报告路径/写权由 SD00 在该纠正包中明确，本次不预创建。

CONT-C 源码须设计纠正独立接收后另签。34 功能／31 已接收／余 3（CONT-C、028、029）／51 正向交付保持。物理鼠标/触控/像素、交互式 PlayMode、Player/Android、公开 AwaitLinks、未覆盖强保存故障/真实崩溃、正式配方整链及 §184 原 NOT VERIFIED 边界继续保留；五分钟自动跟进不恢复。
