# LOC-M17-FIX03 SOURCE 派发

中央 actual `01a11a60-d890-7660-b7ee-1accdb3ffe0c`；唯一收件中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。沿已授权持续开发修复普通技术问题；人类 item `01a11a60-d8c2-71f0-b832-a4e0c8db237e` 批准的一次 M 已由 M04 消耗，本包没有第二次下载/restore 授权。

交付人：LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，Astra/xhigh。实现开始自行读取当前真实 turn，写入 source-receipt；不得复用 M04 actual。中央以完成事件复核，不需要作者跨会话消息。

完整规格与八叶白名单：`engineering-loc-m17-fix03-path.md`，7921B / SHA256 `1e083a841f946fc762af1d1291170d4cab7160f0a9f83d20739ba02e9dd80b01`。F2 固定输入、两个精确 ZIP 别名、未来 M/W 的 m17-fix03 常量及衍生路径、预算/污染门、旧根保护按该规格逐项执行。仅 SOURCE；不创建外置 M/W，不调用下载、restore、Unity、构建、材料 host 或 activation，不执行 Git 写入。

验收：只增加规定八叶；固定包原函数红、新函数八条绿，其余五包不变，指定负例拒绝；一次定向离线≤5秒、机械封签≤30秒，失败停止。生产安全函数/包身份/18 GET/3 restore/预算不变，productionTrustPin=null、executionAuthorized=false、materialReady=false。源码适配增删≤160行，越界先报告。原始命令/输出/耗时/输入及输出封签都在 source-receipt 中，完成停 SOURCE_READY。

中央后续负责机械收件、发布独立 GitHub PR 审查。源码门与材料门分开；旧失败保留，不追认 M04 成功。

## 首次 SOURCE 封签失败后的有限修正

作者 actual `01a11a78-de70-73e0-8576-edad4f807b14` 已返回：26项定向检查符合预期，0.108389542秒；prepare随后因 `fixed 18-archive contract drift` 退出20，三叶草稿不是 SOURCE_READY。新增写域仅允许 `failed-prepare-01/` 下保留这三叶的逐字节副本（`source/m17_driver.py`、`authority/offline-checks.json`、`authority/source-receipt.json`），之后允许补齐/更新原八叶；原失败不抹去。

中央已从封签F2 model提取18项 `(path,url,bytes,sha256)`，确认旧摘要 `6761184244450758f97f5886582a2bd2ae9de2108b4d71b921943460efae215d`。只把每项path的精确旧W前缀改为已定 `m17-fix03/`，仍18项/25172764B，地址及归档身份不变，按原jb/sha得到 `c4f17bfb0e405cd2bc43e4ad1c3537374dff338feab58c9ace7743ca643f2de3`。允许 `download_contract` **仅此摘要字面量**同步；不改条数、总量、行字段/排序/校验逻辑或网络条件。机械重建同一派生关系核对，不能直接信任作者生成的新清单。

允许新一次SOURCE prepare≤5秒及机械封签≤30秒，保留前次26项与失败输出；不调用材料入口，不下载/restore。该次prepare可复用现有短定向用例入口，目标修正固定摘要后完成八叶；不增其他套件。总源码差量仍≤160行。若再出现其他问题，保留实际失败并报告。此项是已准目录迁移衍生常量的纠正，不是新的材料执行授权。
