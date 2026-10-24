# M10 修正后的下一次依赖还原提案

2026-10-12；可执行范围已准备，尚未取得新的整轮执行许可，不是派发令。

M10在5.021159秒因外置回执的历史OS工具编号抄错一个字符而停止，三条restore均未启动。主程已接收单字段更正和一次真实静态启动检查：`consume_source`、`verify_activation`使用磁盘中的实际记录，5.746348秒通过。未重跑源码检查或封存，没有新增下载。详细依据由主程维护于 `engineering-loc-m10-correction-receipt.json`；旧错误回执保存在 `engineering-loc-m10-sidecar-before.json`。

更正已推至PR2提交 `489b28dc62adaa24242c72e4e9e6bf2117fd11a8`（tree `407c6b2bf88181c834cf17ccf79fcaa46f3fbd83`）：1项替换、3项证据新增，其余1913个旧blob不变。源代码仍是GitHub已审提交 `a0bfe7007be65eed6d1afda317934195cb6ebbd6`，没有把记录更正提交冒充为新的已审源码。

## 拟执行范围

- 仍使用FIX08封存的八个源文件和原资源上限；源码描述见 `central-loc-fix08-next-material-proposal.md`，其中旧回执哈希和E10路径仅适用于历史M10。
- 当前正确回执为 `loc-m17-fix08-command-receipt.json`，27675字节，SHA256 `3eba21dc21d4cc0bee679f4431699c3463a972c3c4500bb8a27a981c7a6b1802`。不得复用绑定旧哈希的M10激活记录或host。
- LOC执行、中央唯一接收。全部31个NuGet归档和1个源码归档只读复用；新增下载0。一次完整调用最多900秒（含准备、预检和收尾），最多依次执行三条restore，各180秒；任一步失败即停止，不自动重试。
- 材料根仍拟用 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix08`；工作根仍拟用 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix08`。主程确认两者未创建，实际绑定时如已占用则停止。
- 新证据根拟用 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m11`。取得新许可后，仅填写既审模板CONFIG，绑定新实际执行者、当前人类许可及正确回执；不重跑准备性的长测试。
- 验收要求：三条实际restore成功收据、依赖图及锁文件校验、材料清单、受保护文件未变、已知进程结束，以及独立结果接收。静态检查通过不能代替实际还原成功。

原M10的一次启动额度已使用，失败和修正不会自动增加次数。新提案不包括工具构建、官方测试、本地化编译、Unity、APK或设备验收；这些后续步骤仍需各自的就绪条件和限定执行范围。
