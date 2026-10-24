# LAYOUT FIX04 / N06 — C 直接用户授权与临时目录现状补签

2026-10-05。继承 [N05 接续包](engineering-layout-s1-fix04-n05-continuation.md)及其 N04/N03 合同的执行、保护、资源预算和成功导出门；本包仅变更下列新身份、证据根及七个历史临时目录的当前环境保护。N05 NOT_RUN 及所有历史证据只读保留。本包不是 D04、容差修正或新一次测试授权。

## 1. 权源、责任与顺序

C 聊天 `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local` 的真实 human turn `01a109db-46f9-7950-86a6-9994c2b10d75` / user item `01a109db-47b9-77a0-ac11-feaff1c43abb` 原文：“授权一次场景保存、关闭、只读复开，并在全部检查通过后导出唯一 Scene”。主程已直接 read_thread 读取该 userMessage.content，不以助手转述或旧中央文件替代；执行端本身持有此直接用户消息。正常工具审批仍适用，拒绝即保留并停止。

主程 issuer/唯一收件 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a109dc-3fc9-7693-be65-e296bf7f6814`。先由原源码作者 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local` 完成 §2 并停写；这是精确重复的一处绑定工作，使用 Luna/xhigh。主程机械核固定身份后派唯一 C 按 §3 执行；C 为 Astra/xhigh，保持上列真实 human turn。C 可在同 turn 窄生成 I/runner 并直接执行一次，无机械复批或新聊天。若 owner 已结束，先报告，不能沿用旧绑定。

## 2. 源码作者：唯一 N05 → N06 绑定

- B=`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` 前像162754B/SHA `ca6c421b4c0713fc7cda43c493f935135fbce144e523bb81ee30a1ca08c84b1f`。仅第725行 sceneRepair executionRoot 的 `/FIX04/N05` → `/FIX04/N06` 恰一处，+1/-1，所有其他字节不变。T36769B/`1d53ef04c21b6914a9aa9c9042cf98aba78c58e62b77f321edd11888e02acbdc`及其余14源、native4不变。
- 新根 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N06` 首次必须缺席。除共享B外，仅新增 N06/S 五叶：`before.json`、`FightMatchAndroidBuild.before.cs`、`FightMatchAndroidBuild.cs`、`source.patch`、`source-receipt.json`，合计≤1MiB。沿用 sourceS15After/native4 完整 schema，记录实际作者turn、以上权源和 issuer。
- 固定旧N05精确八叶；旧源码回执36253B/`c6e926750c66ff8730a9465dbf158acf032a798510a223986b357bb97ac2a8dd`、拒绝回执38538B/`24d60a52d18c8de5139b7e55590e212b5118c55133a2561343cb328462bbec9e`。旧N04八叶和更早证据保持。仅做固定身份、精确一替换正反向、其它14源/native4与旧N05八叶及新五叶预算核，最多3轮累计30秒；原42、历史长验证、临时目录扫描不重跑。无 Unity、投影sync、Scene、网络、Git。返回 SOURCE_READY 后停写；不签产品 ACCEPT。

## 3. 唯一 C：新环境现状与一次原生执行

### 3.1 仅七个历史 TMP 的基线补签

C 本 turn 的只读 `exec-1e349b6a-da16-4442-ad06-ec3a9abe5cb9` / chunk51c306 首次在 N02 路径得到 FileNotFoundError，保留失败；随后 `exec-10856468-fc08-4176-86be-6a6071ba4948` / chunk b281cf exit0 列出了全部根元数据。六条缺席路径精确为：

1. `/private/tmp/fm-ln02.9rDNmQQL`
2. `/private/tmp/fm-ld01.voVEfQ7Y`
3. `/private/tmp/fm-ld02.3pV7HUAz`
4. `/private/tmp/fm-ln03.808Kd2tY`
5. `/private/tmp/fm-ld03.jtjZZsWG`
6. `/private/tmp/fm-ln04.Ov6l40lb`

只把以上缺席视为新执行开始前已观察到的环境现状，原因未知；不称原“历史目录保持”门通过，不覆盖旧清单，不重建/删除/恢复这些根。I 保留原六个 oldTemporaryRoots 完整历史记录，并新增固定 `absentLegacyTemporaryRoots` 六路径清单，附本次只读失败和完整成功 raw/命令/actual turn。每次 old_tmp_guard 遍历时，仅对这个显式清单用 lstat；仅 FileNotFoundError 可继续，任何重现（含文件/目录/悬空symlink）或权限/其他错误都失败。禁止广义跳过不存在路径或吞掉 OSError。

第七根 `/private/tmp/fm-ln05.bxLSmcT6` 必须存在，root uid501/gid0/mode0700/device16777232/inode34065318/birthtime1791166245.8622372，沿N05拒绝回执空清单。将其加入 oldTemporaryRoots 并使用原完整身份/清单比较；不得因六缺席规则跳过它。当前TMP另行一次 `mktemp -d /private/tmp/fm-ln06.XXXXXXXX`，沿≤40UTF8、当前用户0700、无link、初始空、16MiB/512叶、最后保留规则。祖先与当前root的原安全门不变。

### 3.2 允许的执行器差量和验证

以 N05/PExecution/runner.py 28309B/SHA `71c07f2d83a14c36e1719dcad1403ee8ab4ea44d608dd9e85a8d854b58c16d65`、314非空行为基线。只允许 N05→N06 活动输出绑定、实际 owner/issuer/直接 human、新S和本合同/hash、新TMP、旧N05八叶冻结集，以及 §3.1 的 old_tmp_guard 精确缺席分支；所有其他执行/几何/序列化/预算/收尾/导出逻辑保持。≤350非空行，最小适配正反向比较/AST及固定身份最多3轮累计30秒；缺席分支以不触盘的 stub 检查 FileNotFoundError 接受、任何存在对象拒绝、PermissionError传播，记录 synthetic-only，不运行历史测试。

I/P/PExecution/PExport 精确叶名与 N05 相同，新根含S最多18叶/192MiB；只向实际生成的证据写真实结果。N05八叶共512800B并入冻结集；S停写后由主程消息绑定最终B/S回执。投影B仍 N03前像162700B/`97d062af40ae59ae8fba67ad05e17860616b243d0949b337a0e336ca07d62fb2`，只sync新B一次，T不写。D03正式还原基线、共享源/资产/meta/资源/依赖及所有持久证据门原样保持；临时目录补签不是持久文件完整性豁免。

preflight≤120秒、唯一 Editor≤180秒含compile/import、retry0；原Intel2022.3.18f1/固定Editor身份、projection/cwd/UPM_CACHE_ROOT、无其他Unity/UPM检查不变。新Host.Editor真实Csc/DLL必需，Core.Tests可沿N03，99项仍未授权运行。保存一次→关闭→同进程只读复开；三JSON、141 targets/3 inputs/6 IDs、0.5角点、Prefab/dirty/byte及只减五旧覆盖门全部不变。所有门/exit0/owned闭包成功后才逐byte导出唯一共享Scene，前像12900B/`0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd`；其他共享产品零写。

首错/预算失败保留，不补跑或自动回滚；自然60秒→fresh owned核→每个仍活owned最多一次SIGTERM→30秒观察，无SIGKILL或外来信号。无 D04、独立compile/tests/discovery、Play/device、安装/网络补料/Git；不触碰并行LOC域/进程。最终报告真实turn/输入/集合hash/时间/退出/闭包/缺席或存在TMP保护/导出事实，主程唯一wait/read接收，后置PR新head、99测试和设备验收保持。
