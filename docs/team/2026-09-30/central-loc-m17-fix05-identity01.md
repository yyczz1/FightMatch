# FIX05 身份修正补充

2026-10-12，中央actual `01a12bf9-e665-7d20-ab65-31ca1dfc8613`；用户继续授权与原SOURCE包不变。LOC报告81/81差量检查通过，完整命令3.328秒，但源码记录原actual `01a12c04-f7b5-7f83-a011-da722589e688`，实际写入/检查发生于 `01a12c0b-aa4f-79a2-b089-d263e2d77e2a`。真实检查结果保留，不等于错误身份已可封签。

中央已通过LOC消息批准如下普通纠正，唯一收件仍中央：

- 修正前源码与offline-checks逐字节存入 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix05-identity-before/` 的 `source/m17_driver.py`、`authority/offline-checks.json`；该根可新建 `receipt.json` 记录原工具、实际回合与差异。路径占用即停，不覆盖。这三个历史叶子独立于最终F5八叶。
- 收到派发后读取实际当前turn/item，修正OWNER/DISPATCH等作者身份字面值。中央ISSUER不变。除这些明确身份常量外，源码函数/语义保持不变，并作机械比对。
- 因源码输入身份改变，增加一次相同差量检查，完整命令≤10秒；旧81项真实结果和新检查分别记录，不把旧通过改名为新输入。已审F4的66项不重跑。
- 原定一次≤30秒SOURCE机械封签尚未执行，继续沿原范围完成。历史身份修正证据随精确新head送GitHub审查。

此补充只授权源码与短验证，不增加任何材料执行、下载、依赖还原、Unity或Git权限。
