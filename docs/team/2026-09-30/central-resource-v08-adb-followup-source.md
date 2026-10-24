# V08 SDK ADB 生命周期修正 SOURCE

中央 actual `01a11a60-d890-7660-b7ee-1accdb3ffe0c`；唯一收件中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。交付人 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，Astra/xhigh，记录实际 fresh turn。

设计固定为 `engineering-resource-v08-adb-followup.md`，7400B / SHA256 `577c040ec197cb9cd056acc142297a27b6a6380f7a46b5d3363b3653f61f541a`。基线为原 run/runner.py（114245B / f5801212606db11dbf9bef5452b071c8b612c4a282ec1039af425abb154f6da8）；先核实际文件名，若原名不是 runner.py，只用同 SHA 对应原始 runner，不推测其他内容。

唯一新写根 `TestArtifacts/FightMatch/RES-COMBINED-V08/adb-followup`；六叶白名单：`runner.py`、`replay-check.py`、`replay-results.json`、`receipt.json`、`fixture.json`、`source.patch`。总≤4MiB。原 run、source-before-fix01、P/K/E 和所有 native 工件只读。新 runner 是保存修正源用于审查，不授予执行它的 execute/main。

只改 sdk_adb_exception 绑定退役/当前候选数量判断和有限诊断；如需要新增小 helper，要在 receipt 列明，不增加通用进程框架。无条件阶段清理不接受。原 argv/UID/映射文件/FD/日志/时序/身份/查询错误/无源码FD约束全部保留。新候选不得继承旧候选资格，双当前候选仍拒绝。raw T 候选缺失，补齐测试数据必须标为 synthetic。

一次≤30秒离线回放，最多一次失败项修复重放；按设计覆盖全部相关生命周期及拒绝场景。不得运行 ps/lsof/进程信号/Unity/ADB/native/cache扫描。验收原始失败不覆盖、12程序集与89测试证据不重跑，业务输入字节不变；固定基线/原始证据引用、新源差量、测试命令原始输出耗时、每个案例结果和六叶闭集进入 receipt。结束 SOURCE_READY；GitHub独立审查由中央发布。新源不得原地替换 V08 已运行 runner。
