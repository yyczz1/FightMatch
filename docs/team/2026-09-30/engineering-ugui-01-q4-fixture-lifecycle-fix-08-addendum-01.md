# UGUI-01-Q4-FIX-08-A01：renderer 证据适用范围澄清

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

本补充绑定：

- 基包 `engineering-ugui-01-q4-fixture-lifecycle-fix-08.md`；
- 187 lines／11520 bytes；
- SHA-256 `e007dbf7d1e922e199ee2128957a335c2dc020ae0d253d4f02d3a425303f98c2`；
- owner C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`。

基包第 4 节末尾“每次保存……renderer 非 Null 证明”与同节后两轮明确要求复用原
`-nographics` 冲突。准确解释并替换该句如下：

- **graphics exact-3**：不传 `-nographics`，必须保存 renderer 非 Null 证明；
- **targeted-9 与 exact-222**：必须保持基包固定的原 `-nographics` argv；保存实际
  renderer 字符串，允许且预期为 Null，不把 Null renderer 当失败，也不得因此切换模式、
  重跑或放宽测试结果门；
- 三轮仍都必须保存 argv/PID/UTC/duration/exit、stdout/stderr/log/XML/hash；测试通过门
  与名称集合完全不变。

除这一 renderer 证据适用范围外，FIX-08 全文继续生效。C 可在 graphics exact-3 通过后
按原顺序续行 targeted-9 与 exact-222；若 exact-3 或任一后续测试失败，仍立即停止。
