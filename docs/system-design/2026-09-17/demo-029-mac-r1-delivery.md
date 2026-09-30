# DEMO-029-MAC-R1：构建交付，设备验收待恢复

状态：**BUILD_READY_DEVICE_PENDING**。按用户最新§457，在必要验证、QA/正常APK构建、证据保存及临时设置恢复后暂停。本报告不构成029整体、首Demo或产品代码ACCEPT；尚未启动独立产品R或后续安卓开发。

作者C：thread `01a0e404-d89d-7ab2-bece-3cd1df3fbc52`，turn `01a0eaf3-31c0-7d83-9dfc-31c24012946f`，startedAt `1790648201`，host `local`。最终进程完成与本报告形成时间见run记录和scope；准确聊天completed由SD00收件确认，本报告不伪造完成时间。

Git起止均为master `9d416e6c9d3c794be355f903b3636c856783601e`；本C回合未作Git写入，交付保留未接收WIP。SD00已有协调文档改动另列，不归作者产品补丁。

## 合同与源码身份

固定计划：`demo-029-mac-r1-plan.json`，354689bytes / SHA256 `cfe1ef8ae52ba4fa9daea475d3f4571d88cdbfe47b87ce89ba7e5a8eb59bd6f1`。原设计与§443、445/446、450～454、456/458的冻结身份均由唯一工具校验；§457仅改变停止条件；§459精确临时设置清理及§460既有APK只读补证只绑定scope/报告，未改冻结工具、root或原计划。

最终root身份：`d5e08e04d029f6cbe18e9125f71882f89506107bc85a45cb6e3923b15652c68c`（71153bytes）；最终工具 `Tools/Invoke-FM029Validation.ps1`：`d2e4d32541ff4b3712578282b2b3f1af703731c241cb7a4b251acb4ae1f5503d`，495行。Editor入口438行。最终被测源码/资产/工具指纹：`a99903bc45fedd9146cdaa6bf321f92b43b8c73d3826d597b5869f73c42cf92a`。逐文件身份、预算、旧输入保全、目标DLL及运行链见[code-scope](demo-029-mac-r1-code-scope.json)。

## 实现范围

- 新Host组合唯一FightMatchDemoArchitecture、原PlayerSession/Navigation和028 battle controller；一次创建保留PreparedProfile，F2沿原请求继续；未知实物根拒绝新建，已有格式沿显式迁移。
- 原HostRequest交原028核owner/revision/head；明确原结算查询走Record.Intent/QueryOperation。返回原结算时消费导航已完成操作；隐藏battle页面重绑不抢导航。根返回经原RootBackRequested入口进入应用退出确认。
- Android适配保持原协议/序列化/UTF16身份。Bionic独立fd/flock、同持有者租约内CreateNew/提升、仅自建流Flush(true)；仅真实争用映射Busy。不宣称内核no-replace或断电事务保证。
- Android StreamingAssets使用有界UWR接收原六件发布物；保持原内容准入、参考和规则。UIDocument、PanelSettings、主题、动态Noto字体/内嵌材质图集及OFL均为明列新资产。
- 新Host测试覆盖生命周期、原创建/恢复、路由、真实面板输入和资源；Android QA包保留八个隔离阶段、双PID锁及四个持久化断点恢复驱动，尚未在设备执行。
- 正式设置只变五项：Android包名、IL2CPP、ARM64、Minimal、TargetSDK32。Min22、0.1/code1及其余设置保留；旧858资产、828实现文件、六件发布物、Packages/锁和旧验证入口原字节不变。

## 实际验证

| 槽 | 阶段 | 结果 | 实际Unity耗时秒 |
| --- | --- | --- | --- |
| 014 | Compile | 通过 | 15.916 |
| 015 | HostTests | 通过；30/30 | 470.153 |
| 016 | FullTests | 通过；4041/4041 | 6865.963 |
| 018 | QaBuild | 通过 | 254.235 |
| 019 | DemoBuild | Unity构建成功；包装误报失败保留；§460结构/签名补证通过 | 181.086 |

完整回归包含全部旧4011具名出现及原3956多重集合；新增Host实例为30。Mac测试未执行Android QA；Android构建产物的DLL另记，未把不同目标DLL当同字节版本。

013原失败：30项中27通过、3失败，0跳过，450.1465627s。XML仍为29814bytes / `86ef662ac627fe13423cfaa0009b97ec53ddef499c79d419d72d29d86607c57d`。014/015验证修正后版本；013证据未覆盖。

017首次QA因Maven Central依赖TLS握手中断而失败，Unity退出3、APK/BuildReport未产出。UTF已自然清理临时场景，但直接退出跳过延迟恢复；工具据正式设置残留判失败。C保存失败设置完整字节及原restored:false，在确认Unity/Bee/Java自然结束后精确恢复captured-before。私有凭据文件均核同UID/0600且公开输出无凭据；同JDK只读探针随后HTTP200，018沿原配置重试成功。此为独立失败及人工收尾证据，不回填017为通过，也不重复016全量。

019正常APK的Unity退出0且BuildReport为Succeeded；原包装因原始二进制字符串包含FightMatch.Core.Tests而失败，result473bytes/SHA`d5f47f76adcec7527ab521f79e79df83d1928a5ca865eb88f2a1ea8891c82c79`保持。§460补证按本机Unity2022.3.18f1的v29布局核全部表边界、所读索引/字符串、image与assembly映射及类型覆盖：正常37个程序集、4771个类型，禁止定义/QA类型为0；Core.Tests的3处及UnityEngine.TestRunner的13处原始命中全在attributeData区。三处既有友元声明原字节不改。相同解析在018实检58个程序集、6986个类型，命中NUnit/TestRunner/FightMatch.Android.Tests三程序集及395个相关类型，形成正对照。六件发布内容在两包内均与冻结原件同长度/SHA。

完整正常/QA image、assembly、type清单、原命中偏移、表大小和SDK文件身份，以及可直接复核的只读Python辅助脚本原文，位于`TestArtifacts/FMDemo029/mac-r1/scope-audit.json`的`normalApkStructuralSupplement`。将其`parserSource`提取至临时.py并用本机python3执行可复核，运行本身不写文件；源码脚本SHA与结果已记录。签名/Manifest实际命令及输出也在同字段。补签前追加到019 BuildReport的两项补证已迁入scope，019原BuildReport精确恢复并核56571bytes/SHA`f34944eaad648ea3ac134ae78987c7159addfb59153f93f70c7963efe722ca0b`；原运行、失败、快照、APK不变。**冻结验证器旧字符串判据本轮未修复，019包装仍失败**；后续独立审查及最小修正须处理，不能把补证记作旧包装通过。

013三处原因和最小修正：根Back原调用链在捕获请求后又递增revision，新Host改用原公开RootBackRequested请求；冷重开原结果后导航仍持有已完成operation，桥接取得有效receipt后调用原Return；Restart原语义返回Battle且携带receipt，测试改为核此语义后执行真实Exit，再冷重开原Restart receipt。增加刷新和整树重绑后仍留导航、保存叶不变断言。028已接收源码不改，30个用例均保留，无临时筛选、跳过或弱化业务要求。

资源验证011实际保存并重新打开场景，核UIDocument的PanelSettings、宿主字体及许可引用；场景只修复PanelSettings绑定并保留原GUID。007额外产生的四个Unity默认主题叶依§452保存原字节/身份后精确清理，其余资源失败和恢复记录保留。

运行次数：PrepareAssets5、Compile8、HostTests2、FullTests1、QaBuild2、DemoBuild1，共19/20槽。总槽限制20；各模式最新上限Prepare5/Compile8/Host3/Full2/QA3/Demo3。90分钟完整回归阈值仅只读诊断，不杀进程、不重启或重复未变化的已成功阶段。

## APK、配置与签名

| 包 | 实际路径 | bytes | SHA256 |
| --- | --- | --- | --- |
| QA | `Builds/FMDemo029/mac-r1/018/fightmatch-qa.apk` | 34676164 | `912d4202b144cee261ac8f47bea8d56b8dd7fa31338583d3852c9fdf31860679` |
| 正常Demo | `Builds/FMDemo029/mac-r1/019/fightmatch-demo.apk` | 32776566 | `cfdc39d87a4f0152230594eb5dce9ed582e332fb91416c17d83157a749628094` |

两包公开证书SHA256：`2045e6c0b3dd319ae5682d10701ffcdd2337fdf5a539c7a065d1d7c8024bac51`。实际apksigner校验成功，APK实际manifest和native-code核包名/0.1/code1/Min22/Target32/ARM64；BuildReport核IL2CPP/Minimal。正常APK使用Development；其BuildReport另有Unity内部CompressTextures/StripDebugSymbols/ForceOptimizeScriptCompilation/Il2CPP标志，无IncludeTestAssemblies或回连/脚本调试标志。§460结构补证核测试/QA排除及六件发布内容原字节；QA包限定测试程序集，去除自动启动/回连/等待/脚本调试/Profiler连接。APK未改包、重签或上传，不进入Git。

QA自然UTF临时场景/元文件的创建身份、清理及Context.Dispose结果见本槽temporary-settings；两个构建的临时签名、包名、graphics/平台、场景和设置字节恢复均有实证。私有签名只保留外置原key/pass，0700目录/0600文件。

§456规定的两个私有缓存根始终0700；Unity继承umask077，专用Gradle禁用常驻daemon，实际含凭据文件仅在批准的Bee/Gradle私有根内，并核同UID/0600/无ACL。公开输出及展开APK条目的凭据排除扫描结果见scope-audit；不交付缓存正文、口令、私钥或其摘要。

§454允许的Unity旁路备份按实际文件系统盘点，不把BuildReport列出的Library内部缓存当作已分发旁路备份。实际017/018/019三个备份根均Absent，备份文件数与字节均为0；逐槽盘点见 `runs/017/build-report.json`、`runs/018/build-report.json`、`runs/019/build-report.json`。

## 保存与范围核对

作者范围为55个新Assets成员、5个其他新文件及唯一原ProjectSettings文件，共61条明列Git路径。913个最终Assets、868个实现成员、482个GUID均核对；旧GUID及原输入保持。009资源准备期间新增的SceneTemplateSettings精确叶按§459先保全3534字节/SHA与完整原文，待全部必要Unity/构建退出后核父目录/同UID普通非链接/原字节一致，仅删除该叶并核Absent。原unexpected记录保留，不将其纳入永久配置或作者Git范围。

原测试物理根最终4276个GUID用例、20038个文件/链接、32393个总条目；Host根69个GUID用例、448个文件/链接、823个总条目。旧3719用例/28188条目保全，无清理扩容。

证据根`TestArtifacts/FMDemo029/mac-r1`：严格按原507个精确叶保存；read-manifest仅排除自身，Host物理GUID用例由独立host-io前后盘点约束。每次Unity PID/UTC/argv/退出、源码/工具/目标、日志/XML/BuildReport和失败均保留。源文本副本保留原字节，二进制资源只记身份。

## 待验门与接续

本次暂停原因是用户§457，不把已连接的模拟器描述为不可获得。BlueStacks曾实读Android13/API33/ARM64，04:28:11Z两产品包未安装；本回合未安装APK或写入正常玩家数据。iQOO未连接。

H01～H04仅有Mac宿主/完整回归证据；H05资源引用与Mac面板通过，Android中文/安全区/触摸待验。A01平台实现待实际Android存储；A02八阶段及双PID恢复未执行；A03构建与内容检查不代替IL2CPP运行时反射/AOT验证。V01/V02以本报告实际结果为准。

接续还需独立复核§460补证并最小修正验证器误报。M01～M10设备门、P1胜利结算→P2同资料重开/重打/Restart/Exit/非终局保存→P3同Attempt续行动、WPS真实进程冷启动、iQOO最终门和独立产品R均未完成。M09当前L1不可达路径保持NOT VERIFIED；未注入胜利、存档、HP或随机状态。

恢复时保留同APK/包名/证书及资料，先核设备/既存安装，再完成隔离QA和正常输入见证；设备事实与模拟器事实分别收件。由SD00确认用户恢复后安排独立R及后续版本保存。本C不新开设备长验收、产品R、后继开发或Git操作。

玩家说明：[PLAY_FIRST_DEMO.md](../../demo/PLAY_FIRST_DEMO.md)。独立R咨询§455仅处理本地签名缓存合同，已完成，不能作为本产品审查ACCEPT。
