# RES-COMBINED-V03：仅更新缓存前像与独立运行绑定

2026-10-08。机械任务，交原源码会话01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local，Luna/low，中央唯一收件。用户已授权继续开发及限定验证；本包不启动native。不要重做设计、修功能或重跑177回放。

V02 C actual01a118e2-2dca-7210-a2ef-99d25f579553 wait233 completed，预检NOT_RUN_BLOCKED：TundraBuildState.state仍封V01之前hash。I/T、同步、43移交、信号均0；runner exit1，完整8.380329秒，含准备168.380329。receipt3107B/SHA8dfd0e9e4cb400104466bd97319b3edee2108780cde11dc7c9a30b54ab0abdef。原现场只读保留，不追认成功或覆盖。

中央当前只读核两个明确允许保留的状态文件：TundraBuildState.state136080B/SHA4008107d99c4ffeb29b12acb7eb49269c43c5014a261d98561ed47e1ca851066，mtime2026-10-07T23:42:10.059994Z位于V01唯一I执行窗口；bee_backend.info127B/SHAcd510c4af398ab352e5bd1fdb74a06658814c4e8da952cb8c068909ac45e87d1未变。观测在/tmp/resource-combined-v03-cache-state-observation.json。这两文件原方案保留原位且不回滚，不能继续用I执行之前的观测。更新expected记录，不修改缓存实物、不清空缓存；不据此接受其它漂移。

仅新建TestArtifacts/FightMatch/RES-COMBINED-V03/run下同五叶runner.py、replay-check.py、replay-results.json、inputs.json、preparation.json。全部V01/V02、产品、P/K、TMP/BC/AS及其它文档只读。基线V02/source已中央ACCEPT，PR20 head281bb731ed08a21a25df67932068279853b3fd09，五seals在resource-combined-v02-source-verdict.json。

1. inputs只将当前task及当前执行目的地V02→V03（E/BC/AS、commands、newRootPreconditions及transfer目的地），再更新compilePlan.preserveInPlaceIdentities中Tundra这一SHA。保留所有历史fixedReferences、来源、产品/包/43移交叶和89用例身份，不全局改历史路径。当前owner/日期记录本次机械绑定；保留原观测为baseline，不伪称重新扫描全工程。
2. runner只变当前V02任务/独立路径标签→V03、fm-rcv2→fm-rcv3及新inputs bytes/SHA硬绑定。**除此之外逐字节相同**；normalize以上替换后必须与V02全文相等，所有逻辑、env、分类、守卫、期限保持。出现其它必要修改则BLOCKED回报，不猜测。
3. replay-check.py与replay-results.json逐字节复制V02，明确它们是**复用的V02回放证据**，不是V03新运行。不要执行该历史checker。preparation根据新runner/input身份及本包重封，明确validation是binding-refresh静态核验＋旧177/177回放复用，native0；沿runner现有SOURCE_REPLAY_PASS门枚举但附明确reused而非新跑的事实，不能虚报新的177运行。source owner为本次actual，更新activationContract为V03和新seals，保存准确两源码基线差量/数据字段清单及复用证据对应V02身份。
4. 只运行一次短机械核验：五文件合法JSON/Python语法（ast.parse无pycache）、原V02五seals未变、上述runner归一后全文相同、数据按明确字段差异可重建、受保护input集合不变（compilePlan只准一个state SHA）、历史checker/results逐字节相等、无额外文件。最多10秒。准备沿160总限，记录本次实际机械时间；不重跑离线、Unity、ps、信号、缓存扫描、网络/Git，不创建activation。

回执只给五seals、机械检查结果、完整差异位置与实际身份即可，每条输出≤2KB。不新写长叙述或额外测试。中央将更新源码审查head（绑定常量变化也交GitHub）后，另签V03的首次限定native。旧两轮运行及所有未完成门原样保留。

## 机械定位澄清

首次Luna回合01a118e7-0dea-75a3-8b25-5791b967977e没有写V03、没有执行核验/回放，却错误地在runner里寻找Tundra内容hash后报告input缺旧值。中央重新核实V02 input仍是1241674B/SHA5042438e26cd9e7fea1c5decee0f689f30af05ee667c25dbec57c472d363e636，目标字段旧SHA198abf41ca51e5b002c3180e95cc14e9710e0c4919b921a2bf1447c650a763b4确实存在于inputs.compilePlan.preserveInPlaceIdentities['Library/Bee/TundraBuildState.state']。**runner中的硬绑定是整份inputs的504243…，不是该状态文件的198abf…。** 先更新数据并序列化求出新input整体bytes/SHA，再替换runner中1241674及504243…这一整体身份。只在输入JSON里替换Tundra内容hash。此澄清不增加范围或核验次数；尚未使用的一次机械核验继续有效。
