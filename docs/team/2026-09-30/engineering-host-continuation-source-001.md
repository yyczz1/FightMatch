# HOST-CONTINUATION-SOURCE-001：隔离根与语言偏好接线

2026-10-07，待激活。[中央任务单](central-host-continuation-2026-10-07.md)，SHA256 `e42b29bf20938bc96ed0e5c62d081481997e5ad2bfa73e7305b476bfa4f73512`；AGENTS §6，文档准备。

## 1. 交接

主程/收件 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a11694-e130-7e21-9f9a-2644027e003c`。拟实施者“FightMatch 首建设置兼容源码实施” `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，completed/notLoaded；Astra/xhigh，激活绑actual。
仅新候选转该作者，原件/共享WIP冻结。作者→主程→中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。

## 2. 输入

路径别名：H=`Assets/Scripts/FightMatch/Host/`；T=`Assets/Tests/EditMode/FightMatchHost/`；A=`TestArtifacts/FightMatch/`。逐叶拼接，SHA256：

| 输入 | 字节 / 摘要 |
| --- | --- |
| 共享H+`FightMatchPlayerHost.cs` | 6595 / `12dba7d11323da883f108ea416699a6cf638a9360587c984d7d070ebeb6cc287` |
| 共享H+`FightMatchHostView.cs` | 17703 / `e72de88638d6c25bfe4d80e2c1b8990dae5ae225c6d4fe976b4b1641b1c975b9` |
| 共享H+`FightMatchHostSession.cs` | 18579 / `892b9d494286f18e41e6f6bfbe474df591d9c06fa19644bec36332391cf50859` |
| A+`HOST-SAVE-ISOLATION-001/S/source/`+H+`FightMatchPlayerHost.cs` | 19136 / `f527b4ceecffff0718ed1214f9d26eaca5d65f6e2c82ba973d6b8774c2e66b7f` |
| A+`HOST-SETTINGS-FIRST-CREATE-001/S/source/`+H+`FightMatchHostSession.cs` | 20308 / `67d62db541c47e3e1548ec32000605b6dcecf38b98a7332195a0824cc51060a9` |
| 共享H+`ILocalePreferenceStore.cs` | 3495 / `e768e6e39fe0f28ec292032ff9986f79606136ad6473b43fb6d5c550487e55a0` |
| 共享H+`FileLocalePreferenceStore.cs` | 13773 / `d392d6808ad1fc8156482020951404eb46390d462ce0164404180dea067cce13` |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocalizationService.cs` | 2786 / `ca8b2c5f9b8afd864410b35f7ab9998c04a7b5ea9efdf3a51e8082b6dd54a408` |

旧接收链：

- [HISO](host-resource-mac-003-integration-receipt.json) `572a51794cdad22f6048592377527cc90c3d04538c3c9a88e18af304c2580f4b`：HISO6/复合17通过。
- [HSC FIX01](host-settings-first-create-fix01-integration-receipt.json) `c522224f58d9811fb4f45d184f049ef1ae6b6b48d2725a4911dda7e978df25ad`：18项通过，生产原字节。
- [B-PREF](resource-raw-pref-m02-integration-receipt.json) `c97713baafe751e3d7a7cf56ffed29571b9b4eaf6443dfe8434cfcbe151b319a`：PREF77/128；旧失败保持。

资源候选：A+`RES-C-CONTRACT-001/FIX01/S/source/Assets/Scripts/FightMatch/AssetAccess/FightMatchResourceRuntimeContracts.cs`，16774B/`4ef8e56e70a19f7df81105fa46b3b1d334f39ce956610285845f1c594ec0520b`；A+`RES-FACTORY-ADMISSION-001/S/source/Assets/Scripts/FightMatch/YooAssetAdapter/YooAssetRuntimeFactory.cs`，14380B/`4e2683c1cfea846589f885c78309c3ce6ae2d89b187a040af5316ee5587daf68`。接收见[契约](resource-contracts-fix01-integration-receipt.json)、[AS/工厂](resource-as-factory-m02-integration-receipt.json)。共享缺席，本包不采纳。
依据：[H](engineering-runtime-localization-integration-001.md)、[HISO](engineering-host-save-isolation-001.md)、[准入](engineering-res-built-in-admission-policy-001.md)。

## 3. 激活后白名单

E=`TestArtifacts/FightMatch/HOST-CONTINUATION-SOURCE-001/S/`，父根缺席；新增10叶：

1. E+`source/Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs`
2. E+`source/Assets/Scripts/FightMatch/Host/FightMatchHostView.cs`
3. E+`source/Assets/Scripts/FightMatch/Host/FightMatchHostSession.cs`
4. E+`source/Assets/Tests/EditMode/FightMatchHost/FightMatchHostLocaleWiringTests.cs`
5. E+`before.json`
6. E+`after.json`
7. E+`source.patch`
8. E+`verify.py`
9. E+`static-checks.json`
10. E+`source-receipt.json`

Host基底19136B、View17703B；Session逐字复制20308B。patch相对三共享前像/新测试；另计Host相对HISO增量。Host/View增删≤320行、测试≤550行、E≤2MiB；超限停报。
仅私有成员及§4 internal接缝；public/serialized/asmdef/依赖/格式不变。新测试/meta缺席；meta后续自然导入另签。其它路径/共享产品/旧证据/资产配置/LOC/Git禁写。

## 4. 接线

1. HISO Start先校验标记/根分离并持lease；生产CanonicalSystemRoot/隔离受验P，一次选根：`new FileLocalePreferenceStore(P, new LocalePreferenceFiles())`。locator/profiles/settings同属P/FightMatch；禁无参store/真实根fallback。
2. 时序增量：首次Bind前选根/Load，LOC=null也可能提前创建/规范化系统根，但不建Session/PlayerSave。HISO“无标记不提前I/O”仅仍约束纯ResolveAcceptancePersistentRoot；中央激活本文才批准Host时序变化。
3. 隔离store构造、Load/Save及yield恢复时复核activation/lease/根；复用HISO检查，补查settings/目标/祖先无链接（叶可缺席）。失败撤权报HostSaveIsolationRejected，解析器不改；不保证抵御同用户恶意换链。
4. Host新增internal `InitializeLocalePreference(LocalizationService, ILocalePreferenceStore, SystemLanguage)`，生产一次Load→SetLocale→Bind；Remembered优先，其它沿系统默认/诊断，零自动Save。null文本源/IsReady阻断保持。
5. Host新增internal `SelectLocalePreference(LocaleId)` 返回既有LocalePreferenceSaveResult：SetLocale→Save，同语言也Save；事件后再核epoch/权限，失效返回null/零I/O。Failed/Unknown保留语言，不触PlayerSave恢复。
6. View保留3参Bind内存切换；新增internal 4参重载，末参`Func<LocaleId, LocalePreferenceSaveResult>`。Host传epoch回调；结果键显示三态，ErrorCode作`errorCode`参数。旧路径fm.language.changed；null/过期不反馈，返回再核generation。保留按钮守卫，Unbind清回调/监听。
7. 失败/取消或Session前Disable：撤epoch/store→停启动协程→解绑→释放lease；yield核取消，Enable不复活，保留诊断。活Session Disable仅撤回调/解绑，保留Session/lease/语言；Enable重绑、不Load。Destroy幂等：停启动/撤回调→解绑→Session.Dispose→finally释放lease；无新static。
8. Session原字节支持先偏好后首建/同P复开；不删settings或扩大EmptyRoot/resource-state准入。

## 5. 验证与返回

核输入，漂移停报；核10叶/patch/Session原字节/预算/公共与序列化零变化/读写解绑顺序。静态≤3轮/累计30秒，首个全通过即停，保留各轮错误/耗时。仅verify.py机械检查；Unity/编译/测试/fixture/下载/restore均0。

新fixture `FightMatch.Host.Tests.FightMatchHostLocaleWiringTests`，8个非参数化[Test]：

| 方法 | 必须断言 |
| --- | --- |
| HCP_01_LoadBeforeBind | 四种Load结果、绑定前语言、Save=0 |
| HCP_02_SelectAndRetry | Set先Save、三态不回滚、同语言Save但LocaleChanged不重复 |
| HCP_03_Feedback | 结果键/errorCode与View绑定一致，无假Saved/存档恢复 |
| HCP_04_Rebind | 旧回调零I/O，监听一次，Load不重复 |
| HCP_05_CancelStartup | 取消恢复点、释放lease前撤权、Enable不复活 |
| HCP_06_DisposeOrder | 活Session禁用持锁，销毁顺序及重复清理安全 |
| HCP_07_SettingsFirst | 自有P先存偏好再首建/冷复开，player身份/提交字节保持 |
| HCP_08_NullSourceAndIsolation | null源View诊断/按钮零保存建档；settings/目标链接拒绝 |

仅偏好store可test double，LocalizationService用null源；禁造catalog/成功源/读草稿。HostPanel会读草稿，不复用；第7项可用HostRig。其余用生产接缝/冻结Prefab；仅组件证据，非真实Host/EventSystem通过。I/O只用自有根，禁默认Start触真实档。

QA固定输入/selector，优先dots，Mac另签C例外。拟测新8＋HISO_05/06；PREF77/HSC18及纯选择器HISO_01-04复用。编译/运行/预算/临时路径另签。
回执含actual/10叶hash/patch/逐项标准及缺口，仅SOURCE_READY/UNCOMPILED/UNTESTED/REVIEW_PENDING。同head GitHub审查/实测/共享采纳待办，作者不签ACCEPT。
LOC/manifest及资源构建缺失阻断真实Host/双语/首帧/设备/Demo，不阻断本包源码。旧P1/UNKNOWN/NEEDS_FIX/LOC拒绝保持，无旧任务重试/Canvas断言改动。
