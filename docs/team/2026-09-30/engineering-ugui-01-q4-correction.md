# UGUI-01-Q4-CORRECTION-01：共享根因修复与资源稳定性闭环

2026-10-01 · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 授权、owner 与完成边界

本包只纠正 UGUI-01 候选在 Q4 暴露的共享根因。它不重开 UI 设计、COPY、
Application/Input/save 语义或 Android/device 工作。

- 用户 authority：仓库 `AGENTS.md` 第 6 节已批准的五角色持续执行流程；中央 AI
  `01a0e401-511d-79f2-b47f-3ab0ade1681b / local` 已要求主程按共享根因收口并继续推进。
- 唯一执行 owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52 / local`。
- 签发与回传：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b / local`。
- 原实现 candidate：
  `b9f2f64bf2aaeb930f83afdd6c1290a5895386cd870b1f7f4c07f7d0f4591669`。
- 完成标准：本包 8 个白名单路径形成一个新 source candidate；Q1、原生 Prefab
  修复、修复后 Q2、Q3、9-case targeted、精确 222-case Q4 按顺序通过；最终资源
  身份稳定；实际 patch/evidence 经主程审查后交同一 R-TOOLS 终审。

执行者在任何一步遇到未列明文件、未解释的新失败、资源越界或 identity drift，先保存
真实证据并停止，返回 `BLOCKED`。不得在同一轮追加第二种修法。

## 1. 固定输入与不可变证据

### 1.1 设计与任务输入

| 输入 | 固定身份 |
| --- | --- |
| uGUI correction plan | `docs/system-design/2026-09-17/runtime-ui-ugui-correction.md` |
| UGUI-01 implementation packet | SHA-256 `ccb7ed615cf9d688f61a19620ff4619ea2c43be75c44f2820a916622552ea623` |
| original source candidate | `b9f2f64bf2aaeb930f83afdd6c1290a5895386cd870b1f7f4c07f7d0f4591669` |
| original 53-file source manifest | `c52dd78f25fc8ba9a51d930803185866d9e0ad5c931707c7e8da391db5e94849` |
| original Q2 receipt | `a64dde829df41fb933b1310e7372d7282d0dbac449efa04a4e70f5e2d112c300` |
| original Q3 receipt | `4c540c4442a90ba9072a5846e0ea08a88bd601cfc08cfd405b276432baa1acd7` |
| original Q4 receipt | `5275d76ec3cb40b98164353e4813dc54f5b50bb64dee5d9fb62a068db1a1787f` |
| original Q4 XML | `6e1738bc0f6def4fca4f0b57c13efd4037edd22168c63dc40bb3ab657ce00b13` |
| 9-case diagnosis receipt | `cb395a3fb933a84f9e3ccd4e11c51390a8d65740286e9564159926db23cd72ae` |
| correction proposal | `c89786645abbc2f65f39d36a3b69df00ea94c5091cd96611da7d4d4f10ed677d` |

原 b9 evidence root 及其所有现有 descendants 自本包签发后只读：

```text
TestArtifacts/FightMatch/UGUI-01/b9f2f64bf2aaeb930f83afdd6c1290a5895386cd870b1f7f4c07f7d0f4591669/
```

不得覆盖、续写、删除、重命名或回填其中的 Q1-Q4、diagnosis、post-q4 或工具证据。

### 1.2 当前资源起点

| 资源 | 当前 bytes / SHA-256 |
| --- | --- |
| `Assets/Scenes/FightMatchDemo.unity` | 12,900 / `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd` |
| `Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab` | 1,060,275 / `934e5bbec0c6cec2a16afa4778fb019a3420e3432cc0d75e07703e6f4cd60487` |
| `Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset` | 8,546,563 / `66e7963607e4e33432af150ebd085e7cdac1324133e102eaf87da24c135febdb` |

当前字体是 Q4 后真实状态：346 characters、346 glyphs、4 atlases，Dynamic、Multi
Atlas、1024×1024、padding 9、SDFAA 与 source OTF GUID 均不变。历史 Q2 字体
`d7d632...` 原字节不存在；它只保留为历史 receipt，不能被重建或冒充当前输入。

## 2. 精确写范围

### 2.1 允许修改的 7 个源码文件

| 路径 | 签发时 bytes / SHA-256 | 唯一允许变化 |
| --- | --- | --- |
| `Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs` | 12,703 / `9fdb9e54d910102d61f38d69835d18c2238877a1e514832ae5cef039a6709e01` | 给真实 Graphic 增加 `[RequireComponent(typeof(CanvasRenderer))]`，不改绘制、命中、gesture 或 public interface |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs` | 27,562 / `20234245b18fde41d0e5f22abe0d80a61a9a9feb5d1a0fb15c680ca5f428043d` | 原生 EditMode lifecycle 初始化；点击 helper 改为真实 left `pointerClickHandler`；无反射/镜像/hook |
| `Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs` | 21,307 / `3ba8e980b64631c8a2840cc08b06850d2ed87f944deef97bb7f6773548e9f453` | 同一原生 input-module 初始化；fixture Dispose 对 Unity 伪空对象幂等 |
| `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` | 71,156 / `4e185d4fbb5fc21eb23f909c4faacfa231a28d4b074db4163579cb9b34100186` | 一个受严格 preflight 约束的 Editor-only Prefab recovery entry；既有 Prepare/Verify 保持 |
| `Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs` | 13,812 / `a83b0085ff779ffd85a224f86fbd1cfd8f33c58f48724cf41890c14fbe3bab8b` | 只修 missing-font/material 负例的内存 font fixture |
| `Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs` | 11,336 / `f14cba1e4cdd9ef2372a082a16e5a0215c0510ea6a1684cf5980a4ff6209a882` | confirmed-result 动态 TMP row 使用独立 `save.ConfirmedResult` ViewId |
| `Assets/Tests/EditMode/FightMatch/PlayerNavigationPresentationTests.cs` | 32,315 / `b132aca990eecc69d0b35b6189bbc3a89ae94bb14ab396644ec9a592c6dba7cb` | 只更新既有 B02 row 查询并在同方法证明 ViewId 唯一 |

### 2.2 允许由 Unity 原生修改的 1 个资产

```text
Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab
```

只给现有 `BoardFrame` / `CandidateBoardElement` GameObject 增加一个
`CanvasRenderer`。保留 Prefab GUID、全部现有 GameObject/fileID、组件、顺序以外的
序列化数据、引用和文本。通过 `LoadPrefabContents`、`AddComponent<CanvasRenderer>`、
`SaveAsPrefabAsset`、`UnloadPrefabContents` 完成；不得手写 YAML。

### 2.3 Evidence 写范围

7 个源码改完并冻结新的 53-file manifest 后，以其 64 位 SHA 作为
`<NEW_SOURCE_CANDIDATE>`，只新建一次：

```text
TestArtifacts/FightMatch/UGUI-01/<NEW_SOURCE_CANDIDATE>/q4-correction-01/
```

该根保存 Q1、recovery、Q2、Q3、targeted、Q4、资源 before/after、运行工具副本和
最终 manifest。不得向原 b9 root 写新文件。

除以上 8 个产品/测试路径与新 evidence root 外，仓库全部只读。特别禁止 `.meta`、
Scene、字体、OTF/OFL、TMP Settings、shader、Packages、ProjectSettings、Application、
Input、save、COPY、LOC/RES/Android/device/release 文件变化。

## 3. 实施合同

### 3.1 EventSystem/InputModule fixture

测试 fixture 使用实际 `EventSystem`、`FightMatchStandaloneInputModule`、真实
`PointerEventData` 与生产 callback：

1. owned event root 在 inactive 状态创建并装配 EventSystem/module/input；在首次激活
   前令现有 module instance `runInEditMode=true`，随后只做一次原生激活，使
   `BaseInputModule.OnEnable` 建立其 EventSystem cache。
2. borrowed module 若需同样初始化，保存并在 Dispose 恢复其原 active/enabled/
   `runInEditMode` 状态；不得留下跨 test 状态。
3. inactive→active 只用于 fixture 构造且发生在 Bind/输入之前；它不得替代原 blur/
   focus/disable acceptance 分支，也不得改变这些既有测试的直接 internal handler 路径。
4. 禁止 reflection、`SendMessage`、手调 `Update`/`OnEnable`、test-only runtime hook、
   新 public interface、镜像输入模块或直接调用业务 controller 冒充 UI 事件。
5. button helper 使用 `ExecuteEvents.pointerClickHandler` 和 left-button
   `PointerEventData`。保留 Button 的 active/interactable 判断和原 listener；同步重建后
   不再让 `ISubmit.Button.OnSubmit` 访问已被 `DestroyImmediate` 的自身。

### 3.2 Renderer 与 Prefab recovery

- `CandidateBoardElement` 的 RequireComponent 是运行时真实依赖，不添加替代 renderer。
- Editor recovery entry 可为 `-executeMethod` 所需的 `public static` Editor-only 方法；
  这不是 runtime public interface。
- 现有 private `UguiOutput` 对 `PrepareResources` 的路径守卫逐字保持。只对
  `mode == VerifyResources` 增加两个精确允许的输出形状，并保留原绑定 `run.json`、
  author/mode/source/argv 与 create-new 检查：

  ```text
  TestArtifacts/FightMatch/UGUI-01/<NEW_SOURCE_CANDIDATE>/q4-correction-01/resources/reopen/resource-reopen.json
  TestArtifacts/FightMatch/UGUI-01/<NEW_SOURCE_CANDIDATE>/q4-correction-01/resources/final-reopen/resource-reopen.json
  ```

  `<NEW_SOURCE_CANDIDATE>` 仍须为 64 位小写十六进制；只允许以上两个固定 leaf，不能
  接受任意目录、通配 suffix 或原 b9 root 的新写入。该路径适配不得改变
  `VerifyUguiResources` 的字体、subasset、placeholder、场景、Prefab 或引用断言。
- recovery preflight 必须验证：author/candidate token、7 个源码新身份、旧 Prefab
  bytes/SHA/GUID、BoardFrame 精确一个、CandidateBoardElement 精确一个、
  CanvasRenderer 精确零、Scene/字体/其余 36 资源符合本包起点。
- preflight 成功才原生加一个组件并保存；保存后验证 BoardFrame/CandidateBoardElement/
  CanvasRenderer 均精确一个、Prefab GUID 不变、Scene/字体/其余资源不变。
- 任一条件不符不保存，返回稳定 blocker。不得调用原 Prepare、重建 Prefab 或修别的
  serialized difference。

### 3.3 其余四个根因

- Host fixture Dispose：先按 Unity 对象有效性判断；清除 held Root/View 后重复 Dispose
  零动作。产品 Host/View 生命周期代码不改。
- missing-font/material 负例：从实际 font 创建只存在于内存的 owned clone，保留合法
  face/glyph/atlas/padding 数据，只让 material 缺失；结束时销毁 clone。仍须由
  `LocalizedTmpText` 报 `MissingFontOrMaterial`、显示 placeholder 并打开真实 host gate。
  不吞日志、不 `LogAssert.Expect` 一个本不应发生的 TMP NRE、不改持久字体。
- navigation：动态 confirmed-result row 与现有 OriginalResult Button 各有唯一 ViewId；
  文案 key、双 locale、saved subtype、precondition、handler 和业务结果不变。
- H05 及 `VerifyUguiResources` 的 font/subasset 断言本包不改。先以当前冻结字体和原断言
  验证；若再次失败，保存 evidence 并停止，另报更小任务。

## 4. 执行顺序与预算

每个 Unity process 前先做进程安全检查；项目已在其它 Unity process 打开则停止。每次
运行前后都冻结：新 53-source manifest、37-resource manifest、字体原字节副本及
character/glyph/atlas/config/external-GUID 结构。Unity 自动生成的
`ProjectSettings/SceneTemplateSettings.json` 只在它仍为已判定的精确 3,534 bytes /
`5baf593374ad67246277a435d8d0ded7167696eef6e05d2d245670e490625d8c`
且包前不存在时，按已有清理证据流程移除；不同身份立即停止。

1. **Preflight 与 source edit**：验证 §1/§2 identities；只改 7 个源码；冻结新
   53-file manifest 和 candidate；运行 Q1 raw/effective 一次。Q1 不通过则停止。
2. **Native Prefab recovery**：一次、≤300s。只运行 §3.2 entry。保存 before/after
   Prefab 原字节和 37-resource delta；除 Prefab 外资源 delta 必须为零。
3. **修复后 Q2**：独立新 Unity process、一次、≤300s。复开并验证现有 scene、修复后
   Prefab、当前字体、119 TMP/3 linked input/116 exact placeholder、CanvasRenderer、
   EventSystem、字体与 shader/TMP Settings。把这次实际 37-resource identities 冻结为
   **新 Q2 基线**；不得复用历史 `d7d632...` 字体身份。
4. **Q3 compile**：独立 process、一次、≤300s；exit 0、无 compiler/unhandled error，
   source/resource 与新 Q2 基线不变。
5. **Targeted 9**：沿用 diagnosis 的精确 9 fullnames，一次、≤180s；必须 9/9 pass、
   0 failed/skipped/inconclusive，资源与新 Q2 基线不变。任何失败停止，不跑 222。
6. **Q4 exact 222**：targeted 全绿后，同原 filter 一次、≤600s；必须准确 222/222、
   0 failed/skipped/inconclusive，名称集合与冻结 222 清单相同。
7. **资源最终化**：比较 Q4 后资源与第 3 步新 Q2 基线。
   - 若 37/37 byte-identical：直接冻结该实际身份作为 final，不追加 Unity。
   - 若只有 dynamic font 发生可解释变化：冻结原字节与结构 delta，再允许最多一次
     独立复开、≤300s；复开前后 37/37 必须稳定且全部原断言通过。
   - 任何其它资源变化或复开后继续变化：停止为 `BLOCKED_RESOURCE_DRIFT`。

最大 Unity launches：recovery、Q2、Q3、targeted、Q4 共 5 个；只有第 7 步条件成立
才有第 6 个。失败步骤后不继续依赖运行，不重跑 4181、旧 APK、Q5、Android 或设备。

## 5. 验收与回传

- [ ] 最终产品/测试 delta 精确落在 7 source + 1 Prefab；无 `.meta` 或其它路径变化。
- [ ] RequireComponent 与 Prefab 中真实 CanvasRenderer 同时存在；9 个原 raycast/
  foremost failures 实际转绿，未放宽命中或 mesh 断言。
- [ ] EventSystem/module 由 native lifecycle 初始化；无 reflection、mirror、hook 或业务
  直调；57 NRE 实际消失。
- [ ] left pointer click 保留真实 Button callback；两个 destroyed Button failure 转绿，
  原 stale/refresh/unchanged 断言保留。
- [ ] Host fixture 重复 Dispose、missing material 负例、navigation B02 各自转绿；没有
  改产品生命周期、吞日志、删除断言或改变用例数。
- [ ] H05 原断言通过；字体保持 Dynamic/Multi Atlas 和固定 OTF/OFL，资源最终身份按
  第 4.7 节稳定。
- [ ] Q1、recovery、Q2、Q3、targeted 9、Q4 222 的命令、进程、耗时、exit、日志、XML、
  source/resource before/after 均有真实 receipt。
- [ ] Q4 精确 222/222，failed/skipped/inconclusive=0；Q5/4181/Android/APK/device 均
  `NOT_RUN`。

Owner 回传：新 candidate/source manifest；8 路径 before/after bytes/SHA；最小 diff；
Prefab GUID/component/fileID 证明；Q1-Q4 与 targeted receipts；新 Q2/final 37-resource
manifests；字体原字节与结构 delta；旧 b9 root unchanged 证明；未运行项。不得自行请求
QA/R，也不得提交、推送、建分支或改 Git history。

主程审 actual patch/evidence 后才送 QA；QA 回同一 R-TOOLS
`01a0f3e3-9663-7be2-84a8-ed23be32f0b9 / local` 作终审。
