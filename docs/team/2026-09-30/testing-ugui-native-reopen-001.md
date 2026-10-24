# UGUI-NATIVE-REOPEN-001：真实场景复开准备包

2026-10-01 · `PREPARED_NOT_AUTHORIZED`；本文不是执行授权或集成ACCEPT。本回合只新增本文，未运行Unity／脚本／测试、未改产品／资源、未派C／云／R／agent。
交付owner主测试 `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local`，准备turn `01a0f6c0-3a76-7162-8c7e-af2963ddec65`；唯一回中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。authority：用户AGENTS §6站立协作授权＋中央本轮准备指令；以后执行仍由中央单独激活C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`，Astra/xhigh，中央唯一收件。
目标：证明已保存的实际 `Assets/Scenes/FightMatchDemo.unity` 经关闭／独立复开后的引用、原生激活、Canvas矩阵和Board几何／画面；Prefab rig、批处理导入和作者口述各有边界。

## 1. 固定输入与激活门

- FIX20 PR head `36ad62c4c71f4b987efaf60f26c942a0a9de10bd`；本地HEAD不等同该head，以FIX20冻结manifest及实际WIP逐项绑定。详细保护集沿[FIX20](engineering-ugui-01-q4-geometry-fix-20.md)，既有证据根 `TestArtifacts/FightMatch/UGUI-01/q4-correction-20/` 只读。
- 中央本轮最新回传：actual12/12 Passed、75.019s exit0、82.39s进程清空，`178+12+30+2` fullname Counter精确222成立；本准备包引用该回传，未重新执行或独立复核其全量原始证据。PR新head审查Completed但有[P1 review-binding缺口](https://github.com/yyczz1/FightMatch/pull/1#discussion_r4153777291)，中央已交ARCH补证，**审查门尚未关闭**。
- G1：中央接收上述222绑定和对应head的P1处置／GitHub审查关闭证据；元数据补证不触发Unity重跑。G2：C前次owned进程已清空、项目无其它Unity占用、实际工具版本／图形后端可用。G1/G2未齐，资源阶段也不启动。
- G3（完整运行时门）：正式LOC源已接入真实Host、实际内容／战斗入口确定、真实Host存档隔离方案已获主程与中央批准、原生观测／截图入口已冻结。G3目前不成立；不得以G1通过代替G3。
- 若LOC接入、探针或其它源码改变候选，绑定新head／manifest，先做影响分析并取得对应GitHub审查；222只能按实际未变范围复用，不能自动覆盖新候选。

本轮只读核得的关键SHA-256（执行时还须与完整冻结清单核对）：

| 文件 | SHA-256 |
| --- | --- |
| `Assets/Scenes/FightMatchDemo.unity`（12900B） | `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd` |
| `Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab`（1060568B） | `55ded35adf6fd7d4e207303801e3ada172190bea6a516443e1bcd21d3193004b` |
| `Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset` | `290a57ac32c5ce7d965bc771d0e65e5c9f543f98df28b6e2e360f8dd6b2196c0` |
| `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` | `2c7a1612954ccc341f72a58918ffc1356074bf1802c866f01b7c9fdb72ad7dac` |
| `Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs` | `12dba7d11323da883f108ea416699a6cf638a9360587c984d7d070ebeb6cc287` |

## 2. 已有入口与明确缺口

1. 复用 `FightMatch.Host.Editor.FightMatchAndroidBuild.VerifyUguiResources`：原生OpenScene实际Demo，验证唯一Host／Canvas／EventSystem、项目输入模块、Host→实际Prefab／font／license、Overlay／540×960／match0.5／Raycaster、119个TMP占位符及字体材质、失联引用／脚本和旧UITK依赖；完成后切空场景，输出scene/prefab/font哈希。它**不SaveScene、不进入PlayMode、不创建Session、不观测矩阵／mesh或截图**。
2. 当前路径guard允许新的 `TestArtifacts/FightMatch/UGUI-01/<64hex>/resources/reopen/resource-reopen.json`，不必改guard或写历史根。相邻`run.json`必须绑定stageId=UGUI-01、mode=VerifyResources、C thread、实际authorTurn与`-ugui01AuthorTurn`一致、Editor源码SHA、完整argv含`-ugui01Output`值；目标必须StandaloneOSX。旧FIX09“入口仅接旧路径”的描述不能套用当前源码。
3. 当前PlayerHost.Start创建 `LocalizationService(null, systemLanguage)`；IsReady要求source非null，故在`LocalizationNotReady`退出，尚未访问存档／创建Session。可观测的只是失败诊断与根Canvas，**正常导航、Battle和Board验收为BLOCKED，不是失败诊断出现即整包通过**。
4. 正式源就绪后Host才使用 `Application.persistentDataPath/FightMatch/{locator,profiles}`；未发现已存在的隔离根注入入口。真实Host在Editor只支持OSXEditor；Linux dots或HostRig的临时工厂不能替代此路径。安全隔离未设计／批准前，不启动可达真实存档的流程。
5. Scene GUID=`1d5124e5b55fe409d8216e78a117dec0`，Prefab GUID=`c36df3cfc25424c8cb3ec6cae6be1237`。场景RuntimeCanvas(fileID1013562322995895578)保存scale/anchorMax为0；这是待原生激活观测的驱动值，不能静态判为运行时零矩阵，也不授权修资源。

## 3. 分阶段最小执行合同（中央另行激活）

**R／资源引用，G1+G2后可独立批准：** C只启动一次Unity2022.3.18f1 Intel Mac Editor，Metal、`-batchmode -quit`、无`-nographics`，显式projectPath／StandaloneOSX／上述executeMethod及两个身份参数；启动至exit≤300s。以冻结argv、JSON、真实exit0、无编译／异常／失联／序列化错误及资源零漂移结束。没有代码或资源写白名单；不调用PrepareResources／SaveAssets。本阶段不满足下面N门，也不重跑12／222／4181。
R证据根由激活单填满新candidateSha256，扩展成唯一绝对路径并确认不存在；仅新建该`resources/reopen/`内 `activation.json`、`run.json`、`resource-reopen.json`、`before.json`、`after.json`、`unity.log`、`editor.stdout.log`、`editor.stderr.log`、`process-events.jsonl`、`quiescence.json`、`receipt.json`。哈希／执行身份缺值即停止，不拿PR短SHA充当candidateSha256。

**N／保存—关闭—复开及真实运行时，G1+G2+G3后再签：** 目前未找到能完成全部观测的现成入口，下面是签发要求而非可立即运行的命令；不为已知LOC缺口先启动一次。

1. 冻结完整实际输入、新head审查、工具／argv、测试专用存档根及真实UI战斗进入步骤；记录scene/prefab/font/meta/settings／源文件、用户存档保护清单及非owned进程身份。存档根须归本包且与真实用户根不相交；保持HOME／CODEX_HOME、PlayerSettings及真实用户存档不变。
2. 进程S只开实际Demo，在非PlayMode、无未授权dirty内容时原生SaveScene到**原路径**一次；保存前原字节副本和保存后hash，要求逐字节相等。自然退出并确认owned清空，才允许独立进程O复开同一路径。差异即封存／停止，不Apply Prefab、不回滚／重生成、不把重序列化自动认定可接受。
3. 进程O复开后先核同一GUID／hash／引用，进入真实PlayMode，由原生生命周期和真实UI完成Host启动／战斗页激活。仅观测真实对象，不Instantiate替代rig、不注入测试catalog/session、不反射写字段、不手调OnEnable/BindNative、不预先激活battlePage／补尺寸。固定Game view为540×960并记录实际分辨率／safeArea；不产生战斗提交／奖励。
4. 在有界就绪点（布局最多三帧及ForceUpdateCanvases，非重试循环）采集§4数据与原始画面；失败先存原始证据，退出PlayMode、关闭owned Editor。两个进程各≤300s、各一次；不追加独立Compile或R重跑。若R已获接收且输入不变，沿用R引用证据。
5. 当前N **无产品／资源变更授权**。仅拟供主程单独签发的最小新探针对为 `Assets/Tests/EditMode/FightMatchHost/FightMatchNativeSceneReopenProbe.cs` 与同路径`.meta`，使用现有Editor测试assembly/friend可见性；不改asmdef/API／现有测试／Host。必要监护脚本仅拟 `N/validation-tools/runner.py`；入口、源码及静态自检冻结／新head审查后才能启动。真实LOC和存档隔离若需产品代码，必须另立精确白名单，不在此探针中绕过。
6. N=`TestArtifacts/FightMatch/UGUI-01/native-scene-reopen-001/<activationId>/`，激活时填成唯一新绝对路径；拟证据白名单：`activation.json`、`input-manifest.json`、`before.json`、`after.json`、`scene-before.unity`、`save-close.json`、`runtime-observation.json`、`startup.png`、`battle.png`、`image-manifest.json`、`process-events.jsonl`、`quiescence.json`、`receipt.json`，以及`save/`和`reopen/`各`run.json`、`unity.log`、`editor.stdout.log`、`editor.stderr.log`。隔离存档目录与其写集合尚未签定，是G3缺项；不能自行扩展此列表。

## 4. 接收数据、画面和失败边界

- **可自动断言的数据：** actual scene.path／GUID和Prefab实例来源；唯一实际Host／Canvas／EventSystem；Host.Session非null；实际Battle页／Board activeInHierarchy与isActiveAndEnabled；InputView.Controller引用等于Session.Battle.Input，face宽高>0，原生激活事件顺序可追溯。按RuntimeCanvas→SafeAreaRoot→ScreenLayer→BattlePage→BattleContent→BattleScroll→Viewport→Content→BattleStatus→BoardFrame记录每级Rect、matrix、active、clip/cull；不得只读保存值。
- **矩阵／几何：** Canvas与Board实际Rect宽高正且有限，m00/m11/m01/m10有限，m00>0、`abs(m00-m11)≤0.0001*m00`、`abs(m01),abs(m10)≤0.0001`；face有效、cell正有限。调用真实CellCenter／CellAtLocal核四角中心往返及半开边界，真实CanvasRenderer mesh.vertexCount>0并记录cull/clip。任何缺项、异常、空mesh不能用尺寸正常抵消。
- **必须有原始画面：** 保存同一O进程真实Game view的startup.png／battle.png，记录UTC、PID、实际像素尺寸／safeArea、候选和文件SHA；人工确认根UI／棋盘可见、格子不被祖先裁空、文字有实际绑定而非错误占位符／缺字方框、图像与数据时点一致。纯数字、离屏mock、设计图或合成截图不等于视觉接收；这仍不替代Android设备／布局全面验收。
- **资源／进程保护：** final font也不得新增glyph/atlas，全部冻结source/resource/meta/settings及旧证据字节相等；Unity自身派生缓存不由worker手工编辑。log各≤8MiB、总新证据≤96MiB、额外temp≤128MiB、可用空间≥2GiB；截图也计总量。先核非owned进程，绝不关闭用户Editor或其它任务进程。
- **首败停止：** 超时保存状态，保留handle观察≤60s，仅对PID／出生时间／argv／projectPath全匹配的本包root单次SIGTERM，再观察≤30s；仍在则`BLOCKED_MANUAL_PROCESS_CLEANUP`，不发SIGKILL／全局kill。之后即便退出仍保留timeout。沿[FIX16 §5](engineering-ugui-01-q4-grouped-validation-fix-16.md)，禁止同根重试／延时追加运行。
- **返回：** C只回中央一次，列实际thread/host/turn、输入、每阶段NOT_RUN／PASS／FAIL／BLOCKED、argv/PID/退出／耗时／清理、所有证据身份与漂移。R通过只记`RESOURCE_REFERENCES_VERIFIED`；N缺G3记`BLOCKED_RUNTIME_LOCALIZATION_AND_STORAGE_ISOLATION`并保留观测入口缺项。完整ACCEPT须实际保存／关闭／独立复开、运行时数据、原始画面接收、零越界及对应GitHub门全部成立；本准备稿不作该结论。
