# C4 图稿验证 · 2026-09-16

本次交付为三份 Archify `architecture` 视图：C1 产品上下文、C2 应用与存储、C3 客户端主要业务组件。节点表示目标设计；C2 在线应用和 C3 组件划分仍为候选，不把当前图视作已实现的部署或代码盘点。

## 结构与确定性产物

三份 JSON 均设置 `meta.quality_profile: showcase`。分别运行 Archify `validate architecture <source> --quality showcase --json` 和 `deliver architecture <source> <html> --quality showcase --json`，全部 exit 0、9/9、组合错误 0、警告 0。原始结果分别保存在本目录的 `*-validation.json` 与 `*-delivery.json`，哈希汇总在[交付记录](handoff.json)。

C1 校正 0 轮；C2／C3 各校正 1 轮，仅根据诊断调整遮住节点或其他连线的关系标签位置。通过后的图源及交付 HTML 未手改。

## 静态图与视觉核对

RUN — `node docs/architecture/2026-09-16/c4/render-static.mjs --sharp C:/Users/YYC/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp`

结果：exit 0，生成三图 × 明暗两种主题，共六组 PNG／SVG。[收据](static-receipt.json)绑定每张图的来源 HTML、SVG 与 PNG 哈希。使用现有 sharp，不安装项目依赖、不访问网络、不启动浏览器。最初指定不存在的 `sharp/lib/index.js` 入口失败，随后使用已存在的包目录成功解析；失败尝试未写为通过。

静态图沿用交付 SVG 的节点与关系几何，调整文字字号以适合直接阅读，并附标题和原图说明卡片。因此它是派生阅读产物，不能当作交互 HTML 的像素截图。

六张 PNG 均通过图像工具实际查看：中文、节点类型、边界、箭头、关系标签、图例及说明卡片可读，未发现裁切或重叠。C3 明确包在 Unity 客户端内；C2 候选在线边界与数据存储说明可见。此视觉结论仅覆盖这些静态图。

## 浏览器证据范围

NOT RUN — Archify `visual-check` 及内置浏览器打开。

原因：本会话此前 Browser Use 已因 URL 安全策略拒绝本地页面。本轮使用文件产物和离线图像交付，没有更换浏览器、启动本地服务器、调用 CDP 或绕过受限访问路径。

浏览器证据记为 `not_run_policy`，不冒用标准收据中的 `passed`，也不冒充 Chrome 不可用时的 `skipped`。没有检查 HTML 的四档桌面响应式尺寸、搜索／聚焦／导出交互；本轮没有浏览器截图。

## 语义与范围

- C1 的 FightMatch 软件系统在 C2 展开，C2 的 Unity 客户端在 C3 展开；图例及各节点标明类型。
- C2 中 QFramework、普通 C# 规则及 FlowPuzzle 不被画成单独部署应用；C3 将共用规则置于客户端内部。
- C3 是主要业务组件的局部视图，省略的配置／保存／平台接入及通用几何由正文和协作基线说明，不代表删除职责。
- C4 使用官方定义作为分层依据；产品职责仍来自本地策划与架构基线。
- 原有图源、游戏代码、依赖、资产与项目设置保持原样；未执行 Unity 或 Git 提交／推送。
