# startup-fix01：Android 进程身份与绑定前诊断候选

2026-10-17。作者 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，本阶段 actual `01a147b9-e6e1-7f92-8162-40cf4c0376e5`；唯一主程收件 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，签准 actual `01a147b6-e72c-7d03-b8b3-d7445e3ca6ee`。依据 engineering.md 顶部五条技术差量批准及 AGENTS §6 持续自主推进授权。**完整四源 SOURCE_READY / UNCOMPILED / UNTESTED / REVIEW_PENDING；整体 NEEDS_FIX。** 未应用共享文件。

诊断阶段 actual `01a147ac-ff60-78c2-b804-1181bc7587d9` 的事实保留：当时只准备 HostView/HostResourceTests，四次正反 dry-run 成功，令牌改动因 UTC 契约未定暂停。旧两源补丁为 6099B / `4077e84d390f413989c5f5b0139270d3e8a760514fb09376a153907d3905a420`；本次单一 source.patch 保留该前缀并追加 Store/StoreTests。原两份候选逐字节冻结，技术暂停由新五条契约解除。

## 诊断依据与运行边界

复用 `../emulator-01/resume-result.json`、`install.txt`、`logcat.txt:190` 和已查看的 `issue.png`：固定 APK `4d72f265bd437d9d0e5131128ae46f71fb4fd968310f3a2d93344a791c017ab6` 覆盖安装成功，普通图标启动报 RES_STATE，resource-state/v1 不存在，画面是诊断占位。没有新运行/复现，设备实际异常本体及修后闭环未验。

确定静态链：既有 Android 生成物 `Library/Bee/artifacts/Android/il2cppOutput/cpp/FightMatch.Host.cpp:24819` → `System__2.cpp:21453/21411` → `System__1.cpp:4935/4887` → 匹配 Unity2022.3.18f1 的 `Unity.app/Contents/il2cpp/libil2cpp/icalls/System/Microsoft.Win32/NativeMethods.cpp:29–33` 不支持 GetProcessTimes；`il2cpp-config.h:304` 宏抛 NotSupportedException。RealToken 捕获后返回 null，Execute 先拒 RES_STATE，早于 Load/目录创建。源码不兼容与红样本一致，不把静态链冒充已采设备异常。

Host 在 Enter 成功后才准备资源、本地化和 Session；原 RefreshDiagnostic 仅在 bound 时绑定正文，所以占位并非已证实的翻译缺项。按本阶段运行禁令，只复用红样本，未执行诊断技能的复现/绿测闭环。

## 已签技术差量与实现

Android 使用 `a2-b<bootCount>-p<PID>-e<startElapsedMs>` / schemaVersion 2；JNI 读取公开 BOOT_COUNT、myPid、getStartElapsedRealtime、elapsedRealtime，前后两次 boot 必须一致。BOOT_COUNT 采用无默认值的 getInt，缺失/非法值抛错并受控拒绝；数值检查为 b∈[0,Int32.MaxValue]、p∈[1,Int32.MaxValue]、e∈[0,Int64.MaxValue] 且 start≤elapsedNow。第一次由已有 Host 主线程启动路径取值，Lazy 缓存成功字符串或失败 null；JNI 包装均 using 释放，重建 store/Activity 不重新采样。依据：[Process API24](https://developer.android.com/reference/android/os/Process#getStartElapsedRealtime())、[BOOT_COUNT/getInt](https://developer.android.com/reference/android/provider/Settings.Global#BOOT_COUNT)、[SystemClock](https://developer.android.com/reference/android/os/SystemClock#elapsedRealtime())、[Unity JNI](https://docs.unity3d.com/2022.3/Documentation/Manual/android-high-level-api-code-examples.html)。

后继仅 boot 增加，或同 boot 的 start 增加；普通 Enter 和 pending 消费都检查。完整 token 相同仍是同进程，同 boot/start 不同 PID 与倒退均拒绝；pending 同时要求晚于 active 和 restart 的进程身份。MarkReady/Prepare/RequestRestart 的精确当前进程归属不变。测试注入入口用于内存状态回归，不是生产平台选择或真实进程证据。

七字段/顺序、2048B、物理 resource-state/v1 九叶、目录守卫、原子提交和 UnknownCommit 保持。读取所有主/备记录时校验 schema/token 族与本实例族；Android 的旧 v1 或混合族、桌面的 v2 均 RES_STATE 零写，不迁移/清理/自动回退。非法语法/越界仍拒绝。桌面继续原 pPID-UTC / v1 编码，原黄金字节断言保留。旧程序不能读 v2，源码回退不代表技术状态可回退。

证明域仅同设备/Android用户/应用的既有受守卫资源根，不证明跨设备复制/备份/系统镜像恢复；未读取/存储/上传 ANDROID_ID，未加入 scopeHash、墙钟/随机 fallback 或新的迁移策略。玩家存档、资源目标/阶段和 HostSession 不变。

已冻结诊断候选仍为 +10 行：pre-bind 时显示原 Placeholder＋最多64个ASCII字母/数字/下划线组成的安全 code；路径、富文本、换行和过长值只保留占位。正常 Bind 仍原本地化，Unbind 清理，无新失败状态或完成文案。

## 唯一输入/输出表

仅 `source/` 下四个原相对路径候选、source.patch 和本文，共六叶。共享 Assets、Prefab/Scene/meta、字体/设置/包、玩家档及旧证据未写。

| 文件 | 当前输入 SHA256 | 候选 SHA256 | 候选字节 | 差量行 |
| --- | --- | --- | ---: | ---: |
| Assets/Scripts/FightMatch/Host/FightMatchHostView.cs | `96c23d88df271120057efde2eea399c89c03606ed0b29c937e2172f472dbf52e` | `4e9aeb71f4afbdb32544daed859d204e7ce34886e8fdf68ab4ccc31cdedcba5e` | 24239 | +10/−0 |
| Assets/Tests/EditMode/FightMatchHost/FightMatchHostResourceTests.cs | `8caf1736ee6e28857ec98234de07264a3101e078cddebda8624c6428404cb1f0` | `1cc7091387b37a4b8e91615b5ce2d9f50f31e644597920aa307882bae7d75037` | 12207 | +65/−0 |
| Assets/Scripts/FightMatch/Host/FightMatchResourceActivationStore.cs | `94a86aaf5413f91913492486e86f03113435ada247558044535c9787cc5c8155` | `6c20d3d9a2727373e695c5f42cde9e2727c763c4724640012d9c4f1d4402d4b7` | 25600 | +82/−14 |
| Assets/Tests/EditMode/FightMatchHost/FightMatchResourceActivationStoreTests.cs | `572c46a5c37734fe6cb8db4ec9b3a0c0c133ce7284ce6e0458451e6bcf9cb029` | `12f9852205a9660819dfcb3737e47a55b09f92224514bbbbed60073e2def78f5` | 45913 | +140/−0 |

合并 `source.patch`：28328B / SHA256 `1d6111a6e6b763838645c74fa5fc0ee5c3c43f992e52845335c87c08b3b4b576`。

## 静态检查与最少后验

四文件标准 `patch --dry-run --batch --fuzz=0` 正反八次均 exit0；原两源补丁前缀和候选身份保持，四共享源仍等于表中前像。新增尾随空白0，public 产品表面和 serialized 声明不变；原 StoreTests 全文为纯插入，原八例、v1 Json 黄金字节及故障断言未改。目录/有界读取、Relations、CreateDirectories/Commit（含备份、清理、UnknownCommit）、MarkReady/RequestRestart 原方法逐字节保持。没有新验证器或台账。

新增 StoreTests AS09–AS12 分别覆盖采样/规范数值、普通重入的同进程/后继/PID复用/跨boot、pending 同进程/歧义/倒退与备份、旧v1/混合主备/schema族零写；H06 仍覆盖真实 Prefab 的 pre-bind、绑定后双语及解绑。以下八例是建议最小自动后验，尚未执行，运行合同由主程另签；AS01–AS03补验受影响桌面编码/阶段，其余原验证按输入边界复用：

```text
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS01_InitialBuiltInAndOrdinaryReentry
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS02_UpdateRequiresNextProcess
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS03_StrictBoundedRecords
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS09_AndroidSamplesAndCanonicalTokenBounds
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS10_AndroidOrdinaryEntryRequiresStrictSuccessor
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS11_AndroidPendingRestartRequiresStrictSuccessor
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS12_AndroidRejectsLegacyMixedAndMismatchedSchemasWithoutWrites
FightMatch.Host.Tests.FightMatchHostResourceTests.H06_PreBindFailureShowsSafeCodeAndClearsOnBindAndUnbind
```

JNI 实际成功/失败缓存、API24设备、冷进程/Activity重建、整机重启与文件提交能力均待真正后验；注入 token/样本测试不能证明这些事实。Mac/Windows 原生保持也不冒称新输入已运行。之后再验普通图标资源启动、诊断画面及 L1。此次 Unity/设备/ADB/模拟器/下载/构建/Git 操作均0，不重复旧head审查；候选冻结并回主程，中央/QA后验与审查由主程统一协调。
