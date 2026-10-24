# LOC-B-PREF DIAG01：只补发现失败诊断

2026-10-03 · APPROVED_FOR_SOURCE · issuer/唯一完成收件：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a1000b-6980-78f3-bbc5-a8a31fd972ae`。用户授权为 AGENTS §6 的既定团队通信／获准开发范围；中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 本轮明确批准本诊断、下列≤35增删行和七叶范围，并正式将责任移交给现源码作者 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，显式 `gpt-6-astra/xhigh`。原作者旧C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` 保持停用；其原PREF turn `01a0f71f-8a14-7bf3-afcb-e92eac92bdf9` 历史不改。新实际turn由接收者绑定；作者先记录before再写源码。

## 输入与范围

- `R` 为当前 FightMatch 仓库。唯一源码前像 `Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs`：476行／30074B／SHA `ef47caf6bbf7b7715f625bc8bd384e82f1dda064b7a9f048c282fb86ac46ea8d`，对应 PR3/head `6818405b859d71bf8c0f116299e7e9605a3d769a`。仅其现有 `LocalePreferenceDiscovery` shim 内可增量修改；其前面的 fixture、参数源、helpers、生命周期和全部断言逐字保持。整文件上限511行／40KiB；相对前像新增＋删除行总计≤35，不压行、重排或改换行规避预算。
- 限读附加固定输入：`Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs` SHA `e768e6e39fe0f28ec292032ff9986f79606136ad6473b43fb6d5c550487e55a0`；同目录 `FileLocalePreferenceStore.cs` SHA `d392d6808ad1fc8156482020951404eb46390d462ce0164404180dea067cce13`；`TestArtifacts/FightMatch/LOC-IMPL-B-PREF/source-activation-001/source-receipt.json` SHA `f24cc4da2581c90d31dd897f8d1f70cdaa4b4813ce2ae983bbfbe2fb8386ab4c`；`TestArtifacts/FightMatch/LOC-IMPL-B-PREF/M01/receipt.json` SHA `d0af4c5f46eeb559bb3399f677e9b8b095c2f9e9b701936401d01669124d9105`；其 `D/editor.log` SHA `0f39df1913ced206e4ffeeea7edf8ca7faec00f362c98ed0e6ce699137e58318`；共享只读 `Library/PackageCache/com.unity.test-framework@1.1.33/UnityEditor.TestRunner/Api/ITestAdaptor.cs` SHA `8a0efe99d6e1942fdc41931498d9fa99bfbc739fe4a131c5ba40e17965e6b7f4`（已核存在 `FullName/RunState/SkipReason/Children`）；及本合同。无需旧40537B方案、全局历史／包树扫描或再开SYS。
- 新stage `S=TestArtifacts/FightMatch/LOC-IMPL-B-PREF/DIAG01/S` 已核缺席。仅创建七叶：`S/Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs`、`S/before.json`、`S/after.json`、`S/source.patch`、`S/verify.py`、`S/static.json`、`S/source-receipt.json`。候选只写stage，不写共享原件；无meta/asmdef/其它源码、产品配置或资源变更。七叶总≤256KiB；验证器≤180非空行／24KiB，沿现有小型静态检查模式，不建通用框架。
- 原失败 M01 已 D1/exit3/T0，owned闭合；日志只报 `PreferenceDiscoveryFailed: InvalidDataException`，未证实具体分支。RAW74 当前在独立 P01 投影运行。本source任务不读取或写入活 `TestArtifacts/FightMatch/RES-01A/P01`（包括其缓存及O）、旧发现O、旧source/M01证据或任何下级活证据；上述共享UTF接口是静态API参考，不是运行DLL证据。

## 唯一改动及验收

1. 补有界诊断，使未来一次真实发现日志能区分：零匹配、非Runnable、重复fullName、缺失children。至少记录匹配数／非Runnable数／重复数以及少量有助定位的name、runState、SkipReason；零匹配时也须能看到有用的树计数或相关名称样例。诊断只使用发现API元数据，不执行测试、不读取玩家数据／文件内容。
2. 一次发现额外诊断最多8条／16KiB；样例总≤3，每个name／SkipReason截断≤240字符并消除换行。按边界汇总，不逐树节点无限输出。只为诊断保存最小本次遍历状态；保持原递归遍历、匹配前缀、排序和四种失败判定。现有异常抛出、catch后exit3、成功exit0、opt-in参数校验、无-runTests／无-quit限制与路径／链接／CreateNew保护不放松。
3. 原 `Leaf/Leaves/DiscoveryReceipt` 输出字段、JSON写入、成功行为、输出目录常量逐字保持；诊断仅进入Editor日志，不新增文件或泄露任意异常/数据正文。允许shim内部为传递有界诊断状态做最小签名改动；不得更改fixture或其helpers，不因猜测根因调整prefix/runState/去重/fixture/参数源/测试断言。超出35行或需要行为修复即BLOCKED，交主程，不自行扩权。
4. 作者逐项自查并写明确“SOURCE_READY / UNCOMPILED / DISCOVERY_NOT_RUN / TESTS_NOT_RUN / REVIEW_PENDING”。原M01失败仍失败，不能用静态检查宣称已定位或修复；PR3原no-major只属于原head，不覆盖本增量。

## 验证、封存与返回

先对以上八个固定文件（含本合同）取bytes/SHA且确认S缺席，写before的owner/host/实际turn、原owner、批准来源。最多两次 `python3 S/verify.py`，每次≤20s，仅新stage的文本、哈希、diff与范围检查；无C#/Unity/SDK执行、discovery、测试、联网、Git写操作或旧runner运行。所有尝试及失败保留在static.runs，不能通过缩减验收得到绿灯。after再次核八输入不变，候选与patch精确可重建、只有shim差异、≤35行／文件预算／fixture全前缀和原判定/DTO/入口/出口保护保持；原字节和raw文件不复制到新增大型快照。

receipt列七叶身份（自身列bytes/SHA由最终回传绑定，避免自引用）、源码前后SHA、diff统计、逐项验收、自查内容、每次静态命令/耗时/退出码、执行均0、未验证项及实际owner。封存后停止写入，沿AGENTS §6授权路线只回主程，主程机械收件后交中央安排唯一GitHub审查；无本地R/重复review。未来实际D1/T0必须排RAW74正式安全收尾后，另签准确输入与输出租约；本合同不激活该运行，也不准写旧O。
