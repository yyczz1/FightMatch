# LAYOUT FIX04 / N03 — 单次保存、关闭、只读复开

2026-10-04 · PREPARED_NOT_DISPATCHED。仅在主程接回N03源码正式完成、机械核八叶并派发固定S receipt/两后像后激活。直接人类授权见 `central-resume-scene-loc-2026-10-04.json`；中央已明确批准真实Overlay技术限定及源完成后直接交唯一C，不新增审批层。继承 [原生提案](engineering-layout-s1-fix04-native-proposal-001.md) 与 [N02](engineering-layout-s1-fix04-n02.md) 的完整保护/预算/收尾/导出，仅替换下列明确差量。

issuer/唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a10641-a816-71d1-919b-97413f71ea56`；唯一C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh。C记录实际新turn，沿D02既有机械方式在同turn创建I并直接执行；这是本包对旧“主程独占I写入”的明确窄委托，issuer/人类权源不可更换，源版本必须等于主程派发固定身份，不得自行选择新源或扩写集合。

## 固定基础与变化

- N=`TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N03`，S由源码作者封存后只读；N/S/source-receipt.json的sourceS15After/native4以及主程实际dispatch SHA共同锁定版本。共享两源已合法改变；只同步B=`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`、T=`Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs`到隔离layout projection，不复制其它13源/资产/配置。
- 投影当前B160434B/5dda3f055c7f78016d91dc533382b53154d8dd1412abcf03913c64f0aeaaaac0、T28866B/8a043c1b55bf870f8b8768bf15bdcd3525a3776caeee8bdd5ba5cece1f17548c。前像基于D02/PExecution/before.json的projectionPreSync/sharedInputs；共享baseline仅两源更新为N03已封后像，投影afterSyncExpected仅两源更新。不可用D02诊断候选或旧N02最初sync前像。
- D02主回执45711B/850788122bad4566bce43f8a8939e5d0601be3e08fab07a61c0fc96f180891b7，after380301B/5bf2df055fc11542a764c78a68c65032618ea3ff8a6a12887bf9158ee0bbd7f3；12叶及其85固定旧输入全部保持。Scene仍12900B/0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd、Prefab1322960B/0064bbee5e0d84dfdfd627310a9abada5093252bb26cf363d0646b38586f19d2，两原meta不变。原1089共享/996投影/513GUID/51依赖/37资源/2自然meta/7保护保持，Scene是唯一许可资产差量。
- 旧三个TMP及其清单保持、不复用：N02 `/private/tmp/fm-ln02.9rDNmQQL`、D01 `/private/tmp/fm-ld01.voVEfQ7Y`、D02 `/private/tmp/fm-ld02.3pV7HUAz`。C仅一次新建 `mktemp -d /private/tmp/fm-ln03.XXXXXXXX`；canonical≤40UTF8字节、当前用户/0700、无symlink/初始空，≤16MiB/512文件，最后保留。

## 白名单和一次执行

只新增N内 `I/activation.json`、`P/native-migration.json`、`P/native-reopen.json`、`P/serialized-targets.json`、`PExecution/before.json`、`PExecution/runner.py`、`PExecution/process.json`、`PExecution/process-events.jsonl`、`PExecution/editor.log`、`PExecution/after.json`、`PExecution/receipt.json`，以及所有门成功后 `PExport/before.json`、`PExport/receipt.json`。加既有S八叶最多21叶，总≤192MiB；未执行的产物不补造。I绑定当前实际C/issuer、人类权源/本合同/固定S/source15/native4/资源保护/新TMP与完整argv。

argv严格沿N02/PExecution/process.json的唯一Editor数组，仅I/P/log前缀改N03；cwd仍layout projection，Editor86759760B/71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f，Intel2022.3.18f1；唯一env差量为原UPM_CACHE_ROOT与新TMPDIR。源方法仍RepairLayoutSceneOverrides。runner从N02/PExecution/runner.py（26284B/2a6cfc84a21e4afe872554050d9e295bd41506b1b3a9d9eea2d569523b29b5fa）最小适配≤350非空行，保留原保存/复开/成功导出规则，不沿D02诊断停止或恢复源码分支。源码与I/runner所有bytes/SHA/blob按最终字节机械生成，runner AST和identity绑定准备检查最多3次累计≤30秒，实际普通制作错误窗口内处理，全部尝试留真实计时；无差量重跑禁止。

preflight≤120秒，完整输入/Editor/新鲜进程与投影锁检查，无其它Unity/UPM才sync两源各一次。Editor最多一次≤180秒（含必要compile/import/UPM），retry0；监护≤2秒、cache≤1GiB、派生≤4GiB、每log≤8MiB、空闲≥2GiB。原D02 Host.Editor.dll是诊断产物；本次必须有N03新源真实编译证据，不得复用旧DLL作通过。无独立Compile、测试/发现、Play/Host/设备、包安装、网络补材料或Git操作。

## 成功门与失败线

1. 仅Unity API原位删除现有RuntimeCanvas/source fileID1013562322995895578的五条陈旧scale/anchorMax覆盖，其它override/顺序/Host/EventSystem/层级/所有fileID/GUID保持；Prefab/其它资源/meta/配置零改。原位SaveScene恰一次→关闭→同进程只读复开，复开字节不变，不增加probe或第二次Verify运行。
2. 此包明确覆盖旧提案第4项仅“实例局部几何匹配Prefab”的错误子句：Prefab源仍scale1/anchors0..1/有限正world；真实Scene实例必须精确root/Overlay/本Canvas驱动归属、有限正local/world/scaleFactor/rect/pixel覆盖以及无五陈旧override。几何/身份断言不能删除、临时关闭Canvas或用人为赋值过门。其它141文本/3input/6sceneIDs与原全部范围门不变。
3. 真实exit0且无编译/import/package/Unhandled/Assertion/LAYOUT错误；三native JSON完整绑定本次I/owner/S，保存与复开实际Scene同hash。外层独立读取已保存Scene确认只减少那五条覆盖、其它实例修改/对象语义保持，不能仅看exit0/JSON数量。任何额外资产序列化差量不自动接收。
4. 首错/预算封存，不重试、不导出；一次≤60秒自然收尾→fresh PID/start/argv/后代归属核定→每个仍活owned最多一次SIGTERM→≤30秒观察，禁止SIGKILL/非owned信号。旧失败和TMP保留。N03两源属于正式候选，失败也不自动恢复到N02或删除编译产物；如投影Scene已被原生保存，准确封存实际后像，不回滚/导出失败场景。
5. 只有上述门全部通过且owned全清，复核共享两源/S/native前像和完整其它保护无漂移，才逐byte导出唯一共享Scene；导出前像必须等于固定12900B/原SHA，Prefab/meta不复制。PExport记录双端前后及精确白名单；源码+真实Scene一并交中央PR4新head。GitHub审查、后置99限定测试和设备验收均未包含在本次，不以本次原生成功签产品ACCEPT。

回执写实际21叶中的已生成集合/身份、所有命令/计时/退出/信号、前后保护与真实Scene几何验证/序列化差量/导出结果；不复制大段旧历史。完成一次回主程并停写；主程唯一收件，中央接主程，不自行续跑测试或向其他角色发信。
