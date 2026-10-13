# M12 目录误判诊断 / FIX10 SOURCE 小包

2026-10-13。主程唯一诊断/执行范围记录，派发后固定；结果进入最终命令侧录，不追加身份或接收文档。依据用户普通组内工作授权及中央本次明确派发，LOC在原280KiB范围内完成新SOURCE；M12一次真实执行额度已经使用，本包不授权重试。

## 已证根因

M12首restore exit0/5.648133秒，随后 `GateError: unexpected fallback package folder`；整轮114.755321秒/host1，后两条未启动。原回执 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m12/receipt.json`（39510B，SHA256 `b47ea4ca686273d61fc2a7dd054fcf780704ff6e966608b94cd36cfcd96ff08d`）。中央接收真实运行，本包不重复整轮验收。

首项目为 `src/Luban.Bson/Luban.Bson.csproj`。其实际 `project.assets.json` 在 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix09/phases/M-runner-generation/source/src/Luban.Bson/obj/project.assets.json`，17921B/SHA256 `fbf1498a9719bffdb0e6ed9fc05288bee25b1a49fa4fd4e5744eb5b14a2ec477`。

```text
实际唯一 packageFolders 键（P）：
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix09/phases/M-runner-generation/env/packages
当前校验器预期：P + "/"
project.restore.packagesPath：P
project.restore.sources：仅同一W9/feed
project.restore.fallbackFolders：字段缺席
```

固定 NuGet.Config 的 fallbackPackageFolders 只有 clear，命令明确 `RestoreFallbackFolders=`。实际 packageFolders 只有一个键，且精确等于指定缓存目录；失败是末尾斜杠格式误判，不是发现了额外目录或路径逃逸。F9 driver第724行只接受带斜杠字符串；其1352行synthetic fixture也只制造带斜杠形式，故未覆盖真实输出。这里不声称余下完整还原已经通过。

## 固定输入 / 最小改动

F9=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix09`；driver284941B/SHA256 `358f7d481f61f6039908f6e5e1c504b1522b282ac8094d99c557a66b1f031688`，freeze2808B/SHA256 `69bc7209daa0e25d31d2a4a4426625cf45cd427b6001cef8a3cf12feb55d8605`。已审head `5e2d3790d371659552c9bb24084252c70972f474`；GitHub完成结果6136280419未报重大问题，非正式APPROVED。QA `testing-loc-m17-fix09-review.json`（9161B/SHA256 `2e9eab8d2d8bfa669ff1028bbefc6d52375125529da924cc16a95c41b29db5c8`）源码限定ACCEPT；不等于M12通过。

生产差量仅修改 `validate_restore` 的 packageFolders门：要求单个指定目录，只接受精确P或精确P加一个末尾 `/`。可用两个singleton键集合比较，避免通用路径归一化。不能用resolve、normpath、casefold、无界rstrip或前缀匹配吞掉其它差异；同时出现P和P/也应拒绝。保留NuGet元数据哈希、固定归档、packagesPath/source、图/包/locked-mode与双还原比较约束。fixture默认模拟实际无斜杠，并单独覆盖带一个斜杠合法形式。

仅另适配当前候选根、身份、输入保护、delta/封存入口及命令侧录绑定；280KiB上限不变，当前仅余1779B。直接复用已有固定输入/保护/封存结构，避免再复制一套历史包装；不删有效检查或外移生产逻辑来凑尺寸。若实际必要完整差量超限，给准确字节与最小缺口再返回，不能自行增额。F9及所有更早源、E12/M9/W9和已有失败字节只读；不枚举旧partial，不扫描SDK，也不建立全树已保护的无证据结论。

## 作者、白名单及过程

LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local` 是唯一作者，Astra/xhigh；其M12回合 `01a132f2-afd1-7233-874f-dbd4f03f78ad` 已completed/idle，才接此SOURCE。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local` 是唯一组内接收者。实际新turn/item及工具放最终sidecar。当前任务是FIX10，不是历史权限答疑；只读本包及相关代码段，不载入长聊天/整份driver历史。

唯一新根 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix10` 八叶：`source/m17_driver.py`、`source/m17-inputs.tsv`、`source/m17-write-set.tsv`、`source/source.patch`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/source-receipt.json`。另允许本目录 `loc-m17-fix10-static-checks.py`、`loc-m17-fix10-command-receipt.json`；本说明由主程固定，LOC不改。全部旧文件、共享索引不写；不Git、安装、host/restore/下载/SDK探针/Unity/B/L，不改产品/API/依赖版本或权限。

未来外置根仅作文字：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix10` 和 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix10`，本阶段不创建。driver≤286720B，八叶≤16MiB，后置sidecar≤64KiB。

## 必要差量验收与返回

先写读取真实assets字段形状的回归，再作最小修正。原M12是已保存的真实红灯；统一静态中验证准确F9旧门拒绝实际无斜杠键、新门接受无斜杠及一个斜杠。必拒：空集、非对象、额外目录、同时两种键、兄弟/父子/跨phase目录、相对路径、dot/dotdot、重复分隔符、多个末尾斜杠、大小写变化、URI/反斜杠伪路径。FIX09哈希分域和其余路径门保持；53项旧测试不全量重跑。

源码固定后唯一一次相关静态差量完整≤10秒，入口 `/opt/homebrew/bin/python3 -I -B docs/team/2026-09-30/loc-m17-fix10-static-checks.py`；通过后一次SOURCE机械封存完整≤30秒，入口新driver的 `prepare`。旧OS证据复用，测试标STATIC/SIMULATED；新测试源码/实际输入身份及结果必须封存。失败保留真实证据，先组内定位，不机械重跑或扩大额度。

最终sidecar在工具返回后记录完整命令、未截断输出、退出状态、完整墙钟、准确八叶/freeze/model、本次身份及保护限制。final只交必要结果和证据路径。主程核输入/交付物/原工具后将准确候选交中央发布GitHub新head审查；不做重复本地代码审查，不将静态或历史审查当作材料通过。
