# HOST-SAVE-ISOLATION-DESIGN-001：真实Host验收存档隔离

2026-10-02 · `PREPARED_NOT_AUTHORIZED`；仅设计完整性回执，不是源码、运行时或Demo ACCEPT。
SYS `01a0f2e6-29bb-7092-abf0-705b41b7bf93/local`，实际turn `01a0fcd4-f58d-7113-93cb-c2a204739c69`。唯一上游/返程：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，派发turn `01a0fccb-bfa4-7fc1-a81b-f8a8a071cdbf`；依据用户继续first Demo、AGENTS §6 standing workflow及中央经主程转交的本次限定设计授权。实际实施、主程/中央G3批准和执行另签；不向C/QA/R派发。

## 1. 去重、固定输入与现状

- 已在`docs/team/2026-09-30`及相关system-design包中按Host/save/isolation/隔离根检索；找到下列G3、RUNTIME-LOC及RES-CD §85的“另包”定位，未找到等价完整隔离包，故仅新建本文，不重写它们。
- [真实复开准备](testing-ugui-native-reopen-001.md)：11,373B，SHA256 `1bf1811ae990a32134829e4b44b576b15c9bcfa3be90e1be3473539471fe2601`；采用§1 G3、§2.4及§3 N安全条件，不把历史布局路径/119文字计数当新布局验收。
- [真实LOC接入](engineering-runtime-localization-integration-001.md)：7,843B，SHA256 `8c0e793f52d8fce9aa39f2e281507e3408613484f8d09d6938cc48a1502ce56a`；采用H正式源顺序及末节存档另包边界。
- 实读Host：目前`Start()`空LOC源先停在LocalizationNotReady；通过内容校验后才调用`CanonicalSystemRoot`，其Mac分支会`Directory.CreateDirectory(persistentDataPath)`。之后真实Mac locator/save工厂指向`FightMatch/{locator,profiles}`。现成HostSession构造参数是rig接缝，不是实际Host.Start的隔离入口，不能以它替换原生启动。
- 独立性：可先做本包源码/根路径守卫验证，不需要LOC M16批准、Luban产物或正式LOC注入；**真实Host正常流程仍等待正式LOC/H等G3条件**，不能用测试source、CSV直读、反射回填或跳过IsReady解除等待。本包不处理M16审批事项。

## 2. 拟实施白名单与当前前像

以下只是后续包候选；本轮全部只读。实施前主程锁定Host单一写入者，与H接线交接，重核前像；不得覆盖H或布局WIP。

| 路径 | 拟动作／当前身份 |
| --- | --- |
| `Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs` | 修改唯一生产文件；6,595B；SHA256 `12dba7d11323da883f108ea416699a6cf638a9360587c984d7d070ebeb6cc287`；只加Editor验收根选择/租约生命周期及原工厂根参数选择。 |
| `Assets/Tests/EditMode/FightMatchHost/FightMatchHostSaveIsolationTests.cs` | 新增；当前ABSENT；仅下列6个非参数化根守卫测试。 |
| `Assets/Tests/EditMode/FightMatchHost/FightMatchHostSaveIsolationTests.cs.meta` | 当前ABSENT；仅后续获准Unity自然导入生成，不手写。 |

Host现有meta只读：243B／`0bb50b15f7d3db5120d36961098d6154044865e26b54ccc721e195b1dfa22952`。不新增生产文件、serialized字段、public符号、接口、asmdef引用或包；复用HostAssemblyInfo已授予的`FightMatch.Host.Tests` internal可见性。
只读语义输入：`FightMatchHostSession.cs` 18,579B／`892b9d494286f18e41e6f6bfbe474df591d9c06fa19644bec36332391cf50859`；`FileLocalePreferenceStore.cs` 13,773B／`d392d6808ad1fc8156482020951404eb46390d462ce0164404180dea067cce13`。后者已有`(string root, ILocalePreferenceFiles files)`入口；不改其实现/审批状态。
其它Host/View/Session/Platform/Core、现有测试、探针、资源/场景/meta、存档格式、Config/Generated/Tools、Packages/ProjectSettings、历史证据和共享索引均禁改。拟代码预算：Host新增＋删除≤350行，新测试≤400行；超限或额外路径先回主程，不拆文件规避。

## 3. 唯一根选择与防碰真实档

1. 仅`UNITY_EDITOR`编译的验收选择器识别成对的`-fmHostSaveIsolationId <32位小写hex>`、`-fmHostSaveIsolationCandidate <64位小写hex>`；无标记沿原生产分支，不提前调用CanonicalSystemRoot或新增真实根I/O。标记缺值/缺另一项/重复/非法值、同前缀未知flag、非OSXEditor均拒绝，不回退真实根；Android Player不编译验收开关，不改变包名/持久化路径。
2. 不接受任意save-root参数、环境变量、EditorPrefs或全局可变开关。从实际`Application.dataPath`的项目根派生唯一目录：`TestArtifacts/FightMatch/UGUI-01/native-scene-reopen-001/<id>/host-save/persistent`，记为P；业务根固定`P/FightMatch`，locator/profiles语义不变。不复制、导入或迁移真实玩家档，首次P必须由获准C预建为空目录。
3. `<id>`对应N激活根必须已有create-once `activation.json`及`host-save/owner.lock`；新增activation字段固定为`hostSaveIsolation={schemaVersion:1,activationId,canonicalProjectRoot,persistentRoot,candidateSha256,ownerThread,ownerTurn}`，id/候选hash须与argv匹配，根与实际项目匹配；C启动前将完整实际输入hash、已审head及owner派发绑定此记录，Host不把一个hash字符串当作已重算所有输入。记录/锁位于P外，不污染HostSession.EmptyRoot。隔离子目录与字段须由后续G3激活单显式纳入N白名单；本文不扩展旧N执行授权。
4. 先做纯字符串规范化与根关系检查，再读取已批准激活根内文件：绝对规范路径、固定目录段/id、不含`.`/`..`别名；逐级拒绝符号链接/重解析点/悬空链接/非目录，Mac realpath与预期相同。与真实`persistentDataPath`及其FightMatch根按路径段、保守忽略大小写验证双向不相交；真实路径不存在时只解析最近已存在祖先拼回余段，**不得创建它来求realpath**。无权检查、不同规范名或不能证明分离即拒绝。
5. 真实根只允许最小只读保护快照/元数据核验，不将正文、用户身份或档案复制到证据/PR。首次激活由执行者把真实根的存在状态、相对路径/类型/大小/hash留在本地受控证据；独立复开前后核零漂移。没有真实根就记录ABSENT，不创建占位目录；非本包并发写者存在时不开始验收，也不关闭对方进程。
6. 成功校验后，Host用`FileMode.Open, FileAccess.ReadWrite, FileShare.None`持有已存在且无链接的`owner.lock`，不向锁写正文；其它实例/进程占用立即拒绝。根与租约在任何Session/偏好存储构造前再次核验，不能在异步LOC等待后直接用过期校验。目录只由本包owner写；此防误触合同不声称抵御拥有同用户权限的恶意并发换链。
7. 新增同文件Editor-only `internal static string ResolveAcceptancePersistentRoot(string[] args, string projectRoot, string realPersistentRoot, RuntimePlatform platform)`：无标记返回null；有效标记只返回受验P；错误抛出受控异常；真实Start传实际Application.platform。`internal static IDisposable OpenAcceptanceStorageLease(string persistentRoot)`复核既定路径/激活记录后打开上条锁，返回现有FileStream作为IDisposable，不新增接口/工厂体系。Host保存私有只读期根/租约，在Start顶部处理标记；失败以现有ShowFailure链报告`HostSaveIsolationRejected`，零Session、零真实根fallback，不新增文案键。不手调Start/Bind或注入session/catalog。
8. 新增`internal string EffectivePersistentDataPath`只读属性，返回验收P或系统persistentDataPath；现有`SystemPersistentDataPath`继续表示真实系统值，`CanonicalProductRoot`表示实际选中根。原Session构造处在验收模式用已验P，生产仍调用原CanonicalSystemRoot，原工厂/SavePurpose/locator格式不变。根选定后不可热切换；OnDisable不释放活Session的租约，OnDestroy先Dispose Session，再在finally释放租约，含启动失败/取消路径。
9. H将来接线偏好时必须把同一EffectivePersistentDataPath传给现有带root的构造器，不得在隔离模式调用无参FileLocalePreferenceStore；本包不提前实施H。偏好仍在`P/FightMatch/settings`，**不另分假根掩盖生产行为**。实读EmptyRoot只容许locator/profiles：首次建档前已有settings会被拒绝，这是H另须闭合的行为，不在本包放宽、不删除settings绕过；若实际流程遇到即停止并回H负责人。

## 4. 最小精确验证与真实Host观测

源码交付先做白名单/前像/条件编译/无默认行为变化静态核验，再走实际新head的GitHub PR Code Review；不得把旧PR或作者自检当本包review。以下测试只验证根策略与租约，既有业务源/存档不替换；命名空间/fixture统一为`FightMatch.Host.Tests.FightMatchHostSaveIsolationTests`，精确方法为：

| 完整fullname后缀 | 必须断言 |
| --- | --- |
| `HISO_01_NoMarkerPreservesDefaultPathWithoutExtraIO` | 无标记返回null、默认分支不提前I/O；既有默认根选择/系统路径含义不变。 |
| `HISO_02_MalformedOrDuplicateMarkerNeverFallsBack` | 单方法内有限坏值表；缺值、重复、穿越、未知同前缀和非支持平台拒绝；不打开真实/候选存储。 |
| `HISO_03_LinkedNonCanonicalOrOverlappingRootIsRejected` | 用owner测试目录模拟真实根，覆盖双向重叠、大小写别名、祖先/叶链接与悬空链接；拒绝且零越界写，不触真实用户目录。 |
| `HISO_04_ForeignActivationOrConcurrentLeaseIsRejected` | 激活id/项目/候选/根不符及锁已持有均fail-closed；没有create-default重试。 |
| `HISO_05_PlayerAndPreferencePathsRemainUnderOneEffectiveRoot` | locator/profiles/settings均从同一P派生，保持FightMatch相对拓扑；无跨根无参偏好工厂；只测路径，不需LOC产物。 |
| `HISO_06_ReopenRetainsRootAndTeardownReleasesLease` | 同id/原owner根二次打开保持数据字节，不清空/换id；守卫异常/Dispose释放锁，未Dispose的owner仍排他。Host禁用/销毁接线另由静态核验和下述真实进程观测证明；测试本地标记不是PlayerSave通过证据。 |

只声明6个非参数化用例；实现后由主测试冻结发现的实际fullnames和转义全锚定selector。优先已验证dots支持的范围；涉及Mac realpath/锁的证明不能用Linux结果代替，另签C Mac一次定向合同，compile/import最多1次≤360s、6项最多1次≤180s，不追加旧222/234/full/布局FIX04。新meta自然导入不能顺带生成其它未授权meta；执行环境不具备则只阻塞该验证门。
真实Host观测不是上述6项的替代实现：须G1/G2/G3齐、正式LOC/H/内容与候选精确身份已接收、根守卫与新head review闭合、主程及中央批准测试存档范围。当前这些运行时条件未由本文建立，状态`WAITING_G3_ACTIVATION`，不依赖也不绕过M16审批来完成本设计。
第一次Host观察必须打开**实际Demo场景**，argv带本包标记，原生进入PlayMode；只读探针核`Session.ProductRoot == CanonicalProductRoot == P/FightMatch`及工厂实际文件落点，正式source/ReleaseSet、Host唯一、Session非null、绑定/双语非placeholder；用真实UI首次创建资料、进入导航及战斗，记录实际playerId/locator/已提交head与屏幕证据，不预造档、不直接调用Session业务方法，不提交攻击/结算/奖励。
退出PlayMode并自然关闭owned进程、确认Session/锁释放后，才用**独立新进程**、同一候选/id/P复开：真实Host自行ObserveStartup，保留同playerId及原已提交存档身份，按既有恢复语义呈现，不再次CreateProfile/写假成功。记录前后文件集合/hash、实际调用/提交计数及源/资源零漂移；仅换scene或重建rig不算玩家档重启证据。
G3原N的“保存场景进程S→复开进程O”只证明场景保存/复开，不自动证明玩家存档跨Host进程恢复；第二个Host观察进程属于本设计的拟增验收，须新激活单明确计数/argv，各Host观察最多1次≤300s，不能暗塞旧N预算或无因重跑引用检查。既定N探针仍单独拥有`FightMatchNativeSceneReopenProbe.cs`（当前ABSENT）；本包不写它、不另造探针/runner。
拟证据复用N激活根：只增加`host-save/`的owner.lock和persistent真实验收数据、`host-save/before.json/after.json/create-observation.json/reopen-observation.json/receipt.json`；日志/截图/进程记录沿N已有槽并分别绑定阶段。隔离数据≤16MiB/512文件且计入N总新证据≤96MiB；日志各≤8MiB、临时≤128MiB、空闲≥2GiB。首错/超时/越界/保护漂移即停，保留原证据；owned身份核验后沿60s＋单次SIGTERM＋30s，无重试、无全局kill。

## 5. 撤回、停止线与交付

- 撤回验收：停止并确认本包owned进程/租约释放，不再带验收标记；生产默认路径始终未变，无环境/PlayerSettings/场景改动需还原。隔离数据原地封存供收件，不自动删除，不向真实根复制合并；需要清理时只按另签的精确激活id清单处理，禁止清整个TestArtifacts或用户目录。
- 撤回源码：由原实施owner在未与H合并或已核准差分的情况下，仅撤回本包Host增量及新测试；保留期间H/其它WIP。无reset/checkout整文件、无自动删除自然meta。已生成候选/审查/测试证据保留，撤回不改写为通过。
- 根无法证明不相交、符号链接/租约异常、默认分支行为变动、新路径/API/依赖需求、H在隔离模式仍访问默认偏好根、settings/EmptyRoot冲突、正式LOC缺失、输入变动或任何真实根变化均停止受影响门回主程；不以mock或多跑一次补救。
- 接收标准：主程能据本文逐项签定唯一代码owner、当前前像、3路径范围、内部符号、根/lease激活记录、精确6例及G3真实观察的独立预算与return path；作者只返回设计准备完成，代码/test/native/device分别收件。本轮仅新增本文，codebase-design用于保持Host根选择这一小接缝；未改产品/布局WIP/索引，未运行Unity、测试、设备、依赖、云或Git操作。
