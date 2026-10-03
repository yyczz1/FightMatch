# RES-D-ACTIVATION-001：独立技术状态源码合同

2026-10-03 · `PREPARED / CENTRAL_SCOPE_APPROVAL_REQUIRED`。授权：用户AGENTS §6／首Demo恢复、中央批准两文件设计；本轮不授权源码。SYS `01a0f2e6-29bb-7092-abf0-705b41b7bf93/local/01a1002e-a7ac-7c51-af7f-79c89d128645`；唯一回主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a1002c-2bd8-7f22-8031-e0cf6401ac97`（thread/host/turn），后续作者/激活turn由主程另绑，Astra/xhigh。
固定依据：[CD §3–4](engineering-resource-runtime-bridge-cd-001.md) SHA `ea3fc6efecd6ea118675cfcf96499cb5c3192bb6af2371d7ca9637cb7bdf9378`；[H](engineering-runtime-localization-integration-001.md) `8c0e793f52d8fce9aa39f2e281507e3408613484f8d09d6938cc48a1502ce56a`；[HSC:19–30](engineering-host-settings-first-create-001.md) `90b7351a13e16aa1d04281c42a27e003815b093e33adfcca9173578aaaba027d`；HISO封存`TestArtifacts/FightMatch/HOST-SAVE-ISOLATION-001/S/source/Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs` `f527b4ceecffff0718ed1214f9d26eaca5d65f6e2c82ba973d6b8774c2e66b7f`。参照已有FileStream.Flush(true)/File.Replace/File.Move及根属性守卫，不调用/修改PREF存储或Mac专用Host探针。
未来仅新`Assets/Scripts/FightMatch/Host/FightMatchResourceActivationStore.cs`（≤650物理行/48KiB）及`Assets/Tests/EditMode/FightMatchHost/FightMatchResourceActivationStoreTests.cs`（≤550行/48KiB）；本文/两源码本轮已核缺席。无新public API/依赖/friend，Host/Host.Tests asmdef保持，复用既有Host.Tests friend；类型/helper均留本文件，不抽通用状态机。meta自然生成另签。
仅resource技术状态，不加载/删除资源、不验真实boot/LOC/业务、不创建Session或触碰PlayerSave/settings。HSC只放行settings，resource-state未进EmptyRoot白名单；本包不接Host、不改首建判定，不宣称H兼容或实际可启动。

## 1. 固定interface与提交语义

- namespace `FightMatch.Host`；internal sealed `FightMatchResourceActivationStore(string productRoot)`；同步internal方法`Read()`、`EnterAtProcessStart(string exactSet,string descriptorSha256)`、`MarkBusinessReady(string activationId)`、`Prepare(string exactSet,string descriptorSha256)`、`RequestRestart(string activationId)`均返internal immutable `ActivationResult`（Outcome=`Confirmed/Rejected/UnknownCommit`、Snapshot、SafeCode）。Snapshot为三记录的只读DTO（缺席=null），Unknown仅带本实例最后确认快照而非磁盘现态；失败不给新阶段，不抛原始IO异常。调用方须先过对应可信锚/内层/Host门，成功持久化不证明这些门已过。
- root为调用方明确提供、已存在的绝对canonical产品根；无默认persistentDataPath、当前目录或自动找根。仅创建`resource-state/v1`并拥有`active.json/prepared.json/restart.json`及逐个附加`.tmp/.bak`共9叶；拒filesystem根、`.`/`..`、路径别名、链接/ReparsePoint（含悬空）、目录冒充文件和未知叶；每次IO重核祖先/叶，检查失败拒绝而非按不存在处理。实例串行，调用方独占该root；不新增锁文件/跨进程协调，不承诺对抗并发目录替换。
- 各记录v1恰7字段，固定顺序UTF8无BOM/空白：`activationId,baseActivationId,descriptorSha256,phase,processToken,resourceSetId,schemaVersion`；version整数1，其余string。activationId=32小写hex；baseActivationId为空（初始）或32hex；SHA=64小写hex；set=`[a-z0-9][a-z0-9._-]{0,127}`且非latest；processToken=`p<正PID>-<正启动UTC ticks>`（两数字分别≤10/19位）。phase仅下行闭集；不接受重复/缺/未知键、转义、尾随或不合法值，bounded专用读写器，不复用业务codec/JsonUtility宽松反序列化。
- `active`只含CodeEntered/BusinessReady；`prepared`只含Prepared；`restart`只含RequestRestart。成功记录对应exact set/SHA与activationId；prepared/restart还绑定当前active.activationId为base，二者目标/id/base须逐字一致。已被active同id接收的prepared/restart为已消费记录，保留但不能再次触发；不匹配残留拒绝，不任取“最新”。每步最多提升一个目标文件，不宣称三文件事务。
- 初始：三主文件均缺且无残留时，Enter在调用方已验内置锚后直接写active.CodeEntered（新id/空base）；无Prepared/RequestRestart。无待重启更新时，已有BusinessReady普通启动只可同set/SHA重新CodeEntered、保留id/base。MarkBusinessReady只认当前进程CodeEntered同id；当前token/id已BusinessReady可幂等确认，不任意回退/换集。
- 更新：当前active.BusinessReady→Prepare新exact set/SHA（新id/base=active.id，禁同set换hash）→RequestRestart同prepared id（写restart）→**下一实际进程**Enter精确目标写active.CodeEntered→MarkBusinessReady。同阶段同意图重试幂等，不跨阶段倒退；未消费restart存在时不得换prepared。active.CodeEntered禁止Prepare/切另一集；失败只能诊断并退出重启，下一进程仅可同目标重新Enter，不自动用bak回旧代码。
- 生产processToken取真实`Process.GetCurrentProcess().Id/StartTime.ToUniversalTime().Ticks`，同进程重新构造store不能绕过RequestRestart；读取失败即拒绝。internal测试构造器可注入文件seam和固定token，注入值不作真实重启证据；实际Android进程门后置。Enter更新要求restart.token≠本进程；MarkReady要求active.token=本进程。
- 每叶≤2048B、最多9叶/总≤18432B；先FileStream.Length验界再精确读＋额外1字节EOF核验，拒短读终止/增长/坏UTF8。候选编码也先验界；固定schema字符串可预界定，不写通用JSON框架。读主文件不修改任何叶，tmp/bak不自动提升；遗留tmp返回`RES_STATE`待另行处置，不以残留创建初始集。
- 写：校验前态→同目录固定tmp用CreateNew→有界write→Flush(true)→close→复读tmp逐byte验证→目标存在用File.Replace、缺席用File.Move→复开目标逐byte及身份核对，全部成功才Confirmed。进入CodeEntered时备份既有BusinessReady到active.json.bak；CodeEntered→BusinessReady/同目标重进用null backup保留该旧备份，其它replace仅用对应bak；不Delete旧目标再Move、不降级覆盖、不以读回相似字段当byte一致。
- promotion前故障Rejected且主文件不变；promotion调用已开始但返回/后验不确定，一律UnknownCommit、实例禁止后续写、不推进内存阶段、不自动回滚或删bak。只可清理本次确定未promotion且自行创建的tmp；既有残留/未知提交证据保留。新进程先Read并核主文件关系/目标，坏状态只诊断；bak从不授权回退PlayerSave。代码不清除任何旧资源集合。
- SafeCode闭集`RES_ROOT/RES_SCHEMA/RES_STATE/RES_STORAGE/RES_COMMIT_UNKNOWN`，Confirmed无code；无路径/异常/输入回显。文件seam限本模块属性枚举/有界读写/flush/原子promotion，生产System.IO与测试内存故障实现共用状态/提交逻辑。File.Replace不支持即受控失败，无非原子fallback；Flush(true)不冒称目录fsync或断电一致性。

## 2. 精确验收与源阶段

以下8个普通非参数化NUnit `[Test]`，无新增旧测试依赖；fullname逐字固定，故障矩阵在方法内有界循环，8是计划不是discovery/pass：
- `FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS01_InitialBuiltInAndOrdinaryReentry`：初始直接CodeEntered、同进程Ready、同集后续启动，无伪造Prepared/Restart；错id/set/SHA零推进。
- `FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS02_UpdateRequiresNextProcess`：完整更新链；同token新store拒绝、不同token成功；跨集/base错配/过期请求/未Ready准备拒绝，重复操作幂等；fake token不证明真实重启。
- `FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS03_StrictBoundedRecords`：2048边界、超限/短EOF/增长、坏UTF8、重复/缺/未知键/阶段/版本/hash/id及尾随；所有拒绝保持原byte，残留不自动提升。
- `FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS04_PrePromotionFailuresPreserveActive`：内存seam在目录检查、创建/写/flush/close/tmp复读前后注入；主文件与旧完整集合哨兵不变，零Confirmed新阶段。
- `FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS05_UnknownCommitNeverAdvances`：promotion已调用但尚未改文件即抛/已改后抛、目标复开/读回/不匹配故障；Unknown阻写、不自动回滚，bak保留；重建读取实际旧/新完整状态，不猜结果。
- `FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS06_RealFilesMoveReplaceAndReopen`：真实隔离FS走首次Move、已有Replace/Flush、全链及新实例复开；核9叶闭包/精确bytes/bak，缺不支持平台不得mock替代或跳过记pass；不证明杀进程/断电原子性。
- `FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS07_RootAndResidueGuards`：相对/父路径/别名、链接/悬空/ReparsePoint、未知叶/目录冒充及枚举异常，读写拒绝且外部哨兵不变；真实FS具备能力则实证，平台能力不足单列BLOCKED不冒称通过。
- `FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS08_PlayerAndOldSetRemainUntouched`：真实FS预置profiles/settings/旧资源哨兵，仅9叶变化；CodeEntered失败无切集/回滚存档；结果不含路径，Host/EmptyRoot/asmdef/public面未变。

源S固定`TestArtifacts/FightMatch/RES-D-ACTIVATION-001/S/`，仅8叶：`source/`下上述两完整源码＋`before.json/after.json/source.patch/verify.py/static-checks.json/source-receipt.json`，≤1MiB。before/after绑定本文/固定输入SHA、两目标及meta缺席、共享Host/Session/HostAssemblyInfo/两asmdef原字节；不复制基线/SDK、不读活投影/大历史树，不写共享Assets或旧证据。
作者仅apply_patch及`python3 -B TestArtifacts/FightMatch/RES-D-ACTIVATION-001/S/verify.py`（≤30秒、最多2次留失败），验证8叶闭包/hash/两路径patch重放/行数与禁止变更；不编译/运行NUnit/Unity/SDK/联网/Git。本轮未建S。源码回执`SOURCE_READY/UNCOMPILED/UNTESTED/REVIEW_PENDING`，owner/实际turn及每叶SHA/bytes，仅回主程；PREF DIAG01作者本消息不转派。
后续主程/主测试另签唯一C单次compile＋AS01–08（dots优先）；真实FS仅新`TestArtifacts/FightMatch/RES-D-ACTIVATION-001/<activation>/state-tests/<case>/product`，各case建/清理限自身≤9状态叶及明列哨兵，进程与读写证据绑定。Mac/Windows/Android File.Replace可用性、实际重启/停流/Host首建兼容另门；无授权不运行，不为本包重跑RAW/B/C/codec或全量。GitHub PR Code Review绑定实际head后单独收代码门。
中央须批准两源码/新S及新增预算：据本轮主程基数D已1788生产/1335测试，本包另加≤650/550行后上界2438/1885，明确超原1600/1400及不能借RAW余量；这不是完整D剩余工作的预算。无既定阶段/存档语义改动；H的resource-state首建准入与跨平台实际原子证据仍缺，不能借本包顺带放行。回滚仅撤本包两候选/已激活增量，不动旧状态、资源、存档；签范围前不得派source。
