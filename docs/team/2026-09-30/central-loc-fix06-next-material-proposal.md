# FIX06 后的依赖还原提案：复用归档、零下载

2026-10-12，中央准备的可评审方案，尚未执行。M07 已实际运行并失败；用户明确的“一次”已经使用。普通源码修正继续完成，不据此自动新增真实还原次数。

## 修正与固定输入

FIX06 为三阶段依赖图中的 49 个准确 obj 目录补上直接 GUID.tmp 临时文件规则，授权、沙箱、目录准备、输出检查使用同一约定。每目录最多驻留 1 个匹配文件、共 16MiB；不开放整个 obj 或源码目录。这是驻留检查额度，不能声称是 OS 逐次写入限额。

- PR2：`454eb4da250262a396b17da97d83c71de408bd48`；触发6116642338。GitHub结果6116669351已完成并由主测试接收，准确head无major，非正式APPROVED；中央 loc-m17-fix06-source-verdict.json 为源码限定ACCEPT。尚需新的明确材料执行许可。
- SOURCE：`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix06`，八叶13351416B。
- driver236347B，SHA256 `715e54c8657265c2311b203e07719b0cb177f88d3140c52fbcc43a6e67e81fb1`。
- freeze2807B，SHA256 `08d12270a97b2a87be19debc9a711b71854d9d8cacacddf256e517db23387521`；model `5edefd5ebe5f443818f80cf3958e631e5a5e166015fe44b42959a800df80debf`。
- 后置真实命令回执 `loc-m17-fix06-command-receipt.json`19691B，SHA256 `10cc6a35761da1464cf3508ccaa9ad0755a92804796ec4a6cde362da3731812b`。中央已直接核对两条原工具完整输出及最终八叶。
- 定向检查55/55，完整调用4.638493秒/exit0；源码封存8.009652秒/exit0。新规则尚未获得真实OS/还原成功证据。

## 拟议一次运行

LOC 执行，中央唯一接收。复用全部31个NuGet包和1个源码归档；新增下载0，缺项或哈希不符即停止。依次只跑 runner生成、tests生成、tests锁定验证三次还原，各最多180秒；整个调用包括预检最多900秒，其中870秒工作、30秒收尾。无自动重试。

材料目录：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix06`。工作目录：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix06`。新目录被占用即停止，不清理或覆盖旧结果；M5/W5 依据已捕获闭集清单检查，旧归档和历史输入保持只读。维持材料512MiB、工作4GiB、至少2GiB剩余空间门与既有进程限制。

源码独立接收及用户授权均满足后，中央才按已审模板绑定新真实执行者、直接人类授权、准确head、八叶及侧录身份，生成新的激活记录/host和本轮四叶证据根；进入正常工具审批。不得复用M07旧激活记录或执行时钟。

成功需要三条本次真实还原收据、进程结束证据、锁文件/资产图校验、材料清单及独立接收。第一条失败即停止其依赖步骤，保留完整原始结果。此方案不运行 B 构建/官方测试、L 本地化编译、Unity、APK或设备验证，不合并、部署、购买或发布。
