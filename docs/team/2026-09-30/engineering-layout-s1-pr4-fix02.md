# LAYOUT-S1-FIX02：重新启用后的首次invalid通知

2026-10-02 · `EXECUTABLE_SOURCE_ONLY`。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，签发turn `01a0faf8-c9a4-7f92-80ec-877252f97536`；唯一作者C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`，显式 `gpt-6-astra/xhigh`。用户AGENTS§6 standing workflow＋中央本轮FIX02分发为授权；主程唯一接C，中央追加PR4，主测试唯一接实际新head的GitHub审查。C旧FIX01 turn `01a0fae6-6765-7532-b1d7-6abc5357338d` 已completed/idle；新实际turn在before写入后才改源码。

## 输入、结论及范围

**VERDICT: NEEDS_FIX**，仅接独立GitHub [review5388449580](https://github.com/yyczz1/FightMatch/pull/4#pullrequestreview-5388449580)：head `ad49aa36ec79505b51dbe5c83ef138f167cdb545`／tree `c0f0350abfb9ba63a61b35084ab9cb96f8a5a25d`。原文回执 [testing-layout-s1-pr4-fix01-review-receipt.json](testing-layout-s1-pr4-fix01-review-receipt.json) 6028B／SHA `0db42a5b6e1693cfd93e4eb7698a5166d9ba17236157237e80a1b354fd65b09e`。新P2 [r4162989184](https://github.com/yyczz1/FightMatch/pull/4#discussion_r4162989184) 指出ReleaseGate把valid清false，导致disable期间起笔后，重新enable首个invalid不通知Host取消pointer。旧两finding未重复出现但线程仍unresolved，全部历史原件保留，不声称bot明确闭合。

前像采用FIX01 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX01/source-receipt.json#sourceS15After`，回执SHA `e5b428d7cfd950a376fece87bf8a52e11d2b4703006ef168b87f041c23ccbb82`；不是dirty HEAD。保留[S/SYS合同](engineering-ugui-layout-source-activation-system-001.md)、原133输入及两补充、[FIX01](engineering-layout-s1-pr4-fix01.md)已完成行为。

| 唯一可改产品路径 | FIX01前像SHA256 | 新增＋删除行上限 |
| --- | --- | ---: |
| `Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs` | `f3b61723e90391f72043d75b6ccf3f3d66d744414c8c93f0e33adcc1235fafdd` | 40 |
| `Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs` | `44697844c24520b7462cba5b4d830c1fc7bd973399ac61d602f7a3fd1950bf8b` | 160 |

最多2文件／总200行、逐项不互借。仅private状态/逻辑及原LAYOUT05内断言与必要private helper；不改Host、builder、public/API/serialized字段、其余13源码、其它测试方法或程序集。Prefab/Scene/全部meta、37资源/字体/TMP/CSV、PR3与LOC WIP、Packages/Settings保持；不新增产品/runner文件。

## 最小验收

1. Disable只退订并释放自身gate/诊断，保留localization与最后有效geometry；disable本身不发false、不取消Host pointer、不改其它锁。重新enable后的首个invalid真实采样必须通知Host，使disable期间开始的pointer被取消；在invalid→disable与valid→disable两种前史下都成立。连续未变invalid采样不重复发通知；恢复valid及再次disable/enable正确，Unbind/Destroy清理仍幂等。使用最小private状态表达“释放gate后尚未采样”，不伪造业务提交或新增接口。
2. 先补现有 `LAYOUT_05_TooShortHeightFailsClosedWithoutBoardOrHotZoneShrink` 真实Overlay回归源码，再改生产逻辑。经rig实际输入驱动起笔，不能只reflection设valid或直接调用Host callback：至少valid→开始pointer→disable（pointer仍在）→主动取消后disable期间再次起笔→改变为invalid→enable→立即无active pointer→迟到Move/Up不提交；同时覆盖原invalid→disable→期间起笔→首个invalid通知。必要事件计数订阅在finally退出，证明disable零false、首个invalid一次且重复Canvas刷新不重复。head、保存次数/文件、LastRequest、最后几何、独立锁、现有清理断言全部保留。
3. 12个原测试名／8个顶层Overlay Enter/Exit／47计划集合保持，不增测试方法、不删或弱化原断言。实跑发现／执行为0，不能称red/green通过。若真实输入无法在现有rig复现，交主程最小原因，不复制生产predicate或另建不相关fixture。

## 输出、机械核验与交回

- 新create-once根 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX02/` 仅 `before.json`、`source.patch`、`source-receipt.json`。before先记真实C thread/host/turn、主程签发/合同hash、两源码原字节文本/bytes/SHA和冻结时点；保护基准为S/after.inventory的1079项虚拟覆盖FIX01全部15后像。逐项重核匹配才写产品，新增/缺失路径同样检查；S六证据、FIX01三证据、原合同/补充/发布/两review回执均只读。
- postflight与该前像机械对比只许上述两路径变化；保留原测试行/其它11方法体/12声明/8生命周期，校验逐项及总行预算；重核原133输入保护（调用原staticVerifierPython postflight仅stdout，不覆盖S）。patch只含2路径并在内存重放等于实际后像；receipt列全部15最终bytes/SHA/gitBlob与原finding映射、命令UTC/退出码、before/patch hash、未验证项。不得替换旧S/FIX01。
- 本包Unity/native/编译/发现/测试/dotnet/设备/下载/Git写/新review触发均为0；状态仅 `SOURCE_READY / UNCOMPILED / UNIMPORTED / OLD_PREFAB_NOT_MIGRATED / NOT_PLAYABLE_CANDIDATE`，作者不签ACCEPT。需第三路径、预算超限或输入漂移则停受影响项上报，不回滚他人文件。
- C正式停写且三文件齐后回主程；主程只作范围/证据检查，立即交中央追加PR4，不等额外候选文档、不另开ARCH/SYS/R。本包不激活I/P/T；[原生准备](engineering-layout-s1-native-execution-001.md)仅存已核事实。writing-for-agents用于薄合同，uGUI用于局部所有权；diagnosing-bugs实跑复现与回归因本轮禁跑延期。
