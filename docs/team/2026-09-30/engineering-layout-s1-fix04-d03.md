# LAYOUT FIX04 / D03 — 新覆盖谓词的精确参数捕获

2026-10-04 · PREPARED，N03正式收尾并由主程派发固定失败回执后激活。中央已明确允许：N03已获精确红信号，不原样复现；只捕获缺失的local/world/rect/pixel/四角/scaleFactor与比较项。复用 [D02](engineering-layout-s1-fix04-d02.md) 及其继承D01的隔离诊断/保护/收尾机制，不新建框架，不推测性ForceUpdate或改断言。

issuer/唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a10641-a816-71d1-919b-97413f71ea56`；唯一C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh。同新actual turn机械封S、签I并直接执行，无逐门审批。人类授权仍 `central-resume-scene-loc-2026-10-04.json`。

- **固定基线：** N03/S/source-receipt.json16075B/a3f350ca4c0cd8aecd1d31b19459620d5b0c8b292ff271d4fa7207f0899b83a4；共享/投影当前B162700B/97d062af40ae59ae8fba67ad05e17860616b243d0949b337a0e336ca07d62fb2，T36769B/1d53ef04c21b6914a9aa9c9042cf98aba78c58e62b77f321edd11888e02acbdc。只读恢复副本N03/S/B。N03全部实际叶/失败与四旧TMP（其新TMP以N03回执为准）冻结；不再使用N02的旧源码baseline。共享Scene/Prefab仍固定原身份，若N03实际回执不支持该事实则停止，不猜前像。
- **唯一诊断增量：** 从N03 B逐字派生，仅sceneRepair根N03→D03，及在LayoutVerifySceneCanvas新覆盖谓词调用点记录**同一次计算使用的参数与bool结果**：localScale/lossyScale、rect和pixelRect的xy/wh/min/max、scaleFactor、四个原始world corners及四个映射后的screen corners、每个finite/positive/四角0.5像素比较项与误差；补Screen尺寸、pivot/anchorMin/anchorMax/anchoredPosition/offsetMinMax、该rect位置/父变换（含矩阵或等价完整分量）、Canvas身份/驱动对象/source fileID、五覆盖，以及I/S/候选SHA。标记 `[LAYOUT-D03]`，数值用R/Invariant往返字串，不用日志截断的默认Vector格式。不得给Transform赋值、刷新、禁启组件或额外Revert；谓词/Require行为与其它算法保持，仅可缓存原本会计算一次的输入/结果以确保记录同一值。自然调用可记录多点，准确标明顺序；至少完整捕获首次失败的谓词，不能将其它位置的数据冒充。
- **保存前停止：** 沿D02固定无条件PRE_SAVE_STOP，在Repair调用后且MarkSceneDirty/SaveScene之前；若原断言先失败保留真实异常，finally仍闭场。整个源码相对N03新增删行≤100。没有产品修复、保存/导出/复开；不带D02的旧三点调试代码。
- **写域：** D=`TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/D03`，首次写前缺席，只沿D02同名12叶（S四、I一、PExecution七）；S≤1MiB/D≤192MiB，空P可自然创建，PExport缺席。共享全部零写，隔离投影只B可临时sync。候选与所有metadata从最终字节统一生成；复用已有checker/runner最小变换，静态+AST最多3次累计30秒，普通制作错误窗口内纠正、各失败保留、不改检查意图。
- **唯一运行：** 同固定Intel2022.3.18f1/RepairLayoutSceneOverrides/独立projection/UPM_CACHE_ROOT；I/P/log前缀D03。仅一次新 `mktemp -d /private/tmp/fm-ld03.XXXXXXXX`，沿29～40字节/0700/无link/初始空/≤16MiB和512文件/保留规则。preflight≤120s，单Editor≤180s，retry0，runner≤350非空行；监护≤2s/cache≤1GiB/派生≤4GiB/log≤8MiB/free≥2GiB。沿D02监护、一次自然60s→fresh owned单TERM→30s，禁止SIGKILL/外来信号。不得测试/设备/Play/Host/Git/补下载。
- **结束：** owned全清且诊断B仍等封签，才仅该文件逐byte恢复N03/S/B，T/共享/Scene/Prefab/其余资产不变。诊断DLL保留但明确非正式验证。原始捕获与结构化回执逐项一致、固定旧输入与TMP保持、没有保存/导出，才称DIAGNOSTIC_READY；原生exit/新失败按实际记录，不称产品成功。返回最先失败的具体比较项及数值，不凭此臆断内部原生机制；后续必要正常布局刷新须基于这些数据另作实际修正，不在本探针偷偷执行。
