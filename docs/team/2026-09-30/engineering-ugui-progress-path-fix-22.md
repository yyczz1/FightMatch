# UGUI FIX22：单次进度路径与 callback 候选闭合

2026-10-01 · r1 · 限定修复合同；中央派 C 后实施，本设计回合不执行。
依据：中央提供 PR #1 已审 head `755409ecc8f513c9547911af470c0ba307b63bfc` 及 [P1](https://github.com/yyczz1/FightMatch/pull/1#discussion_r4154220400)；本轮只读本地对应文件，未联网/Git核验，不另作代码审查。
固定前序：`docs/versioning/2026-10-01-ugui-pr/fix21/` 下 runner、fixture、contract、offline-result；中央实施派发前核其真实head归属及身份。H20/tree沿该contract的 `historical` 原值；不匹配则BLOCKED，不静默换输入。
设计 ARCH `01a0f2e6-1ac3-7d10-8855-54192b9489d4/local`；拟执行 C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`；唯一回中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。通信沿用户批准团队工作流，派发显式 Astra/xhigh。

## 1. 定位与最小选择

- FIX21 runner `execute_from_bound_inputs` 第1324行以 `" + OUT.name + "` 在 callback 中查字符串；callback 第108–113行只接受项目根下 FIX20 完整路径。上层绑定成功不代表真实执行入口可用，旧35项fixture的假 launcher未进入这一段。
- 采用一个固定新根：`O=TestArtifacts/FightMatch/UGUI-01/q4-correction-22`；唯一进度文件为 `P=O/test-events.jsonl`。runner、callback、contract、中央外置 activation 同时锁定 P；不接受任意目录，不复用/覆盖 FIX20，也不延后 callback 修正。
- callback 仅增加/使用一个私有完整相对路径常量、将 FIX20 字面路径及错误标签更新为 FIX22，并要求传入绝对路径本身与项目根组合出的预期路径 ordinal 相等，拒绝经 `..` 等归一化才相等的别名。现有重复参数、非link、顺序/大小限制、注册与追加语义保持。
- runner取消basename/子串判断：先核授权 `outputRoot==O`、activation路径恰为 `O/activation.json`、最终argv进度参数恰为项目根/P；从已绑定的新callback blob读取唯一常量并检查其被原路径校验使用，与contract的完整相对P逐字相等。所有祖先/目标仍拒绝link，失败发生在spawn前。
- O只允许一次create-new：中央激活前必须不存在，准备后只含activation和受绑定runner副本；进度文件由runner以排他新建创建空文件，callback只打开追加。已用O、非空进度文件或旧根一律BLOCKED，禁止自动清理或换根重试。

## 2. 精确写白名单

- 唯一现有源码：`Assets/Tests/EditMode/FightMatch/FightMatchTestProgressCallback.cs`（下称 Cb）。产品、原测试用例/断言、场景、资源、meta、Packages、ProjectSettings及所有FIX20/FIX21文件只读；正式LOC缺口不在本包。
- `N=TestArtifacts/FightMatch/UGUI-01/q4-correction-22-source`，`A=docs/versioning/2026-10-01-ugui-pr/fix22`。仅在N新增 `validation-tools/runner.py`、`validation-tools/test_review_binding.py`、`binding-contract.json`、`execution-delta.json`、`offline-result.json`、`source-receipt.json`；A仅新增这六个同相对路径的字节一致归档。
- 本设计文件由ARCH独占；C只读。C源码阶段不建O，不写Git、不启动Unity/编译；归档上传、新head对象与外置激活由中央依已有权限办理。
- 仅中央另行激活验证后允许O新增：`activation.json`、`validation-tools/runner.py`、`before.json`、`after.json`、`preflight.json`、`run.json`、`quiescence.json`、`final-receipt.json`、`grouped-exact222-coverage.json`、`tests.xml`、`test-filter.txt`、`test-events.jsonl`、`process-events.jsonl`、`progress.jsonl`、`unity.log`、`editor.stdout.log`、`editor.stderr.log`。
- 同一激活允许O的 `static/inputs.json`、`static/delta-manifest.json`、`static/expected-fullnames.json`、`static/partition-proof.json`、`static/static-result.json`；不新增其它证据目录/脚本或另开测试窗口。

## 3. 新旧 callback 身份分离

1. `execution-delta.json` 只声明 Cb 这一旧→新修改：旧身份来自H20真实blob及原manifest，新bytes/SHA256来自FIX22源码；记录旧/新完整P和差异。该delta自身必须纳入新head raw blob绑定，不靠作者自填SHA放行；Cb的新身份还须等于新head对应blob及实际磁盘字节。
2. 历史H20/tree、FIX20静态anchor/59固定输入/旧目录摘要/receipt及FIX21事后补证保留原义。历史callback按旧blob与旧before/after核验，不要求旧SHA等于新工作区Cb；不重写旧manifest或在新工作区伪重跑旧posthoc。
3. 执行preflight从已验证旧baseline派生当前候选，仅在 `addedFiles`（Cb+原meta两项）、`protectedCandidate`、92项诊断集内替换Cb记录并重算诊断摘要；source53/product29/resources37集合与字节保持。所有集合/原像先核旧证据，新像再核新head；不得简单跳过旧SHA断言或接受任意delta列表。
4. 紧接着完整核当前授权head的Assets/Packages/ProjectSettings及既有工具/外部CSV依赖闭包；非Cb新增、缺失、漂移仍阻断。既有8项独立LOC排除及依赖证明不扩张；其保护快照在激活时固定，不借本包改工具或表。
5. 修改 runner 的候选校验/输出引用，使当前 `frozen_now(addedFiles)`、`protected_files()`、diagnostic hash、before/after和返回计数都使用派生的新候选；历史证据链继续使用原baseline。源码/资源/绑定启动前复核及运行后漂移判定保持。
6. 新runner/fixture/contract/delta及本文映射新head真实tree，正式runner副本也核同一blob；源码合同不嵌入自身新commit SHA。保留FIX21全部对象/授权/依赖门和原35项证据，不把旧35通过称为FIX22已通过。

## 4. Source-first、离线与单次验证

1. C先交Cb路径小补丁、新runner/contract/delta及fixture；离线入口为 `python3 -B TestArtifacts/FightMatch/UGUI-01/q4-correction-22-source/validation-tools/test_review_binding.py`，一次≤120s，修复后最多一次复验，不启动外部编译器/Unity、不生成pycache。
2. 复用原35项断言在新fixture下执行，并新增真实调用链用例：至少到 `execute_from_bound_inputs` 的路径门、preflight当前候选校验和最终argv生成；只替换文件/进程I/O与最终spawn，不能继续把整个生产launcher或execute替换为立即成功。离线只确认C#源码常量/使用关系与限制差异，不能冒充执行过C# callback。
3. 正例：新授权P、新callback、新head完整闭包及单Cb新候选同时匹配，假spawn恰一次；旧H20证据仍可按旧blob核，local HEAD不同且独立WIP存在不误拒绝。另验证同域及跨域合法事件序列仍受既有限制。
4. 路径反例均spawn零且无写旧根：旧FIX20根；其它新basename；同basename异父目录；前缀相似根；相对路径/`..`别名/大小写变体；父或文件link；非本次create-new准备的O/已有非空progress；重复或缺进度参数；activation、contract、Cb常量、argv任一不一致；只改runner未改callback。
5. 身份反例：新Cb仍配旧addedFiles/protectedCandidate/诊断SHA必须在明确候选门失败；正确单Cb派生应通过；Cb新SHA不符真实新blob、第二个源文件变更、删除原meta、篡改历史manifest、绑定后进度路径/源字节漂移仍失败。记录每例原因与到达的生产检查点，不以字符串包含为绿测。
6. 中央固定源码/离线归档并上传新head，核真实head/tree→完整当前执行字节，提交GitHub PR Code Review；该head的路径修复审查门无未解决阻塞后，再单独激活C的本机串行验证。若源码改动，重新固定head/相关离线证据及审查，不沿用旧审查结论。
7. 需要一次Unity验证：Cb是Unity编译、Editor注册与跨域回调的实际C#，离线Python不能证明进度会产生。首选复用原exact12完整选择（两类各6项），不新增用例或改断言；它已有多次域切换与26条事件的证据，能同时覆盖注册、启动/结束和跨域追加。无需先跑独立compile、Core190、Host30、exact2或完整回归。
8. 激活合同沿原Unity2022.3.18f1图形exact12 argv，仅改绑定的新runner/P/输出根；一次启动，test≤360s，原8MiB单日志/192MiB证据/2GiB余量门、首次失败及owned自然60s+SIGTERM后30s收尾保持。首次失败/超时停止并返回，不自动重试、原生复开、APK或设备测试。
9. 新验证必须有exact12 XML全过、26条连续合法事件（RunStarted、12对Started/Finished、RunFinished）、全部选中fullname恰一次、无callback错误、exit0、owned-clear和新head前后闭包不变。检查P确属O且旧20/21证据未变；启动前阻断记录零启动并回中央，不是pass，也不授权自行重试。

## 5. 收件条件

- 保留FIX20实际12/12与原178+12+30+2=222证明，注明原输入；新exact12只验证Cb路径/进度集成及原选择，不追溯改写历史222或宣称新产品/Host正式LOC、布局、设备验收完成。
- C分阶段回中央≤200字+`N/source-receipt.json`／激活后的`O/final-receipt.json`，注明实际thread/host/turn、head/tree、白名单差异、离线/审查/Unity各门状态及阻塞。新head审查和单次集成证据齐备才可接收本修复；本设计本身不是修复完成或作者自批ACCEPT。
