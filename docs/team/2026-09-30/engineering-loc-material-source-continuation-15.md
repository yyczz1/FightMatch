# LOC-M15：复用已补齐正文，继续准备源码

2026-10-01 · 中央激活 `SOURCE_PREPARATION_ONLY`；不是 restore、材料接收或产品运行授权。
实施 LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，模型 `gpt-6-astra/xhigh`；唯一收件中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。

继承 [M14](engineering-loc-material-restore-continuation-14.md) 的源码准备、输入、执行设计与验收，仅作以下有限替换。

1. M14 在 driver 生成前留下 `BLOCKED_LICENSE_CLOSURE`，实际 turn `01a0f6cc-4e1b-7d23-b3ea-3ad13e7976e4`；四个 E14 文件永久保留原字节。T14/M14 仍 absent；本包使用新根，不改写旧状态。
2. T15=`/private/tmp/fightmatch-loc-lic-alt-m15/loc-lic-alt-yamldotnet-16.3.0-m15`。
3. E15=`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m15`。
4. M15=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m15`。
5. 准备前确认三根 absent；本轮仅新增 E15 六叶：`source/m15_driver.py`、`source/m15-inputs.tsv`、`source/m15-write-set.tsv`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`。T15/M15 保持 absent，不生成缓存／pyc或额外文档。
6. 新增只读固定输入 G=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-gap-bodies-15` 全19叶，及[输入说明](engineering-loc-license-gap-resolution-15.md)。G/files.tsv SHA256=`375966074d1221862c70e10bee51aa49d2abf1830355b07a16e921ccecb9deed`；G/receipt.json SHA256=`7af32c1887d18ce3e94982af2da8015b9a8c7433da005279f51a3faf58df28d4`。只按有限 manifest 取叶。
7. 中央已机械复核 G 的18项清单、19文件/2722329B，以及七正文与原工具正文、Git blob和tree路径相等；E14四叶仍一致。七项正文输入允许后续材料引用；不是整个 M 或许可证适用性的扩大结论。
8. NeoLua 与原六项 license gaps 采用 G 的七个自身 package/provenance 键。保留 Annotations 独立关系、MessagePack 完整附录、NOTICE 检查实际范围、NeoLua 无包内commit／不可复现构建声明。原 M13 xunit license-snapshot lane、005签名 warnings与环境偏差继续保留。
9. M14 §3 的未来执行路径／driver名由 m14 改为上述 m15；旧 M09/M10/M13 引用与实际哈希不改，E14/G作为新增只读保护集。新增许可证叶在未来 T15 acquisition/material 中的目标逐项展开到 write-set，不能用任意通配放行。
10. 只做 M14 §5步骤1–2：准备真实 driver、有限输入／写集、离线正反核验后冻结。网络0、dotnet0、restore0、验签0、Unity0、Git写0；不再获取已补齐的七正文。Apache网站标准文本的后续单次归档仍待执行激活，本轮不发请求。
11. 后续设计保持 runner restore0、tests generation1＋locked verification1，各≤180s，13nodes/29edges/13locks；复用31包、旧runner和已通过签名，不重新下载或验证。准备阶段要把实际网络隔离与父环境继承写成可核机制，不能以参数存在代替已验证结论。
12. 本轮完成以 `SOURCE_READY` 的真实六叶和离线结果为准；若出现具体缺项，保留该阶段事实与最小修订建议。固定源后由中央上传新 LOC PR head、接 GitHub审查，再另行激活执行；无需提前拥有未来head，也不自行启动后续阶段。

唯一回执：实际 thread/host/turn、六叶身份、离线结果、未运行项和具体阻塞（如有），≤200字＋E15/authority/source-freeze.json；随后结束。中央接回并推进，不经过主程重复转发。
