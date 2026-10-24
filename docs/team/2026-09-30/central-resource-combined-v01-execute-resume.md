# RES-COMBINED-V01：未使用运行的短接续

2026-10-08。中央在C actual01a118b9-3655-7540-9430-400869281d29误提旧N07后撤回了启动。C完成回合，确认native/source同步/测试均0，E只有五输入及activation，BC/AS均未创建；临时目录仍空。中央实核工具序列和文件。原因是打印整份302KB preparation后上下文压缩，短暂重新回答旧问题；后来C已纠正。旧激活6245B/SHA cb2543f8ca1c9190426a5dc5b97b077f711895eacc0397a32c8c47ca796f45d9逐字节保存在resource-combined-v01-withdrawn-activation.json，不能执行。旧TMP保留未使用。

现在明确恢复**同一尚未使用的**首次I→T89任务，唯一C thread不变，使用本次fresh actual。不是重试native，也不是N07；原central-resource-combined-v01-execute.md的源码、输入、写范围、900含160预算和验收均保持。先前绑定实际0.042596秒计入160，不重置准备计时。新的重新绑定与读检累计不超过剩余20秒预算。不要打印preparation/inputs全体、历史文档或重新读全套技能；只读取所需字段和本页、原执行包，输出每条不超过2KB。

允许在比对原activation hash后**替换数据activation.json**：fresh actual、fresh新空TMP/身份、重新核实ADB旧log/SDK与当前消费者、重封原五输入；其它固定字段保持。保留旧TMP，不删除历史。读取已接收source-verdict即可，不做新代码审查/离线回放。新activation写完即按原包调用已审runner一次，外部monotonic记录完整真实exit。不要停在派发/身份环节，也不要发跨会话消息；中央在等待结果。

若工具实际拒绝本次执行，原样记录并停止受影响动作；不得更换执行者或间接运行。若预检失败，保留具体失败且0重试。只有一次有效原执行包所定I/T，不添加其它Unity入口。结束返回真实exit/89结果/恢复/完整时间/activation hash，中央唯一收件。
