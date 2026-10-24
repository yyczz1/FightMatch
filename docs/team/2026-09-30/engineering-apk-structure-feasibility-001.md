# APK-STRUCT-FEAS-001 · Unity 2022.3.18f1 IL2CPP 最终 APK 定义表样片

状态：**FEASIBLE_FOR_VERSION_LOCKED_IMPLEMENTATION_DESIGN**

任务：`APK-STRUCT-FEAS-001`

执行角色：architecture owner，thread `01a0f2e6-1ac3-7d10-8855-54192b9489d4` / host `local`

回传：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`

范围：这是中央明确授权的版本锁定结构解析可行性样片；不是产品实现、不是新
manifest、不是新 APK 接收，也不改变 019 原包装失败或 §460 历史证据。

## 1. 唯一结论

在以下五项同时锁定时，结构化解析是可行的：

1. Unity 精确版本为 `2022.3.18f1`；
2. `GlobalMetadataFileInternals.h`、`il2cpp-metadata.h`、`GlobalMetadata.cpp`
   三份实现源的 SHA/bytes 与本报告一致；
3. APK 内唯一、精确路径为
   `assets/bin/Data/Managed/Metadata/global-metadata.dat`；
4. metadata magic 为 `0xFAB11BAF`、version 为 29，全部结构宽度、表范围、
   索引与 image/assembly/type 覆盖检查通过；
5. 最终 verdict 只读取 `Il2CppImageDefinition`、
   `Il2CppAssemblyDefinition`、`Il2CppTypeDefinition` 定义表以及用于恢复嵌套
   名称的 `nestedTypes` TypeDefinitionIndex 表。

同一只读程序对固定 018 QA 正对照和 019 normal 负对照各执行一次成功：QA
从定义表识别测试程序集及 probe；normal 没有禁止的测试/QA 定义。AssemblyRef、
TypeRef、custom attribute、`InternalsVisibleTo`、字符串池或 raw byte token 即使
存在，也完全不参与 verdict。

这证明“版本锁定的最终 APK 结构提供器”可进入实施设计；不证明泛版本 IL2CPP
兼容，也不证明旧 019 wrapper、UGUI/API24 或未来候选已通过。

## 2. 固定输入身份

### 2.1 设计、仓库与历史绑定

| 文件/身份 | lines | bytes | SHA-256 / 值 |
| --- | ---: | ---: | --- |
| `docs/team/2026-09-30/engineering-apk-structure-001.md` | 262 | 17532 | `6a54ae64f6b59734799755badb8c198122b0a2bfbb5185aa530961ef8f51a9d4` |
| `TestArtifacts/FMDemo029/mac-r1/scope-audit.json` | 80920 | 2134273 | `41c321530556651ea1ff53938ff2f4bff4d1cc2e4e7ba6e7556fd8131cc201e0` |
| 其中 `normalApkStructuralSupplement.parserSource` 原文 | 141 | 10336 | `e71f3ecee2d60b363d5ce04d15da96507c8b810d944ddb30af564e11e1fc296f` |
| `TestArtifacts/FMDemo029/mac-r1/root-identity.json` | 1466 | 71153 | `d5e08e04d029f6cbe18e9125f71882f89506107bc85a45cb6e3923b15652c68c` |
| 当前仓库 HEAD（仅识别本次样片环境） | — | — | `125b849be13fe2ebf5b1185f3cdb83d19c6327ef` |
| 029 root identity 的历史 `gitStart` | — | — | `9d416e6c9d3c794be355f903b3636c856783601e` |

当前工作区含别的已授权 UGUI/协调 WIP；样片不读取其半成品作 018/019 source，
也不把当前 HEAD 冒充 APK 构建源。每份结果同时记录“当前解析环境 HEAD”和“029
历史 build gitStart”，避免把两个身份混成一个 source SHA。

### 2.2 APK、原 wrapper 与 BuildReport

| 文件 | lines | bytes | SHA-256 | 历史结果 |
| --- | ---: | ---: | --- | --- |
| `Builds/FMDemo029/mac-r1/018/fightmatch-qa.apk` | binary | 34676164 | `912d4202b144cee261ac8f47bea8d56b8dd7fa31338583d3852c9fdf31860679` | QA 固定正对照 |
| `runs/018/result.json` | 45 | 1502 | `62b6fd3feba1902bfbaf00143cb4d454bc5a3acb3b57b9d9da1e66b1847091c4` | wrapper `passed=true` |
| `runs/018/build-report.json` | 583 | 81245 | `6d44484a729d297381729c7e3c523a721f27e302955152d281183ec03b4952f9` | Unity `Succeeded`、IncludeTestAssemblies |
| `Builds/FMDemo029/mac-r1/019/fightmatch-demo.apk` | binary | 32776566 | `cfdc39d87a4f0152230594eb5dce9ed582e332fb91416c17d83157a749628094` | normal 固定负对照 |
| `runs/019/result.json` | 14 | 473 | `d5f47f76adcec7527ab521f79e79df83d1928a5ca865eb88f2a1ea8891c82c79` | 原 raw-string wrapper `passed=false`，保持 |
| `runs/019/build-report.json` | 409 | 56571 | `f34944eaad648ea3ac134ae78987c7159addfb59153f93f70c7963efe722ca0b` | Unity `Succeeded` |

表中 `runs/...` 均相对 `TestArtifacts/FMDemo029/mac-r1/`。程序在解析前逐项重算
identity，并交叉核 `result.apk` 与 BuildReport 的 APK bytes/SHA/target/result；
任一差异 fail closed。

### 2.3 Unity 精确实现源

共同绝对前缀：

```text
/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/il2cpp/libil2cpp/
```

| 相对路径 | lines | bytes | SHA-256 |
| --- | ---: | ---: | --- |
| `vm/GlobalMetadataFileInternals.h` | 343 | 10513 | `de8b8b8a879bf4213e5a34ac48111525a1d39439cb81276104e2c778f5937138` |
| `il2cpp-metadata.h` | 127 | 4121 | `a11e3fb8c1d5649b12182c99166121afcb0c21866c1d864a71ddcd7258d41731` |
| `vm/GlobalMetadata.cpp` | 1558 | 72524 | `d5a41e534e5e391e372ce4fea0ef4a9bd9f32de8313d18b6c4b29b217e676a74` |

读取这些文件只证明解析器采用的 v29 布局来源；它本身不证明某 APK 由该 Editor
生成。未来正式提供器还必须把 Unity executable/version、BuildReport、source
manifest 与最终 APK SHA 绑定到同一 build run。

## 3. 样片 Module、Interface 与 seam

样片 Module 是 `/private/tmp/FightMatch-APK-STRUCT-FEAS-001/parse_il2cpp_v29.py`。
它的外部 Interface 只有：

```text
--mode qa|normal --apk <exact frozen path> --out <exact create-new result>
```

Interface 固定输入身份、错误为非零 exit、成功返回一个 JSON；复杂的 ZIP、v29
header、定义表、嵌套名称、正负对照实现全部隐藏在 Module 内。未来产品 seam
应保持同样小：Editor build callback 把“最终 APK＋本次 build/source 身份”交给
一个版本锁定结构 Adapter，Adapter 返回 canonical definition manifest 或明确
失败；外部 PowerShell 只核 manifest/APK identity 与正负判据。

样片 schema 为 `fightmatch.apk-structure-feasibility/v1`，version 1。每份结果均
写入 parser 自身 identity、三份 Unity 源 identity、APK/result/BuildReport/
scope/root/design identity、当前 HEAD、历史 gitStart、metadata identity、完整
assembly/type definitions、忽略项和 verdict。输出使用 create-new，写完 `fsync`。

## 4. Fail-closed 检查

### 4.1 ZIP 与输入

- APK 必须是授权的绝对输入，regular file、非 symlink、bytes/SHA 精确。
- ZIP entry 名称唯一、entry 数≤30000、总解压尺寸≤4 GiB、无 symlink entry。
- metadata entry 必须唯一且路径精确；单项解压尺寸为 1..64 MiB，实际读长、
  CRC 和 §460 metadata bytes/SHA 匹配。
- QA/normal result 与 BuildReport 必须是固定字节；BuildReport 必须 Android/
  Succeeded 且绑定同一 APK。019 wrapper 必须仍为 `passed=false`。

### 4.2 metadata v29

- 256-byte header；unsigned magic `0xFAB11BAF`、version 29。
- 官方 header 必须仍恰为 64 个 int32 字段；每个 offset/size 非负、在 metadata
  内，非空表不侵入 header，全部非空 region 两两不重叠。
- 关键记录宽度固定并整除：image 40、assembly 64、type definition 88、
  referenced assembly/type/nested/interface/vtable 4、interface offset 8、method 32、
  field/parameter 12、event 24、property 20、generic container 16、attribute range 8。
- image/assembly 数相等且为 1..1000；TypeDef 为 1..100000。
- 字符串索引必须在 string table 内，限定范围内找到 NUL，并以 strict UTF-8
  解码；不以忽略错误的方式拼出名字。
- image→assembly 与 assembly→image 必须一一反向一致；名称须为 `name` 或
  `name.dll`；程序集/image 名称均不重复。
- 每个 image 的 type/exported/custom-attribute/method 范围、每个 assembly 的
  reference 范围、每个 TypeDef 的 field/method/event/property/nested/interface/
  vtable/interface-offset/generic-container 范围均在对应表内。
- `nestedTypes` 的 TypeDefinitionIndex 均合法、同一 child 只有一个 parent、不得
  自嵌套或跨 image、不得形成循环；canonical nested 名称为 `Outer+Nested`。
- 每个 TypeDef 被且只被一个 image 的连续范围覆盖；同一 assembly 内 canonical
  full name 不重复，声明数量与 image.typeCount 精确相等。

`Il2CppTypeDefinition.declaringTypeIndex` 在此版本是 `TypeIndex`，不是
`TypeDefinitionIndex`。第一次 QA 运行因误把它当 TypeDef 行号而正确 fail closed；
修正后只用官方 `nestedTypes` 定义索引表恢复嵌套关系。该失败已记录在
`run-receipt.json`，没有产生伪结果。

### 4.3 verdict 独立性

QA 必须从定义表包含：

```text
nunit.framework.dll
UnityEngine.TestRunner.dll
FightMatch.Android.Tests.dll
FightMatch.Android.Tests.AndroidQaRun
```

Normal 必须没有以下定义程序集：

```text
FightMatch.Core.Tests.dll
FightMatch.Host.Tests.dll
FightMatch.Android.Tests.dll
nunit.framework.dll
UnityEngine.TestRunner.dll
```

并且不能有 `FightMatch.Core.Tests.*`、`FightMatch.Host.Tests.*`、
`FightMatch.Android.Tests.*`、`NUnit.*`、`UnityEngine.TestTools.*` TypeDef。
Reference/custom-attribute/string/raw-token 路径没有任何 fallback，也没有“忽略
某个字符串命中后算通过”的分支。

## 5. 实际结果

| 对照 | APK metadata bytes/SHA | archive entries | images/assemblies/types | 定义表结果 |
| --- | --- | ---: | ---: | --- |
| QA 018 | 5576828 / `11704f54b529d2ab46885e1a6fdf7aeb5c05c07c8e7141f7ed01a37582b4f3f9` | 38 | 58 / 58 / 6986 | 三个 required image 与 `AndroidQaRun` 全命中；PASS_POSITIVE_CONTROL |
| normal 019 | 3810220 / `0ec761ad1c5f6f47e0899ab575e8139386e8fdbf8dfef321dd19fa8e2b384055` | 54 | 37 / 37 / 4771 | 禁止 image/type definitions 均 0；PASS_NEGATIVE_CONTROL |

结果中的 `ignoredForVerdict` 明列：referenced assembly indices/AssemblyRef、
TypeRef、custom attribute、string-pool/raw-token、`InternalsVisibleTo`。因此 normal
旧属性字符串仍可存在，但它不会被误认为程序集或 TypeDef。

## 6. 命令、时点与预算

QA：

```text
python3 -B /private/tmp/FightMatch-APK-STRUCT-FEAS-001/parse_il2cpp_v29.py --mode qa --apk /Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Builds/FMDemo029/mac-r1/018/fightmatch-qa.apk --out /private/tmp/FightMatch-APK-STRUCT-FEAS-001/qa-result.json
```

Result：exit 0；`2026-09-30T18:18:19.619487Z` 至
`2026-09-30T18:18:19.718609Z`；98 ms；58/58/6986；QA 正对照通过。

Normal：

```text
python3 -B /private/tmp/FightMatch-APK-STRUCT-FEAS-001/parse_il2cpp_v29.py --mode normal --apk /Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Builds/FMDemo029/mac-r1/019/fightmatch-demo.apk --out /private/tmp/FightMatch-APK-STRUCT-FEAS-001/normal-result.json
```

Result：exit 0；`2026-09-30T18:18:24.691208Z` 至
`2026-09-30T18:18:24.795162Z`；103 ms；37/37/4771；normal 负对照通过。

两次都低于每包 5 分钟、合计 15 分钟预算。没有运行 Unity、构建、测试或网络。
首个 QA 诊断运行使用 parser SHA
`a5aa713a64a9ef2616c8555f60891bd067bfbecae3ef17b6dd7573e767e64668`
（669 行/29313 bytes），exit 2，stderr 为
`FMFEAS019: declaring type index: 1`，未创建结果；因当时未独立捕获单调时长，
回执明确记 unknown，不补造数值。

## 7. 临时四件身份

| 文件 | lines | bytes | SHA-256 |
| --- | ---: | ---: | --- |
| `/private/tmp/FightMatch-APK-STRUCT-FEAS-001/parse_il2cpp_v29.py` | 685 | 30005 | `0af3a702605e29298399c3ea3e73070371e2a65f4dd609500a29d99533cb33f8` |
| `/private/tmp/FightMatch-APK-STRUCT-FEAS-001/qa-result.json` | 7635 | 428069 | `303f91280d213fe076e53fb774a6c0c6d140083f1c8e2efd9145e24d885adcfc` |
| `/private/tmp/FightMatch-APK-STRUCT-FEAS-001/normal-result.json` | 5294 | 294217 | `440ae74a2be230a199ff4944458c281ec8ac4f3e07e3535078e80fca29b1ed8b` |
| `/private/tmp/FightMatch-APK-STRUCT-FEAS-001/run-receipt.json` | 79 | 3042 | `2be154913a2f26bc7c2b2d91cbbaa44fca0f6f33e2e755a25359ba158ee0ee78` |

未创建 stdout/stderr 文件。报告自身 identity 因不能无穷自引用，按团队既有
做法由完成后的正式回执外部绑定；报告内不写一个会因写入而立即失效的自哈希。

## 8. 与 §460 parserSource 的逐条差异

### 8.1 可直接复用的认识

- 使用本机精确 Unity IL2CPP 源定义 v29 header 与三个核心结构宽度。
- 固定 APK/result/BuildReport identity；ZIP 中要求唯一 metadata entry 与大小上限。
- header 全 region bounded/non-overlap，字符串 NUL/UTF-8 有界读取。
- image/assembly 一一映射、TypeDef 连续范围全覆盖。
- QA 正对照与 normal 负对照必须使用同一解析器。
- 只把 image/assembly/type definition 当包含证明；友元声明和属性名称不是定义。

### 8.2 正式提供器必须收紧

| §460 原样片风险 | 本样片/未来要求 |
| --- | --- |
| Python `assert` 在 `-O` 下可失效，错误码不稳定 | 全部合同检查用显式 `require`＋非零 exit；产品实现用具名失败码 |
| parser 输出只到 stdout，未绑定 schema/self identity | schemaId/version、parser/source/APK identities、create-new JSON、fsync |
| 动态正则推导结构总宽度但未完整处理 padding/循环 | 只接受精确源 SHA；核心 record width 固定并与结构声明交叉核 |
| 只校 images/assemblies/types/references 宽度 | 增加 method/field/event/property/nested/interface/vtable/attribute 等关键表宽和 TypeDef 子范围 |
| TypeDef full name 仅 `namespace.name` | 由 `nestedTypes` TypeDefinitionIndex 表恢复 `Outer+Nested`，拒绝重复父/跨 image/循环 |
| 未检查 ZIP symlink、精确 metadata 路径别名 | entry 名唯一、无 symlink、精确路径且 suffix 候选也只能一个 |
| raw token occurrences 与结构结果同报，易被调用者误用 | 正式 manifest 不需要 raw scan；引用/属性/string 明列 `ignoredForVerdict` |
| 旧 plan/published-file/friend 检查与结构解析耦合 | 结构 Module 只返回定义事实；source/build/signature/内容发布由各自门组合，不相互替代 |
| 用旧 scope 中整份对象相等证明复现 | 未来用 schema/canonical排序/精确输入 identity；不能依赖旧 JSON 字段偶然顺序 |
| 本机头身份可能被误当 APK 生成来源证明 | 同一 build run 另绑 executable/version、BuildReport、source manifest、APK SHA |

旧 parser 的 raw-token 搜索可继续作为历史误报解释材料，但不得进入未来正式
provider、manifest 或 pass/fail 路径。

## 9. 剩余风险与实施设计要求

1. metadata version 29 并非足够版本键；三个 Unity 源 SHA 任一变化都必须
   `UNSUPPORTED_LAYOUT`，不能凭相同 version 猜兼容。
2. Python 样片不是产品实现。正式 C# Editor Module 必须对同两 APK 产生 canonical
   assembly/type 集合并与本样片逐项相等，再进入新 APK 验证。
3. 最终 APK SHA、source manifest SHA、BuildTarget/BuildOptions、Unity executable
   identity 和 provider version 必须由同一次 build callback 串起；读本机 header
   不代替该绑定。
4. sidecar manifest 是本机可写证据，不是对恶意管理员的密码学 attestation；其
   接收威胁模型仍是受控构建与独立 R。若要抗任意本机篡改，另立签名/远端方案。
5. 完整定义表可达数百 KiB；正式格式须 canonical 排序、明确大小上限并避免把
   全表重复写进多个结果。外部验证器只学 schema，不学习 v29 二进制布局。
6. 对未知 header、额外 metadata entry、索引异常、字符串错误、source/APK
   identity 差异都失败；不得降级旧 raw string scan 或裁剪前程序集清单。

## 10. 未来产品实现白名单建议

原 `APK-STRUCT-01` 五路径保持：

```text
Assets/Scripts/FightMatch/Host/Editor/FightMatchApkAssemblyManifest.cs
Assets/Scripts/FightMatch/Host/Editor/FightMatchApkAssemblyManifest.cs.meta
Assets/Tests/EditMode/FightMatchHost/FightMatchApkAssemblyManifestTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchApkAssemblyManifestTests.cs.meta
Tools/Test-FightMatchApkAssemblies.ps1
```

建议在第一个 C# 文件内形成一个深 Module：小 Interface 接受 final APK、冻结
build/source identity 与 exact layout identity，返回 canonical manifest；ZIP/v29
解析保持 implementation 细节。PowerShell 只核 manifest/APK identity 与
QA/normal 正负条件，永不读取 `global-metadata.dat`。

仍需 proposed explicit permission，不能据本样片自动修改：

```text
Assets/Tests/EditMode/FightMatchHost/FightMatch.Host.Tests.asmdef
```

若稳定输入仍缺 `FightMatch.Host.Editor` 引用，则只允许新增该 reference；新 C#
文件以 assembly-level IVT 给 `FightMatch.Host.Tests` internal test seam，不新增
public runtime API、不用反射。

仅当最小 callback 样片证明独立 `IPostprocessBuildWithReport` 无法获得同一 run 的
可信 source/build 绑定时，再提议：

```text
Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs
```

其最小许可只能是把已冻结 run/source/build identity 交给 manifest Module；不得
顺带改 BuildOptions、签名、target SDK、资源或旧报告。无需修改任何产品 asmdef。

## 11. 与 UGUI/API24 的串行关系

1. UGUI-01 当前正在修改 Host/Presentation/测试 asmdef 等共享面；先完成实际补丁
   稳定和独立审查，本样片不锁其半成品 SHA。
2. `ANDROID-API24-01` 随后只改 ProjectSettings 与稳定后的 Android build 断言/
   minSdk 报告字段，并先解决两个新 create-new APK 输出前置。
3. `APK-STRUCT-01` 最后基于同一稳定候选加入新解析 Module、tests 和 PowerShell
   validator；如获准改 Host.Tests asmdef/build callback，须在派工时记录确切
   输入 SHA 和责任转移，不与 UGUI/API24 并发写共享文件。
4. Compile、EditMode、QA/normal build、同一 validator 两次和独立 R 都在产品
   实施包内运行；本历史样片不能复用为新候选结果。

## 12. 验证、失败与回滚

未来实现至少要有：

- parser 单元：header/version/source mismatch、region overlap、width remainder、
  invalid index、unterminated/invalid UTF-8、duplicate mapping/parent/fullname、ZIP
  duplicate/symlink/bomb、nested canonical name、unknown layout 全部 fail closed；
- golden parity：018/019 C# canonical definitions 与本样片逐项一致；
- anti-self-reporting：test assembly 无登记 Interface，QA probe 只能被定义表观察；
- actual builds：同一稳定候选 QA＋normal 各一次，同一外部 validator 各一次；
- tamper：改 APK/manifest/source/build identity 任一字节均失败；
- scope：只含获准白名单，旧 018/019 与 §460 字节保持。

样片回滚只需删除 `/private/tmp/FightMatch-APK-STRUCT-FEAS-001/`，不会影响仓库或
旧证据；本回合不执行删除。产品实现回滚按新五文件及另行批准的 asmdef/callback
最小补丁进行，不用 `git reset`，不覆盖失败 evidence。

## 13. 主程送独立 R 的审查清单

- [ ] 核本报告外部 identity、四件临时文件 identity 与准确 architecture turn。
- [ ] 重算原设计、scope/root、018/019 APK/result/BuildReport、三份 Unity 源身份。
- [ ] 静态确认 parser verdict 只使用 image/assembly/type/nested definition tables；
      raw/string/reference/custom attribute 没有进入分支。
- [ ] 核全部 header/table/索引/字符串/image↔assembly/TypeDef coverage 检查确实
      fail closed；特别复核 TypeIndex 与 TypeDefinitionIndex 的修正。
- [ ] 复核 QA required definitions 与 normal forbidden definitions 的完整列表和
      实际 full definition 集合，不只看摘要布尔值。
- [ ] 核 019 原 wrapper `passed=false` 与 BuildReport Succeeded 并存且未改写；
      §460 仅历史补证。
- [ ] 判断唯一结论是否严格限定 exact Unity/source hashes/v29，而非泛化承诺。
- [ ] 核未来白名单、proposed asmdef/callback 权限、UGUI→API24→APK-STRUCT 串行
      顺序与测试/回滚门；不得提前授权产品修改。
- [ ] 给唯一 `ACCEPT`、`NEEDS_FIX` 或 `REJECT`；样片作者不能自签。

## 14. 未发生的写入与 NOT RUN

- 仓库唯一新增：本报告。
- 临时根唯一四件：第 7 节所列；无额外 stdout/stderr、缓存或 `__pycache__`。
- 未修改原 `engineering-apk-structure-001.md`、018/019 APK、
  `TestArtifacts/FMDemo029`、Tools、Assets、ProjectSettings、Packages、Library、
  `.meta` 或 Git。
- NOT RUN — Unity/compile/EditMode/Player build/ADB/device/release；原因：本任务只
  允许已有 APK 的离线结构样片。
- NOT RUN — 网络或依赖安装；原因：标准库足够且任务明确禁止。
- NOT PRODUCED — 新产品 manifest；原因：样片结果是 feasibility JSON，不是
  接收 sidecar。
