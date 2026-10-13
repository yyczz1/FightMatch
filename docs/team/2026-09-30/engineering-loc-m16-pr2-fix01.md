# LOC M16 / PR2 — 三项 P1 源码修正 FIX01

2026-10-04 · SOURCE_ONLY_ACTIVATED · 主程签发。人类授权与消息身份见 `central-resume-scene-loc-2026-10-04.json`；签发者 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，turn `01a1061d-c7b2-79b0-a72e-7fc5c30d7f1a`。唯一交付 owner 为既有 LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`；回执记录实际执行 turn，唯一接收人主程，主程向中央汇总。

## 输入与范围

- 固定 PR2 head `bb642ab3897fa587b1f41f8298533229874f9617`；三项 P1 及原文以 `testing-loc-m16-pr2-review-receipt.json` 的 r4163732952、r4163732956、r4163732959 为准。沿用 `engineering-loc-material-source-continuation-16.md` 的产品/工具链选择、931 源码叶子、31 包许可闭包、未来执行边界，不重新研究许可或运行材料化。
- 旧根 E16=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m16` 全部只读。原 driver 125870B / `0d4f0e7baccf14862eb5fce8dfdb558ccafacdd45f1683e8880c70c482d7dbf8`；source-freeze 2666B / `0c9960ae9b861e023b26a8ff914ace2e6cd0021edf8784650261076ab1abaf50`。先验证六叶子与其封签；任何漂移先报主程。
- 新根 F=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m16-fix01`。只允许新建以下八叶子：`source/m16_driver.py`、`source/m16-inputs.tsv`、`source/m16-write-set.tsv`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/correction-receipt.json`、`source/source.patch`。F 首次写前须不存在；总量≤16MiB。表格/封签仅作修正所需的来源、命令和新证据绑定变化；旧 T16/M16 及其他根均保持原状。

## 必须满足的修正

1. r4163732952：实际逐项比较 `controlAssertions.otherBControls` 要求的 semantic/raw/canonical 输出，而非只看 role/argv/退出码。正确退出但错误内容必须拒绝；原正常对照须通过。
2. r4163732956：B runner 文件/许可验收与冻结 `expected-deployment.tsv` 及包闭包联结，要求完整可信集合、每项身份、禁止缺失/额外项目；不能以交付者自报的截短 manifest 作为全集。原仅 DLL+两许可的假阳性必须拒绝，完整 fixture 须通过。
3. r4163732959：execute 必须绑定独立可信的中央实际授权记录（thread/host/turn/item/文件 hash 与精确 source freeze、reviewed head、argv），调用者自填 stdin、公开 thread ID、随机 turn 或自报 hash 均不能获得执行权。当前授权是 SOURCE_ONLY；真实 execute 必须保持关闭，未来材料化仍需中央另签实际激活，不能将本包当该激活。若可信信任根需要本白名单以外的中央文件，先给主程最小方案/精确字段，源码可先实现 fail-closed 接口，不能伪造中央产物或跳过该门。

## 验证、停止与回执

- 离线只用原允许 Python 进程内检查，无子进程/网络/dotnet/restore/Unity/Git 写；初次检查+一次必要修正，累计≤120秒。只补三项 P1 的正反 fixture 与必要旧离线回归，保留每次失败及实测耗时；不消耗原材料执行预算。源语法/哈希/补丁与原六叶子前后完整性可进程内检查，不执行原 prepare/execute 修改旧根。
- 首次写前冻结旧六叶子，修后证明其字节不变；每个 P1 在 receipt 中对应修改位置、红/绿 fixture 和实际结果。八叶子清单、bytes/SHA256、source patch、owner/issuer/authority、所有命令与耗时、零外部执行、未来门状态一并回执。不要自签集成 ACCEPT；最高为 SOURCE_READY / UNCOMPILED / UNTESTED（产品）。
- 只修上述问题及必需的新根/实际 owner 绑定，其他产品源码/资产/Packages/ProjectSettings/.meta/共享索引不得写。缺输入、额外文件或预算不足向主程给精确差额；普通范围内修正继续至上述收件条件满足，然后返回主程等待中央发布及 GitHub 独立审查。
