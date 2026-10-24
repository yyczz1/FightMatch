# RES-D-ACTIVATION-001：已消费 restart 与下一轮 prepared

2026-10-03 · `APPROVED_CLARIFICATION`；仅设计澄清，不是独立code review或SYS派发source/native。依据用户AGENTS §6；唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a1005f-28da-7cd2-806e-a3106e5fe1db`；SYS `01a0f2e6-29bb-7092-abf0-705b41b7bf93/local/01a10061-7366-7d82-8b77-11f27c06b64b`（thread/host/turn）。
中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local/01a0fd75-6e53-7b81-a7ce-9010190ae130` 已明确批准四字段精确匹配的consumed分支、AS02第二轮/畸形负例及新FIX01/S同八叶；主程转达并已核其原turn批准消息。主程核本增补符合此范围即可另激活，无需再次等待同方向确认。
只读固定[原合同](engineering-res-d-activation-store-source-001.md)：11363B/SHA256 `95c55bfff61e3b49f49ea86411ed8bd06f4311ac3305f9d6e83fb63ac284aa4e`；原作者turn `01a10057-9fbf-7693-b543-201df3ab1b9b` 已停在BLOCKED，无候选源码。
原根`TestArtifacts/FightMatch/RES-D-ACTIVATION-001/S/`两叶只读：`source-receipt.json` 4172B/`14406a99b39f74bf7d25e583482f8988a9c3a98f7bc4f886d46f370cb691a3dd`；`before.json` 5045B/`59c59cc9cf3218b02d42a39a54e7fb75745ba9d7b3e02a02958528ba83fd6074`。本文核符号关系，不把BLOCKED改写为SOURCE_READY。
结论：作者反例成立；建议可用，须将原第13行“base绑定当前active／prepared与restart相等”明确限于**下一轮未消费请求**，不能限制已消费历史。以下谓词优先解释这一局部，其余原合同保持。

- 记A/P/R分别为active/prepared/restart；`V(X)`先验原七字段/schema/预算及该文件允许的phase，processToken也须合法；更新记录base非空且不等于自身id，空base仅初始active。`K(X)=(activationId,baseActivationId,resourceSetId,descriptorSha256)`，四项按ordinal逐字比较；不比较整record，phase/token不在K中。
- `Consumed(X,A) := X与A存在且V均成立且K(X)=K(A)`；A可为CodeEntered或BusinessReady，表示目标已由active接收，不宣称业务就绪。这里X.base匹配**A.base**，不是A.id；不以仅id相同、当前token相同或“旧时间”认定已消费。
- `Next(P,A) := P与A存在且V均成立，P.id≠A.id、P.base=A.id、P.set≠A.set`。A若CodeEntered，此关系可只读保留，不能据此Prepare/RequestRestart；变更动作仍要求A.BusinessReady。SHA为有效精确字段，未消费配对还须K相等。
- 有A时：P只可缺席、Consumed(P,A)或Next(P,A)。R只可缺席、Consumed(R,A)，或`A.BusinessReady且Next(P,A)且K(R)=K(P)`；最后一种才是未消费restart，必需完整P，不可换prepared。有P/R而无A、任一现存记录不合法或不落入这些关系，拒绝且不写；合法格式关系错用RES_STATE，格式错误仍用原码。
- 因而Consumed(R,A)不要求匹配当前P；它可以与下一轮Next(P,A)并存。Prepare只替换P，保留R；RequestRestart(该Next的id)再单文件原子替换这个consumed R。未消费R仍强制匹配P；不删除历史、不新增清理步骤/双文件事务，也不从bak补配对。
- 轨迹（A/B/C表示互异id及set/SHA，括号为base）：A Ready→P=B(A)、R=B(A)→新进程active=B(A) CodeEntered/Ready，P/R均Consumed；Prepare C后P=C(B)、R仍B(A) Consumed，**合法**；RequestRestart C后P/R=C(B)未消费；新进程进入C后两者Consumed。该规则可重复，不限一次更新。
- 幂等只同阶段：Prepare同一Next set/SHA保留其id；RequestRestart同一未消费id保留原token/bytes，不伪造新请求。RequestRestart(consumed.id)拒绝RES_STATE、不再触发旧更新；其它幂等与MarkReady门沿原合同。已消费不授权跨阶段回退。
- 普通同集复开保留active的id/base/set/SHA，仅phase/token按原流程变化；Consumed分类不变，已存在Next prepared可保留，待Ready才能请求它。未消费restart存在则仍走原精确更新/下一进程门，不以普通复开绕过；CodeEntered故障仍只许同目标重进。
- “畸形consumed”：仅id同而base/set/SHA任一不同、空/自环base、非法phase/token/schema均不得豁免；缺P/错P的未消费R、P错误base、旧R既不匹配active也不匹配Next P均拒绝、原byte保持。合法phase/token不同本身不破坏Consumed；Enter/MarkReady的实际processToken检查并未放宽。

在原`FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS02_UpdateRequiresNextProcess`内加入有界矩阵：完整A→B→C各落盘点Read/重建可达；consumed R＋Next P；重复Prepare/RequestRestart不改id/token；同集复开保留关系；逐项变异id/base/set/SHA、phase/token/schema、缺/错P及未消费时改P，验证拒绝/零推进/原byte不变。其余七个普通fullname保持，仍共8项；fake进程不冒充真实重启，原AS05/06证据要求不减。
不扩大public接口、七字段、九状态叶、两源码/650生产＋550测试行/各48KiB；既有promotion/UnknownCommit、Host首建缺口、PlayerSave与旧资源保护不变。累计预算沿原before中已获批2447/1921，非历史合同2438/1885；本澄清不另增额度或执行权。
后续由主程核范围并另激活新`TestArtifacts/FightMatch/RES-D-ACTIVATION-001/FIX01/S/`同八叶source，before同时绑定原合同、本文及旧BLOCKED两叶；旧S永久只读。SYS本轮不建新S、不派作者/C，不改源码/旧文/证据，不运行产品/Unity/测试/Git/审查，不等中央或QA。
