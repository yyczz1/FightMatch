# N11R1 完整序列化差量诊断

2026-10-06。SYS actual `01a10f26-7313-7aa2-882a-d289f1fe882d`（read_thread实核）。
主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`唯一收件；issuer `01a10f24-f8a4-7cb0-9a7b-835c366a6146`。按AGENTS§6及[中央短单](central-layout-n11r1-receipt-next.md)后继B。

## 结论

完整重建：**原Scene12900B + 79覆盖14023B + marker875B = 两个27798B后像**。其它字节不变；五条无Set冷载重现、保存复开保留。“磁盘五条0”不等于有效缩放0。

保留 **NEEDS_FIX/P1/历史UNKNOWN**，非审查或Player验收。diagnosing-bugs/uGUI Understand只读复用封存证据；仅写本稿，无运行/投影/产品改动。

## 固定输入与实际机械命令

以下N=`TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N11R1/`；实核：

| 文件 | bytes | SHA256 |
| --- | ---: | --- |
| N/PExecution/receipt.json | 390157 | `2b14702e26abaca73b22644635abea6fe75aa1ad1de1df5364e29697ea87405d` |
| N/P/native-probe.json | 369276 | `25fbc18732fe9674b84640acc00ee403ec0c523658ae04fe68e422be6b564875` |
| N/S/scene-original.unity | 12900 | `0ceee6f17e953…` |
| N/S/scene-cold22.unity | 12012 | `9eceb484adf68…` |
| N/P/scene-after-save.unity = scene-final.unity | 27798各 | `69a927c8f43e…` |

Scene完整SHA匹配receipt/native封存字段，表仅缩写。N10诊断8180B/`fd574aae84eb…`、短单6264B/`39a14770e59f…`实核；共享只读Prefab1322960B/`0064bbee5e0d…`匹配receipt.resources37；ResponsiveLayout/PlayerHost哈希同N10。

命令：`shasum -a 256`、`jq`、`rg/nl/sed`及只读`node <<'NODE'`（fs/Buffer/crypto/assert），未导入runner。重建e5189d、严格断言858ddf、映射b584f6均exit0；核GUID/字段/引用/唯一性/顺序/全Buffer.equals。

## 字节覆盖证明

偏移0起半开区间，latin1无损解析，不重排YAML：

- marker：GameObject1858777893 [26790,27185)/395B；Transform1858777894 [27185,27640)/455B；SceneRoots引用[27773,27798)/25B。结构及三片段SHA匹配receipt。
- 剥离后26923B，SHA `71bf02138c95154f743b27d302d156fb25ba3776cd841f86676e15d677f811ad`。冷22仅删五块888B，全Buffer相等。
- 原Scene修改序列[7070,11937)，4867B；剥离后[7070,25960)，18890B。前7070B与后963B逐字节保留；以新序列替换原序列所得26923B与剥离后像完整相等。
- 新序列中79条新增原始块共14023B，按出现顺序拼接SHA `5f43d8eb0747305b28e54bfe17caa8da49e41622ed9070d002b8ae663392aa8f`。其五个连续区间为[7070,9542)/14条、[11138,12200)/6、[12920,15412)/14、[17603,21157)/20、[21517,25960)/25。移除这些区间完整复得原12900B。
- 原27每块target/property/value/reference及原始字节各保留一次、相对顺序不变（绝对索引因插入变化）；106无重复。原15对象/顺序保留，只有PrefabInstance1017091371的m_Modifications变；其余对象、头尾、serializedVersion/ID/GUID/字段/数值/空白均相同。

无未覆盖字节；不代表已定位原生writer。

## 79条逐对象归属

C=`RuntimeCanvas/SafeAreaRoot/ScreenLayer/StartupPage/StartupScroll/Viewport/Content`，H=`RuntimeCanvas/SafeAreaRoot/HudLayer/TopBar`（均在FightMatchRuntimeRoot下）。A=anchorMax.xy、anchorMin.xy、sizeDelta.xy六条；B=anchoredPosition.xy两条。A/B值全部字符串0、objectReference均fileID0；target GUID均c36df3cfc25424c8cb3ec6cae6be1237/type3。

| target fileID | 路径 | 新增 |
| --- | --- | --- |
| 20060541972559401 | C/Create | A |
| 525732035437362821 | C/Reload/Caption | A+B |
| 2083101828246034960 | C/ProfileTitle | A |
| 3603424187799618609 | C/Reload | A |
| 3613386703531820566 | C/Create/Caption | A+B |
| 5408553659951591733 | H/Back | A |
| 6280369087512753137 | C/Continue/Caption | A+B |
| 6783473654260464751 | C/Continue | A |
| 8076177029604565045 | H/Language | A |
| 8217930521597258139 | H/License | A |
| 9020564264068189081 | H/Title | A |
| 9181528876988089927 | C/StartupStatus | A |
| 9054230707113910417 | RuntimeCanvas（Canvas组件） | m_AdditionalShaderChannelsFlag=25 |

合计12×6+3×2+1=79；前12为RectTransform，路径由固定Prefab父链解析。

79条首次见cold-two-updates。C及三按钮的VerticalLayoutGroup、H的HorizontalLayoutGroup均启用/控制子宽高；uGUI1.0.0 LayoutGroup.cs294–314驱动anchor/position/size。TMP3.0.6 TMPro_UGUI_Private.cs4423–4426向Canvas通道OR25。均与观察相容，但没有子项driver/call stack；LayoutGroup的anchor写值含y=1，不能说磁盘0是C#原样赋值或已定位唯一writer。

## 五边界：重现事实与未知

| 边界（log行） | 内存条数 | 磁盘条数 | 变化 |
| --- | ---: | ---: | --- |
| cold-open-returned（366） | 27 | 22 | 内存精确复得原27，磁盘仍冷22 |
| cold-two-updates（395） | 106 | 22 | 内存加入全部79 |
| save-returned（410） | 106 | 106 | marker及106落盘 |
| reopen-returned（429） | 106 | 106 | 内存序列与磁盘106精确相同 |
| reopen-two-updates（441） | 125 | 106 | 另19仅内存出现，磁盘未变 |

末19=九个非Caption Rect的anchoredPosition.xy＋C本身6960656654093734010的sizeDelta.y，均0；106原序列保留无删改重复。C的ContentSizeFitter verticalFit2/源码86–119与高度项相容，未证明调用链。

五次75项geometry完全相同/有限：local/lossyScale三轴≈.769800365、anchorMax=(0,0)、pivot=(.5,.5)、rect831.3844×623.538269、pixel640×480，角误差≤7.05013736e-6。driver=自身Canvas，Overlay/root/active=true，dirty=false。五记录0≠三scale有效值，anchorMax则相同。只排除这些Editor时点为零缩放，不排除Player首帧风险。

首次出现仅指OpenScene返回后的Observe边界（含Get等访问器）；无Set重现已证实，不依赖N10旧Set残留，具体回调/writer未知。driver不是逐字段mask；driven/default语义见N10官方出处。

## Unity成功与wrapper UNKNOWN

native complete=true、firstError空、secondaryErrors空，Open1/Save1=True/Reopen1/Close2/marker1/自然update4；Unity exit0（中央实收22.315秒）。runner.classify_image仅把剥离marker后精确等于原27或冷22分成两个已知类；26923B都不等，330–343后检查抛UNKNOWN，wrapper exit1。另有meta身份漂移清理诊断/三null保护字段，由A处理，不宣称投影干净。补全79条解释不追改旧UNKNOWN，非Unity保存失败。

## 唯一后继：真实Scene首次布局门回归

原P1（head `e3d581df42f3a25cfdfc319bca5509f8b5382ada`）担心零覆盖触发Apply拒绝，Prefab测试漏Scene。无Set重建/几何正削弱“磁盘22/仅删五块”断言的必要性，未证明Player安全。向中央提议**精确序列化包络＋首次布局/交互门**，不自行改门/撤P1。

一个后继包：固定原27真实Scene/Prefab/运行时代码，隔离冷入真实Host Play，仅观测OnEnable、首次Host.Bind→Apply、SafeArea版本、尺寸/scale、Gate原因、EventSystem输入和首个渲染。禁InstantiatePrefab替代、预Force、手动Bind/Apply、Set/Save及注入LOC成功源。

可证伪门：在本包固定的支持竖屏/safeArea下，首个有效Canvas+safeArea样本后最迟下一次自然willRender完成布局、解除布局锁；无效阶段关闭该组交互/取消已有指针，恢复不提交取消手势。必须区分布局Gate(true)与真正游戏输入通过，后者需要真实会话和非空输入。现Host35仍null source、37–40停止Session：首次布局Bind可观察，非空交互项则BLOCKED，不能绕过LOC。若主程已有独立批准的就绪候选，先固定其版本；否则完整功能门不关闭，本包不向LOC发新任务。

预算：启动/Play各1，≤10帧或10秒取先到，总Unity≤60秒，retry/Save/Set/导出0；异常/漂移/未见Bind/超时停。失败按实证另包最小初始化/重采样修复，不猜删资产。执行器/观测白名单/编译预算另批，本稿不授权运行。

原P1、本字节重建、上述功能证据及替代断言必须交**同候选head GitHub独立审查**。N11R1不是原审查head；Editor Play不替代Android设备验收。只有功能门及独立审查成立才能收口原风险。
