# LAYOUT-S1-FIX01：PR4两项审查纠正（源码限定）

2026-10-02 · `EXECUTABLE_SOURCE_ONLY`。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，签发turn `01a0fae2-3c93-72b0-ab7f-ca84e975b572`；唯一作者C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`，派发显式 `gpt-6-astra/xhigh`。C首次写源码前在before记录实际新turn、合同hash和交接；唯一完成收件主程，中央发布，主测试接实际新head的GitHub结果。

## 1. 输入与本次结论

- 用户AGENTS§6 standing workflow及中央本轮“接续实际PR4 review NEEDS_FIX，立刻签最小纠正交C”授权；复用既定LAYOUT设计、S合同及已激活correction001/002，不开新ARCH/SYS/R轮。
- **VERDICT: NEEDS_FIX**，仅对应已完成的GitHub审查：[review5388315376](https://github.com/yyczz1/FightMatch/pull/4#pullrequestreview-5388315376)，准确head `0c5e0580d685e0f4d777446a641b8220333903c6`／tree `a9f24193a808020e5bb3bf273b2445586a762713`；不是重复本地review。主测试原始正文见 `testing-layout-s1-pr4-review-receipt.json`，6883B／SHA256 `5f21173480fd7290aa1e9a0c7a9228c73e0378fba41642a650b52ccf94b3b549`；保持两条原记录OPEN，代码交付不自行声称审查已解决。
- P1 [r4162866083](https://github.com/yyczz1/FightMatch/pull/4#discussion_r4162866083)：迁移租约只强制前9个源码，漏掉6个已修改fixture/assertion文件仍可通过，导致未受审测试输入参与原生迁移。
- P2 [r4162866085](https://github.com/yyczz1/FightMatch/pull/4#discussion_r4162866085)：invalid safe-area后单独禁用responsive组件，OnDisable仅退订，自己的LayoutDiagnostic和battle CanvasGroup gate滞留；应释放自身gate并保留最后有效几何。
- 冻结S原件 `TestArtifacts/FightMatch/LAYOUT-S1-001/S/source-receipt.json` SHA `b2971a32973d241be9284a7cc969b9462ff07d4b08cc264eefc1eb9eea1da322`（28162B）、`source.patch` SHA `eb9db2fb5ce9c4cd921b76c3c6f8b4e82a4cc3983b561b37576a7dd2f8c838ec`（233350B）。S的15路径after身份是本包前像，不是dirty master。主程04:34:12 UTC重核133输入及1079文件后像与S完全一致，C旧turn已completed/idle。

## 2. 唯一产品写入范围

| 路径 | 冻结S前像SHA256 | 本次新增＋删除行上限 |
| --- | --- | ---: |
| `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` | `1b1d48274161a577f4eea848247f7200005859b640f6f5adfb7bdd7cbc0f4b1a` | 80 |
| `Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs` | `7547081e427434d5fa6a8e506eaaaa8de24b343f7bbf389568eb0b3de2d30c07` | 60 |
| `Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs` | `c872ef5bdc3a6932adc94c711b29af2e9913a714aa8ad04f2a1a226de2d4dc5d` | 220 |

最多3文件／总360增删行，逐文件不互借；不创建产品文件，不改变public API、serialized字段/格式、程序集或依赖。builder仅改S新增的layout租约校验及必要private helper；PR1已有builder行仍完整保留。其它12份S源码、Prefab/Scene/全部meta、字体、CSV、PR3三源码及缺席meta、LOC/Packages/Settings/ArtSource均冻结。S六证据、原合同及两补充、原发布/审查回执只读。

## 3. 最小实现与可观察验收

1. **P1精确集合。** activation.sources必须与既有LayoutSourcePaths全部15路径一一相等（Ordinal、顺序无关）。拒绝null/空、任一缺失（含每个原后6项）、额外、重复替代及重复追加；null条目/路径同样fail-closed。保留每个文件的bytes/SHA和canonical/symlink校验，15项全进LayoutCheckFiles，迁移前后绑定继续有效。允许把该集合校验抽成同文件private helper，生产LayoutGuard必须调用它；不增通用验证器或公共接口，不改变37资源/2meta/进程/租约/输出守卫。
2. **P2只释放自身。** OnDisable退订并释放该组件独占的LayoutDiagnostic与battleInteraction gate，与Unbind共用最小清理逻辑亦可；最后有效RectTransform几何保持。单独disable不等于Host Unbind，不销毁本地化绑定，不改Host/播放/输入/其它CanvasGroup或按钮的锁；再次enable重新采样并按当前有效/无效安全区设门。重复disable/Unbind/销毁幂等，不泄漏Changed/willRenderCanvases监听，不额外提交业务或保存，不更改缩放/压缩/槽/Caption。
3. **针对性测试源码。** 仅在已有 `LAYOUT_01_ReferenceRecoveryAndModalHierarchyMatchesAdjudication` 与 `LAYOUT_05_TooShortHeightFailsClosedWithoutBoardOrHotZoneShrink` 内增加断言和必要private helper；12个方法名、8个顶层Overlay Enter/Exit生命周期及其余原断言保留，未来47计划集合不扩张。先写能捕捉原漏洞的断言，再作对应生产修正；本轮不实跑red/green。
   - P1经reflection到真实Editor生产校验helper（现有测试程序集不增Editor程序集引用），覆盖完整15及乱序可接受、逐项删去15项均拒绝、只留原9项拒绝、额外/重复/空/null拒绝。独立固定预期15路径，不从被测集合反推期望。再通过实际LayoutCheckFiles验证原后6项中的真实文件错误SHA/bytes被拒；测试仅读现有源码，不调用迁移入口、不落activation/P根或修改资源。不可把生产predicate复制到测试冒充覆盖。
   - P2复用05真Overlay rig：有效几何→invalid→只设layout.enabled=false且Host仍bound，断言自身诊断隐藏/自身group恢复、最后有效几何不变；Canvas刷新及safe-area变化在disabled期间不得重新锁回。另一个非本组件锁/诊断保持原状态，迟到pointer/业务head/保存原断言不变。重新enable在invalid下再次锁定，恢复有效后正常，重复禁用/清理不遗留监听。测试内临时对象仅内存fixture，按原生命周期销毁。

## 4. 静态核验、输出与停止线

- create-once根 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX01/`，仅3文件：`before.json`、`source.patch`、`source-receipt.json`。before先记录实际C thread/host/turn、签发/合同hash、3前像UTF-8原字节文本及bytes/SHA、S六证据hash；保护快照引用不可变S/after.json的inventory，重核全部1079文件匹配后才写产品。不新建runner/脚本；只读脚本可命令内执行。
- 源码后与S/after.inventory机械对比，仅上述3路径可变；逐文件和总行预算从before原字节计算。重核S六证据/合同/原review身份未变；原133输入核验脚本可只读postflight作累计检查，但stdout不得覆盖S/after。保留旧Scene尾空格历史结果，不清理资源。计算15源码新后像bytes/SHA/gitBlob，patch仅3路径、以本包before为基线，内存重放逐字节等于实际文件。
- receipt合并命令/UTC/退出码、范围与预算、保护检查、P1/P2实现/测试源码位置、两条review映射、未验证项、patch hash及before hash；实际测试发现/执行数明确为0，继续标 `SOURCE_READY / UNCOMPILED / UNIMPORTED / OLD_PREFAB_NOT_MIGRATED / NOT_PLAYABLE_CANDIDATE`。作者不签ACCEPT。该回执替代不了旧S，也不激活未来I/P租约；后续迁移需另绑定FIX01后的实际review head和全部15源码。
- 本轮Unity/native/编译/测试/发现/dotnet/下载/设备/Git写为0；不触发GitHubreview。只能做源码/文本/哈希/补丁机械检查。执行发现所需第4源码、预算不足、输入漂移、无法保留原断言/锁所有权，停受影响项交主程，不自扩、不回滚他人WIP。
- 完成条件：C停写且正式回执与3文件齐，主程唯一接回做范围/证据核对后交中央正常追加PR4新head；主测试接独立GitHub结果，之后才决定I/P/T。主程持续收到正式返回，不仅派发即止。writing-for-agents用于薄合同和完成条件，uGUI用于局部组件所有权；diagnosing-bugs的实跑复现/回归阶段因本轮明确禁跑而延期，不据此伪称验证通过。
