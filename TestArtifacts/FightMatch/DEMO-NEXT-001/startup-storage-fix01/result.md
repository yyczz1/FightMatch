# 当前交付（2026-10-18）

SOURCE_READY / UNCOMPILED / UNTESTED / REVIEW_PENDING；整体 NEEDS_FIX。三候选及补丁已冻结，共享源码未写。原 BLOCKED 分析逐字保留于下方历史。

作者 actual `01a14d75-7519-7b03-98a3-fd4a4dc949d4`，原 thread/local；唯一签发/收件主程 actual `01a14d64-97e1-7632-abcd-60c49e3de2c3`。按 [engineering.md 顶部已签合同](../../../../docs/team/2026-09-30/engineering.md)和 SYS actual `01a14d6f-152b-76a1-9bb8-37ec6d53f2a1` 实施显式 Android 系统锚。这是批准的证明责任调整，不称与旧祖先拼写证明完全等价；原真人权源及真实红样本见历史。

仅 Store、StoreTests、PlayerHost 三源；Host 显式传已有 canonical 系统锚，实际创建入口先验后建再复核；Android 只跳过锚上枚举，整条目录链仍检查属性/native 无链接、反复核 A，锚下继续精确枚举和布局保护。ENOENT/访问/其它原生错误均拒绝，无 token 选策略或缓存放行。已有产品根不重复创建；创建后失败可保留空目录，但不写资源记录或创建 Session。Mac/Windows、进程/格式/提交/UnknownCommit 保持。未改 Platform/程序集可见性/公共 API/依赖/资产。

本根五叶：result.md、source.patch、source/下三份原路径。完整前像路径及 SHA 见历史表，与 startup-storage01 一致；本次候选如下。

| 候选文件 | 字节 / 增删 | SHA256 |
| --- | --- | --- |
| FightMatchResourceActivationStore.cs | 33820 / +115 −4 | ee2a3964d1c3f5f3266e1af927ca265f1a09d1f4d1a0bc51250b6fd8e99dbdb1 |
| FightMatchResourceActivationStoreTests.cs | 63482 / +189 −3 | 4aa90f09148c331864d6d91bf13a43bd813f4cc3cea2972d80162b4939b9f2fd |
| FightMatchPlayerHost.cs | 32827 / +16 −0 | 8c16f671f2e8873a582163a2ac5e86fcb559880818e40c9ceaf34f4a2d111971 |

[source.patch](source.patch)：26218B，SHA256 `b8be629b16cb166ba6d5bdc5bed9f3510e9dc280e9c3c8ba0554b5599ae9b134`。此前 `6a97ae37…` 对应 Store `b1206216…`；随后仅增加固定 anchor 诊断标签，形成当前 Store `ee2a3964…` 和 b8be 补丁。这是已完成的有限源码差量，不是单纯补丁格式变化，冻结后不再改动。

先新增5个回归候选，再改生产；未执行新红绿。最终三源正反 patch --dry-run --batch --fuzz=0 共6次 exit0，新增尾随空白0。原15个 AS 测试方法逐字保留，AS14不变；原文件接口、token、转换、Execute、Canonical、ReadLeaf及以后所有记录/提交方法原文保留；Host 初始化来源及桌面创建块原文保留。四共享前像仍等于冻结输入；HostResourceTests 未修改。

仅建议以下7项后验：

```text
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS04_PrePromotionFailuresPreserveActive
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS05_UnknownCommitNeverAdvances
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS16_ExplicitAndroidAnchorSkipsOnlyUpstreamEnumeration
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS17_AndroidChecksEveryExistingComponentAgainAndRejectsProbeErrors
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS18_AndroidRejectsMissingForgedOrMismatchedAnchorBeforeCreating
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS19_AndroidKeepsDownstreamCaseLinkLayoutAndEnumerationGuards
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS20_HostAndroidStorageChecksBeforeCreateAndRechecksAfterIt
```

新增项覆盖显式模式、全链错误和锚重核/不缓存、假锚、下游大小写/链接/未知项/延迟枚举，以及实际 Host 内部入口传锚与创建前后顺序。注入接缝不证明 native/应用 UID/API24；Android 条件分支编译、真实探测/首次写入/替换及保档仍待后验，新 head 需 GitHub 独立审查。原生仍拒绝时保留诊断，不减守卫。

零 Unity/ADB/设备/构建/下载/Git。A余207.905966秒、B余399.566976秒未消耗或重置；上一轮测试54.919210秒＋构建181.644621秒已超过当前A余量，必要时限由中央/主程统一处理，不自动执行。只回主程后结束空闲回合。

---

## 裁定前历史（原文保留，不覆盖当前交付）

# DEMO-NEXT-001 / startup-storage-fix01

2026-10-18。**BLOCKED：祖先段的大小写证明方式需要明确裁定；整体 NEEDS_FIX。** 已定位真实失败操作，但在本包禁止改变信任模型的条件下，没有证成等价的平台替代检查。未生成修复源码或 source.patch，不将空补丁标成 SOURCE_READY；本目录仅此一叶。

作者 thread `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，fresh actual `01a14d6b-afca-7081-a2cf-9c739fe38a35`。唯一签发/完成接收者主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，本次 active actual `01a14d64-97e1-7632-abcd-60c49e3de2c3`。沿真人 turn `01a14d27-e460-70b1-ab5d-a5ed1fa6aefd` / item `01a14d27-eb36-79a0-9a2b-ba591af92a3d` 的必要相关修正许可及 AGENTS §6；只向主程返回设计差量，不自行询问用户、派 SYS 或运行后验。

## 已证明与仍未知

- 实读 [emulator-03 日志](../emulator-03/logcat-start.txt) 第190–191行：13:08:52.312，PID11887，`stage=activation.guard.ancestor.entries; error=UnauthorizedAccessException`，随后 RES_STORAGE。日志25949B，SHA256 `bdced509e2fe1b314c60b5820d2edc809f55b5efa2b9144d495e471776d52b7d`。
- [设备结果](../emulator-03/result.json) 18879B，SHA256 `29e6f68361ec2e5e5ec4ab9e11d04afc3f8b65dded5854a86335eba307bbc3aa`：精确生产 Activity 的 MAIN/LAUNCHER 冷启动，无 extras，CommandLine 为空/GLES3；APK SHA256 `72186bd6acf65fba5bd02f6d7da3c67ad2ca532d768fddc68172850e4ebd47ee`。六份可读业务文件保持，两锁文件未读取。图标点击验收、L1/状态恢复/奖励未通过，不把标准 launcher intent 误写成图标点击成功。
- [诊断后验](../startup-storage-run01/result.json) SHA256 `533ba0e18ff2756249a78b85d1d6f913ef562f0846c2c3210730ff2c230d1e73`：AS04/05/13/14/15、H07共6项通过、无跳过，诊断包构建成功；已审产品 head `902006fa002088556caa69002ecc1b021c9146f1`。这验证诊断，不证明修复。
- 确定执行到了 Enter → Execute → DirectoryGuard 的祖先 `files.Entries(parent).Any(...)`，枚举获取或迭代抛出 UnauthorizedAccessException。DirectoryGuard 可由 Load/Layout 或 Commit 中的复核调用，日志没有区分第几次检查；不能据此断言此前没有目录/临时文件操作。当前记录的失败操作不是 CreateDirectory、promotion 或资源 SDK 调用。精确祖先路径、原生 errno、具体权限层和其它 API24+/设备行为仍未知；没有增加设备探测。

## 等价保护缺口

已读 [原资源状态合同](../../../../docs/team/2026-09-30/engineering-res-d-activation-store-source-001.md) §1：明确绝对 canonical 产品根，拒路径别名/链接，每次 IO 重核祖先/叶，检查失败拒绝；本次主程另明确不能删除大小写约束或缩小可信边界。当前 DirectoryGuard 的 Ordinal 父目录枚举同时承担“该名字确实存在且大小写准确”的证明。

已读 `Assets/Scripts/FightMatch/Platform/AndroidLocalSaveStorage.cs`（12567B，SHA256 `bb3521c96ce9c384d5bfa908a0556c042bce107b636ca95b85401a4c5df3854c`）中的 SystemRoot/Absolute/Root/Check：

1. SystemRoot 调 realpath、检查结果是目录并返回路径；没有逐目录项的精确名字证明，也没有把输入与结果作拒绝式比较。Host 已使用它建立系统路径。
2. Check 逐级 readlink 拒绝链接，只有 EINVAL/ENOENT 可继续，其它错误拒绝。这可以作为链接检查依据，不能独立证明每段名字的实际大小写；对已存在目录仍需属性/存在性验证，不能把 ENOENT 当作存在证明。
3. AndroidStoragePaths 是 Platform 程序集 internal，Host 当前没有该程序集的 friend 通道。为复用而新增公共 API/friend 或假借存档适配器构造器也不应悄悄加入。此访问问题可以通过另行限定内部实现解决，但解决它不等于解决大小写证明。

“Android 天然大小写敏感”不能作为替代论据：[AOSP Android7.1.2 sdcard 源码](https://android.googlesource.com/platform/system/core/+/android-7.1.2_r6/sdcard/sdcard.c) 的 find_file_within 明确可作不区分大小写查找，并区分请求名与 actual_name。这个例子处于 API24+支持范围，却不是本次模拟器镜像的实现证据。

“realpath(path)==path 即等价于原枚举”也尚未证成：[历史 Bionic realpath 实现](https://android.googlesource.com/platform/bionic/+/f0777843c03deb26b1f78c8edd17c557041696e9/libc/upstream-freebsd/lib/libc/stdlib/realpath.c) 逐段拼输入、lstat/解析链接；[另一版本](https://android.googlesource.com/platform/bionic/+/ce84677/libc/bionic/realpath.cpp) 使用 O_PATH 和 fd 路径。这些实现资料没有给本任务覆盖所有受支持存储层的精确大小写保证。上述不足是源码推论，不是对该机的 native 实测；Android7.0指定 tag 页面本轮未读取成功，不拿其它提交冒充 API24 精确实现。

因此没有采用 catch Unauthorized 后继续、只检查产品根、删 Ordinal 枚举、把存在/realpath 相等当完整证明或改变存储位置。这些都不能满足本包的等价保护验收。

## 交主程的具体选择与最小范围

**建议裁定平台信任锚，而非继续猜测同义 API。** 若允许 Android 由 OS/Unity 提供并经 canonical 验证的 persistentDataPath 作为明确系统锚：整条祖先链仍核目录/无链接，系统锚来源与产品根拼接关系逐次核验；系统锚以下的 FightMatch、resource-state/v1 与九叶继续精确枚举/大小写/布局保护。系统锚以上的名字由平台来源承担，不再由应用枚举证明。风险/语义差量必须写明：上游名字的独立大小写证明改为信任平台返回值，**这不是当前合同下已证等价的实现**，更不能冒称完全没有信任模型变化。批准前不得写放行代码。

若主程接受这一具体设计方向，最小交付仍可先限定原四源（实际力争 Store/StoreTests/PlayerHost 三源），内部传递明确系统锚，Android 专属私有原生检查留 Store 内，无新平台文件/公共 API/依赖、无存档迁移，Mac/Windows原路径不变；需主程在同任务记录明确批准上游证明责任以及对应验收。若要求复用 Platform 内部方法而非私有实现，则需另签 AndroidLocalSaveStorage.cs 和程序集可见性变更，不能隐含加入本包。

**若必须保持上游每段独立精确名字证明，继续失败关闭。** 需要先给出/验证不依赖父目录列举、适用于 API24+实际存储层的原生名字证明机制；现有 SystemRoot/Check 不够。单纯扩大文件白名单或 mock 一个“canonical成功”并不能补上证明。可由主程只激活一次有明确问题的 SYS 设计裁定，问题限定为“Android 受限系统祖先的身份/大小写证明责任”，不扩成存档重构或新验证框架。

## 回归、身份和未执行边界

本轮没有批准的可通过条件，故未制造会把删除守卫判绿的新测试。旧 AS14 已在真实 Store/延迟迭代接缝证明异常会拒绝，继续有效。设计选定后，先在同一生产路径接缝写回归：受限祖先枚举永不调用且仍有明确锚/全链检查；错误锚、别名/链接（含祖先与悬空）、属性检查失败仍拒绝；锚以下大小写错误、未知叶、枚举拒绝保持零写入。模拟检查器仅证明调用边界，API24及当前 Android 的 native 保护仍需真实证据；不得以 fake 路径证明冒充 native 通过。原子提交/UnknownCommit/记录格式测试按实际差量选择，不重跑全部历史。

四源已机械核对与 startup-storage01 候选逐字节一致，本轮未修改，前像与收尾后像相同：

| 仓库相对路径 | 字节 | SHA256（前后相同） |
| --- | --- | --- |
| Assets/Scripts/FightMatch/Host/FightMatchResourceActivationStore.cs | 28809 | 6b1c2ec454b0da1c19e58664fafadcabae39af4419cb47c5c0017f40592de639 |
| Assets/Tests/EditMode/FightMatchHost/FightMatchResourceActivationStoreTests.cs | 50822 | 479e97873033f2914d4e98ca100603c3585bb24f86fa3c03d3aad060c92aa60b |
| Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs | 31980 | 04adfaadeab2cdb61bc8ea478e691e505ed45937c97cc460f9f4a988a5d34a84 |
| Assets/Tests/EditMode/FightMatchHost/FightMatchHostResourceTests.cs | 15273 | e6a19129c985b560d8a36c26743630b395453b16bcc9be842aad12812c83d2bd |

补丁适用性 N/A（无补丁）；没有新编译/测试/红绿或产品通过主张。按 SOURCE-only 显式复用原红样本，未重复复现/最小化；零 Unity、ADB、设备/模拟器、构建、下载或 Git 操作，旧证据/候选/共享源未写。仅只读源码/合同/实际日志、官方资料和本结果身份检查。原运行累计 A692.094034167/900秒（余207.905966），B800.433024334/1200秒（余399.566976）不重置、不消耗；后验由主程按实际预算决定。
