# LAYOUT-S1 原生执行准备：事实与待激活顺序

**最新结果：** P实际Unity exit0／16.141989s，native迁移/同进程复开与141目标已完成；原 `P_FAILED` 是唯一新增隔离SceneTemplate设置越出路径门，保持原件。[中央单项例外与主程补核](engineering-layout-s1-p001-reassessment-001.json)已限定接收原生P，设置只留隔离且后续测试绑定其身份，不导出共享。[无Unity四路径导出](../../../TestArtifacts/FightMatch/LAYOUT-S1-001/PExport/receipt.json)已完成并封存：Prefab替换、自然2meta创建、Scene字节不变；C turn `01a0fcae-baa6-7843-a24e-0b428ff7624b` completed，主程13:03:31 UTC／649f6f exit0接收，固定输出已交中央用于现有PR4发布。新head独立审查及T/Android/设备/视觉仍未接受。下文P“待运行”仅为历史。

**当前接续：** I-002已实际运行一次，Unity exit0／79.102686s；原wrapper因清单漏处理 `UNITY_ANDROID` 条件而 `I_FAILED`，保留全部原件。[主程静态补核](engineering-layout-s1-i002-reassessment-001.json)限定Mac导入/编译 `ACCEPT`：20适用项目程序集＋QFramework新产物、冻结输入、自然2meta、GUID与owned闭包均核验通过。Android编译和测试未验证；收尾信号历史因原runner覆盖列表记UNKNOWN。中央授权据补核接续的[P-001租约](engineering-layout-s1-p-activation-001.json)和原I路径[activation](../../../TestArtifacts/FightMatch/LAYOUT-S1-001/I/activation.json)已签派给C实际turn `01a0fc9d-5ab5-7851-9c2a-e0ecf504a55d`，等待一次P≤180s的实际结果；P helper信号记录追加保留。不重跑I、无CUA或设备操作。下文准备时点的不存在/未运行均为历史，不能覆盖本段。

2026-10-02 · `I002_ACTIVATED_P_PENDING_I_EVIDENCE`。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，当前签发turn `01a0fb8d-a348-7982-8eab-61664d7e120a`。原[I租约](engineering-layout-s1-i-activation-001.json)实际在启动前计量门阻塞、Unity启动0，七证据封存；当前唯一执行权限为[后继I-002](engineering-layout-s1-i-activation-002.json)，复用已核993投影输入和17包。中央明确将连续计量/512MiB硬门替换为下载字节UNKNOWN及原时间/落盘/固定依赖约束；下文旧计量条款仅记录原尝试，不再适用于I-002。本页不是替代租约。I实际成功后才串行另签P，继续复用 [SYS I/P/T 合同](engineering-ugui-layout-source-activation-system-001.md#6-后续ipt只待激活) 和 [固定输入](engineering-ugui-layout-source-activation-inputs-001.json)，不重做ARCH/SYS。

## 1. 已核事实与实际缺口

| 项目 | 本轮只读结果／解决路径 |
| --- | --- |
| 候选 | 实际FIX03 head `ee556f2653582534016e371f3d7dce9037de40ca`／tree `665c86084c090fd8cab1f061f1634cb1596f7e58`；全部15源码见 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX03/source-receipt.json#sourceS15After`（SHA `44b8fd8572548ccee8a4c4591275b242f4459e06a04837f58eff70bcbafa042b`）。[QA](testing-layout-s1-pr4-fix03-review-receipt.json) SHA `c67d5479d895e66be6db649afe9ccb00139a1dc27270200028aff202b9c6bc77` 限定代码门ACCEPT，无formal APPROVED；I/P/测试尚未由此验收。 |
| 工具／宿主 | 外盘 `Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity` 为 x86_64，plist 版本2022.3.18f1，SHA `71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f`。只读进程表未见 Unity Editor；WindowServer 存在。许可证文件存在（2559B），未读内容。这些不证明本次启动许可／图形成功；复用 FIX22 Metal 成功仅作历史宿主证据。 |
| 隔离工程 | 固定 `TestArtifacts/FightMatch/LAYOUT-S1-001/projection` 仍不存在。PR1 tree 的 Assets/Packages/ProjectSettings 共990文件／31,644,149B；覆盖 FIX01 15源码后992文件／31,811,476B，另加固定测试CSV。基线所有资产／目录meta齐，无symlink/submodule；布局仅缺已签的2份源码meta。未来用真实Git blob只读导出，不复制dirty checkout，不创建Git worktree。 |
| 磁盘／隔离 | 外盘可用442,368,000,000B。C仍唯一串行Unity执行者；共享工程禁止启动，避免自然生成PR3被冻结的3份meta。隔离排除PR3与LOC Config/Generated/Tools WIP，不移动共享文件。 |
| 包内容 | lock固定51包：17 registry／34 builtin，所有package.json名称版本及依赖名称闭合。17个本地 `Library/PackageCache` payload共9,929文件／394,441,107B、无symlink；34内置包在固定Editor内。仅为包内容清单，不是冷导入验证。 |
| **缓存缺口与有限恢复** | 已查常用全局路径未找到registry元数据／归档缓存；UPM_CACHE_ROOT、UPM_NPM_CACHE_PATH、UPM_CACHE_PATH未设置。`~/Library/Unity`指向外盘ApplicationData/Unity，cache下只见bee/sysroots。未证实离线解析闭包，不再以完整离线审计阻塞首次导入准备。中央随后明确准许：优先复用锁定51依赖及17payload；必要时在同一次隔离I中正常UPM解析并只获取冻结版本缺失缓存，按下方硬预算止损。 |

Unity2022.3的全局缓存包含包内容及元数据，能支持离线安装；已有项目PackageCache不能单独证明新工程就绪。[官方缓存说明](https://docs.unity3d.com/2022.3/Documentation/Manual/upm-cache.html)。本轮没有下载、复制缓存、建隔离工程、启动Unity或读取许可证内容。

## 2. 唯一串行顺序（实际执行受逐阶段租约约束）

1. **准备门。** 中央已接FIX03实际head审查门；C实际turn及唯一收件、全argv/工作目录、日志路径和owned PID/starttime收尾器按I租约绑定。只导出PR1固定tree＋FIX03受审15源码＋固定CSV；外盘projection首次准备≤120s、导出输入≤64MiB。允许的缓存复用例外仅为共享 `Library/PackageCache` 中锁定17个name@version包目录（先后bytes/SHA相同、总payload≤400MiB，无symlink）；只复制到projection/Library/PackageCache，不复制共享Artifacts/ScriptAssemblies/其它Library或Temp。34 builtin直接使用固定Editor。准备后核对应阶段保护项、37资源及预期路径，拒绝额外meta。其它LOC WIP不投影、不覆盖，可按中央独立包并行；不能把其已授权变动误报为布局源码改变。旧缓存文件内容存在不等于本次网络/沙箱/许可/图形能力已验证。
2. **I-002一次≤360s（含一次UPM恢复，不另开预热Unity）。** 原I在网络计量启动前置失败、Unity启动0；I-002独立新证据根，复用已核投影及payload，不重建或重抄。`UPM_CACHE_ROOT`仍单独指向外盘 `TestArtifacts/FightMatch/LAYOUT-S1-001/package-cache`；共享环境变量／缓存／Packages/ProjectSettings不改。不关闭UPM；若预置payload仍不足，则同次正常解析固定manifest/lock，仅向其中既定registry取51依赖中的缺失元数据/冻结版本内容，不添加包/registry或改版本，不运行Client.Add/Resolve重试循环。恢复子预算≤180s（包含在360s内）、新cache≤1GiB、projection生成区≤4GiB。中央已明确取消连续meter及512MiB硬门：不再运行或修复nettop，实际网络字节记UNKNOWN，不把落盘量冒充下载量。首次错误、版本/依赖集合漂移、其它预算触顶即owned收尾，重试0；沙箱/网络能力依据实际调用与工具审批。正常导入／编译自然生成 `Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs.meta` 与 `Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs.meta`。退出0、完成导入/编译/域重载证据、两meta/GUID存在且唯一；15源码、37资源、原meta、Packages/Settings/CSV不漂移，才能填P租约。不得手写meta或自动API更新。
3. **P一次≤180s。** 工作目录是projection；直接固定Editor、StandaloneOSX、batchmode、Metal，执行 `FightMatch.Host.Editor.FightMatchAndroidBuild.MigrateLayoutResources` 后quit。完整argv需与 `I/activation.json`一致；P根入口前不存在，日志不可提前创建P根。现有同步入口完成Prefab原位迁移/保存/卸载→Scene既有实例保存/关闭→同进程只读复开，不另跑Verify入口，不进Play/真实Host。命令行为依据[官方参数](https://docs.unity3d.com/2022.3/Documentation/Manual/EditorCommandLineArguments.html)，不是本轮运行证据。
4. **绑定与交回。** builder目前硬绑定旧 `S/source-receipt.json`；保留该原件仅作历史基线，P租约sources必须是FIX03实际受审head全部15源码，外层执行回执同时绑定FIX03回执和实际head，不能把旧S SHA冒充执行源码。P仅产 `native-migration.json`、`native-reopen.json`、`serialized-targets.json`。37资源中只Prefab/Scene两字节可变，其余含字体/TMP冻结；141个TMP目标、398glyph/4atlas及ViewId/fileID按既有合同核，临时美术仅已签纯色Image占位。交回共享区仅上述两meta＋Prefab/Scene四路径，先核共享前像，漂移即停。native新字节需绑定后续真实head审查。
5. **T仍独立待激活。** P后再冻结实际47叶子名、字面全锚定selector、源码／资源身份及执行环境，最多一次≤360s；不凭12声明称已发现，不重跑222/234/full或额外28。PR3代码审查已接收，但其3meta／编译／Host门另签，可在布局之外独立排序；不为减少启动次数混入本projection。

每阶段首错停、重试0；owned进程自然收尾≤60s，确证身份后一次终止≤30s；日志≤8MiB／阶段证据≤192MiB／余量≥2GiB。只终止本租约进程，不覆盖失败证据。工具/包技能用于区分已核环境与未验证运行；没有把未来步骤当通过。
