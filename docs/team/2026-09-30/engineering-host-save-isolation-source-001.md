# HOST-SAVE-ISOLATION-SOURCE-001

2026-10-03 · AUTHORIZED_SOURCE_ONLY；不是编译、测试、运行时或Demo ACCEPT。

## 1. 授权、唯一负责人和固定输入

用户经中央本轮明确要求“继续做能够实际开发的内容”，中央已限定实施已收设计并允许仓库内独立源码暂存区；AGENTS §6 standing workflow授权团队内限定派发。主程thread `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，当前turn `01a0fd7b-5ea7-72f0-ad67-3ba4d6441453` 是唯一实施收件者；中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 是唯一上游/发布者。实际实施交既有C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`，显式 `gpt-6-astra/xhigh`，在before中记录本次实际turn而不复用旧turn。
本包是独立源码工作，不是被拒FIX04 native或LOC M16动作的重试；无Unity、进程执行或旧投影同步授权。若本包派发/工具亦遭拒绝，保留原文停止该动作，不改执行者/路径规避，不重试被拒的回主程消息；主程直接wait/read及本地收件即可。

完整采用[已批准设计](engineering-host-save-isolation-001.md) §2–4：14056B，SHA256 `b40d64ae3f955ceca285031ef5bf3caf186fb3e313b012779b430fc4cb4b8f82`；设计内PREPARED状态由本包仅对源码解除，所有运行门仍关闭。不得新开设计轮或降低断言。
固定共享只读前像：
- `Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs`：6595B／`12dba7d11323da883f108ea416699a6cf638a9360587c984d7d070ebeb6cc287`。
- 上述Host.meta：243B／`0bb50b15f7d3db5120d36961098d6154044865e26b54ccc721e195b1dfa22952`。
- `Assets/Scripts/FightMatch/Host/FightMatchHostSession.cs`：18579B／`892b9d494286f18e41e6f6bfbe474df591d9c06fa19644bec36332391cf50859`。
- `Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs`：13773B／`d392d6808ad1fc8156482020951404eb46390d462ce0164404180dea067cce13`。
- 设计§1两文档及HostAssemblyInfo、现有Host测试/asmdef、直接调用的Platform根工厂可只读；现有业务/LOC等依赖全部不改。

## 2. 唯一写入区（无Git／无Unity工程）

根 `TestArtifacts/FightMatch/HOST-SAVE-ISOLATION-001/S/` 当前ABSENT。这是普通源码暂存目录，不是checkout/worktree或可运行Unity工程。只允许新建以下8叶（根以下相对路径）：
1. `source/Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs`：以固定共享前像为底，仅本设计增量。
2. `source/Assets/Tests/EditMode/FightMatchHost/FightMatchHostSaveIsolationTests.cs`：新测试源码。
3. `before.json`：create-once固定授权、实际身份、前像和保护快照，含Host原始UTF8供机械重放。
4. `verify.py`：离线机械取证脚本，仅read-only检查共享文件；生成本白名单证据可使用标准库，禁止shell/进程/网络/Unity调用。
5. `source.patch`：仅两逻辑产品路径的unified patch（Host旧前像→后像；新测试/dev/null→后像）。
6. `after.json`：保护快照及两后像。
7. `static-checks.json`：实际命令、exit/UTC、scope/行数/六名/条件编译和生命周期静态检查，不宣称执行了C#。
8. `source-receipt.json`：最终SOURCE_READY或BLOCKED、各准则、产物SHA/bytes/gitBlob、真实限制和发布范围。

所有作者文件用apply_patch；机械生成patch/JSON由本脚本生成可行。只许这两逻辑产品文件；Host additions+deletions≤350，新测试≤400行。不要为凑预算压缩可读性或拆生产文件；实有必要超限先报主程。不新增/修改public API、MonoBehaviour serialized字段、存档格式、依赖或asmdef；设计精确internal接缝除外。
共享Assets/Scripts/Tests、资源Scene/Prefab、所有meta、Packages/ProjectSettings/Config/Generated/Tools、LOC/H WIP、历史TestArtifacts和索引均禁止写；不复制完整工程、不创建.cache/pycache、不新造probe/runner。新测试meta本包不得创建；后续自然导入另签。

## 3. 实现与接受条件

- 精确实现设计§3的paired Editor-only flags、固定P推导、activation字段绑定、纯字符串先验/非规范路径/逐祖先链接/双向case-insensitive路径段非重叠、Mac realpath和缺失真实根nearest-existing验证、已有owner.lock独占租约及异步后重验。绝不创建或读取真实用户存档正文。
- 无marker原生产启动路径与I/O时点保持；Start拒绝经ShowFailure `HostSaveIsolationRejected`，零Session/零真实根fallback。SystemPersistentDataPath继续实际系统值、CanonicalProductRoot实际选定根、internal EffectivePersistentDataPath同根；不提前接H偏好、不掩盖settings/EmptyRoot冲突。
- OnDisable保留活Session租约；启动失败/取消及OnDestroy先Session.Dispose再finally释放；原工厂/业务/LOC检查保留。只在Host同文件加入设计两个Editor internal static seams，不新建抽象框架。
- 新fixture `FightMatch.Host.Tests.FightMatchHostSaveIsolationTests` 恰好六个非参数化[Test]，名称及必须断言严格遵守设计§4（HISO_01至06）。测试只用测试拥有的temp目录，不能接触实际player root；当前只交源码，不运行。所有默认/错误/链接/activation/独占/同根/重开与Dispose情形保留，不能用无条件pass或mock Host成功代替真实门。
- 最终补丁仅两产品路径，所有共享产品和FIX04字节/集合不变；没有编译/导入/test/native/device/Git副作用。

## 4. 机械核验与证据

编辑前先核上述固定输入、新test/meta共享ABSENT、新S根ABSENT；固定当前共享保护：
- 复用 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/before.json` 的inventoryRoots重枚举，当前预期1089文件；本包全部保持（不是拿FIX04前像两源作期望）。
- 固定当前FIX04目录全部叶（含source3、I/activation及PExecution封存6）及既有before的sealedEvidence、projectionProtected、native/protectedRecords所列实际路径；只按已存在清单元数据/hash读取，不扫Library/cache或真实玩家存档。额外包括本设计及设计固定依赖。
- 结束比较集合/类型/大小/SHA全相等；共享Host仍固定SHA、共享新测试/meta仍ABSENT。输入变化即停止，不回滚他人修改。

可执行验证仅普通shell只读、git status/diff/check等非变更命令、`python3 -B TestArtifacts/FightMatch/HOST-SAVE-ISOLATION-001/S/verify.py` 离线机械脚本。脚本参数由作者明确记实；运行次数以排除脚本问题所需为限，保留实际失败，不重跑历史验证。记录Host增删/测试行数、六完整名、patch在内存对before重放逐字节等于两后像、条件编译Player不含验收flags和Editor API、public/serialized表面一致、原默认分支及生命周期接线静态证据。静态检查不是编译/行为测试/独立review。
不下载/安装/编译工具，不启动Unity、C#编译器或测试，零网络/设备/真实Host/进程探查或关闭。没有构建项目和meta均明确NOT_RUN。

## 5. 交回与下一门

C冻结后停写，用final给主程8叶、实际turn、两后像hash、patch/receipt hash、准则及真实静态结果；无需发送回主程消息。主程唯一机械收件，不重复本地Code Review；中央负责Git发布，QA独立接收GitHub Code Review。
中央已核合适固定发布父提交为PR3 head `6818405b859d71bf8c0f116299e7e9605a3d769a`，base `codex/locale-preference-s1`，包含相同Host前像及FileLocalePreferenceStore。拟独立PR只两产品路径＋必要设计/证据，不重复依赖、不混PR4。此记录不授权C或主程创建Git branch/worktree/commit/push；发布之后按实际新head触发一次GitHub审查，未得到结果保持REVIEW_PENDING，编译/六例/G3各自NOT_RUN。

