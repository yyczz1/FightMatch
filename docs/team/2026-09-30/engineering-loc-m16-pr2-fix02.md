# LOC M16 / PR2 — FIX02 生成与消费一致性

2026-10-04 · SOURCE_ONLY_ACTIVATED。中央依据 `central-resume-scene-loc-2026-10-04.json` 的人类授权，明确批准这两项必要返修，无需重问用户或等待另一轮设计。主程issuer `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a10641-a816-71d1-919b-97413f71ea56`；唯一作者既有LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，Astra/xhigh，记录实际新turn，唯一回主程。Scene D01完全独立，勿写其范围。

## 固定输入与新叶

已审head `8533cb5270c0ba324a0af5a51fb078e096189cce` / tree `0c1908fa032af82d480f7861ffec034555eede03`；review5405309989，两个P1原文见 `testing-loc-m16-fix01-pr2-final-review-receipt.json`，发布身份见 `loc-m16-fix01-publication-receipt.json`。主程已读GitHub原文的QA机械回执，本交付当前唯一判定 NEEDS_FIX，不再做本地代码审查。

F01=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m16-fix01`；driver172015B / `11f56aacf087c141277bacd0d929bafcaf903ffe534fea699349f12931ffb2a3`，source-freeze3881B / `d84ef5066b7a7e9450c372fa46e252a78864dfad75b2902b111bc238aa7ca0dc`。F01八叶及原E16六叶全部只读，原失败、23/46历史检查保持；先核封签和精确集合，不重跑原prepare。

仅新建 F02=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m16-fix02` 下八叶：`source/m16_driver.py`、`source/m16-inputs.tsv`、`source/m16-write-set.tsv`、`source/source.patch`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/correction-receipt.json`，总≤16MiB，写前F02必须缺席。可机械复制固定只读底稿后apply_patch实际差量；manifest/封签的生成由本次准确prepare输出，不能手填通过证据。

## 两项必须修正

1. **r4176965421**：真实生成seal与`verify_central_activation`消费要求一致。`executorComplete`、`materialWriteSetComplete`等状态必须由实际生成流程给出并有证据；不得只给正例手填freeze字段、默认缺字段为真或移除既有检查。缺失旧材料目录是独立未就绪事实，不伪称材料可用或真实执行已通过。
2. **r4176965430**：生成的inputs/write-set必须由实际消费入口同一确定性生成模型逐字重现。F02的owner/root/source声明、实际派生输入、历史保留与当前写权限分开；不能旧表拼几行后令消费端重算另一集合，不能放宽byte相等门。源码/表/封签自引用必须明确排除循环或分层封存并由真实调用验证；不可把后验观察回填成独立期望。仅做这两finding及必要F02身份/封签变更，不新建通用框架或更改工具/包/许可选择。

## 有界离线验证与回执

- 仅Python进程内，初次+最多一次必要纠正、累计≤60秒；子执行、restore、dotnet、Unity、网络、材料化、Git写均0。只补新改动的针对性检查，原23/46作为历史，不无因全部复跑。可以复用现有纯函数seam；不得触发OS隔离、真实execute外部动作或靠伪造pin解开生产门。
- 正例必须经过**真实生成seal/tables → 实际消费验证入口**，不是手填同形对象；同一冻结模型重复生成应逐byte相等，并覆盖缺字段、篡改输入/写集合、root/owner漂移等反例。可用明确标记的纯内存材料读取适配测试声明一致性，但不能将它作为丢失的真实材料已恢复；无论测试通过与否，生产`TRUSTED_CENTRAL_ACTIVATION`仍为空且execute必须在任何外部动作前拒绝。
- 回执逐P1给实际修改点、生成/消费调用链、正反例与准确结果、所有尝试/命令/耗时、旧14叶前后身份、F02八叶bytes/SHA/patch以及owner/issuer/授权。seal身份与源/表层次可验证，无自指hash循环。明确SOURCE_ONLY、材料可用性未闭合、产品UNCOMPILED/UNTESTED、未来真实activation及新head GitHub审查仍待完成；作者不签ACCEPT。
- 所有其他产品/配置/工具材料根与共享索引禁写；不得恢复旧/tmp、扩白名单、下载或改已有证据。缺输入若确实阻挡本次一致性修正，报精确缺槽和最小技术方案，保留已完成独立部分；普通本范围修正继续至实际回执，主程收件后交中央发布。
