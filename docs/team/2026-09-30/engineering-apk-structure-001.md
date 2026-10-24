# APK-STRUCT-01-DESIGN · APK 程序集结构证明

状态：**BLOCKED_AT_OFFICIAL_API_FEASIBILITY / IMPLEMENTATION_NOT_AUTHORIZED**

任务：`ENG-RELEASE-GATES-001/B`

设计负责人：工程架构子会话 `01a0f2e6-1ac3-7d10-8855-54192b9489d4` / `local`

回传：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`

固定代码起点：`125b849be13fe2ebf5b1185f3cdb83d19c6327ef`

前置：`ANDROID-API24-01-DESIGN` 已先完成设计内审；本包不实施、不运行 Unity。

```text
BLOCKED
Reason: Unity 2022.3.18f1 的公开 Editor API 没有提供最终 UnityLinker/IL2CPP 包所含程序集与类型定义的完整后处理清单；可用回调只能读取裁剪前 Player script DLL，BuildReport 只列输出文件/模块，不能满足“最终包绑定的结构清单”。
Missing information: 一个在 Unity 2022.3.18f1 上实际通过的最小样片，证明可由受支持 API 取得最终裁剪后 assembly/type definitions 并绑定同一 APK；当前官方 API 与安装程序集检查均未发现该能力。
Required decision: 主程/中央须选择并获准其一：提供可复核的受支持 API 路径；或另开变更提案，明确允许版本锁定的 IL2CPP 元数据结构解析（仍禁止原始字符串扫描）及相应范围/风险。不得把裁剪前编译清单降低为最终包证明。
Files inspected: engineering-system-001.md §24.3/§25；system-task-packets.md §457-461（重点 §460）；demo-019-delivery.md；固定起点的 FightMatchAndroidBuild.cs、Invoke-FM029Validation.ps1；当前 Host/测试 asmdef；Unity 2022.3 官方 API 与本机 2022.3.18f1 UnityEditor.CoreModule.dll 公开接口。
No changes made: 产品/测试/工具文件 Yes；仅新增本设计报告。
```

## 1. 历史事实不可改写

本包修复的是后继验证能力，不改写 029 历史：

- 019 的 Unity `BuildReport.result` 为 `Succeeded`，正常 APK 已产生。
- 019 原包装 `result.json` 仍为失败：`FM029: Normal APK contains QA/test metadata: FightMatch.Core.Tests`；该原始文件、字节、SHA 与失败口径必须保留。
- §460 的补充结构检查在当时固定 IL2CPP 元数据布局上报告 normal 为 37 个程序集/4771 个类型且无禁止定义，并以 018 QA 包识别测试程序集/类型作正对照。它是只读补证，不是“019 原包装通过”，也不是新 manifest 的证明。
- 冻结的 `Tools/Invoke-FM029Validation.ps1` 与 018/019 证据不修改。旧 `global-metadata.dat` 原始字符串误报不得通过白名单、忽略命中、删除 `InternalsVisibleTo` 或回填旧结果来消失。

本设计使用“定义”而不是“名称出现”：程序集/类型必须来自结构化定义表；AssemblyRef、TypeRef、custom attribute、`InternalsVisibleTo` 和普通字符串中的名字均不算定义。

## 2. 官方 API 可行性结论

### 2.1 已证能力

- `IPostBuildPlayerScriptDLLs.OnPostBuildPlayerScriptDLLs(BuildReport)`：Unity 官方说明它在 Player scripts 编译后立即回调，并可从 `report.files` 读取或修改中间 managed assemblies。该时点早于 UnityLinker/IL2CPP 最终裁剪，能证明“编译输入”，不能证明“最终 APK 定义”。
- `CompilationPipeline.GetAssemblies(AssembliesType.Player)`：返回 Unity 编译的脚本程序集集合，是项目编译图，不是特定最终包的裁剪结果。
- `BuildReport.GetFiles()`：返回 build process 输出文件；没有程序集/类型定义枚举。
- `BuildReport.strippingInfo`：公开表面只有 included native modules 与包含原因，不是完整 managed assembly/type definition 表。
- `IUnityLinkerProcessor`：Unity 2022.3.18f1 的公开接口只有 `GenerateAdditionalLinkXmlFile(BuildReport, UnityLinkerBuildPipelineData)`，用于在裁剪前追加 preservation 规则；`UnityLinkerBuildPipelineData` 的公开信息只含 target。该版本没有公开 `OnAfterRun` 或最终 linker output 回调。

### 2.2 不足与判定

因此，以下看似可用的方案全部不合格：

| 方案 | 为什么不满足 |
| --- | --- |
| 反射当前 Editor AppDomain | 得到 Editor 已加载类型，不是 Player build，更不是最终 APK |
| `CompilationPipeline.GetAssemblies(Player)` | 项目级编译集合，未绑定特定 BuildOptions/QA 包，且在 stripping 前 |
| `IPostBuildPlayerScriptDLLs` 读取 DLL | 可结构化读定义，但仍在 UnityLinker/IL2CPP 前；会把最终已裁掉类型误报为包含 |
| `BuildReport.GetFiles/strippingInfo` | 文件/模块粒度不足，没有完整类型定义 |
| 在 manifest 中让测试程序集自行登记 | 自报告可造假，无法证明真正进入最终包 |
| 外部或 Editor 端扫描 `global-metadata.dat` 原始字符串 | 重复 019 根因，明确禁止 |

官方 API 结论是：**当前包不能按既定验收安全实施。** 这不是说最终结构在技术上不可解析，而是现有获准范围没有受支持 API 来产生所要求的最终包绑定清单。

官方依据：

- [Unity 2022.3 `IPostBuildPlayerScriptDLLs.OnPostBuildPlayerScriptDLLs`](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Build.IPostBuildPlayerScriptDLLs.OnPostBuildPlayerScriptDLLs.html)
- [Unity 2022.3 `BuildReport.GetFiles`](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Build.Reporting.BuildReport.GetFiles.html)
- [Unity 2022.3 `CompilationPipeline.GetAssemblies`](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Compilation.CompilationPipeline.GetAssemblies.html)
- [Unity 2022.3 `IUnityLinkerProcessor.GenerateAdditionalLinkXmlFile`](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Build.IUnityLinkerProcessor.GenerateAdditionalLinkXmlFile.html)
- [Unity 2022.3 `UnityLinkerBuildPipelineData`](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/UnityLinker.UnityLinkerBuildPipelineData.html)

本机精确版本补核：`UnityEditor.CoreModule.dll` 的公开元数据中 `IUnityLinkerProcessor` 只有 `GenerateAdditionalLinkXmlFile`；这只是对已安装 2022.3.18f1 API 表面的只读确认，不等于运行过 Player build 样片。

## 3. 解阻后候选范围

只有第 2 节阻断被正式关闭后，才可把本节提升为实施包。既定候选路径保持：

```text
Assets/Scripts/FightMatch/Host/Editor/FightMatchApkAssemblyManifest.cs
Assets/Scripts/FightMatch/Host/Editor/FightMatchApkAssemblyManifest.cs.meta
Assets/Tests/EditMode/FightMatchHost/FightMatchApkAssemblyManifestTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchApkAssemblyManifestTests.cs.meta
Tools/Test-FightMatchApkAssemblies.ps1
```

不得修改旧 `Tools/Invoke-FM029Validation.ps1`、旧证据、Packages、ProjectSettings、产品 runtime、场景/Prefab 或 Git。

### 3.1 需要单独显式许可的 asmdef

当前 `FightMatch.Host.Tests.asmdef` 没有引用 `FightMatch.Host.Editor`；新测试文件无法以正常编译引用直接测试 Editor manifest 逻辑。若解阻后仍要求直接、无反射的 EditMode 单测，须先单独批准：

```text
Assets/Tests/EditMode/FightMatchHost/FightMatch.Host.Tests.asmdef
```

最小改动仅在 `references` 增加 `FightMatch.Host.Editor`；新生产文件可用 assembly-level `InternalsVisibleTo("FightMatch.Host.Tests")` 暴露 internal 测试缝，不新增 public API。未经这项许可不得用反射绕过程序集边界，也不得静默改 asmdef。

当前证据没有证明必须修改 `FightMatchAndroidBuild.cs`：独立新 Editor callback 理论上可观察同一 BuildReport 并在 APK 已存在后写 sidecar。若最小样片证明必须由现有 build 入口传递不可替代的 run/source 身份，则把该文件作为“proposed explicit permission”另报，限于一次可信绑定调用；不得先改，也不得混入 UGUI/API24 合并。

### 3.2 与 UGUI/API24 的串行合并

- UGUI-01 先稳定并审查；API24 再按独立包串行合并并形成新候选身份。
- APK-STRUCT 的样片和实现只能读取该稳定候选，不预填未来 `FightMatchAndroidBuild.cs` SHA。
- 语义合并点是实际 `BuildReport.summary.outputPath/platform/options/result`、最终 APK 身份和 API24 的新 `build-report.minSdk`；不依赖旧行号、旧 UI Toolkit 场景或 018/019 APK。
- 若三包都触及 build callback 顺序，主程先冻结 callback order 与 sidecar 完成时点，再运行任何 APK；不让两个作者同时写 build 入口。

## 4. 解阻后 manifest 合同

### 4.1 产生时点与信任边界

Editor 生产者必须从同一次 Player build 的最终结构提供器取得 assembly/type definitions；该提供器必须先通过最小样片门。最终 APK 存在后，生产者再读取 `BuildReport` 与 APK 字节，create-new 原子写 sidecar。外部 PowerShell 只做以下工作：

1. 解析 JSON schema；
2. 重新计算 APK bytes/SHA256并与 manifest 比较；
3. 比较调用方给定的固定 source SHA、build target/options 和 QA/normal 模式；
4. 对结构化 `assemblies[].types[]` 做正/负断言。

外部脚本禁止打开或扫描 APK 内 `global-metadata.dat`，禁止把 reference/custom-attribute 字符串当 definition，禁止信任测试程序集提交的自报列表。

“不可由测试伪造”的准确含义是：接受用的结构列表只能由 Editor build callback 对最终结构提供器读取，Android/NUnit 测试代码没有登记 API，且 sidecar 在 BuildReport 成功和 APK 身份确定后由 create-new 生产者写出。它不是抵御可任意改仓库/文件的恶意本机管理员的密码学证明；若要求该威胁模型，需另立签名/远端 attestation 方案，不能在本包虚称。

### 4.2 schema v1

格式名固定为 `fightmatch.apk-assembly-manifest/v1`，UTF-8 无 BOM、LF、程序集和类型按 Ordinal 排序、重复拒绝。字段最少为：

```json
{
  "schema": "fightmatch.apk-assembly-manifest/v1",
  "generator": {
    "unity": "2022.3.18f1",
    "provider": "approved-final-structure-provider",
    "phase": "post-link-final"
  },
  "source": {
    "sha256": "<64 lowercase hex>",
    "manifestSha256": "<64 lowercase hex>"
  },
  "build": {
    "mode": "qa|normal",
    "target": "Android",
    "options": "<BuildSummary.options canonical symbolic value>",
    "includeTestAssemblies": true,
    "startedUtc": "<round-trip UTC>",
    "endedUtc": "<round-trip UTC>"
  },
  "apk": {
    "leaf": "<file name only>",
    "bytes": 1,
    "sha256": "<64 lowercase hex>"
  },
  "structure": {
    "assemblies": [
      { "name": "Assembly.Simple.Name", "types": ["Namespace.Type", "Namespace.Outer+Nested"] }
    ]
  }
}
```

规则：

- `source.sha256` 是构建前冻结 `source-manifest.json` 的 canonical 内容 SHA；canonical 行为 `relative/path<TAB>lowercase-file-sha256<LF>`，Ordinal 排序，末尾 LF。输入至少覆盖当前获准的 `Assets/`、`Packages/`、`ProjectSettings/` 正常文件；QA 的 Unity 临时 `Assets/InitTestScene*.unity(.meta)` 单独记录为 ephemeral，不并入共同 source SHA。
- `manifestSha256` 是上述 source-manifest 原始字节 SHA；两个 APK 必须绑定同一共同 source SHA。
- `includeTestAssemblies` 必须与 BuildOptions 位一致：QA 为 true，normal 为 false。options 同时保存规范符号串，不只保存易混整数。
- `apk.leaf` 禁止目录、`..`、链接；验证器由调用参数提供 APK 路径并重新计算 identity，manifest 不能指向另一文件。
- `structure.assemblies` 只列定义程序集；`types` 只列 TypeDef 全名。引用、属性和字符串不设“白名单字段”。空结构、重复、非 Ordinal、未知 schema/provider/phase 均失败。

### 4.3 QA 正对照与 normal 负对照

同一个脚本、同一个 schema 解析器，准确运行两次：

QA 必须结构化包含：

- assembly definition `FightMatch.Android.Tests`；
- probe types `FightMatch.Android.Tests.AndroidQaRun`、`FightMatch.Android.Tests.AndroidContentPlayerTests`、`FightMatch.Android.Tests.AndroidStoragePlayerTests`；
- 实际 NUnit 与 Unity TestRunner 定义程序集，名称由最小样片冻结后写入实施修订，不在无样片时猜测。

Normal 必须不存在以下 definition：

- assembly `FightMatch.Core.Tests`
- assembly `FightMatch.Host.Tests`
- assembly `FightMatch.Android.Tests`
- NUnit definition assembly/type
- Unity TestRunner definition assembly/type
- 上述三个 QA probe types

出现同名 AssemblyRef、TypeRef 或 `InternalsVisibleTo` 字符串不导致失败；只有 definition 集合参与判断。QA 缺任一冻结正对照为假阴性失败；normal 有任一禁止定义为真污染失败。不能用 QA 通过推断 normal，也不能用 normal 通过证明验证器会识别测试内容。

## 5. 测试合同（解阻后）

`FightMatchApkAssemblyManifestTests.cs` 精确 10 个非参数化 EditMode cases：

1. schema v1 最小合法值可 round-trip，写出 Ordinal 稳定。
2. 未知 schema/provider/非 `post-link-final` 被拒绝。
3. assembly/type 重复、乱序、空定义或非法全名被拒绝。
4. source SHA/source-manifest SHA 非小写 64 hex 或不同候选被拒绝。
5. APK leaf 穿越、bytes≤0、SHA 非法或实际 identity 不同被拒绝。
6. target 非 Android、mode/options/IncludeTestAssemblies 组合不一致被拒绝。
7. QA 含完整 Android test assembly/probes 与 TestRunner 定义时通过。
8. QA 缺任一 probe 或只有 reference 名称时以 `FMAPK008` 失败。
9. Normal 无禁止 definitions、但含三个 friend/reference 名称时通过。
10. Normal 含任一禁止 assembly/type definition 时以 `FMAPK009` 失败并报告准确全名。

这些测试只验证模型/解析/判据，不能生成或签署接收用 manifest，不能代替最小 API 样片或实际 APK 对照。

PowerShell 脚本需要独立语法/fixture 测试：同一合法 QA/normal fixture 各一次、APK hash 篡改一次、manifest 篡改一次、reference-only 一次。fixture 不是产品通过证据。

## 6. 错误码、结果与预算

错误码冻结：

| 码 | 条件 |
| --- | --- |
| `FMAPK001` | schema/JSON/必填字段错误 |
| `FMAPK002` | sidecar/result 非 create-new、路径或文件类型错误 |
| `FMAPK003` | source/candidate 绑定错误 |
| `FMAPK004` | target/mode/options/IncludeTestAssemblies 错误 |
| `FMAPK005` | APK leaf/bytes/SHA identity 错误 |
| `FMAPK006` | provider/phase 不是批准的最终结构来源 |
| `FMAPK007` | assembly/type 定义空、重复、乱序或非法 |
| `FMAPK008` | QA 正对照缺失 |
| `FMAPK009` | normal 出现禁止 definition |
| `FMAPK010` | QA 与 normal source/schema/generator 不同版 |
| `FMAPK012` | Unity 最终结构 API/提供器不可用；必须 BLOCKED |

验证器正常通过 exit 0；合同/污染失败 exit 2；输入/格式/identity 失败 exit 3；环境或 provider 缺失 exit 4。所有结果 JSON 包含 `passed`、`errorCode`、`message`、manifest/APK/source identity、mode、运行 UTC；失败也 create-new 保留。

结果根：

```text
TestArtifacts/FightMatch/APK-STRUCT-01/<candidate-sha>/
  feasibility/
  editmode/
  qa/assembly-manifest.json
  qa/validation-result.json
  normal/assembly-manifest.json
  normal/validation-result.json
  review/
```

预算与顺序：

| 门 | 上限 | 接收条件 |
| --- | --- | --- |
| Q0 API 样片 | 1 个最小 clean Player build 样片，≤10 分钟 | 能从受支持最终结构来源识别 QA probe；否则 `FMAPK012/BLOCKED`，不进入实现 |
| Q1 静态/语法 | 每候选 1 次，≤5 分钟 | 白名单、PowerShell parse、无 raw metadata scan、无 public/API/config 扩张 |
| Q2 EditMode | 精确 10 cases 一次，≤10 分钟 | 10/10、零失败/跳过；源码变更后原结果失效 |
| Q3 APK | QA 一次＋normal 一次，各≤10 分钟 | 同 source/schema/provider；BuildReport 成功、APK identity 稳定 |
| Q4 验证器 | QA 一次＋normal 一次，各≤2 分钟 | 同一脚本；正对照与负对照都通过 |
| Q5 独立 R | 一次；NEEDS_FIX 只审修正 | 核实际 patch、原始结果和历史未改写，给唯一 verdict |

构建失败或样片不能提供最终清单时不自动重试，不回退字符串扫描；只保留真实失败并交回主程。

## 7. 回滚、接收与非目标

- 每个新 C#/PowerShell 文件及其 meta 为本包回滚单位；删除候选前先保留失败证据。需要回滚旧文件或 asmdef 时只按另行批准的精确补丁，不用 `git reset`。
- manifest 生成失败必须使该 APK 的结构门失败；不能保留 APK 成功而将结构结果默认为空/通过。
- 旧 018/019 只作历史参考，禁止复制为新 `qa/normal` 结果。
- 独立 R 必须分别核 Standards 与 Spec，并只给 `ACCEPT`、`NEEDS_FIX` 或 `REJECT`。作者、样片或脚本不能自签产品接收。
- 本包不覆盖设备玩法、完整 release、账号、广告、网络、资源下载、商店政策、签名轮换或目标 SDK 升级。

## 8. 本回合 NOT RUN

- NOT RUN — Unity 2022.3.18f1 API feasibility Player build；原因：本回合只获准设计，不获准 Unity/构建，且官方 API 静态结论已触发样片前阻断。
- NOT RUN — Compile/EditMode；原因：无实施候选，且当前测试程序集缺少 Editor asmdef 引用许可。
- NOT RUN — QA/normal APK 与 `Test-FightMatchApkAssemblies.ps1`；原因：生产者/脚本未创建，最终结构提供器未获准。
- NOT RUN — 设备、PlayMode、ADB、完整 release；原因：明确非目标。
