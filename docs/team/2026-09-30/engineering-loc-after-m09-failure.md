# M09 静态诊断：restore 项目根临时文件遗漏

2026-10-12；主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，本次 actual `01a12ef5-0026-7140-92cf-f98d0cb0d364`，收件 item `fco_01a12ef5-0036-7173-afc8-16d28e429bc6`。中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 唯一收件；本文件是诊断和最小 SOURCE 修正建议，不是实施派发或新 native 许可。

## 结论与证据

**已确认 FIX07 写入合同漏掉三个 M restore 图内的项目根 GUID.tmp。** `write_plan` 已允许项目根 `packages.lock.json`，却仅给 `obj` 配临时叶；`_fix06_guid_rule` 又强制 `/obj/{guid-tmp}`，`_fix06_validate_rules` 固定49条。失败叶不在任何可写规则中。补齐已知六种最终输出的两个父目录，需98条窄规则，不是只给 Gdscript 打补丁。

记 `BASE=TestArtifacts/FightMatch/LOC-LIC-ALT-M`、`F7=BASE/loc-lic-alt-yamldotnet-16.3.0-m17-fix07`、`E09=BASE/loc-lic-alt-yamldotnet-16.3.0-m17-m09`。M7=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix07`；W7=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix07`。

本轮读取 E09 的 `before.json`、`stdout.log`、`stderr.log`、`receipt.json`，及其指向的 W7 三叶：`receipts/M-runner-generation-restore.json`、`logs/M-runner-generation-restore.stdout`、同名 `.stderr`；没有遍历 M7/W7。另读 F7 冻结源码/模型、已有 FIX07 OS 回归和中央 partial 清单。

- M09 实际执行者 LOC actual `01a12ee6-b260-7693-9f5f-ecf014c0482d`；原工具 `exec-6240c5ef-e6bf-4d38-af3b-48716e784015`，host exit1、完整调用128.818624秒；首 restore6.342396秒/exit1，后两条未启动，0新增下载。一次材料执行额度已用完。
- 准确失败叶：`W7/phases/M-runner-generation/source/src/Luban.Gdscript/43195cad-a941-4061-8b31-b7e1a14bbb92.tmp`；stdout第22–23行记录 Access denied / Operation not permitted，**不在 obj 内**。restore 回执显示1次启动、0子进程、非超时、quiescent=true。
- 三叶鲜核SHA分别为 `226df45e042af1e7a18c368163dac58529094a77f342dfb5dcbc8d23ec35a65a`（2998B）、`700e3ef31fcd97e35fe04a575d247d9c5fd2414848d01855f27ac350c0e4c5e9`（1758B）、`e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855`（0B），与 E09 一致。
- F7 driver261874B/SHA `9aa2db85274369fa27365c3fa39b120fa68deed0effb527b7fccaa5a06894f48`；模型SHA `8c66978b5f7a86d77e158baf7b34b7f1c7ab9616987b20e263ff70ec7e6a5f58`；freeze2807B/SHA `5f747b1441f79bb43ff00f17757b36fee2e7c374d3e8dd00570d42d54b835027`。原生记录 profile SHA `7616e122dd4e4e5a1a5e85ada600e95df8e430d4c4adde3b9d9610fb17f12d6c`，本轮没有重建/安装 profile。
- E09 保存284项保护未变、knownProcessQuiescent=true、stillOwned空、manifest缺席；本轮仅引用已结束记录，没有重新运行 ps 或声称新鲜全机进程检查。

按 diagnosing-bugs，复用本次真实失败作为红色证据；中央明确限制静态诊断，因此跳过重复复现、OS探针、修复执行与完整绿色验证。下面机械比较不是新的 DUT/OS 测试。

## 三个可证伪解释

1. **父目录遗漏（确认）**：若成立，冻结模型匹配实际失败路径应为0，同名 obj 叶应为1。本轮 jq 静态比较得到0/1；源码509–549、2756–2768、2822–2836行支持这一差异。1064–1090行按命令选规则，DIRECTORY vnode许可只允许目录自身，不允许其中任意普通文件。
2. **原子替换 packages.lock.json（推断）**：项目根唯一已声明 restore 最终叶是该文件，因此它是临时叶的合理目标；但现有日志没有文件调用栈或 rename 目的地，不能写成已证实。修正覆盖全部已知最终叶父目录，不依赖这一推断成立。
3. **FIX07 名称表达或链接/外层策略问题（未支持为本次首要原因）**：若同名在 obj 也不匹配，或现有OS正例失败，应回查表达。静态同名 obj 匹配，FIX07已有 valid exit0、invalid-length/extra-suffix exit1、全部reaped，三项0.623595秒；本轮复用且不重跑。不能因此排除新规则后的其它OS/权限/rename问题，也不宣称完整 NuGet 修复已验证。

## 三阶段全部已知目标与临时父目录

令 `P=<新W>/phases/<phase>/source/src/<项目名>`，`C=<项目名>.csproj`；以下六个最终叶对相应图的**每一个项目**都已声明，精确输出共138+78+78=294条。表内父目录表示需覆盖的同目录原子临时叶位置，不声称六种写入均已在日志观测为原子操作。

| 已声明最终叶 | GUID.tmp直接父目录 | FIX07覆盖 |
| --- | --- | --- |
| `P/packages.lock.json` | `P` | 缺失 |
| `P/obj/project.assets.json` | `P/obj` | 已有 |
| `P/obj/project.nuget.cache` | `P/obj` | 已有 |
| `P/obj/C.nuget.dgspec.json` | `P/obj` | 已有 |
| `P/obj/C.nuget.g.props` | `P/obj` | 已有 |
| `P/obj/C.nuget.g.targets` | `P/obj` | 已有 |

| phase / 唯一restore writer | 项目图 | 最终叶 | FIX07临时父数 → FIX08建议 |
| --- | --- | ---: | ---: |
| `M-runner-generation` / `M-runner-generation-restore` | runner23 | 138 | 23 → 46 |
| `M-tests-generation` / `M-tests-generation-restore` | tests13 | 78 | 13 → 26 |
| `M-tests-verification` / `M-tests-verification-restore` | tests13 | 78 | 13 → 26 |

runner23准确项目名：`Luban.Bson, Luban.CSharp, Luban.Core, Luban.Cpp, Luban.Dart, Luban.DataLoader.Builtin, Luban.DataTarget.Builtin, Luban.DataValidator.Builtin, Luban.FlatBuffers, Luban.Gdscript, Luban.Golang, Luban.Java, Luban.Javascript, Luban.L10N, Luban.Lua, Luban.MsgPack, Luban.PHP, Luban.Protobuf, Luban.Python, Luban.Rust, Luban.Schema.Builtin, Luban.Typescript, Luban`。

tests13准确项目名：`Luban.Bson, Luban.CSharp, Luban.Core, Luban.DataLoader.Builtin, Luban.DataTarget.Builtin, Luban.DataValidator.Builtin, Luban.FlatBuffers, Luban.L10N, Luban.Lua, Luban.MsgPack, Luban.Protobuf, Luban.Schema.Builtin, Luban.Tests`。两次 tests 各有独立 phase 根；不可合并成共享可写树。

其他已声明写入保持原范围：每阶段 `env/packages/<固定31个id/version>/{safe-leaf}`（每规则≤5000叶/128MiB，既有归档条目与NuGet元数据校验不变），writer为对应 `phase-restore`；`env/{dotnet-home,http,plugins,scratch,tmp}` 分别沿用128叶/8MiB、32/1MiB、32/1MiB、512/8MiB、20000/512MiB，writer为对应phase，由命令选择器同次选入。没有证据支持新增第三种项目临时父目录；不扫描SDK猜测路径，也不新增整个项目/obj/SDK/用户目录授权。stage源文件、锁种子、feed复制及seal输出属于各自writer，不转授restore。

## FIX08 最小源码建议

新候选仅在 `BASE/loc-lic-alt-yamldotnet-16.3.0-m17-fix08/` 保留现有八叶结构：`source/m17_driver.py`、`source/m17-inputs.tsv`、`source/m17-write-set.tsv`、`source/source.patch`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/source-receipt.json`；工具完成后另存 `docs/team/2026-09-30/loc-m17-fix08-command-receipt.json`。这些是后续中央 SOURCE 包建议白名单，本轮只写本文。

1. `write_plan` 对上述每个项目同时产生 `P/{guid-tmp}` 与 `P/obj/{guid-tmp}`；封闭图校验明确比较全部98个 `(parent,writer)`，拒绝缺失、重复、其它项目、跨phase或额外父目录。最终输出六类不得删减。只增加49条：M规则5365→5414，全模型9412→9461；B3888条保持不变（均为只增加此差量的预期计数）。
2. `_fix06_guid_rule` 只接受这两个形态，结合封闭图校验，不仅将后缀检查放宽为任意 `/{guid-tmp}`。沿用已验证展开字符类的GUID表达（小写8-4-4-4-12＋`.tmp`）、父路径转义、首尾锚定、直接子叶、准确M writer。命令选择、profile生成、目录准备、写入判定与输出检查继续使用同一规则，不另建通用目录授权框架。
3. 每父规则仍≤1驻留普通文件、≤16777216B；新旧两父各独立计数。父/祖先/叶无symlink，已存在叶须regular且nlink=1，重复路径/硬链接/目录叶/越界全部失败。继续验证父目录安全后准备、按规则核输出；这些驻留配额不是累计写量或OS实时峰值保证，不能虚报。
4. 最小附加变更仅新SOURCE/作者/权源/根绑定、M09保护输入、差量检查和下述driver预算一致化。`FIX06_GUID`、sandbox网络/fork约束、三条命令argv（仅重绑定新M/W路径）、归档/版本/图/许可证、生产null pin、激活校验、REVIEW02外层侧录启动门及一次运行/清理门保持原逻辑。

### driver 字节预算：显式小幅调整，不另拆信任文件集合

F7为261874B，256KiB上限262144B，仅余270B；现有 `fix07_checks` 3342–3423行就有6011B。新父集合校验、M09清单绑定和差量检查不能假装仍有足够空间。**建议仅新FIX08的driver上限改为272KiB=278528B（+16384B，6.25%），相对F7净增≤16384B；封存前实测超额即停止缩小实现，不继续加额或压行规避。**

这是本次最小选择：当前 `prepare`/seal/consume及REVIEW02侧录绑定恰好八叶；现在拆测试为第九叶还需改变封存集合与启动信任接口。保留八叶、将新检查集中为一个差量入口，比同时重构这些门范围更小。未来独立拆分可另签包；本轮既不拆旧测试也不重跑它们。

必须同时修改新候选 `execution_budget.driverMaxBytes`（940行）、`seal_source`（1289行）、`_review02_receipt_content`（2313行），并使新SOURCE说明/侧录及将来新activation读取同一数值；旧F7及旧activation一字不改。SOURCE总16MiB、侧录64KiB、M材料512MiB、work4GiB、0GET、三restore及900秒均不变；没有新增运行权。

## M7/W7 保护与中央捕获要求

中央已经完成唯一新库存：`docs/team/2026-09-30/central-loc-m09-partial-capture.json`，1334742B/SHA `46033e3be907cc0dac725e09fe6bf28a2caf08c0ed1781c51df089575acd3f20`；3701文件/129493955B、1328目录、links/nonregular=0；UTC07:06:51.334691→07:06:51.990624，中央actual `01a12ee5-5e38-7863-b291-95e758e24669`。无需再让LOC或主程枚举一次。

该清单 canonical inventory SHA为 `8b9abdfdb17a3f3865e454482c7cd2851dbffc9219485cbbbb6e652ea77ac681`，定义是UTF-8、sort_keys、无空格紧凑JSON、**无换行**；不得误用带缩进/换行的 `jb` 重犯FIX07格式问题。FIX08固定原文件身份、双根、计数、父链完整/路径唯一及中央身份时间；仅按闭合清单核已知叶bytes/SHA、目录类型、无链接/nlink保护，前后保持只读。该清单未保留历史目录时间戳，也不能证明不存在未列出的新叶；据实保留限制，不新增枚举补称完整证明。

F7八叶、FIX07侧录、M09四叶/A/H/binding/原日志/回执及中央清单作为新的保护输入，与所有早期保护集合并存。后续新M/W建议使用独立 `...m17-fix08` 根并由中央确认缺席；M7/W7永不作为新执行输出。这里不建根、不更新清单、不删除partial或改权限。

## 最小后继验证与接收顺序

1. 中央普通技术SOURCE包先固定本报告、F7身份、M09证据及捕获清单，明确上述八叶/侧录和新driver限额；不需要重新询问产品技术选择，也不自动批准材料运行。
2. 新候选仅一次≤10秒的统一 `fix08_checks()` 差量回归：经生产 `write_plan`→`command_write_rules`→`child_sandbox`/matcher/inspect接口验证全部98父；冻结旧候选对此失败叶为红、新候选为绿。覆盖三个writer、六最终叶父集合、图外/跨phase/兄弟和嵌套路径/大写或错长或后缀、重复规则、symlink/hardlink及驻留数量/字节越界；用内存fixture/injected probes，不创建材料文件、不安装profile或启动SDK。同时核执行前缺pin/缺侧录仍零写、driver三处限额一致、M09 canonical计算正确、其余写合同和命令只发生批准差量。报告标注STATIC/SIMULATED，不冒充NuGet成功。
3. 复用FIX07三个真实OS结果及M08六探针；不再跑这些探针或55/81/109旧套件。沿用一次≤30秒SOURCE封存，外层工具完整返回后再生成侧录；记录准确新八叶、model、原始tool/exit/时间与所有失误，不用内部函数时间代替完整调用时间。
4. 新准确head交GitHub PR Code Review，由既定接收者核结果；不得本地重复review。SOURCE与差量通过仅关闭源码门。之后中央才可提出下一次真实0GET/三restore/900秒执行方案并取得有效授权；本轮不派发、不执行，也不以补49条宣称M已ACCEPT。

## 本轮交付与纠偏

前一回合误恢复N07历史方向，仅执行文档/聊天读取及两源码哈希检查；没有发送跨聊消息、启动Unity/native或修改文件。本回合已停止该方向；未将旧授权续用。B方案原文件SHA `99824e78b1486ddb02c3528d89b1b21a2f3acb0701e3205b64107ba19bfb8084` 保留。

本轮仅新建本文；未导入/执行driver，未进行SDK/partial扫描、restore、OS探针、下载、Git写或跨聊派发。静态jq首次表达式有编译错误，修正括号后比较exit0；未把失败调用记为测试通过。writing-for-agents使本报告以封闭范围和可检查完成条件组织；codebase-design使修正复用同一规则接口，避免为本次小修扩大封存信任接口。当前结论是**诊断完成、FIX08未实施、M09仍NEEDS_FIX**，中央据本文接续。
