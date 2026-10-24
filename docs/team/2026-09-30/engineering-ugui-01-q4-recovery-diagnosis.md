# UGUI-01-Q4-RECOVERY-DIAG-01：原生 Prefab preflight 单点诊断

2026-10-01 · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 目的、权限与停止边界

本包只回答一个问题：原 correction candidate 在 `LoadPrefabContents` 后为何触发
`FM029: UGUI correction native board/renderer before`。它不修复 Prefab，不重跑原
recovery，不运行 Q2/Q3/targeted/Q4，也不改变其余六个 correction 源文件。

- 用户 authority：仓库 `AGENTS.md` 第 6 节已批准的五角色持续执行流程。
- 唯一执行 owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52 / local`。
- 签发与接收：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b / local`。
- 诊断方法：遵循 `diagnosing-bugs`，先产生能区分复合 guard 各分支的只读证据；本包
  禁止依据未记录的假设保存修复。

任一步身份、进程安全、编译、输出路径或资源稳定性不满足，保存真实证据并停止。
不得在本回合顺手修 guard、允许 native count 1、保存 Prefab 或尝试第二种方法。

## 1. 固定输入

### 1.1 上游 correction 与失败证据

| 输入 | 固定身份 |
| --- | --- |
| 上游 correction packet | `docs/team/2026-09-30/engineering-ugui-01-q4-correction.md` / SHA-256 `2e8615a0b38dc7e141043ee5d077d9bb5c13f85c5665206f64c1ccba9fb9bebc` |
| blocked source candidate | `f21c8c41b76817cafc1ec7be812f115ac65e74122c476c60b0c559834dcc1486` |
| blocked receipt | `TestArtifacts/FightMatch/UGUI-01/f21c8c41b76817cafc1ec7be812f115ac65e74122c476c60b0c559834dcc1486/q4-correction-01/blocker-native-prefab-preflight.json` / 3,619 bytes / SHA-256 `738d31c7f9d56a217eae9413f8758c205e4c44ec3f96e249d678b8b71dea09c2` |
| failed process log | 30,568 bytes / SHA-256 `0e02e64bf85f7b3e1174df7b9862214aa9786e97f55edb7e729cbc1f1c4be39b` |
| failed run binding | 28,168 bytes / SHA-256 `b837726594fb64e733785b3b3c26723bb182426d0cc5c1f5dc301a3e8203c654` |
| exact failure | `FightMatchAndroidBuild.cs:503`, `FM029: UGUI correction native board/renderer before` |

该失败发生在 `SaveAsPrefabAsset` 前。失败运行后 53/53 source 不变、37/37 resources
不变，Prefab before/after 均为 1,060,275 bytes / SHA-256
`934e5bbec0c6cec2a16afa4778fb019a3420e3432cc0d75e07703e6f4cd60487`；旧 b9 evidence
49 个文件不变。不得覆盖或续写 blocked candidate evidence root。

### 1.2 当前七源码与三关键资源

| 路径 | 当前 SHA-256 |
| --- | --- |
| `Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs` | `956744b6f5f1fb571e5c069589d422e0dd8f668b604f50c35f00b22c8cdc357d` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs` | `34c37acf134231045ee0752d14dc647a8567ba9b76e804b2eed546410fa6981c` |
| `Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs` | `81f3009728dc0522253a1f56e642225829e767c54e2031e2a8ee90e7cb0d79a8` |
| `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` | `7ce1a48c878469af602c0e87c76fbb24705b0d53513bdbf28bfc33372d34efb9` |
| `Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs` | `095ce3720258d860c70883201288c009efa397cddcb6bad5ec7bc713f6358cd1` |
| `Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs` | `dbfdfe0e333777d19d76e8a84860d6912fbd916e44425a99cbb7b1a9ffd128e9` |
| `Assets/Tests/EditMode/FightMatch/PlayerNavigationPresentationTests.cs` | `0ee946a00433e0fdebb08b1699ebc6cbafb8816d88ea4a53ac26a1f1631279a4` |
| Scene | `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd` |
| Runtime Prefab | `934e5bbec0c6cec2a16afa4778fb019a3420e3432cc0d75e07703e6f4cd60487` |
| TMP font | `66e7963607e4e33432af150ebd085e7cdac1324133e102eaf87da24c135febdb` |

磁盘 Prefab 的 `BoardFrame` 精确一个，GameObject fileID
`4805194842302782578`，现有 component class IDs 为 `224,114,114,114`，候选 board script
GUID 为 `5a7f13aa50aa909428d61ff8d87781d2`，序列化 CanvasRenderer class ID `222` 数量为 0。

## 2. 精确写范围

只允许修改：

```text
Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs
```

唯一允许的代码变化是新增一个 Editor-only、`-executeMethod` 可调用的只读诊断入口及其
private serializable receipt 类型/帮助函数。现有 `RecoverUguiBoardRenderer`、
`UguiOutput`、Prepare/Verify、全部产品/测试逻辑与其余六源码逐字不变。

代码冻结后按新的 53-source manifest 生成 `<DIAG_SOURCE_CANDIDATE>`，只新建：

```text
TestArtifacts/FightMatch/UGUI-01/<DIAG_SOURCE_CANDIDATE>/q4-recovery-diag-01/
```

该根只能保存 packet 副本、source/resource manifests、Q1、process run/log/receipt、原始
Prefab before/after 副本和诊断 JSON。禁止写 blocked candidate、原 b9 root 或其他
evidence root。除新 evidence root 外，37 resources 必须零字节变化。

## 3. 红灯回路与诊断入口合同

### 3.1 静态与运行前门

1. 核对 §1 全部 identity、当前 Git HEAD、Unity `2022.3.18f1` Intel editor、项目无其他
   Unity process；不满足即停止。
2. 只改 §2 单文件；冻结新的排序 53-source manifest 与 candidate SHA；运行既有 Q1 raw
   及已有 scene-whitespace adjudication。除已接受四个 Unity YAML 空字段外必须通过。
3. 新入口的 output 必须精确为新 root 下
   `resources/diagnosis/native-preflight-diagnosis.json`，create-new；run binding 必须固定
   author thread/turn、packet SHA、新 candidate、新 source SHA、53 sources、37 resources、
   Prefab SHA/GUID 与完整 argv。不得接受通配 evidence 目录或 blocked/b9 candidate。

### 3.2 一次只读原生探针

只允许一个 Unity process、`≤300s`。入口必须：

1. 在 `LoadPrefabContents` 前重新解析磁盘 Prefab，记录 BoardFrame GameObject fileID、
   component fileIDs/class IDs、board script GUID、序列化 class ID 222 数量，并断言与
   §1.2 相等。
2. 调用 `PrefabUtility.LoadPrefabContents` 一次；不得调用 `AddComponent`、
   `SaveAsPrefabAsset`、`AssetDatabase.SaveAssets`、原 recovery 或任何 Prepare 方法。
3. 在访问 `Graphic.canvasRenderer` property 之前分别记录，而不是合并断言：
   - loaded root 是否非 null；
   - name=`BoardFrame` 的 Transform 数量；
   - CandidateBoardElement `MonoScript` asset 是否非 null及其 GUID；
   - 全 Prefab `MonoBehaviour` 数量；
   - 与该 MonoScript 精确匹配的 component 数量；
   - 匹配 component 是否在唯一 BoardFrame；
   - BoardFrame `GetComponents<Component>()` 的逐项 type/name 顺序；
   - BoardFrame `GetComponents<CanvasRenderer>()` 数量；
   - BoardFrame activeSelf/activeInHierarchy；匹配 board component enabled 状态。
4. 这些观测均写入 create-new JSON，即使原复合 guard 某一项为 false。诊断入口不得把
   native renderer count 0 或 1 当成通过保存的条件；本包只观测。
5. `finally` 只 `UnloadPrefabContents`。进程结束后重新冻结磁盘 Prefab、37-resource、
   53-source 和旧 evidence manifests；资源 delta 必须为空，source 必须等于新的诊断
   candidate，旧 roots 必须不变。

这是本轮唯一 red-capable loop。若入口自身编译/身份失败，或未生成逐字段观测，返回
`BLOCKED_DIAGNOSTIC_LOOP`；不得转而运行原 recovery。

## 4. 预注册假设与判定

诊断 JSON 到手前不选择修法。按以下可证伪预测排序：

1. **H1：native load 已补 renderer。** 磁盘 serialized 222=0，但任何
   `Graphic.canvasRenderer` property 访问前 native CanvasRenderer count=1；其余四个 guard
   子条件为 true。
2. **H2：script identity 匹配失败。** native renderer count=0，但 MonoScript asset
   缺失、GUID 不符或 matching board count≠1。
3. **H3：BoardFrame topology 与原假设不同。** frame count≠1，或唯一 matching board
   不在唯一 frame；native renderer count 不足以解释 FM029。
4. **H4：加载/激活产生其他异常状态。** 前三项不成立，但 component/active/type 顺序
   证明另一个明确子条件失败。

只有 receipt 能唯一判定 H1-H4 后，主程才可签另一个最小修复包；C 本回合不得自行进入
修复。若观测仍不能区分，返回缺失字段，继续缩小探针而非猜测。

## 5. 预算、验收与回传

- 产品/测试/asset 写入：0；唯一 repo source delta：1 个 Editor 文件。
- Unity launches：最多 1；不运行 Q2/Q3/targeted/Q4/Q5/Android/APK/device。
- 不新增依赖、公共 runtime API、`.meta`、Package/ProjectSettings 或 Git 变更。
- 诊断进程需有 absolute argv、PID、UTC、duration、exit、stdout/stderr hash、timeout、
  process-tree quiescence；shell 自述不能代替真实 receipt。

回传：新 candidate；单文件 before/after bytes/SHA 与最小 diff；Q1 receipt；诊断
run/log/JSON bytes/SHA；H1-H4 逐字段判定；运行前后 53-source/37-resource/旧根清单；
全部未运行项。状态只能是 `DIAGNOSED_<H1|H2|H3|H4>` 或
`BLOCKED_DIAGNOSTIC_LOOP`。完成后停止，等待主程签发后续包；不得自行请求 QA/R。
