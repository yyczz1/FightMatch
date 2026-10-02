# HOST-SETTINGS-FIRST-CREATE-001：同根偏好与首次建档

2026-10-03 · `READY_FOR_SOURCE_DISPATCH`；本轮仅设计，不是源码、测试、审查或 Demo 接收。

## 1. 责任与固定输入

用户继续首个 Demo＋AGENTS §6 站立授权；签发主程/唯一收件者 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，委托 turn `01a0fde0-d533-7b00-ad78-34111f776dda`。设计 SYS `01a0f2e6-29bb-7092-abf0-705b41b7bf93/local`，实际 turn `01a0fe02-012d-7d33-9c92-4b498c68c111`。按中央最新批准，由主程派发时在 before 绑定唯一实际独立源码 owner 的 thread/host/turn，显式 `gpt-6-astra/xhigh`；只在新 S 八叶并行实施、无 Unity，不等待或占用 C 的 M03 T/RES-A 串行槽，不唤醒已停用旧 C；现任唯一 Unity C 不执行本源码包。
产品基线采用 PR3 head `6818405b859d71bf8c0f116299e7e9605a3d769a`；下面六文件已逐项核对该对象/当前文件。`H/`＝`Assets/Scripts/FightMatch/Host/`，`T/`＝`Assets/Tests/EditMode/FightMatchHost/`（只用于本表及后文简称）。

| 输入 | SHA256 |
| --- | --- |
| H/FightMatchHostSession.cs（18579B） | `892b9d494286f18e41e6f6bfbe474df591d9c06fa19644bec36332391cf50859` |
| T/FightMatchHostProfileTests.cs（9648B） | `6ef83e3ca2fa3226b48e0b33c6707726f9f311249845d88da2030727ba553958` |
| T/FightMatchHostTestFixture.cs（只读） | `b38559e449bb9f87c0f4b2159196000894b1b0a2893be2a1c66fadb609623733` |
| H/FileLocalePreferenceStore.cs（只读） | `d392d6808ad1fc8156482020951404eb46390d462ce0164404180dea067cce13` |
| H/ILocalePreferenceStore.cs（只读） | `e768e6e39fe0f28ec292032ff9986f79606136ad6473b43fb6d5c550487e55a0` |
| T/LocalePreferenceStoreTests.cs（只读） | `ef47caf6bbf7b7715f625bc8bd384e82f1dda064b7a9f048c282fb86ac46ea8d` |

PR5 head 由委托固定为 `a8b9758ed2db1a43dac9328f162433a4523624c9`，本机无该 Git 对象，未 fetch；本轮实际检查其已冻结暂存 `TestArtifacts/FightMatch/HOST-SAVE-ISOLATION-001/S/source/Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs`（19136B／`f527b4ceecffff0718ed1214f9d26eaca5d65f6e2c82ba973d6b8774c2e66b7f`），与[发布 manifest](../../versioning/2026-10-03-host-save-isolation-pr/publication-manifest.json)（`47cf565b5324e8dc4141fd3acdd8c89675564db25c6325dd3b009beff5bd95fb`）一致；不把共享旧 Host 冒称 PR5，也不宣称本轮验证了远端 head。
设计依据：[隔离设计](engineering-host-save-isolation-001.md) §3.9（`b40d64ae3f955ceca285031ef5bf3caf186fb3e313b012779b430fc4cb4b8f82`）、[H](engineering-runtime-localization-integration-001.md)（`8c0e793f52d8fce9aa39f2e281507e3408613484f8d09d6938cc48a1502ce56a`）、[RES-CD](engineering-resource-runtime-bridge-cd-001.md)（`ea3fc6efecd6ea118675cfcf96499cb5c3192bb6af2371d7ca9637cb7bdf9378`）；前者明确把本冲突留给后续，三者均未给出已批准的实现，本文只补这个独立片段。

## 2. 最小行为合同

- 原因：`EmptyRoot()` 仅承认 `locator/profiles`，语言 Save 先创建的 `settings` 会使尚无玩家档的根成为 `UnclaimedData`。只在该私有目录准入判定增加明确的偏好所有权分支；不改变 Player/Application/Platform。
- 唯一根保持 `P = PR5.EffectivePersistentDataPath`：偏好用既有显式根构造器接收 P，Session 用 `P/FightMatch`；本包不改 Host 装配、不创建第二根，也不以移动/删除 settings 恢复“空”。
- 仅允许产品根下精确小写 `settings` 目录为空，或直接包含 `locale-preference-v1.json`、`locale-preference-v1.json.<32个[0-9a-f]>.tmp`、同式 `.bak` 非目录叶；名称逐字 ordinal 匹配，不用宽泛扩展名或整个 settings 豁免。每层复用 `NoLinks`，拒绝链接（含悬空）、ReparsePoint、子目录及其他名字；不递归跟随或解析叶内容。
- 这是偏好文件命名空间准入，不是 JSON/玩家档有效性判断：已知偏好叶即使空、坏 UTF-8/JSON或失败残留也原字节保留；加载继续由 B-PREF 返回既有 fallback/诊断，不在 Session 修复、删除、采用 tmp/bak、改语言或把偏好失败转成 PlayerSave 恢复。未知/损坏 locator/profile 数据仍按原门处理，绝不据“有 settings”跳过玩家数据检查。
- `settings` 是文件、含未知叶/嵌套目录或畸形残留名时拒绝首建；枚举/属性/链接检查异常沿既有 `StorageUnavailable`，普通未知结构沿 `UnclaimedData`。拒绝后零新身份、零新存档写入，已有字节/目录保留；不新增状态码、异常吞并或权限修复。
- 保留 `ObserveStartup`、显式 `CreateProfile` 及 locator 仍 Absent 时 `ContinueCreation` 的现有重复检查点；原 `locator/writer.lock` 零长例外、空 profiles 规则、original identity、持久 create-intent/active 恢复及重入语义均不变。只增加私有小 helper，不引入接口、服务、public API或对偏好 parser 的依赖。
- 可独立于 LOC-A/M16、YooAsset、RES-C/D/H 实施与验证；不会让空文本源的真实 Host 可玩。RES-D 所有的 `resource-state` **不在本包放行范围**，完整 H 必须另行闭合其与首次建档的目录所有权兼容，不能把本文当作任意技术目录白名单或完整 H 接收。

## 3. 源码白名单与保护

实施只新建普通暂存根 `TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/S/`（本轮已核 ABSENT），不是 Unity 工程/worktree；恰好以下八叶，已有目录/文件或输入漂移即回主程，不覆盖或回滚：

1. `source/Assets/Scripts/FightMatch/Host/FightMatchHostSession.cs`：固定前像＋§2私有判定，增删总计≤80行。
2. `source/Assets/Tests/EditMode/FightMatchHost/FightMatchHostProfileTests.cs`：固定前像＋§4七例及必要私有测试 helper，新增≤320行，既有断言不删不弱化。
3. `before.json`、`after.json`：实际独立源码 owner thread/host/turn、issuer/authority、上述固定输入/设计/PR5暂存依赖的类型/bytes/SHA、两源码前后像及共享两源码/meta不变证明；before create-once，包含两前像供内存 patch 重放。
4. `source.patch`、`verify.py`、`static-checks.json`、`source-receipt.json`：仅两逻辑产品路径补丁、离线机械检查脚本、原始检查结果与封存回执；总暂存≤1MiB。作者文件用 apply_patch，脚本仅标准库生成本八叶中的机械证据，禁止子进程/网络/编译/缓存。

逻辑产品只改上述两个**已有** C#，无新产品文件/meta。其余共享 Assets/Tests/fixture、Host装配、B-PREF、Application/Platform、asmdef、资源/场景/Prefab/meta、Packages/ProjectSettings、Config/Tools/Generated、M03/RES-A/PR4/FIX04与既有证据全部禁写；不全工程扫描/复制，不读真实玩家目录，不改 Git/分支/提交/PR，不解除已有拒绝或通过换路径/执行者重试。

## 4. 观察准则、最小验证与回执

在现有 `FightMatch.Host.Tests.FightMatchHostProfileTests` 增加以下七个非参数化 `[Test]`（多情形用固定小循环）；复用 HostRig，仅其测试拥有根，显式偏好根取 `Path.GetDirectoryName(rig.Root)`，不调用默认 persistentDataPath 构造器：

- `H03_SettingsOnlyAllowsExplicitFirstCreate`：分别经真实 FileLocalePreferenceStore 保存 EN/zh-Hans；Observe 可首建但不自动建档，显式建档一次，原偏好字节/Load值不变，重复 Create 不新初始化。
- `H03_OwnedPreferenceResiduesPreserveBytes`：空 settings、坏目标、仅合法 tmp/bak、目标与合法残留并存均可首建；目标和残留全部不变，Load维持 Missing/Invalid/Remembered 对应语义，不采纳残留。
- `H03_UnknownSettingsOrPlayerDataStillBlock`：未知文件、目标同名目录、嵌套目录、残留32位长度/大小写/字符/后缀错误各拒绝；合法 settings 与根未知文件、孤立 profiles、未知 locator 锁并存仍拒绝，零新身份/工厂调用，全叶/目录保留。
- `H03_SettingsLinksNeverAuthorizeCreation`：settings 自身及目标/tmp/bak 叶的实链和悬空链均拒绝，外部测试哨兵不变，零建档；复用既有 Mac symlink 测试方式，不改生产 NoLinks。
- `H03_SettingsAreRecheckedBeforeCreation`：空根提示后添加未知 settings 叶，Create 拒绝；另在准备身份后/storage factory首次失败且 locator仍Absent时添加未知叶，Continue拒绝，原身份与所有旧字节保留。
- `H03_SettingsSurviveInterruptedCreation`：合法偏好＋残留，分别注入现有 create-intent 写后与 active 发布故障；冷重建继续同一 record/player，已初始化分支 commit不变，不重复初始化，所有偏好叶原字节不变。
- `H03_ActiveProfileWithSettingsReopensUnchanged`：建档后重建/重复Observe，player/commit不变、不开放首建，所有玩家/偏好叶不变；再令偏好目标无效，Load按既有策略降级但 Active玩家仍可复开、不重新初始化或改写坏偏好。

最小回归仅现有七方法：`H03_AbsentLocatorDoesNotAuthorizeCreationOverExistingPhysicalData`、`H03_ProductSubtreeLinksIncludingDanglingAreBlocked`、`H03_FreshCreateRechecksTheRootAfterTheEmptyScreenWasShown`、`H03_ReadFailureNeverFallsBackToAbsentOrANewPlayer`、`H03_PreparedIdentityIsRetainedBeforeTheStorageFactoryCanFail`、`H03_F2ColdReconstructionContinuesOriginalCreationAtBothDurableBoundaries`、`H03_ActiveReopenAndRepeatedObserveDoNotInitializeAgain`；依据上表固定 ProfileTests 的 `[TestCase]/[Test]` 静态逐项计数，依次3＋2＋1＋1＋1＋2＋1＝11叶，加新7个非参数化Test推得18叶，非 discovery 结果；不包含 UI/迁移/全量套件，不重跑已未变 B-PREF。
源码阶段仅 `python3 -B TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/S/verify.py` 与只读 shell/hash：核八叶/预算、输入及共享保护、两路径 patch内存重放、七准确名字和旧断言保留；记录真实 UTC/命令/exit，脚本单次≤20秒，只为修脚本有限重试并保留失败。静态检查不证明 C# 编译或行为。
后续由主程/主测试一次绑定实际候选head、PR5发布字节、18叶准确 fullname、专属根/输出和资源限额；支持者优先dots，Mac NoLinks/物理盘验证仍由主程登记的现任唯一 Unity C 按定向Mac合同执行，独立源码 owner 不执行。至多一次编译及一次定向18叶运行，失败按证据缩小修复，不在源码包启动 Unity/测试/discovery/设备/G3/M16/FIX04。GitHub PR Code Review 对实际新head独立进行；不得由作者自批或加本地review。
独立源码 owner 封存后 final 只交主程：实际thread/turn、八叶SHA、两后像/patch hash、各准则和真实限制；主程用wait收件，无重复回消息/中央或QA转发。失败报精确BLOCKED，不改用户决策；就本文范围无需新增产品选择。源码回执只能 `SOURCE_READY / UNCOMPILED / UNTESTED / REVIEW_PENDING` 或 `BLOCKED`；测试、审查、真实Host及Demo接受各自留门。本轮只新增本文，仅作文件/hash机械核对，未派工、改产品、运行Unity/测试或发起审查。
