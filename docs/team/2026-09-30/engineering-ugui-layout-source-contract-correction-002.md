# LAYOUT-S1 固定Battle标题引用补充002

2026-10-02 · `APPROVED_FOR_SOURCE_ONLY`。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`、turn `01a0fa69-024a-75d3-9301-b4f493f39a45`，签给C当前turn `01a0fa6a-62bb-7c32-b577-4d268818b774`。仅修复既定固定Header层级在SYS字段表中的遗漏，不是代码审查或产品接收。

## 精确增量

- 固定输入仍为原system SHA `3220629d60fdb279e9328353d5a90156f0abc66415cf24bf1e6da42d4beba06d`、inputs SHA `5a2f1394e2b6fe03fdeaf7cb4d4a54a23f5b6f493d33b4dcf430e31f899a690d`；更正001 SHA `f70d408ad021ccdebda17d4cc99f1808a46a5f854b84dd81d4e0c5d7e86f851c`继续生效。上述文件保持原字节，以下是两项原本不存在的虚拟字段补充。
- 唯一新增成员位于 `Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs`，均为 `[SerializeField] private LocalizedTmpText`：`dialogTitle` → `BattleConfirmationPanel/Header/Title`；`recoveryTitle` → `BattleRecoveryPanel/Header/Title`。不新增public成员、接口、业务字段或其它序列化字段。
- 依据原布局base固定Header及correction §7–8的面板所有权／固定Header合同。原有 `battle-end-title`／`battle-recovery-title` 动态正文标题改由固定Header承担；保留其既有语义及稳定标识，不在Header和Body重复生成同一标题。Header永不加入动态rows或OwnContainer销毁范围。
- 确认退出／重开标题分别沿用 `fm.battle.exit.preview_title`／`fm.battle.restart.preview_title`；回退预览Header使用现有短标题 `fm.history.title`，Body原 `fm.history.undo_preview` 及actionCount和预览内容完整保留；恢复Header沿用 `fm.save_recovery.title`。不新增或修改CSV/key/翻译，不把带参数长正文移入48k Header。
- 两引用纳入既有Bind完整性检查，由当前PlayerBattleView独占绑定／刷新／Unbind；没有活动面板时不留下旧状态的可见标题，重复Bind/Unbind不增加监听，过期页面/locale生命周期合同保持。不能用运行时层级搜索代替typed引用。
- 同一原白名单内builder、共享fixture及新增布局测试显式接线和断言这两个固定目标；精确TMP目标集合包含它们，默认诊断占位符和既有英／简中运行时绑定保持。不得据此增加第16源码文件或第27总产品路径。

## 保持的执行边界

- 原15个C#／2新文件、7,840新增删除行总上限及逐文件上限不变；原S六证据文件不变，补充的path/SHA记录在现有static-checks/source-receipt内，旧before/handoff不重写。
- 新布局builder的严格私有I激活DTO属于原限定入口的实现细节：实际字段/类型/拒绝条件须在源码及回执固定，供未来I签单填写；不得改游戏存档/发布格式、已有Android入口或自行生成激活单。未知未来head/meta值不是本轮可执行值。
- Unity、dotnet、编译、测试／发现、Prefab/Scene/meta写入、Git、下载、正式艺术导入和I/P/T仍未激活。现有WIP保留，不新增审查轮；源码冻结后仅走对应新head的GitHub PR Code Review。
