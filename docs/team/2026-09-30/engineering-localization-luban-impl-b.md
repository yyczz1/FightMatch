# LOC-IMPL-B · bounded catalog adapter 与 locale preference 实施包

2026-10-01 · revision `prepared-01`

## 0. 包状态与责任

| 字段 | 固定值 |
| --- | --- |
| Task | `LOC-IMPL-B` |
| Status | `PREPARED_NOT_AUTHORIZED / BLOCKED_ON_LOC_A_ACCEPT_AND_C_SERIAL_SLOT` |
| Execution | `NOT_RUN` |
| 唯一未来 delivery owner | C，thread `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / host `local` |
| 签发与回传 | 主程，thread `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / host `local` |
| 包准备者 | 系统设计，thread `01a0f2e6-29bb-7092-abf0-705b41b7bf93` / host `local` |
| 专业接收 | QA／主测试在签发时绑定；作者和 C 不得接受自己的交付 |

本文是可绑定、可签发的候选任务包，不是实施授权。只有以下条件全部满足，主程才可
把状态改为 `AUTHORIZED_FOR_C` 并发送给 C：

1. LOC-IMPL-A 的实际 patch、新 evidence 和最终候选已由原独立 R 给出
   `ACCEPT`；
2. C 已完成当前 UGUI-01，实现及新证据获独立 R `ACCEPT`，并释放本项目唯一
   Unity 串行槽；
3. 主程已把第 2 节所有占位符替换为当时观测值，并重新计算本文身份；
4. 第 4 节 12 个新路径在签发前仍全部不存在，且所有只读输入逐文件身份一致；
5. QA／主测试已绑定独立接收者、聚焦测试总数与 evidence 闭集。

任一占位符未绑定、任一输入漂移或任一新路径已存在时均为 `BLOCKED`。不得由 C
猜值、沿用历史预测、把在途 LOC-A／UGUI 文件当成已接受输入，或先写代码再补签。
准备本文期间没有运行 Unity、没有写产品／测试／`.meta`／evidence，也没有进行
Git、网络、安装、发布或外部模型操作。

## 1. 唯一权威与结果边界

本包唯一产品设计权威是：

- `docs/team/2026-09-30/engineering-localization-luban-001.md`
- SHA-256 `022c060801df22520653856c24bd78dda394deb26f946dbff1b70ec9c338380a`
- 777 lines / 52,323 bytes
- 实施语义取 §9.2～§9.4 与 §10。该文不存在 §9.5，本包不得伪造该章节。

本包只交付三件事：

1. 同步、有限预算、fail-closed 的 canonical catalog parser；
2. 实现现有 internal `FightMatch.Presentation.ILocalizedTextSource` 的 adapter；
3. 独立 locale preference v1 store 及其故障语义。

它不定位、打开或下载 production artifact，不拥有 transport，不接
`FightMatchPlayerHost`，不改变现有 View，不把 repository-relative 路径伪装为 Player
build 的 artifact 来源。production artifact 装载、outer ReleaseSet SHA/admission、
RES-01D `fm.text.full` raw-byte bridge 与 Host composition 全部保持 `NOT_RUN`。

## 2. 签发前必须绑定的精确输入

主程须替换下表全部 `{{...}}` token。绑定值进入本文和 evidence
`binding.json`；不得只在聊天中口头给出。

### 2.1 authority、owner 与运行环境

| Token | 必须绑定的观测值 |
| --- | --- |
| `{{DISPATCH_AUTHORITY_RECEIPT}}` | 用户既有授权出处与主程签发 turn／receipt |
| `{{C_DISPATCH_TURN_ID}}` | C 实际收到本 revision 的 turn id |
| `{{QA_THREAD_ID}}` / `{{QA_HOST_ID}}` | 独立 QA／主测试身份 |
| `{{UGUI_ACCEPT_R_THREAD_TURN_FINAL}}` | 当前 UGUI patch 与新证据的独立 R 身份及唯一 `ACCEPT` |
| `{{LOC_A_ACCEPT_R_THREAD_TURN_FINAL}}` | LOC-A 实际 patch、新 evidence、候选的原独立 R 身份及唯一 `ACCEPT` |
| `{{PROJECT_ROOT}}` | canonical absolute FightMatch 根路径 |
| `{{UNITY_EDITOR_PATH}}` | Unity `2022.3.18f1` Mac Intel executable 绝对路径 |
| `{{UNITY_EDITOR_IDENTITY}}` | 实测 version、architecture、executable SHA-256、安装 receipt |
| `{{UNITY_PROCESS_GATE_RECEIPT}}` | 时间戳化进程检查，证明本项目未被其它 Unity 打开 |
| `{{COMPILE_TIMEOUT}}` | 单次 compile/import 非零有限上限 |
| `{{EDITMODE_TIMEOUT}}` | 单次聚焦 EditMode 非零有限上限 |

### 2.2 已接受 LOC-A 身份

| Token | 必须绑定的观测值 |
| --- | --- |
| `{{LOC_A_PACKET_SHA256}}` | 最终实际执行 LOC-A 包身份 |
| `{{LOC_A_EVIDENCE_ROOT}}` | 获 R 接受的 create-once A evidence 根 |
| `{{ACCEPTED_A_ARTIFACT_SHA256}}` / `{{ACCEPTED_A_ARTIFACT_BYTES}}` | `Generated/FightMatch/Localization/fm-text-v1.json` 完整身份 |
| `{{ACCEPTED_A_MANIFEST_SHA256}}` / `{{ACCEPTED_A_MANIFEST_BYTES}}` | `fm-text-v1.manifest.json` 完整身份 |
| `{{ACCEPTED_COPY_SHA256}}` | manifest 中独立的 `acceptedCopySha256`；预期仍为接受的 AMEND-04 身份 |
| `{{FORMAL_SOURCE_SHA256}}` | manifest 中独立的 `formalSourceSha256`；不得等同 COPY 身份 |
| `{{ARTIFACT_SCHEMA_VERSION}}` | A 接受 artifact 的实际 schema version |
| `{{ACCEPTED_RECORD_COUNT}}` | A manifest 与 artifact 共同证明的 record／unique-key count，必须至少 264 |
| `{{ACCEPTED_LOCALE_SUMMARY}}` | 精确为两种 locale `en,zh-Hans` 的实际 manifest 摘要 |
| `{{ACCEPTED_SEVERITY_SUMMARY}}` | A manifest 的 info/warning/error/blocking 实际分布 |
| `{{ACCEPTED_PARAMETER_SUMMARY}}` | A manifest 的 parameterized／parameter-name 实际摘要 |
| `{{ACCEPTED_CANONICAL_FIELD_TABLE}}` | 从接受 artifact/compiler 合同逐层抄录的 root、record、locale、parameter 精确字段、类型、必选性 |
| `{{ACCEPTED_ARTIFACT_TERMINATOR}}` | 接受 artifact 在 JSON root 后的精确终止 bytes；无则明确记 `NONE` |
| `{{ACCEPTED_A_INPUT_RECEIPT_SHA256}}` | 同时核对 artifact、manifest、formal source、COPY 和 A R 的只读 receipt |

测试必须读取这两个真实、已接受的 Generated 文件 bytes。不得复制数据为 test-only
catalog，不得继续用 263-key COPY CSV、`HostMemoryTextSource` 或任意仓库相对路径
冒充 Player artifact 加载。测试读取仓库文件只是 B 的已接受输入验证，不构成产品
加载路径。

### 2.3 UGUI 后当前源码／程序集输入

签发时生成 `{{CURRENT_SOURCE_INPUT_RECEIPT}}`，逐文件记录下列**全部**路径的
SHA-256 与 bytes。表中每个 `{{SHA256_*}}` / `{{BYTES_*}}` 都必须替换：

| 只读路径 | SHA-256 / bytes token |
| --- | --- |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTextSource.cs` | `{{SHA256_LOCALIZED_TEXT_SOURCE}}` / `{{BYTES_LOCALIZED_TEXT_SOURCE}}` |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocaleId.cs` | `{{SHA256_LOCALE_ID}}` / `{{BYTES_LOCALE_ID}}` |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocalizationService.cs` | `{{SHA256_LOCALIZATION_SERVICE}}` / `{{BYTES_LOCALIZATION_SERVICE}}` |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTmpText.cs` | `{{SHA256_LOCALIZED_TMP_TEXT}}` / `{{BYTES_LOCALIZED_TMP_TEXT}}` |
| `Assets/Scripts/FightMatch/Presentation/PresentationAssemblyInfo.cs` | `{{SHA256_PRESENTATION_FRIEND}}` / `{{BYTES_PRESENTATION_FRIEND}}` |
| `Assets/Scripts/FightMatch/Presentation/FightMatch.Presentation.asmdef` | `{{SHA256_PRESENTATION_ASMDEF}}` / `{{BYTES_PRESENTATION_ASMDEF}}` |
| `Assets/Scripts/FightMatch/Host/HostAssemblyInfo.cs` | `{{SHA256_HOST_FRIEND}}` / `{{BYTES_HOST_FRIEND}}` |
| `Assets/Scripts/FightMatch/Host/FightMatch.Host.asmdef` | `{{SHA256_HOST_ASMDEF}}` / `{{BYTES_HOST_ASMDEF}}` |
| `Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs` | `{{SHA256_PLAYER_HOST}}` / `{{BYTES_PLAYER_HOST}}` |
| `Assets/Scripts/FightMatch/Host/FightMatchHostView.cs` | `{{SHA256_HOST_VIEW}}` / `{{BYTES_HOST_VIEW}}` |
| `Assets/Tests/EditMode/FightMatchHost/FightMatch.Host.Tests.asmdef` | `{{SHA256_HOST_TEST_ASMDEF}}` / `{{BYTES_HOST_TEST_ASMDEF}}` |
| `Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs` | `{{SHA256_HOST_TEST_FIXTURE}}` / `{{BYTES_HOST_TEST_FIXTURE}}` |
| `Assets/Tests/EditMode/FightMatch/FightMatch.Core.Tests.asmdef` | `{{SHA256_CORE_TEST_ASMDEF}}` / `{{BYTES_CORE_TEST_ASMDEF}}` |
| `Assets/Tests/EditMode/FightMatch/LocalizationContractTests.cs` | `{{SHA256_LOCALIZATION_CONTRACT_TESTS}}` / `{{BYTES_LOCALIZATION_CONTRACT_TESTS}}` |
| `Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs` | `{{SHA256_LOCALIZED_BINDING_TESTS}}` / `{{BYTES_LOCALIZED_BINDING_TESTS}}` |
| `Assets/Tests/EditMode/FightMatch/LocalePolicyTests.cs` | `{{SHA256_LOCALE_POLICY_TESTS}}` / `{{BYTES_LOCALE_POLICY_TESTS}}` |

若现有 `ILocalizedTextSource.Resolve`、`LocaleId`、`LocalePolicy`、friend 或程序集引用
与本包不一致，终态为 `BLOCKED_INTERFACE_MISMATCH`。不得复制接口、用反射、增加
friend、改 asmdef 或扩大 public surface。

### 2.4 预算、诊断、测试与 evidence 绑定

| Token | 必须绑定的观测值 |
| --- | --- |
| `{{PARSER_BUDGET_TABLE}}` | 第 6 节每一 measured maximum、margin、最终常量的完整数表 |
| `{{PARSER_DIAGNOSTIC_TABLE}}` | 每个 parser failure 类别的稳定 internal code、stage、retryability |
| `{{PREFERENCE_DIAGNOSTIC_TABLE}}` | 每个 load/save fault stage 的稳定 internal errorCode 与三态结果 |
| `{{UGUI_ACCEPTED_FULLNAME_RECEIPT}}` | 接受 UGUI Q4 XML 的完整 test fullname 清单及 SHA |
| `{{UGUI_ACCEPTED_BASELINE_COUNT}}` | 从上述 XML 实测的 passed/failed/skipped/inconclusive 总数，不抄旧预测 |
| `{{NEW_TEST_FULLNAME_RECEIPT}}` | 第 8 节矩阵展开后的每个 NUnit leaf fullname，含参数化 case 参数 |
| `{{NEW_TEST_COUNT}}` | 上述 fullname receipt 的实际 leaf 总数，不从文件／method 数推算 |
| `{{FOCUSED_TEST_FILTER}}` | 接受 UGUI fullname baseline 与全部 B 新 fullname 对应的单行冻结 filter |
| `{{FOCUSED_EXPECTED_TOTAL}}` | filter 实际应运行的精确总数；重复 fullname 去重后冻结 |
| `{{CREATE_ONCE_EVIDENCE_ROOT}}` | 精确为 `TestArtifacts/FightMatch/LOC-IMPL-B/{{ACCEPTED_A_ARTIFACT_SHA256}}/`，签发前不存在且为空 |
| `{{NEW_PATH_ABSENCE_RECEIPT}}` | 第 4 节 12 路径签发前全部不存在的时间戳化 receipt |

`{{PARSER_DIAGNOSTIC_TABLE}}` 和 `{{PREFERENCE_DIAGNOSTIC_TABLE}}` 只冻结 internal
结果与测试断言，不授权新增 public API。若无法在现有 friend 内完成，立即
`BLOCKED_INTERFACE_MISMATCH`。

## 3. 允许读取的最小上下文

C 可读取：

- `AGENTS.md`、`START_HERE.md`、`.agent/PROJECT_CONTEXT.md`、
  `.agent/CODING_RULES.md`、`.agent/VALIDATION.md`、`.agent/TEAM_WORKFLOW.md`；
- 第 1 节权威设计和签发后本文；
- `docs/system-design/2026-09-17/runtime-ui-ugui-correction.md`；
- 获接受的 `docs/team/2026-09-30/engineering-ugui-01-implementation.md`；
- 第 2.3 节全部只读源码／测试／asmdef；
- `Config/FightMatch/Luban/luban.conf`；
- `Config/FightMatch/Localization/__tables__.csv`；
- `Config/FightMatch/Localization/FightMatchText.csv`；
- `Tools/FightMatch.Localization.Compiler/FightMatch.Localization.Compiler.csproj`；
- `Tools/FightMatch.Localization.Compiler/Program.cs`；
- `Tools/FightMatch.Localization.Compiler/global.json`；
- 两个接受的 `Generated/FightMatch/Localization/fm-text-v1*` 文件；
- `{{LOC_A_EVIDENCE_ROOT}}` 中由接受 R 指明的 `inputs.json`、`run.json`、
  `comparison.json`、两次 canonical／manifest、semantic digest、self-test、negative
  与 diff-check；
- 接受 UGUI 的 Q4 XML／log／source identity receipt，以及现有 TMP font、material、
  OFL 只读输入，仅用于第 8 节 binding/glyph 验证。精确路径是
  `Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset`、其 `.meta`、
  `Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular.otf`、其 `.meta`、
  `Assets/UI/FightMatch/Fonts/OFL.txt` 及其 `.meta`；签发 receipt 必须记录各自
  SHA-256/bytes，缺一即阻断 CAT-10。

读取权限不等于写权限。不得重跑 LOC-A、修改其产物或 evidence。

## 4. 唯一写白名单

### 4.1 只允许新建的 12 个产品／测试路径

```text
Assets/Scripts/FightMatch/Host/FightMatchLocalizationCatalogParser.cs
Assets/Scripts/FightMatch/Host/FightMatchLocalizationCatalogParser.cs.meta
Assets/Scripts/FightMatch/Host/FightMatchLocalizedTextSourceAdapter.cs
Assets/Scripts/FightMatch/Host/FightMatchLocalizedTextSourceAdapter.cs.meta
Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs
Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs.meta
Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs
Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs.meta
Assets/Tests/EditMode/FightMatchHost/FightMatchLocalizationCatalogTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchLocalizationCatalogTests.cs.meta
Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs
Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs.meta
```

精确为 4 个 production C#＋4 个 `.meta`、2 个 test C#＋2 个 `.meta`。不得修改任何
既有生产或测试文件。`.meta` 由固定 Unity Editor 首次导入这些新脚本生成并由 C
核 GUID；不得手写 YAML、复用 GUID 或让别的 Unity 实例生成。

### 4.2 evidence 唯一写域

除上述 12 路径外，只允许在 `{{CREATE_ONCE_EVIDENCE_ROOT}}` 创建第 10 节闭集。
该根 create-once、append-by-creation；不得覆盖旧证据或在根外写临时产品输出。
正常 Unity `Library/Temp/Logs` 副作用不属于交付物，不得手工编辑或用来藏差异。

### 4.3 明确禁止

- 禁止修改任何现有 production/test 文件，尤其 `FightMatchPlayerHost.cs`、
  `FightMatchHostView.cs`、`FightMatchHostTestFixture.cs`、所有 asmdef 和 friend 文件；
- 禁止修改 Core、Application、Save、Platform、Content、Input、Presentation；
- 禁止修改 Config、Tools、Generated、scene、Prefab、font、material、
  StreamingAssets、YooAsset、Packages、ProjectSettings、文档、旧 evidence、Git；
- 禁止新增／升级 dependency、Unity Localization、Addressables、Luban runtime、
  Luban 类型、runner、第三方 DTO 或 generated C#；
- 禁止公共 API、Unity serialized field／format、PlayerSave、SaveEnvelope、业务恢复、
  OperationId、云档、CI、build script 或 release 设置变更；
- 禁止删除、移动、重命名、顺手清理、重排或格式化任务外文件。

### 4.4 protected-change 权限

| 类型 | 允许 | 精确范围 |
| --- | ---: | --- |
| 新 internal 类型 | YES | 只在 4 个新 production C#；复用既有 friend |
| 新 public API | NO | 无 |
| 新独立 preference schema | YES | 仅 v1 `{ "version": 1, "locale": "en|zh-Hans" }`，不进入业务 save |
| 修改 artifact schema | NO | 只消费 A 接受 schema v1 |
| 修改现有 serialization | NO | PlayerSave／SaveEnvelope／ReleaseSet 均不变 |
| 新 package/dependency | NO | 无 |
| asmdef/friend/config/build/CI | NO | 无 |
| scene/prefab/font/StreamingAssets | NO | 无 |
| 新 `.meta` | YES | 仅第 4.1 节 6 个新脚本对应 `.meta` |
| Git mutation | NO | 不 branch/commit/push/merge/rebase/reset/stash |

最大范围：12 个新文件；production C# 4、test C# 2、`.meta` 6；C# 新增约
1,800 行以内。超过预算或需要第 13 个路径即 `BLOCKED_SCOPE_EXPANSION`，不得自行拆
接口或扩白名单。

## 5. parser 与 adapter 的冻结合同

### 5.1 输入所有权与同步边界

- parser 只接受未来调用方／测试注入的已受验 memory bytes 或 stream，以及上游
  注入的 expected artifact identity；它不接受路径，不找文件，不打开／下载 artifact，
  不读取 manifest 路径，不拥有 transport。
- memory 由调用方拥有；调用方保证同步 parse 返回前不改写。parse 返回后 production
  result 不保留 memory、array segment、span 或其 backing buffer 引用。
- stream 由调用方创建并 Dispose；parser 不关闭、不 Dispose、不 seek 回写。对未知
  长度 stream 最多读取 `maxArtifactBytes + 1` bytes 后即判超限并停止，不得
  `ReadToEnd` 到无界缓冲。
- 所有 parse、Resolve、load/save 都同步完成；不得创建 Task、coroutine、后台线程、
  全局 mutable cache 或 watcher。
- byte limit 必须在分配完整对象图前检查；token、record、text、parameter 每次读取
  即递增对应预算，不得先完整 materialize 后再计数。

### 5.2 canonical schema、UTF-8、duplicate、unknown 与 trailing

签发后的唯一 schema 是 `{{ACCEPTED_CANONICAL_FIELD_TABLE}}`：

- root、record、locale、parameter 对象只允许表中 exact-case 字段；所有必选字段
  必须恰好出现一次。任何层级 unknown、case-variant 或 duplicate property 都是
  schema failure；不得由 `JsonUtility` 的忽略行为吞掉。
- 只接受 `{{ARTIFACT_SCHEMA_VERSION}}`、manifest/expected identity 的精确 SHA、
  `{{ACCEPTED_RECORD_COUNT}}` 与 exact locale set `en,zh-Hans`；无隐式 fallback。
- 输入必须是严格 UTF-8；拒绝 BOM、invalid/overlong sequence、孤立 surrogate、
  非法 control character。完整 root 后只允许
  `{{ACCEPTED_ARTIFACT_TERMINATOR}}` 所冻结的 exact termination；不得有额外 token、
  第二个 root、垃圾 byte、额外 whitespace 或其它 trailing bytes。
- localization key、JSON property、locale id、parameter name 与 duplicate 检测均用
  ordinal、case-sensitive 比较；不得 culture fold、大小写折叠或 Unicode
  normalization。key 必须非空且以 `fm.` ordinal 开头。
- records 必须按 key ordinal 严格递增；同 key 重复或乱序均拒绝。locale 固定
  `en` 后 `zh-Hans`；parameter names 非空、唯一且 ordinal 严格递增。
- severity 只接受 exact lowercase `info`、`warning`、`error`、`blocking` 并映射到
  既有 `LocalizedTextSeverity`；有效业务 severity 不等于 binding failure。
- 两语言 template 非空；`{name}` 集合逐字一致并与声明 parameters 双向相等；拒绝
  空名、重复、位置参数、未闭合／额外 brace 和未声明变量。
- `fm.diagnostic.placeholder` 两语言精确为 `【if you see this, it is a bug.】`、
  severity `blocking`。`fm.language.save_failed` 与
  `fm.language.save_unknown` 都只声明 `errorCode` 且 severity `error`。
- 玩家文本不得含 AMEND-03 禁止的 `OperationId`、`CommitId`、`FaceId`、`PairId`、
  `TargetKind`、内部版本号或 enum 原值。此门读取真实 A bytes 验证，不另造副本。

### 5.3 SHA、count 与 fail-closed publication

- parser 对 caller 注入的 expected artifact SHA 做完整 bytes SHA-256 比较，并核
  schema/count/locale 摘要。expected identity 最终由 RES-01D outer admission 提供；
  B 只实现注入 seam 和测试，不模拟 ReleaseSet。
- 任一 SHA/schema/version/count/duplicate/key/text/parameter/severity/JSON/UTF-8/
  budget/stream failure 都只返回 `{{PARSER_DIAGNOSTIC_TABLE}}` 中的稳定 code、stage、
  retryability；异常类型、堆栈、原始 message、路径和输入片段不得进入 Player 结果。
- failure result 的 catalog 和 adapter 都必须为 null／不可取得；不得发布部分字典、
  保留半解析记录或替换现有 source。只有全部验证成功才一次性构造不可变 catalog。
- parser/adapter 不写磁盘、不写 static current source、不触发 locale event、不改变
  `LocalizationService`。

### 5.4 adapter 的现有接口语义

- adapter 保持 `internal`，精确实现现有 `ILocalizedTextSource.Resolve(string key,
  LocaleId locale, IReadOnlyList<KeyValuePair<string,string>> namedArgs)`；不新增 public
  wrapper 或第二份 Presentation interface。
- `LocaleId.En` 只选 `en`，`LocaleId.ZhHans` 只选 `zh-Hans`；未知 enum 返回既有
  `UnknownLocale`，绝不 fallback。
- key 缺失返回 `MissingLocalizationKey`；locale text 缺失返回
  `MissingLocaleText`；template 合同破坏返回 `InvalidTemplate`。
- named args 的 null collection 等同 empty；name 空、value null 或 ordinal duplicate
  返回 `InvalidNamedArguments`；缺或多参数返回 `ParameterMismatch`。参数输入不保留，
  替换值按 literal 使用，不再解释其中 brace。
- 成功结果返回精确替换后的 text 与业务 severity。缺 key／参数／template／locale、
  TMP/font/material/glyph 属 binding/infrastructure failure；失败结果无业务 severity，
  继续由现有 `LocalizationService`／`LocalizedTmpText` 产生 diagnostic placeholder 和
  Host gate。
- adapter 不重新实现字体检查、Host gate、locale event、UI rebind 或 business action。

## 6. 必须在签发时冻结的 parser budgets

下表每一行都要由接受 A manifest/artifact 实测最大值 `observed`、明确正 margin 和
最终 `max = observed + margin` 填满，并逐项写入 production constant 与测试。所有值
必须非零、有限、可安全相加，禁止 `int.MaxValue`、浮动配置或实施者临场放宽。

| budget | observed token | margin token | final max token | 测量口径 |
| --- | --- | --- | --- | --- |
| `maxArtifactBytes` | `{{OBS_ARTIFACT_BYTES}}` | `{{MARGIN_ARTIFACT_BYTES}}` | `{{MAX_ARTIFACT_BYTES}}` | 完整 accepted artifact bytes |
| `maxRecords` | `{{OBS_RECORDS}}` | `{{MARGIN_RECORDS}}` | `{{MAX_RECORDS}}` | records；final 必须 ≥ 264 |
| `maxKeyChars` | `{{OBS_KEY_CHARS}}` | `{{MARGIN_KEY_CHARS}}` | `{{MAX_KEY_CHARS}}` | 单 key UTF-16 decoded chars |
| `maxTextCharsPerLocale` | `{{OBS_TEXT_CHARS}}` | `{{MARGIN_TEXT_CHARS}}` | `{{MAX_TEXT_CHARS_PER_LOCALE}}` | 单 record 单 locale template chars |
| `maxParametersPerRecord` | `{{OBS_PARAMETERS_PER_RECORD}}` | `{{MARGIN_PARAMETERS_PER_RECORD}}` | `{{MAX_PARAMETERS_PER_RECORD}}` | 单 record 唯一参数数 |
| `maxParameterNameChars` | `{{OBS_PARAMETER_NAME_CHARS}}` | `{{MARGIN_PARAMETER_NAME_CHARS}}` | `{{MAX_PARAMETER_NAME_CHARS}}` | 单参数名 chars |
| `maxTotalDecodedChars` | `{{OBS_TOTAL_DECODED_CHARS}}` | `{{MARGIN_TOTAL_DECODED_CHARS}}` | `{{MAX_TOTAL_DECODED_CHARS}}` | keys＋两 locale templates＋parameter names 的累计 decoded chars |
| JSON `maxDepth` | `{{OBS_JSON_DEPTH}}` | `{{MARGIN_JSON_DEPTH}}` | `{{MAX_JSON_DEPTH}}` | root 起算的最大 container depth |

locale 边界不是开放字符串预算：数量固定为 2，唯一值精确为 `en`、`zh-Hans`，按
ordinal 比较。template 边界就是 `maxTextCharsPerLocale`；parameter 边界由 count 与
name chars 两项共同限制。不得另造第三 locale、隐式 locale 别名或第二套预算 API。

每一预算都必须有独立 `limit-1`、`limit`、`limit+1` fixture。多预算同时接近上限
时，fixture 必须明确唯一先触发项，避免一条 case 偶然由别的更小预算拦截。

## 7. locale preference v1 冻结合同

### 7.1 文件、load 与首次选择

- 唯一路径：
  `Application.persistentDataPath/FightMatch/settings/locale-preference-v1.json`。
  测试以注入的临时 root／internal fault seam 等价验证；production default 才组合
  `Application.persistentDataPath`。不写 PlayerSave、SaveEnvelope 或 cloud。
- 唯一 schema：`{ "version": 1, "locale": "en|zh-Hans" }`。字段 exact-case、
  必选且只出现一次；unknown、duplicate、trailing、坏 UTF-8、坏 JSON、未知 version、
  未知 locale 都是 invalid preference。
- target missing 时用既有 `LocalePolicy.FromSystemLanguage`：Chinese、
  ChineseSimplified、ChineseTraditional → `zh-Hans`；其余包括 Unknown → `en`。
- 合法文件优先于 system language，恢复 remembered manual selection。
- corrupt／unknown version／unknown locale／read failure 必须保留原 target bytes，
  本 session 用 system locale 初始化并返回稳定诊断；不得写回“修复”、删除或覆盖。

### 7.2 手选、session 与持久化顺序

模块级验收 harness 必须按以下顺序使用现有 `LocalizationService` 和新 store：

1. 用户选择 desired locale；
2. 先调用现有 `SetLocale` 更新 `CurrentLocale`，仅在 locale 真变化时触发恰好一次
   `LocaleChanged`／现有活树 rebind；
3. 再调用 store 持久化；store 自身不得拥有或重复触发 UI/business event；
4. 无论 saved、`save_failed` 或 `UNKNOWN`，当前 session 都保持 desired locale；
5. 同 desired locale retry 重做完整 preference 写入，但 `SetLocale` 返回 false，
   不重复 locale change、rebind 或 business event。

本包不把该顺序接进 `FightMatchHostView`／`FightMatchPlayerHost`，只以真实现有 service
的 module harness 证明合同。Host composition 等 RES-01D 后续包。

### 7.3 原子写状态机与错误结果

所有 stage 的稳定 `errorCode` 使用签发时的
`{{PREFERENCE_DIAGNOSTIC_TABLE}}`。Player-visible 映射只有：

| 结果 | 可见 key | 必须满足 |
| --- | --- | --- |
| saved | `fm.language.saved` | commit 后 reopen/read/strict parse/逐字段 compare 全成功 |
| `save_failed` | `fm.language.save_failed(errorCode)` | replace/rename commit 尚未发生，或 post-replace rollback 已获 byte proof |
| `UNKNOWN` | `fm.language.save_unknown(errorCode)` | commit 已发生且新值后验不可证明，且旧值也未获 rollback byte proof |

固定过程：

1. 同目录创建唯一 temp，写完整 v1 bytes，Flush 到稳定存储；
2. 重新打开 temp，strict parse 并逐字段 pre-verify desired locale；
3. 对已存在 target 保存 pre-write backup 事实；对原本 absent 保存 absence 事实；
4. 用同目录 atomic replace／rename commit temp 到 target；该成功点是状态边界；
5. commit 后必须重新打开 target、读取、strict parse 并逐字段比较 version/desired
   locale；全部通过才 saved；
6. post-replace 验证失败可尝试 backup rollback。只有 rollback 操作成功，且再次
   reopen/read 逐 byte 证明旧 bytes 或旧 absence 恢复，才降为 `save_failed`。

temp create/write、Flush、pre-verify open/read/parse/compare、commit-before-replace
任一失败都尚未发生 commit，**恒为** `save_failed`。尽力 cleanup 和旧 target
read-back 只记受限诊断；即使 cleanup 或随后旧 target read-back 也失败，仍不得升级
为 `UNKNOWN`。

commit 成功后 reopen、read、parse、version/locale compare 任一失败／mismatch 均先
为 `UNKNOWN`。rollback 失败、rollback 后 reopen/read 失败、bytes 不同或 absence
未证明都继续 `UNKNOWN`，不得声称旧值保留或“未保存”。`UNKNOWN` 不能出现在
pre-replace 阶段。

`save_failed` 与 `UNKNOWN` 都不得进入 PlayerSave 的 SaveFailed Retry、业务 unknown、
OperationId、恢复或云档；不改任何业务状态。异常、路径、原始消息和文件片段只进
受限诊断，不进 `errorCode` 或 Player 文本。

## 8. 冻结测试矩阵

两个新 fixture 必须使用真实接受 A bytes；synthetic bytes 只用于边界／负例，并
由同一 test builder 从签发 schema 生成。不得复制 production parser 或用另一套宽松
JSON parser 计算期望值。签发时把每个 leaf fullname 和参数写进
`{{NEW_TEST_FULLNAME_RECEIPT}}`，以下每行至少一个独立可观察 leaf case。

### 8.1 `FightMatchLocalizationCatalogTests`

| ID | 必测输入 | 可观察结果 |
| --- | --- | --- |
| CAT-01 | 接受 artifact＋manifest identity | parse 成功；record/unique key、两 locale、severity/parameter 摘要精确匹配 |
| CAT-02 | 全部接受 records | key ordinal 严增；`en`/`zh-Hans` 都可解析；无 fallback |
| CAT-03 | 全部 parameterized records | 两 template placeholder 与声明集合 ordinal 完全一致，literal substitution 正确 |
| CAT-04 | 接受 bytes | AMEND-03 禁止 internal ID/enum 原值不存在；两 language save key 与 placeholder 精确 |
| CAT-05 | 接受 manifest | COPY 与 formal source 两身份独立且精确，artifact SHA/schema/count 一致 |
| CAT-06 | 接受 bytes 的 semantic round-trip | key/text/args/severity 无丢失；不要求新增 production serializer |
| CAT-07 | missing key／unknown locale／missing locale text | 分别得到既有 stable failure；无业务 severity |
| CAT-08 | missing/extra/duplicate/null named arg、brace-bearing literal value | 参数错误 fail；合法 literal 不被二次解析 |
| CAT-09 | 四种有效 severity | 成功且保留 tone；不打开 binding failure |
| CAT-10 | 真实 adapter＋现有 binding | 缺 key/参数/template/font/glyph 各产生 diagnostic placeholder 和现有 gate |
| CAT-11 | expected SHA、schema version、declared count 各自错误 | 对应 stable parse failure；catalog/adapter 都未发布 |
| CAT-12 | root/record/locale/parameter 的 missing、null、wrong-type | strict schema failure，无部分对象 |
| CAT-13 | 每层 unknown、case-variant、duplicate property | strict schema failure，不被忽略 |
| CAT-14 | duplicate key、key 非 `fm.`、record ordinal 乱序 | fail closed |
| CAT-15 | locale 缺/多/重复/乱序；severity 非 exact lowercase | fail closed |
| CAT-16 | parameter 空/重复/乱序/爆炸；template mismatch/坏 brace | fail closed |
| CAT-17 | truncated JSON、第二 root、extra trailing token/byte | fail closed；只允许 canonical 终止换行 |
| CAT-18 | BOM、invalid/overlong UTF-8、孤立 surrogate/control | fail closed，不泄漏输入片段 |
| CAT-19 | JSON depth `limit-1/limit/limit+1` | 前两者只在其它合同合法时成功，`+1` 对应 budget failure |
| CAT-20 | 其余七项 budget 各 `limit-1/limit/limit+1` | 每项边界精确，唯一触发预期 budget |
| CAT-21 | non-seekable unknown-length stream | 最多读 `maxArtifactBytes+1`；超限后不再读且不关闭 stream |
| CAT-22 | stream 中途抛异常 | stable stream failure；stream 仍由 caller Dispose；零 partial publication |
| CAT-23 | memory parse 后覆写 caller buffer；stream parse 后 caller Dispose | 已发布 immutable catalog 不变，未保留输入引用 |
| CAT-24 | 已有成功 adapter 后解析另一个坏输入 | 原 adapter 不变；失败不能替换 current source |
| CAT-25 | 同一坏类别使用不同 exception/message/path/input fragment | 对外 code/stage/retryability 稳定且不含敏感原文 |
| CAT-26 | 静态源码检查 | 无 path/open/download/Task/coroutine/reflection/Luban/third-party DTO/JsonUtility 宽松解析 |

CAT-19 和 CAT-20 必须展开为第 6 节八个 budget 各自的三个 leaf cases；不能只用循环
里多个 Assert 冒充 fullname 清单，也不能从 `[TestCase]` method 数猜总数。

### 8.2 `LocalePreferenceStoreTests`

| ID | 必测输入／fault stage | 可观察结果 |
| --- | --- | --- |
| PREF-01 | missing＋Chinese/ChineseSimplified/ChineseTraditional | 首次均为 `zh-Hans`，零写入 |
| PREF-02 | missing＋English/Japanese/Unknown/其它 | 首次均为 `en`，零写入 |
| PREF-03 | 合法 `en` 与 `zh-Hans` | remembered manual selection 覆盖 system locale |
| PREF-04 | corrupt JSON／bad UTF-8／unknown field／duplicate/trailing | 原 bytes 不变；本次 system fallback；零 repair write |
| PREF-05 | unknown version／unknown locale／read I/O failure | 原文件保留；本次 system fallback；零 repair write |
| PREF-06 | 完整成功路径 | temp/Flush/pre-verify/commit/reopen/read/parse/compare 顺序正确，saved key 精确 |
| PREF-07 | temp create/write failure | `save_failed`；session desired locale 保持 |
| PREF-08 | Flush failure | `save_failed`；session desired locale 保持 |
| PREF-09 | temp pre-verify open/read/parse/mismatch | 各自 `save_failed`；target 未被本流程修改 |
| PREF-10 | commit-before-replace/rename failure | `save_failed`；target 未被本流程修改 |
| PREF-11 | PREF-07～10 后 cleanup failure／旧 target read-back failure | 仍为 `save_failed`，绝不 UNKNOWN |
| PREF-12 | commit 后 target reopen failure | `UNKNOWN`＋save_unknown key；不得声称旧值保留 |
| PREF-13 | commit 后 read failure | `UNKNOWN`＋save_unknown key |
| PREF-14 | commit 后 parse/version/locale mismatch | 各自 `UNKNOWN`＋save_unknown key |
| PREF-15 | post-replace rollback 成功且 reread byte-equal old target | 降为 `save_failed`；旧 bytes proof 存在 |
| PREF-16 | 原 target absent，rollback 后 reread 证明仍 absent | 降为 `save_failed`；old absence proof 存在 |
| PREF-17 | rollback 失败／rollback 后 reopen/read 失败／bytes 不同 | 各自保持 `UNKNOWN` |
| PREF-18 | save_failed 与 UNKNOWN | `CurrentLocale` 均保持 desired，恰好一次 locale event/rebind |
| PREF-19 | 同 desired locale retry，首轮失败后次轮成功 | 完整 storage flow 重做；零重复 locale/rebind/business event；最后 saved |
| PREF-20 | 结果与错误矩阵 | 每 stage 精确匹配 `{{PREFERENCE_DIAGNOSTIC_TABLE}}`，无 path/message/input 泄漏 |
| PREF-21 | 边界静态检查 | 无 PlayerSave/SaveEnvelope/OperationId/cloud/business recovery/Host composition |

PREF-18/19 直接使用接受后的真实 `LocalizationService` 和事件，不复制 locale service。
文件 fault seam 必须 internal 且只服务 production store 的真实步骤；不得 public、
conditional test hook 或修改 asmdef/friend。

## 9. 执行顺序、Unity 串行门与停止条件

1. C 重读签发后的本文、权威设计和规则；验证本文 SHA、两份独立 R、12 路径缺席、
   A/UGUI/source 输入、预算／诊断／test fullname/filter/evidence 所有绑定。
2. 在启动任何 Unity 前重做 process gate。若本项目已由任意 Unity 打开，立即
   `BLOCKED_PROJECT_OCCUPIED`，请用户关闭；不得 kill、attach 或并发启动。
3. C 只创建 6 个 C#；首次由唯一固定 Editor 导入并生成对应 6 个 `.meta`，核路径、
   GUID uniqueness、无额外 asset 变化。
4. 先做静态 whitelist、input SHA、artifact/manifest SHA、budget constant 与 test
   fullname 自检。任一漂移先停，不运行 Unity。
5. 用第 10 节 `compile-run-1` 执行一次 compile/import。exit 非零或 compiler error
   时只在 12 路径内修；源码变化后最多用 `compile-run-2` 再编译一次。不得覆盖 run-1。
6. compile 通过后，以冻结的单一 `{{FOCUSED_TEST_FILTER}}` 跑 `focused-run-1`。
   预期 `{{FOCUSED_EXPECTED_TOTAL}}`，failed/skipped/inconclusive 全 0。
7. 若 run-1 失败，先用 `targeted-repair-1` 只跑实际失败 fixture，修复仍限 12 路径；
   通过后再以完全相同 filter 跑一次 `focused-run-2`。不得改 filter、总数、budget、
   fixture 或断言求绿；第二次完整集仍失败则停止并回传。
8. C 返回 patch/evidence 给主程。主程核实际 diff 后交 QA，由 QA 安排独立 R；C、
   本文作者和主程的自检都不能替代独立 `ACCEPT`。

准确 argv 在签发时把 token 展开为 JSON argv 数组；不得经 shell 重解析。compile：

```text
{{UNITY_EDITOR_PATH}} -batchmode -nographics -quit -projectPath {{PROJECT_ROOT}} -logFile {{CREATE_ONCE_EVIDENCE_ROOT}}/compile-run-1/unity.log
```

聚焦 EditMode（不得加 `-quit`）：

```text
{{UNITY_EDITOR_PATH}} -batchmode -nographics -projectPath {{PROJECT_ROOT}} -runTests -testPlatform EditMode -testFilter {{FOCUSED_TEST_FILTER}} -testResults {{CREATE_ONCE_EVIDENCE_ROOT}}/focused-run-1/tests.xml -logFile {{CREATE_ONCE_EVIDENCE_ROOT}}/focused-run-1/unity.log
```

同一 run 最长分别为 `{{COMPILE_TIMEOUT}}` / `{{EDITMODE_TIMEOUT}}`。超时终止须保留
完整 receipt；不得改为无界等待或自动第三次重跑。

## 10. create-once evidence 闭集

`{{CREATE_ONCE_EVIDENCE_ROOT}}` 在执行前必须不存在。C 首先创建
`evidence-files.tsv`，冻结以下相对路径；未发生的可选 repair/run-2 文件标
`NOT_CREATED_NOT_NEEDED`，不得以其它名称替代：

```text
evidence-files.tsv
binding.json
process-safety.json
accepted-a-inputs.json
current-source-inputs.tsv
new-path-absence.tsv
budgets.json
parser-diagnostics.json
preference-diagnostics.json
test-fullnames.txt
test-filter.txt
static-before/status-short.txt
static-before/name-status.txt
static-before/input-hashes.tsv
static-before/artifact-hashes.tsv
static-before/scope-verdict.json
compile-run-1/argv.json
compile-run-1/exit.txt
compile-run-1/unity.log
compile-run-1/diagnostics.txt
compile-run-2/argv.json
compile-run-2/exit.txt
compile-run-2/unity.log
compile-run-2/diagnostics.txt
focused-run-1/argv.json
focused-run-1/exit.txt
focused-run-1/tests.xml
focused-run-1/unity.log
focused-run-1/summary.json
targeted-repair-1/argv.json
targeted-repair-1/exit.txt
targeted-repair-1/tests.xml
targeted-repair-1/unity.log
targeted-repair-1/summary.json
focused-run-2/argv.json
focused-run-2/exit.txt
focused-run-2/tests.xml
focused-run-2/unity.log
focused-run-2/summary.json
static-after/status-short.txt
static-after/name-status.txt
static-after/diff-check.txt
static-after/new-file-hashes.tsv
static-after/protected-paths.txt
result.json
```

每个 argv 文件保存 executable 与逐项 argument list；exit、XML root counts、Unity
version、起止时间、timeout、候选源码 SHA 和 input SHA 必须相互一致。不得把
`Library/Logs`、聊天摘要或手抄数字作为原始证据。

## 11. 可观察验收

- [ ] 第 0、2 节全部前置与 token 已绑定；A 和 UGUI 各有实际新 evidence 的独立 R
  `ACCEPT`，C 串行槽空闲。
- [ ] changed/new 产品路径精确等于第 4.1 节 12 项；所有既有文件 byte-identical。
- [ ] 4 个 production 类型保持 internal，复用既有 friend；public/API/asmdef/dependency
  零变化。
- [ ] parser 同步、有限预算、不持有输入、不关闭 caller stream；未知长度读取上限精确
  为 `maxArtifactBytes+1`。
- [ ] 八项 budget 的 measured/margin/max 全冻结并写入 production/tests；每项
  limit-1/limit/limit+1 全覆盖。
- [ ] accepted A 真实 bytes 的 SHA/schema/count/locale/key/severity/parameter/COPY/
  formal identity 全部通过，canonical semantic round-trip 无丢失。
- [ ] unknown/case-variant/duplicate fields、duplicate key、trailing、坏 UTF-8、JSON
  depth、stream failure、所有 schema/budget 负例均 fail closed，零 retained input、
  零 partial publication、零 current source replacement。
- [ ] adapter 精确实现现有 `ILocalizedTextSource`，ordinal/no-fallback/parameter/
  severity 语义与既有 binding diagnostic 一致；missing glyph 等仍由现有 binding 处理。
- [ ] locale 首次选择、manual remembered selection、corrupt/unknown/read failure 保留
  原文件并 system fallback，零 repair write。
- [ ] 所有 pre-replace fault 恒 `save_failed`，包括 cleanup/read-back 二次失败；所有
  post-replace 未证明状态为 UNKNOWN；只有 rollback＋reread byte proof 可降级。
- [ ] session 在所有持久化结果下保持新 locale；retry 同 desired 幂等重写但不重复
  locale/rebind/business event。
- [ ] compile exit 0 且无 compiler errors；最终 frozen focused run 精确
  `{{FOCUSED_EXPECTED_TOTAL}}`，failed/skipped/inconclusive 均 0。
- [ ] evidence 是第 10 节 create-once 闭集，内部 hash/argv/result 一致；未运行项明确。
- [ ] production artifact loading、RES-01D、outer admission、Host composition、全量、
  Windows、Android、APK、设备与发行仍明确 `NOT_RUN`／`NOT_VERIFIED`。

## 12. 阻塞、回滚与回传

稳定停止分类：

- `BLOCKED_ON_LOC_A_ACCEPT_AND_C_SERIAL_SLOT`：当前预备态；
- `BLOCKED_INPUT_DRIFT`：design/A/UGUI/source/artifact/manifest/test baseline 任一漂移；
- `BLOCKED_INTERFACE_MISMATCH`：现有 internal contract/friend/asmdef 无法承载本包；
- `BLOCKED_PROJECT_OCCUPIED`：另一个 Unity 正在打开本项目；
- `BLOCKED_SCOPE_EXPANSION`：需要第 13 个路径、现有文件修改、public/dependency/
  scene/prefab/font/Host composition 等越界；
- `FAILED_COMPILE` / `FAILED_FOCUSED_TESTS`：在冻结运行预算内未通过。

回滚只删除第 4.1 节本包新建的 12 项和未接收的本包 create-once evidence；不得改
`FightMatchPlayerHost`、A 产物、COPY、旧 UGUI、玩家资料、ReleaseSet 或其它 WIP。
任何删除须由主程另行明确授权；C 不自行用 Git reset/checkout/stash 清理。

C 回传必须包含：

```text
STATUS: COMPLETED | BLOCKED
PACKET/DESIGN/A/UGUI IDENTITIES:
CHANGED/NEW FILES:
BUDGET TABLE:
TEST FULLNAME RECEIPT AND FROZEN TOTAL:
ACCEPTANCE: PASS | FAIL | NOT VERIFIED — each criterion
VERIFICATION: exact argv, RUN/NOT RUN, exit, counts, duration, evidence path
SELF-CHECK: 12-path scope, protected paths, dependencies, public/API, unrelated diff
NOT_RUN: RES-01D/raw-byte bridge/outer admission/Host composition/full suite/Windows/Android/device/release
BLOCKER: only when blocked
```

主程检查 actual patch 与 evidence 后才可交 QA；QA 再安排独立 R。最终只接受 R 的
一个 `ACCEPT`、`NEEDS_FIX` 或 `REJECT`。`NEEDS_FIX` 必须另签更小纠正包，不把本文
原包整包重跑。

## 13. prepared-01 自检（非实施证据）

### 13.1 精确路径与预算自检

- 新路径：12/12，精确为 4 production C#＋2 test C#＋6 `.meta`；无重复路径。
- 既有文件修改额度：0；asmdef、Host、Config/Tools/Generated、scene/prefab/font、
  StreamingAssets、YooAsset、Packages、ProjectSettings 变更额度均为 0。
- parser budget 维度：8/8，分别为 artifact bytes、records、key chars、单 locale
  template chars、单 record parameter count、parameter name chars、total decoded chars、
  JSON depth；每项要求 limit-1/limit/limit+1。
- locale 集精确为 2：`en`、`zh-Hans`；不新增第三语言或 alias。

### 13.2 测试矩阵自检

- catalog matrix：CAT-01～CAT-26；CAT-19/20 必须在签发 fullname receipt 中展开预算
  leaf cases，不能把 26 当成实际 test count。
- preference matrix：PREF-01～PREF-21；pre/post-replace、rollback proof、session、retry
  与 corrupt recovery 均有独立观察点。
- 实际 `{{NEW_TEST_COUNT}}`、`{{UGUI_ACCEPTED_BASELINE_COUNT}}`、
  `{{FOCUSED_EXPECTED_TOTAL}}` 当前故意未绑定；不得从 2 个文件、47 个矩阵 ID 或
  parameterized method 数推算。

### 13.3 当前未绑定字段

所有第 2 节 token，以及第 6 节 measured/margin/max token，仍为 dispatch-time
占位符。尤其未绑定：LOC-A canonical/manifest/formal-source identity、A 独立 R
final、UGUI 独立 R final、当前 Host/Presentation/test asmdef/source SHA、parser 与
preference diagnostic tables、八项 exact budgets、accepted UGUI fullname baseline、
new test fullname/count、最终 filter/total、Unity identity/process gate、timeouts 和
create-once evidence root。

### 13.4 当前 NOT_RUN

```text
NOT RUN — product/test/.meta implementation
Reason: LOC-A 尚未获实际 patch＋新 evidence 的原独立 R ACCEPT，C 的 UGUI 串行槽尚未释放。

NOT RUN — Unity compile / focused EditMode
Reason: 本文仅准备任务包且没有实施授权；输入 SHA、预算、filter、总数和 evidence root 尚未绑定。

NOT RUN — production artifact loading / RES-01D fm.text.full / outer ReleaseSet admission / Host composition
Reason: 明确属于后续包，不得由 LOC-IMPL-B 模拟。

NOT RUN — full suite / Windows / Android / APK / device / release
Reason: 不属于本包，且相应后续门未满足。
```

**prepared-01 结论：`PREPARED_NOT_AUTHORIZED / BLOCKED_ON_LOC_A_ACCEPT_AND_C_SERIAL_SLOT`。**
