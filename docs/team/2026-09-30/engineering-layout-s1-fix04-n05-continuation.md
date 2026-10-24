# LAYOUT FIX04 / N05 — 明确人类批准后的窄接续

2026-10-05。继承[N04源码](engineering-layout-s1-fix04-n04-source.md)与[N04原生合同](engineering-layout-s1-fix04-n04-native.md)全部行为、保护、预算、成功导出和收件门，仅下列身份/新根差量优先。不是新几何修复或D04，也不重试旧N04证据根。

权源：中央`01a0e401-511d-79f2-b47f-3ab0ade1681b/local`，human turn`01a109c3-90e2-7902-87ba-278c302cfe0a`/item`01a109c3-9188-7c81-ac9e-8d47c11944ea`原文“都允许”，直接回复前turn`01a109ba-2514-7430-a78f-360e0aee4408`中明确“场景保存复开并导出”的请求。主程已通过read_thread读取原userMessage.content及前问；作者/C也先读取这两回合，不仅依赖文件。留档`central-explicit-execution-approval-2026-10-05.json` SHA`a78063af653820665b388dbc96ff626b64a78b16f8b91585b82cb09f89f1451e`。正常工具审批仍适用，拒绝则保留原文并停止，不改道。

主程issuer/唯一收件`01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a109c5-51c1-7312-aaeb-7285a2f6564a`。原源码作者`01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，唯一Unity C`01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，均Astra/xhigh，各登记实际新turn；先作者封S停写，主程核固定身份后才派C。同一新C turn可按本合同窄委托生成I/runner并直接执行一次，无需再机械复批。

## 1. 源码作者：仅换新证据根

- 共享B=`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`前像162754B/SHA`40e282120a95035446b4d56be6c690470d8c084c5ab94aa236c4845d8157ee81`。仅第725行sceneRepair的字符串`/FIX04/N04`替换为`/FIX04/N05`，恰一处、+1/-1；其余所有字节不变。ForceUpdateCanvases、0.5门、身份/驱动/Prefab/五覆盖/保存复开全部不改。T36769B/SHA`1d53ef04c21b6914a9aa9c9042cf98aba78c58e62b77f321edd11888e02acbdc`保持。
- 新根N=`TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N05`首次ABSENT。作者除共享B外只建N/S五叶：`before.json`、`FightMatchAndroidBuild.before.cs`、`FightMatchAndroidBuild.cs`、`source.patch`、`source-receipt.json`，合计≤1MiB；沿N04/S字段记录完整sourceS15After/native4供原生消费。N04全部八叶、共享其它源/Scene/Prefab/meta及旧证据零写；原N04/PExecution/receipt.json固定37416B/SHA`84dccee5c8209455c157a592304175fda8d81039a5fc22a82fe39ef186f4b7bc`，不能混用LOC回执。
- 最小静态核：固定前像/精确一替换反向还原/T与其余14源/native4/旧N04八叶保持，新五身份统一来自最终字节；至多3轮累计30秒，保留实际raw/失败/完整耗时，不重跑旧42或全历史扫描。无Unity、投影sync、网络、测试、Git。返回唯一源码回执后停写，SOURCE_READY不等于native接受。

## 2. 唯一C：复用执行器，一次真实保存复开

- 新S以主程派发最终B/receipt身份锁定。旧N04的I/runner/拒绝回执保持；原N04 runner28309B/SHA`e64d930561e1467167b05e657c4db57909fb312795b668f008371f0d9beaf798`（314非空行）作为最小适配基线。只换N04→N05输出绑定、实际owner/issuer/新human权源、新S/本合同/固定历史/新TMP；保留所有执行/几何/序列化/收尾逻辑，≤350非空行，准备AST/身份最多3轮累计30秒。
- 新增N/I一叶、N/P三叶、N/PExecution七叶、成功后N/PExport两叶与N/S五叶，总最多18叶/192MiB，精确叶名沿N04合同；未运行产物不补造。新I固定新human原消息、实际C turn与issuer，记录原审批拒绝为历史而非本次仍待答。旧N04八叶加入冻结集，继承D03前后原完整保护；所有旧S/I/runner/receipt零写。
- 隔离B仍应是N03前像162700B/`97d062af40ae59ae8fba67ad05e17860616b243d0949b337a0e336ca07d62fb2`，本次只sync新N05 B一次；共享B预检用新S后像，T不sync。D03正式还原的projectedAfter/sharedInputs基准保持，只有获准B身份及最后成功的唯一Scene导出可变化。新Host.Editor真实Csc/DLL必需；Core.Tests未改可沿N03身份，不强求重编，不称99测试通过。
- 六旧TMP（N04新空根`/private/tmp/fm-ln04.Ov6l40lb`加N04合同五旧根）保留，只准一次`mktemp -d /private/tmp/fm-ln05.XXXXXXXX`，沿≤40UTF8/0700/当前用户/无link/空根/16MiB/512叶全部门。投影/Editor/cwd/UPM_CACHE_ROOT/入口/完整保护不变，仅TMP和新输出绑定改变。准备不得启动Unity。
- preflight≤120秒；唯一Editor≤180秒含compile/import，retry0。一次SaveScene→关闭→同进程只读复开；三JSON/141targets/3inputs/6IDs、实际角点与dirty/字节门及只减五旧覆盖保持。全门和owned闭包成功才导出唯一共享Scene，固定前像12900B/`0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd`；Prefab/meta不复制。
- 首错/预算失败封存，60秒自然收尾→fresh owned核→每个仍活owned至多一SIGTERM→30秒观察；无SIGKILL/他人信号/自动回滚或重试。无D04、独立compile/test/discovery、Play、设备、包安装/网络补料、Git。M02可能并行读其独立源码/外置Luban根，不更改或杀其进程。

一次回主程准确turn、实际集合/hash、命令/计时/退出/owned/范围/导出事实；源码＋真实Scene交中央PR4新head审查，后置99与设备仍未包含。成功只记native/export对应门，作者/C不签产品ACCEPT。
