# LOC M06：FIX04 一次材料执行

2026-10-11。用户在中央 actual `01a1298d-4509-7132-8e0d-9f433010ad0a` / item `01a1298d-4589-7d93-a269-75e3a3e1fc05` 明确授权一次13官方归档/约9MB/外置盘/3restore/15分钟/中央收件。原话见 `central-loc-m06-approval-2026-10-11.json`；不是复用M05许可。

唯一执行者为原LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，Astra/xhigh；中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 唯一接收。执行者不向其他会话发信。先读包并保持fresh回合，等中央读取实际turn后生成 `central-loc-m17-m06-host-binding-receipt.json`；最多3分钟、每次等待≤30秒。不要自行创建或修改A/H。

输入：已接受FIX04八叶，PR2准确head `816263eaacf4f3e276c0cef7703c5824af086cc6`，GitHub6105581700已Completed，中央 `loc-m17-fix04-source-verdict.json` 为源码限定ACCEPT。中央已重新核对源封签与远程head一致、M4/W4/E06缺席。所有旧源码/失败实物/五复用归档保持只读。

绑定就绪后，使用正常工具审批执行中央准确host argv一次。新M/W及E06四叶、原文件保护、完整13官方GET清单/9014649B、三restore和900秒（870工作＋30收尾）全部沿 `central-loc-fix04-next-material-proposal.md` 与固定模型。所有材料输入/SDK核验在同一timer内；不得另跑prepare、SOURCE套件或全SDK前置扫描。五复用文件身份复核失败立即停止，不改成重下载。

仅创建新外置M4/W4和新E06四叶 before.json、stdout.log、stderr.log、receipt.json（总≤16MiB、单日志≤4MiB）。保持HOME/home/CODEX_HOME。禁止重试、B/L、build、Unity、Git写入、清理旧根或临时放宽门。工具真实拒绝则原样报告，不换通道。

必须实际执行并给真实回执，或记录真实阻塞；状态询问/上下文压缩不自动取消本任务。回执包含原工具ID/外层墙钟/实际退出、GET数和正文B/复用身份/3次restore结果/manifest与进程收尾/受保护输入。作者不自签材料ACCEPT；中央直接读取该会话及文件并安排独立接收。失败保留原错误与partial roots，不追加第二次执行。

## 审批服务超时后的同次恢复

首次工具请求在CreateProcess前因自动审核超时被拒绝，原始返回明确允许retry once；host未启动，GET/restore均0。中央处置见 `central-loc-m17-m06-approval-timeout-disposition.json`。允许同一LOC正常审批重提一次尚未启动的M06，不是第二次材料运行；旧四叶及A/H不覆盖。新绑定前缀 `central-loc-m17-m06-resume1`，新四叶证据根 `loc-lic-alt-yamldotnet-16.3.0-m17-m06-resume1`；M/W/源码/预算/用户授权不变。仍禁止实际材料执行后的自动重试。
