# LOC M17 FIX05 SOURCE：复用中断前全部下载

2026-10-12，可执行SOURCE包；不是新的材料执行许可。中央actual `01a12bf9-e665-7d20-ab65-31ca1dfc8613`，人类继续指令item `01a12bf9-ee2d-7140-a43f-a33e3bafec6c`。唯一作者LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，Astra/xhigh；中央唯一收件，实际作者turn由完成回执绑定。

## 输入与目的

执行[主程最小方案](engineering-loc-after-m06-interruption.md)第3–4节，形成零GET、全归档复用的SOURCE候选。F4已审head `816263eaacf4f3e276c0cef7703c5824af086cc6`；132552B driver SHA256 `8dd4d5daa8b46f2e1a2fdb076636a095e9c0b398a5f8ae9aff83ced8c3f8db6c`，freeze `9b5ea95beb2f148f6c3f99a55aff33df5bf3d192aa66403f7f7ba7d15213b7c8`。旧66项有效结果保留。

LOC中断摘要3060B / `28a5d17a5a47cc8493b54fad9f7f492427b7b36c1265bae2a5c07e518e43f9dc`；E06-resume1/receipt.json 74084B / `0d3facf1273c829bd830e8c27e71406bbdfb774dd1223f2997f0cf38c3c1d039`。13归档9014649B实核有效；3restore仍UNKNOWN，材料未接收。完整原工具与下载实物逐项身份在该receipt。中央当前进程观测见 `central-loc-m06-process-observation.json`，不伪造历史退出/后代闭合。

## 白名单与改动

仅新建 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix05/` 下原结构八叶：

- source/m17_driver.py、source/m17-inputs.tsv、source/m17-write-set.tsv、source/source.patch。
- authority/prepare.json、authority/offline-checks.json、authority/source-freeze.json、authority/source-receipt.json。

限定差量为F4→F5输出根、13归档只读复用、0GET/0正文预算、相应输入/写集/命令/保护与差量检查。原14包及W3五包保持；其余12包与Yaml源码tar精确读取W4/acquire。缺失或不符即失败，不下载。归档内容安全语义、依赖版本/闭包、SDK清单、restore顺序、900秒总限时、进程与污染门不改。新M5/W5只声明，不创建。不能为适应已有partial根增加恢复框架或降低门槛。

补齐方案中唯一尚缺的partial保护身份：允许一次只读枚举精确M4/W4的当前普通叶子、链接状态和身份，沿已有create-once/read-only保护方式纳入F5，并将原始清单记于上述source-receipt/prepare内；不得扫描SDK或别的根。此枚举是新证据，不声称来自旧LOC回执，不因缺历史退出码推定当前M通过。若发现相关活跃写者、链接、差异或无法稳定封签即停交中央；不能删除、补写或修复旧partial根。

## 验收与验证

31包+1源码归档均existing、三来源映射准确，M命令无GET、下载0B；旧W3/W4/M4零写权；新根占用拒绝；归档缺失/错误必须失败；阶段顺序与旧安全语义保持。完整变化函数/字段与基线差异列于回执，不作本地代码审查。

一次定向离线差量检查≤10秒；一次SOURCE机械封签（含限定partial清单）≤30秒，八叶合计≤16MiB、driver≤256KiB。机械封签比方案的10秒放宽为30秒，以覆盖新增精确partial清单；这是SOURCE检查，不恢复材料额度。源码写完后执行，分别记录完整命令耗时/exit及每项结果；超时或失败交回，禁止自动第二轮。不重跑旧66项整套、prepare默认suite或SDK全扫描。

旧F4/M06/A/H、外置M/W及产品文件只读；无网络、restore、native、Unity、Git写或新激活。完成后交中央发布既有PR2审查分支并接收精确head的GitHub PR Code Review。源码接收后才准备一次新的0GET/3restore/≤900秒材料执行提案；本包不授权执行它，也不解锁B/L。
