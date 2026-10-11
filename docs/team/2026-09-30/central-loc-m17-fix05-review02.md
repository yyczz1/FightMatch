# FIX05 REVIEW02 可执行SOURCE包

2026-10-12，中央actual `01a12bf9-e665-7d20-ab65-31ca1dfc8613`，用户继续item `01a12bf9-ee2d-7140-a43f-a33e3bafec6c`。主程actual `01a12c3a-44d1-7e01-a74e-66366c9aa08e` 已完成 [启动校验设计](engineering-loc-fix05-review02-activation.md)，中央接受该最小设计作为本包实现依据。唯一作者LOC/local，Astra/xhigh；中央唯一收件。

解决准确head `5c12ddabfc784927a936b428d9f1b90638cf108e` 的P1 `4242690950`：activation必需绑定 `sourceCommandReceipt` 的固定path/bytes/SHA256，driver fresh核对真实后置回执与SOURCE、原工具结果，并在 `state['m']` 建立前拒绝错误记录。原host模板不改；本包不创建任何新activation/host文件。

执行上述设计第2–6节的完整schema、顺序、内容对应与负例。生产读取有64KiB上限、拒绝链接，原始命令仅作数据；同一verify_activation也在每次native命令前重核。后置回执不进入它自身的SOURCE inputs；中央未来签发activation时必须固定并核验它。除设计明确的verify_activation、来源/保护/模型、source_artifacts/prepare元数据及小型review02 helper/检查外不改生产函数。

唯一新建SOURCE根 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix05-review02/`，八叶为 `source/m17_driver.py`、`source/m17-inputs.tsv`、`source/m17-write-set.tsv`、`source/source.patch`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/source-receipt.json`。唯一额外新文件 `docs/team/2026-09-30/loc-m17-fix05-review02-command-receipt.json` ≤64KiB，在真实封签工具返回后捕获。旧R1/F5/M06/partial根只读；新根占用即停。

R1 driver162125B/SHA `ec629ed6be21319add7d0a5b75bcd0747ca07930aef91f88a114ade1347b6b81` 为补丁基线；R1真实侧录20148B/SHA `a419a868035f9bdc8817aa91a6b5ccdda593c1cd404e13409214af9218161160`及R1八叶保持历史固定输入。本设计和本包在写源码前固定bytes/SHA；源码作者/派发身份先从实际本轮read_thread取得，固定后不因后置封签回合而改作者。

验证合为一次≤10秒完整定向调用，覆盖设计规定的缺失/改动/伪一致摘要/错误绑定/异常工具输出/有效记录、重复fresh读取以及execute的无M写入/native哨兵。使用小内存fixture，不执行材料、命令字符串或真正native。旧81/66/15不跑。随后一次≤30秒SOURCE机械生成/封签，复用原partial清单/捕获时间，不重枚举目录或扫描SDK；八叶≤16MiB、driver≤256KiB。真实完成后把完整未截断工具命令/结果/时限和最终freeze/八叶记入外部sidecar，source与sidecar一起交付。

15项旧SOURCE结果不证明新启动门，故本次新检查不可省；本次检查也不接受材料。失败/超时保留原始记录交中央，不自动追加尝试。未来M5/W5、31+1归档、0GET、三次restore/900秒及其他既存保护保持。无Git、GET、restore、Unity、native、B/L或新A/H；完成后中央推既有PR2审查分支并接回新head的GitHub审查。
