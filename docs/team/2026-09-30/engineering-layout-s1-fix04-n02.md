# LAYOUT FIX04 / N02 — 当前执行者的最小绑定后继

2026-10-04 · SOURCE_BINDING_ACTIVATED；NATIVE_AWAITING_EXACT_ACTIVATION。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，turn `01a1061d-c7b2-79b0-a72e-7fc5c30d7f1a`；唯一 owner 当前 C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，首次准备实际 turn `01a10626-296f-7da2-8249-740f345dd050`，回执以实际工作 turn 为准。主程唯一收件，中央唯一收主程。

## 授权和差量

直接人类授权见 `central-resume-scene-loc-2026-10-04.json`。中央同一 turn `01a1061c-3792-7a31-925f-0f6caac83979` 已补充技术确认：允许最小源码 delta 更换当前 owner/issuer 和新证据路径，保持旧 mode 与五属性算法/测试不变。此为旧 `engineering-layout-s1-fix04-native-proposal-001.md` 的窄后继，原执行和验收上限全部继承；不授权再次运行旧拒绝尝试。

唯一允许更改的共享产品文件：`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`（B），前像160371B / `ebccb4d9cec5d5d9fffa025fac78c76829fc53a85370dedfee7dd30d93308ca5`。仅 `LayoutGuard` 中 sceneRepair 分支的精确绑定变化：

1. executionRoot 的该分支由 `/FIX04` 改为 `/FIX04/N02`。
2. 该分支 ownerThread 精确为当前 C；所有非 sceneRepair mode 仍要求原 C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52`。issuerThread 仍为主程，无需改；实际 ownerTurn/issuerTurn 随新激活写入。
3. 该分支 sourceReceiptPath 精确为 N02 的 `/S/source-receipt.json`；旧非 sceneRepair 的原 `/S/source-receipt.json` 行为不变。

上述三个锚点总新增行+删除行≤20（中央同 turn 补签）；保留路径 canonical、create-once、精确 argv、隔离投影、15源码/37资源/2meta/保护输入/GUID、批模式和其他所有门。不得扩大 owner/path 白名单或引入通用模式；`RepairLayoutSceneOverrides` 算法、原测试文件和旧六测试字节保持。

## 源码阶段白名单与验收

N=`TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N02`，首次写前整个 N 不存在。除共享 B 外，仅新建 N 内 `S/before.json`、`S/FightMatchAndroidBuild.before.cs`、`S/FightMatchAndroidBuild.cs`、`S/source.patch`、`S/source-receipt.json` 五叶子（≤1MiB）。所有编辑用 apply_patch；原 FIX04/I、PExecution、source-receipt、patch及其他旧证据只读。

旧 source-receipt 固定33609B / `3f20fb6c83f6d18652f08940d4088bcc47abff89e63dbc4e340020db0566519f`；15源以其 `sourceS15After` 为前像，新 receipt 记录同名完整后像数组，只有 B 可变化。前/后检查所有旧 FIX04 叶子、其余14源、native Scene/Prefab/meta 和合同保护身份。用有界进程内静态检查≤30秒确认精确变更范围、旧mode常量行为及修复算法/测试字节不变、补丁可还原原 B；无 Unity/UPM/子执行/Git 写。保留命令、实测时间、失败、bytes/SHA、实际owner/issuer与source-only状态。

源完成立即给主程 receipt+hash+最小diff，状态 SOURCE_READY/UNCOMPILED/UNTESTED。此时尚不 sync 或开 Unity；主程以新封签和当前实际执行 turn 签 N/I/activation.json 后另发准确执行消息。

## 原生阶段后继（待精确激活）

旧 native 提案中 FIX04 的 I/P/PExecution/PExport 输出前缀全部换为 N02，sourceReceipt 改用 N/S/source-receipt.json；旧输出不覆盖。其余 Editor 路径/身份、独立 layout projection、UPM_CACHE_ROOT、Scene前像/五属性目标、保存/关闭/只读复开、资源上限和收尾规则原样继承。投影仅同步原 FIX04 两源，其中 B 使用 N02 后像，测试使用原 FIX04 后像；15项必须匹配新封签。

主程独占新建 `I/activation.json`；builder 独占 `P/native-migration.json`、`P/native-reopen.json`、`P/serialized-targets.json`；C 仅新建 `PExecution/before.json`、`runner.py`、`process.json`、`process-events.jsonl`、`editor.log`、`after.json`、`receipt.json`；成功才可新建 `PExport/before.json`、`receipt.json` 并精确导出唯一共享 Scene。runner≤350非空行，复用旧 runner 最小适配，不能新增 helper C# 或绕过保护。

preflight≤120秒，Editor最多一次≤180秒（含必要compile/import/UPM），retry=0；监护≤2秒，cache≤1GiB、投影派生≤4GiB、单日志≤8MiB、证据≤192MiB、空闲≥2GiB。仅 UPM_CACHE_ROOT 环境覆盖，不新签 TMP 或工具替代。首错封存，必要 owned 收尾≤60秒自然等待+单次精确 SIGTERM+≤30秒观察，无SIGKILL或他人进程信号。成功须逐项满足旧提案全部门，不能仅凭exit0；先 source+真实Scene同head GitHub 审查，再另签97项D≤60/T≤600，设备另验。作者不签集成ACCEPT。
