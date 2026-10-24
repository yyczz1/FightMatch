# FIX05 REVIEW01：关闭GitHub两项P1

2026-10-12，中央actual `01a12bf9-e665-7d20-ab65-31ca1dfc8613`，用户继续指令item `01a12bf9-ee2d-7140-a43f-a33e3bafec6c`。唯一作者LOC/local，Astra/xhigh，中央唯一收件。依据 `testing-loc-m17-fix05-review.json` 与 `loc-m17-fix05-source-verdict.json`：PR2 head `c4c7a5d313c1193191447b525ade226268b616e1` 两项P1未关闭。

## 最小修正

1. 评论4242623018：将 `central-loc-m17-fix05-identity01.md`、`central-loc-m17-fix05-seal01.md` 以准确bytes/SHA加入受保护输入，缺失/改动时消费失败。本包本身及原成功封签记录 `central-loc-m17-fix05-seal-success.json` 同样固定，避免再遗漏实际修正来源。原FIX05四份文档仍保留；不要解除其他身份校验。
2. 评论4242623020：中央已归档原成功工具 `exec-0a6d03d4-489e-4f65-bd61-282109520212` 的完整命令/输出/exit0/4.916111583秒/八叶身份到上述seal-success文件，随本次PR交付。新候选也必须在工具返回后归档真正成功结果，不能只留占位描述。为避免封签自引用循环，新工具回执放在八叶之外的明确sidecar；该sidecar绑定本次freeze/hash和真实tool/turn/exit/完整耗时/命令/原输出，随同PR发布，中央验收及后续激活以其为必需证据。八叶内只记录结构化sidecar路径和独立外层收件要求，不伪填尚未发生的工具ID/exit。

## 精确写域

旧F5及其identity-before/seal-before、F4/M06和所有外置M/W均只读。仅新建 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix05-review01/` 下原结构八叶：source/m17_driver.py、source/m17-inputs.tsv、source/m17-write-set.tsv、source/source.patch、authority/prepare.json、authority/offline-checks.json、authority/source-freeze.json、authority/source-receipt.json。另新建 `docs/team/2026-09-30/loc-m17-fix05-review01-command-receipt.json` 保存真实外层工具记录，≤64KiB。

以F5 driver148848B/SHA256 `1d294a975d47dc851eb43a3c7667818b70cf29b51936a51a6287b18d5e8bd473`为补丁基线。只改变本次来源绑定、明确回执结构、SOURCE E根、实际作者/派发身份及相应模型/表/封签/定向检查。未来M5/W5根保持原未创建的m17-fix05，不为SOURCE修正再迁材料路径。31包+源码tar复用、0GET、3restore/900秒以及归档/进程/污染/消费安全规则均不变。不增加新执行框架。

复用F5 prepare中的partialProtection清单和原捕获时间，禁止重新枚举partial根或扫描SDK；可按固定清单校验已有叶子。文档身份在源码写入前一次固定；先read_thread取得真实作者turn/item，再写源码，固定后不因封签收据回合变化去重写源码作者。

## 验收与短验证

两个纠正授权、本包和历史成功原始回执全部有不可变输入绑定；缺失或篡改各必需授权均被拒绝。SOURCE校验成功必须有外部真实工具证据并对应最终freeze。新的sidecar不作为它自身封签的上游输入，避免循环；没有sidecar则中央不得接受候选或启动材料。

源码写定后一次≤10秒定向检查：增加授权缺失/错hash回归；F5的81项仅在新源码身份/检查器需要时与本次回归合为同一次短调用，F4旧66项不跑。之后一次≤30秒SOURCE机械生成/封签，复用固定partial清单，八叶≤16MiB、driver≤256KiB。源码函数/字段差量和真实工具输出分别记全；元数据使用既存jb规范序列化，收件元数据完成后再冻结。失败或超时交中央，不自行第二次尝试。

完成后中央只推既有PR2审查分支并请求新head审查。作者不做Git、不跨发消息；不运行任何下载、restore、native、Unity、B/L或新激活。源码接收与新的材料许可分开。
