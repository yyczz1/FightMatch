# HOST-NEXT-001/M03：PR17 两条 P2 最小纠正

2026-10-08。GitHub PR17 head `521f1c78c70f4e88e9139ff543202c22dcf67133` 的独立审查已由主测试 actual `01a11779-f5f2-70a2-a22e-9726f9d68fe3` 完成收回；回执 `testing-host-next-pr17-review-receipt.json` 7443B/SHA `54b2dfb9d954d1f222011e74752671df152f748599f8eb0c9bbeb193115f4afe`，两条P2待修，中央唯一结论 NEEDS_FIX。原M02实际观察/恢复及M01失败均保留。

唯一C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh，fresh actual，中央唯一完成接收。主程/QA不重复转发或本地审查。现依据用户持续开发授权修技术缺陷并验证，无需另问用户。

## 固定范围

复制M02两个工具源至新E=`TestArtifacts/FightMatch/HOST-NEXT-001/M03/`：probe37281B/SHA `135250f5b151ab2e759620385eef6b7c3a2a0a0e0a3b826046d61bd8c2540596`；runner40966B/SHA `138791f683160d9d83ce31752997fe175055f23a7178a1c0acdf1d2a91494328`。只修以下两条、加入对应小型反例核验及新身份/路径。沿已签M02/R1全部清理、严格SDK ADB、连续监护和恢复规则，不另改这些已经通过的部分。

产品1012输入、N11场景/meta、probe固定meta及加入probe前1014闭包不变；新probe源码摘要/完整1016清单随实际源更新。原M02 receipt150204B/SHA `85b749d31420de6fef28c83321b4709e71acfe20e3b1f299bafff1bfbfce1d1f`、原observations81625B/SHA `d1a9ba4005c711f48745f1a8cf60cb11c1bd56964d68d16f2c6f861453536194` 只读。新D、新nonce、新TMP `fm-hn03.XXXXXXXX`；P由原1008启动，最终恢复1008；M01/M02、旧TMP/Git/真实档不改。

E叶白名单沿M02（含correction.patch）。反例结果放既有preparation.json／observations.json的独立字段，不新增通用测试框架或产品测试。probe≤430、runner≤480非空行。

## 两条纠正及有意义的验证

1. [r4210135624](https://github.com/yyczz1/FightMatch/pull/17#discussion_r4210135624)：runner只有在报告status为OBSERVATION_COMPLETE且A/B/C/D全部PASS时才能将stage记PASSED。FAIL、UNOBSERVED、PARTIAL、缺字段即使native exit0、cleanup全真也必须走现有失败／非零退出路径，不能只在嵌套receipt保存非通过而外层返回成功。可用很小的纯判定函数接入现有check；静态准备中对真实函数做正例及逐A–D的FAIL/UNOBSERVED、顶层PARTIAL/FAILED、缺字段反例，保持清理字段为真，确保旧漏洞确实被捕获。不得仅匹配源字符串当成行为测试。
2. [r4210135632](https://github.com/yyczz1/FightMatch/pull/17#discussion_r4210135632)：C的比较natural样本须同时为cancelled、unbound、responsive unsubscribed以及该布局gate已释放的实际状态；不能由稍后的D终态替代样本条件。原M02自然样本的layout `gate=false`，首次同步样本gate=true；先核Snapshot/ReleaseGate语义，不把它同CanvasGroup.interactable混为一谈。保留早期观察器、首次有效Apply、正几何/支持尺寸、safeVersion推进和原FAIL条件，不削弱任何原断言。

第二条可将现有C选择谓词提为同一probe中的小型纯样本判定，供真实观察和内存反例调用。至少覆盖：只有取消前的好几何natural、但末尾D终态正确时C不能PASS；以及真正取消后且unbound/unsubscribed/gate释放的natural才可支持PASS。逐一缺失四个状态时均不合格。反例是独立DTO，不加GameObject、不伪造actual samples/frame、不注入成功LOC或Session；结果与真实四项观察分栏。借本次唯一Unity编译/入口验证实际C#判定，不另开Unity测试。

## 一次验证与返回

准备机械≤120秒、静态最多2轮累计30秒；固定源后正常工具审批，一次图形Unity≤240秒、Play一次、准备至归档总机械≤600秒；无额外I/T/旧十项/全量/下载/安装/Git。保留2秒调度实际时间与收尾开销，不能伪写精确实时保证。相同总体资源上限、owned精确信号、16后像归档与P1008恢复照旧。

C交两finding的实质patch、正反例结果、一次实际运行/清理/监护/恢复证据并停写；不自签ACCEPT、不resolve GitHub线程。中央实际wait接收后更新同PR17分支并交GitHub审查新head。正常交互、正式LOC、设备、Demo和PR4 P1仍开放，旧M02的四项PASS不被冒用为已验证本修正。
