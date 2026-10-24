# HOST-RESOURCE-MAC-003 — 短TMPDIR环境纠正

2026-10-03 · 中央已明确批准本次纠正直接执行。签发主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`／turn `01a0fde0-d533-7b00-ad78-34111f776dda`；唯一C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh。C完成回执由主程唯一接回再交中央。无需新设计、代码审查或逐阶段再批。

## 固定输入与最小改动

M02已completed/idle，I1/PID35471/20.162s/exit1，Bee IPC路径140>104字符，T0；[M02 receipt](../../../TestArtifacts/FightMatch/HOST-SAVE-ISOLATION-001/M02/receipt.json)3491B/SHA `5be3a27c79d3574dc71bab358a6b69fd8727ad3d2b8125c63fd6b53bb7ebd2c0`，主程10f979机械收件通过。这是已存在的确定性红证据，不额外启动Unity重复复现；只改变TMPDIR验证同一编译入口，不加产品回归测试或调试代码。

复用 `TestArtifacts/FightMatch/HOST-SAVE-ISOLATION-001/M02/projection/` 和同M02的 `package-cache/`，不重建/复制投影或Library。M02固定998源（31749332B／canonical SHA `136fe1dff5295d9e37b8fec55cb9bbd915b9b06a2e22bb5ccfd3e2f6941b953c`）、已生成10meta的字节/GUID（M02/after.json）、51包及17预期名保持。组合来源、两PR已审head、Editor、阶段门及安全规则继承[固定M02包](engineering-host-resource-mac-002.md)与M02/activation.json（15465B/SHA `8a89e5e6fe60c65b301f19cae399e4ea290343eee74f9259dbe8ba10e4440ae7`）；只按下文覆盖根/临时目录/记录方式。零C#、断言、包图、场景或共享源码变更，不重新审查未变head。

M03唯一新证据根 `TestArtifacts/FightMatch/HOST-SAVE-ISOLATION-001/M03/` 起始ABSENT。允许新文件恰为 `preflight.json`、`activation.json`、`runner.py`、`process-events.jsonl`、`process-after.json`、`after.json`、`receipt.json`、`I/editor.log`、`I/launcher.log`、`I/result.json`、`T/editor.log`、`T/launcher.log`、`T/results.xml`、`T/result.json`、`T/discovered.json`；条件未运行的文件不补造。M02全部14证据叶（receipt所列13＋receipt本身）冻结，M02/tmp保持；M02/T不得出现。历史失败/activation/runner不重写。

仅C通过一次 `mktemp -d /private/tmp/fm-m03.XXXXXXXX` 创建本任务短临时根，实际canonical绝对路径UTF8≤40字节、根与全部父组件无symlink、初始空、owner为当前用户；只覆盖子进程TMPDIR，不更改系统环境。≤16MiB/512叶，任务之外/private/tmp不读不写不清理；测试可按原TearDown清自己的子树，失败残留留证。临时根最终即使非空也保留，报告真实路径/占用；不做宽泛删除。工程/cache/日志/其它证据继续外置盘。

## 同一执行回合完成，不重复准备大清单

1. 只读核上述输入、10meta、M02证据身份、共享1089/保护114/S/FIX04/旧LAYOUT投影及缓存；复用原before/inputs清单并记录引用hash及差异摘要，不复制MB字典。fresh ps证明无Unity/UPM/owned残留、投影锁不在；失败即停。新建短tmp与M03/preflight.json，记录实际thread/turn、root/权限/长度、证据原件、空间与检查结果。主程派发后即通知真实新turn；C不得冒认前一次turn。
2. 本包已预授权一次I→T。C把精确owner/turn、主程签发/本包hash、preflight hash、实际短TMPDIR、固定M02 projection/cwd/cache、两条完整argv和预算机械写入M03/activation.json，满足全部前检后直接继续，不等第二批准。argv只把原M02 activation的I/T日志/results目标改至M03；projectPath/cwd仍M02/projection，UPM_CACHE_ROOT仍M02/package-cache，TMPDIR改新短根；17 selector原样，I带-quit、T无-quit。严禁自主新增flags/放宽门。
3. 复用M02/runner.py的208行监督结构（SHA `92e99c5032cdeaac707b712a344fa479e3db066311f3b99de4b412f08dd3f62e`），只在新M03/runner.py适配证据根与复用投影/旧输入引用、实际owner/activation、短TMP资源/进程归属检查、10meta精确冻结及原M02证据守卫。≤300非空行，用apply_patch，记录实际hash及最小diff摘要；不重写监督器、不改变原失败/超时/测试判定，原M02 runner零改。短tmp可作owned进程cwd关联之一，但仍须精确PID/start/exe/argv、父子闭包匹配，不能按名称杀进程。
4. 一次compile≤360s→仅exit0/无compiler-domain错误/实际4个DLL及import完成/保护通过才一次17例≤180s。XML Counter必须等于原17全名各1次，17pass、fail/skip/inconclusive0，分别报6HISO/11contract。失败停止定位，0自动重试；不另起发现、Play/真实Host/G3/设备/全量或RES-A。

## 写入与收尾边界

除M03证据和短tmp，只有M02/projection既有Library/Temp/Logs/UserSettings/obj/生成csproj-sln及M02/package-cache允许Unity必要生成写入，合计生成≤4GiB、cache≤1GiB，不删旧Library/cache。1008源/meta全部保持；唯一允许额外ProjectSettings仍为原合同精确3534B默认SceneTemplateSettings.json。每日志≤8MiB，M03证据≤32MiB，可用空间≥2GiB；无下载/安装/CLI更新/许可配置。已有CLI检查失败不重试，本包固定Editor直启，CLI默认2s后SIGKILL不适用。

超时停止新工作→自然退出60s→重新匹配owned身份后单次SIGTERM→30s；不SIGKILL/全局kill。工具拒绝保留并停止，不换目录/owner绕过。末尾核源/meta/共享/历史证据、空间、M02旧14叶和M02/tmp不变、owned完整闭包与实际信号；receipt绑定两次尝试关系但只报告本轮实际I/T。不存在的PID/exit/XML用null/NOT_RUN。C不自给integration ACCEPT；主程收结果交中央/QA。RES-A等本纠正正式收件后另包接续。
