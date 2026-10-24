# LOC 后续回执方式提案

2026-10-08，PROPOSED_NOT_ACTIVATED。原被拒动作是 LOC 主动向主程调用 send_message_to_thread 发送 M02 身份回执；用户文字批准和一次精确 /approve 重试后仍被工具自动审核拒绝，理由为历史引用不是可信当前授权。原记录 central-loc-communication-hold-2026-10-05.json 保持，不重试该发送，也不换发送方代发同一旧回执。

建议改为：中央直接使用只读 wait_threads/read_thread 接收 LOC 的新实际回合与完成结果，取消该任务的 LOC→主程主动发送步骤。主程不重复等待。旧 A2/H2 绑定已结束，不复用；若用户接受该后续流程，中央先取得 fresh actual，再按既有经独立审查的模板重新固定新的激活数据与启动身份。

只接续已批准的 M 阶段：18 个固定官方归档约25MB及3次受限离线依赖还原。保留现有源/封签/写域/870+30秒预算、实际工具审批及失败证据；不改变生产校验器，不跳过工具审核，不扩为build/产品导入/Unity/付费。网络或外置写入若仍被正常工具审核拒绝，记录拒绝并停止受影响操作，不改道重试。

当前只形成可供用户明确选择的方案；未唤醒 LOC 执行、未创建新 A/H、未下载或还原。现有场景与GitHub审查工作独立继续。

## 当前处理：按既有协作授权采用只读收件

2026-10-08，中央 actual `01a116c2-6a9d-7a03-b161-11bd6079a631`。此前选择题未收到答案，不能记录为用户新批准。复核用户本次提供的AGENTS第6节与TEAM_WORKFLOW第3节，均已授权调度者直接接回指定回合；后者明确“工具不支持跨会话主动回复时，由授权调度者接回指定回合并转交正式摘要”。因此接收路径作为普通协作适配由中央确定，不额外要求重复确认。

原主动send动作不再执行，不更改权限设置或改派代发。LOC的新回合及结果由中央正常只读read/wait接收，主程不重复收件。原M执行授权见central-explicit-execution-approval-2026-10-05.json；其尚未使用的一次M、25MB/18归档/三次restore及外置范围保持，正常工具审批仍逐项有效，拒绝时保留实际理由，不换执行器绕过。旧A2/H2始终失效。先建立新LOC实际回合，再生成新的M03 A/H/E绑定；中央只填已审模板数据，不改执行逻辑。此前“等待额外流程选择”不再作为通信读取的阻塞，但不替代下载/写入的工具审批。

## 最新工具结果：M04派发被自动审核拒绝

2026-10-08。M03实际回合结束时转而回答旧的审核机制问题；中央读取完整工具序列并核M/W/E03均缺席，确认无材料入口或新拒绝。A3/H3为未使用且已结束owner绑定，不可执行。

中央随后尝试纠正上下文并派发新M04，被send_message_to_thread自动审核拒绝（isError=true），原文：“该派发会 initiate a new 900-second external-material/download-and-write workflow, while the user has not clearly approved this specific LOC execution or resolved the pending passive-receipt workflow; the prior broad approval is insufficient for these side effects.”

这次是新工具拒绝，影响具体外部材料下载/写入派发。没有创建M04实际回合/A4/H4，没有材料动作；不通过中央、C或其它执行器间接执行，不再自动重试。旧明确用户授权不删除，但不能把工具结果描述为放行。向用户集中说明准确25MB/18官方归档/3离线restore/两个外置根/900秒及只读收件，申请本次具体批准；其它源码/资源/GitHub工作继续。

## 当前明确批准与M04正常重试

2026-10-08当前恢复：人类在中央actual01a11a60-d890-7660-b7ee-1accdb3ffe0c/item01a11a60-d8c2-71f0-b832-a4e0c8db237e明确批准LOC一次18官方归档/约25MB/三restore/外置盘/15分钟重试及中央读取。派发已成功，LOC actual01a11a62-0e7c-7553-aa47-c16cd6a276b9，M04中央新绑定就绪；旧拒绝和M03失效记录保留，尚无材料完成结论。PR20 V08 FIX01 head a360ad864171a3b49f7785ab7c797028d583d5c0 已收到GitHub6054341623 no-major、summaryCompleted；QA actual01a11a63-574e-7001-8edf-f4e963fa3204核收，中央resource-combined-v08-fix01-source-verdict.json限定ACCEPT。C已接一次V08 I→T89新包，实际结果待收。两条线分别写外置M/W和独立P/K/E，普通保护门不放宽。

本次下载/外置写入仍经过正常工具审批；不是忽略旧拒绝或更换执行器。具体授权与封签见central-loc-explicit-approval-2026-10-08.json和central-loc-m17-m04-host-binding-receipt.json。

## M04 实际结果

人类具体批准后，正常工具审核已允许并实际执行；3.646504秒退出1，`unsafe relative path`，GET0/restore0。中央及QA已读取实际结果，见 `loc-m17-m04-native-verdict.json`。一次额度已用完，不再沿用旧审批阻塞的描述。当前完成FIX03源码修正/审查，不自动第二次材料执行。
