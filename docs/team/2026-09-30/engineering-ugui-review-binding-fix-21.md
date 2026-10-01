# UGUI-01 FIX21：真实审查对象与执行字节绑定

2026-10-01 · r1 · 修复设计，非重复代码审查；中央确认并派发后 C 才实施。
设计：ARCH-ENG-001，`01a0f2e6-1ac3-7d10-8855-54192b9489d4/local`，本回合 `01a0f6c4-af1a-70d0-980e-3014871769c6`。
唯一交付执行者 C：`01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`；唯一接收者中央：`01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。
授权来自用户已批准的团队协作及中央本次限定修复设计派发；模型按现行 `gpt-6-astra/xhigh`。本回合只写本文，不启 Unity、不改 Git、不派 R。

## 1. 固定事实与边界

- P1：[原始发现](https://github.com/yyczz1/FightMatch/pull/1#discussion_r4153777291)，旧 runner 第 403 行只检查 SHA 格式／不是前序 SHA；原缺陷保留。
- `H20=36ad62c4c71f4b987efaf60f26c942a0a9de10bd`，`T20=94ad1b6976834b15a1d5582fbc980480e1c3ba51`；已从 [GitHub commit API](https://api.github.com/repos/yyczz1/FightMatch/git/commits/36ad62c4c71f4b987efaf60f26c942a0a9de10bd) 独立确认关联，PR #1 当前 head 为 H20。
- 本机 HEAD 为 `125b849be13fe2ebf5b1185f3cdb83d19c6327ef`，T20 对象存在、H20 commit 对象缺失。不得要求 HEAD=H20，不 checkout/reset/stash，不触碰独立 LOC WIP。
- 旧运行：C turn `01a0f6bd-a064-7192-9e47-a3cd8161b84d`；exact12 为 12/12、exit0、75.019s，82.39s owned-clear；历史分组为 178+12+30+2=222，不再跑 Unity 来补元数据。
- 路径别名：`E=TestArtifacts/FightMatch/UGUI-01/q4-correction-20`；`S=TestArtifacts/FightMatch/UGUI-01/q4-correction-20-source`；`A=docs/versioning/2026-10-01-ugui-pr`；`N=TestArtifacts/FightMatch/UGUI-01/q4-correction-21-source`。
- 本回合 raw-blob/磁盘只读比对：before/after 的 source53、product29、resources37、delta2 全同；92 项诊断集及 runner 均等于 T20 对应 blob；受保护 1006 项中 998 项在 T20 且全同，990 项恰为完整 Assets/Packages/ProjectSettings 文件集合。不是新测试通过声明。

## 2. C 的精确读写合同（待中央派发）

- 产品写域为空。允许读取 T20／后续授权 head 的真实 Git 对象、上述旧证据、下述依赖闭包及对应工作区文件；不启动 Unity、编译器或下载 SDK／仓库，不改旧证据、产品、包／设置、资源／meta、Git refs/index 或共享索引文档。
- 仅新增 `N/validation-tools/runner.py`、`N/validation-tools/test_review_binding.py`、`N/binding-contract.json`、`N/offline-result.json`、`N/posthoc-binding.json`、`N/final-receipt.json`；不写 `__pycache__`。中央激活前无真实执行输出根。
- 审查归档仅新增 `A/fix21/validation-tools/runner.py`、`A/fix21/validation-tools/test_review_binding.py`、`A/fix21/binding-contract.json`、`A/fix21/offline-result.json`、`A/fix21/posthoc-binding.json`、`A/fix21/final-receipt.json`；每对为原始字节复制，不替换 fix20。
- runner 从 E 原文件派生，只改绑定门、延迟旧输入解析、FIX21 身份／新输出根参数；旧代码中与 FIX20 runner 自身 SHA 相等的门改为新 head 自绑定，旧基线仍单独验证。保留 exact12 选择、断言、360s、日志／空间上限、首次失败与 owned 清理语义。真实执行参数须另有中央激活合同；本包只有离线和只读补证模式。
- 测试 fixture 放进测试脚本，以内存 raw Git 对象／文件读取适配器覆盖生产绑定函数；不得写项目 `.git` 来造反例，不用整套 mock runner 替代实际入口。

## 3. 信任根、闭包和启动门

1. 中央从已连接 GitHub 只读 API 核准 repo/PR/head/tree，冻结本次 runId、C thread/host/turn、模式及激活有效范围；通过既有派发消息给 C，不能仅相信 C 自填 activation。新运行用其新 head，不把 H20 硬编码成永远有效；PR 移动或激活撤回则停止并重新派发。
2. 中央在既有明确 Git 对象写权限内补齐真实 commit 对象；C 本包不写 Git。可取得原始 commit bytes，或从 GitHub 字段恢复候选 bytes；先 `git hash-object -t commit --stdin` 无写入计算，必须恰等于 GitHub H，才可由中央 `-w` 导入。规范化字段不能恢复同 SHA 就 BLOCKED；不得制造替身 commit、改 parent/message/date 或自行 fetch 整库。
3. 门内固定真实仓库位置，发现对象库环境重定向／replace 映射即 BLOCKED，读取仍禁用 replace；读 raw commit/tree/blob，按 `type + 空格 + 字节数 + NUL + raw bytes` 重算 Git 对象 ID，校验对象类型及 commit 的 tree==授权 T。不能只用 `rev-parse` 输出或作者声明的两串 SHA 互证。
4. `ls-tree -rz`／等价 raw 解析取精确路径、mode、blob OID；raw blob 的长度与 SHA-256 必须等于实际读取字节，禁止换行／编码归一化、smudge/filter。拒绝路径逃逸、重复映射、symlink／gitlink、缺失、非普通文件和未获准额外输入；不因主工作区 dirty 而误拒绝。
5. 产品输入闭包先枚举 T 的 Assets/Packages/ProjectSettings 全集，核对集合与实际磁盘字节；不能只检查本包改变的 92 项。再核对 T 中 Config/Generated/Tools 的 8 个既有文件及实际使用的外部依赖。已有 Editor/工具版本身份门保留；缓存不充当审查源码权威。
6. 本次仅可把以下 8 项列为独立 LOC 非执行 WIP，保持其原始 before/after 身份，不要求纳入 PR；必须从 runner 读取／启动入口及 Unity 导入／测试依赖确认无调用、导入或业务读取，单纯文本搜索无命中不足以作为证明。有相关依赖就 BLOCKED，不扩大忽略规则：

   - `Config/FightMatch/Localization/FightMatchText.csv`、`Config/FightMatch/Localization/__tables__.csv`、`Config/FightMatch/Luban/luban.conf`；
   - `Generated/FightMatch/Localization/fm-text-v1.json`、`Generated/FightMatch/Localization/fm-text-v1.manifest.json`；
   - `Tools/FightMatch.Localization.Compiler/FightMatch.Localization.Compiler.csproj`、`Tools/FightMatch.Localization.Compiler/Program.cs`、`Tools/FightMatch.Localization.Compiler/global.json`。
7. runner／旧证据绑定从 T20 的 `A/fix20/static-result.json` raw blob 起步（与 S 同名文件字节全同），其 `peerEvidence` 固定 S 的 7 项；其中 `S/delta-manifest.json` 为 264210 bytes／SHA256 `b6f53e42c4de8eba841acfb05843d3affd61bc428b59256465b4cb1c3c30dbc3`。先验证再解析，继而核其全部 59 项 `fixedInputs`。
8. 对该 manifest 的 `oldEvidenceBefore` 按旧 runner 的 sorted tree_manifest／canonical 算法重建目录摘要，核 fileCount/totalBytes/manifestSha256，再取内部文件身份；这覆盖 R17 `preflight.json`、`validation-tools/runner.py` 和前序证据，不能因不在 59 项中就漏掉。R17 根由 manifest 精确指定，不用前缀／任意最新目录。
9. 显式映射 `T20:A/fix20/validation-tools/runner.py → E/validation-tools/runner.py`，并核 S 副本；55023 bytes／SHA256 `6850d44cf8ae8e58356b14a31253db02bafefde0d1fad8fe5a1e4dd19155813c`。`A/fix20/review-manifest.json` 是整理版，不能冒充原始 delta-manifest 字节。新 runner 映射新 head 的 `A/fix21/validation-tools/runner.py → N/validation-tools/runner.py`，测试脚本／contract 同理。
10. contract 显式记录每项实际路径→tree 路径或已锚定摘要链、角色和集合规则；不得嵌入自身所属新 commit 的 SHA（避免自引用），新 head 由中央外置激活指定。所有会影响选择／保护／清理的辅助文件都须覆盖，包括 R17 preflight／runner、R19 tests/run/quiescence、S selector/partition/manifest、激活 packet。未知依赖或断链即 BLOCKED_DEPENDENCY_UNBOUND，不能现场自签哈希放行。
11. 模块顶层仅标准库、常量与函数定义；移除旧 `pre=json.loads(...)` 的顶层读取。可信启动方先核 runner 字节，runner 进入 main 再核自身、授权及闭包，之后才加载辅助数据／进入旧 preflight。门失败必须在创建 Unity 子进程前终止，并记录具体失配路径和期望／实际身份。
12. 启动前紧邻 spawn 再核完整闭包、activation/selector/runner；中央／C保持本次输入单写者冻结。读取已校验的相同 bytes，避免校验后重新读取未核版本；输出在受控新目录。运行后再核闭包；中途漂移使结果失去绑定，不能宣称只靠前后快照消除了所有并发竞态。

## 4. FIX20 事后补证（与未来修复分开）

- 补证工具只读 E；入口固定 E/final-receipt.json 为 8223 bytes／SHA256 `410fa32deadd3163a9f181a259c4a88b3f5ff200ce791838b697401ddd91b705`，逐项核其 evidence（含 before/after、preflight、run、XML、events、coverage、quiescence 和 runner），不是直接接受 receipt 的布尔值。
- 验证真实 H20→T20、以上 raw-blob／依赖链，再从原始 XML/fullnames 及分组关系核 exact12／retained178／Host30／exact2；原始 before/after 保持1006项全同，记录998绑定项和8项排除依据。此处只解析原结果，不执行用例。
- `N/posthoc-binding.json` 标为 `POSTHOC_BINDING_SUPPLEMENT`，记录旧 run 身份、新核验时间／工具身份、对象来源、每项路径/blob/bytes/SHA256、依赖链与失配、原证据哈希；成功才能写 `bindingEstablished=true`。
- 明示“原 runner 未实施真实启动前 Git 绑定；本补充是对既有证据及当前独立 Git 对象的事后关联，不是原时点新增证明，也不是新 Unity 运行”。不能改写原 P1、旧 preflight 或把本次核验时间当原运行时间。
- 任一原证据／依赖无法独立核实则写 `bindingEstablished=false` 和 BLOCKED 原因；保留原 12/12 事实及其绑定限制，不自动重跑、不把新 runner 的测试结果归给旧运行。

## 5. 离线验收与返回

- 入口：仓库根执行 `python3 -B TestArtifacts/FightMatch/UGUI-01/q4-correction-21-source/validation-tools/test_review_binding.py`；只测实际绑定函数／main门控，以假启动器计数，无 Unity／dotnet／Git写入。预算一次完整离线集≤120s；修复后最多一次复验，失败再回中央。只读补证入口由 runner 提供 `--posthoc-fix20`，同样禁止启动路径。
- 正例：合法真实对象结构、准确映射、闭包完整；本地主 HEAD≠审查 head且只有上述独立 WIP；旧 runner 的 archive→actual 映射；预期均通过，假启动器恰一次（补证模式恒零）。
- 反例逐项断言失败原因且假启动器为零：任意40hex／不存在对象；旧但真实的错误 head；commit类型错误／raw hash损坏；head/tree错配；源码／资源单字节漂移；非92集合内源码变更；新增／删除导入文件；runner／辅助runner漂移；篡改S manifest并同步自填SHA；R17 preflight漂移；缺失依赖链；selector／activation错配；路径穿越／symlink；Git replace／环境重定向；临近spawn漂移；撤回／过期激活。
- 补证反例：旧XML／before／after／receipt身份变化，任一不得生成 bindingEstablished=true；运行后漂移保留运行事实但标绑定失败。测试必须覆盖真实入口，不只测正则或手工写 pass。
- 验收 A：上述离线正反例、真实 H20 对象链及全部输入／旧证据闭包可核；B：补证如实区分历史事实与新机制；C：scope外文件／旧证据／Git不变、Unity启动次数0；D：中央发布新 head后，新 runner自身与新树映射复核，并由 GitHub PR Code Review审查对应新 head，原12/12不自动批准新机制。
- 交付分两阶段：C先交源码／离线／补证，D明确待新head；中央固定归档上传后接收D，不为把新head写回其自身树中的receipt而循环产生提交。发布后新增核验结论走中央外置接收记录，不回写已冻结归档。
- C 只回中央≤200字摘要＋N/final-receipt.json，含真实 thread/host/turn、修改清单、逐项A–D状态、证据身份、唯一阻塞；不自给 ACCEPT。中央持有新 head 激活／Git写入责任，按既有主测试接收规则完成审查门；本方案和事后补证都不关闭设备／视觉门。
