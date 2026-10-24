# LAYOUT-S1-FIX03：禁用期间Bind延后布局采样

2026-10-02 · `EXECUTABLE_SOURCE_ONLY`。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，签发turn `01a0fb19-3973-7b62-a9ea-c335c6dcf65b`；唯一作者C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`，显式 `gpt-6-astra/xhigh`。用户AGENTS§6 standing workflow＋中央本轮FIX03分发授权；主程唯一接C，中央发布PR4，主测试接实际新head独立GitHub结果。C前轮 `01a0fb04-044e-7301-92fe-d6793a13a708` 已completed/idle；新turn先记录before再写产品。

## 固定输入与最小范围

**VERDICT: NEEDS_FIX**，依据[GitHub review5388584142](https://github.com/yyczz1/FightMatch/pull/4#pullrequestreview-5388584142)，受审head `24dcb0c18119f7d420efafdb7a02dd04a9389347`／tree `e57ad0526034b2edde0df8f4bd007bea2b956f17`。仅处理新P2 [r4163111945](https://github.com/yyczz1/FightMatch/pull/4#discussion_r4163111945)：disabled时Bind仍Apply，重新激活自身diagnostic/gate并通知Host取消pointer。原始[QA回执](testing-layout-s1-pr4-fix02-review-receipt.json) 6098B／SHA `4224b661e2be445758576b572ef2e9b0bd0f2cee3a94d614cca19428e3f4ebd3`；历史三finding状态原样保留，不因未重复或outdated谎称机器人明确闭合。本包不是重复本地review。

前像为 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX02/source-receipt.json#sourceS15After`（15011B／SHA `5b546c3d85245d21ade2e9e74313a7240edcc7a3994f2069d275087ac7eccec3`），其[发布回执](layout-s1-fix02-publication-receipt.json) SHA `f01dceaf9832fb0772d54d6cc3c2324d22308734dffa16414c874e7dc07ded66`。主程05:32:38 UTC只读核1079项虚拟覆盖FIX02全部15后像一致，FIX03根缺席；不是dirty HEAD。复用S/SYS与correction001/002、FIX01/02，不重开ARCH/SYS/R。

| 唯一可改产品路径 | FIX02前像SHA256 | 新增＋删除行上限 |
| --- | --- | ---: |
| `Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs` | `7b05c43b64467afff705d66eaf4486fe3a12da38d5804eaef450f986e6e48049` | 40 |
| `Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs` | `15375c668050f24f8b29e5d78323661f3713633e104ca40f255507f0e35510d2` | 160 |

最多2文件／200行，逐项不可互借。限Bind/采样生命周期所需最小private逻辑、既有LAYOUT05断言与必要private helper；其余13源码、Host/builder/public API/serialized字段/其它测试方法/程序集不改。Prefab/Scene/meta/37资源/字体/TMP/CSV/PR3与LOC WIP/Packages/Settings/ArtSource均冻结。现有native准备不重做，本包不授权Unity、下载或资源写入。

## 验收与验证边界

1. `isActiveAndEnabled`为false时Bind仍按原合同验证必需绑定、持有新LocalizationService及更新诊断文字绑定，但不订阅/采样/应用geometry、不启用自身diagnostic/gate、不发ValidityChanged；保持释放状态及最后有效geometry。通过最小守卫延后至真实OnEnable；保持FIX02首次真实invalid通知一次、持续invalid不重复、valid恢复、重复Bind/Unbind/Disable/Destroy退订幂等。不把禁用自身当业务invalid，不改变Host既有Unbind取消语义或其它锁。
2. 先补原 `LAYOUT_05_TooShortHeightFailsClosedWithoutBoardOrHotZoneShrink` 的真实Overlay回归源码，再修生产。至少证明：layout disabled且safe-area invalid，在实际Host绑定存在时经driver起笔→调用真实layout.Bind→pointer仍在、ValidityChanged计数0、自身diagnostic隐藏/group释放、geometry不变、layout订阅0；重复Bind和Canvas刷新仍然如此。随后真实enable立即采样、一次false经Host取消pointer，迟到Move/Up零提交，重复invalid不重复通知；valid恢复及其它独立锁、head/保存文件/次数/LastRequest原断言保持。布局对象不active时同样不得绕过isActiveAndEnabled条件。
3. 另经现有rig的真实Host重绑路径验证disabled布局依旧释放、无布局ValidityChanged且订阅0，enable恢复正常；明确Host.Bind自己的Unbind可合法取消旧pointer，不能把“整个Host重绑不取消pointer”写成错误要求，也不能用该取消掩盖layout.Bind回归。事件观察finally退订，不注入private状态/直接调用Host回调、不复制生产predicate。12原测试名、8顶层Overlay生命周期、47计划集合及全部旧断言保留；测试发现/执行均0，不能称red/green通过。

## 证据、停止线、交回

- 新create-once `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX03/` 仅 `before.json`、`source.patch`、`source-receipt.json`。before记实际C thread/host/turn、签发/合同hash、两前像UTF8原字节/bytes/SHA、前轮停写交接；1079保护基准为S/after.inventory虚拟覆盖FIX02全部15后像，写前核缺失/新增/漂移。沿FIX02/before的保护表冻结已确认发布README现像；另冻结FIX02三原件、FIX02合同/发布回执、fix02发布目录四文件及本轮QA回执，S/FIX01历史不改。
- postflight仅两路径可变，逐文件/总预算、所有旧测试行及其它11方法体/12名/8顶层生命周期保留。重核原133输入（原staticVerifierPython postflight只stdout，不覆盖旧证据）；patch内存重放逐字节等于两后像，回执列全部15最终bytes/SHA/gitBlob、精确命令UTC/退出码、before/patch hash、finding映射与未验证项。需第三路径、超预算、漂移或无法保留旧断言即停受影响项交主程，不自扩/回滚。
- 本包Unity/native/compile/discovery/tests/dotnet/设备/下载/Git写/review触发均0；作者只报 `SOURCE_READY / UNCOMPILED / UNIMPORTED / OLD_PREFAB_NOT_MIGRATED / NOT_PLAYABLE_CANDIDATE`，不签ACCEPT。C停写后正式回主程，主程完成机械收件立即把确切文件/SHA/turn交中央追加现有PR4；不等额外候选文档、不代中央发布。新head代码门过后再按既有I/P计划签实际租约。
- writing-for-agents用于薄合同和可核完成条件，uGUI保持组件局部所有权；diagnosing-bugs实跑复现/回归因本轮禁跑延期，不能据静态源码宣称实际行为已通过。
