# LOC-IMPL-B-PREF · locale preference v1 精确实施包

2026-10-01 · revision `prepared-01`

## 0. 包状态与责任

| 字段 | 固定值 |
| --- | --- |
| Task | `LOC-IMPL-B-PREF` |
| Status | `PREPARED_NOT_AUTHORIZED / BLOCKED_ON_UGUI_ACCEPT_AND_C_SERIAL_SLOT` |
| Execution | `NOT_RUN` |
| 唯一未来 delivery owner | C，thread `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / host `local` |
| 签发与回传 | 主程，thread `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / host `local` |
| 包准备者 | 系统设计，thread `01a0f2e6-29bb-7092-abf0-705b41b7bf93` / host `local` |
| 专业接收 | QA／主测试在签发时绑定；作者、主程和 C 不得接受自己的交付 |
| 上游 authority | 中央依赖驱动调度指令、`AGENTS.md` 五角色流程、主程对拆分方案一的本次接收 |

本文只准备 `LOC-B-PREF` 的精确执行包，不是实施授权。只有以下条件全部满足，
主程才可复制本 revision、绑定第 3 节全部 dispatch token、重新计算包身份，将状态
改为 `AUTHORIZED_FOR_C` 并发送给 C：

1. UGUI-01 实际 patch、资源与 fresh evidence 已由独立 R 给出唯一 `ACCEPT`；
2. 第 3.2 节最终 localization／程序集／Host tests 输入均来自上述接受候选；
3. C 已释放本项目唯一 Unity 串行槽，并收到本 revision 的准确 dispatch turn；
4. 第 5.1 节 6 个新路径仍全部不存在；
5. QA／主测试已绑定准确 leaf fullname、filter、count、create-once evidence root
   和独立 R reviewer；
6. Unity 进程门证明本项目未被任何其它 Unity 实例打开。

任一 token 未绑定、输入漂移、目标路径已存在、项目被占用或需要扩大白名单时，
C 必须返回 `BLOCKED`，不得先写代码再补签。

本包**不以 LOC-A 为前置**。LOC-A artifact、manifest、runner、license、M/B/L、
material、build、canonical field table、JSON terminator 和 parser budgets 均不得被
加入本包的签发门、实现输入、测试 fixture 或 evidence。

准备本文期间没有运行 Unity，没有创建产品／测试／`.meta`／evidence，没有修改
原 `engineering-localization-luban-impl-b.md`，也没有进行 Git、下载、安装、发布
或外部模型操作。

## 1. 唯一目标与结果边界

本包交付一个深的 locale preference module：调用者只通过小型 internal interface
加载初始 locale 或保存 desired locale；文件路径、严格 JSON、同目录 temp、Flush、
pre-verify、atomic commit、post-verify、rollback proof 和受控诊断全部留在
implementation 内部。

本包只交付：

1. internal `ILocalePreferenceStore` interface 与最小结果类型；
2. production `FileLocalePreferenceStore` implementation；
3. internal file-operation seam 及 production adapter，供同一真实状态机注入 fault；
4. `LocalePreferenceStoreTests`，完整覆盖 PREF-01～PREF-21；
5. 本包 compile、聚焦 EditMode 和静态 scope 的 create-once evidence。

本包不交付：

- catalog parser、localized text source adapter 或任何 LOC-A artifact 消费；
- `FightMatchPlayerHost`／`FightMatchHostView` 接线或 LanguagePopup composition；
- YooAsset、RES-01D、`fm.text.full` raw-byte bridge、ReleaseSet 或 outer admission；
- PlayerSave、SaveEnvelope、OperationId、业务恢复、cloud save 或账号同步；
- full suite、Windows、Android、APK、设备、发行或跨重启产品验收；
- Unity Localization、Addressables、Luban runtime 或 generated C#。

即使本包独立 R `ACCEPT`，也只证明 locale preference v1 module 与真实
`LocalizationService` 的 session harness 行为；不证明 catalog、资源加载、Host
composition、玩家设置入口或端到端双语已经完成。

## 2. 固定设计输入

### 2.1 权威输入

| 输入 | 固定身份／用途 |
| --- | --- |
| `docs/team/2026-09-30/engineering-localization-luban-001.md` | SHA-256 `022c060801df22520653856c24bd78dda394deb26f946dbff1b70ec9c338380a`；§7.2、§9.2、§9.4、§10 |
| `docs/team/2026-09-30/engineering-localization-luban-impl-b.md` | SHA-256 `7941c42e99d0552daf5241de156da415c902f1ee11eac277a2396dc82487a795`；复用 PREF 合同，不继承 LOC-A/catalog 前置 |
| `docs/team/2026-09-30/engineering-localization-luban-impl-b-dependency-split.md` | SHA-256 `9265bb9f16fcf250100945407cd19e7a63827479938f841214a5a3066340128b`，467 lines / 24,299 bytes；已接收方案一 |
| `docs/team/2026-09-30/planning.md` | SHA-256 `35c51adca0eeb34155ad6e392c347e652ca24e8979f926074a229b9dbedb68f5`；AMEND-04 UNKNOWN 语义 |
| `docs/system-design/2026-09-17/runtime-ui-ugui-correction.md` | runtime UI 为 uGUI；本包只消费已接受 localization interface |
| `docs/team/2026-09-30/engineering-ugui-01-implementation.md` | 当前文档 SHA-256 `ccb7ed615cf9d688f61a19620ff4619ea2c43be75c44f2820a916622552ea623`；执行时必须另绑定实际接受候选与 R receipt |

如固定设计输入内容或身份变化，终态为 `BLOCKED_INPUT_DRIFT`。主程先判断变化是否
只更新 receipt，还是需要系统设计复核；C 不自行调和冲突。

### 2.2 当前观察值不是签发值

准备本包时观察到：

- `LocaleId.cs` SHA-256
  `f5338ff0fdde20a362281380a7d84183f552ad23a7964edb51d042fca48174a5`；
- `LocalizationService.cs` SHA-256
  `ca8b2c5f9b8afd864410b35f7ab9998c04a7b5ea9efdf3a51e8082b6dd54a408`；
- 第 5.1 节 6 个目标路径全部不存在。

这些只是 2026-10-01 的只读观察值。UGUI 尚未独立 `ACCEPT` 前，它们不得被写进
签发后的 accepted source receipt，也不得作为 C 开始执行的依据。

## 3. 签发时必须绑定的 dispatch token

主程必须在独立的签发 revision 中把以下 `{{...}}` 全部替换为真实观测值，并重新
记录该 revision 的 SHA-256、lines、bytes。token 不得只在聊天中口头补充。

### 3.1 authority、owner 与 Unity 运行门

| Token | 必须绑定的真实值 |
| --- | --- |
| `{{DISPATCH_AUTHORITY_RECEIPT}}` | 中央调度、主程签发 turn 与本包授权状态 |
| `{{C_DISPATCH_TURN_ID}}` | C 实际收到同一签发 revision 的 turn id |
| `{{QA_THREAD_ID}}` / `{{QA_HOST_ID}}` | 主测试／QA 身份 |
| `{{INDEPENDENT_R_THREAD_ID}}` / `{{INDEPENDENT_R_HOST_ID}}` | 本包未来实际 patch reviewer；不得是作者或 C |
| `{{UGUI_ACCEPT_R_THREAD_TURN_FINAL}}` | UGUI 最终实际候选的独立 R 身份、final 与唯一 `ACCEPT` |
| `{{UGUI_ACCEPTED_CANDIDATE_SHA256}}` | UGUI 接受候选的完整 source/resource identity |
| `{{PROJECT_ROOT}}` | canonical absolute FightMatch 根路径 |
| `{{UNITY_EDITOR_PATH}}` | Unity `2022.3.18f1` Mac Intel executable 绝对路径 |
| `{{UNITY_EDITOR_IDENTITY}}` | 实测 version、architecture、executable SHA-256 与安装 receipt |
| `{{UNITY_PROCESS_GATE_RECEIPT}}` | 时间戳化检查；签发时和每次 Unity 启动前均证明本项目未被其它 Unity 打开 |
| `{{C_SERIAL_SLOT_RECEIPT}}` | C 是唯一 Unity executor、该时段无冲突写入的 receipt |

### 3.2 最终 UGUI source identities

签发时生成 `{{CURRENT_SOURCE_INPUT_RECEIPT}}`，逐文件记录 SHA-256、bytes、是否来自
UGUI 接受候选及读取时间。至少绑定：

| 只读路径 | SHA-256 / bytes token |
| --- | --- |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocaleId.cs` | `{{SHA256_LOCALE_ID}}` / `{{BYTES_LOCALE_ID}}` |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocalizationService.cs` | `{{SHA256_LOCALIZATION_SERVICE}}` / `{{BYTES_LOCALIZATION_SERVICE}}` |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTextSource.cs` | `{{SHA256_LOCALIZED_TEXT_SOURCE}}` / `{{BYTES_LOCALIZED_TEXT_SOURCE}}` |
| `Assets/Scripts/FightMatch/Presentation/PresentationAssemblyInfo.cs` | `{{SHA256_PRESENTATION_FRIEND}}` / `{{BYTES_PRESENTATION_FRIEND}}` |
| `Assets/Scripts/FightMatch/Presentation/FightMatch.Presentation.asmdef` | `{{SHA256_PRESENTATION_ASMDEF}}` / `{{BYTES_PRESENTATION_ASMDEF}}` |
| `Assets/Scripts/FightMatch/Host/HostAssemblyInfo.cs` | `{{SHA256_HOST_FRIEND}}` / `{{BYTES_HOST_FRIEND}}` |
| `Assets/Scripts/FightMatch/Host/FightMatch.Host.asmdef` | `{{SHA256_HOST_ASMDEF}}` / `{{BYTES_HOST_ASMDEF}}` |
| `Assets/Tests/EditMode/FightMatchHost/FightMatch.Host.Tests.asmdef` | `{{SHA256_HOST_TEST_ASMDEF}}` / `{{BYTES_HOST_TEST_ASMDEF}}` |
| `Assets/Tests/EditMode/FightMatch/LocalePolicyTests.cs` | `{{SHA256_LOCALE_POLICY_TESTS}}` / `{{BYTES_LOCALE_POLICY_TESTS}}` |

`LocaleId`、`LocalePolicy`、`LocalizationService.SetLocale`、friend access 或 Host tests
assembly 引用若不能承载第 6～8 节合同，返回 `BLOCKED_INTERFACE_MISMATCH`。禁止复制
interface、使用 reflection、增加 friend、修改 asmdef 或扩大 public surface。

### 3.3 tests、evidence 与路径缺席

| Token | 必须绑定的真实值 |
| --- | --- |
| `{{UGUI_ACCEPTED_FULLNAME_RECEIPT}}` | 接受 UGUI baseline XML 的完整相关 fullname 清单及 SHA |
| `{{UGUI_ACCEPTED_BASELINE_COUNT}}` | 上述 baseline 的实际 passed/failed/skipped/inconclusive；不得抄历史预测 |
| `{{PREF_TEST_FULLNAME_RECEIPT}}` | 第 9 节 PREF-01～21 展开后的每个 NUnit leaf fullname 与参数 |
| `{{PREF_NEW_TEST_COUNT}}` | 上述 receipt 的真实 leaf 总数；不得从 method 或矩阵行数推算 |
| `{{FOCUSED_TEST_FILTER}}` | 接受 UGUI 相关 baseline 与全部 preference fullname 组成的单行冻结 filter |
| `{{FOCUSED_EXPECTED_TOTAL}}` | filter 去重后的真实 leaf 总数 |
| `{{CREATE_ONCE_EVIDENCE_ROOT}}` | `TestArtifacts/FightMatch/LOC-IMPL-B-PREF/{{UGUI_ACCEPTED_CANDIDATE_SHA256}}/`；执行前不存在 |
| `{{NEW_PATH_ABSENCE_RECEIPT}}` | 第 5.1 节 6 路径签发前全部不存在的时间戳化 receipt |

实际 leaf names、filter 和 counts 故意留到签发：它们必须来自接受后的 UGUI XML、
最终 test source 和本包准确参数化展开，不能从当前 WIP 或 PREF 编号猜测。

## 4. 允许读取的最小上下文

C 可读取：

- `AGENTS.md`、`START_HERE.md`、`.agent/PROJECT_CONTEXT.md`、
  `.agent/CODING_RULES.md`、`.agent/VALIDATION.md`、`.agent/TEAM_WORKFLOW.md`；
- 第 2.1 节固定设计输入及签发后的本文；
- `{{UGUI_ACCEPT_R_THREAD_TURN_FINAL}}` 指定的 UGUI receipt、Q4 XML/log 与 source
  identity receipt；
- 第 3.2 节全部只读源码、测试、friend 和 asmdef；
- 第 5.1 节新建路径的父目录，仅用于创建白名单文件；
- `{{CREATE_ONCE_EVIDENCE_ROOT}}` 的父目录，仅用于确认新根不存在并创建闭集。

读取权限不等于写权限。C 不得读取 LOC-A artifact 来影响实现，也不得重跑 UGUI、
修改其 evidence、把 UGUI 的历史预测 count 当作 accepted baseline。

## 5. 唯一写白名单与 protected changes

### 5.1 只允许新建的 6 个产品／测试路径

```text
Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs
Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs.meta
Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs
Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs.meta
Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs
Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs.meta
```

精确为 2 个 production C#、1 个 test C# 与对应的 3 个 `.meta`。不得修改任何既有
文件。`.meta` 只能由绑定的 Unity Editor 首次导入生成，
C 核对 GUID 唯一；不得手写 YAML、复用 GUID 或由第二个 Unity 实例生成。

最大产品范围：6 个新路径；C# 总新增建议不超过 1,100 行。超过 6 路径或明显超过
预算必须先返回 `BLOCKED_SCOPE_EXPANSION`，由主程判断是否需要更小 corrective
packet；C 不自行加 helper 文件。

### 5.2 evidence 唯一写域

除第 5.1 节外，只允许在 `{{CREATE_ONCE_EVIDENCE_ROOT}}` 创建第 11 节闭集。
该根 create-once、append-by-creation；不得覆盖旧 evidence 或改写别包结果。
Unity 正常产生的 `Library/`、`Temp/`、`Logs/` 副作用不是交付物，不得手工编辑、
清理或用来存放本包唯一证据。

### 5.3 明确禁止

- 禁止修改 `FightMatchPlayerHost.cs`、`FightMatchHostView.cs`、任何既有 production/
  test、asmdef、friend、scene、Prefab、font、material 或 `.meta`；
- 禁止修改 Core、Application、Save、Platform、Content、Input、Presentation；
- 禁止修改 Config、Tools、Generated、StreamingAssets、YooAsset、Packages、
  ProjectSettings、文档、旧 evidence 或 Git；
- 禁止新增 package/dependency、Unity Localization、Addressables、Luban runtime、
  generated DTO、runner、日志框架、重试调度器或 service locator；
- 禁止新增／修改 public API、Unity serialized field、PlayerSave、SaveEnvelope、
  ReleaseSet、cloud schema、CI、build script 或 release setting；
- 禁止删除、移动、重命名、格式化或顺手修复白名单外文件；
- 禁止 Host composition、LanguagePopup 接线、production startup load 或 UI popup。

### 5.4 protected-change 权限表

| 类型 | 允许 | 精确范围 |
| --- | ---: | --- |
| 新 internal interface/result/type | YES | 只在 2 个 production C#；interface 保持第 6 节最小 seam |
| internal file-operation seam | YES | 只在 `FileLocalePreferenceStore.cs`；production 与 scripted-test adapter 两个真实用途 |
| 新 public API | NO | 0 |
| 新 preference serialization | YES | 仅独立 v1 文件；精确 schema 见第 7 节 |
| 修改现有 serialization | NO | PlayerSave／SaveEnvelope／cloud／ReleaseSet 均 0 |
| asmdef/friend/dependency/config | NO | 0 |
| scene/prefab/font/resource | NO | 0 |
| 新 `.meta` | YES | 仅第 5.1 节 3 个新 C# 对应 `.meta` |
| Git mutation | NO | 不 branch/commit/push/merge/rebase/reset/checkout/stash |

## 6. Module、interface 与 internal seam

### 6.1 外部 interface

`ILocalePreferenceStore` 是唯一 external seam，所有类型保持 `internal`。签发后的
实现精确使用以下 type/member 名，不由 C 另行设计：

```text
internal interface ILocalePreferenceStore
    LocalePreferenceLoadResult Load(SystemLanguage systemLanguage)
    LocalePreferenceSaveResult Save(LocaleId desiredLocale)

internal enum LocalePreferenceLoadDisposition
    Remembered
    MissingSystemDefault
    InvalidSystemDefault
    ReadFailedSystemDefault

internal sealed class LocalePreferenceLoadResult
    LocaleId Locale
    LocalePreferenceLoadDisposition Disposition
    string DiagnosticCode

internal enum LocalePreferenceSaveDisposition
    Saved
    SaveFailed
    Unknown

internal sealed class LocalePreferenceSaveResult
    LocalePreferenceSaveDisposition Disposition
    string LocalizationKey
    string ErrorCode
```

这些声明放在 `ILocalePreferenceStore.cs`；构造器／factory 只允许建立满足下列不变量
的结果，不暴露可变 setter。最小结果合同：

- load disposition 只需区分 remembered、missing-system-default、invalid-system-default、
  read-failed-system-default；
- save disposition 精确为 `Saved`、`SaveFailed`、`Unknown`；
- `Saved` 的 key 仅为 `fm.language.saved` 且 `errorCode == null`；
- `SaveFailed` 的 key 仅为 `fm.language.save_failed` 且有受控 `errorCode`；
- `Unknown` 的 key 仅为 `fm.language.save_unknown` 且有受控 `errorCode`；
- 结果不暴露 path、exception、stack、raw message、原始 bytes、temp/backup 名称或
  `LocaleId` enum 文本；
- unknown/invalid `LocaleId` 传给 `Save` 时抛 `ArgumentOutOfRangeException`，不创建
  文件、不产生伪保存结果。

不得增加第三种 external 方法、异步 interface、callback、event、global singleton
或 static mutable state。internal file-operation seam 的成员只服务第 8.2 节真实
步骤，不得进入 `ILocalePreferenceStore`。若现有 UGUI interface 需要不同调用形状，停止为
`BLOCKED_INTERFACE_MISMATCH`，不得由实现者兼容两套 seam。

### 6.2 internal file-operation seam

文件 fault seam 只属于 `FileLocalePreferenceStore` implementation：

- 位于同一个 `FileLocalePreferenceStore.cs`；
- internal，不 public、不 conditional compile、不 reflection、不 `#if UNITY_EDITOR`；
- production adapter 执行真实 directory/file/open/read/write/flush/replace/move/delete；
- scripted test adapter 驱动真实状态机的指定 stage failure 和 bytes 状态；
- tests 只通过 store interface 和可观察 adapter transcript 断言，不复制状态机；
- seam 不暴露给 Presentation、Host composition 或未来 caller；
- production default constructor 才组合
  `Application.persistentDataPath/FightMatch/settings/locale-preference-v1.json`；
- 测试构造只注入 temp root 和 file-operation adapter，不读取真实玩家目录。

这是 production/test 两个 adapter 支撑的真实 seam，不得再包一层 pass-through
repository、serializer interface、clock、logger 或 dependency container。

### 6.3 同步与所有权

- 全部调用同步完成；不创建 Task、coroutine、thread 或 background I/O；
- store 不拥有 `LocalizationService`，不触发 UI、locale、rebind 或 business event；
- caller 拥有 session 顺序；测试 harness 使用已接受的真实 `LocalizationService`；
- implementation 不缓存玩家文件原始 bytes，不保留 caller 可变 buffer；
- 同一 store 不承诺跨进程锁或并发写；本包通过 C 单进程串行使用证明合同，不新增
  锁文件或全局 mutex。

## 7. locale preference v1 serialization 与 load 合同

### 7.1 唯一路径与 schema

production 唯一路径：

```text
Application.persistentDataPath/FightMatch/settings/locale-preference-v1.json
```

唯一逻辑 schema：

```json
{ "version": 1, "locale": "en|zh-Hans" }
```

合同：

- root 必须是单一 JSON object；
- `version`、`locale` exact-case、必选且各出现一次；
- `version` 必须是整数 `1`；
- `locale` 只接受 exact ordinal `en` 或 `zh-Hans`；
- unknown、case variant、duplicate、null、wrong type、第二 root、非空 trailing token/
  byte、bad UTF-8、bad JSON 全部 invalid；
- writer 输出确定、UTF-8 无 BOM、仅两个字段，不写 enum name 或额外 metadata；
- writer 固定字段顺序为 `version` 后 `locale`，使用 compact JSON、无末尾换行；reader
  可接受 JSON grammar 允许的 token 间 whitespace，但 root 结束后的任何额外 byte
  均按 trailing 拒绝；
- 文件不是第二个人工编辑权威，也不加入 PlayerSave／SaveEnvelope。

### 7.2 Load

- target missing：调用现有 `LocalePolicy.FromSystemLanguage`；Chinese、
  ChineseSimplified、ChineseTraditional → `LocaleId.ZhHans`，其它包括 Unknown →
  `LocaleId.En`；零写入；
- 合法 target：remembered manual locale 覆盖 system language；
- corrupt JSON、bad UTF-8、unknown/duplicate/trailing、unknown version/locale：保留
  target 原 bytes，本 session 采用 system locale，返回稳定 invalid diagnostic；
- open/read I/O failure：保留 target，不重试、不覆盖，本 session 采用 system locale，
  返回稳定 read-failed diagnostic；
- load 绝不 repair、delete、rename、write temp 或写回默认值。

## 8. Save、session、commit 与 rollback 合同

### 8.1 caller 的 session 顺序

模块验收 harness 必须精确：

1. 用户选择 `desiredLocale`；
2. caller 先调用真实 `LocalizationService.SetLocale(desiredLocale)`；
3. 只有 locale 真变化时，`CurrentLocale` 先更新并触发恰好一次 `LocaleChanged`；
4. caller 再调用 store `Save(desiredLocale)`；
5. 无论 `Saved`、`SaveFailed` 或 `Unknown`，当前 session 保持 desired locale；
6. 同 desired locale retry 重新执行完整 Save，但 `SetLocale` 返回 false，不重复
   locale/rebind/business event；最后只有 post-verify 成功才是 `Saved`。

本包只在 tests 中组合上述 harness，不修改 Host 或 UI。

### 8.2 固定写入状态机

1. 确保 settings directory；在 target 同目录创建本次唯一 temp；
2. 写完整 canonical v1 bytes，Flush 到稳定存储；
3. 重新打开 temp，读取、strict parse 并逐字段 pre-verify desired locale；
4. commit 前记录旧 target 是否存在；存在则保留 exact old bytes/backup 事实，不存在
   则保留 old-absence 事实；
5. 使用同目录 atomic replace（已有 target）或 same-volume atomic rename/move（原
   target absent）完成 commit；
6. commit 返回成功后重新打开 target、读取、strict parse 并逐字段比较 version 与
   desired locale；全成功才返回 `Saved`；
7. post-commit verify 失败时可尝试 rollback；只有 rollback 操作成功，且随后重新
   open/read 逐 byte 证明 old bytes，或重新证明 old absence，才降为 `SaveFailed`；
8. 所有路径尽力清理本次 temp/backup；cleanup 结果只作受限诊断，不得改写已经由
   commit 边界决定的主结果。

implementation 必须明确记录 commit 是否已确认成功。测试 fault seam 不得在一个
stage 同时隐式改变多个事实；每个 fault case 应能唯一证明预期分支。

### 8.3 三态结果

| 状态 | player-visible key | 精确条件 |
| --- | --- | --- |
| `Saved` | `fm.language.saved` | commit 后 target reopen/read/strict parse/逐字段 compare 全成功 |
| `SaveFailed` | `fm.language.save_failed(errorCode)` | commit 未发生，或 post-commit rollback 已由 reopen/read byte proof 证明旧 bytes／absence |
| `Unknown` | `fm.language.save_unknown(errorCode)` | commit 已发生但 desired 后验不可证明，且旧 bytes／absence 也未获 rollback proof |

pre-commit temp create/write、Flush、temp pre-verify open/read/parse/compare、
commit-before-mutation failure 恒为 `SaveFailed`。随后 cleanup 或旧 target read-back
失败不得升级为 `Unknown`。

commit 成功后的 target reopen/read/parse/version/locale mismatch 先进入 `Unknown`。
rollback 失败、rollback 后 reopen/read 失败、bytes 不同或 absence 未证明都保持
`Unknown`。`Unknown` 永不用于 pre-commit fault。

`SaveFailed` 与 `Unknown` 均不改变 PlayerSave、SaveEnvelope、OperationId、业务
unknown、SaveFailed Retry、cloud 或其它业务状态。

### 8.4 冻结 preference diagnostic table

以下 code 均为 stable internal ASCII token；不得拼 path、locale enum、exception
type/message 或 bytes。一个结果只暴露一个 primary code，cleanup 等 secondary
failure 只进入受限 test transcript／operator diagnostic。

| Stage | Primary code | 结果 |
| --- | --- | --- |
| load open/read I/O | `PreferenceReadFailed` | system fallback，文件不变 |
| load invalid UTF-8/JSON/root/trailing | `PreferenceInvalidDocument` | system fallback，文件不变 |
| load field missing/null/type/unknown/duplicate/case | `PreferenceInvalidSchema` | system fallback，文件不变 |
| load version != 1 | `PreferenceUnsupportedVersion` | system fallback，文件不变 |
| load locale 非 `en`/`zh-Hans` | `PreferenceUnsupportedLocale` | system fallback，文件不变 |
| directory/temp create | `PreferenceTempCreateFailed` | `SaveFailed` |
| temp write | `PreferenceTempWriteFailed` | `SaveFailed` |
| Flush | `PreferenceFlushFailed` | `SaveFailed` |
| temp reopen/read | `PreferenceTempReadFailed` | `SaveFailed` |
| temp strict parse/compare | `PreferenceTempVerificationFailed` | `SaveFailed` |
| commit-before-mutation | `PreferenceCommitFailed` | `SaveFailed` |
| post-commit target reopen | `PreferenceTargetReopenFailed` | `Unknown`，除非 rollback proof |
| post-commit target read | `PreferenceTargetReadFailed` | `Unknown`，除非 rollback proof |
| post-commit parse/version/locale mismatch | `PreferenceTargetVerificationFailed` | `Unknown`，除非 rollback proof |
| rollback operation failed | `PreferenceRollbackFailed` | `Unknown` |
| rollback proof reopen/read failed | `PreferenceRollbackProofFailed` | `Unknown` |
| rollback proof bytes/absence mismatch | `PreferenceRollbackMismatch` | `Unknown` |

当 post-commit primary failure 经 rollback proof 降为 `SaveFailed` 时，保留原
post-commit code 作为 `errorCode`，不得伪装成 pre-commit failure；结果类型和可见
key负责表达已证明旧值恢复。

## 9. 冻结测试矩阵 PREF-01～PREF-21

每行至少形成一个独立可观察 leaf；参数化行按实际输入展开。签发时把每个真实
fullname 与参数写入 `{{PREF_TEST_FULLNAME_RECEIPT}}`，不得用一个循环中的多个
Assert 冒充 leaf 清单，也不得从 21 个 ID 推算总数。

| ID | 必测输入／fault stage | 可观察结果 |
| --- | --- | --- |
| PREF-01 | missing＋Chinese/ChineseSimplified/ChineseTraditional | 首次均为 `zh-Hans`；零写入 |
| PREF-02 | missing＋English/Japanese/Unknown/其它 | 首次均为 `en`；零写入 |
| PREF-03 | 合法 `en` 与 `zh-Hans` | remembered selection 覆盖 system locale |
| PREF-04 | corrupt JSON／bad UTF-8／unknown field／duplicate/trailing | 原 bytes 不变；system fallback；零 repair write |
| PREF-05 | unknown version／unknown locale／read I/O failure | 原文件保留；system fallback；零 repair write |
| PREF-06 | 完整成功路径 | temp/Flush/pre-verify/commit/reopen/read/parse/compare 顺序正确，`Saved` key 精确 |
| PREF-07 | temp create/write failure | `SaveFailed`；session desired locale 保持 |
| PREF-08 | Flush failure | `SaveFailed`；session desired locale 保持 |
| PREF-09 | temp pre-verify open/read/parse/mismatch | 各自 `SaveFailed`；target 未被流程修改 |
| PREF-10 | commit-before-mutation failure | `SaveFailed`；target 未被流程修改 |
| PREF-11 | PREF-07～10 后 cleanup failure／旧 target read-back failure | 仍为 `SaveFailed`，绝不 `Unknown` |
| PREF-12 | commit 后 target reopen failure | `Unknown`＋`fm.language.save_unknown`；不得声称旧值保留 |
| PREF-13 | commit 后 read failure | `Unknown`＋`fm.language.save_unknown` |
| PREF-14 | commit 后 parse/version/locale mismatch | 各自 `Unknown`＋`fm.language.save_unknown` |
| PREF-15 | rollback 成功且 reread byte-equal old target | 降为 `SaveFailed`；old bytes proof 存在 |
| PREF-16 | 原 target absent，rollback 后再次证明 absent | 降为 `SaveFailed`；old absence proof 存在 |
| PREF-17 | rollback 失败／proof reopen/read 失败／bytes 或 absence mismatch | 各自保持 `Unknown` |
| PREF-18 | `SaveFailed` 与 `Unknown` | `CurrentLocale` 均保持 desired；恰好一次 locale event/rebind |
| PREF-19 | 同 desired retry，首轮失败后次轮成功 | 完整 storage flow 重做；零重复 locale/rebind/business event；最后 `Saved` |
| PREF-20 | 全诊断矩阵 | 每 stage 精确匹配第 8.4 节；无 path/message/input/enum 泄漏 |
| PREF-21 | 边界静态检查 | 无 PlayerSave/SaveEnvelope/OperationId/cloud/business recovery/Host composition/catalog/LOC-A |

测试要求：

- PREF-18/19 使用 UGUI 接受后的真实 `LocalizationService` 和事件；不复制 locale
  service，不新增 fake `LocalizationService`；
- fault adapter 只控制第 8.2 节真实 file-operation stage，并记录调用 transcript；
- production adapter 至少有 temp-root 正向 round-trip，证明真正 bytes/schema；
- 每个测试创建独立 temp root，结束只清理自己的 root；不读写
  `Application.persistentDataPath` 或真实玩家资料；
- 测试不以另一个宽松 JSON parser 作为 oracle；期望由冻结 schema 和观察 bytes
  明确断言；
- 不使用 sleep、网络、随机重试、时间依赖或参数顺序不稳定的集合遍历。

## 10. 执行顺序、验证命令与资源上限

### 10.1 签发后执行顺序

1. C 重读签发 revision 与规则，验证 packet SHA、UGUI R `ACCEPT`、source receipt、
   6 路径缺席、filter/count、evidence root 和全部 token；任一不符先 `BLOCKED`。
2. 在启动 Unity 前重做 process gate。若本项目被任一 Unity 打开，返回
   `BLOCKED_PROJECT_OCCUPIED`；不得 kill、attach 或并发启动。
3. C 只创建 3 个 C#；由唯一固定 Editor 首次导入并生成对应 3 个 `.meta`；核 GUID
   唯一、路径精确、没有额外 asset 变化。
4. 写完后先做静态 scope、input SHA、diagnostic table 与 test fullname 自检。发现
   输入漂移或第 7 个路径时停止，不启动 Unity 验证。
5. 执行 `compile-run-1`。exit 非零或 compiler error 时只在 6 路径内修；源码变化
   后最多执行一次 `compile-run-2`，不得覆盖 run-1。
6. compile 通过后用冻结 `{{FOCUSED_TEST_FILTER}}` 执行 `focused-run-1`；实际总数必须
   等于 `{{FOCUSED_EXPECTED_TOTAL}}`，failed/skipped/inconclusive 全 0。
7. 若 focused-run-1 失败，只允许一次 `targeted-repair-1` 运行实际失败 fixture；
   修复仍限 6 路径。通过后用完全相同 filter 执行一次 `focused-run-2`。不得修改
   filter、expected total、诊断合同或断言求绿；完整集再次失败即停止。
8. C 返回 patch 与 evidence。主程检查 actual diff 后交 QA；QA 安排第 3.1 节绑定的
   independent R。只有 R 的唯一 `ACCEPT` 才能接收本包。

### 10.2 准确命令形状

所有 argv 在签发时展开成 JSON argument array，直接启动 executable，不经 shell
重解析。compile：

```text
{{UNITY_EDITOR_PATH}} -batchmode -nographics -quit -projectPath {{PROJECT_ROOT}} -logFile {{CREATE_ONCE_EVIDENCE_ROOT}}/compile-run-1/unity.log
```

聚焦 EditMode；不得添加 `-quit` 或 `-nographics`。该 filter 包含已接受的真实 uGUI
回归，必须沿用其非 Null 图形设备条件；日志须证明 renderer 非 Null：

```text
{{UNITY_EDITOR_PATH}} -batchmode -projectPath {{PROJECT_ROOT}} -runTests -testPlatform EditMode -testFilter {{FOCUSED_TEST_FILTER}} -testResults {{CREATE_ONCE_EVIDENCE_ROOT}}/focused-run-1/tests.xml -logFile {{CREATE_ONCE_EVIDENCE_ROOT}}/focused-run-1/unity.log
```

run-2 使用对应新目录，参数其余完全相同；targeted repair 只替换为失败 fixture 的
冻结 filter，另存 `targeted-repair-1/`。

### 10.3 资源、进程与尝试上限

- 同一时刻最多 1 个 Unity Editor／batchmode 进程，且只能由 C 启动；
- 不并发运行 compile、tests、asset import 或其它 Unity 项目操作；
- compile 每次上限 5 分钟，最多 2 次；
- focused full run 每次上限 10 分钟，最多 2 次；
- targeted repair 上限 5 分钟，最多 1 次；
- 每次 run 结束后确认原 PID 退出，再启动下一次；尾进程未退出即停止并保留证据；
- 禁止 full suite、Player build、Android/Windows、下载、restore、package install、
  LOC-A、RES-01D 或无界等待；
- 超时只保留当次 evidence 并终止本包，不自动第三次重跑；不得用提高上限掩盖挂起。

## 11. create-once evidence 闭集

`{{CREATE_ONCE_EVIDENCE_ROOT}}` 执行前必须不存在。C 首先创建
`evidence-files.tsv`，冻结以下相对路径。未发生的可选 run-2/repair 文件在
`evidence-files.tsv` 与 `result.json` 标记 `NOT_CREATED_NOT_NEEDED`；不得创建其它
名称替代。

```text
evidence-files.tsv
binding.json
process-safety.json
accepted-ugui.json
current-source-inputs.tsv
new-path-absence.tsv
preference-diagnostics.json
test-fullnames.txt
test-filter.txt
static-before/status-short.txt
static-before/name-status.txt
static-before/input-hashes.tsv
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

证据约束：

- `binding.json` 记录 packet SHA、authority、C/QA/R identity、UGUI ACCEPT 与所有
  dispatch token；不含 LOC-A identity；
- `process-safety.json` 记录签发检查、每次 run 前检查、PID、开始/结束和尾进程；
- `accepted-ugui.json` 绑定 UGUI R receipt、candidate identity 与 baseline XML；
- `preference-diagnostics.json` 与第 8.4 节逐项相等；
- `test-fullnames.txt` 一行一个实际 leaf fullname；`test-filter.txt` 保存准确单行参数
  与去重总数；
- 每个 argv 文件保存 executable 和逐项 argument list；每个 exit 文件保存实际
  process exit，而不是从日志推断；
- XML root counts、Unity version、起止时间、timeout、候选源码 SHA、source input
  SHA 与 `result.json` 必须互相一致；
- 不把 `Library/Logs`、聊天摘要、手抄 count 或已覆盖文件作为原始证据。

## 12. 可观察验收

- [ ] 签发 revision 的全部 token 已绑定；UGUI 最终候选有独立 R `ACCEPT`，C 串行
  槽和 Unity process gate 成立。
- [ ] LOC-A artifact/runner/license/M/B/L/material/build 未成为前置、输入或 evidence。
- [ ] changed/new 产品路径精确等于第 5.1 节 6 项；所有既有文件 byte-identical。
- [ ] 2 个 production C# 中所有新类型 internal；public/API、asmdef、friend、package、
  ProjectSettings、scene、Prefab、font、resource 变化均为 0。
- [ ] 唯一 preference v1 路径和 schema 精确；writer UTF-8 无 BOM、确定性，reader
  strict 拒绝 unknown/duplicate/case/trailing/bad UTF-8。
- [ ] missing 使用真实 `LocalePolicy`；合法 remembered selection 覆盖 system；invalid/
  read failure 保留原 bytes、system fallback、零 repair write。
- [ ] 所有 pre-commit fault 恒 `SaveFailed`；cleanup/read-back secondary failure 不升级
  为 `Unknown`。
- [ ] 所有 post-commit 未证明状态为 `Unknown`；只有 rollback 成功且 reopen/read
  byte-equal old bytes 或证明 old absence 才降为 `SaveFailed`。
- [ ] 第 8.4 节 primary code、disposition 与 player-visible key 精确；无 path、exception、
  message、input、enum 或 temp/backup 信息泄漏。
- [ ] 所有持久化结果下 session 保持 desired locale；同 desired retry 重做 storage，
  不重复 locale/rebind/business event。
- [ ] PREF-01～PREF-21 全部映射到实际 leaf fullname；最终 frozen focused run 数量
  等于 `{{FOCUSED_EXPECTED_TOTAL}}`，failed/skipped/inconclusive 全 0。
- [ ] compile exit 0，日志无 compiler error；所有 run 符合第 10.3 节进程、时间和
  尝试上限；focused/targeted test renderer 非 Null。
- [ ] evidence 精确为第 11 节 create-once 闭集，hash/argv/exit/XML/count 一致。
- [ ] catalog、LOC-A、RES-01D、raw-byte bridge、outer admission、Host composition、
  full suite、Windows、Android、APK、device、release 均明确 `NOT_RUN / NOT_VERIFIED`。

## 13. 阻塞、修复与回滚合同

### 13.1 稳定停止分类

- `BLOCKED_ON_UGUI_ACCEPT_AND_C_SERIAL_SLOT`：当前准备态；
- `BLOCKED_INPUT_DRIFT`：design、UGUI receipt、source/test/asmdef identity 漂移；
- `BLOCKED_INTERFACE_MISMATCH`：接受后的 internal contract 或 friend/asmdef 无法承载；
- `BLOCKED_PROJECT_OCCUPIED`：其它 Unity 正在打开本项目或上次 PID 未退出；
- `BLOCKED_PATH_ALREADY_EXISTS`：6 个 create-only 路径任一已存在；
- `BLOCKED_SCOPE_EXPANSION`：需要第 7 个路径、既有文件修改、public/dependency/
  asmdef/friend/Host composition 等越界；
- `FAILED_COMPILE`：两次 compile 预算内未通过；
- `FAILED_FOCUSED_TESTS`：一次 targeted repair 与第二次 frozen full run 后仍失败；
- `EVIDENCE_CONFLICT`：create-once root 已存在、闭集外文件或 hash/count 不一致。

出现阻塞时按 `.agent/CODING_RULES.md` 返回：

```text
BLOCKED
Reason: <精确原因>
Missing information: <精确缺口>
Required decision: <主程／QA 必须处理的事项>
Files inspected: <路径>
No changes made: Yes/No
```

不得把 LOC-A 完成与否列为 blocker。

### 13.2 修复边界

- compile/test fault 只在 6 路径内修复；
- 不改断言、filter、count、诊断表、v1 schema 或状态边界求绿；
- 需要修改接受后的 UGUI 文件时立即 `BLOCKED_INTERFACE_MISMATCH`，回主程；
- `NEEDS_FIX` 后由主程签更小 corrective packet，列精确文件/hunk/test；不整包重跑；
- 后续 B-CATALOG 不得静默修补或重写本包已接受文件。

### 13.3 回滚

本包回滚候选仅包括：

- 第 5.1 节本包新建且尚未接受的 6 个路径；
- 本包尚未接受的 `{{CREATE_ONCE_EVIDENCE_ROOT}}`。

任何删除必须由主程另行明确授权。C 不自行使用 Git reset/checkout/stash，不修改
UGUI accepted inputs，不动玩家真实 preference、PlayerSave、旧 evidence、LOC-A、
ReleaseSet 或其它 WIP。若已有路径混入其它 owner 工作，停止并先做责任转移，不删。

## 14. C 回传与独立 R gate

C 回传必须使用：

```text
STATUS: COMPLETED | BLOCKED
PACKET/UGUI/SOURCE IDENTITIES:
DISPATCH/C/QA/R RECEIPTS:
CHANGED/NEW FILES:
PREFERENCE DIAGNOSTIC TABLE:
TEST FULLNAME RECEIPT AND FROZEN TOTAL:
ACCEPTANCE: PASS | FAIL | NOT VERIFIED — each criterion
VERIFICATION: exact argv, RUN/NOT RUN, exit, counts, duration, evidence path
SELF-CHECK: 6-path scope, protected paths, public/API, asmdef/friend, serialization, unrelated diff
NOT_RUN: LOC-A/catalog/RES-01D/raw-byte bridge/Host composition/full suite/Windows/Android/device/release
BLOCKER: only when blocked
```

主程须检查 actual patch、6-path scope 和 evidence 闭集后才可交 QA。QA 绑定的
independent R 必须检查实际 diff 与原始 evidence，只返回一个：

- `ACCEPT`
- `NEEDS_FIX`
- `REJECT`

作者、主程、C 的 self-check、compile 通过或 tests 通过均不能替代 R `ACCEPT`。
本包 `ACCEPT` 不自动授权 B-CATALOG、RES-01D 或 Host composition。

## 15. prepared-01 自检（非实施证据）

### 15.1 路径与 protected scope

- 新路径：6/6，精确为 2 production C#＋1 test C#＋3 `.meta`；无重复；
- 当前只读观察：6/6 路径不存在；签发仍须重新生成 absence receipt；
- 既有文件修改额度：0；public/API、asmdef、friend、package、ProjectSettings、scene、
  Prefab、font、resource、Git 变化额度：0；
- 唯一新增 serialization：独立 locale preference v1；既有业务 serialization 变化 0。

### 15.2 依赖与测试

- 前置只包含冻结设计、AMEND-04、UGUI 独立 `ACCEPT`、最终 source identities、C
  串行 Unity 槽和 QA/R/evidence 绑定；
- LOC-A artifact/manifest/runner/license/M/B/L/material/build 前置：0；
- PREF matrix：PREF-01～PREF-21；pre/post commit、rollback proof、session、retry、
  corrupt/read fallback、诊断隔离均有观察点；
- 实际 leaf fullname/filter/count 当前故意未绑定，不从 21 个 ID 猜测。

### 15.3 当前 dispatch tokens

当前未绑定：UGUI 独立 R final、UGUI accepted candidate identity、最终 LocaleId／
LocalizationService／LocalizedTextSource／friend／asmdef／Host tests identities、C
dispatch turn、QA/R identities、Unity executable identity、process/serial-slot receipts、
accepted UGUI fullname baseline、PREF leaf fullname/count、focused filter/total、
create-once evidence root 和签发时 6-path absence receipt。

### 15.4 当前 NOT_RUN

```text
NOT RUN — product/test/.meta implementation
Reason: 本文仅为准备态；UGUI 独立 ACCEPT、最终 source identities 与 C 串行槽尚未绑定。

NOT RUN — Unity compile / focused EditMode
Reason: 无实施授权；实际 leaf fullname/filter/count、Unity/process gate 与 evidence root 尚未绑定。

NOT RUN — LOC-A artifact/runner/license/M/B/L/material/build
Reason: 与 B-PREF 无真实依赖，不属于本包输入或验证。

NOT RUN — catalog / RES-01D / raw-byte bridge / outer admission / Host composition
Reason: 属于后续独立包，不得由 B-PREF 模拟。

NOT RUN — full suite / Windows / Android / APK / device / release
Reason: 不属于本包且相应后续门未满足。
```

**prepared-01 结论：`PREPARED_NOT_AUTHORIZED / BLOCKED_ON_UGUI_ACCEPT_AND_C_SERIAL_SLOT`。**
