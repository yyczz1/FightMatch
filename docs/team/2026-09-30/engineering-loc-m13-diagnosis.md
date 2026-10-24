# M13：活跃输出计量竞争与 FIX11 SOURCE

2026-10-13。主程签发／唯一源码收件：`01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a1332f-61b9-7e53-a24b-d5451446635a`。权源为中央 `central-loc-m13-diagnosis.md`，中央 actual `01a13323-9503-7942-8536-bb479256d06b`／人类 item `01a13323-95c4-77a2-910c-5a03d6e5bb02`。本包只诊断和修源码，不授权新的 native、restore、下载、B/L 或 Unity。

## 1. 已完成的诊断

固定 F10=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix10`，source head `7be2852330b7a0138be7b4fe3a8bed06924f0da6`。driver 285972B／SHA256 `575184fadf76e5df39d1ed6610f0524687c0e55a6d1533f27502c606322cb41f`；source-freeze 2808B／`57bbe75e1088af7376b018ba794c2c4e91ec4fd3f9ce6e4a769875e061794275`。其八叶、原静态与审查证据保持。

M13 原结果由中央唯一接收，不在这里重作 native verdict。首 restore exit0；第二 restore 在监护捕获包缓存临时叶 `phases/M-tests-generation/env/packages/microsoft.testplatform.objectmodel/17.11.1/qci5yr0f.zv2` 的 FileNotFoundError 后被终止，exit143；第三未启动。第二 stdout 有正常还原进度，stderr 0B；没有独立的 NuGet 错误或沙箱拒绝记录。成功的第一条也有 workload advisory，不能把该提示单独解释为第二条失败原因。

原外层 stderr 1549B／`899a869cdc0a37002bbe3d4e22ce791aac2981b8ddb4ffe7e7ee5c19f5457df2` 只显示 host→execute→consume_m_stage→require；生产异常处理仅保留 `type(error).__name__ + str(error)`，没有内部堆栈。不能声称原运行已定位到下列两处分支之一，也不能将回放堆栈冒充原堆栈。

三个可证伪解释：①活跃缓存叶在枚举后消失，会在生产计量函数重现同类 ENOENT；②沙箱漏规则应有先行写入拒绝，现有日志不支持，且对应版本目录已有 bounded-pattern；③包／依赖真实失败应有独立 NuGet 错误，现有日志不支持。现有证据最支持①；日志缺少错误不等于证明不存在所有其它问题。

唯一确定性回放已完成，命令 `/opt/homebrew/bin/python3 -I -B TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-m13-diagnosis-01/repro.py`，工具 chunk `de8cbf`，exit0，0.065705958秒，native子进程／下载／restore均0。诊断于02:55:44 UTC开始，02:58:39 UTC前完成回放；不再启动第二诊断会话或重跑此脚本。

回放 AST 提取未修改的生产函数，只把枚举根、stage／budget换成新目录夹具；真实新叶在 `is_file()` 返回 true 后被删除，随后的真实 `stat()` 报 errno2。两个 EXPECTED_RED：`inspect_command_outputs` 原1121行、`MaterialWriter.budget` 原1081行。回放脚本4783B／`edc7ff5a6c2addda981350bc8a391c5bf7ff36bc60920149c38969e6e584bc14`；result.json2461B／`dd37234ee8fdeeaec5ddc60b437e7a8dc8ba373574aa04b9b932e5087c6409fe`。两文件留在上述新诊断目录，只读保留。

共同调用点是 `run_material_command` 的活跃监护循环：先 command quota，再 aggregate budget；异常进入 stop_owned。143符合该终止路径，不据此称 NuGet 自身退出失败。两函数在子进程结束后也被调用，`MaterialWriter.budget` 还服务普通写入，因此不能全局吞掉 FileNotFoundError。

## 2. FIX11 的最小设计

修正同一计量模式，不再叠加历史 wrapper。用一次不跟随链接的元数据快照完成每个已枚举叶的类型／计数／字节检查，避免 `is_file` 后再次 `stat` 的窗口。允许一个小型共用私有 helper 替换重复扫描；不造通用文件系统框架，不改路径规范化、下载、restore argv 或包内容验证。

只有**当前活跃命令**且属于其既存 mutable bounded 输出规则的安全叶，才允许枚举后、元数据采样前出现 ENOENT；aggregate budget必须收到当前命令规则，不能把整个 W/M 根视作可消失。固定／受保护输入、其它phase、越界路径、父目录／根异常及非ENOENT错误仍失败。已采到元数据后叶再消失，不再二次读取该叶；已观察到的大小保守计入本次预算。

`inspect_command_outputs` 与 `MaterialWriter.budget` 默认保持严格模式；只在 `run_material_command` 活跃循环显式传活跃上下文，普通 put、结束后的扫描、最终保护清单均不能继承。保留原本允许尚未产生的可选输出语义；不能用此修正把受保护固定输入的缺席判成通过。原链接／路径守卫及文件数、字节数、free-space上限不减弱；对已观察条目用 lstat 区分符号链接和正常类型，链接与非法类型不能伪装成消失。

相邻 `_fix06_guid_snapshot` 的活跃临时叶 lstat 同类窗口可在同一窄机制中修正；只限现有合法 GUID.tmp 规则，不扩名字／父目录。其严格结束态、regular/nlink检查和规则计数保持。若这一分支不能在同一简单修正与预算内覆盖，先明确报告边界，不默默省略或加第二套机制。

driver上限286720B，仅余748B。先测必要替换的净增量：优先复用／替换上述重复扫描，不堆额外历史 wrapper。若无法安全装入，停止产出可执行候选，回报具体减法提案（拟删准确函数／块、字节数、引用证明、保留保护与回归），由主程确认后再实施；本包不授权删除其它历史逻辑、压行或放宽预算。当前故障不授权顺带重构或新增诊断日志框架。

## 3. 唯一作者与写白名单

LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，请求 `gpt-6-astra/xhigh`。M13 actual `01a13327-e085-7760-846e-d6b6014f86b4` 已由wait确认completed/idle；派发后绑定新的真实作者turn。主程单一收件，不另派本地代码审查。

新 F11=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix11`，仅八叶：

- `source/m17_driver.py`
- `source/m17-inputs.tsv`
- `source/m17-write-set.tsv`
- `source/source.patch`
- `authority/prepare.json`
- `authority/offline-checks.json`
- `authority/source-freeze.json`
- `authority/source-receipt.json`

其它仅新建 `docs/team/2026-09-30/loc-m17-fix11-static-checks.py`、`docs/team/2026-09-30/loc-m17-fix11-command-receipt.json`。唯一详细诊断／计划就是本文，不另写身份、回执或设计长文。未来外置 m17-fix11 的 M/W 名称只作封存文本；不创建外置目录或执行材料准备。

继承 F10 精确输入／write-set／封存schema与源绑定规则，仅替换候选版本身份和本节授权改动；不通过新增历史绑定层解决预算。F10及更早所有源、证据、partial、SDK、真实工程源码、配置和Git只读。候选driver≤286720B、八叶合计≤16MiB、命令sidecar≤64KiB；其它原限制保持。若F11或侧录已存在，先核所有权，不覆盖未知成果。

## 4. 验证、封存与返回

先将已完成的两个生产函数红例转为同一生产入口的定向回归，再实施；旧红证据复用，不重复运行旧回放。新脚本调用候选真实函数／调用点，不只测自造helper。仅一次相关静态调用≤10秒、一次源码封存≤30秒；整条外层命令计时，任何失败保留，不自动重试。使用已批准本地Python，无子进程、联网或全量旧测试。

验收逐项报告：

1. 两个生产扫描对合法当前活跃临时叶在枚举→元数据间消失通过；元数据取得后消失不重读、不少计已观察容量；合法GUID临时规则若调整亦有真实函数回归。
2. 非活跃／结束态的已观察叶消失仍失败；受保护固定输入缺失、跨phase／规则外路径、链接、非法类型、父根异常、权限错误、计数／容量超额仍拒绝。遵守原硬链接约束，不降低原检查。
3. 实际 `run_material_command` 仅活跃循环传递窄上下文；结束态与普通写入严格，最终保护检查必达；未删活跃容量监督、stop_owned、唯一执行／无重试、授权、包哈希或任何既有守卫。
4. F10→F11改动仅本设计与必要候选身份，patch可重建，八叶与封签一致，driver／总文件／sidecar预算成立，旧输入未变；写清未运行native，不能称材料已成功。

sidecar绑定实际thread/host/turn、权限来源、完整两条命令／原始工具id与输出、真实整调用耗时／exit、逐项结果、最终八叶封签；若测试或预算阻塞如实返回，不填写虚构成功。主程只核原工具／交付范围／证据一致性，不重复跑测试或做本地代码审查。SOURCE_READY后交中央现有GitHub PR审查路线，代码审查与下一次native均另行接收；主程向中央只报一次影响整体进度的结果或具体减法阻塞。

## 5. 主程减法决定：批准唯一历史测试块退出新候选

LOC actual `01a1333a-a2b5-7980-adf0-7810c64ffc0f` 已completed；只产出预算提案，尚未生成F11、执行静态或封存。所测生产草案净增1805B、超限1057B，不含身份改动；这是草案实测，不声称全局最小。原阻塞sidecar36940B／SHA256 `68a9bef40a682f84ed5eb12e0f804685c92ef9264a9f1d7d7c20848ec7ad39f8` 保留其原始测量、命令和状态演变，允许在同一sidecar续记最终交付。

中央明确由主程在原280KiB内判定最小删减。主程一次只读机械核对（chunk `30b9f0`，exit0，0.116967秒）确认：F10完整driver身份正确；`fix08_checks()`准确3541–3664行、8723B、块SHA256 `ffd494d9aaf339fb977382f2b36744d675e1d6f40b181ea15b6767cd1f7c542d`；无装饰器／AST Name Load引用，唯一字符串引用为3534行历史delta白名单；当前prepare／execute／封存入口及外部FIX10静态脚本不调用它。此核对未执行该函数或测试，不是独立代码审查。

现批准**只从新F11副本删除该完整函数块及其最后换行**，不动周围分隔和其它历史函数；旧F10和更早文件、原测试证据不删。仅对应调整现有delta守卫以精确允许此删除与第2节修正，不能将任意函数删除改成合法。减去8723B后，草案估算279054B、余7666B供必要绑定；最终仍实测≤286720B，不增额、不压行、不外移生产代码。原保护和第4节回归全保留。

LOC新回合按此决定直接完成最小候选；同一八叶／两个侧录文件白名单、静态一次≤10秒与封存一次≤30秒额度尚未使用。最终侧录记录原预算停止与本次主程决定，不新增交接文档。若出现新的实际失败，仅返回那个具体缺口；没有native许可。
