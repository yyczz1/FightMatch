# LAYOUT-S1-SYS-001：完整布局源码冻结合同

2026-10-02 · `SOURCE_CONTRACT_READY_NOT_EXECUTED`。只冻结实施边界，不是代码审查、编译或Demo接收。
SYS `01a0f2e6-29bb-7092-abf0-705b41b7bf93/local`，实际turn `01a0fa51-fb29-7022-8a3e-5bbdd9302d03`；唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`。依据主程转达用户2026-10-02继续首Demo、明显临时美术占位及standing workflow授权。本文不派C/R，不产生产品写权限之外的新权限。

## 1. 权威、输入与门的划分

- 复用ARCH `engineering-ugui-layout-source-activation-architecture-001.md`，58行，SHA256 `83a359f352cbc0ab953ab25179f6378529ec5334fc338e9e25419d8b0b5c7f84`，ARCH turn `01a0fa4a-6790-7731-8159-e196f3a9b7a1`。中央裁定＞correction＞base；已接受R/主美设计不重审，不改层级、几何或Caption。
- 唯一机器输入为同目录[inputs-001.json](engineering-ugui-layout-source-activation-inputs-001.json)，含133条path/存在状态/bytes/SHA、真实Git blob映射、逐文件行预算、字段合同、静态命令与47用例候选；不得只复制本文而遗漏JSON。
- PR1 head `3541930877837e14b58020b910976b8b67b56e42`，真实tree `37326f759fc065f268c23f66a674be1a61f12268`；已用本地Git对象逐项核对相关blob与工作区相同。29产品、37资源均与固定manifest一致。FIX22限定ACCEPT回执SHA `89255ba4c154a37168895d2daa7a249ac09b4249888cdb932ade2de06fc14b04`，不扩大为布局/Host/Demo通过。
- PR3 head `6818405b859d71bf8c0f116299e7e9605a3d769a` 的3份LOC源码独立标注真实blob，3份meta均ABSENT；CSV、设计及LOC authoring输入没有伪称来自PR1。当地master/HEAD不是本次前像。
- **S可独立执行；I/P/T待激活不阻塞S。** 旧Prefab尚未迁移，真实Host仍受正式LOC接入阻塞；S不创建meta、不改资源、不运行Unity。历史222只作历史证据，本次不跑222/234/full/额外28。

## 2. S唯一产品白名单与预算

以下目录简写仅为阅读；JSON列出全部绝对仓库相对路径。`P=Assets/Scripts/FightMatch/Presentation/`，`T=Assets/Tests/EditMode/FightMatch/`。共最多15个C#，其中2个新文件，新增＋删除行总上限**7,840**；逐文件上限不可互借。未实际需要修改的条件文件不动。

| 必需路径 | 新增＋删除行上限 |
| --- | ---: |
| `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` | 1800 |
| `Assets/Scripts/FightMatch/Host/FightMatchHostView.cs` | 300 |
| `P/PlayerBattleView.cs` | 700 |
| `P/PlayerDefaultReferenceView.cs` | 160 |
| `P/PlayerNavigationRecoveryView.cs` | 450 |
| `P/CandidateBoardInputView.cs` | 650 |
| `P/CandidateBattlePlaybackView.cs` | 320 |
| 新 `P/FightMatchResponsiveLayout.cs` | 650 |
| 新 `T/UguiResponsiveLayoutTests.cs` | 1400 |

| 从13候选中纳入的最小6项 | 上限／必要性 |
| --- | --- |
| `T/CandidateBoardInputTestData.cs` | 550；直接构造旧enemies/members/hp/intent/dialog/recovery字段，须统一适配固定槽与内panel。 |
| `T/PlayerNavigationTestFixture.cs` | 180；当前禁用Host并强开两个外根，改为观察真实Host共享根同步逻辑，不镜像XOR策略。 |
| `T/PlayerBattlePresentationTests.cs` | 200；旧测试直接操作常显History，需先经真实History入口，保留原业务与过期回调断言。 |
| `T/CandidateBattlePlaybackPanelTests.cs` | 180；aggregate HP断言改为OriginalSlot固定标签，保留HP值、语言、诊断、令牌及完整链断言。 |
| `T/UguiSceneCompositionTests.cs` | 180；替换119/116硬编码为独立枚举的精确目标集合；WorldSpace mapper用例仍仅证明mapper。 |
| `T/LocalizedTextBindingTests.cs` | 120；同样替换旧文字计数，保留3个输入框、诊断placeholder与逐项绑定断言，禁止改成“至少”。 |

排除的7项及逐项理由在JSON：PlayerBattleTestFixture、CandidateBattlePlaybackTestData、FightMatchHostTestFixture经共享构造/真实Prefab即可获得新引用；CandidateBoardInputTests、PlayerDefaultReferenceTests、PlayerNavigationRecoveryTests、FightMatchHostRoutingLifetimeTests的稳定身份/业务合同未变。不为布局批量改写它们；若实施证明必须改，先回主程，不自行扩白名单。

## 3. 字段、所有权与公开符号

- JSON `serializedContract`逐项固定新增/退休字段名称、类型、目标；所有字段保持private `[SerializeField]`。Host新增responsiveLayout及8个共享根/mask/内panel引用；Host独占外根，Battle/Navigation只写自己的内panel，双panel同时active是诊断，不选赢家。
- Battle退休dialog/recovery混合容器，拆为StateRows、ChoiceRows、SafeActions、DangerActions；HUD文本与DynamicBattleActions分离，新增History固定Header/Close/Open与Normal/History根。OwnContainer只销毁自己记录的动态行，固定Header/Button/槽/Viewport/模板绝不清除。
- Navigation退休confirmationRows/recoveryRows、共用title/status及固定originalResultButton；分别绑定两panel的固定标题、状态、选择、动作区域。OriginalResult为当前ChoiceRows内最多一个`fm.row.save.OriginalResult`，原SelectOriginalOperation语义不变，不runtime reparent。
- Input保持CandidateBoardElement类型，退休enemies/members，新增3组各3个槽及显式Stage标签数组；Playback退休hp/intent聚合字段，通过JSON所列internal seam独占HP/intent更新。合法敌方slot>2保持domain合法，只进入UnsupportedEnemySlotLayout；不压缩槽、不改保存，Exit仍可达。
- Reference使用固定Header/Close、Header/Title和Body内既有绑定；Actions高0。内部close通知由Battle以owner/page/renderEpoch守护；先记pointer→Close→feedback，不取消首个真实drag。Normal/History/Reference互斥，均不盖Board。
- 新公开生产符号仅`FightMatchResponsiveLayout : sealed MonoBehaviour`和现有Editor类的`public static void MigrateLayoutResources()`、`VerifyLayoutResources()`；另有测试类及12个声明测试。既有public签名不变、**不新增接口**。JSON列internal缝隙；新增序列化字段或public符号需求先报主程。
- Responsive只读SafeArea/Canvas并控制布局与自身gate；Host在invalid转换取消既有pointer。订阅Changed/willRenderCanvases与解绑幂等，保留最后有效rect。各轴只有一个owner；ScrollRect→Viewport(Image+Mask)→Content为明确链，Clamped、inertia=false、elasticity=0。不得把synthetic/world-space fixture称为实际Overlay验收。

## 4. 安全builder与临时美术

- 老`PrepareResources()`要求资源不存在且写font/TMPSettings；**禁止重跑**。旧builder/Android实现保持原行不删除不修改，只在同一文件新增布局入口及私有helper；旧119计数核验不作为布局验收，也不放宽它。
- 新入口无托管参数；仅接受`-fmLayoutContract LAYOUT-S1-SYS-001`、`-fmLayoutInputManifest <I/activation.json绝对路径>`、`-fmLayoutEvidenceRoot <P绝对路径>`。I激活时填真实候选路径/源hash/资源37/meta前像。拒绝路径越界、symlink、复用输出、未知flag、前像漂移及真实Host Start。
- P只LoadPrefabContents**既有**Prefab→定点迁移/显式重连→同路径native保存/卸载→更新既有Scene实例→native保存/关闭/只读复开；保留根、存续对象fileID、稳定ViewId、字体引用。不得替换整个Prefab、手写YAML、创建字体/图集、SaveAssets顺带保存无关dirty对象或改Android设置。
- Prefab GUID `c36df3cfc25424c8cb3ec6cae6be1237`；Scene GUID `1d5124e5b55fe409d8216e78a117dec0`。布局核验按独立枚举的完整TMP目标路径/类型集合派生精确数量；源码交付时冻结集合与计数，P复开比对，不从实际观测反向降低预期。一个Canvas/Raycaster/项目EventSystem，BoardFrame只一个Graphic。
- P证据输出仅`native-migration.json`、`native-reopen.json`、`serialized-targets.json`，置于JSON指定C-owned create-once P根；迁移进程内调用同一核验核心，不另开第二次verify进程。
- 新增/改动Prefab必须显眼灰盒：既有Image＋纯色/线框/双色块；装饰子节点名`TempArt_`、raycast=false，位于原actor/member槽内部。无新图、纹理、动画或Prefab，不采用未选ArtSource素材，不改槽/热区/Caption。正式美术未选定。
- 不新增“临时美术”等玩家文案/key；所有TMP仍存`【if you see this, it is a bug.】`并通过既有EN/简中绑定替换。临时图形不得遮住漏绑定诊断。Caption固定18、禁autosize/压字/改写，宽184/224/200/144/152/256/248/152k，高48k，LR12k/TB8k；命令滚动1624→252k，首Retry与末Reference完整可达。

## 5. C交接、静态核验与回执

- 主程在**首次源码写入前**记录旧功能写入已停的时间、具体交出/接收thread/host/turn、实际C当轮turn、两份合同hash与范围；未填未来C身份、未来PR head或I/P/T租约不阻塞SYS合同完成，但C不得跳过自身实际交接。不覆盖/stash/reset其它WIP。
- S证据根固定`TestArtifacts/FightMatch/LAYOUT-S1-001/S`，首次创建；仅`handoff.json`、`before.json`、`after.json`、`static-checks.json`、`source-receipt.json`、`source.patch`。不得新增产品外脚本或通用runner；内嵌只读核验代码在JSON内。
- 精确命令见JSON `staticCommands`：执行`python3 -B -c 'import json,sys; m=json.load(open("docs/team/2026-09-30/engineering-ugui-layout-source-activation-inputs-001.json")); exec(m["staticVerifierPython"])' preflight`；源码后仅将末参改为`postflight`。保存stdout为before/after，命令/UTC/退出码进static-checks。脚本不写文件、不跑Unity，前后快照保护Assets/Packages/Settings/LOC/ArtSource；差分按PR1 blob而非dirty HEAD。
- C按JSON `staticChecklist`逐项给源码位置/结论，包括字段、轴owner、scroll链、共享根、动态行清理、epoch/退订、槽、Caption、临时图与12测试声明；这些是作者静态证据，不是独立review或绿测。新增serialized字段需先报主程，不能用未来编译解释未授权变更。
- source-receipt含实际C身份、合同hash、changed paths/逐文件before-after bytes/SHA/新增删除行、patch hash、所有保护项、逐条AC结果、未验证项；必须标`SOURCE_READY / UNCOMPILED / UNIMPORTED / OLD_PREFAB_NOT_MIGRATED / NOT_PLAYABLE_CANDIDATE`。仅回主程，由主程派GitHub PR Code Review；作者不能给自己ACCEPT。

## 6. 后续I/P/T只待激活

- GitHub仅审实际新head；旧PR1 no-major不覆盖新源码，未解P0/P1阻止native执行。I最多1次compile/import≤360s；P最多1次迁移/保存/卸载/复开≤180s；T最多1次精确并集≤47实例≤360s。每进程owned-only自然60s＋终止后30s；日志≤8MiB、阶段证据≤192MiB、剩余≥2GiB。首错停、重试0。
- 已从历史222证据机械匹配20方法/32实例；JSON保存这32个完整名＋12个设计声明＋3个资源名。**实现后的真实叶子尚未发现**；P后由主测试/C冻结实际名单、字面转义全锚定selector与源/资源hash，任何漂移先报主程，不凑数/删断言。8个Overlay用例保留enabled CanvasScaler与两safeRect、首帧、resize、滚动/Caption、mask及生命周期要求。
- 改动的其它fixture依赖者不自动继承历史绿测；超47的必要覆盖交主测试另定，不扩大本门。最终natural meta/native资源产生后再绑定实际head审查；主美视觉、真实LOC Host、保存关闭复开、Android/L1仍未完成。
- dots优先，但本候选未验证；已知graphics001/002卡Xorg socket，不能把002的目录warning/sudo缺失推成唯一原因。本轮不重试云环境。
- 只读候选路径`TestArtifacts/FightMatch/LAYOUT-S1-001/projection`当前不存在；本盘可用442,368,000,000字节，PackageCache约425,208KiB，51个锁定package.json均在；已核外盘Intel Unity2022.3.18f1可执行身份。**这些不证明离线导入已就绪**；完整缓存闭包、准备预算及图形/许可须后续签准确激活单，当前不建目录、不复制cache、不下载。
- 隔离投影只取PR1真实tree＋S delta＋固定测试CSV及已准包；排除PR3三源码/meta和Config/Generated/Tools LOC WIP。不用Git worktree/checkout，不移动共享文件。共享工程Unity会自然生成未授权LOC meta，因此禁止直接启动；该限制只挡I，不挡S。
- I只准自然生成两个新C#的meta；P只准改现有Prefab/Scene，其现有meta只读。交回前验证共享目标仍等于原前像，保护LOC；漂移先停，不覆盖。I/P/T真实argv、候选工具、owned PID/starttime/租约在对应激活单填写后才运行，不能照抄旧FIX22硬编码runner。

## 7. 停止线与本轮结果

任一输入hash/blob/absence漂移、未交接重叠WIP、第16源码/第27总产品路径、逐文件/总行预算超限、额外meta/资源/字体/TMP/CSV/依赖/配置变更、不能保持固定字段/轴/生命周期/Caption、需要LOC业务改动，立即停止受影响门并回主程；不回滚用户文件、不自行换方案。后续编译/测试非零、超时、failed/skipped/inconclusive、名字集合漂移或owned清理失败同样停止。
本轮仅新增本文和JSON；核验固定输入与Git对象，未执行源码、Unity、dotnet、测试、联网、下载、环境准备或Git写操作。uGUI技能用于固定scroll/typed-reference约束，codebase-design用于保持现有组件边界；没有新架构层或接口。
