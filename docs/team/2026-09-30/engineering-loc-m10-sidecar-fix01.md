# LOC M10：历史 OS 工具 ID 侧录纠正

主程 actual `01a12f6b-6305-7281-9d8a-ebf6dcdaa1e5`，接收中央 item `fco_01a12f6b-631a-7ef3-adb0-1b4833e3dcd9`。依据用户在中央 actual `01a12f63-745a-70e0-8fd5-d5256a39e1fb` / 人类 item `01a12f63-7521-75c0-897e-2bdff545c277` 要求继续，以及 AGENTS §6 / TEAM_WORKFLOW §6 的组内普通修正授权。中央本次明确授权主程诊断、直接派LOC、接收并完成一次≤30秒完整静态启动门检查。

唯一执行者：LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，请求Astra/xhigh，新actual在回执记录。唯一完成接收者：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`；在本聊天final返回，由主程wait收件。中央只接最终汇总。

## 已确定的错误与固定输入

根因是新后置侧录单字段录入错误，不改已审生产代码。历史 `loc-m17-fix07-repair01-inputs/failed-tool.json` 的原始 commandExecution 是 `exec-4edf9488-4e3e-4c64-8627-a36ae7e16de9`；该文件4119B/SHA `f03178a56aefb269cc83c4a4828a953c06063e2874bc63011cfaea9328e548e7`，原作者actual `01a12e5d-f4c9-7443-87c9-3eb4d3f47ce4`。原工具aggregate exit20由M08 inventory SHA引起，已完成的三个OS探针另有结果，不能改成aggregate成功。

记 R=`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`，D=`R/docs/team/2026-09-30`，BASE=`R/TestArtifacts/FightMatch/LOC-LIC-ALT-M`，F8=`BASE/loc-lic-alt-yamldotnet-16.3.0-m17-fix08`，E10=`BASE/loc-lic-alt-yamldotnet-16.3.0-m17-m10`。

- 原始failed-tool在 `BASE/loc-m17-fix07-repair01-inputs/failed-tool.json`；FIX07侧录 `D/loc-m17-fix07-command-receipt.json`35151B/SHA `5c027e28a18b37d2e8abca762bbb84b6304d276840b824f75f04aa7faea56797` 的原工具与摘要也都是4e3e。
- F8 driver278195B/SHA `666faf4d3ed8bc586e8edb91d2c4d07013490d045a11dc47716ef4aa5cd93dbb`，3504行 `_fix08_os_sidecar` 要求4e3e；freeze2808B/SHA `c942f1456cdccf2c1a55f8685eb4d055aad72325d4848877e12d97a380824c0f`；model SHA `edf98191eb9cf105d8e4c6361aea2b69718dc75a2380b853128bd03eb5392209`。
- 错误侧录 `D/loc-m17-fix08-command-receipt.json`27675B/SHA `313f9cc46d470adb5689c273800ceb6f846628ba1567ee0a4d335dbce014e7e1`，只有 `/osVerification/toolId` 把4e3e写成4e3c。完整字节仅该ASCII字符更正后的预期SHA是 `3eba21dc21d4cc0bee679f4431699c3463a972c3c4500bb8a27a981c7a6b1802`，长度仍27675B。
- M10 actual `01a12f65-de4c-7901-a66a-9c6b58cd7c33` / tool `exec-69b3ec4e-284f-41fa-9edb-31ba86c5d175`，host1/exit1、5.021158958秒；restore0/GET0。已补原工具的E10 receipt25568B/SHA `b115986276954797f62eb1c722d1e82ba172915a7b98c45f63dc395a83f32d6f`。
- 旧 M10 A 为 `D/central-loc-m17-m10-activation.json`22778B/SHA `055218ee3bc531cc7597b8aa831099373839d89bd838cf05b953af6fb347cd11`；H SHA `2c0f752ed91e74754be1b4e37a2ecfa2db4fa9d5cd901b2d5f135b2e6c26a8c5`；binding SHA `fee5c97f220009629c3636eb37dcf13e89b60650b9b7ff9bcf5357ed9426f5e7`。均保持原记录，旧A仍指向错误侧录前像。
- 既有 SOURCE verdict SHA `431b901eb571e9f6f619ebd7b3f6cdd90f08d19c69a78e0d01e97b31889052af`，PR2已审head `a0bfe7007be65eed6d1afda317934195cb6ebbd6`。该历史verdict引用旧侧录，保留；本次另立纠正回执补足追溯。

## 精确写入范围与顺序

LOC新增且仅新增D下三文件：`engineering-loc-m10-sidecar-before.json`（逐字节前像）、`engineering-loc-m10-static-gate.json`（一次检查原始结果）、`engineering-loc-m10-correction-receipt.json`（身份、差量、保护检查、原工具完整回执与结论）。三文件合计≤256KiB。主程单独维护本文和后续组内接收文件，LOC不写它们。

1. 核对上述身份、新三文件缺席，记录本次actual。先逐字节复制错误侧录到before文件并回读核对原SHA；机械副本允许复制，实际修订用apply_patch。前像未成立则停止。
2. 前像成立后，唯一允许修改的既有路径是 `D/loc-m17-fix08-command-receipt.json`，且仅 `/osVerification/toolId` 的4e3c→4e3e这一个ASCII字符。不得重排JSON、变换换行或改作者/时钟/记录/状态。新SHA必须等于上述预期值。纠正者新actual记录在单独correction-receipt，不将其冒充原作者或重新封存执行者。
3. F8八叶、F7及原OS证据、E10四叶、旧A/H/binding、历史verdict、所有M/W根只读。仅核已列输入，不遍历SDK或partial根。保存上述小集合及F8八叶的前后身份；旧M10失败保持不变。

## 一次完整静态启动门检查，完整调用≤30秒

使用真实已审F8 driver定义和真实磁盘输入，不重跑fix08_checks/prepare/历史SOURCE或OS套件。允许在一个带30秒计时上限的Python检查调用中载入driver定义；`__name__`不能是`__main__`，生产 `TRUSTED_CENTRAL_ACTIVATION` 从始至终保持None。精确驱动如下：

1. 验证driver/freeze/八叶身份，调用真实 `consume_source(freeze_bytes)`；使用默认真实reader及生产模型重建，不传 `expected_model`、fixture模型或替身source reader。这只读取既有闭合输入；不得调用SDK readiness扫描。
2. 用刚保护的真实错误侧录前像，经真实 `_review02_receipt_content` 记录预期 `receipt mismatch: old OS tool`，作为已有失败的静态对照；不执行任何原OS命令。
3. 读取真实旧M10 A及binding。只在内存派生A的一个差量：将 `sourceCommandReceipt` 的bytes/SHA替换为磁盘更正侧录的实际身份，path不变；除此全部字段与旧A相等。据这份内存序列化字节派生函数参数pin的bytes/SHA。它是静态诊断输入，保留已消耗的旧身份，不写新activation/host，不构成新中央授权，也不注入生产全局pin。
4. 调用真实 `verify_activation(pin, candidate_bytes, parsed_candidate, freeze_bytes, consumed_model, original_argv)`，**不传receipt_reader/source_reader**，让默认固定路径bounded-read读取已更正的真实侧录，完成全部激活、source receipt、OS历史绑定检查。成功必须返回True，并记录所用磁盘侧录SHA、八叶/model、仅一个A字段差量及pin仍None。
5. 检查期间不调用 `execute`、`execute_material`、`MaterialWriter`、`run_material_command`、sandbox安装、SDK/NuGet/Unity/下载，不启动子进程。可加只拦写入/网络/子进程的检查期audit guard；不替换任何被验证函数或reader。保留被拦事件；真正检查窗口只读，窗口结束后仅写指定检查结果JSON。

完整计时包括解释器内最早clock后的导入、源消费和校验，记录外层工具duration及完整墙钟、exit、异常、stdout/stderr。仅一次≤30秒，失败保留并交主程处理，不隐式追加。工具完整返回后，再通过read_thread捕获这一次原commandExecution及未截断输出到correction-receipt；不得内部自称成功工具。生成证据文件用apply_patch；不增加独立检查脚本叶。

## 接收标准

前像SHA与旧M10绑定一致；新侧录精确一字节差量、无其他JSON变化；原工具4e3e有原始记录；F8八叶/代码与已审head不变；真实静态完整门返回True且≤30秒；不启动任何host/还原/OS探针，旧失败及A/H保持；本次实际thread/host/turn、原工具输出/时间、检查前后保护身份完整。

这是元数据纠正，生产源码不变，无须为此重新封存SOURCE或重复GitHub代码审查；不进行Git变更。若发现代码也必须更改，先停在准确失败证据并交主程签独立小包，不在本包改门或降低断言。

最终明确：静态门通过不代表材料M通过。旧M10 host一次已返回；真实执行需中央另行给出新A/H和当前执行身份，绑定新侧录SHA并保留M10/E10，仍按0GET/最多三restore/900秒的具体新合同。LOC本回合结束后由主程收件并向中央一次汇总。
