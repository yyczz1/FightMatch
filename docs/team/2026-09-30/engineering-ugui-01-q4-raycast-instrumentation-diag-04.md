# UGUI-01-Q4-DIAG-04：真实 Raycast / InputModule 失败瞬间探针

2026-10-01 · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 目的与授权边界

FIX-03 已用单变量 A/B 反证 H1：移除 `-nographics` 后 Unity 使用 Apple M4 Metal，exact
3 仍 0/3，failure message/stack 与 Null-device 运行一致。DIAG-04 只在三个既有失败点
加入临时、无副作用的测试诊断，采集 board 是否进入 Canvas registry、Graphic 的
depth/cull/rect/filter 状态、完整 raycast 排序，以及 EventSystem/InputModule 状态。

- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央 AI
  `01a0e401-511d-79f2-b47f-3ab0ade1681b / local` 已授权 scoped common-root
  diagnosis/fix，保留真实 Input、业务断言与 222 名称。
- 唯一执行 owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52 / local`。
- 签发／接收：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b / local`。
- 固定 product candidate：
  `0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91`。

本包不是产品修复包。不得修改 production、Prefab、Scene、font、meta、asmdef、设置或
测试断言；不得添加 wait/yield、直接派发 handler、替换 GraphicRaycaster、手工 capture 或
业务 commit。执行后四个测试文件必须恢复到精确 preimage；诊断 evidence 保留。

## 1. 已机械核对的固定输入

以下 8 项在签包前从实际路径机械读取 byte length 与 SHA-256：

| path | bytes | SHA-256 |
| --- | ---: | --- |
| `docs/team/2026-09-30/engineering-ugui-01-q4-raycast-graphics-fix-03.md` | 11,721 | `d5f1fda9bc8781cf271d723fa3cbf784cb426bcff59afadc69b74da6ac3f2787` |
| `TestArtifacts/FightMatch/UGUI-01/0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/q4-correction-03/final-receipt.json` | 19,957 | `7d0a45b84c59bf3937d1442fc0a4d00a07b37eb4f6e56ac0dc605fccd3d95edb` |
| `TestArtifacts/FightMatch/UGUI-01/0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/q4-correction-03/exact-3-graphics/run.json` | 41,361 | `aa2be8775b4e7305bc67a1e8f2dbf5bca71f06807c40d1178a2768182fd67b2b` |
| `TestArtifacts/FightMatch/UGUI-01/0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/q4-correction-03/exact-3-graphics/tests.xml` | 11,550 | `cf832b77b089df0d3fa22a36d3e3185d3f59957eb49fe8f563ac6f93d3aedc36` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs` | 27,769 | `34c37acf134231045ee0752d14dc647a8567ba9b76e804b2eed546410fa6981c` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs` | 42,233 | `d469f443786b22f7212c48685922faa33e7764b9277eab06ba2cf2cee7904b9d` |
| `Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs` | 16,206 | `9c85a4fa10c938fa234b8ed5b55ca8753e97693adef1e049ba7129db318f7ac0` |
| `Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs` | 21,477 | `81f3009728dc0522253a1f56e642225829e767c54e2031e2a8ee90e7cb0d79a8` |

FIX-03 事实：PID 86739；exit 2；Editor 29.406379292006022s；launch→quiescence
33.52676941700338s；exact 3 为 0 pass / 3 fail / 0 skip / 0 inconclusive；graphics evidence
为 `Using device Apple M4 (high power)`、`Initializing Metal device caps: Apple M4`；53
source、37 resources 与四棵旧 evidence roots 均不变；targeted 9 / exact 222 未运行。

新 evidence root 当前实测不存在：

```text
TestArtifacts/FightMatch/UGUI-01/
  0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/
    q4-diagnosis-04/
```

## 2. 仍存的可证伪假设

按判断顺序固定：

1. **H2A — registry/depth lifecycle**：board 未注册到自身 Canvas 的 GraphicRegistry，或
   `depth == -1` / `canvasRenderer.cull == true`，所以 GraphicRaycaster 在矩形检查前过滤。
2. **H2B — screen/rect/filter**：board 已注册且 depth 有效，但 screen point 不在矩形内，
   或 `Graphic.Raycast` / CanvasGroup / Mask / RectMask2D filter 拒绝。
3. **H3 — foremost overlap**：board 是有效 hit，但同一 point 有更高 depth 的 active
   raycastTarget；H01 与其它两项可能因此分裂。
4. **H4 — module/event-system lifecycle**：显式 Raycast 能命中 board，但
   `FightMatchStandaloneInputModule.Process()` 未处于 active/current 状态、输入样本不正确，
   或事件未派发，因此 capture 未建立。

本包不预选结论。若三个测试呈现不同原因，回传 `DIAGNOSED_SPLIT_ROOTS`，不强行归并。

## 3. 临时写白名单与强制恢复

只允许临时修改以下 4 个既有 test `.cs`：

```text
Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs
Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs
Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs
```

只允许新增诊断 helper、只读状态采样和 `TestContext.Out.WriteLine`；不得改变任何 Assert、
setup/teardown、yield/frame、对象创建/启停、input sequence、pointer id/position、route、
Canvas.ForceUpdateCanvases 次数或执行顺序。不得修改 test name/attribute/filter。

运行结束（成功、失败或 compile error）后，删除本包新增的全部诊断代码，使四文件分别
恢复为 §1 的 bytes/SHA。使用精确反向 patch；禁止 `git reset/checkout/stash`。恢复后先
机械核对四个 preimage，再核 53-source manifest、37-resource manifest 与旧 evidence。

永久写入只允许 create-new `q4-diagnosis-04/`；可含 packet、preflight、temporary diff、
probe run、diagnostic extraction、restore proof、process receipts 与 final receipt。旧
q4-correction-02/03、b9/f21/8a3 全部只读。

## 4. 探针字段与插入点

每个探针输出单行前缀 `UGUI_RAYCAST_DIAG|`，后接稳定 key/value 或 JSON。字符串不得包含
玩家资料或任意文件内容。每个插入点只采一次，完整诊断输出总 cap `2 MiB`。

### 4.1 通用 board/raycast snapshot

每个 snapshot 至少记录：

- `test`, `phase`, pointer kind/id, point x/y；`Screen.width/height`、display relative point；
- EventSystem：instance/current equality、active/enabled、currentInputModule type/equality；
- input module：gameObject active、enabled、`IsActive()`、`inputOverride` type；
- board：activeSelf/activeInHierarchy、enabled/isActiveAndEnabled、runInEditMode、raycastTarget、depth、
  CanvasRenderer cull/absoluteDepth/materialCount/mesh vertexCount；
- RectTransform：rect、position、lossyScale、四个 world corners、
  `RectangleContainsScreenPoint`；
- canvas/rootCanvas：identity、active/enabled、renderMode、pixelRect、targetDisplay、
  sorting layer/order、renderOrder、worldCamera/eventCamera；
- GraphicRaycaster：active/enabled、runInEditMode、eventCamera、blockingObjects、blockingMask；
- GraphicRegistry：该 canvas 的 graphic count、是否按 reference 含 board；
- `board.Raycast(point,eventCamera)`；父链所有 enabled CanvasGroup 的 alpha/interactable/
  blocksRaycasts/ignoreParentGroups，以及 Mask/RectMask2D presence；
- `EventSystem.RaycastAll` 的完整有序 hit list：index、transform path、module type、depth、
  sortingLayer、sortingOrder、distance、displayIndex；空 list 明确写 count=0。

若 API 在 Unity 2022.3/uGUI 1.0.0 不可访问，只能省略该字段并在 packet receipt 记
`API_UNAVAILABLE:<field>`；不得用 reflection、修改 package 或 product API 获取。

### 4.2 三个固定插入点

1. `CandidateBoardInputTests.B16B03_RealPanelDownCapturesOnlyAndLastUpCellSubmitsOnce`：在原
   `r.Down(start)` 之前 snapshot `B16-before-down`；原 Down 之后、原 capture Assert 之前
   snapshot `B16-after-down`，并记录 controller active pointer/stage 与 board capture。
2. `UguiHostRig.BeginRoute`：原 `Driver.Raycast(start)` 后、两个原 Assert 前 snapshot
   `OwnerEnded-before-assert`，必须复用该原始 hit list，不能第二次 Raycast 改变观察。
3. `HostPanel.Draw`：只在 `i == 0`，原 RaycastAll 后、foremost Assert 前 snapshot
   `H01-before-assert`，必须复用该原始 hit list。

Core.Tests 可把通用格式 helper 放在 `CandidateBoardInputTestData.cs`；Host.Tests 因不引用
Core.Tests assembly，在 `FightMatchHostTestFixture.cs` 内保留最小等价 helper。不得改 asmdef。

## 5. 执行与进程合同

1. Preflight：核 packet/input identities、新 root absent、当前无其它 Unity process；冻结
   53-source、37-resource、四棵旧 root 与 Git read-only状态。
2. create-new evidence root；保存 packet 和四个 preimage；应用临时 instrumentation；保存
   exact diff 与 temporary hashes；静态确认所有原 Assert/name/attribute/yield/input calls
   未改。
3. 一次 Unity graphics exact-3：`-batchmode`、不传 `-nographics`，同一 3 fullname、
   create-new XML/log。必须使用非 Null renderer。
4. 不论 run 结果，先自然等待所有 owned/Unity process 退出，再按 §3 精确恢复四文件；
   恢复完成前不得启动第二次 Unity。
5. 若仅因 instrumentation compile error 且未产生 test XML，可在恢复后回传
   `BLOCKED_INSTRUMENTATION_COMPILE`。本包不授权现场修后第二跑，避免探针漂移。
6. 提取全部 `UGUI_RAYCAST_DIAG|` 行及三项原 failure；按 §6 判别并封存 final receipt。

唯一 Unity process launch 上限为 1；launch→最终静止总预算 `≤240s`。一次只允许一个顶层
process tree；不发送 signal。测试日志/标准输出各 cap `8 MiB`，evidence root `≤32 MiB`，
temp `≤64 MiB`，free-space floor `2 GiB`。超限保存事实并停止。

## 6. 诊断判别

逐 test 独立使用首个不满足条件：

- board 不在 registry，或 depth=-1/cull=true → `DIAGNOSED_REGISTRY_OR_DEPTH_LIFECYCLE`；
- registry/depth/cull 正常，但 rectangle false → `DIAGNOSED_SCREEN_RECT_MAPPING`；
- rectangle true，但 board.Raycast false / parent filter false → `DIAGNOSED_RAYCAST_FILTER`；
- board 在 ordered hits 且 index>0 → `DIAGNOSED_FOREMOST_OVERLAP`，列 exact blocker path；
- board 是 hit index0，而 module inactive/current mismatch → `DIAGNOSED_MODULE_LIFECYCLE`；
- board 是 hit index0、module active/current，Down 后仍无 capture →
  `DIAGNOSED_EVENT_DISPATCH_STATE`；
- 数据不足或不同 tests 为不同首因 → `DIAGNOSIS_INCOMPLETE` 或
  `DIAGNOSED_SPLIT_ROOTS`，保留原始字段，不猜修复。

本包不因获得诊断而修改产品/测试 fixture；主程根据 receipt 另签最小 FIX-04。

## 7. 验收与回传

- [ ] exact 3 名称、原 Assert、pointer/input/route/frame 语义完全不变；仅新增只读探针。
- [ ] 三个固定插入点均输出完整 snapshot 或逐字段 API_UNAVAILABLE。
- [ ] run 使用 Apple Metal/其它非 Null renderer；真实 failure/pass 原样保存。
- [ ] 四个 test source 最终精确恢复 §1 preimage；53 source、37 resources、旧 evidence
  均不变；无 production/asset/meta/setting/Git 写。
- [ ] targeted 9、exact 222、Q1、recovery、Prepare/SaveAssets、4181、Q5、Android、APK、
  device 均 `NOT_RUN`。
- [ ] process exit/quiescence、argv、XML/log、temporary diff、restore proof、diagnostic
  extraction 与 final classification 全部 create-new 封存。

Owner 回传：packet/preflight/run/XML/log/process/final receipt identities；temporary diff 与
四文件 temp/final hashes；每个 `UGUI_RAYCAST_DIAG` snapshot；逐 test 判别和公共/分裂根因；
source/resource/old-root equality；NOT_RUN 清单。完成后停止，不自行修复、送 R 或写 Git。
