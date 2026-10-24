# LAYOUT-S1-FIX04：真实Scene的Canvas实例覆盖修复

2026-10-02 · `EXECUTABLE_SOURCE_ONLY`。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，签发turn `01a0fccb-bfa4-7fc1-a81b-f8a8a071cdbf`；唯一作者C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`，显式 `gpt-6-astra/xhigh`。授权为用户first Demo继续任务、AGENTS§6 standing workflow，以及中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b` 本轮FIX04与Editor入口技术范围确认（turn `01a0fc83-b951-7cb0-8af3-31269097058f`）。C只读握手turn `01a0fccf-603f-7432-9f99-691fb95db374` 于13:31:24.426213 UTC确认前轮PExport停写、两前像匹配、FIX04缺席；实施时记录实际turn，不冒用握手turn。主程唯一收C；中央负责发布，QA接独立GitHub结果。

## 固定输入与白名单

**VERDICT: NEEDS_FIX**，依据[GitHub review5392275691](https://github.com/yyczz1/FightMatch/pull/4#pullrequestreview-5392275691)／[P1 r4166096874](https://github.com/yyczz1/FightMatch/pull/4#discussion_r4166096874)。受审head `e3d581df42f3a25cfdfc319bca5509f8b5382ada`，tree `52c306b98275111955280da919b45e2806e15ebb`；[QA原回执](testing-layout-s1-pr4-native-review-receipt.json) 11930B／SHA `bd43d0afa20f541e95d08d85532323e81da14797271bf00e65aa2a17f005450f`。仅修RuntimeCanvas fileID `1013562322995895578` 的 `m_LocalScale.x/y/z`、`m_AnchorMax.x/y` 五个旧零覆盖及验证遗漏，不做重复local review。

| 唯一可改产品路径 | 前像bytes／SHA256 | 新增＋删除行上限 |
| --- | --- | ---: |
| `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` | 154822／`01b73f95a355ee5eaa3839360ab56094b158d16254e49cb0eb3a470834c75482` | 200 |
| `Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs` | 24402／`83e556cc3348faa0210985af3d910f9e32f450b08e05f8eb72230252bb543f72` | 120 |

最多2文件／320行，各预算不可互借。其余13源以 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX03/source-receipt.json#sourceS15After` 为准（14056B／SHA `44b8fd8572548ccee8a4c4591275b242f4459e06a04837f58eff70bcbafa042b`）。实际native四件以[publication manifest](../../versioning/2026-10-02-layout-s1-pr/native-p/publication-manifest.json) SHA `1be36cbf7b78985aa565dfff88256a3b195447f492c1c68db6fca26ee96a3a23` 固定：Prefab SHA `0064bbee5e0d84dfdfd627310a9abada5093252bb26cf363d0646b38586f19d2`；Scene SHA `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd`；两自然meta亦冻结。共享资产/meta/程序集/其它代码、Packages/ProjectSettings、Config/Tools/Generated/ArtSource、LOC WIP、投影及旧证据都不改。无运行时public API或序列化字段变更。

## 最小实现与验收

1. 先新增一个 `[Test]`：`FightMatch.Core.Tests.UguiSceneCompositionTests.SavedDemoScenePreservesUsableCanvasWithoutStaleOverrides`，再改生产源码。必须以EditMode只读打开真实 `Assets/Scenes/FightMatchDemo.unity`，取得其现有Host.RuntimeRoot/RuntimeCanvas及Prefab来源；不是UguiHostRig或Instantiate(prefab)。验证唯一Canvas/Host、正确Prefab连接、局部几何与来源一致、非零有限正lossyScale，且上述五项没有与Prefab不一致的残留覆盖。断言应在当前冻结Scene上暴露旧零值，不得在断言前修复、激活Host/进入Play、调用生产修复或保存资产。
2. 新测试保留原有scene：优先Additive打开仅本测试owned Scene，若目标已开则显式失败且不关闭它；finally只关本次打开对象并恢复原active Scene，不用Single/NewScene清空其它Scene，不丢弃用户dirty内容。记录并断言目标Scene/Prefab字节不变；不得为了测试改asmdef引用Editor builder。原6测试名、全部旧方法体/断言、rig与PlayMode清理逐字节保留，只允许必要using及新方法/private helper。
3. builder新增最小private原位修复：严格定位既有Scene实例的RuntimeCanvas及对应Prefab RectTransform，只归一化/移除上述五属性的陈旧覆盖，保留其它PropertyModification及所有层级、对象fileID/GUID、ViewId、Host/EventSystem/Prefab连接。禁止重建对象、全量RevertPrefabInstance、清空全部覆盖或文本YAML编辑；规范目标来自已迁移Prefab（scale=1、anchorMin=0、anchorMax=1）。实际失真/零几何必须被验证拒绝，不能仅数绑定/引用。
4. 修复接入原 `MigrateLayoutResources` 的Scene分支，在保存前处理五属性。原Prefab迁移依赖旧层级，不为本次已迁移候选重跑或扩展成通用幂等迁移。获明确许可新增仅Editor用 `public static void RepairLayoutSceneOverrides()`：只验证现有已迁移Prefab、原位修复现有Scene、原生Save/close/read-only reopen；Prefab字节不可改，原生保存后再次检查五属性与实际Canvas几何，不能以保存前内存值代替复开值。保存后仍有陈旧覆盖即失败，不循环尝试。
5. 新入口复用既有guard/protect/create-once证据结构，仅其mode精确选 `FIX04/I/activation.json`、`FIX04/P`，新mode sourceReceipt精确绑定 `FIX04/source-receipt.json`；旧mode仍原 `I/activation.json`、`P`、`S/source-receipt.json`。路径均相对现 `LayoutStageRoot`，candidate仍原projection；保留完整源15／资源37／meta2／保护输入／owner/issuer/argv/GUID/clean-startup约束，禁止宽泛路径或旧租约重放。复用现native三叶结果模式标明Scene修复；本轮不创建I/activation或P目录，也不改旧JSON格式。`VerifyLayoutResources`／`LayoutReopen`只读，检查真实Scene实例几何和五项覆盖，不得自愈以掩盖坏输入。

## 静态证据与后继验证

- 主程已对精确Scene SHA作只读失败复现：命令chunk `4001f4` exit1，五属性均0，错误 `RED: actual Demo Scene retains zero Canvas scale/stretch overrides`。这是序列化红点，不是Unity测试。源码包结束时Scene必须仍是该原像，实际回归为 `NOT_RUN`；不得伪称red-green或此finding已闭合。
- [QA覆盖计划](testing-ugui-layout-coverage-delta-001.json) 51650B／SHA `f9e3138f29b5ef5d0e8ce2271719718ba1bc165e6dd32737b044962cc9d0190b` 的原47+49=96精确叶与10个NOT_RUN保留。仅追加上面全名为97候选，不重做59项分类、不替换原断言。实际97须后继发现绑定；新head审查、另签原生Scene修复通过后，再单独签discovery最多1次≤60s及合并定向run最多1次≤600s，自动重试0。本包不激活任一执行。
- 唯一新证据根 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/`，create-once仅 `before.json`、`source.patch`、`source-receipt.json`。before记录实际C thread/host/turn、授权/本合同hash、两前像完整UTF8原字节与身份；枚举当前共享Assets/Packages/ProjectSettings/Config/Tools/Generated/ArtSource存在的文件（含隐藏/未跟踪叶、不得随符号链接）建立保护清单。原1079旧基准不得把已接受native变化误判或覆盖为旧值。另冻结source15、native4、上述QA/发布manifest/coverage，以及S/FIX01/FIX02/FIX03/I/I02/P/PExecution/PExport全部证据叶；projection保持只读且至少源15/native4/隔离SceneTemplateSettings身份不变。中央/主程/SYS各自索引文档并发不在C写范围，不作污染。
- postflight只允许两源码delta；无新增/删除产品叶，逐文件行预算、原6测试名及旧测试文件除using/新增方法外全部原字节保持；patch内存重放精确等于两后像，source15列全量bytes/SHA/gitBlob。回执包含before/patch/保护清单身份、具体命令UTC/退出码、实际改动与验收项映射、精确新测试名和97仅拟、未来入口/路径以及未验证项。工具输出是证据，不新建无白名单脚本。发生额外路径/预算/固定输入漂移或设计歧义，停止受影响项回主程，不自扩、不回滚别人工作。
- 本包Unity/compile/import/discovery/tests/dotnet/native/设备/CUA/下载/云写/Git写/review触发均0。交回只能 `SOURCE_READY / UNCOMPILED / UNTESTED / SCENE_REPAIR_NOT_RUN / NOT_PLAYABLE_CANDIDATE`；停写后主程机械核范围/证据，直接交中央固定新源码发布PR4，不自行签ACCEPT。旧I/P限定接收及失败记录保留，但它们未检查本次Canvas几何，不能证明真实Scene可用。
- diagnosing-bugs用于明确红点和因果，实际测试因当前禁跑延期；writing-for-agents用于短白名单/停止线；uGUI要求原位属性修复而非重建层级。Unity [GetPropertyModifications](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PrefabUtility.GetPropertyModifications.html)／[SetPropertyModifications](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PrefabUtility.SetPropertyModifications.html) 为2022.3官方API参考；保持未命中修改项是本包约束，不把API调用等同于保存成功。
