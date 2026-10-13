# LOC M11：修正回执后的一次依赖还原

用户在中央当前回合01a12fed-8088-74d1-ba8b-a1dcb9a09467、人类item01a12fed-810e-76b1-8fe8-cd001b8307c4直接授权：“允许按修正版再执行一次，复用全部下载文件、零新增下载，在外置硬盘执行三次依赖还原，最长15分钟，由中央接收结果。继续推进，减少重复记录和会话交接。”具体范围沿central-loc-fix08-m11-proposal.md；这是一次新的M11许可，不复用M10。

LOC 01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local为唯一执行者，中央为唯一收件者，Astra/xhigh。中央取得本次实际owner后，只填写既审模板CONFIG；等待central-loc-m17-m11-host-binding-receipt.json就绪，最多3分钟，每次不超过30秒。身份由中央读取，无需转发或新增身份回执。

复用FIX08已审源码a0bfe700及主程已接收的M10记录修正489b28dc。正确sourceCommandReceipt为loc-m17-fix08-command-receipt.json，27675字节，SHA256 3eba21dc21d4cc0bee679f4431699c3463a972c3c4500bb8a27a981c7a6b1802。八个源文件不变；不重跑SOURCE检查、封存或静态启动门。绑定就绪后按准确argv经正常工具审批执行host一次，不改A/H、输入、回执或权限。

M8/W8仍须不存在；新证据根E11为TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m11，仅before.json/stdout.log/stderr.log/receipt.json四叶，沿既有监护方式和原日志限制。31个NuGet包与1个源码归档只读复用，0下载。三条restore按原顺序各180秒，完整调用包含预检和SDK核对最多900秒（870工作+30收尾），不在计时外扫描SDK。保留父HOME/home/CODEX_HOME及全部旧材料、partial和证据。

记录实际工具、完整墙钟、三条restore结果、manifest、保护项和进程结束；沿已有方式补原工具输出，不新建汇报文档。任一步失败停止后续依赖步骤，不自动再启动；成功也不启动B/L或Unity。将实际结果留本会话，中央wait/read直接接收，不跨会话转发，不唤醒主程/主测试做重复接收，不自行签ACCEPT。没有新增Git、部署、发行或购买范围。
