# LAYOUT FIX04 / N03 — 区分Prefab约束与活动Overlay几何

2026-10-04 · SOURCE_ACTIVATED。依据同一人类授权 `central-resume-scene-loc-2026-10-04.json`；中央在D02实际证据与SYS actual `01a10669-9c69-7861-98ed-59e90528f266` 后明确批准技术限定：覆盖“所有原断言逐字不变”中仅把活动Overlay实例等同Prefab源的几何断言；Prefab严格约束、其它覆盖/身份/保存关闭只读复开门保留。不是UI/资源/玩法设计变化，禁止禁用Canvas或只靠内存过门。

issuer/唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a10641-a816-71d1-919b-97413f71ea56`；唯一源码作者既有实现 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，Astra/xhigh，记录实际turn。交接：作者只写下面新S和共享两源码，不写隔离投影；原C的D02已completed/停写/恢复B。后继Unity仅C、另签准确I；主程接源码后直接安排，不再等待用户或中央重复许可。

## 固定前像与精确白名单

- B=`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`，160434B / `5dda3f055c7f78016d91dc533382b53154d8dd1412abcf03913c64f0aeaaaac0`。
- T=`Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs`，28866B / `8a043c1b55bf870f8b8768bf15bdcd3525a3776caeee8bdd5ba5cece1f17548c`。不是UguiResponsiveLayoutTests；该文件与其余13源保持。
- N02/S/source-receipt.json 25964B / `30623e645013634aaf6ad472f42ea5b11ffa23ce4288eeefdb23f04e328abce6` 的sourceS15After/native4为固定前像。D02/PExecution/receipt.json 45711B / `850788122bad4566bce43f8a8939e5d0601be3e08fab07a61c0fc96f180891b7`，12叶1389992B；log358/369/380分别为实例scale .769800365→1→1、max0→1→0、五覆盖5→5→0，source稳定，实例由本Canvas驱动。主程905288已机械核85封存/12叶、原始三点、闭包与B恢复；不重跑诊断。
- N=`TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N03`，主程e186c3已核ABSENT。只新建S八叶：`before.json`、`FightMatchAndroidBuild.before.cs`、`UguiSceneCompositionTests.before.cs`、`FightMatchAndroidBuild.cs`、`UguiSceneCompositionTests.cs`、`source.patch`、`static.json`、`source-receipt.json`。S≤2MiB；两前像/候选可机械复制，实际差量用apply_patch。只共享B/T可改，其它共享文件与N02/D01/D02/旧FIX04全部只读；不写任何Scene/Prefab/meta/配置/依赖或投影。

## 最小实现与验收

1. B保留 `LayoutVerifyCanvasGeometry` 对Prefab源的原严格scale1/min0/max1及有限正world断言。真实Scene的 `LayoutVerifySceneCanvas` 使用活动Overlay检查：精确RuntimeCanvas/root关系、enabled/activeInHierarchy、isRootCanvas/rootCanvas、自身驱动归属；local/world scale、rect/pixelRect及scaleFactor全部有限且正；经实际四角映射核屏幕覆盖与Canvas.pixelRect一致，固定数值容差≤0.5像素，拒绝零尺寸/NaN/Infinity/缺覆盖。不得仅断言有Canvas或把错误吞掉；不硬编码D02的640×480/.7698作为期望。必须保留exact prefab/source/fileID与五陈旧覆盖拒绝。
2. `LayoutRepairSceneCanvas` 仅过滤原五陈旧覆盖后SetPropertyModifications，移除给驱动实例localScale/anchorMax的手动赋值；其它target/path/value/顺序原样保留，不扩删pivot或其它override。不新增ForceUpdate、组件启禁、额外Revert或保存调用。同步 `LayoutVerifyRoot` 中的重复几何检查，使Prefab源走原严格门、真实Scene走新驱动门；显式调用上下文可用私有参数，不能把未知对象作为通过分支。保存后、关闭后只读复开仍走同一真实Scene门，Prefab检查不被绕过。
3. 仅sceneRepair的executionRoot由FIX04/N02改FIX04/N03；当前C/主程身份、/S/source-receipt.json与其它mode/全部租约护栏不变。不得带入D02诊断代码或PRE_SAVE_STOP；native流程仍原位SaveScene恰一次→关闭→只读复开，全部141/3/6SceneID/字节/其它override/资源/GUID约束保留。
4. T只修FIX04新增的 `SavedDemoScenePreservesUsableCanvasWithoutStaleOverrides` 中实例=Prefab的错误断言，替换为独立明确的驱动/可用几何/覆盖期望，保留Prefab原断言、精确GUID/fileID、五覆盖逐值检查、finally关闭/active恢复与Scene/Prefab字节不变。其余旧六测试及rig/输入/存档等字节保持。
5. 新增且仅新增两个普通 `[Test]`：`SceneCanvasGeometryRejectsZeroNonFiniteAndIncompleteScreenCoverage` 与 `SceneCanvasOverrideClassifierRejectsAllFiveStaleFields`。前者针对实际生产检查使用的私有数值seam（可反射，不新增public API）覆盖合法数据及每组零/负/NaN/±Infinity/不完整像素覆盖反例；后者对固定Prefab源逐一证明原五属性的0/非有限/不可解析覆盖被拒绝、匹配源的有效值可保留且非目标覆盖不被误删。不得给真实Transform设置NaN或通过修改真实Scene制造反例，不保存资产。实例正例继续由真实只读Scene测试验证，不用synthetic代替native。
6. B增删合计≤100行、T≤160行，总≤260；若必要实现超出，报告精确差量原因，不删除必要断言凑行。原97集合加上述两名=99候选；新测试当前不运行，回执准确记录后置发现/测试必须99，不把历史97当新源绿测。

## 源码检查与返回

仅进程内静态检查，最多3次累计≤30秒，技术性字符串/身份/工具类型错误在窗口内合并修正并留各次原始结果，禁止原样重试或改变检查意图。用最终候选统一生成bytes/SHA/gitBlob/15源后像；核精确两文件diff、scope/行预算/原六测试与其它代码保留、两个新名唯一、保存/复开路径和Prefab严格断言保持，记录预期source-only未编译未测试。不得启动Unity/编译器/SDK/网络/Git，不新增本地review；不重跑旧113/23/46或大历史扫描。原证据引用固定清单，不复制大段历史正文。

source-receipt提供与N02兼容的sourceS15After/native4、真实owner/issuer、授权、八叶身份/精确patch/每项验收和真实静态耗时。共享两后像与S候选一致；其它13源/四native/旧封存保持。完成一次回主程并停写，不能自签ACCEPT。主程固定后像后交C一次限定原生保存复开/成功才导出；两源码+实际Scene再交中央PR4同head审查，随后限定99与设备验收独立进行。
