# 图稿验证记录

此文件记录文档图稿的生成与显示验证，不代表 Unity 编译、游戏逻辑测试或 Android 性能验证。

2026-09-15 后续同步：用户保留连线自动施放，明确战士击杀跳过嘲讽、盾骑仍按可用条件放盾；并明确玩法讨论归策划会话。已更新对齐清单、职业/技能 UML、行动时序与阅读入口。重新生成后 14/14 页面通过浏览器检查，51 项 Mermaid/阅读版哈希关系一致；已审阅更新的职业图、行动时序及分工说明截图。Archify 图源与产物未在此轮改动。正式策划的同步结果由策划任务追加记录在 planner-response.md 中。

2026-09-15 更新：补齐技能/配置展开图、可直接讨论的问题清单及策划核对结果；本次重新验证总架构和全部 Mermaid 阅读页面。其他五张 Archify 图源与产物未变，保留此前证据。

交付前核对 69 项哈希关系，全部一致：Archify 图源/HTML 与原子收据、浏览器证据与相同 HTML、12 份 Mermaid 图源及对应 HTML/明暗 SVG、总览及策划对齐源文档/阅读版。检查结果未发现收据对应过期产物。

## Archify 架构与流程

六张图均完成最终 validate、原子 deliver 和 visual-check：每张 **9/9 showcase，0 errors，0 warnings**。浏览器在 1440×900、1600×1000、1920×1080、2048×1320 四种尺寸测量，全部无页面横向或纵向溢出；1440×900 与 2048×1320 均捕获浅色和深色截图。

已通过图像查看工具检查各图的明暗截图与最终汇总图，核对节点、连接线、标签、图例和说明卡片。`visual_review: passed` 是图像审阅判断，与自动浏览器结果分别记录；原始 visual-check 文件中的 `visualReview: pending` 是工具自身的固定状态，未被修改。

| 图稿 | 类型 | 验证 | 自动浏览器 | 图像审阅 |
| --- | --- | --- | --- | --- |
| [总架构](overview.html) | architecture | 9/9 | passed | passed |
| [代码复用](reuse.html) | architecture | 9/9 | passed | passed |
| [一次行动](action.html) | workflow | 9/9 | passed | passed |
| [历史回退](rollback.html) | workflow | 9/9 | passed | passed |
| [成长与结算](progression.html) | workflow | 9/9 | passed | passed |
| [完整攻略](guide.html) | workflow | 9/9 | passed | passed |

精确输出路径、图源/产物 SHA-256、各项状态及构图修正轮次见 [Archify 交付记录](archify-handoff.json)。[原子生成收据](archify-delivery.json)保留工具原始摘要；每张 HTML 旁的 `.visual-check.json` 绑定相同产物 SHA-256，并记录视口与截图。

构图修正轮次：总架构 1，回退 2，其余 0。之后同步了战术装备的新增策划内容，并把渲染器的通用图例名称改为“界面与工具 / 业务与规则 / 状态数据 / 接入边界 / 在线计算”；每次源更新后重新验证、生成并采集浏览器证据。图例不代表数据库、服务器或 SDK 选型。

## Mermaid UML 与时序

使用本机已有 Mermaid **10.9.5**，当前生成 **11 张核心类图、1 张行动时序图**，共 24 个明暗 SVG 和 12 个独立 HTML；另生成总览和[策划对齐阅读版](../2026-09-15-planning-alignment.html)。阅读版使用 runtime 已有 marked 17.0.5，无需联网即可阅读，本轮没有新增项目包依赖。

总览、对齐阅读版和 12 个图稿页面均实际在 Chrome 打开：**14/14 页面检查通过**，没有页面脚本错误、重复元素 ID、页面横向溢出或失效的本地链接，并保存明暗截图。对齐阅读版额外检查 390×844，页面宽度与 scrollWidth 均为 390。此次图像审阅覆盖新增技能/配置图的双主题、受影响类图、更新总览和对齐文档的标题、问题与窄屏显示；此前双主题 SVG 箭头 ID 修复仍保留。

生成与浏览器检查摘要见 [Mermaid 收据](uml/render-receipt.json)。图源与 HTML/SVG 的 SHA-256 均在收据中；截图位于 `output/playwright/fightmatch-architecture/`。图源为 `uml/*.mmd`，页面说明为 `uml/views.json`，生成器为 [render.cjs](uml/render.cjs)。交付前再次同步了补充策划的“背包不限总格数、统一每叠 99”，删除了旧示例及过时的容量待定项。

重新生成时，向生成器显式传入本机 Mermaid bundle、Playwright 模块、Chrome 可执行文件及 marked ESM 路径（`--marked`）；可选 `--verify` 指定截图目录。它只生成架构文档目录及指定截图目录，不安装包或修改 Unity 配置。初次 Chrome 启动被沙箱以 `spawn EPERM` 阻止后，使用获准的本地生成命令完成验证。

## 范围与限制

- 文档图源、生成图和验证证据属于本轮范围；未编写 FightMatch 游戏实现。
- 已对照当前策划案与补充讨论检查状态归属、回退、道具消耗、正常结算及攻略有效性边界，并与策划任务「规划游戏后续开发」交流。完整核对答复位于 [planner-response.md](../2026-09-15-planner-response.md)；具体问题与候选建议位于 [planning-alignment.md](../2026-09-15-planning-alignment.md)，旧 C01—C09 已链接到明确问题。未决方案不伪装为用户已确认或已实现规则。
- 类图仅为候选核心类型与代表性方法，没有宣告全部字段、异常路径、公共 API、存档格式或部署方式已经定稿。
- 本轮没有运行 Unity 编译、EditMode 测试、Player 构建或 Android 性能测试；图稿检查不能替代这些验证。
- 没有提交、推送、创建分支或工作树；仓库原有改动未作清理。
