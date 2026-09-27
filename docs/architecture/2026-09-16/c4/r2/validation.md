# C4 扩展 r2 · 交付与验证范围

2026-09-16 · 图类型：architecture。正文入口为[README](README.md)，精确图源与HTML指纹见[handoff.json](handoff.json)。

| 图稿 | 确定性校验 | 图源／产物收据 |
| --- | --- | --- |
| [C2 应用与数据](c2-runtime.html) | showcase 9/9，0错误、0警告 | [delivery](c2-runtime-delivery.json) |
| [C3 更新、账号与同步](c3-client-services.html) | showcase 9/9，0错误、0警告 | [delivery](c3-client-services-delivery.json) |

每图初稿有3个标签距路线过近的诊断，各进行一轮针对性位置修正，再通过validate和deliver。用户随后确认广告机会联网使用，C3只修订对应节点和说明卡文字，重新validate、deliver及生成静态图；收据只指向最终版本，不沿用旧HTML哈希。

四张明暗PNG由交付HTML内的SVG离线生成，适配静态阅读字号，未改变图的节点关系与几何。已实际查看C2明暗图；用户决定后重新查看C3明暗图，未发现文字截断或歧义交叉。生成指纹见[static-receipt.json](static-receipt.json)。这是静态图检查，不是浏览器截图或HTML查看器验收。

**浏览器证据：NOT RUN。** 本会话此前访问本地页面被浏览器安全策略拒绝，本轮未再调用浏览器，也未换CDP、localhost或其他访问路径绕过。没有运行visual-check，故没有把状态伪造成工具返回的passed、failed或skipped。1440×900、1600×1000、1920×1080、2048×1320的HTML容纳性、交互和导出尚未验证。

**实现验证：NOT RUN。** 未启动Unity、安装依赖、构建Player、使用真机、接入SDK／云服务或注入跨设备故障。研究报告的官方平台声明与架构场景推演不替代这些验证。

文档交付前实际检查10份Markdown、125个本地链接、14个标题片段、12个图源／HTML／静态产物指纹，以及内容策划新回复的SHA-256和35个唯一场景编号；检查退出0，问题0。handoff与当前delivery收据一致。该检查只证明文档与产物对应，不证明账号、热更新或权益协议已经实现。
