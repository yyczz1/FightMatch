# UGUI-01-Q4-FIX-08-A02：三轮统一 Metal graphics 启动条件

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

本补充绑定：

- FIX-08：187 lines／11520 bytes／SHA-256
  `e007dbf7d1e922e199ee2128957a335c2dc020ae0d253d4f02d3a425303f98c2`；
- A01：23 lines／1248 bytes／SHA-256
  `4e99778b1a92841525efa8287198408815b6c2013683101d262e04c7595729e0`；
- owner C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`；
- central authority `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local`。

A02 完整 supersede A01 的 renderer/argv 澄清。FIX-08 的 exact-3、targeted-9 与 exact-222
都包含真实 uGUI raycast/mesh 验证，三轮必须统一：

- 使用 `-batchmode`；
- **不得传 `-nographics`**；
- 保存实际 renderer，并要求 Metal／非 Null；
- filter、测试名称集合、顺序、timeout 与通过门仍按 FIX-08，不因启动条件更改而放宽。

已按 FIX-08 启动或完成的 graphics exact-3 可原样复用，不重跑。targeted-9 与 exact-222
若尚未启动，必须按本 A02 的 graphics argv 执行；若已错误以 `-nographics` 启动，保留
真实证据并停止，不得重跑或倒写。纯 compile 不在本包执行范围，A02 不改变其它包已批准的
纯 compile 启动方式。

除上述两轮移除 `-nographics` 并恢复三轮统一非 Null renderer 门外，FIX-08 全文继续生效。
