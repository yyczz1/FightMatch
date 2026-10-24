# 下一次LOC材料准备：零下载，三次依赖还原

2026-10-12，中央提出的具体执行方案，尚未启动。用户上一条明确材料授权是M06一次、最多15分钟；该次已实际启动并在回合中断后结束观测，不能作为未使用额度再次启动。当前“继续任务”已用于中断核收、普通源码修正、短验证及提交审查。

## 固定候选

- PR2 head `01a5ab7afa5f3d2d55df4b5d6b72c8fa6e353736`，审查触发评论6112501532，完成结果6112531539报告无major。中央 loc-m17-fix05-review02-source-verdict.json 已给出源码限定ACCEPT；材料尚未启动。
- SOURCE：`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix05-review02`，八叶合计13287833B。
- driver205640B，SHA256 `ae1d2c3cb2a8cc1ea95d92fd5c7f3c5fce0eb90fcfa2235e9650023b9bd4ec5f`。
- freeze2870B，SHA256 `d32e5af44ba61473b333f919910167c6cad2b6691d19cc12e1395fe45e7147de`；model `de9f7e47850dcd2f6cdcb3e6f2c605b3d45975c2116dd3d19722cf16623b2580`。

外部真实成功工具回执 `loc-m17-fix05-review02-command-receipt.json` 20990B / SHA256 `55cb8c419ab9fe4da8f3a4b0a4fb69e38d925bed45a27bab8d510d177cf25cc5` 是源码接收及材料激活的必需证据；必须鲜核，缺失或不符即停。它位于八叶之外，不能通过改变自身封签形成循环。新activation必须含 sourceCommandReceipt 的准确绝对path/bytes/SHA256；driver在材料状态建立前及每次native命令前fresh核对，缺失或不符会拒绝。中央host模板逻辑不改，只固定新activation全文身份。

## 一次执行的范围

LOC负责，中央唯一收件。复用现有31个NuGet包和1个源码归档，缺失或校验不符就停止；**新增下载为0，自动重试为0**。在全新的外置目录完成原定三阶段依赖还原：runner生成、tests生成、tests验证。总上限900秒（工作870秒、收尾30秒），包括输入校验；旧失败、下载和partial目录全部只读保留。

新工作目录为 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix05`；新材料目录为 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix05`。占用即停止，不能清理旧根或另换目录。保留已审512MiB材料、4GiB工作区、2GiB剩余空间门及全部原进程/写入/身份限制。

用户同意后，中央按已审模板绑定本次真实执行者/人类授权、PR精确head、源码封签和预算，生成新的激活记录与外层证据根；经正常工具审批运行。该方案不启动工具构建/官方测试、Unity、APK、设备测试，不合并master、不部署或购买服务。

完成依据是本次真实退出/耗时、三次还原收据、锁文件和依赖图校验、材料清单及独立接收；文件出现或旧检查通过不足以接收。失败时保留完整原始记录，只处理具体原因，不自动重跑。M接收后才准备独立B构建验证，B接收后才接入正式文本配置。
