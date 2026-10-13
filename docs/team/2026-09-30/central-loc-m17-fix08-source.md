# LOC FIX08 SOURCE：补齐三阶段项目根 GUID 临时文件

中央actual `01a12ee5-5e38-7863-b291-95e758e24669` / 人类item `01a12ee5-62af-73d3-a5b5-c3827b86dfba`。M09已实际失败，一次native许可已使用；用户既有普通技术修正授权继续。此包只修SOURCE，不运行restore/SDK/OS探针/Unity或下载。LOC唯一作者Astra/xhigh，中央唯一接收，GitHub负责独立代码审查。

## 固定输入与明确批准的差量

执行主程 `engineering-loc-after-m09-failure.md`（13561B，SHA256731a8c1e1c0d995255f549ee6b09a281d1df324df7ff1378498b86f1d81af096），其列出的49个项目、294个最终叶、三个writer是封闭范围。F7基线：`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix07`八叶；driver261874B/SHA9aa2db85274369fa27365c3fa39b120fa68deed0effb527b7fccaa5a06894f48；freeze2807B/SHA5f747b1441f79bb43ff00f17757b36fee2e7c374d3e8dd00570d42d54b835027；headbd92f6bcedaf89751256a4a7e0084d39cd244893已源码限定接收，M仍未接收。

生产差量只包括：各图内项目同时允许P/{guid-tmp}与P/obj/{guid-tmp}，新增49条，共98条（46/26/26，各准确writer）；六类最终输出及原49obj规则保留。M写规则5365→5414，全模型9412→9461，B3888不改。封闭图验证拒绝缺失、重复、额外/图外/跨phase父目录；规则检查不只是放开任意后缀。复用FIX07展开GUID及普通SBPL字符串/转义/锚点。每父最多1驻留regular文件/16MiB，链接、硬链接、父目录安全、输出和进程限制保持，不授权整目录。

中央接受主程272KiB方案，明确仅新候选driverMaxBytes=278528；相对F7源码净增仍≤16384B。execution_budget、seal、后置侧录内容校验及本候选说明保持同值，不修改旧源/旧A/H，不为满足字节数压缩重排代码。八叶总≤16MiB，后置侧录≤64KiB，M512MiB/W4GiB/空闲2GiB、零GET/三restore/180各条/870+30整体保持。超额先报告准确字节和最小差异，不自动再加额。

## 白名单与保护

唯一新根 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix08` 八叶：
source/m17_driver.py、source/m17-inputs.tsv、source/m17-write-set.tsv、source/source.patch、authority/prepare.json、authority/offline-checks.json、authority/source-freeze.json、authority/source-receipt.json。
唯一后置叶 `docs/team/2026-09-30/loc-m17-fix08-command-receipt.json`。本包和主程报告固定为新输入。当前真实作者turn/item须先固定再写源及执行检查。只适配新根/身份、保护引用、所述差量及必要检查；其余生产消费者和启动/侧录强制门不改。

未来M8/W8仅作为路径文字：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix08`、`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix08`；本包不创建。

M7/W7已有中央唯一清单 `docs/team/2026-09-30/central-loc-m09-partial-capture.json`1334742B/SHA46033e3be907cc0dac725e09fe6bf28a2caf08c0ed1781c51df089575acd3f20；3701文件/1328目录/129493955B，紧凑sort_keys无换行inventory摘要8b9abdfdb17a3f3865e454482c7cd2851dbffc9219485cbbbb6e652ea77ac681。固定并按闭合已列对象只读验证，不重新枚举partial/SDK或改清单，保留历史目录时间及未知新叶的限制。

保持全部旧保护，新增固定F7八叶/侧录/已接受源码verdict、M09 E09四叶、授权/A/H/binding/执行包、已存在W7两日志/首restore和material-validation收据、主测试 `testing-loc-m17-m09-native-receipt.json`（5800B/SHAa24db6272fd7a3eb0f210cf9f6bce2b0a1995b05a9ce3bbb491aece246517694）及中央 `loc-m17-m09-native-verdict.json`。原始M09工具6240c5ef，不能误带M08工具dd926858。保留所有失败和真实OS历史证据。

## 验证、封存及返回

一次统一fix08_checks完整调用≤10秒，无native/network/材料写入：通过生产规则生成、命令选择、sandbox序列化、matcher/inspect接口核98父及旧红新绿；三个writer、六最终叶覆盖、图外/跨phase/兄弟/嵌套、名称/后缀、重复规则、symlink/hardlink、驻留数量/字节越界、缺pin/侧录的零写拒绝、三处预算一致、canonical捕获摘要、其余合同批准差量。仅内存fixture/注入探针，记录STATIC/SIMULATED，不把Python匹配当OS实证。复用FIX07已完成3个OS及M08六探针；不重复旧55/81/109等套件。运行前在内存检查数据格式和固定引用，避免再次把序列化格式错误拖到封存阶段。

其后一次SOURCE机械生成/封存完整调用≤30秒；失败保留证据、报告具体差异，不自行重复。工具真正返回后再生成≤64KiB完整侧录，绑定最终八叶/freeze/model/真实author、两条tool完整命令与未截断输出、OS退出与完整墙钟；作者声明不代替原工具。本包不开A/H、不运行M/B/L、不Git、不自签ACCEPT。中央wait/read核收后发布GitHub审查，再提出必要真实验证。
