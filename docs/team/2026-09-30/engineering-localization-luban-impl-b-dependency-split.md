# Localization / Luban LOC-B 依赖拆分建议

状态：`PREPARED_NOT_AUTHORIZED`

日期：2026-10-01

角色：系统设计（SD）

上游授权：中央 AI `01a0e401-511d-79f2-b47f-3ab0ade1681b` 的最新指令，以及仓库 `AGENTS.md` 已批准的五角色协作流程

来源任务：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` 对 LOC-B 的只读依赖拆分委托

本文性质：只读设计与派工建议。本文不授权产品代码、测试、资源、配置、`.meta`、程序集定义、序列化格式或证据写入；不替代原 LOC-B 实现包，也不恢复暂停的产品目标。

## 1. 结论

原 LOC-B 的 12 路径可以按真实依赖拆成两个各 6 路径的实现包：

1. `LOC-B-PREF`：locale preference v1 的接口、文件存储与测试。
2. `LOC-B-CATALOG`：catalog parser、localized text source adapter 与 catalog 测试。

推荐采用这一拆分。

关键判断如下：

- `LOC-B-PREF` 不读取 LOC-A catalog，不依赖 manifest、canonical field table、JSON 终止符或解析预算。因此它的设计包现在可以准备；其产品代码执行只等待 UGUI 独立 `ACCEPT` 与 C 串行 Unity 槽位。
- `LOC-B-CATALOG` 的模块边界、失败关闭原则、测试类别和命名规则可以先准备；但 parser、adapter、测试 builder、精确诊断表、精确测试 fullname/filter 不能在真实 LOC-A 产物被独立接受前执行或冻结。
- 两个包都不得实现资源加载、ReleaseSet 外层准入或 Host composition。这些属于 `RES-01D / Host composition` 的后续独立包。
- 本拆分绝不把“LOC-A 正在生成”“已有草稿 CSV”或“已知大概 schema”当成已接受产物。`LOC-B-CATALOG` 仍必须消费真实、已验收、可追溯的 LOC-A artifact/manifest/evidence。

## 2. 固定输入与当前基线

本建议仅基于以下固定输入版本；任一输入发生语义变化，主程必须先做漂移审查：

| 输入 | 固定版本 |
|---|---|
| `docs/team/2026-09-30/engineering-localization-luban-001.md` | SHA-256 `022c060801df22520653856c24bd78dda394deb26f946dbff1b70ec9c338380a` |
| `docs/team/2026-09-30/engineering-localization-luban-impl-b.md` | SHA-256 `7941c42e99d0552daf5241de156da415c902f1ee11eac277a2396dc82487a795` |
| `docs/system-design/2026-09-17/runtime-ui-ugui-correction.md` | 当前仓库版本；运行时 UI 必须为 uGUI |
| `docs/team/2026-09-30/engineering-ugui-01-implementation.md` | SHA-256 `ccb7ed615cf9d688f61a19620ff4619ea2c43be75c44f2820a916622552ea623` |
| `docs/team/2026-09-30/planning.md` | SHA-256 `35c51adca0eeb34155ad6e392c347e652ca24e8979f926074a229b9dbedb68f5`；含 AMEND-04 |
| `docs/team/2026-09-30/planning-localization-draft.csv` | SHA-256 `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`；只是策划输入，不是已接受 LOC-A 运行时产物 |
| `docs/team/2026-09-30/engineering-yooasset-001.md` | SHA-256 `2e12b5f24ecedef03df5795c9632318c55fe6330562731892d4c9704c54740d8`；定义 RES-01D 边界 |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTextSource.cs` | SHA-256 `0c830db5aa297a08e236e7a8edb7e42d62f3c779b1293b362733b450283901a6` |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocaleId.cs` | SHA-256 `f5338ff0fdde20a362281380a7d84183f552ad23a7964edb51d042fca48174a5` |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocalizationService.cs` | SHA-256 `ca8b2c5f9b8afd864410b35f7ab9998c04a7b5ea9efdf3a51e8082b6dd54a408` |

当前观察到的 localization 类型仍属于 UGUI WIP 输入，而不是已独立接受的稳定 API。本文据此描述预期边界，但不把其当前文件哈希提升为执行授权。

## 3. A–D 依赖分类

A–D 是四个独立门禁维度，不是必须按字母顺序执行的单一流水线。一个工作项可以同时依赖多个维度。

### A：冻结设计、AMEND-04 与当前 internal interfaces

A 包含：

- 已冻结的 localization 设计；
- `planning.md` 中 AMEND-04 的 UNKNOWN 语义；
- 当前 internal `LocaleId`、`LocalizationService`、`ILocalizedTextSource` / `LocalizedTextResult` 形状；
- 原 LOC-B 已批准的 fail-closed、bounded parsing、无隐式 fallback、无 Unity Localization/Addressables 等约束。

A 足以准备模块职责、输入输出、测试类别、诊断原则与派工包草案。A 不等于 UGUI 接口已验收，也不授权写 Unity 产品文件。

### B：已接受的 LOC-A 产物事实

B 必须来自真实 LOC-A 交付及独立验收，至少包括：

- artifact bytes 与 SHA-256；
- manifest bytes 与 SHA-256；
- 已接受的正式文本来源与输入 receipt；
- schema version 与 record count；
- locale、severity、parameter 等摘要；
- 精确 canonical field table；
- 精确 JSON 终止符约定；
- 从真实产物观测值推导的八项 parser budgets 与 margin；
- LOC-A 的独立 R `ACCEPT`。

B 不能由草稿 CSV、设计示例、预测尺寸、临时 runner 输出或未验收的生成结果代替。

### C：UGUI 独立 ACCEPT 与 C 串行 Unity 槽位

C 同时要求：

- UGUI-01 已获得独立 R `ACCEPT`；
- localization internal interfaces 的最终身份和哈希已记录；
- C 是该时段唯一 Unity 写入执行者；
- 没有与目标文件、程序集导入或 Unity 进程冲突的并发任务；
- 严格按签署后的包执行并返回 fresh evidence。

C 是产品代码、`.meta`、编译和 EditMode 测试的执行门禁。文档准备不占用 C 槽位。

### D：RES-01D / Host composition

D 包含：

- `fm.text.full` 的 raw-byte resource bridge；
- ReleaseSet 外层 DTO、codec 与准入；
- YooAsset/resource build 的定位与读取；
- Host 中的生产装配、启动顺序和生命周期；
- catalog load failure 与更外层启动/恢复策略的连接。

D 不属于 `LOC-B-PREF` 或 `LOC-B-CATALOG`。两个包只能提供内部可组合构件，不得越界模拟生产装配。

## 4. 工作项依赖矩阵

| 工作项 | A | B | C | D | 当前允许动作 | 说明 |
|---|:---:|:---:|:---:|:---:|---|---|
| B-PREF contract、v1 schema、诊断语义、测试矩阵 | ✓ |  |  |  | 可准备 | 与 catalog bytes 无关 |
| B-PREF 精确 task packet 与 6 路径白名单草案 | ✓ |  |  |  | 可准备 | 执行前仍需绑定已接受 UGUI 哈希 |
| B-PREF 产品文件与 `.meta` 创建 | ✓ |  | ✓ |  | 不可执行 | 等 UGUI `ACCEPT` 与 C 槽位 |
| B-PREF focused compile / tests / evidence | ✓ |  | ✓ |  | 不可执行 | 可在 LOC-A 仍等待时执行，但 C 未满足前不可运行 |
| Catalog 模块 seam、所有权、fail-closed 原则 | ✓ |  |  |  | 可准备 | 只冻结结构，不猜 schema 常量 |
| Catalog 通用诊断分类 | ✓ |  |  |  | 仅可准备 | 可定义“格式/预算/字段/语义”等类别 |
| Catalog 精确 diagnostic table | ✓ | ✓ | ✓ |  | 不可执行 | code/stage/retryability 必须落到真实字段和预算 |
| Catalog 测试 fullname 命名规则 | ✓ |  |  |  | 仅可准备 | 可定 suite/prefix 规则，不可声称最终叶节点 |
| Catalog 精确 fullname、leaf count、test filter | ✓ | ✓ | ✓ |  | 不可冻结 | 来自真实 builder 展开与已接受 UGUI baseline |
| Catalog canonical field mapping 与 JSON terminator | ✓ | ✓ |  |  | 不可冻结 | 必须逐字绑定已接受 LOC-A 定义 |
| Catalog 八项 budget 常量与 limit 三点测试值 | ✓ | ✓ |  |  | 不可冻结 | 必须使用 observed maxima + approved margin |
| Catalog test builder 设计 | ✓ |  |  |  | 仅可准备 | 可定 builder 责任，不可写实际字段 payload |
| Catalog test builder 产品/测试代码 | ✓ | ✓ | ✓ |  | 不可执行 | CAT-19/20 等需按真实预算展开 |
| Catalog parser 与 adapter 产品代码 | ✓ | ✓ | ✓ |  | 不可执行 | adapter 也需绑定已接受接口身份 |
| Catalog focused compile / tests / evidence | ✓ | ✓ | ✓ |  | 不可执行 | 必须使用真实接受产物作 fixture/reference |
| raw artifact load、ReleaseSet admission、Host composition | ✓ | ✓ | ✓ | ✓ | 不在本包 | 另立 RES-01D/Host composition 包 |

矩阵中的“可准备”只允许文档与任务包设计工作；它不允许提前创建白名单中的 `.cs` 或 `.meta` 文件。

## 5. 特别判断：B-PREF 是否可以独立

可以，而且应当独立。

`LOC-B-PREF` 的唯一持久化对象是用户手动语言偏好：

- 路径：`Application.persistentDataPath/FightMatch/settings/locale-preference-v1.json`
- schema：`{ "version": 1, "locale": "en|zh-Hans" }`
- 支持的 locale：`en`、`zh-Hans`
- 责任：读取已保存偏好、保存用户选择、按 AMEND-04 区分保存失败发生在 replace 前还是 replace 后；在无法证明落盘状态时返回 UNKNOWN。

它不需要知道：

- catalog 有多少条记录；
- catalog 的 canonical fields；
- catalog JSON 是否以换行或其他字节终止；
- parser 的任何字节、字符串、参数或深度预算；
- LOC-A manifest 的字段；
- YooAsset 地址或 ReleaseSet identity。

因此，LOC-A runner、material、build、license alternative 或 artifact acceptance 的等待不应阻塞 `LOC-B-PREF` 的包准备，也不应在 C 门禁满足后阻塞其实现。

但 `LOC-B-PREF` 不能仅凭 A 立即落地，原因不是 catalog 依赖，而是它引用 `LocaleId` / `LocalizationService` 的已接受语义并需要 Unity 导入、编译、EditMode 测试。若 UGUI 尚未独立 `ACCEPT`，当前 internal interfaces 仍可能漂移；若 C 无串行槽位，写入会违反既有执行纪律。

### 5.1 B-PREF 不得误报的完成范围

即使 `LOC-B-PREF` 独立 R `ACCEPT`，也只证明：

- locale preference v1 文件合同已实现；
- pre-replace failure、post-replace uncertainty、rollback proof、retry 和 session locale 行为通过已批准测试；
- 未污染 PlayerSave、SaveEnvelope 或 cloud save。

它不证明：

- catalog 可解析；
- localization copy 完整；
- `fm.text.full` 可加载；
- Host 已完成生产装配；
- 玩家 UI 已能端到端切换语言。

## 6. Catalog 中只能准备、不能执行的设计

### 6.1 Test builder

现在可以准备：

- builder 只构造最小有效 catalog，并允许单维度覆盖；
- builder 必须从一个 accepted baseline 派生，避免每个测试手写完整 JSON；
- malformed syntax、unknown field、missing field、duplicate key、unsupported locale、parameter mismatch 和各预算边界必须可单独注入；
- limit tests 统一采用 `limit - 1`、`limit`、`limit + 1` 三点展开；
- builder 不得把 production parser 作为自己的 oracle。

现在不能执行或冻结：

- 实际字段名、字段顺序和必需/可选集合；
- schema version 常量；
- JSON terminator bytes；
- record / byte / depth / string / key / parameter 等八项 budget 数值；
- CAT-19/20 及其他参数化叶节点的实际 payload；
- 以草稿 CSV 或推测 schema 生成测试 fixture。

这些内容必须等待 B，并在 C 槽位中随产品代码一同实现。

### 6.2 Diagnostic table

现在可以准备诊断表的稳定列：

- diagnostic code；
- detection stage；
- fail-closed result；
- retryability；
- safe operator detail；
- 是否允许包含 record/key 定位；
- 是否禁止回显原始玩家可见文本或大块 artifact bytes。

现在只能准备通用类别，例如：

- input missing / empty；
- malformed JSON；
- unsupported schema；
- canonical field violation；
- locale set violation；
- duplicate localization key；
- placeholder/parameter contract violation；
- budget exceeded；
- adapter lookup miss。

现在不能冻结每个类别的精确 code、检测 stage、预算名称、字段定位或 retryability。它们必须与真实 schema、terminator、manifest 和八项 budgets 对齐；否则诊断合同会先于事实固化。

`fm.language.save_unknown(errorCode)` 属于 B-PREF/AMEND-04 语义，可独立准备和实现，不应被错误归入 catalog 诊断等待。

### 6.3 Test fullname 与 filter

现在可以准备统一命名形状，例如：

- fixture 保持 `FightMatchLocalizationCatalogTests`；
- 测试名表达行为与预期结果；
- 参数化 limit tests 的 case name 必须显式包含 budget 维度和边界位置；
- 验证命令最终使用精确 fullname/filter，不用模糊全项目通配替代目标证明。

现在不能宣称：

- 最终 leaf fullname 列表；
- 最终 leaf count；
- CAT-19/20 展开后的每一项名称；
- 可直接交给 Unity Test Framework 的最终 filter。

最终 fullname/filter 必须由已实现 builder、真实预算维度与已接受 UGUI baseline 共同确定，并在派工包中逐项列出。

## 7. 方案一：按真实依赖拆成 B-PREF 与 B-CATALOG（推荐）

### 7.1 LOC-B-PREF

#### 交付目标

实现独立、internal、fail-closed 的 locale preference v1 store，并验证 AMEND-04 的保存结果语义。

#### 精确 6 路径白名单

1. `Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs`
2. `Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs.meta`
3. `Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs`
4. `Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs.meta`
5. `Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs`
6. `Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs.meta`

#### 执行前置

- A 已确认无漂移；
- UGUI 独立 R `ACCEPT`；
- `LocaleId`、`LocalizationService` 及相关 internal access 的最终哈希已写入包；
- C 串行 Unity 槽位已分配；
- 主程签署 exact packet，测试主导确认验证与 evidence 路径。

LOC-A artifact acceptance 不是该包的前置。

#### 验收边界

- 只新增上述 6 路径；
- preference v1 路径和 JSON schema 精确；
- 未修改 PlayerSave、SaveEnvelope、cloud save 或任何既有序列化；
- 使用真实已接受 `LocalizationService` 语义；
- 覆盖原 LOC-B 的 PREF-01 至 PREF-21；
- 独立 R 给出 `ACCEPT`、`NEEDS_FIX` 或 `REJECT`。

### 7.2 LOC-B-CATALOG

#### 交付目标

实现 bounded、fail-closed 的 catalog parser 和 `ILocalizedTextSource` adapter，并基于真实、已接受的 LOC-A 产物验证 schema、语义及边界预算。

#### 精确 6 路径白名单

1. `Assets/Scripts/FightMatch/Host/FightMatchLocalizationCatalogParser.cs`
2. `Assets/Scripts/FightMatch/Host/FightMatchLocalizationCatalogParser.cs.meta`
3. `Assets/Scripts/FightMatch/Host/FightMatchLocalizedTextSourceAdapter.cs`
4. `Assets/Scripts/FightMatch/Host/FightMatchLocalizedTextSourceAdapter.cs.meta`
5. `Assets/Tests/EditMode/FightMatchHost/FightMatchLocalizationCatalogTests.cs`
6. `Assets/Tests/EditMode/FightMatchHost/FightMatchLocalizationCatalogTests.cs.meta`

#### 执行前置

- A 已确认无漂移；
- B 的每一项真实输入均已收到并记录 SHA/receipt；
- LOC-A 已有独立 R `ACCEPT`；
- UGUI 独立 R `ACCEPT`；
- `ILocalizedTextSource`、`LocalizedTextResult`、`LocaleId` 的最终身份与哈希已绑定；
- C 串行 Unity 槽位已分配；
- 主程已把 exact canonical fields、terminator、八项 budgets、diagnostic table、test fullname/filter 写入签署后的 packet。

#### 验收边界

- 只新增上述 6 路径；
- parser 不自行加载 YooAsset 或访问生产路径；
- adapter 不拥有 Host composition；
- tests 使用真实接受 schema/bytes 作为基线，但不以 production parser 为 oracle；
- 覆盖原 LOC-B 的 CAT-01 至 CAT-26，含每一 budget 的三点边界；
- 独立 R 给出 `ACCEPT`、`NEEDS_FIX` 或 `REJECT`。

### 7.3 两包之间的关系

`LOC-B-CATALOG` 在逻辑上不依赖 `LOC-B-PREF`。二者只是因为 C 是唯一 Unity executor 而在执行时间上串行。

- 如果 `LOC-B-PREF` 先获 `ACCEPT`，后续 `LOC-B-CATALOG` 的回归应包含已接受的 preference tests，证明没有程序集级回归。
- 如果 LOC-A 先完成且 C 调度决定先执行 catalog，不应人为增加“必须先完成 B-PREF”的依赖。
- 任一包的 `ACCEPT` 都不能替代另一包或最终 D 集成的验收。

### 7.4 推荐理由

这一方案把稳定的用户偏好持久化与不稳定的生成 catalog 合同隔离：

- 允许 B-PREF 在 LOC-A license/runner/material/build 等待期间前进；
- 避免为了提前执行而猜测 catalog schema 或预算；
- 把每个实现包压缩为 6 路径、单一责任、独立回滚和独立 R gate；
- 不改变原有模块边界，也不引入新抽象层；
- 失败域清晰：preference I/O 问题不会与 catalog parse 问题混为一次验收。

## 8. 方案二：只准备拆分设计，保留一个 12 路径运行时包

另一可行方案是：

1. 当前只准备 `LOC-B-PREP` 文档，完成 A 类合同、诊断分类和测试矩阵；
2. 等 A、B、C 全部满足后，仍以原 12 路径 `LOC-B-RUNTIME` 一次执行；
3. 一次 compile/test/evidence，一次独立 R verdict；
4. D 仍另立后续包。

该方案不改变原实现包，协调成本最低，也完全避免半交付状态；但它让与 LOC-A 无关的 preference store 被无必要地绑定到 LOC-A 关键路径，失去真实可用的并行准备与提前实现窗口。单次 12 路径审查的故障定位和回滚粒度也更粗。

因此方案二是保守可行备选，不是推荐方案。

## 9. Public/API、asmdef、serialization 与白名单影响

### 9.1 Public/API

两种方案都不得新增或修改 public API。

- 新类型保持 `internal`；
- 不改变既有 `ILocalizedTextSource`、`LocalizedTextResult`、`LocaleId`、`LocalizationService` 签名；
- 不为未来扩展引入额外 public facade、provider interface 或 service locator；
- 若已接受 UGUI 输入与当前观察接口不同，状态为 `BLOCKED_INTERFACE_DRIFT`，由主程回到系统设计复核，而不是在实现包内兼容两套接口。

### 9.2 asmdef 与 friend access

两种方案都不允许修改：

- `FightMatch.Presentation.asmdef`；
- `FightMatch.Host.asmdef`；
- `FightMatch.Host.Tests.asmdef`；
- 任一 `AssemblyInfo.cs` 或 `InternalsVisibleTo`；
- Packages、依赖或 lockfile。

当前已有程序集引用与 friend access 应足以支持原设计。如果执行时发现不足，状态为 `BLOCKED_SCOPE_EXPANSION`，不能静默扩大白名单。

### 9.3 Serialization

`LOC-B-PREF` 会新增且只新增一个独立的 locale preference v1 文件合同。它不是现有业务存档的版本升级。

禁止修改或复用：

- PlayerSave；
- SaveEnvelope；
- cloud save schema；
- 关卡或战斗存档；
- LOC-A catalog schema。

`LOC-B-CATALOG` 只消费 LOC-A 已接受的 catalog schema，不拥有、不重写、不发布该 schema，也不产生第二个可人工编辑的数据权威。

### 9.4 原 12 路径白名单影响

推荐方案严格分区原 12 路径：

- B-PREF 拥有 preference 相关 6 路径；
- B-CATALOG 拥有 catalog 相关 6 路径；
- 路径不重叠；
- 两包合并后与原 12 路径集合完全相等；
- 不新增第 13 个产品、测试、`.meta`、配置或证据路径；
- 每个包自己的 task packet 必须重新列出精确白名单，不能引用“原包相关文件”等模糊表述。

原 `engineering-localization-luban-impl-b.md` 保持不变，继续是 `PREPARED_NOT_AUTHORIZED`。本建议不会自动拆改或授权它。

## 10. 责任转移与独立 R 门禁

### 10.1 责任链

1. 系统设计：提交本文建议，只负责依赖判断与边界，不拥有执行授权。
2. 主程：审查并选择方案；若选择方案一，分别制作 B-PREF 与 B-CATALOG 的精确执行包。
3. 中央 AI：确认拆分不会改变用户决定、全局顺序或当前暂停状态，并完成跨角色派工。
4. 测试主导：为每包绑定既有验收项、fresh evidence 路径、运行顺序与独立 R reviewer。
5. C：仅在收到签署后的包和串行槽位后成为相应 6 路径的唯一写入者。
6. R：独立检查实际 patch、白名单、fresh evidence 与 acceptance criteria，给出唯一 verdict。

本文不是从 SD00、主程或 C 向新实现者的隐式责任转移。任何所有权变化都必须先记录实际 thread/host/turn identity 和用户授权来源。

### 10.2 R gate

推荐方案需要至少两个独立 implementation verdict：

- `R-B-PREF`：只判断 preference 6 路径与 PREF 验收项；
- `R-B-CATALOG`：只判断 catalog 6 路径、真实 LOC-A 绑定与 CAT 验收项。

后续 RES-01D / Host composition 仍需要自己的独立 R gate。作者、C 或交付 owner 都不能自我批准。

`NEEDS_FIX` 必须产生更小的 corrective packet，保持原包白名单或进一步缩小；不得借修复扩大到另一半包。`REJECT` 不得通过重新命名包绕过。

## 11. 阻塞状态与不得绕过的条件

| 状态 | 触发条件 | 允许的下一步 |
|---|---|---|
| `BLOCKED_ON_UGUI_ACCEPT` | UGUI 尚无独立 `ACCEPT`，或接口最终身份未固定 | 只准备文档；等待 UGUI receipt |
| `BLOCKED_C_SERIAL_SLOT` | C 正在执行其他 Unity 写入或项目被占用 | 保持包已准备；等待正式调度 |
| `BLOCKED_ON_LOC_A_ACCEPTED_ARTIFACT` | 缺任一真实 artifact/manifest/canonical field/terminator/budget/R receipt | Catalog 只做 A 类准备；不得编码或猜值 |
| `BLOCKED_ON_RES_01D_HOST_COMPOSITION` | 请求涉及生产加载、ReleaseSet 或 Host 装配 | 转交独立 D 包；不得塞入 LOC-B |
| `BLOCKED_INTERFACE_DRIFT` | 已接受 UGUI 接口与当前设计假设不同 | 回主程/系统设计复核并重签输入 |
| `BLOCKED_SCOPE_EXPANSION` | 需要改 public API、asmdef、friend、依赖、第 13 路径或既有序列化 | 停止；由主程重新拆包并申请授权 |

特别禁止以下绕过：

- 用当前 draft CSV 替代 LOC-A accepted artifact；
- 用设计文档中的示例字段推断 canonical field table；
- 用预计文件大小拍定 parser budgets；
- 先写 parser 再让 LOC-A 产物适配实现；
- 以 B-PREF 已接受为由宣称 LOC-B 整体完成；
- 在 catalog 测试中内嵌另一个“临时正式 schema”；
- 由 LOC-B 自行实现 raw-byte resource bridge 或 Host composition。

## 12. 推荐调度顺序

在不恢复未授权产品工作的前提下，建议主程采用以下依赖驱动顺序：

1. 现在：接受或驳回本文的两包边界；可准备 B-PREF exact packet 和 Catalog 的 A 类设计附件。
2. UGUI 独立 `ACCEPT` 后：绑定接口哈希。
3. C 有空闲串行槽位时：即使 LOC-A 仍等待，也可单独授权并执行 B-PREF。
4. LOC-A 真实产物与独立 R `ACCEPT` 到达后：从 receipt 生成 Catalog 的 exact field/budget/diagnostic/test fullname 附件。
5. C 下一个合适槽位：执行 B-CATALOG，并纳入当时所有已接受相关回归。
6. B-CATALOG 独立 `ACCEPT` 后：把其可组合构件交给后续 RES-01D / Host composition 包；不得在此之前假设端到端完成。

这一路径让准备工作并行、Unity 写入串行、事实依赖不被数字编号伪造。

## 13. 本文验收自检

- [x] 只新增本设计文档；未修改原 LOC-B、产品、测试或证据。
- [x] 明确 A/B/C/D 是独立门禁维度。
- [x] 明确判断 B-PREF 可与 LOC-A 解耦，但仍需 UGUI `ACCEPT` 和 C 槽位才能执行。
- [x] 明确 catalog builder、diagnostic table、fullname/filter 中只能准备与必须等待的部分。
- [x] 给出两个可行拆分，并推荐真实依赖拆分方案。
- [x] 精确说明 public/API、asmdef、serialization 与原 12 路径白名单影响。
- [x] 明确责任转移与每包独立 R gate。
- [x] 明确不绕过真实 LOC-A artifact acceptance。
- [x] 状态保持 `PREPARED_NOT_AUTHORIZED`。

## 14. 返回合同

主程接收本文后应返回：

- 选择方案一、方案二，或附具体冲突点退回；
- 若选择方案一，分别给出 B-PREF 与 B-CATALOG delivery owner、fixed inputs、exact whitelist、acceptance、verification、evidence、rollback 和 R reviewer；
- 明确当前仍未授权执行的项目；
- 若任何批准决定发生变化，先经中央 AI 升级，不得由实现包自行改写。

本文在被主程与中央 AI 明确接收并转化为签署任务包前，不产生实现授权。
