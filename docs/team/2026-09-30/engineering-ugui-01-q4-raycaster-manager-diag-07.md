# UGUI-01-Q4-DIAG-07：RaycasterManager 与 direct raycast 分界探针

2026-10-01 · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 目的、裁定与授权

DIAG-05 已取得四个有效 snapshot，但结论仍为 `DIAGNOSIS_INCOMPLETE`：

- B16 before/after：board depth `10`、registry/rect/filter 全通过，hits `0`；
- OwnerEnded：board depth/absoluteDepth `-1`，hits `0`，已判
  `DIAGNOSED_REGISTRY_OR_DEPTH_LIFECYCLE`；
- H01：board depth `3`、registry/rect/filter 全通过，hits `0`；
- 三个 fixture 的 `GraphicRaycaster.runInEditMode` 都是 false；EventSystem current/module
  关系也未建立，但现有数据没有证明它就是空 hits 的首因。

DIAG-07 只补两个缺失分界：目标 GraphicRaycaster 是否存在于全局
`RaycasterManager`，以及绕开 manager 直接调用该 GraphicRaycaster 是否能命中 board。
它不修改产品、Prefab、Scene、资源、断言、输入、frame/yield 或业务行为。

- owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52 / local`。
- 签发／接收：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b / local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央 AI
  `01a0e401-511d-79f2-b47f-3ab0ade1681b / local` 已授权 scoped diagnosis/fix 与准确
  compiler-server 正常退出处理。
- 固定 product candidate：
  `0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91`。

## 1. 固定输入

表内以 `q4-` 开头的相对路径均以
`TestArtifacts/FightMatch/UGUI-01/0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/`
为唯一 base：

| path | bytes | SHA-256 |
| --- | ---: | --- |
| `docs/team/2026-09-30/engineering-ugui-01-q4-raycast-instrumentation-diag-05.md` | 8,494 | `d210851a7fb347e0dbedc240f1f046ab29c6192c636b7a889b1cbc5bcfec9a5f` |
| `q4-diagnosis-05/final-receipt.json` | 62,003 | `c4ad7cab81d6c95702bacb7288fac07648fba51450528b8616341ce93e34374e` |
| `q4-diagnosis-05/temporary.patch` | 25,834 | `00be54f77cb4f8e8b31cc8bfce769d2c972038aff513ac764152c868f6432684` |
| `q4-diagnosis-05/probe-run/tests.xml` | 31,150 | `d2fb1afbcf3b279491e8c4342567c8531d22cd38b9561d86a941c04f9d16c5f5` |
| `q4-diagnosis-05/diagnostic-extraction.json` | 55,017 | `980664a12e7e2271a4c8915a59c2ca3470c42988ab59554b47a2aa3d048bd724` |
| `q4-diagnosis-05/restore-proof-followup.json` | 1,930 | `78e42f5f8183d602343e59519dfc674fc2a980872a6f256737dbe1eab5815d16` |
| `q4-diagnosis-05/process-cleanup-06/final-receipt.json` | 3,382 | `03f20314575314da38d9d87c99c0c797fc8ad1d3deb3951e94125617433f832b` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs` | 27,769 | `34c37acf134231045ee0752d14dc647a8567ba9b76e804b2eed546410fa6981c` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs` | 42,233 | `d469f443786b22f7212c48685922faa33e7764b9277eab06ba2cf2cee7904b9d` |
| `Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs` | 16,206 | `9c85a4fa10c938fa234b8ed5b55ca8753e97693adef1e049ba7129db318f7ac0` |
| `Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs` | 21,477 | `81f3009728dc0522253a1f56e642225829e767c54e2031e2a8ee90e7cb0d79a8` |

DIAG-05 的 360 秒退出超时保持原样；VBCSCompiler 后来自然退出，PROCESS-06 为
`ALREADY_QUIESCENT`，没有运行 shutdown、没有 signal；四文件、53 source、37 resources
已恢复一致。

## 2. 临时写白名单与唯一增量

只允许临时修改与 DIAG-05 相同的四个 test source。先从 §1 preimage 重新应用 DIAG-05
temporary patch；两处 registry membership 必须仍为 `registry.Contains(board)`，不得恢复
`Any` 或枚举 `IndexedSet`。

相对 DIAG-05 只允许在两个临时 `UguiRaycastDiag04.Capture` helper 内增加：

1. `RaycasterManager.GetRaycasters()` 的 count；使用索引循环判断 exact raycaster reference
   是否存在，并列出每项 index、identity/type/active/enabled/runInEditMode；该返回值是普通
   `List<BaseRaycaster>`，仍禁止 LINQ/foreach；
2. 用同一 `EventSystem`、point、pointerId 创建新的 `PointerEventData`，仅调用一次目标
   `GraphicRaycaster.Raycast(data, directHits)`；记录完整有序 direct hit list，字段与既有
   EventSystem hit list一致，并记录 board index。该调用不得派发事件、调用 module.Process、
   改 input 或复用 direct hits 作为原测试结果；
3. 单行仍以 `UGUI_RAYCAST_DIAG|` 输出；新增 key 前缀固定为
   `raycasterManager.*` 与 `directHit.*`。

不得改变四个 snapshot 插入点、原 EventSystem hit list、原 Assert、test name/attribute、
input/route、Canvas.ForceUpdateCanvases、yield/frame、对象生命周期。静态门必须证明本次
temporary diff 相对 DIAG-05 只有上述两个 helper 的等价增量，且 production/resource/
meta/asmdef/setting 写为 0。

## 3. evidence root 与执行

create-new root 必须不存在：

```text
TestArtifacts/FightMatch/UGUI-01/
  0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/
    q4-diagnosis-07/
```

旧 q4-diagnosis-04/05、PROCESS-06、q4-correction-02/03、b9/f21/8a3 全部只读。

1. 核 packet/input/root/process/free-space；冻结 53-source、37-resource、旧 roots。
2. 保存四 preimage、应用临时探针、保存 diff/hashes/static proof。
3. 一次 graphics exact-3，`-batchmode` 且不传 `-nographics`，filter 仍为 B16B03、
   OwnerEnded、H01 三个固定 fullname 的单个分号连接参数；证明非 Null renderer。
4. Unity 主进程退出后保存 XML/log/argv/exit 与当前 process tree。先自然等待
   UnityAutoQuitter/ShaderCompiler/bee 等非 compiler-server children。
5. 若唯一剩余 current-launch descendant 是 PPID 1 的 VBCSCompiler，必须机械核它使用
   本 Unity 2022.3.18f1 bundle 的 exact NetCoreRuntime/dotnet 与 VBCSCompiler.dll，并解析
   唯一 `-pipename:`。只有 Unity/AutoQuitter/ShaderCompiler/bee 全不存在且身份全匹配，
   才允许以同 executable/DLL/pipe 调用一次 `-shutdown` 正常协议；保存 raw argv/stdout/
   stderr/exit。禁止 `kill`/signal、全局 `dotnet build-server shutdown`、多实例或 retry。
   若 server 已自然退出则记录 NOT_RUN。shutdown client/原 server 最多自然等 30s。
6. 所有相关进程静止后立即精确恢复四文件，重算 preimage、53/37/旧 roots；不得为进程
   处理启动第二次 Unity。
7. 提取四 snapshot，按 §4 分类；封存 final receipt 后停止。

唯一 Unity launch 1；可选 pipe-specific graceful client最多 1；不得设置/覆盖 `HOME`、
`home`、`CODEX_HOME`。Unity/test timeout `<=240s`，graceful shutdown `<=30s`；每次等待
`<=30s`。log/XML 各 `<=8 MiB`，diagnostic `<=2 MiB`，evidence `<=32 MiB`，temp
`<=64 MiB`，free-space floor `2 GiB`。

## 4. 决策规则

逐 snapshot 用第一条匹配：

1. board depth/absoluteDepth `-1`：
   `DIAGNOSED_GRAPHIC_DEPTH_LIFECYCLE`；manager 缺失若同时发生记 secondary；
2. depth/rect/filter 有效，direct hits 含 board，而 RaycasterManager 不含 exact raycaster：
   `DIAGNOSED_RAYCASTER_MANAGER_REGISTRATION`；
3. manager 含 exact raycaster、direct hits 含 board，但 EventSystem hits 不含 board：
   `DIAGNOSED_EVENTSYSTEM_RAYCAST_DISPATCH`；
4. depth/rect/filter 有效但 direct hits 不含 board：
   `DIAGNOSIS_INCOMPLETE_DIRECT_RAYCAST_FILTER`，保留完整 direct list；
5. EventSystem/direct 都含 board，再按 DIAG-05 的 foremost/module/event-dispatch规则判别。

三个 test 不同首因返回 `DIAGNOSED_SPLIT_ROOTS`；不得强行归并。得到结果后不修改 fixture
或产品；主程另签最小 FIX 包。

## 5. 验收与回传

- [ ] 4 个原 snapshot + manager/direct 增量完整，无 probe exception。
- [ ] 原测试语义与 DIAG-05 探针字段不变；只有 helper 增量。
- [ ] exact3 graphics 真实结果、manager/direct hit list、逐 test 分类原样保存。
- [ ] 四 source 恢复 §1；53 source、37 resources、旧 evidence 相等。
- [ ] compiler server 只自然退出或 exact pipe graceful shutdown；无 signal/全局操作。
- [ ] targeted9、exact222、Q1、recovery、Prepare/SaveAssets、4181、Q5、Android、APK、
  device、Git 均 `NOT_RUN`。

Owner 回传 packet/static/run/XML/log/diagnostic/process/shutdown/restore/final receipt identities；
四 snapshot 摘要和完整 manager/direct lists；classification；source/resource/old-root equality。
完成后停止，不自行修 fixture、产品或送 R。
