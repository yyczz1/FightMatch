# 2026-09-16 整体架构图与阅读页验证

本轮交付：[系统关系图](systems.html)、[系统协作基线 r1 阅读页](index.html)、[可编辑正文](review.md)、[系统设计交接](../../handoffs/2026-09-16-system-design.md)。这是架构推荐基线，游戏规则与存档行为未因此获得实现验证。

## 图稿产物

- 类型：Archify `architecture`，10 个展示节点、10 条主要关系；“玩家长期资料”在正文分别展开成长、背包与关卡进度。
- [最终结构验证](systems-validation.json)：showcase **9/9**，组合错误 0、警告 0。
- [确定性交付收据](systems-delivery.json)：图源与生成 HTML 的哈希和字节数绑定，未直接修改生成 HTML。
- [自动浏览器证据](systems.visual-check.json)：`passed`，1440×900、1600×1000、1920×1080、2048×1320 均无页面横向或纵向溢出。
- 默认 READ／Still 状态的明暗截图已实际查看：[截图索引](systems.visual-check.html)。检查了节点／文字、标签／路径、图例、阅读控件和整体留白；四张截图审阅通过。
- 图源校正 2 轮：第一轮修正三处标签位置；第二轮缩短画布宽度并同步右侧节点位置，以满足桌面可读性。最终图源未再修改。

| 对象 | SHA-256 |
| --- | --- |
| systems.architecture.json | `53ac118f60f68c62755e47a236318ecda2e76c8af3d50ebd912e8d67ccac565a` |
| systems.html | `65798816a512045741160c724ab0b90cd187f0af24bfe67e290e59613e760088` |

## 阅读页首轮视觉证据

`review.md` 经本地已安装的 marked 生成 `index.html`，由[生成收据](review-render-receipt.json)绑定当前正文与 HTML。只使用现有运行环境，没有向 Unity 项目安装依赖。下列截图属于首轮阅读页，HTML SHA-256 为 `4bd521087a6f54d6650938e08666d96b0eed07a1d7dfec094a5bf5ff43009356`；r1 已更新正文，不能将这些截图视为当前文字版的重新视觉验证。

Playwright 使用已安装 Chrome 截图，实际查看了以下结果：

- [1440×900 浅色阅读入口](../../../output/playwright/fightmatch-architecture-20260916/reader-desktop-light.png)。
- [1440×900 深色状态归属表](../../../output/playwright/fightmatch-architecture-20260916/reader-ownership-dark.png)。
- [390×844 窄屏阅读入口](../../../output/playwright/fightmatch-architecture-20260916/reader-mobile-light.png)。

首轮阅读页允许正常纵向滚动；桌面表格与目录、窄屏标题和正文显示正常。首轮正文中的 12 个本地引用在生成时检查存在；目录锚点对应 7 节。初次截图命令遇到沙箱 `spawn EPERM`，经批准重试后成功；这里记录的是成功截图结果，不将失败尝试写成通过。

## 推荐基线 r1 的文档复核

本次继续工作补齐职责交接和下一阶段设计任务，图源、图稿 HTML 及阅读页样式保持原样。按最新策划接收记录复核以下内容；编号覆盖是文档检查，不是程序行为测试。

- 13 项逻辑职责、18 行状态归属；主动／被动技能、概率／CD、等级属性、敌人定义均有明确负责者和消费者。
- A01～A12 共 12 项冲突场景；新增永久操作中断、内容验证隔离、属性／开关条件交接。
- 系统设计交接列出 H01～H10 共 10 组交互；每项 A 场景都至少映射到一组，未发现未映射编号。
- 对基线、交接、README 和层级说明共 4 份 Markdown 检查了 44 个本地链接，缺失 0；表格列数一致。
- 当前生成阅读页的 14 个正文链接、全部 25 个 href（含目录和页尾）均检查；本地目标缺失 0，目录锚点缺失 0，仍为 7 节。
- 当前正文／HTML 的哈希由生成收据绑定；首轮截图在汇总收据中单列为旧版本证据，当前文字修订未重新执行浏览器视觉检查。

RUN — `node docs/architecture/2026-09-16/render-review.mjs --marked C:/Users/YYC/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/marked/lib/marked.esm.js`

结果：exit 0；7 节、14 个正文链接、缺失链接 0。没有新增项目依赖。文档结构、引用和场景对应关系由本轮只读 PowerShell 检查完成，exit 0。

RUN — `node --check docs/architecture/2026-09-16/render-review.mjs`

结果：exit 0。文件范围检查记录了开始时的 127 个架构／交接文件：本次修改 8 个既有文件、新建 1 个系统设计交接，无删除；既有 34 个已修改的 Git 跟踪文件内容哈希未变。当前系统图与全部旧图／UML 哈希未变。

## 补充浏览器限制

之后尝试在内置浏览器中打开系统图时，Browser Use 的 URL 安全策略拦截了本地 `file://` URL。本轮未绕过限制，也不声称已自动展示页面或验证搜索、聚焦、信息面板、导出等全部交互。此限制单独记录，不覆盖此前从实际交付 HTML 取得的自动浏览器证据和静态视觉审阅结果。

## 范围与维护

- [汇总收据](handoff.json)分别记录未改动图稿的验证、默认状态视觉审阅、首轮阅读页截图与内置浏览器限制。
- 原 2026-09-14 图稿及 UML 文件未修改；当前系统关系图亦未修改。本次继续工作更新基线正文与阅读页、入口与证据范围，新增系统设计交接稿。
- A01～A12 是架构约束与后续验收场景，不是已执行游戏测试；未运行 Unity、改动依赖、提交或推送。
- 重建阅读页：运行本目录 `render-review.mjs --marked <本机已有的 marked/lib/marked.esm.js>`；修改图源后依次执行 Archify `validate`、`deliver`、`visual-check`，并重新查看实际截图。
