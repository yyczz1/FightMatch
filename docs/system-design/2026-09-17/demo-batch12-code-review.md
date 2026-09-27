# FM-DEMO-016 / DEMO-B12-R1 独立代码审查

VERDICT: NEEDS_FIX

范围与原始验证证据通过；规格轴有一个 P2 诊断协议缺口 F1。当前头的 I/O 读取失败仍被包装成成功观察，Prepare 随后丢失原异常。写门保持关闭，但尚不能接收 016 或开放 017。

## 门槛、依据与审查方法

- R：`01a0c1cd-dce1-7ac3-8780-06163cb0acfc`；本次原复审 turn：`01a0c520-2e9e-7e82-9337-1d5b689c898c`；local，gpt-6-astra / max。
- C：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；本次原实施 turn：`01a0c51f-7ccd-73c3-84cb-ddd4eaa629be`。工具确认其 final_answer 与 completed，完成于 2026-09-21 18:46:26 UTC，error=null；没有借用旧 C1 收尾回合。
- 契约：r81 §§143–148，正式范围／接口／验收按 §§144–147，审查按 §148；派发整稿 SHA=`97002e90ce13fd2a8b94b68432f9ee7796b9cd2e630d83cadac7aef31d622e8d`。后续状态／回合登记不改变该契约。
- 作者交付实读：`demo-016-delivery.md`，88 行，SHA=`fda9b73aec7a6ceb38ee8bb8f3c1fbe4899327c4c648158233b6ded8d97c4192`。
- 作者 scope 实读：`demo-016-scope.json`，588520 字节，小于 600 KiB，SHA=`f96435dc84d05cf185ac7d4c1837ff9867470f180c5c4c3ec2b752defc0558bf`。
- R 于 18:02:23.9140523 UTC 独立固定原 471 项、旧 asmdef 完整字节、260 meta／GUID、488 个原 Assets 路径和 477 个受保护输入。正式门槛满足前未读取本批新实现；18:08:32.3234566 UTC 已独立推导二进制黄金样本。
- 门槛满足后读完 8 个新 C#、新 asmdef、10 个新 meta 和旧 asmdef 的精确差异；依 `.agent/REVIEW_CHECKLIST.md`、`.agent/VALIDATION.md` 检查实际补丁与证据。R 不运行 Unity／项目测试，不改实现／测试／meta／scope／自然产物，不作 Git 写入，不建任务或子代理。

## F1 · P2 · 保留当前头 I/O 检查失败的结果及原始原因

位置：[SaveHeadInspection.cs:76](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/SaveHeadInspection.cs:76)、[同文件:93](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/SaveHeadInspection.cs:93)、[RecordFailure:150](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/SaveHeadInspection.cs:150)；结果传播见 [LocalSaveStore.cs:50](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/LocalSaveStore.cs:50)、[Prepare:66](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/LocalSaveStore.cs:66)、[Inspect:108](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/LocalSaveStore.cs:108)。

触发：目录已有一个完整、身份相符的已提交当前头；真实存储包装在打开／读取其现有最终 marker 时抛出 `IOException("head-read-failure")`，文件并未被删除或改坏。分别在 `Open(Existing)` 的初检，以及已打开会话的 `Prepare` 重查阶段注入。

静态支路可直接证明现状：

1. 第 70 行读取异常进入第 76 行；`RecordFailure` 第 153 行只重新抛出 Limit，把 I/O 留在文件诊断中并清空候选 marker。
2. 没有可用当前头，最终第 146 行返回 `Status=RecoveryBlocked / Current=null` 的观察对象；`Inspect` 第 108 行仍返回 `IsAccepted=true / Code=Inspected / Value非空`，`Open` 因此保存同样的成功 InitialInspection。
3. 同一异常出现在 `Prepare` 重查时，第 66 行新建 `RecoveryBlocked / FieldPath=Head`，丢弃观察内的原失败。结果的 `Diagnostic.ExceptionType`、`ExceptionMessage` 均为空，阶段只剩 `Head`。

§144 明确要求“检查本身因 I/O／Limit 失败时 InitialInspection 保留失败及空 Value”，同时保留已取得租约的 Opened 会话；§144／§145 又要求 I/O 的原异常类型、消息和具体阶段可追溯。这里没有错误授权写入，但把检查失败与已完成的损坏证据观察混在一起，并让调用者无法从 Prepare 的失败结果识别实际存储问题。

最小修正：区分无法完成检查的 I/O 故障与已确认的坏格式／缺正文等物理证据；对影响当前头完整核验的检查失败，保留原原因并返回失败观察，Open 仍保留诊断会话。Prepare 传播阻断原因，不能改写成无异常信息的通用 Head 失败。保持已索引旧坏副本的冗余规则、未知时的写门及发布后的 CommitUnknown 分界。

这是源码逐支路结论，R 未执行新增复现。现有 [初始诊断测试:313](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/LocalSaveStoreTests.cs:313) 的 I/O 注入仅覆盖枚举异常；55 个 Write 故障用例覆盖提交阶段，不能证明 Open／Prepare 的上述读取失败协议。需补定向回归后关闭 F1。

## 两轴与七项验收

| 轴 | 结论 | 独立证据 |
| --- | --- | --- |
| 规范 | 通过 | 白名单、变更行数、程序集隔离、meta 来源、旧文件保持及真实验证记录符合本包；未发现额外依赖、Core API／格式／友元、Git 或其他批证据改动。 |
| 规格 | 待修正 | 核心提交／重试／查询协议及现有验收见证成立；初检与 Prepare 的 I/O 失败结果不满足 §144／§145，见 F1。 |

| §147 项 | 结果 | 可核对的实现与测试 |
| --- | --- | --- |
| ① 固定格式与隔离 | 通过 | MarkerCodec 22–120：严格 magic／版本／故障域／词法／长度／自摘要／尾字节／文件身份；WindowsStorage 15–43：原 UTF-16 小端目录散列、Purpose 隔离。MarkerTests 19–136：固定完整字节、三代独立写出见证、16 种破坏、7 种词法、孤立代理／NUL／空格与 Candidate／Player。R 的先验样本与常量及两轮实际样本逐字节相同。 |
| ② 一次接纳与稳定票据 | 通过 | Store 57–103、264–274 与 Models 82–114：先冻结元数据／嵌套索引／摘要，回调一次，逐字段比较，接纳前无候选文件。StoreTests 19–123、125–154：真实 015B 五片 Begin／Attack／Rollback 三代写盘，015A 读回、015B Decode、五片字节对齐；列表／摘要复制与 8 类元数据／索引篡改拒绝。 |
| ③ 原键查询与回执 | 通过 | Store 242–255：双键同一行、原 Descriptor＋最新头；Write 135–138：旧成功票据查询返回。StoreTests 48–61、125–154、335–348：三代释放锁后重开、删第一代物理文件仍查旧索引、双键冲突、外来／结束／Disposed 票据、重复 Write 不产生写调用。 |
| ④ 故障分界及重试 | 通过 | Store 144–198：真实 snapshot 写／Flush／Close／校验／提升，再 marker 同序操作；177 行先置发布标志，发布后失败为 CommitUnknown。StoreTests 194–260 的 55 个真实文件参数用例全部 Passed，包含首代、两次 Promote 前后及最终枚举／读；MarkerTests 163–197 覆盖最终 Math 耗尽与冲突 snapshot。回调、身份、实际字节及业务随机状态保持。 |
| ⑤ 排他与重入 | 通过 | WindowsStorage 45–50、59–94：真实 FileShare.None 租约、CreateNew、所有者 FileStream.Flush(true)、不覆盖 Move、最终 marker 禁删。Store 24–54、229–240：先能力／身份再租约，Interlocked Busy；StoreTests 158–190 核第二 Store Busy、遗留锁重新取得、无能力时零存储调用、并发／重入与回调异常释放 Preparing。 |
| ⑥ 唯一头及未决处置 | 待 F1 | HeadInspection 51–148 完整计枚举、未知证据阻断、索引归属旧坏副本、未封记候选 Pending；Store 203–217 仅原票据确认未成后删除获准候选。StoreTests 265–348 与 MarkerTests 181–216 的缺正文／坏封记／分叉／高代／预算／未决结束场景通过，未回退旧头。当前头 I/O 检查失败的成功标记及原诊断丢失仍需修正。 |
| ⑦ 范围及实际验证 | 通过 | 精确 19 新实施文件＋旧 asmdef 一行；490 文件、270 唯一 GUID；同一 B12-F1 源码实际编译和最终测试 exit 0，2640 全过。原 2525 名称重数／Passed、完整归档、原命令和 1090 个实验文件已独立比对。证据严格限于同进程 Windows 行为。 |

表内源码简称位于 `Assets/Scripts/FightMatch/Platform/`，测试简称位于 `Assets/Tests/EditMode/FightMatch/`；它们的完整路径和 SHA 在已核对的交付 scope 中。

## 独立范围、黄金字节与回归核对

- 原 471 基线和 before／startedSnapshot 逐项一致；原 inherited 时间为 `2026-09-21T17:09:01.0995492Z`，C 本批 startedSnapshot 为 `2026-09-21T18:02:42.6722216Z`。规范 SHA=`2e447f19aabed033939d88d90c3b712c8cf0505fb19f4ec9d089db45d7bd1b99`。
- 唯一旧改动为测试 asmdef，553 → 584 字节。去掉新增的 `        "FightMatch.Platform",\n` 一行，完整字节 SHA 恢复为 `829acdb13ac53136202aaf85b7e9cc5572085e720bb78f149a649ae63dfd66e5`；其余字段、顺序与 LF 不变。
- 其余原 470 项 SHA 不变；原 260 meta 的内容与 GUID 均不变。Assets 全清单由 488 → 507，仅新增白名单 19 项，无缺失或额外项；新 10 GUID 无碰撞，总计 270。
- 手工实现／配置十文件冻结 B12-F1 的规范 SHA=`3ee913061417319cde0674e0fa13939e31d3dab80d1739bf5582f75cf7857750`；最终全部 490 文件规范 SHA=`4a88037f6d67b95e3ab46faf3d176d7bb7cce0ab8dd7166efecd1b37bc593c1d`，与实际文件及测试前后记录一致。
- 新 19 文件共 1852 行，加旧 asmdef +1/-0，总计 1853/3600。纯 Platform asmdef 仅引用 FightMatch.Core，noEngineReferences=true、allowUnsafeCode=false、平台列表为空；没有新 Core 友元。
- R 独立固定的 477 个受保护输入全部保持，覆盖旧报告／scope／批次 XML／日志、规则、Packages、ProjectSettings 和本机权限文件；作者记录的 39 项也全部保持。Git 现有 34 个已跟踪差异属于先前工作，不能当成本包补丁；`git diff --check` 实际 exit 0。
- 独立最小 snapshot：252 字节，SHA=`6de3d0102b5f261399b5355eb454f13d7a84c4caf27e7158498bcc67b4a98041`；marker：256 字节，完整 SHA=`dc0cc1dfa707b0cb8c5610f5e386b9748663625b8688b2494dcf4b9cb8847e9e`，尾摘要=`d4949c9bbbfa6ce51632a553b6d6410805eb357529f98f5555ef344e88607d60`。两轮固定样本实际字节均完全一致。
- 原 XML SHA=`6df01fa60096bc32c1dd2691a623637d00e119921933ca3f7192022f0e5aeb64`，2525 条、2520 个 Ordinal fullname。R 的 fullname／Count／Passed 规范 SHA=`ea7559fcacc4dade4d5049045a6636b4e70e770f04db04327d74a82c50653fb3`；最终逐名、重数、Passed 差异均为空。
- 最终 XML 实数 2640，新增 115，全部 Passed；failed／skipped／inconclusive 均 0。scope 的原 2525 和新增 115 完整清单均与 XML 一致，包含 55 个故障参数用例。

## 原始执行、导入和实验归属

R 从 C 本次原 turn 的会话事件读取 53 个 CommandExecution、9 个 FileChange。scope 归档的 17 个关键命令经 shell 引号解码后与原 argv 完全一致，完整 output、cwd、状态与退出码一致且未截断；所有 8 个非零辅助命令均已归档。实际 Unity 启动恰为下列 3 次，没有其他 Unity／Git 写入命令。

| 原运行及 CommandExecution | PID | UTC 起止（2026-09-21） | Process.ExitCode |
| --- | ---: | --- | ---: |
| B12-COMPILE-1 · `exec-5d1a348e-1f42-489e-9720-c99c8dd0b755` | 33864 | 18:31:51.9635511 → 18:32:23.9896150 | 0 |
| B12-TEST-1 · `exec-ce2baedb-2115-4b53-9639-1bd6c7b7a1a2` | 17804 | 18:33:07.0438671 → 18:33:54.7444076 | 0 |
| B12-TEST-2 · `exec-b32161d1-30e5-40b4-aeff-7cad2a7c2599` | 23044 | 18:35:15.8813573 → 18:36:03.3413074 | 0 |

三个原命令均在启动前检查 Unity 版本与无其他 Unity，使用固定 `D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe`、Hidden／PassThru／Wait，WaitForExit 后读取真实 Process.Id／ExitCode。编译含 -quit；两个 EditMode 调用均不含 -quit；项目、logFile、testResults 均为正确引用的绝对路径。不是用外层 shell exit 代替 Unity exit。

最后源码 FileChange 在 18:30:37.719，十文件冻结在 18:31:32.2151205，先于编译。编译源清单 480 → 490，十份增量恰为自然导入 meta；原事件没有手写 meta，编译日志的十个导入 GUID 与现物相同。源码冻结前后保持，日志无 C# 编译错误。

首次 GUID 辅助检查误用 Scripts／Tests 的 259 份计数，对应非零命令完整保留。C 未等该检查通过就启动 TEST-1；该轮 2640 全过，但不据此替代正确顺序。18:34:23.2760801 完成全 Assets 270 GUID／490 文件检查，18:35:10.3879435 无损归档 TEST-1 全日志／XML，再运行 TEST-2；因此最终验收满足导入核对先于同版测试。后续 CRLF 检查式、XML InnerText 和只读诊断辅助失败均有原记录，无源码修改。

| 产物 | 字节数 | SHA-256 |
| --- | ---: | --- |
| 最终编译日志 | 65917 | `4301c63c407217ce48f5129681613ab5dc96a3121164d95f07bf4a16a2ef543f` |
| TEST-1 完整日志归档 | 89006 | `4602f27420e675850a8d9f179b54e072dbfa3417375122c051a6b434c5a3d3e7` |
| TEST-1 完整 XML 归档 | 1765981 | `7cca6202f466ded97aba3060ce1a40815c420f66f8300686d5d72e6d2d9c94c1` |
| 最终测试日志 | 89027 | `7a1bb2016d3719071021cc507c47c00c2b5ffa1ffbcf62897ccdab296ff97b29` |
| 最终 XML | 1765962 | `888092f8871282eb309c4209325cde94383f2b2917e9f40f8c8779af4e35859d` |

scope 六份 Brotli 证据均已解压核长度／SHA；原进程即时产物摘要、被覆盖归档及最终现物相符。两轮实验根分别为 `TestArtifacts/FMDemoB12/133da444453b41fb94f9c6db1a9a0041`、`TestArtifacts/FMDemoB12/9e1e5826674e407aabd3c9f637c06600`。

每轮 XML 的输出均独立对应 116 个 GuidN 用例目录和 545 个文件；实际 1090 个文件全部逐项核长度／SHA，规范 SHA=`39dba4bd1539b930c4103ef525910e83483bfe38ccd56e18b48e8b4b6e2f7a9f`，无未知根、根级文件或 reparse 项。测试 355–368、420–423 的绝对路径边界／reparse 检查先于新建、写入和单文件删除；没有递归清父目录。故障流 463–533 取得并释放实际文件流，Flush 转发原 FileStream；现场均保留。

## 最小修正包建议 · FM-DEMO-016-C1

本节供 SD00 按既有门槛派发；R 不自行启动实施。目标只关闭 F1，不重做整包或展开 017／018。

- 基线：本次 490 文件规范 SHA `4a88037f6d67b95e3ab46faf3d176d7bb7cce0ab8dd7166efecd1b37bc593c1d`、270 meta／GUID及最终 2640 用例。保留原 471／2525 继承链、本报告及本批交付／scope／日志／XML／两个实验根。
- 实施白名单仅现有 `Assets/Scripts/FightMatch/Platform/SaveHeadInspection.cs`、`Assets/Scripts/FightMatch/Platform/LocalSaveStore.cs`、`Assets/Tests/EditMode/FightMatch/LocalSaveStoreTests.cs`；其中 Store 仅允许必要的失败传播分支。三文件新增＋删除建议 ≤200 行；不新建实施文件，其他 487 文件保持。
- 禁改 Core、marker 格式、公开 API、asmdef、meta、GUID、包／配置／权限、旧证据、三协调稿；不扩展清理、跨进程恢复、M02 或 UI。
- 补真实文件包装的定向回归：现有完整当前 marker／snapshot 的打开、读取或关闭 I/O 故障导致 Open 仍返回 Opened 会话，但 InitialInspection／Inspect 失败且 Value 为空、保留原类型／消息／阶段；Prepare 重查失败保留同一原因、回调零次、无新 ticket／候选文件；故障解除后原键查询和有效准备可恢复。
- 已确认坏格式／缺正文继续按既有 RecoveryBlocked 观察处理；已索引旧坏副本、Pending、最终发布后 Unknown、原票据重试及结束规则全部保持。不要把所有文件错误不加区分地提前抛出而破坏这些规则。
- 验证：C 仍是唯一 Unity 运行者，先只加回归并记录预期红测，再修最小实现；每阶段先冻结，最终同版实际编译＋全量 EditMode exit 0，原 2640 的 Ordinal fullname／重数／Passed 全保持，新增全过。R 继续只审查，不启动 Unity。
- 建议新交付：`docs/system-design/2026-09-17/demo-016-c1-delivery.md` ≤220 行、`docs/system-design/2026-09-17/demo-016-c1-scope.json` ≤600 KiB；自然产物精确为 `Logs/FMDemoB12C1RedCompile.log`、`Logs/FMDemoB12C1RedTests.log`、`FMDemoB12C1Red-EditMode.xml`、`Logs/FMDemoB12C1Compile.log`、`Logs/FMDemoB12C1Tests.log`、`FMDemoB12C1-EditMode.xml`。实际派发由 SD00 确认这些新证据路径，不覆盖 B12 证据。
- 文件实验继续使用 `TestArtifacts/FMDemoB12/<全新runGuidN>/<caseGuidN>/`，仅记录本修正运行新建根；已有两根只读，不清理父目录。保留所有失败产物、原命令／PID／退出码、冻结与前后 SHA。
- 停止条件：如三文件或预算不足，向 SD00 报准确 BLOCKED；不得自行放宽。交正式 handoff 并结束原 C1 turn 后，R 再独立复审；在 F1 关闭并满足完成门槛前不派 017。

用户无需补充业务决定。后续动作是 SD00 派发上述局部修正与复审；已通过的 B11／C1 结论和本批已核事实保持，R 本回合交回正式报告后停改。
