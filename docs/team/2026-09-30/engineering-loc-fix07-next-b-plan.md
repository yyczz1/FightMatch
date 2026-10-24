# FIX07 后继 B-SOURCE01：独立入口设计差量

2026-10-12；主程 actual `01a12eea-6d56-77c0-994b-f4e0aeeeee79`，thread `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`。依据中央 turn `01a12ee5-5e38-7863-b291-95e758e24669`、人类 item `01a12ee5-62af-73d3-a5b5-c3827b86dfba` 的静态准备派发，中央唯一收件。本轮未查看活动 M7/W7/E09；不判断 M09 结果，不扩大其一次 native 许可。

## 1. 取代旧引用，不改历史文件

本页只修订[原 B 就绪方案](engineering-loc-m07-next-b-readiness.md)的源码组织、输入版本及待绑定项；原文件保留。其R2 driver/model、M5/W5、M07接收前提全部降为历史，不用于新B派发。当前唯一基线 F7=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix07`：

- driver261874B / SHA256 `9aa2db85274369fa27365c3fa39b120fa68deed0effb527b7fccaa5a06894f48`，距262144B上限仅270B。
- freeze2807B / SHA256 `5f747b1441f79bb43ff00f17757b36fee2e7c374d3e8dd00570d42d54b835027`；model `8c66978b5f7a86d77e158baf7b34b7f1c7ab9616987b20e263ff70ec7e6a5f58`。
- head `bd92f6bcedaf89751256a4a7e0084d39cd244893`；[源码判定](loc-m17-fix07-source-verdict.json)为SOURCE_ONLY ACCEPT，非材料接收；固定侧录35151B / SHA256 `5c027e28a18b37d2e8abca762bbb84b6304d276840b824f75f04aa7faea56797`。

静态源码仍是M-only execute；B只有合同及结果校验函数。**不复制整份F7再追加、不压缩旧源码腾字节、不提高M的256KiB限制、不把M入口放开为B。**

## 2. 推荐两个新 Module，保持一个外部 Interface

推荐独立 E_B=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix07-b-source01`，两个新增代码文件合计≤256KiB，B源码封签整体≤16MiB；这是明确的新B包组合上限，原M八叶和原M上限完全不变。

| 新文件 | 单一职责及 Interface |
|---|---|
| `source/b_driver.py` | 唯一B生产入口 `execute(argv)`：独立B授权/九叶与侧录校验、M接收门、有限staging、53项串行调度、真实进程监护及候选收据；默认trusted pin为null。源码检查/封签只走明确SOURCE-only分支。 |
| `source/b_contract.py` | 只读合同 Adapter：由已核验F7源码/prepare字节生成固定B投影，并提供既有校验器的明确调用；不启动进程、不创建M/W、不扫描SDK。数据/校验与执行的 Seam 在此，不抽象通用调度框架。 |

Adapter优先复用不可变F7的restore/图包/TRX/输出比较校验，不复制26万字节历史执行器。建议未来加载方式为：先验证固定文件bytes/SHA，再以非`__main__`模块名加载已核验字节；禁pycache、任意路径/自动发现、顶层额外动作，不调用其main/prepare/model/consume_source/execute/run_material_command，不设置或替换原M模块的pin/全局根。F7中只有被逐名核对的结果校验函数可被Adapter调用；若某函数必然触发不允许的物料发现/源码重建，则报告具体依赖，不增加通用AST提取器或绕过其校验。

`b_driver` 顶层仅标准库与固定数据；在production `execute`内先用独立pin校验A，再验证B freeze/全部叶子/后置侧录及F7来源，才加载b_contract及F7模块，避免未验辅助代码先运行。先验证M接收与B专属执行合同，再允许B写入/任何子进程。原M pin始终关闭。SOURCE-only小fixture可注入内存数据/clock/runner，但不是生产跳过开关。

沿现中央host的pin/`execute(argv)`调用 Interface设计，成功打印 `B_CANDIDATE_NOT_ACCEPTED` 并返回整数0，失败非0；适配与源码闭集须列入review和纯fixture。不能因host只核入口文件便漏核b_contract。若现host不支持此结果协议或需要逻辑变化，另签准确模板文件，不悄悄扩本包。

## 3. 中央可签发的精确新文件与 owner

唯一源码owner建议LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`、Astra/xhigh，由中央登记实际新turn/item；不得打断其活动M09回合或并发争用其执行职责。本轮不派发。

仅E_B内九叶：`source/b_driver.py`、`source/b_contract.py`、`source/b-inputs.tsv`、`source/b-write-set.tsv`、`source/source.patch`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/source-receipt.json`。另一个后置叶为 `docs/team/2026-09-30/loc-m17-fix07-b-source01-command-receipt.json`，≤64KiB。

九叶是**新B契约**，freeze封其余八叶；不能调用M的八叶seal/consume函数后偷偷改计数。B输入表固定F7八叶、SOURCE判定、后置侧录、本文及下一 `central-loc-m17-fix07-b-source01.md` 的准确身份；B投影必须从F7生成，不能独立编辑成第二权威。source.patch只陈述新B文件及来源，不产生旧M修改。新根占用即停，是否缺席由未来owner在签发范围内检查。

不得改F7、任何旧材料/证据/A/H、产品源码、配置、Program.cs、Generated、SDK、Packages/ProjectSettings；不得因分文件把原单份256KiB变成每个模块各256KiB。

## 4. 当前即可做与M接收后才可绑定

中央签SOURCE包后，可不依赖M结果编写闭锁B代码、从F7生成合同、做小内存fixture、SOURCE封签和GitHub审查准备；所有读写留在明确静态输入/新E_B域。无需读取或预建M7/W7/E09，不要求整个M/W不存在，也不扫描SDK。材料依赖未绑定不阻止源码准备，但必须阻止production执行。

仅从F7模型取未来路径文字：M7=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix07`，W7=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix07`。本轮没有访问两根。后续B只创建自己的三个phase/control/log/receipt叶，M材料和W内M证据只读；不迁移、不清理原根。

以下全部**待真实M独立接收后绑定**：中央M verdict的path/bytes/SHA及ACCEPT范围；实际M09 owner/tool/退出与子树闭合证据；M7 manifest及source/model来源关系、31包feed、runner/tests锁/assets/部署/许可清单身份；W7实际B目标无污染情况。材料实物与F7不符则报差异，不修改SOURCE身份“适配”假结果。

另须未来B专属授权记录固定B源码/模块闭集、后置成功工具记录、独立GitHub准确新head、M接收与manifest身份、精确命令/写集、明确整体墙钟/收尾及实际owner/argv。不能接受null、历史M pin或调用者自填摘要；未有新B许可时默认关闭。

## 5. 需中央明确的一个合同差量与既有限额

静态F7仍有53条B命令、3888条B写规则，但 **B的bounded-guid-tmp规则为0**；FIX07修正只覆盖M。推荐中央在B-SOURCE01明签：为B-pass-1、B-pass-2、B-official-tests的23＋23＋13个项目obj新增59条同型有限GUID规则，使用已经修正的展开表达，仍为每obj1驻留文件/16MiB，只授予各自restore writer。B新投影因此拟为3947条规则；不回写F7的M/B合同，不放开整个obj。此推荐须明确签发，不声称59条已批准或已原生验证。

其余原限额不变：3条locked restore各180秒；2 publish＋1 build＋1 unfiltered官方test各600秒；12三侧场景＋33个005动作各300秒；1个assert-controls为60秒。每条maxStarts1/retry0/killGrace5秒，两路日志各4MiB，4条build/publish/test的maxChildren128、其余0，单并发、禁网、本地feed。B执行器必须真实记录/收拢允许的子树，不能沿用M的childStarts常量0或fork全禁实现。

16500秒timeout合计及16765秒含声明grace合计仅作核对，**不是整体B执行许可**；F7未提供B wallSeconds，整体墙钟和收尾仍由中央单独签发。本页不给新数字，M900秒不复用。W≤4GiB/200000文件、M≤512MiB/25000文件、空闲≥2GiB保持；共享M/B/L收据逐叶另签，作者只交候选，不能写独立B ACCEPT。

## 6. 必要review与小fixture

新包需GitHub PR Code Review核准确B head、两代码文件的加载信任链/九叶封签、F7只读复用、M依赖门、59条明确差量、进程/写入限额与host协议；F7 SOURCE ACCEPT不能继承为B实现ACCEPT。沿既定QA与中央接收职责，不新增本地审查代理。

未来仅一次新小fixture调用（建议中央明签≤10秒）＋一次SOURCE封签（建议≤30秒），旧套件不重跑：辅助模块改字节/错freeze/侧录缺失在加载或写入前拒绝；无/错M接收及未签总时限拒绝；有效内存M合同到唯一B调度seam；53项顺序/预算/预期退出/0或128子启动/超时停止与失败收据；49条M规则不变、59条B临时规则归属精确；TRX/锁/部署/许可证/三侧结果失败不晋升ACCEPT。模拟clock/runner和文件集不能冒称真实OS/子树验证。

真正B启动还需：上述源码门、真实M接收、B单独执行许可、目标/存量保护及新B OS策略/子树监护就绪。已有M正则实证可复用，但B允许子进程的策略与59个B位置未因此原生通过；若仍需小型native能力验证，先由中央另签隔离fixture，不挪用M09次数、不直接拿53项长执行试错。B接收后才讨论L/正式LOC-A；LOC-IMPL-B、Host、Unity、Android仍另签。

本方案初稿落盘并核对66行时耗时282.043秒，≤5分钟。仅新增本文；未import driver、测试、源码/配置写入、SDK扫描、native、Unity/Git或跨聊派发，未读取M7/W7/E09。后续M09诊断另起计时、另写指定文件，不修改本方案。
