# 玩家动作 REDO-01

**状态：11套重做人物候选已交付，待用户看效果，未导入。** 原 battle_idle 保留。主美已关闭本次 PXL-1/2/3 静态缺陷；其余7套复用前轮逐帧检查结果。正常速度观感及最终采用由用户确认，特效延期。

[全部新稿原速分段预览](overview-all-actions-4x.gif) · [关键姿势总览](overview-keyposes-4x.png) · [出入鞘连续对照](corrections/visual-fix-01/preview-transition-cream-6x.gif) · [参考来源](references/redo-01-source-notes.md)

总览用于逐项回看，不是游戏状态流程。死亡末帧保持900ms；原动作时长未变。

| 动作 | 帧 / 毫秒 | 当前状态 | 动画与逐帧 |
|---|---:|---|---|
| 收剑待机 `sheathed_idle` | 6 / 1340 | 候选，最终视觉待确认 | [浅底GIF](actions/sheathed_idle/preview-cream-6x.gif) · [逐帧](actions/sheathed_idle/keyposes-4x.png) · [QC](actions/sheathed_idle/qc.json) |
| 收剑移动 `move_sheathed` | 6 / 600 | 静态修正通过，待用户确认 | [浅底GIF](actions/move_sheathed/preview-cream-6x.gif) · [逐帧](actions/move_sheathed/keyposes-4x.png) · [QC](actions/move_sheathed/qc.json) |
| 对侧拔剑 `draw_ready` | 8 / 800 | 静态修正通过，待用户确认 | [浅底GIF](actions/draw_ready/preview-cream-6x.gif) · [逐帧](actions/draw_ready/keyposes-4x.png) · [QC](actions/draw_ready/qc.json) |
| 持剑移动 `move_guarded` | 6 / 600 | 静态修正通过，待用户确认 | [浅底GIF](actions/move_guarded/preview-cream-6x.gif) · [逐帧](actions/move_guarded/keyposes-4x.png) · [QC](actions/move_guarded/qc.json) |
| 横斩 `attack_slash` | 6 / 730 | 候选，最终视觉待确认 | [浅底GIF](actions/attack_slash/preview-cream-6x.gif) · [逐帧](actions/attack_slash/keyposes-4x.png) · [QC](actions/attack_slash/qc.json) |
| 直刺 `attack_thrust` | 6 / 720 | 候选，最终视觉待确认 | [浅底GIF](actions/attack_thrust/preview-cream-6x.gif) · [逐帧](actions/attack_thrust/keyposes-4x.png) · [QC](actions/attack_thrust/qc.json) |
| 上举下劈 `attack_overhead` | 8 / 1020 | 候选，最终视觉待确认 | [浅底GIF](actions/attack_overhead/preview-cream-6x.gif) · [逐帧](actions/attack_overhead/keyposes-4x.png) · [QC](actions/attack_overhead/qc.json) |
| 剑面敲盾两下 `skill_taunt` | 8 / 1150 | 候选，最终视觉待确认 | [浅底GIF](actions/skill_taunt/preview-cream-6x.gif) · [逐帧](actions/skill_taunt/keyposes-4x.png) · [QC](actions/skill_taunt/qc.json) |
| 受击 `hurt` | 4 / 500 | 候选，最终视觉待确认 | [浅底GIF](actions/hurt/preview-cream-6x.gif) · [逐帧](actions/hurt/keyposes-4x.png) · [QC](actions/hurt/qc.json) |
| 收剑退出 `sheathe_exit` | 8 / 800 | 静态修正通过，待用户确认 | [浅底GIF](actions/sheathe_exit/preview-cream-6x.gif) · [逐帧](actions/sheathe_exit/keyposes-4x.png) · [QC](actions/sheathe_exit/qc.json) |
| 后仰倒地 `death_fall` | 8 / 1980 | 候选，最终视觉待确认 | [浅底GIF](actions/death_fall/preview-cream-6x.gif) · [逐帧](actions/death_fall/keyposes-4x.png) · [QC](actions/death_fall/qc.json) |

[原接受 battle_idle](actions/battle_idle/preview-cream-6x.gif) 保持原字节。每动作目录另有深底／透明 GIF、原始生成网格、PNG帧、Aseprite与来源；历史否决稿保存在已有／新增 attempt 目录。

## 本轮像素修绘

用户已明确允许逐帧像素修绘，实际使用 Aseprite 修改原生128×128像素，保留原生成图和失败稿。

- 拔／收剑以两端参考的头脸、盾、衣摆和靴局部像素统一角色外观；动作手臂和刀路保留生成底稿。没有增加整帧继承，没有逐帧缩放。
- 补回盾后手臂与连续鞘身，再由盾覆盖；收剑6–8帧补齐盾下、鞘根与大腿外侧的遮挡。
- 两套移动第6帧修复髋—膝—小腿连接并补膝褶和阴影；本次保留各自第1–5帧。

主执行已查看当前逐帧及实际原速GIF。主美已独立关闭本次主体跳变、装备缺口和第6帧腿部拼接问题；作者原速回看与主美静态检查分别署名。

## 来源与验证边界

拔／收剑和行进参考同一套加拿大官方操典；三类攻击按正式历史剑术教材路线补间，用户已同意。后倒借鉴Judo Canada的支撑／关节关系；受击、死亡与两次敲盾是游戏艺术设计，不伪称官方完整教程。

74交付帧＝72个生成来源底稿帧（本轮26帧像素修绘）＋2个原Idle原字节端帧。74帧实际Aseprite往返、33个GIF共222解码帧及原时长相符。本轮差分重验28帧ASE、12个GIF共84解码帧，原时长保持；其他7套复用46帧ASE／138帧GIF证据。两处继承仍保持原PNG字节。死亡的尺度统计仍为FAIL／姿态例外，不能称严格通过。

本次核对1657项冻结保护文件和4个外部固定输入均未变，包含其他7套、原Idle、FX及已有历史稿。旧100／300验证只属于旧否决版本，详见[修正前回执](corrections/visual-fix-01/before-delivery-receipt.json)。

[本次视觉问题和版本](corrections/visual-fix-01/visual-review.json) · [当前验证汇总](qc-summary.json) · [交付回执](delivery-receipt.json)

主美最新结论（turn 01a1344e-250b-7b60-86af-68c0acb66529）为 **ACCEPT，仅本次 PXL-1/2/3 静态修正**，新增必改项0；前轮NEEDS_FIX保留为历史。主美未独立原速观看，不代表整包或最终用户接受。详见[主美记录](../../../../docs/team/2026-09-30/art.md)。
