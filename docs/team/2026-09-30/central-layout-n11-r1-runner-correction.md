# N11R1：修复诊断执行器并完成一次原定观测

2026-10-06。中央签发；用户已授权真实场景修复、保存复开、限定验证及团队持续接续。本包只修 N11 工具缺陷，不新增产品方案或放宽验收。

## 固定输入与责任

- 继承 [N11 中央合同](central-layout-n11-probe-preparation.json) 的冷22副本、独立根 marker、五次观察、分类及全部保护/资源限制；以下差量优先。
- N11 已消耗：C actual `01a10f01-dbd6-71a0-93a5-8bfb8f5ad1d7` completed，主程收件 actual `01a10f0d-9343-7fc2-be46-c75196eac737` completed。命令 `exec-165d5cc5-6e31-41d1-ac42-8443f92d52a1` exit1/1172ms，Unity0。首错为 `event() got multiple values for argument 'kind'`；不是审批拒绝。
- 固定 N11 receipt：`TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N11/PExecution/receipt.json`，41420B，SHA256 `32564ef907c6beee5b3be437ad88baf41d52e7983c6014bc2ffbe46cfa5ea752`。N11 全部12叶1041082B作为只读前任证据，不覆盖或补写。
- 复用主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local` 与唯一 C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`；双方绑定新实际回合，显式 `gpt-6-astra/xhigh`。C交付，主程唯一收C，中央收主程。无新SYS/作者/审查轮转，无新聊天。

## 精确范围

- C新证据根：`TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N11R1/`。允许原N11相同17个相对叶，另加 `PExecution/predecessor-cleanup.json`，共最多18叶/总201326592B。不得借新目录改写旧运行结果。
- 主程只更新 `docs/team/2026-09-30/engineering.md`；本差量可直接派发，不再制作重复长合同。中央维护本单与README。
- 新执行器由冻结N11 runner派生；允许修正两处事件负载冲突、下述启动前部分创建收尾、必要的新路径/输入身份/owner/issuer/nonce绑定。其余探针流程、结果分类、资源/进程/保护守卫不变。
- C#只变新证据目录和实际owner/nonce等本轮绑定；原marker名、事件标签、观察及保存逻辑保持，不能顺便修产品代码。S原Scene与冷22保持N11字节精确相同。C#仍须标记未编译，直至实际编译证据。
- 原共享及投影B/T/Scene/Prefab/其他原Assets、meta、Packages、ProjectSettings均禁止写入。新投影四路径仍仅N11合同所列Scene、probe源码及各自meta。

## 先清理前任唯一残留

- 仅允许删除 `TestArtifacts/FightMatch/LAYOUT-S1-001/projection/Assets/Scenes/FightMatchCanvasPersistenceProbe.unity` 这一已归档、未导入副本，不删除目录或其他文件。
- 必须fresh确认无Unity/UPM正在使用投影、N11 owned闭合；无符号链接；该叶12012B/SHA256 `9eceb484adf685778966179095f3a69722f1b86fbdef3a167db59806d6b47590`，device16777244/inode10919108/uid501/birthtime1791253925.150476/权限0644，和N11创建记录一致；N11 `P/scene-final.unity` 及 `S/scene-cold22.unity`归档字节相同；其余三个临时投影路径必须缺席。
- fresh核对全部通过才逐项记录删除前身份、删除动作及删除后缺席到新 `predecessor-cleanup.json`。不修改N11旧回执中的null。任何身份/进程/路径漂移即保留并回报，不猜测清理。
- 本项是已授权限定工作内的可恢复临时文件清理，可以先完成；不视为Unity启动许可。清理后才能封存新的四路径缺席基线。

## 最小修正与验证

- N11第416行 `NEW_COPY_DONE` 和第218行 `NEW_META_OBSERVED` 将带 `kind` 的创建记录展开到 `event(kind, **values)`，同型冲突两处都修。用独立嵌套字段保存创建记录，保留顶层事件 `kind`；只改形参名称但让payload覆盖顶层kind不算修复。核对实际消费者，不另造事件协议。
- 回归检查必须调用从新runner提取的真实 `event` 函数和上述两处真实调用表达式，以内存sink代替文件，证明均不抛异常，顶层事件名和嵌套创建类型/身份均保留。不得只搜字符串或手写一份近似实现来证明。
- 如处理本轮启动前失败，只允许 `actualLaunches==0`、owned为空、meta两路径缺席时，清理本轮已记录、已归档且fresh身份相同的两个独占副本的实际存在子集。源码归档可用本轮S源，Scene用P实际归档；不存在叶保持缺席。Unity已启动后的原四叶完整归档及fresh清理门不放宽。该分支加内存fixture，漂移必须保留。
- 只跑与上述差量有关的语法、实际调用、部分创建收尾及身份/范围检查。未改变的marker分类沿用原15项准备证据，不重复整套检查或产品测试。最多3轮/累计30秒，首过即停；完整详情入preparation，stdout总≤8KiB。
- 新TMP仅 `/private/tmp/fm-ln11r1.XXXXXXXX`，继承短路径/0700/同UID/无link/空目录和预算；原N09/N10/N11 TMP保持。封存原N11全部证据及新准备输入。

## 一次执行与回传

- 准备完成后C按正常工具流程提交新的具体命令：`python3 -B '/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N11R1/PExecution/runner.py'`。旧N11批准不复用；任何拒绝原样记录即停，不换路径/权限/执行者绕行。无需再次询问用户开发范围。
- 获实际许可才复制新Scene/源码并启动固定Intel Unity2022.3.18f1一次。preflight≤120秒；Unity含导入/编译≤120秒；Save≤1/只读Reopen≤1/retry0；自然收尾60秒后fresh owned各最多TERM1/30秒，其余预算均继承N11。不跑112测试、不做Play/Host/设备/构建、无产品导出和Git/PR动作。
- 主程核对真实差量、完整有界回执、实际命令/编译/观察、保护与清理、C completed/停写；回中央 `DIAGNOSTIC_READY + outcome` 或 `BLOCKED/UNKNOWN + 首错/实际计数`，不据文件出现提前完成。
- 诊断结果仍不能替代原PR P1修复、同head独立审查、功能/设备验收。若有工具审批等待，给准确聊天/实际回合/命令/可见调用ID；无变化不轮询或重复转发。
