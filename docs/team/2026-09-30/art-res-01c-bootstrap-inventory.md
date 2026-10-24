# ART-RES-01C fixed bootstrap 资源／文本准备清单

2026-10-01  
状态：`PREPARED_NOT_AUTHORIZED / WAITING_FOR_UGUI_AND_LOC_A / NO_ASSETS_CREATED`

本文是 `RES-01C` 的只读美术／资源／本地化输入准备，不是实现包、资源导入许可、
Unity 执行许可或产品验收。本文没有创建或修改任何 Unity 资源、`.meta`、CSV、
Generated、Packages、ProjectSettings、场景、Prefab、代码或测试。唯一新增文件是本文。

## 1. 固定输入与结论

| 输入 | 本次核对身份／结论 |
| --- | --- |
| `docs/team/2026-09-30/engineering-yooasset-001.md` | SHA-256 `2e12b5f24ecedef03df5795c9632318c55fe6330562731892d4c9704c54740d8`；§11／§18 `RES-01C` 是唯一 bootstrap 边界。 |
| `docs/team/2026-09-30/engineering-ugui-01-implementation.md` | SHA-256 `ccb7ed615cf9d688f61a19620ff4619ea2c43be75c44f2820a916622552ea623`；当前是 C 的 WIP 输入，不能作为已接收资源身份。 |
| `docs/team/2026-09-30/planning-localization-draft.csv` | COPY AMEND-04 接受身份 SHA-256 `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`，65,358 bytes，265 物理行，264 data records／264 unique keys。本文只读，不改 CSV。 |
| LOC-A 当前候选 | 已观察到 `PASS_LOC_IMPL_A` 证据和两个 Generated 候选，但仓内状态仍写明尚缺原独立 R 对实际 patch＋新 evidence 的 `ACCEPT`；本文不得提前接收。 |
| uGUI 当前候选 | C 的脚本和选择性 TMP Essential 文件已出现，但 Root Prefab、Noto TMP asset、自然 `.meta`、迁移后场景及复开证据尚未形成；本文不得冻结 WIP SHA／GUID。 |

结论：`RES-01C` 只能先冻结“复用什么、缺什么、谁提供、通过什么门”。实际派发仍
等待 uGUI 候选独立接收、LOC-A 候选独立接收、缺失 COPY amendment 独立接收，以及
工程 owner 对最终白名单／字节预算／启动引用的补签。

## 2. 当前资源盘点

### 2.1 可复用且身份已固定的本地字体源／许可

| 路径 | bytes | SHA-256 | GUID | 处理 |
| --- | ---: | --- | --- | --- |
| `Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular.otf` | 16,437,364 | `2c76254f6fc379fddfce0a7e84fb5385bb135d3e399294f6eeb6680d0365b74b` | `960a9cda7a6b34935b8c18586488d314` | 复用；不得复制第二份字体。 |
| `Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular.otf.meta` | 481 | `93869223cf061c6ee983b1942834dad5aac12319181501c30a94178a4c6ffa66` | 同上 | 原字节保留。 |
| `Assets/UI/FightMatch/Fonts/OFL.txt` | 4,301 | `6a73f9541c2de74158c0e7cf6b0a58ef774f5a780bf191f2d7ec9cc53efe2bf2` | `e6f3cf4d3cbf34266af76d305095c7e9` | 复用并随包可查看。 |
| `Assets/UI/FightMatch/Fonts/OFL.txt.meta` | 158 | `0d8d0b72c7d326dd2a6bb5db7c8e5769edd5b611bde7f729ac79f177b9fe8aa7` | 同上 | 原字节保留。 |

来源身份：Noto Sans CJK SC Regular `2.004`，官方 `notofonts/noto-cjk`
`Sans2.004` 对应 commit `523d033d6cb47f4a80c58a35753646f5c3608a78`，字体路径
`Sans/OTF/SimplifiedChinese/NotoSansCJKsc-Regular.otf`；许可为仓库根 `LICENSE`，
SIL Open Font License 1.1。该来源、commit、字节和许可已在既有 029 记录中冻结。
运行时不得联网取字体，也不得依赖系统字体。

`Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular.asset` 是旧 TextCore／UI Toolkit
动态 FontAsset（5,678 bytes，SHA-256
`0db8ff6c3c3c21b582058f5a2000f399448ee0b0743970f12cf9927a50f01bea`，GUID
`05ef1c5c9ac7e412caf6604e174ec91e`）。它只作回滚实物，不能作为 uGUI/TMP bootstrap
字体，也不能复制其 GUID。

### 2.2 当前 C WIP：只记录路径和绑定时机，不接收身份

本次只读快照观察到：

- `Assets/TextMesh Pro/**` 的 10 个官方路径项／17 个物理文件均已出现；它们是未跟踪
  WIP，尚未随 uGUI 候选独立接收。
- `Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab(.meta)`、
  `Assets/UI/FightMatch/Runtime.meta`、
  `Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset(.meta)` 均尚不存在。
- 新脚本 `SafeAreaFitter.cs`、`FightMatchStandaloneInputModule.cs`、
  `FightMatchViewId.cs`、Host／Presentation friend 文件已出现，但对应自然 `.meta`
  尚不存在；它们仍属于 C 的未完成写域。
- `Assets/Scenes/FightMatchDemo.unity` 仍是迁移前内容：SHA-256
  `257708df579d05a29eb3edd945214b5308297e88e3c8cd91bbc2cc8aae0e2f40`，场景 GUID
  `1d5124e5b55fe409d8216e78a117dec0`，只有 `FightMatch` 根、Host、旧 `UIDocument`
  和旧 TextCore 字体引用；没有 Canvas、SafeArea、RuntimeRoot 或 EventSystem。

因此本文不抄录 C WIP 文件的最终 SHA／新自然 GUID。`RES-01C` 只在 uGUI 独立
`ACCEPT` 后读取其最终 receipt，并在实现包签发时冻结以下绑定：

1. 先接受官方 TMP 资源和 Noto TMP asset；
2. 再接受 `TMP Settings` 对 Noto、两份 CJK 换行表、标准 SDF shader 及 Mobile
   fallback 的复开绑定；
3. 再接受单 `FightMatchRuntimeRoot`、单 Canvas／EventSystem、SafeArea 和场景引用；
4. 最后由 bootstrap 引用这些已接收共享资源，不重新生成第二套 TMP／字体／shader／
   line-break 资源。

### 2.3 官方 TMP 共享项

下列 GUID 来自 TMP `3.0.6` Essential Resources，只有 uGUI 接收后才能成为
`RES-01C` 输入：

| 项目路径 | 官方 GUID |
| --- | --- |
| `Assets/TextMesh Pro.meta` | `f54d1bd14bd3ca042bd867b519fee8cc` |
| `Assets/TextMesh Pro/Resources.meta` | `243e06394e614e5d99fab26083b707fa` |
| `Assets/TextMesh Pro/Resources/TMP Settings.asset(.meta)` | `3f5b5dff67a942289a9defa416b206f3` |
| `Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt(.meta)` | `d82c1b31c7e74239bff1220585707d2b` |
| `Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt(.meta)` | `fade42e8bc714b018fac513c043d323b` |
| `Assets/TextMesh Pro/Shaders.meta` | `e9f693669af91aa45ad615fc681ed29f` |
| `Assets/TextMesh Pro/Shaders/TMP_SDF.shader(.meta)` | `68e6db2ebdc24f95958faec2be5558d6` |
| `Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader(.meta)` | `fe393ace9b354375a9cb14cdbbc28be4` |
| `Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc(.meta)` | `3997e2241185407d80309a82f9148466` |
| `Assets/TextMesh Pro/Shaders/TMPro.cginc(.meta)` | `407bc68d299748449bbf7f48ee690f8d` |

Mobile shader 当前观测字节与设计冻结相符：asset SHA-256
`970db4b1e73b8289c1dd56cff60877f426b49c9682620b2d14da6cd70b64cb9c`，meta
SHA-256 `f2112f50c72762d4e6731391495ea11f76975d677b698e9bcfdd38275ea18f9d`。
但 `TMP Settings` 仍是官方初始引用（Liberation／Sprite／Style）；必须等待 C 按
uGUI 包改绑、保存、关闭重开并获接收，bootstrap 不可直接消费当前 WIP。

## 3. 264 键权威中的 bootstrap 现有键

纳入规则仅覆盖固定 bootstrap §11 明示的检查、大小、移动数据、取消、重试、
损坏、存储、离线、重启、fatal diagnostics 及它们直接使用的控件。264 个接受键中
有 20 个可复用候选；其它业务键不进入 bootstrap 子集。

| key | zh-Hans | en | parameters | 结论 |
| --- | --- | --- | --- | --- |
| `fm.common.action.cancel` | 取消 | Cancel | 无 | 通用取消可复用；下载页优先用专用键。 |
| `fm.update_ready.title` | 更新已准备好 | Update Ready | 无 | 复用。 |
| `fm.update_ready.body` | 新版本已下载。重启后应用更新，也可以稍后继续使用当前版本。 | A new version has been downloaded. Restart to apply it, or keep using the current version for now. | 无 | 复用。 |
| `fm.update_ready.restart_button` | 重启并更新 | Restart and Update | 无 | 只用于已准备更新，不可当 fatal 通用重启。 |
| `fm.update_ready.later_button` | 稍后 | Later | 无 | 复用。 |
| `fm.initial_download.title` | 下载游戏内容 | Download Game Content | 无 | 复用。 |
| `fm.initial_download.summary` | 需要下载 {totalSize} 的游戏内容，剩余 {remainingSize}。当前网络：{networkName}。 | Game content download: {totalSize} total, {remainingSize} remaining. Current network: {networkName}. | `networkName;remainingSize;totalSize` | 复用；网络名必须是受控本地化值，不得显示 SSID。 |
| `fm.initial_download.start_button` | 开始下载 | Start Download | 无 | 复用。 |
| `fm.initial_download.cancel_button` | 取消下载 | Cancel Download | 无 | 复用。 |
| `fm.initial_download.failed` | 下载未完成。已保留当前可用内容，请重试。诊断码：{errorCode} | The download did not complete. Currently available content was preserved. Try again. Diagnostic code: {errorCode} | `errorCode` | 复用为普通可重试失败正文；仍缺明确重试按钮。 |
| `fm.mobile_data.title` | 使用移动数据？ | Use Mobile Data? | 无 | 复用。 |
| `fm.mobile_data.body` | 继续下载将使用移动数据，剩余 {remainingSize}。本次同意不会用于以后的下载。 | Continuing will use mobile data. {remainingSize} remains. This approval will not apply to future downloads. | `remainingSize` | 复用；逐次授权。 |
| `fm.mobile_data.continue_button` | 使用移动数据继续 | Continue with Mobile Data | 无 | 复用。 |
| `fm.mobile_data.wait_button` | 等待 Wi-Fi | Wait for Wi-Fi | 无 | 复用。 |
| `fm.mobile_data.cancel_button` | 取消下载 | Cancel Download | 无 | 复用。 |
| `fm.diagnostic.placeholder` | 【if you see this, it is a bug.】 | 【if you see this, it is a bug.】 | 无 | Prefab 唯一允许的玩家可见初值。 |
| `fm.diagnostic.missing_binding` | 文本未正确加载。诊断码：{errorCode} | Text failed to load. Diagnostic code: {errorCode} | `errorCode` | 复用为 binding 诊断。 |
| `fm.diagnostic.missing_key` | 缺少文本：{localizationKey} | Missing text: {localizationKey} | `localizationKey` | 复用为缺键诊断。 |
| `fm.diagnostic.parameter_mismatch` | 文本参数不匹配：{localizationKey} | Text parameters do not match: {localizationKey} | `localizationKey` | 复用。 |
| `fm.diagnostic.missing_glyph` | 字体缺少字符。诊断码：{errorCode} | A font character is missing. Diagnostic code: {errorCode} | `errorCode` | 复用。 |

参数集合必须在中英文本完全一致，次序按 CSV `parameters` 精确保存；实现不得拼接、
漏参、加参或将任意异常消息带入玩家文本。

## 4. 汇总给主策的 16 个最小缺键

以下名称是 `RES-01C` 候选合同，不是已经接受的 COPY。主程应把完整清单一次性交给
主策；主策可以在不改变语义／参数的前提下返回更合适的 exact key name，但必须提供
一一映射、双语成品文案、severity、state/trigger、qa_notes，并走独立 COPY review。
实现者不得自己写成品文案。

| 候选 key | 必需语义 | parameters | 建议 severity |
| --- | --- | --- | --- |
| `fm.bootstrap.checking` | 正在检查本地内容／可用版本 | 无 | `info` |
| `fm.bootstrap.retry_button` | 对同一 immutable target 发起新 epoch 的重试 | 无 | `warning` |
| `fm.bootstrap.offline.title` | 当前离线 | 无 | `warning` |
| `fm.bootstrap.offline.body` | 空缓存且网络不可用，不能进入主要内容；本地 bootstrap 仍可解释并重试 | 无 | `error` |
| `fm.bootstrap.corrupt.title` | 下载或缓存内容损坏 | 无 | `error` |
| `fm.bootstrap.corrupt.body` | 只会清理／重取精确目标，旧完整集合保留；显示受控诊断码 | `errorCode` | `error` |
| `fm.bootstrap.storage.title` | 存储空间不足 | 无 | `error` |
| `fm.bootstrap.storage.body` | 显示所需空间和当前可用空间 | `availableSpace;requiredSpace` | `error` |
| `fm.bootstrap.fatal.title` | 无法安全启动 | 无 | `blocking` |
| `fm.bootstrap.fatal.body` | 固定 bootstrap 自身发生不可继续的受控故障 | `errorCode` | `blocking` |
| `fm.bootstrap.restart_button` | 通用“重新启动”动作，不暗示更新已安装 | 无 | `warning` |
| `fm.bootstrap.exit_button` | 退出应用 | 无 | `info` |
| `fm.network.kind.wifi` | 受控网络类别：Wi-Fi | 无 | `info` |
| `fm.network.kind.mobile` | 受控网络类别：移动数据 | 无 | `warning` |
| `fm.network.kind.offline` | 受控网络类别：离线 | 无 | `warning` |
| `fm.network.kind.unknown` | 受控网络类别：未知／正在确认 | 无 | `warning` |

计数门：接受的 264 键中现有候选 `20`；缺失 `16`；若主策不合并／替换语义，最终
bootstrap 精确子集候选为 `36` 键，live authority 将至少为 `280` unique keys。
只有新的 COPY verdict、CSV SHA／bytes／records 和后续 LOC 产物 receipt 才能改变
这些数字。不得把本文的候选键直接手写进 bootstrap JSON。

## 5. 动态参数边界与字形集合方法

### 5.1 参数边界（实现包签发前冻结）

| 参数 | 唯一允许来源／格式 | 长度门 | 禁止 |
| --- | --- | ---: | --- |
| `totalSize`、`remainingSize`、`availableSpace`、`requiredSpace` | Host 的同一个 invariant byte formatter；字符只来自 ASCII 数字、`.`、空格及 `B/KB/MB/GB` | 每值 `1..16` Unicode scalars | 原始 long 拼接、locale-dependent 系统格式、负数、无限／NaN、任意单位。 |
| `networkName` | 只能先解析到 §4 四个 `fm.network.kind.*` key，再取当前 locale 成品文本 | `1..24` scalars | SSID、运营商、接口名、系统错误、任意外部字符串。 |
| `errorCode` | Host 固定诊断映射；字符集 `[A-Z0-9._-]` | `1..32` ASCII | exception message/type/stack、URL、路径、对象 ID、服务器正文。 |
| `localizationKey` | 当前 bootstrap allowlist 中的 exact key；字符集 `[a-z0-9._-]` | `1..96` ASCII | 任意用户输入、整份模板、未登记 key。 |

超界不是截断后继续显示；必须进入固定 `fm.bootstrap.fatal.*`／binding diagnostic，
同时保留内部证据。富文本标签、换行和双向控制字符一律不允许作为动态参数。

### 5.2 确定性 glyph-set

生成方法必须由正式 264+ amendment 的同一 CSV／Luban authority 驱动：

1. 取最终获接受的 36 个 exact keys，按 key ordinal 排序；同时取 `zh_cn` 与 `en`。
2. NFC 规范化；模板语法只移除 `{parameterName}` token，保留所有玩家可见标点、
   普通空格和 `【】` 占位符字形；拒绝未闭合／未知 token。
3. 加入动态闭集：ASCII 数字、大小写英文字母、`.`、`_`、`-`、`/`、`%` 和普通
   空格。实际参数仍受 §5.1 更窄的逐参数集合约束。
4. 以 Unicode scalar ordinal 去重／排序，输出 glyph receipt（key SHA、字符数、
   codepoint 列表、Noto/TMP asset SHA）；调用共享 Noto TMP asset 的
   `HasCharacters`／预热验证。不得创建第二个字体资产或远程 fallback。
5. 两种 locale 逐页运行时绑定后扫描全部 active/inactive `TextMeshProUGUI`；任何
   missing glyph 都阻断接收。Android 实机字形／内存仍留给后续设备门。

当前可复证的预览计数（不是最终 glyph freeze）：20 个现有键去掉参数 token 后，
中英 literal union 为 `122` scalars（含一个普通空格，非空白 `121`）；加入上述
`68` 字符动态闭集后 union 为 `152`（非空白 `151`）。全部 264 键的 literal union
为 `449`（非空白 `448`）。16 个缺键没有接受文案，因此最终 36 键计数现在不可诚实
宣称；COPY 接收后必须重算。

## 6. 未来资产／文件路径模板

### 6.1 首选复用，不创建第二套

`RES-01C` 应直接引用 uGUI 最终接受的：

```text
Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset
Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset.meta
Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular.otf
Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular.otf.meta
Assets/UI/FightMatch/Fonts/OFL.txt
Assets/UI/FightMatch/Fonts/OFL.txt.meta
Assets/TextMesh Pro.meta
Assets/TextMesh Pro/Resources.meta
Assets/TextMesh Pro/Resources/TMP Settings.asset
Assets/TextMesh Pro/Resources/TMP Settings.asset.meta
Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt
Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt.meta
Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt
Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt.meta
Assets/TextMesh Pro/Shaders.meta
Assets/TextMesh Pro/Shaders/TMP_SDF.shader
Assets/TextMesh Pro/Shaders/TMP_SDF.shader.meta
Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader
Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader.meta
Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc
Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc.meta
Assets/TextMesh Pro/Shaders/TMPro.cginc
Assets/TextMesh Pro/Shaders/TMPro.cginc.meta
```

共享 Noto TMP asset 的 material／atlas 必须继续是同一 `.asset` 的 sub-assets。
bootstrap 不复制 `FightMatchRuntimeRoot.prefab`：它含主导航／战斗等被 §11 明确排除
的业务页面；bootstrap 只复用其已接收的基础资源合同和 SafeArea 能力。

### 6.2 新文件候选（签发包必须逐项确认）

```text
Assets/FightMatch/Bootstrap.meta
Assets/FightMatch/Bootstrap/Scenes.meta
Assets/FightMatch/Bootstrap/Scenes/FightMatchBootstrap.unity
Assets/FightMatch/Bootstrap/Scenes/FightMatchBootstrap.unity.meta
Assets/FightMatch/Bootstrap/Prefabs.meta
Assets/FightMatch/Bootstrap/Prefabs/FightMatchBootstrapRoot.prefab
Assets/FightMatch/Bootstrap/Prefabs/FightMatchBootstrapRoot.prefab.meta
Assets/FightMatch/Bootstrap/Graphics.meta
Assets/FightMatch/Bootstrap/Graphics/BootstrapStatusIcons.png
Assets/FightMatch/Bootstrap/Graphics/BootstrapStatusIcons.png.meta
Assets/FightMatch/Bootstrap/Localization.meta
Assets/FightMatch/Bootstrap/Localization/fm-bootstrap-v1.json
Assets/FightMatch/Bootstrap/Localization/fm-bootstrap-v1.json.meta
Assets/FightMatch/Bootstrap/Localization/fm-bootstrap-v1.receipt.json
Assets/FightMatch/Bootstrap/Localization/fm-bootstrap-v1.receipt.json.meta
Assets/Scripts/FightMatch/Host/FightMatchBootstrapController.cs
Assets/Scripts/FightMatch/Host/FightMatchBootstrapController.cs.meta
```

`BootstrapStatusIcons.png` 仅允许两个中性单色图标（warning、network），候选为
`64×32 RGBA`、2 个 `32×32` FullRect sprites、无 mipmap、无额外 material。背景、
进度条、按钮均用 uGUI `Image` 的纯色／内置白图，不再创建 texture。若最终布局可用
文字和纯色达到同等可读性，工程 owner 可在签发前删掉该 PNG 路径及其 meta；不得在
实施中临时增加其它图片。最终画风、插画、动画、音频和主游戏资源不属于本包。

测试路径、PlayMode asmdef、启动 scene/reference 修改路径尚未在接受设计中精确冻结，
由工程 owner 在真正 `RES-01C` 实施包中列名；这不是 art owner 可代签的白名单。

## 7. GUID／meta 冻结顺序

1. 签发前记录 HEAD、uGUI verdict、LOC-A verdict、COPY verdict 和所有共享输入
   SHA／bytes／GUID；任一漂移即 `BLOCKED_INPUT_DRIFT`。
2. 对全 `Assets` 做 GUID 唯一性扫描。§2.3 官方 TMP 项只能保留官方 GUID；OTF/OFL
   只能保留 §2.1 既有 GUID；不得手写、复制或重映射。
3. 由唯一 Unity C 按目录→PNG→scene→prefab→JSON TextAsset→Host script 的实际
   创建顺序自然生成项目自有 `.meta`。每生成一项立即记录 path/type/GUID/bytes/SHA。
4. 先保存并复开共享 uGUI/TMP，再生成 bootstrap prefab/scene；所有引用写入后
   `SaveAssets`／`SaveScene`，关闭再开一个独立进程核对。
5. 最终 receipt 必须列出每个新文件及 `.meta`，并证明没有额外叶文件、重复 GUID、
   missing script/reference、旧 UI Toolkit／TextCore 依赖或远程-only 依赖。

## 8. 两个阻断性资源门

### 8.1 零远程依赖遍历

以 bootstrap scene、root prefab、bootstrap localization artifact、共享 Noto TMP
asset 和 `TMP Settings` 为根，调用 Unity `AssetDatabase.GetDependencies(..., true)`，
把每个依赖的 path／GUID／SHA 写入排序 receipt。通过条件：

- 闭包只包含签发 allowlist、Unity builtin、已接收共享 uGUI/TMP/font/license；
- 不包含主导航／battle/map/roster Prefab、完整 business catalog、remote bundle、
  Addressables、UI Toolkit `UIDocument/PanelSettings/Theme`、旧 TextCore font asset；
- 清空 Yoo cache、阻断网络／host 不可达时，英／简中离线、大小、移动确认、取消、
  重试、损坏、存储和 fatal 页面仍可读；
- `Shader.Find` 标准／Mobile SDF 命中，line-break、font/material/atlas 均非空；
- 不把磁盘存在、Editor cache 或已下载 bundle 当作 Player-built 依赖证明。

### 8.2 runtime placeholder replacement

- prefab 中每个玩家可见 TMP 序列化初值必须逐字为
  `【if you see this, it is a bug.】`；不得存成中／英文成品。
- 分别以 `zh-Hans` 和 `en` 创建 bootstrap，在首帧可见前完成同一 localization
  service 的 key＋named-args 绑定；随后扫描 active/inactive TMP，placeholder 数为 0。
- 缺 key、参数不一致、缺 glyph、font/material 为空时必须显示接受的固定诊断并阻断
  页面接收，不得回退硬编码成品文案或另一语言。
- 唯一性门同时证明：一个 bootstrap RuntimeRoot、一个 Canvas、一个 EventSystem、
  一个 SafeAreaRoot；不得嵌套主游戏 RuntimeRoot 造成第二套对象。

## 9. 预算与回滚

### 9.1 冻结预算

| 项目 | 上限／规则 |
| --- | --- |
| Scene／Prefab | 1 个 bootstrap scene＋1 个 bootstrap root prefab。 |
| 新美术二进制 | 最多 1 张 `64×32` icon sheet，源 PNG `≤8 KiB`；0 插画、0 动画、0 音频、0 第二字体。 |
| 字体／TMP | 只复用 1 个获接受的 Noto TMP asset、其内嵌 material/atlas、1 份 OTF/OFL 和 §2.3 精确官方资源。 |
| 文本 | 候选 36 keys；双 locale；artifact＋receipt 合计建议 `≤64 KiB`。最终 key 数和 bytes 必须由 COPY/LOC receipt 取实测值。 |
| 生产代码 | 接受设计硬上限：最多 4 个 production C#，合计 `≤700` physical lines。 |
| 测试 | 接受设计硬上限：最多 3 个 focused tests，合计 `≤900` physical lines。 |
| 总依赖大小 | 签发前以 dependency closure 实测；当前设计没有接受的总 byte ceiling，不能虚构。未由工程 owner 冻结精确增量 byte 上限前保持 `BLOCKED_SIZE_BUDGET`。共享 16,437,364-byte OTF 只计一次。 |

### 9.2 回滚

1. 回滚前先确认没有后续包依赖 `RES-01C` 新 GUID。
2. 恢复实施包记录的 prior startup scene/reference 原字节；不得修改玩家档、清空全部
   Yoo cache、删除旧完整 ReleaseSet 或回滚业务保存。
3. 只删除 §6.2 最终签发清单中的新文件及自然 `.meta`；若 icon 在签发前取消，则
   不应出现在回滚清单。
4. 不删除或重建共享 uGUI prefab、Noto OTF/OFL/TMP、官方 shader/settings/
   line-break 资源；它们属于先行获接受输入。
5. 再跑依赖／GUID／启动引用静态核查，证明没有 dangling reference；需要 Unity
   compile/reopen 时只能由串行 C 按正式验证包执行。

## 10. 责任拆分与未运行项

| owner | 输入／责任 | 明确不负责 |
| --- | --- | --- |
| 主策／本地化 | §4 的 16 个缺键；双语成品、参数同构、severity、trigger、qa_notes；新的 COPY verdict 和 accepted CSV identity。 | 不写 bootstrap JSON，不改 Host，不导入 Unity。 |
| 主美／资源 | 本文盘点；最多一张中性 icon sheet 的源规格；字体来源／许可／glyph 方法；资源预算和验收检查。 | 不运行 Unity，不创建 asset/meta，不决定下载状态机。 |
| LOC-A／后继 LOC owner | 从获接受 CSV／Luban authority 确定性生成完整 catalog，并提供可追溯 receipt；后继包生成精确 bootstrap 子集。 | 不接受自己的产物，不手写第二权威。 |
| RES-01C／Host owner | 下载政策状态机、同 target/epoch retry、Wi-Fi→mobile cancel、storage/corruption/fatal 映射、运行时绑定、dependency traversal、测试和 evidence。 | 不改 COPY 文案，不扩业务／存档／remote code。 |
| 串行 Unity C | 按最终白名单生成自然 meta、保存／复开 scene/prefab/TMP 绑定并运行获准验证。 | 不吸收 R2、付费、Windows 或全量 Android release。 |
| 独立 R／testing lead | 检查实际 patch、资源闭包、empty-cache/offline/locale/network-policy 证据并给唯一 verdict。 | 作者不能自接收。 |

本次明确 `NOT RUN`：Unity、EditMode、PlayMode、Player build、Android 构建／设备、
Windows、R2／Cloudflare、付费服务、远端上传、全量 Android release、独立 R。当前
阻断项精确为：

1. uGUI 最终 prefab／Noto TMP／scene／meta／reopen evidence 尚未获接收；
2. LOC-A `PASS_LOC_IMPL_A` 候选尚未取得原独立 R 的 `ACCEPT`；
3. §4 的 16 个 COPY 缺键尚无双语接受文本；
4. 最终 bootstrap asset whitelist、自然 GUID、启动引用、总依赖 byte ceiling 尚未由
   工程 owner 补签；
5. RES-01A/RES-01B 的实际前置接收必须由主程在正式派发时再次核对，本文不推定通过。

