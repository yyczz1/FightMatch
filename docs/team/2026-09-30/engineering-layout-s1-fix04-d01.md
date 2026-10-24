# LAYOUT FIX04 / D01 — 保存前三点隔离诊断

2026-10-04 · ACTIVATED_DIAGNOSTIC_ONLY。中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 在原用户授权 turn `01a1061c-3792-7a31-925f-0f6caac83979` 明确批准一次隔离诊断，无需再等激活；人类消息与原文见 `central-resume-scene-loc-2026-10-04.json`。主程 issuer `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local` 当前 turn `01a10641-a816-71d1-919b-97413f71ea56`，收到中央消息 item `fco_01a10641-a81b-72b0-911b-ed4e6791b621`。唯一 owner 当前 C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh；C记录本次实际turn并机械签本包activation后直接运行，主程唯一接收，中央接主程。

## 红点与固定输入

复用 N02 真正运行的红点，不重复最小化或原样复现：`TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N02/PExecution/receipt.json` 32163B / `6fbbecdc9680f06da59e31f145414815c53b0b408d25e525544658a4a99cba21`；log:368–373 是实例局部 scale/min/max 复合断言，source 检查已过，SaveScene 未达，lossyScale 后置门未达。主程539ba5已核14叶1312286B、共享1089/投影996、66封存证据及零导出。N02两源已sync，不用旧首次sync前像。

- B=`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`：共享与投影当前均160434B / `5dda3f055c7f78016d91dc533382b53154d8dd1412abcf03913c64f0aeaaaac0`；唯一只读恢复副本为 N02/S/FightMatchAndroidBuild.cs。N02/S/source-receipt.json 25964B / `30623e645013634aaf6ad472f42ea5b11ffa23ce4288eeefdb23f04e328abce6` 的15源/native4为输入。
- Scene共享/投影仍12900B / `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd`，Prefab1322960B / `0064bbee5e0d84dfdfd627310a9abada5093252bb26cf363d0646b38586f19d2`。原513GUID、51依赖、37资源/2自然meta/7保护均保持。
- 沿N02前后清单与原FIX04/before全量baseline；新D01根不在旧证据集合中，N02十四叶/两I、旧FIX04十叶及其历史保护全部冻结。N02短TMP `/private/tmp/fm-ln02.9rDNmQQL` 的原根身份及after.json最终清单保持，不复用、不清理。

## 唯一调试差量及白名单

D=`TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/D01`，首次写前ABSENT且祖先无link。只准新建 D 内12叶：`S/before.json`、`S/FightMatchAndroidBuild.cs`、`S/source.patch`、`S/source-receipt.json`、`I/activation.json`、`PExecution/before.json`、`PExecution/runner.py`、`PExecution/process.json`、`PExecution/process-events.jsonl`、`PExecution/editor.log`、`PExecution/after.json`、`PExecution/receipt.json`。既有LayoutGuard可创建空D/P目录，但不生成三份产品native证明；D/PExport必须缺席。源证据≤1MiB，整个D≤192MiB。实际未执行的文件不补造。

共享B/测试/Scene/Prefab/其余产品文件全部零写。仅隔离 `TestArtifacts/FightMatch/LAYOUT-S1-001/projection/` 中 B 允许临时诊断后像，仍沿Unity自然生成目录和原cache写入例外。新增删行合计≤100，计入以下全部差量：

1. 从N02 B逐字派生候选；sceneRepair的固定执行根只由FIX04/N02换为FIX04/D01，receipt仍精确D/S/source-receipt.json，当前C/主程身份门和其他mode/保护门不变。不消费或修改旧activation。
2. `LayoutRepairSceneCanvas` 原赋值、过滤、SetPropertyModifications、验证顺序逐字保持，只在修改前、赋值后/SetPropertyModifications前、其后/原验证前，各加入一次带唯一 `[LAYOUT-D01]` 标记的结构化快照。每阶段记录phase顺序、activation/source封签身份、实例及对应Prefab源的localScale/anchorMin/anchorMax/lossyScale、Canvas renderMode/enabled/active/scaleFactor及五项target/path/value overrides。数字保留可往返精度，实例/源必须对应fileID1013562322995895578。仅记录事实，不通过ForceUpdateCanvases、启禁组件、额外Revert、调换顺序或修复值来改变实验条件。
3. 诊断入口在原repair调用后、任何MarkSceneDirty/SaveScene之前无条件停止：可用一个固定只抛 `LAYOUT-D01-PRE_SAVE_STOP` 的私有停止函数；原断言若先失败，保持原异常。finally仍关闭到空场景。不得SaveScene、SaveAssets、Prefab保存、执行reopen/export分支；不弱化/删除任何原断言。成功拿到三快照不等于产品修复通过。

中央在本包尚未封签/launch时追加：同三点只读记录 `Canvas.pixelRect`、`Screen.width/height`、`CanvasScaler.uiScaleMode/referenceResolution`、目标 `RectTransform.drivenByObject`（API不适用则明确不可用，不新增探针）。用于区别序列化覆盖与显示尺寸/驱动上下文，仍计入≤100行；已launch时不得为补字段另跑。

## 一次执行与恢复

1. C在同一实际turn封S四叶：输入/旧完整清单引用、N02→D01精确patch、诊断候选bytes/SHA、15源的投影后像、明确独立的N02共享15源、行预算及三快照/无Save停点静态检查。候选编辑用apply_patch；逐字节sync/恢复属于机械复制。准备静态检查累计≤30秒，最多初次+一次必要纠正，无额外Unity/探针。
2. 当前C仅一次 `mktemp -d /private/tmp/fm-ld01.XXXXXXXX`，canonical≤40 UTF8字节、当前用户/0700、祖先无link、初始空；≤16MiB/512叶。实际路径/根身份/前后清单记本包原具名证据。只对本Editor及后代TMPDIR覆盖，UPM_CACHE_ROOT仍原layout package-cache；其余环境不变。新旧TMP均保留，不宽删。
3. C获本合同直接授权，机械签 D/I/activation.json：绑定本合同hash/真实owner turn/issuer与中央授权、S封签、原N02身份、精确15/37/2/7与GUID、临时根、完整argv和预算；不得先填伪turn再续写。argv沿N02的固定Intel2022.3.18f1及RepairLayoutSceneOverrides方法，只将I/P/log路径换为D01。cwd仍独立layout projection；Editor86759760B / `71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f`。无需回主程再次等待source/activation批准。
4. runner从N02/PExecution/runner.py（26284B / `2a6cfc84a21e4afe872554050d9e295bd41506b1b3a9d9eea2d569523b29b5fa`）最小适配≤350非空行：新身份/单B输入sync、完整共享保护用N02源而非D01诊断源、精确新TMP、三快照提取、彻底关闭保存/导出验收分支，末尾受限恢复。保留完整保护、首错、预算、进程归属和收尾。任何自然/故障退出都记录真实exit与首错；预期原断言或固定PRE_SAVE_STOP不改记为产品exit0。其它异常仍真实失败，不放宽错误匹配。
5. preflight≤120秒：核完整输入、Editor、投影锁及fresh进程无其它Unity/UPM；只sync诊断B，其余14源/全部资产不变。仅一次Editor≤180秒（含必要compile/import/UPM），自动retry0；监护≤2秒、cache≤1GiB、自然派生≤4GiB、单log≤8MiB、空闲≥2GiB。无独立compile、测试/发现、Play/Host/设备、包安装、网络补材料或Git写。
6. 首错/预算封存，沿一次≤60秒自然等待→fresh PID/start/argv/闭包核定后每个仍活owned最多一次SIGTERM→≤30秒观察；无SIGKILL/外来信号。只有owned全清且投影B仍等于本次诊断后像时，机械恢复此一个文件到固定N02/S/B字节，记录恢复前/后SHA；若有漂移先停，不覆盖他人修改。绝不删编译产物；清楚标记它们来自diagnostic source，不能作为正式编译/测试证据。

## 收件条件

固定三阶段每个各一次，来源/顺序/数值与raw log可逐项对照；不足三点即DIAGNOSTIC_INCOMPLETE，明确缺项，不猜根因。报告五项过滤前后及现场向量差异，并区分“首次观察到变化的调用边界”与“已证明根因”；仅三快照仍不足区分时如实保留不确定性。共享全量/所有资产与旧证据/TMP不变、Scene未保存未导出、进程全清、诊断B已限定恢复（或准确阻塞），是本诊断的结束门。最终回执绑定12叶实际集合/身份、所有真实命令/时长/退出/信号、生产验收未执行，不发ACCEPT。简短返回主程实际事实与最小下一修复建议后停写；无需重读历史、另开架构轮或发消息给中央/QA。
