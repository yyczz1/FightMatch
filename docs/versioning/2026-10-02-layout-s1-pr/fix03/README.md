# LAYOUT-S1 FIX03：停用时禁止布局重新采样

修复 PR4 FIX02 受审 head `24dcb0c18119f7d420efafdb7a02dd04a9389347` 的 P2 r4163111945。组件停用（包括父层级 inactive）时，Apply 不再采样、创建诊断或重锁交互；重新启用后首次实际样本仍发通知。Host 自己 Unbind 时合法的指针取消保持独立。

仅两份 C#，1+1 行生产修正、75 行回归断言，合计77行增删；保留12个测试名称、8个Overlay生命周期和47项拟定测试集合。中央复用主程机械验收，不执行本地代码审查。

source-receipt.json 与 source.patch 是原 FIX03 的逐字节副本；旧 S/FIX01/FIX02 和各 GitHub 审查原件不改。新断言尚未编译或执行。仍须最终 head 的 GitHub 审查，再激活已有有限 I/P 原生流程；未导入、未生成新 meta、未迁移 Prefab/Scene，非可玩候选。后续美术采用明显占位图形。
