# LOC-M16：完成材料驱动源码，复用31项许可证输入

2026-10-01 · 中央激活 `SOURCE_PREPARATION_ONLY`；owner为既有LOC，Astra/xhigh，唯一完成接收中央。本包不授权restore、dotnet、Unity、联网或Git写入。

继承[M15](engineering-loc-material-source-continuation-15.md)及其M14执行设计，只作以下增量。M15六叶封存保留：30离线检查通过，但executor、完整写集及旧目录保护未完成，不能当SOURCE_READY。

1. 新E16=`<repo>/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m16`，T16=`/private/tmp/fightmatch-loc-lic-alt-m16/loc-lic-alt-yamldotnet-16.3.0-m16`，M16=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m16`。先核三根absent；只建E16六叶：`source/m16_driver.py`、`source/m16-inputs.tsv`、`source/m16-write-set.tsv`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`。T16/M16保持absent。
2. 新只读输入H16=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-closure-bodies-16`，receipt SHA256=`3e2cfb8b0671cf713cfd5626ec38920fd4aa8a3fa2608009e23694909079280d`。按其有限files.tsv读；复用G15七项和M13三项。H16已覆盖31项全文，27项关系就绪，四项官方catalog绑定仍待主程H17补证；不把正文齐全当全部材料门通过。
3. H16的`standard/Apache-2.0.txt`及receipt已实际归档；全文11358B、SHA256=`cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30`。覆盖M14/M15“未来再GET Apache”安排，后续仅复制已核原字节，新增网络请求为0。
4. 主程将补H17=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-catalog-bindings-17`六叶元数据；中央接收后另消息绑定其实际receipt/hash。此前先完成不依赖该输入的全部driver逻辑、有限写集、旧根保护和离线用例；不因外部输入未到而中断能独立完成的源码工作，也不能先填假hash。
5. 本轮须完成M15 remainingSourceWork：材料封装/部署与B-L合同输出、旧root/缓存继承保护、执行activation与0/1/1 restore链、材料stage与create-once seal。保留931源叶只改两处YamlDotNet引用、31包、原runner23/56/23，future tests13/29/13及现有预算；不重新研究已定设计、下载包或验签。
6. 旧M10 absence只能按实际后继证据转成preservation，不能运行过期断言或把现场hash当历史权威。M15及G15/H16封存字节只读。H16 embedded-license lane按自身包/entry/catalog绑定，短声明不算全文；保留所有真实签名warning、来源限制与旧失败。
7. 离线验证只针对新增/变动逻辑，保留已有效用例；源阶段至多初次及一次必要修正复验，总≤120s，child starts=0。网络隔离代码可完成并离线检查，但不能声称OS隔离实跑已证明；该事实留未来明确执行门。
8. 六叶完整且必要输入均绑定后封`SOURCE_READY`；仍缺外部输入时准确列出，其余executor工作应已完成。中央上传实际新PR head、接GitHub Code Review后另行授权执行；本包不要求未来head预先存在，不自行运行后续阶段。

回传实际thread/host/turn、六叶身份、离线结果、零执行项、剩余具体缺口，≤200字及source-freeze路径；随后结束回合，不启动重复审查或其它owner。
