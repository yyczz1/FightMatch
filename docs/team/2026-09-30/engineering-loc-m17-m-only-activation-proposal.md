# LOC M17：M-only 中央激活提案

2026-10-04 · **PREPARED / NOT_EXECUTABLE**。本次仅只读准备和本提案落盘；没有制作pin/启动器、下载、SDK全量复核、材料写入或restore。主程actual `01a106dc-0140-7783-9ec7-aab4235c4800`。中央请求复用既有模型补真实执行缺槽；本提案不授权执行，也不替代GitHub代码门。

## 1. 当前固定候选和阻止签发的条件

发布回执 `loc-m17-fix01-publication-receipt.json`：PR2 head `837dd4e39683548ee23dce5f5228074ec87231e5` / tree `3bda3a82142cbefef2e83b6ea5bb1d637239a859`，review trigger5979838858，QA唯一接服务结果回中央。主程不重复等待/审查。

E=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix01`：driver114268B/`2198eeae7616b31821c77453507886ec5640f7432225d76fcf0c794096121a2f`；source-freeze2806B/`8c8e50f8af094918bdac323ab4080c942616e646d4c4a56a12eaf6a8b74cc2d1`；prepare5719389B/`c426579fbe09d4754871558eb9342a14272b193887396f2474679864bff771a3`；model `3155e40832ce36c7b1a57738cc9a65f221efd79f3696e65fe996350cd617378a`；write-set1428902B/`ab2266fd0f3ab30943c64d1be15b36b800edd7e5db4370d9fb25064d1602afbf`。以上是观察基线，**不能直接签成已批准执行版本**。

实际入口读取发现：driver1050的计时对象`stage=writer.stage`被1058写域循环`for root,rel,kind,stage,...`覆盖成`"M"`，1066随后执行`stage.clock()`。这是实际首条命令的阻碍，且前面可能已创建目录；不能用真实M启动验证它。已交中央与其唯一QA收件，不另建本地review。需原作者最小修正及对应新head/封签与独立代码结论；旧73、失败及封签保留。下文机制/配额不因该修正扩大，签发时机械替换**整组**最终E/driver/seal/model/head身份，不能混用旧seal和新driver。

另外当前CLI会因`TRUSTED_CENTRAL_ACTIVATION=None`在任何外部动作前拒绝；不存在可直接传入的`--pin`、stdin或环境授权入口。不能把`python m17_driver.py execute`写成已经可运行的命令。

## 2. 一次签发的信任链（提议，尚未创建）

保留driver的磁盘字节、SOURCE_ONLY封签和`productionTrustPin:null`。使用现有`execute(argv)`这个seam，中央独占提供很小的启动胶水，不复制执行器或新建通用框架：

1. 原LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`作为唯一M执行者（不是Unity C），先取得其新的**实际**待执行turn。没有真实turn不得造UUID。主程唯一收件，中央接主程；模型Astra/xhigh。
2. 中央在代码门真实通过后一次写 `docs/team/2026-09-30/central-loc-m17-m-activation.json`（A）。记录准确源/seal/model/最终reviewed head、该真实owner、M范围、下方argv/env/预算和原始review证据；没有完成结论不能填`COMPLETED_NO_UNRESOLVED_FINDINGS`。
3. A字节确定后，中央独占写 `docs/team/2026-09-30/central-loc-m17-m-host.py`（H）。H内固定A的绝对路径/bytes/SHA及central/owner七身份；先核最终driver的固定bytes/SHA、禁止链接/替换，从这些已核字节创建正常Python模块（`__file__`为原driver、`__name__`非`__main__`），只把**中央真实**pin赋给该模块的`TRUSTED_CENTRAL_ACTIVATION`，立即调用未改的`module.execute(sys.orig_argv)`。不改reader/模型/谓词/计时/子进程函数，不运行`prepare`，不从执行者的stdin、CLI、env或可选路径读取pin。磁盘driver仍为空pin；直接CLI仍拒绝。这是待明确批准的中央信任初始化，不是测试fixture解除生产门。
4. 中央可信派发消息同时绑定A和H的最终bytes/SHA、真实owner turn及准确命令；执行者只核并执行这些固定字节。A里的`hostExecutorSha256`按现接口必须等于driver SHA，**不拿它冒充H的SHA**；H身份单独由可信派发固定。顺序为`已审源/seal → A → H → 真实中央派发`，A不回指H摘要，seal也不回指A/H，无自指hash循环。

H是尚未存在的执行胶水，不声称PR2当前代码审查包含H。中央必须明确批准这个有限seam及最终启动字节；若中央不批准内存pin初始化，当前原入口就保持不可执行，需另行最小实现与独立review，不能由LOC自行塞入pin或跳过门。该前置也不能靠把文件名叫central来满足。

## 3. 准确A字段与启动命令

现`verify_activation`真实消费字段：`centralThread/centralHost/centralTurn/centralItem`、`ownerThread/ownerHost/ownerTurn`、`scope:"EXECUTE_M17"`、`stage:"M"`、`argv`、`sourceFreezeSha256`、`modelSha256`、`driverSha256`、`hostExecutorSha256`、`refinedWriteSetSha256`、`reviewStatus`、`reviewedHead`、`executionHead`、`budgets`。`budgets`必须逐值复制最终prepare.executionBudget全对象；其中B/L仅保留声明，stage校验与入口拒绝它们，不构成授权。额外记录原始GitHub结果/QA receipt、cwd/env、人类权源及此次派发身份；pin另持A的path/bytes/SHA和相同七身份，A不放自己的摘要。

设R=`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`，PY=`/opt/homebrew/Cellar/python@3.13/3.13.7/Frameworks/Python.framework/Versions/3.13/Resources/Python.app/Contents/MacOS/Python`。实际argv固定展开为：

```text
[PY, "-I", "-B", R+"/docs/team/2026-09-30/central-loc-m17-m-host.py", "execute"]
```

cwd=R；父环境不覆盖HOME/home/CODEX_HOME，`-I/-B`用于隔离Python配置并禁pycache。只读36d698已确认此二进制Python3.13.7，`sys.orig_argv`保留实际上述PY，而`sys.executable`会返回`/opt/homebrew/opt/python@3.13/bin/python3.13`；所以H传**orig_argv**，不要重组出不同的argv再声称等于实际启动。PY51584B/SHA `118e0584d0d61952b3039768169ff6731db8b5ade373c53de65c07bbd3877360`，签发前按此固定身份核（不升级Python）。

21个M子命令的argv/cwd/env只从最终prepare.model.commands筛`stage=="M"`逐项取，禁止手抄/另生参数：18 GET（curl单次125秒，内部max-time120/connect15/retry0/不重定向）＋三次restore各180秒。restore顺序是runner-generation、tests-generation、tests-verification；前两LockedMode=false、后者true，完整本地feed，网络由既定OS隔离禁止。所有子命令PATH前置固定SDK；六个dotnet/NuGet/TMP目录与DOTNET/MSBuild标志完全沿各command.env，HOME/home/CODEX_HOME保持。没有build/publish/official tests/oracle/B/L或Unity动作。

## 4. 输入、写域与真实总限时

- M材料根：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17`；W工作根：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17`。本轮5be18f只读存在性均ABSENT。实际启动前再次检查，M须缺席；本次提案也要求W缺席，任何实际残留先报，不删除/复用失败环境。仅最终已封write-set中M/对应writer规则及`receipts/material-validation.json`，不开放B/L写者；共享/历史/工具安装根零写。
- SDK=`.../Luban/5.1.0/dotnet-x64`，版本8.0.425。模型现有4707角色行对应4704唯一叶590543782B；重复三行是许可/tool角色，不是少核三个文件。dotnet本体111744B/`e307c181562634fd59416032f56d506f85de5b405604dae4a20e2117e8709686`。**没有在本次准备中全量hash**，也不运行SDK做计时试跑。
- 实际`execute`首先创建MStage，随后source消费/model重建，再在execute_material对5895条existing输入逐bytes/SHA复核，包括所有SDK叶；读函数拒绝文件/祖先symlink。全部在同一monotonic870秒工作截止及signal看门狗之内、首个GET/restore之前，不通过旧source静态结论跳过fresh门。若SDK读取/核验耗时过长，当次即失败，不补第二轮、不给下载阶段重新计时。590MB本身不是完成耗时保证。记录实际开始/结束与核验完成门，现有循环只证明固定清单所有叶；不将其夸大为对未声明SDK额外叶的全树证明。
- M总900秒=工作870＋收尾最多30。计时口径是**真实execute入口**至其收尾结束；H固定字节校验/模块装载属于启动准备，单独记墙钟，不能把任意材料工作搬到H逃开计时。剩余额度裁剪每条命令，工作失败后禁止下一命令；原计时23340仅是M/B/L清单总和，不是一次授权。收尾沿已审生产器，只处理本次实际子进程、保留真实退出/超时和输出，不能延期补成功。
- 固定18归档共25172764B，集合SHA `6761184244450758f97f5886582a2bd2ae9de2108b4d71b921943460efae215d`。只准精确官方URL/期望hash，缓存只能减少实际GET，不能换包/重试；curl响应体计量缺失即失败，部分下载仅报告下界，TLS/协议总流量UNKNOWN。M≤512MiB/25000叶，W≤4GiB/200000叶，free≥2GiB，最大并发树1。

## 5. 外层证据与一次收件

拟Erun=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m01`，本次未创建。需要中央额外列明四个外层证据叶`before.json`、`stdout.log`、`stderr.log`、`receipt.json`（总≤16MiB、各日志≤4MiB），仅承载可信启动/退出/原始stdout、身份和失败回执；它们不进入旧SOURCE封签、不写入W以免破坏入口的W缺席检查。原M命令日志/receipt及材料manifest由既有精确write-set产生，旧八叶/三十历史叶只读。

成功只称`M_MATERIAL_CANDIDATE_NOT_ACCEPTED`：实际18固定包来源（含缓存）、三restore返回和独立locks/assets比较、31闭包/完整许可证与部署清单、精确材料manifest、源/授权绑定、所有进程收尾与写域/资源门到齐；B/L仍NOT_RUN。主程机械收件后交中央指定独立材料接收，作者不签ACCEPT。失败保留partial根/日志，不自动删/重试或续B；全局期限可能早于磁盘报告完成，外层stdout/真实退出仍是证据，不补造成功receipt。

**签发缺槽汇总：** stage覆盖的小修对应最终source/head及QA代码结果；中央是否批准上述有限可信H；新M执行actual turn；真实中央签发turn/item；A/H最终字节和可信派发；执行端明确允许上述外置M/W精确写域与18公开GET。当前均不伪填或先执行。其余命令/版本/范围/配额全部复用，N04审批阻塞与本提案无耦合。
