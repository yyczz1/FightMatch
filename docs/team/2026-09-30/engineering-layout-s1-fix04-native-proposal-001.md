# LAYOUT-S1-FIX04-NATIVE-001：单次Scene原生修复提案

2026-10-02 · `PREPARED_NOT_ACTIVATED`。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`／turn `01a0fccb-bfa4-7fc1-a81b-f8a8a071cdbf`；拟唯一执行者C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`、Astra/xhigh，主程唯一收件。本稿不创建activation、不启动Unity、不复制源码或资产。中央本轮技术指令覆盖[FIX04源码包](engineering-layout-s1-pr4-fix04.md)末尾旧发布顺序：源码冻结→中央另行激活本次原生修复→source＋actual Scene＋证据同一新PR4 head→一次GitHub审查；97发现/T另签，不减门。

## 固定输入与工具

- 来源head `e3d581df42f3a25cfdfc319bca5509f8b5382ada`／tree `52c306b98275111955280da919b45e2806e15ebb`，review5392275691／P1 r4166096874仍 `NEEDS_FIX`。新source15以C冻结 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/source-receipt.json#sourceS15After` 为唯一身份：33609B／SHA `3f20fb6c83f6d18652f08940d4088bcc47abff89e63dbc4e340020db0566519f`。C turn `01a0fcd8-986b-7f61-92c2-72a233830313` 于13:59:59 UTC封存；主程14:02:29 UTC／e209ab exit0一次机械收件核1089仅两源变化、15身份、原6测试全字节、5保护记录/41旧证据/20投影保护及patch内存重放通过（非code review）。builder后像160371B／`ebccb4d9cec5d5d9fffa025fac78c76829fc53a85370dedfee7dd30d93308ca5`；测试后像28866B／`8a043c1b55bf870f8b8768bf15bdcd3525a3776caeee8bdd5ba5cece1f17548c`；各84/60行，共144行。source.patch 16308B／`402429bc8bbf1c6c023dcaa253610f818d1d44f9a62aecacfb49117c7f3525a8`。当前UNCOMPILED/UNTESTED/SCENE_REPAIR_NOT_RUN；不使用旧DLL或dirty HEAD代替。
- native前像沿[发布manifest](../../versioning/2026-10-02-layout-s1-pr/native-p/publication-manifest.json) SHA `1be36cbf7b78985aa565dfff88256a3b195447f492c1c68db6fca26ee96a3a23`：Prefab 1322960B／`0064bbee5e0d84dfdfd627310a9abada5093252bb26cf363d0646b38586f19d2`；Scene 12900B／`0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd`；两自然meta原字节。五个0覆盖仍是当前红点，不能把旧P计为几何通过。
- 复用既有投影 `TestArtifacts/FightMatch/LAYOUT-S1-001/projection` 与package-cache，只机械同步FIX04两源码；其余13源、37资源、2meta、保护7及原始投影输入不变。保留已获例外的隔离 `ProjectSettings/SceneTemplateSettings.json` 3534B／`5baf593374ad67246277a435d8d0ded7167696eef6e05d2d245670e490625d8c`，绝不导入共享工程。原I/P/失败记录/复核、S/FIX01–04原件及全部既有证据保持。
- 工具事实复用[P-001租约](engineering-layout-s1-p-activation-001.json)，不是本轮新检测：Unity2022.3.18f1 Intel x86_64，86759760B／SHA `71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f`。执行前只按此固定身份核对，不升级/安装CLI、Editor、Pipeline或包；Unity CLI技能的原生编辑原则用于本提案，具体运行沿用户限定的既有直接Editor合同。

## 准确一次argv与输出

将下列argv数组原样绑定后继activation；cwd为其projectPath，唯一environment override为 `UPM_CACHE_ROOT=/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LAYOUT-S1-001/package-cache`，不覆盖HOME/CODEX_HOME。

```json
[
  "/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity",
  "-batchmode", "-force-metal", "-buildTarget", "StandaloneOSX",
  "-projectPath", "/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LAYOUT-S1-001/projection",
  "-executeMethod", "FightMatch.Host.Editor.FightMatchAndroidBuild.RepairLayoutSceneOverrides",
  "-fmLayoutContract", "LAYOUT-S1-SYS-001",
  "-fmLayoutInputManifest", "/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/I/activation.json",
  "-fmLayoutEvidenceRoot", "/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/P",
  "-quit", "-logFile", "/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/PExecution/editor.log"
]
```

未来新根均相对 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/`，当前不得创建：主程签 `I/activation.json`（含实际新C/issuer turn、精确argv、FIX04 receipt/source15、资源37/meta2/保护输入/GUID）；builder独占create-once `P/{native-migration.json,native-reopen.json,serialized-targets.json}`；C外层仅 `PExecution/{before.json,runner.py,process.json,process-events.jsonl,editor.log,after.json,receipt.json}`。runner从旧PExecution原件最小适配≤350非空行，apply_patch新建且冻结hash；旧runner不改，不另添helper C#或框架。成功导出仅另建 `PExport/{before.json,receipt.json}`。

## 执行、保护与停止线（须中央激活后）

1. preflight≤120s：C前轮停写，确认新实际turn及所有新根缺席；完整固定输入/Editor身份、共享保护和projection当前清单匹配，源15同步后与FIX04完全相等。新鲜PID/starttime/argv检查无共享工程或本投影的其它Unity/UPM，保留非owned身份，检查空闲≥2GiB。不重复旧I导入、包安装、Prefab迁移或全量历史验证；发现变化停止，不猜测或重新生成。
2. 仅一次Editor启动，启动至exit≤180s（包含新源码导致的必要编译/import与native修复，UPM亦在同一180s内），自动重试0。只调用新Scene入口；不额外compile进程、不第二次Verify、不Play/真实Host/设备/tests/discovery。cache总≤1GiB、projection派生输出≤4GiB、单日志≤8MiB、新证据≤192MiB，监护≤2s cadence；固定51依赖版本/registry/lock不变，下载量UNKNOWN，不重新开启meter或补下载循环。
3. 唯一可变投影资产为 `Assets/Scenes/FightMatchDemo.unity`：Unity API修正既有RuntimeCanvas fileID `1013562322995895578` 的五项陈旧覆盖，原位SaveScene一次→关闭→同一进程只读复开。Prefab（含所有绑定/字体）、其它资源/meta/settings/source均相等；其余Scene修改项及Host/EventSystem/链接/所有fileID/GUID保持，不接受额外层级重建或YAML手编。新header/重排等未列资产变化须报告，不自动接受。
4. 成功要求：真实exit0、无编译/import/package/Unhandled/Assertion或LAYOUT错误；三份native证据存在并绑定当次activation/owner/source receipt，141精确TMP targets/3 inputs/6sceneIDs与既有合同相符；保存后及只读复开实际Canvas局部几何匹配Prefab、lossyScale有限正、五项无陈旧覆盖，复开不改字节。回执哈希等于实际新Scene；独立机械读取已保存Scene确认目标五项不再为0且其它实例覆盖保持。不得只凭exit0或三JSON数量认定修复通过；任何缺项是失败。
5. 首错/预算超限封存，收尾只执行一次：≤60s自然等待，若仍活且PID/出生时间/argv/projectPath/后代身份全匹配，只向本包owned进程单次SIGTERM，再≤30s观察；无SIGKILL/全局kill/非owned信号。每个信号追加记录，后续错误不得覆盖首轮历史；最后新鲜owned闭包证明，无需猜测退出原因。超时后退出仍保留timeout，不重试。
6. owned全清且以上全部门成功后，复核共享源15/native4及其它保护无漂移，才机械导出唯一共享 `Assets/Scenes/FightMatchDemo.unity`，字节必须等于原生投影产物；导出前像仍12900B/原SHA，Prefab/meta不复制。PExport保存双端前后身份与范围；失败不导出、不回滚别人文件。C停写后主程机械收件，将源码+新Scene+原/新证据交中央合成head；GitHub code review、97测试与设备门分别等待，作者不自签ACCEPT。

本提案不授权Git操作、实际Scene运行或新增外部服务；中央必须基于已冻结源/实际owner补签才可执行。旧源码包的执行禁令保留，只有后继租约可激活以上精确范围。
