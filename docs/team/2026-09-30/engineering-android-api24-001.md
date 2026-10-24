# ANDROID-API24-01-DESIGN · Android 7 最低版本门

状态：**DESIGN_COMPLETE / IMPLEMENTATION_NOT_RUN**

任务：`ENG-RELEASE-GATES-001/A`

设计负责人：工程架构子会话 `01a0f2e6-1ac3-7d10-8855-54192b9489d4` / `local`

回传：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`

固定代码起点：`125b849be13fe2ebf5b1185f3cdb83d19c6327ef`

用户已定决定：首发最低 Android 7.0（API 24），排除 Android 5.1/API 22 与 Android 6/API 23；本包只落实最低版本门，不扩大为 Google Play 或完整发行配置。

## 1. 输入与并发边界

本设计绑定固定提交中的以下输入，不绑定 C 正在实施的 UGUI-01 半成品：

| 输入 | 固定起点观察 |
| --- | --- |
| `ProjectSettings/ProjectSettings.asset` | `AndroidMinSdkVersion: 22`；SHA256 `2ea02ff3b5acc6127f29d58049317eee4a52a84d9afde809eaf1efb30ed4ce1b` 为开工时工作区观察值，仅用于识别前置状态，不是未来合并 SHA |
| `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` | `CheckProduction()` 断言 `(int)PlayerSettings.Android.minSdkVersion == 22`；开工时工作区观察 SHA256 `84c84c0123222861f365e63bd48057ddc1187eae74b1a5c28b768ce224d9ebe9`，不作为 UGUI-01 后文件身份 |
| `Tools/Invoke-FM029Validation.ps1` | 只读历史参考；SHA256 `d2e4d32541ff4b3712578282b2b3f1af703731c241cb7a4b251acb4ae1f5503d`；本包禁止修改或复用其旧 029 结果作为新证据 |

`FightMatchAndroidBuild.cs` 与 UGUI-01 有写冲突。实施顺序固定为：

1. 等 UGUI-01 实际补丁稳定并完成其专业审查；记录当时该文件 SHA、行数和语义锚点。
2. 以该已审补丁为新输入，仅在 `CheckProduction()` 的 `Version/SDK` 断言和 `WriteReport()`/`BuildResultRecord` 的 SDK 证据字段处做本包最小合并。
3. 不预填未来 SHA，不把固定起点的行号当合并定位；使用成员名和现有错误文本作语义锚点。
4. 若 UGUI-01 已改变这些锚点或发布参数合同，停止为 `BLOCKED`，由主程重签窄范围，不覆盖其修改。

固定起点的 029 入口还把 stage、plan identity、输出正则和 `001..020`
create-new 槽位写死；现有历史已使用到 019，不能据此安排两个新 APK。UGUI-01
稳定后还须先核其实际 build 入口能否在不改第三个文件、不覆盖旧槽位的条件下各产生
一个新 QA/normal 输出。若仍不能，本包实施在构建前返回 `BLOCKED`；不得借 API24
包修改冻结验证器、复用旧槽或扩大 release 配置。

## 2. 精确改动合同

### 2.1 唯一候选文件

```text
ProjectSettings/ProjectSettings.asset
Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs
```

不得新增文件。两文件外任何差异立即停包。

### 2.2 `ProjectSettings.asset`

只允许一项序列化值变化：

```diff
-  AndroidMinSdkVersion: 22
+  AndroidMinSdkVersion: 24
```

同文件其余字节必须与实施起点逐字节一致，特别是以下值不得改变：

- `AndroidTargetSdkVersion: 32`
- `applicationIdentifier.Android: com.yyczz1.fightmatch`
- `bundleVersion: 0.1`
- `AndroidBundleVersionCode: 1`
- `AndroidTargetArchitectures: 2`（当前 ARM64）
- `scriptingBackend.Android: 1`（当前 IL2CPP）
- `managedStrippingLevel.Android: 4`（当前 Minimal）
- 签名、keystore、图形 API、AAB/APK、扩展文件、符号、Gradle 模板和所有其它 PlayerSettings 字节

不得借本包更新 target SDK、包名、版本、ABI、IL2CPP、裁剪或签名。API 24 决定不是这些字段的变更授权。

### 2.3 `FightMatchAndroidBuild.cs`

语义锚点一：`private static void CheckProduction()` 中错误文本为 `Version/SDK` 的断言。最低版本部分准确改为命名枚举：

```csharp
PlayerSettings.Android.minSdkVersion == AndroidSdkVersions.AndroidApiLevel24
```

同一断言中的 `targetSdkVersion == 32`、`bundleVersionCode == 1` 与 `bundleVersion == "0.1"` 保持原语义和数值。官方 Unity 2022.3 API 将 `PlayerSettings.Android.minSdkVersion` 定义为 `AndroidSdkVersions`，且 `AndroidApiLevel24` 明确对应 Android 7.0/API 24。

语义锚点二：`private static void WriteReport(BuildReport report)` 创建 `BuildResultRecord` 的位置。给每次实际 QA/normal 报告增加只读整数证据：

```text
minSdk = (int)PlayerSettings.Android.minSdkVersion
```

语义锚点三：`BuildResultRecord` 增加且只增加 `int minSdk`。不改变旧字段含义、文件名、BuildOptions、输出路径或构建流程。报告写出前仍须由 `CheckProduction()` 阻止非 API24 输入；报告的 `minSdk` 只是证据，不是设置来源。

### 2.4 接口与格式影响

- public runtime API：无。
- 游戏存档、业务序列化和资源格式：无。
- Unity 项目序列化：有且仅有 `AndroidMinSdkVersion` 从 22 到 24；这是本包明确批准的受保护变更。
- Editor 内部证据 JSON：`build-report.json` 增加 `minSdk` 整数字段；旧 029 报告不得回写、迁移或冒充新格式。
- 依赖、Packages、asmdef、meta、场景、Prefab：无变化。

## 3. 实施任务包

唯一交付负责人由主程在 UGUI-01 稳定后指定；同一时刻仍只有一个 Unity 执行者。

### 3.1 允许与禁止

允许修改：第 2.1 节两文件。

允许生成的未跟踪验证产物：仅第 4 节新结果根。

禁止修改：`Tools/Invoke-FM029Validation.ps1`、Packages、其它 ProjectSettings、Assets 代码/测试/资源/meta、旧 018/019 APK 与全部旧证据、Git 历史。

禁止动作：安装工具、升级 Unity、改 target SDK、运行设备验收、提交/推送/建分支或工作树。

### 3.2 前置与保存复开

1. 核 Unity 进程；项目已在 Editor 打开时不启动 batch mode。
2. 冻结实施起点两文件的 SHA/字节/行数以及全工作区既有差异；确认 UGUI-01 已稳定并可串行合并。
3. 应用第 2 节准确差异。通过 Unity 2022.3.18f1 载入并保存项目设置后关闭，再重开一次；读取 API 必须为 `AndroidApiLevel24`。
4. 复开后比较 `ProjectSettings.asset`：除准确一行 `22→24` 外无字节差异。Unity 若重写其它设置，保留差异和日志并停止，不手工掩盖。

## 4. 验证与证据

候选身份用两文件定版后的 canonical SHA 表示；结果根：

```text
TestArtifacts/FightMatch/ANDROID-API24-01/<candidate-sha>/
  static/
  reopen/
  compile/
  qa-build/
  normal-build/
  review/
```

每个运行目录至少保存 `run.json`、完整 `unity.log`、实际退出码、输入文件身份和结果摘要；构建目录另保存 `build-report.json`、APK 路径/字节/SHA256及 manifest 查询原始输出。不得覆盖失败运行。

### 4.1 静态门 Q1（一次，≤5 分钟）

- `git diff --name-only` 只含两文件；`git diff --check` exit 0。
- `ProjectSettings.asset` 恰一项值变化，且第 2.2 节保护字段与实施起点相同。
- C# 差异只含命名枚举断言与 `minSdk` 报告字段；没有 BuildOptions 或发布参数变化。
- 搜索不得残留本构建门的 `minSdkVersion == 22` 或 `AndroidMinSdkVersion: 22`。

### 4.2 保存复开 Q2（一次有效运行，≤5 分钟）

- Unity 版本准确为 2022.3.18f1。
- 保存、关闭、重开后 `PlayerSettings.Android.minSdkVersion == AndroidSdkVersions.AndroidApiLevel24`。
- 项目设置精确字节检查通过，额外差异为零。

### 4.3 Compile Q3（定版候选一次，≤5 分钟）

使用 UGUI-01 稳定后实际获准且满足第 1 节前置的 Mac 验证入口，由主程/主测试在实施包中填入新的 create-once 路径；不得直接复用旧 029 槽位。要求 Unity exit 0、无编译错误。源码再变时原成功失效，只能在记录原因后补一次。

### 4.4 Android Q4（QA 一次＋normal 一次，各≤10 分钟）

构建必须使用同一候选、同一 Unity 2022.3.18f1 和各自新输出路径。每包同时满足：

- `BuildReport.summary.result == Succeeded`，输出 APK 存在且 SHA 与报告一致。
- 新 `build-report.json.minSdk == 24`。
- 用该 Unity 安装随附的 `SDK/cmdline-tools/6.0/bin/apkanalyzer manifest min-sdk <apk>` 读取最终 APK manifest，原始输出精确为 `24`。
- QA 与 normal 的包名仍分别遵守既有合同；target SDK、版本、ABI、IL2CPP、裁剪、签名等保持本包前既有值。本项只核未变，不改变它们。

旧 018/019 Development APK、旧报告或当前 `ProjectSettings.asset` 的文字值不能证明本包通过。两包中任一失败，不把另一包成功扩称为整包通过。

### 4.5 独立接收 Q5

作者提交差异、输入身份、全部原始证据和逐项验收回执。主测试核实际产物后，独立 R 对本包给唯一 `ACCEPT`、`NEEDS_FIX` 或 `REJECT`；作者不能批准自己。

## 5. 可观察验收

1. 唯一产品设置变化是最低 API 22→24；Android 5.1/6 被最低系统门排除。
2. 构建前以 `AndroidSdkVersions.AndroidApiLevel24` 精确拒绝非 24 输入。
3. 保存复开后 API 和序列化字段均为 24，ProjectSettings 其余字节不变。
4. 同一候选 compile 通过；QA 与 normal 各一次实际构建成功。
5. 两份新 BuildReport 证据和两个最终 APK manifest 都独立报告 minSdk 24。
6. target SDK、包名、版本/code、ABI、IL2CPP、裁剪、签名、BuildOptions 和依赖均未被本包改变。
7. 旧失败/成功证据未改写；未宣称 Google Play、设备或完整发行就绪。

## 6. 失败停止、回滚与后继门

- 出现第三个修改文件、额外 ProjectSettings 字节、UGUI 语义锚点冲突、Unity 版本不符或项目正被另一 Unity 占用：立即停止，不继续构建。
- 构建前断言非 24：记录实际值并停止；不得临时改回 22 让构建通过。
- compile 或任一构建失败：保留原日志/输出/部分 APK 身份；只允许在两文件白名单内定位。需要其它文件即 `BLOCKED`。
- 回滚单位是本包两个文件相对其真实实施起点的最小补丁；不得用 `git reset`、删除旧证据或覆盖 UGUI-01。回滚后再核两文件 SHA 与实施起点相同。
- 本包 `ACCEPT` 只关闭 API24 技术门。APK 结构、设备玩法、冷启动、商店政策、账号、广告、网络、完整 release 配置均是独立后继门。

## 7. 本设计验证

本设计回合未修改产品文件，未运行 Unity、compile、测试或 Player build。正式实施必须按第 4 节产生新证据。

官方依据：

- [Unity 2022.3 `PlayerSettings.Android.minSdkVersion`](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PlayerSettings.Android-minSdkVersion.html)
- [Unity 2022.3 `AndroidSdkVersions`](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AndroidSdkVersions.html)
