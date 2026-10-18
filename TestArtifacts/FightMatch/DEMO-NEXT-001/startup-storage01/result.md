# DEMO-NEXT-001 / startup-storage01

2026-10-18，SOURCE_READY / UNCOMPILED / UNTESTED / REVIEW_PENDING。整体仍 **NEEDS_FIX**；本候选只补安全诊断，未定位或修复真实 Android 存储故障。

作者 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，actual `01a14d4e-b656-73d0-89ba-4f2c0d825a71`；唯一签发/完成接收者主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，issuer actual `01a14d4d-036b-7c53-824a-8b1752d138bb`。权限来自中央真人 turn `01a14d27-e460-70b1-ab5d-a5ed1fa6aefd` / item `01a14d27-eb36-79a0-9a2b-ba591af92a3d` 的“许可”及主程本次 SOURCE 白名单；团队内回执依据 AGENTS §6。

## 证据与判断

- 复用 [startup-run01](../startup-run01/result.json)：已审 `1d75386b2ba30e6746cb0123b12e86dfa70f4907` 四源已接入，8项定向通过、无失败/跳过，普通 APK 构建成功。新包 SHA256 `e50b711ecbebc7d380fa62d1abd5d7bbedfda0760b292eba2cf6eb17ea8b2480`。旧审查及测试不覆盖本次新诊断差量。
- 唯一真实红样本为 [emulator-02](../emulator-02/result.json)：日志第191行，PID11584，12:32:02.768，`FightMatch UI: RES_STORAGE`；已实际查看截图，显示诊断占位符及该码。L1、三状态恢复、奖励均未运行；六份可读业务文件 SHA 不变，两个锁文件不可读，不能外推所有存档字节。
- 已有只读结果为 `resource-state` 和 `resource-state/v1` 缺席；产品 FightMatch 根存在。不能把技术状态父目录缺席写成产品根不存在，也不能由 shell 可访问性推断应用 UID 权限或 Host 实际 canonical root。
- 确定调用路径：Host 在资源工厂后执行偏好根校验、产品目录创建、再次校验、Store 构造/Enter、Inspect/Prepare/Bridge；Store Execute 的普通异常及 Host 普通异常均汇聚为 RES_STORAGE，finally Dispose 异常也可能使 loading task fault 后显示此码。旧日志只有 ShowFailure/InitializeHost 栈，没有原异常或失败操作。
- Store Load → Layout → DirectoryGuard 确实检查至文件系统根，并枚举每层父目录作大小写/链接防护；`Directory.EnumerateFileSystemEntries` 的异常也可能在迭代时发生。祖先枚举受限仍仅为候选，不能据此删除守卫或改变可信根。
- 可区分的旧分支：Android 的 OpenResourcesAsync 内 VerifyLocaleStorage 只作根字符串一致性校验；最初 canonical root 解析失败走 HostSaveIsolationRejected，进入 Session 后的存档适配异常走 HostInitializationFailed。这些不是当前所见错误码的直接报告分支，但不证明相应平台操作已全部通过。没有 RES_STATE 也不能独立证明 JNI/重启语义。

原始异常无法从现有证据恢复，因此未作根因修正。日志 SHA256 `4fc2c2b7b4fa9b0138a91fbe5ba26eb5d9978cf1174dbb1d7a4bfaf7912c2d28`；截图 SHA256 `9961c2dc2679f35d026f77e6c023db403aa568db594e6cb24a3af2e4eceeb4f7`。旧现场与 startup-fix01 冻结产物未修改。

## 候选及准确身份

只新增本目录六叶：本 result、source.patch、下表四份完整暂存 source。共享 Assets 未写。表中路径相对仓库根；候选位于本目录 `source/` 下同路径。

| 路径 | 共享前像 SHA256 | 候选 SHA256 | 候选字节 / 差量 |
| --- | --- | --- | --- |
| Assets/Scripts/FightMatch/Host/FightMatchResourceActivationStore.cs | 6c20d3d9a2727373e695c5f42cde9e2727c763c4724640012d9c4f1d4402d4b7 | 6b1c2ec454b0da1c19e58664fafadcabae39af4419cb47c5c0017f40592de639 | 28809 / +63 −9 |
| Assets/Tests/EditMode/FightMatchHost/FightMatchResourceActivationStoreTests.cs | 12f9852205a9660819dfcb3737e47a55b09f92224514bbbbed60073e2def78f5 | 479e97873033f2914d4e98ca100603c3585bb24f86fa3c03d3aad060c92aa60b | 50822 / +78 −0 |
| Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs | 77b7a7ca590e7ee704d0bee2aed66536436c038b6dbf702b234c2c54304ee412 | 04adfaadeab2cdb61bc8ea478e691e505ed45937c97cc460f9f4a988a5d34a84 | 31980 / +29 −4 |
| Assets/Tests/EditMode/FightMatchHost/FightMatchHostResourceTests.cs | 1cc7091387b37a4b8e91615b5ce2d9f50f31e644597920aa307882bae7d75037 | e6a19129c985b560d8a36c26743630b395453b16bcc9be842aad12812c83d2bd | 15273 / +39 −0 |

[source.patch](source.patch)：24900字节，SHA256 `d84054d945b47afdafe0f6221d38c02c5e158d3f6a93fadda9a4eb4214089128`。

Store 仅记录固定操作阶段、ancestor/product/outer/state 级别及受控异常类别。未知异常归 OtherException；不使用异常 Message、ToString、动态类型名、路径、标识或资料内容。提交 catch 在清理之前保留诊断；下一次普通操作清空旧诊断，UnknownCommit 保留原诊断且仍禁止重试。Host 对 RES_STORAGE 输出这条有限诊断；Host 自身异常及 Dispose 单独标阶段，日志使用 NoStacktrace，UI 仍只接收原安全码。Dispose 原异常继续传播，取消和失败关闭语义不变。示例 `FightMatch resource startup: stage=activation.guard.ancestor.entries; error=UnauthorizedAccessException` 只是预期格式，不是本轮实测结果。

没有新公共 API、序列化字段、依赖、资产或通用诊断框架。v2/严格后继、九叶/2048B、目录/链接/大小写检查、原子写入、备份及 UnknownCommit 原规则保留。诊断是受控托管阶段，不采原生 errno、绝对祖先路径或 JNI 明细；它不能替代 Android UID/真实文件系统验证，异常展开期间的二次异常仍可能遮蔽先前异常。

## 检查与后验

先新增回归候选，后改生产候选；因 SOURCE 禁止执行，本轮没有新增用例的实际红/绿结果。AS13 通过原 MemoryFiles 故障点检查原操作、清理后诊断及字节保护；AS14 在真实 Store Execute/迭代接缝抛出延迟异常，覆盖四层级及受控/未知类别，并检查零新增节点/写入；AS15 检查 UnknownCommit 的原诊断及禁止重试。H07 是支持 Mac/Windows Editor 的 UnityTest：读取既有固定 boot，经真实工厂进入 Host 的实际异常捕获，在目录创建前拒绝缺失 preferenceRoot，断言安全日志、RES_STORAGE 和没有 Session/activation；没有增加生产注入框架。它不证明 Android 权限行为。

实际标准静态检查：四源前像 SHA 匹配；正反 `patch --dry-run --batch --fuzz=0` 共8次均 exit0；新增尾随空白0；两测试文件仅插入，既有断言无删除。令牌、转换、Canonical、Decode/Encode/Relations 源段逐字节不变；去掉诊断赋值/必要局部变量后，目录守卫、目录创建、Commit 控制流和文件操作文本一致；Host 的 OpenResourcesAsync 之外逐字节不变。公共/serialized 声明不变。以上是文本检查，不是编译或行为测试。

建议主程只安排以下6项受影响定向（此前8项按其原输入复用，不宣称覆盖候选）：

```text
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS04_PrePromotionFailuresPreserveActive
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS05_UnknownCommitNeverAdvances
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS13_StorageFailureReportsTheOriginalOperationBeforeCleanup
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS14_DeferredEnumerationFailureReportsScopeWithoutLeakingExceptionData
FightMatch.Host.Tests.FightMatchResourceActivationStoreTests.AS15_UnknownCommitKeepsItsDiagnosticAndDoesNotRetry
FightMatch.Host.Tests.FightMatchHostResourceTests.H07_ResourceStartupReportsSafeStageAndKeepsStorageFailureClosed
```

随后仅由主程按剩余额度安排 C 的必要接入/编译、诊断包和保档启动，采同一 PID/新 APK 身份的有限阶段日志与错误画面，保留旧失败。即使出现 ancestor.entries/UnauthorizedAccessException，也需据实际证据确定后续最小安全修正，不能自动放宽守卫。若没有诊断或出现 OtherException/ReturnedCode，按实际阶段再缩小，不猜通过。新差量需 GitHub PR Code Review；本作者不自批。

本轮零 Unity/ADB/设备/模拟器/构建/下载/Git 操作，无运行预算消耗。A 已用404.808553秒、剩495.191447秒；B 已用354.195276秒、剩845.804724秒，上限不重置。只向主程回唯一完成收件并结束空闲回合，不派下游或重复同步中央/QA。
