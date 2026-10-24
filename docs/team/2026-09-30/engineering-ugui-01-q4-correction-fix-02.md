# UGUI-01-Q4-CORRECTION-FIX-02：复用原生补入的 CanvasRenderer

2026-10-01 · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 结论、权限与边界

本包是 `UGUI-01-Q4-CORRECTION-01` 的单点续包。只读诊断已经证明：磁盘 Prefab 的
`BoardFrame` 没有 serialized `CanvasRenderer`，但 Unity 2022.3.18f1 在
`PrefabUtility.LoadPrefabContents` 后、诊断代码访问 `Graphic.canvasRenderer` property
之前，native object 已经有且仅有一个 `CanvasRenderer`。原 recovery 仍要求 native
count=0，因此在保存前触发 FM029。

- 用户 authority：仓库 `AGENTS.md` 第 6 节已批准的五角色持续执行流程。
- 唯一执行 owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52 / local`。
- 签发与接收：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b / local`。
- 最终修法：保留磁盘 serialized count=0 的强门；要求 native loaded count **精确为 1**，
  复用这个已存在组件并原生保存。不得调用 `AddComponent<CanvasRenderer>`，也不得同时
  接受 native 0/1 两种未冻结行为。

本包不重开 UI/COPY/Application/Input/save 设计，不改另外六个 correction 源文件，不
修改 Scene/font/meta，不运行 Android/APK/device。任一步失败即保存证据并停止，不再
追加修法或重跑。

## 1. 固定输入与已接收诊断

### 1.1 上游包

| 输入 | 固定身份 |
| --- | --- |
| 原 correction packet | `engineering-ugui-01-q4-correction.md` / SHA-256 `2e8615a0b38dc7e141043ee5d077d9bb5c13f85c5665206f64c1ccba9fb9bebc` |
| 只读 diagnosis packet | `engineering-ugui-01-q4-recovery-diagnosis.md` / SHA-256 `895b5c9ae8446e0d5851dc59554f603402233357fd70d59588b747c6aad7c017` |
| first blocked candidate | `f21c8c41b76817cafc1ec7be812f115ac65e74122c476c60b0c559834dcc1486` |
| diagnosis candidate | `8a3cd796241ceb6fb953544771b8a4edb4bdf272d79a3631d6367dd7ef1b1c43` |
| diagnosis final receipt | `TestArtifacts/FightMatch/UGUI-01/8a3cd796241ceb6fb953544771b8a4edb4bdf272d79a3631d6367dd7ef1b1c43/q4-recovery-diag-01/diagnostic-receipt.json` / 32,560 bytes / SHA-256 `d82258c89c62b0324b54e0c8f351c3fd4e1b80f33cb8565593e44781dde81a6a` |
| native observation | 1,805 bytes / SHA-256 `90fba1a6c4bd50bffab76b85f8f0b2c71c6b26880829a81175905d0589183f97` |
| quiescence follow-up | 664 bytes / SHA-256 `4d581867b8c10bc07b25eadf989cd440c4da113c5795d09346d0272b17f37e05` |

诊断 verdict 为 `DIAGNOSED_H1`：root non-null、BoardFrame count=1、script asset 存在且
GUID `5a7f13aa50aa909428d61ff8d87781d2`、734 个 MonoBehaviour 中 matching board count=1、
board 在唯一 BoardFrame 且 enabled。磁盘 class IDs=`224,114,114,114`、serialized
renderer=0；native component types 精确为 RectTransform、LayoutElement、
CandidateBoardElement、FightMatchViewId、CanvasRenderer，renderer count=1。

诊断 Unity exit=0、23.702846s；没有 AddComponent/SaveAsPrefabAsset/SaveAssets。raw wrapper
在 Unity 刚退出时仍观察到尾部子进程，所以原 run 保留 `passed=false`；随后只读补核确认
全部 10 个记录 PID 和 Unity process 自然消失，未发信号、未启动第二次 Unity。该区分
必须保留，不得改写 raw receipt。

### 1.2 当前 source/resource 起点

| 路径 | bytes / SHA-256 |
| --- | --- |
| `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` | 88,423 / `296683192ea817ed93ff5771d1033e7a4c6767170ec92daa8214606f0c077689` |
| `CandidateBoardElement.cs` | 12,750 / `956744b6f5f1fb571e5c069589d422e0dd8f668b604f50c35f00b22c8cdc357d` |
| `CandidateBoardInputTestData.cs` | 27,769 / `34c37acf134231045ee0752d14dc647a8567ba9b76e804b2eed546410fa6981c` |
| `FightMatchHostTestFixture.cs` | 21,477 / `81f3009728dc0522253a1f56e642225829e767c54e2031e2a8ee90e7cb0d79a8` |
| `LocalizedTextBindingTests.cs` | 13,982 / `095ce3720258d860c70883201288c009efa397cddcb6bad5ec7bc713f6358cd1` |
| `PlayerNavigationRecoveryView.cs` | 11,337 / `dbfdfe0e333777d19d76e8a84860d6912fbd916e44425a99cbb7b1a9ffd128e9` |
| `PlayerNavigationPresentationTests.cs` | 32,437 / `0ee946a00433e0fdebb08b1699ebc6cbafb8816d88ea4a53ac26a1f1631279a4` |
| Scene | 12,900 / `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd` |
| Runtime Prefab | 1,060,275 / `934e5bbec0c6cec2a16afa4778fb019a3420e3432cc0d75e07703e6f4cd60487` |
| TMP font | 8,546,563 / `66e7963607e4e33432af150ebd085e7cdac1324133e102eaf87da24c135febdb` |

诊断前后 53 source、37 resources、原 b9 root 49 文件、f21 root 32 文件均保持固定。

## 2. 精确代码写范围

只允许修改：

```text
Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs
```

必须同时完成且仅完成以下变化：

1. 完整删除 `DiagnoseUguiNativeBoard`、`UguiNativeBoardFrame`、
   `UguiNativeBoardDiagnosis` 及本次 `[DEBUG]`/diagnosis-only 代码；最终产品 Editor source
   不保留临时探针。旧 diagnosis evidence 只读保留。
2. `RecoverUguiBoardRenderer` 绑定本 FIX-02 packet SHA 与新的 evidence path：
   `TestArtifacts/FightMatch/UGUI-01/<NEW_SOURCE_CANDIDATE>/q4-correction-02/resources/recovery/renderer-recovery.json`。
   candidate 必须排除 b9、f21、8a3 三个历史值，output create-new。
3. recovery 在 native load 前仍解析磁盘 Prefab，并严格断言：唯一 BoardFrame、固定
   GameObject fileID、原 4 个 component fileID/class ID/script GUID、serialized class ID
   222 count=0、Prefab bytes/SHA/GUID、53 source 与 37 resources 全部符合 run binding。
4. `LoadPrefabContents` 后分别断言：唯一 BoardFrame、唯一 CandidateBoardElement、script
   GUID 正确、board 在该 frame、board enabled、native CanvasRenderer count=1；可同时记录
   native component type order。不得访问 `Graphic.canvasRenderer` property 来制造结果。
5. **删除** recovery 中的 `AddComponent<CanvasRenderer>()`。native count 不是 1 就停止，
   不接受 0 时添加、>1 时删除或任何自愈分支。
6. 直接用 `SaveAsPrefabAsset` 保存 loaded contents。保存后保留并执行原严格证明：
   Prefab GUID 不变；serialized objects 精确 `+1`；新增 object 是唯一 class ID 222；
   BoardFrame 只新增一个对应 component reference；全部旧 fileID 与旧 serialized block
   移除该单行后逐字节相同；Scene/font/其余 36 resources 原字节不变；重载 asset 后
   BoardFrame CanvasRenderer count=1。
7. `UguiOutput` 的原 Prepare 路径保持；VerifyResources 的两个 correction leaf 从旧
   `q4-correction-01` 精确改为当前 `q4-correction-02` 的 `reopen`／`final-reopen`。仍禁止
   b9/f21/8a3 写入、任意目录或通配 suffix；Verify 内部全部断言逐字不变。

除上述单文件外，另外六个 correction source 当前 bytes 必须逐字不变。不得修改
CandidateBoardElement、fixture、tests、runtime/view、Prefab YAML、Scene、font、meta、
Package/ProjectSettings、Git 或其它 docs。

## 3. 新 candidate 与 evidence

单文件清理/修正后重算排序 53-source manifest，其 canonical SHA 为
`<NEW_SOURCE_CANDIDATE>`。只新建：

```text
TestArtifacts/FightMatch/UGUI-01/<NEW_SOURCE_CANDIDATE>/q4-correction-02/
```

保存 packet、preflight、single-source delta、Q1、recovery、Q2、Q3、targeted、Q4、资源
before/after、process receipts 与 final manifest。b9、f21、8a3 roots 全部只读；不得把
旧失败/诊断输出复制成新运行结果。

## 4. 执行顺序与停止规则

每个 Unity process 前做独占进程检查；任何其它 Unity 打开即停止。每步前后冻结 53
source、37 resources、Prefab/font 原字节及旧 roots。

1. **Source/Q1**：只改 §2 单文件，另六源逐字不变；新的 Q1 raw + 已接受四个 Unity
   YAML 空字段 adjudication 通过。确认 final source 中 diagnosis-only 类型/方法为零，
   recovery 内 `AddComponent<CanvasRenderer>` 为零。
2. **Recovery**：一次、≤300s。只允许 §2 的 native-count=1 reuse 路径。成功必须产生
   exactly-one serialized CanvasRenderer；除 Prefab 外资源 delta=0。任何 guard/save/post
   compare 失败即停止，不跑后续。
3. **Q2**：新 process、一次、≤300s。复开验证 Scene、修复后 Prefab、当前 font、119
   TMP/3 linked input/116 placeholder、CanvasRenderer/EventSystem/font/shader/TMP Settings；
   冻结实际 37-resource identities 为新的 Q2 baseline。
4. **Q3 compile**：新 process、一次、≤300s；exit 0、无 compiler/unhandled error，
   source/resource 与 Q2 baseline 不变。
5. **Targeted 9**：新 process、一次、≤180s；精确原 9 fullnames，必须 9/9、零
   failed/skipped/inconclusive，资源不变。失败则不跑 222。
6. **Q4 exact 222**：新 process、一次、≤600s；名称集合精确冻结 222，必须 222/222、
   零 failed/skipped/inconclusive。
7. **Final resource stability**：若 Q4 后 37/37 与 Q2 baseline 相同，直接封存。只有 dynamic
   font 单独变化且有结构证据时，才按原 correction §4.7 最多一次 final reopen；否则
   `BLOCKED_RESOURCE_DRIFT`。

最大 Unity launches 为 recovery/Q2/Q3/targeted/Q4 共 5；仅第 7 步条件成立才有第 6 个。
失败后不重跑、不运行 4181/Q5/Android/APK/device。

## 5. 验收与回传

- [ ] 最终相对原 b9 的产品/测试 delta 仍精确为原 7 source + 1 Prefab；本包相对 8a3
  只改一个 Editor file，并已移除所有 diagnosis-only code。
- [ ] on-disk 0 / native-loaded 1 的 H1 回归门实际通过；recovery 没有 AddComponent、没有
  手写 YAML；保存后只新增一个 serialized class ID 222 和一个 BoardFrame reference。
- [ ] Prefab GUID、所有旧 serialized objects、Scene/font/其余资源保持；新 Q2 baseline
  与 final 资源身份稳定。
- [ ] 另外六个 correction 根因 patch 未改；H05、Verify、raycast/mesh、business assertions
  和 222 名称集合未放宽。
- [ ] targeted 9 与 full 222 实际全绿；不得用 compile/Q2 代替测试。
- [ ] 每一进程有真实 argv/PID/UTC/duration/exit/log/XML/quiescence；退出瞬间与后续
  quiescence 若不同，必须像 diagnosis 一样分别保留，不覆盖原 receipt。
- [ ] Q5/4181/Android/APK/device 均 `NOT_RUN`。

Owner 回传：新 candidate/source manifest；本单文件 before/after bytes/SHA 与最小 diff；
diagnosis cleanup 证明；Prefab before/after/GUID/fileID/component 证明；Q1、recovery、Q2、
Q3、targeted9、Q4 receipts/XML/log；Q2/final 37-resource manifests；font structure；旧 roots
unchanged；未运行项。完成后停止，不得自行请求 QA/R、提交、推送、建分支或改 Git。

主程审实际 patch/evidence 后才送 QA；QA 回同一 R-TOOLS
`01a0f3e3-9663-7be2-84a8-ed23be32f0b9 / local` 作最终独立验收。
