# UGUI-01-Q4-FIX-03：真实 GraphicRaycaster 的图形设备验证修正

2026-10-01 · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 目的、权限与唯一结论边界

本包只验证并修正 Q4 的 **Unity 启动条件**。当前 3 个红灯都经过真实
`GraphicRaycaster`；既有 targeted-9 以 `-nographics` 启动，日志明确记录
`Forcing GfxDevice: Null` 与 `Renderer: Null Device`。本包用同一 source/resource
candidate、同一测试名称、同一业务断言，只移除 `-nographics`，先执行 exact 3 的 A/B
反证；若 exact 3 全绿，再执行 targeted 9 与 exact 222。

- 用户 authority：仓库 `AGENTS.md` 第 6 节已批准的五角色持续执行流程；中央 AI
  `01a0e401-511d-79f2-b47f-3ab0ade1681b / local` 已明确授权一个 scoped common-root
  diagnosis/fix packet，保留真实 Input/业务断言与 222 个名称。
- 唯一执行 owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52 / local`。
- 签发与接收：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b / local`。
- 当前 source candidate：
  `0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91`。

本包不授权产品、测试、Prefab、Scene、字体、meta 或 Unity 设置修改；不授权绕过
`GraphicRaycaster`、直接调用 pointer handler、手工注入业务请求、删除/改名测试、放宽
foremost-hit/capture/commit 断言。若仅移除 `-nographics` 仍不能使 exact 3 全绿，保存证据并
停止，返回 `BLOCKED_HYPOTHESIS_REJECTED`，不得猜修。

## 1. 固定输入与红灯

### 1.1 上一轮身份

| 输入 | 固定身份 |
| --- | --- |
| FIX-02 packet | `docs/team/2026-09-30/engineering-ugui-01-q4-correction-fix-02.md` / 10,795 bytes / SHA-256 `7ef805179f0d4f7d4b7585857679f7124a7041b1f7a3b50417fc8bfbbb375f12` |
| continuation packet | `docs/team/2026-09-30/engineering-ugui-01-q4-validation-continuation.md` / 9,168 bytes / SHA-256 `fc69ddf8d6041de6bb3db75f25ee6acad3c1d93ed6e365c11a453639f6ba78b6` |
| continuation final receipt | `TestArtifacts/FightMatch/UGUI-01/0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/q4-correction-02/continuation-final-receipt.json` / 70,811 bytes / SHA-256 `8ebe75bde864bf5b1ae48df163d133c86e04e973ea4759be4104e5ed3898a3db` |
| targeted-9 XML | `TestArtifacts/FightMatch/UGUI-01/0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/q4-correction-02/targeted-9/tests.xml` / 17,377 bytes / SHA-256 `590df0deec6cc046950327e5ab1ff4722cc210a0a60ec2365c402d5f1bc64ee3` |
| repaired Prefab | `Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab` / 1,060,568 bytes / SHA-256 `55ded35adf6fd7d4e207303801e3ada172190bea6a516443e1bcd21d3193004b` |
| Scene | `Assets/Scenes/FightMatchDemo.unity` / 12,900 bytes / SHA-256 `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd` |
| TMP font | `Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset` / 8,546,563 bytes / SHA-256 `66e7963607e4e33432af150ebd085e7cdac1324133e102eaf87da24c135febdb` |

七个 FIX-02 source SHA-256 必须保持：

| 路径 | bytes | SHA-256 |
| --- | ---: | --- |
| `Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs` | 12,750 | `956744b6f5f1fb571e5c069589d422e0dd8f668b604f50c35f00b22c8cdc357d` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs` | 27,769 | `34c37acf134231045ee0752d14dc647a8567ba9b76e804b2eed546410fa6981c` |
| `Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs` | 21,477 | `81f3009728dc0522253a1f56e642225829e767c54e2031e2a8ee90e7cb0d79a8` |
| `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` | 83,965 | `2c7a1612954ccc341f72a58918ffc1356074bf1802c866f01b7c9fdb72ad7dac` |
| `Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs` | 13,982 | `095ce3720258d860c70883201288c009efa397cddcb6bad5ec7bc713f6358cd1` |
| `Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs` | 11,337 | `dbfdfe0e333777d19d76e8a84860d6912fbd916e44425a99cbb7b1a9ffd128e9` |
| `Assets/Tests/EditMode/FightMatch/PlayerNavigationPresentationTests.cs` | 32,437 | `0ee946a00433e0fdebb08b1699ebc6cbafb8816d88ea4a53ac26a1f1631279a4` |

以上 14 个已存在输入在 `2026-09-30T23:15:01Z` 从实际路径机械读取并同时核对
64-bit byte length 与 SHA-256；与 continuation final receipt 中对应 identity 一致。
未来 `q4-correction-03`、runner、XML、log、receipt 和 process snapshot 均是待生成输出，
本表不预填其 identity。审计时 `q4-correction-03` 根实测不存在。

### 1.2 已复现 exact 3

上一轮真实结果为 6 pass / 3 fail；不得重跑同一个 Null-device 红灯来冒充新诊断。固定
失败名称：

```text
FightMatch.Core.Tests.CandidateBoardInputTests.B16B03_RealPanelDownCapturesOnlyAndLastUpCellSubmitsOnce
FightMatch.Core.Tests.UguiSceneCompositionTests.OwnerEndedCommitsOnceDespiteBothPointerUpAndEndDrag
FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H01_ActualPanelDetachKeepsPresentationTokenAndPauseCompletesItOnlyOnce
```

已观测失败分别是：mouse down 后未 capture；真实 Host raycast hit list 为空；真实 Host 的
board 不是 foremost hit。三者均在真实输入派发前/处失败，尚无证据指向 business commit
语义。上一轮 `unity.log` 的 argv 含 `-nographics`，并在同一次进程记录 Null renderer。

## 2. 可证伪假设

按优先级固定，不在执行中改写：

1. **H1（主假设）**：Null 图形设备没有建立真实 uGUI raycast 所需的绘制/depth 状态，
   因此当前真实 `GraphicRaycaster` 断言与 `-nographics` 不兼容。只移除该参数即可使 exact
   3 全绿，且业务/资源/source 均不变。
2. **H2**：即使有真实图形设备，Canvas/layout/registry 尚未到可 raycast 状态；exact 3
   仍会失败，但新的 hit/capture 结果会否定 H1，之后才允许另签临时 instrumentation 包。
3. **H3**：H01 是其他 raycast-target 覆盖 board，而前两项为空 hit；若去掉
   `-nographics` 后出现这种分裂结果，保存每项原始 failure/log，停止，不做复合修补。
4. **H4**：共享/借用 EventSystem 的 input/module 生命周期造成 stale pointer state；只有
   graphics A/B 未通过后，且证据指向 module state 时才进入后续诊断。

通过判据不是“日志看起来正常”，而是 exact 3 名称集合精确、3/3 pass，并且新 argv 与
上一轮的唯一测试语义差异是删除 `-nographics`，新日志明确使用非 Null renderer。

## 3. 写入白名单与禁止项

产品、测试、asset、meta 写入白名单为 **空集**。只允许 create-new：

```text
TestArtifacts/FightMatch/UGUI-01/
  0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/
    q4-correction-03/
      fix-03-packet.md
      preflight.json
      exact-3-graphics/
      targeted-9-graphics/       # 仅 exact 3 通过后
      editmode-222-graphics/     # 仅 targeted 9 通过后
      final-receipt.json
      validation-tools/
```

可从上轮 validation tool 复制到本 root，再做最小 runner 调整：测试 Unity argv 不含
`-nographics`；不得原地改写上轮工具/evidence。所有 output create-new；路径已存在即停。
不得写 `Library/`、`Temp/`、`Logs/`、`UserSettings/`、Packages、ProjectSettings、生成项目
文件、source/resources。Unity 自身不可避免的导入缓存不作为授权写入产品文件；若任何
受管 source/resource identity 变化，立即 `BLOCKED_SCOPE_DRIFT`。

禁止 Q1/recovery/PrepareResources/SaveAssets/4181/Q5/Android/APK/device、Git 写、kill/signal、
并发 Unity、直接修改旧 evidence。

## 4. 进程与资源合同

每步启动前核对项目未被任何其它 Unity process 打开。一次只允许一个顶层 Unity process；
记录 absolute argv、PID、UTC、duration、exit、stdout/stderr/log/XML hash、immediate child
snapshot 与 final quiescence。尾部 child 只读等待自然退出；每步 launch→静止总预算：

- exact 3：`≤180s`；
- targeted 9：`≤240s`；
- exact 222：`≤600s`。

超过预算或仍有 owned/Unity process 时保存原始事实并停止，不发送 signal。每步前后比较
既有 53-source manifest、37-resource manifest、七个 FIX-02 source、Prefab/Scene/font、
meta、旧 b9/f21/8a3 roots 与 q4-correction-02 evidence。不得覆盖旧 root。

测试进程仍使用 `-batchmode`、同一 Unity 2022.3.18f1 Intel Mac Editor、同一 project、
EditMode、XML/log/output 路径与测试过滤机制；**唯一启动条件修正是测试进程不再传
`-nographics`**。日志必须证明 renderer 不为 `Null Device`；否则视为
`BLOCKED_NO_GRAPHICS_DEVICE`。

## 5. 有序执行

### 5.1 Preflight

1. 核 §1 identities、当前无 Unity、q4-correction-03 不存在；
2. 冻结 source/resource/old-root/旧 evidence before manifest；
3. 复制本 packet 与最小 runner 到 create-new root；runner source hash 入 manifest；
4. 固定 exact 3、targeted 9、exact 222 的期望 fullname 集合，沿用上轮清单，不自行发现、
   删除或替换名称。

### 5.2 Exact 3 graphics A/B

一次 Unity，仅执行 §1.2 三个 fullname；不传 `-nographics`。必须同时满足：

- process exit 0，最终静止；
- XML 精确 3/3 pass，0 failed/skipped/inconclusive；
- 日志 renderer 非 Null；
- source/resources/旧 evidence 不变。

任一不满足立即停止；保存每个 failure 原文与完整日志，返回对应 blocked 状态，不进入
targeted 9。exact 3 全绿时，记录结论：既有红灯由 Null-device 验证条件造成，不需要产品
代码修复；不得夸大为 Unity 所有 headless 测试都不能运行。

### 5.3 Targeted 9 graphics 回归

仅 §5.2 通过后，新 Unity、同一 graphics 条件，运行上轮精确 9 fullname。必须 9/9 pass、
名称集合精确、0 failed/skipped/inconclusive、非 Null renderer、source/resources 不变。
失败即停，不进入 222。

### 5.4 Exact 222 graphics 验收

仅 §5.3 通过后，新 Unity、同一 graphics 条件，运行冻结的精确 222 fullname。必须
222/222 pass、名称集合精确、0 failed/skipped/inconclusive、非 Null renderer、最终静止。

完成后比较最终 37 resources 与 preflight baseline：必须 37/37 byte-identical。若动态字体
或任何资源变化，本包不授权 final reopen/save/修复；保存结构与字节 delta，返回
`BLOCKED_RESOURCE_DRIFT`。53 source 与七个 FIX-02 source 必须 byte-identical。

## 6. 验收与回传

- [ ] exact 3 graphics 3/3；唯一 A/B 启动变化为删除 `-nographics`，renderer 非 Null。
- [ ] targeted 9 graphics 9/9；exact 222 graphics 222/222；名称集合精确，无
  skip/inconclusive。
- [ ] 真实 `GraphicRaycaster`、foremost board、pointer capture、last-up commit、业务与资源
  断言均未修改或绕过。
- [ ] 产品/测试/Prefab/Scene/font/meta/source 零修改；37 resources、53 source、旧 evidence
  与旧 roots 不变。
- [ ] 每步真实 argv/PID/time/exit/log/XML/process snapshots 完整；所有进程自然静止，无
  signal、无并发 Unity。
- [ ] Q1/recovery/PrepareResources/SaveAssets/4181/Q5/Android/APK/device/Git 均 `NOT_RUN`。

Owner 回传：packet/preflight/final receipt identities；三步真实 argv、renderer、PID、时间、
exit、XML statistics/fullname sets、log/hash、process snapshots；before/after source/resource/
old-root/evidence manifests；未运行项。完成后停止，不提交、不推送、不建分支。

主程审实际 evidence 后送 QA；QA 回原 R-TOOLS
`01a0f3e3-9663-7be2-84a8-ed23be32f0b9 / local` 作独立最终验收。作者与执行者不能自批。
