# 发条步兵 A/B/C 静态造型候选

状态：`CANDIDATE / USER_SELECTION_PENDING / NOT_IMPORTED`。2026-10-12。

三者是同一个早期基础发条步兵的外观选择，不是三种单位、装备等级或新战术原型。共同保留已定的“小钥匙、圆锡帽”，使用短剑、小圆盾、青绿锡壳和黄铜关节。C 已按主美 F1 改为圆顶浅沿锡帽，保留叶纹胸板与弹簧腰的差异；旧方帽完整保存在 `candidate-c/history-v1/`，已退役，不供当前选择。

| 方案 | 实际外观差别 | 后续 6 帧 Idle 可用的次级部件 |
| --- | --- | --- |
| A 圆盔铆钉 | 宽帽沿、方形检修扣、棕色方靴；轮廓最朴素 | 发条小幅错拍、肩甲轻沉、剑尖回稳 |
| B 圆胸盖 | 圆形黄铜胸盖、较圆的头胸、黄铜鞋头；与 A 保持近亲轮廓 | 胸盖轻亮暗、发条错拍、肩部承重 |
| C 叶纹锡壳 | 圆顶浅沿锡帽、叶纹胸板、外露弹簧腰、锡靴；轻型机械结构与小钥匙 | 腰簧轻压缩、肩肘余动、发条迟拍 |

这些只是未来动作建议；本包每个 Aseprite 都只有 **1 帧**。需用户选 A/B/C，才进入新的敌人 Idle 制作包。

## 查看顺序

1. `clockwork-foot-soldier-abc-review-4x.png`：浅色背景并排 A/B/C，最近邻 4×，1632×666。
2. `player-enemy-scale-check-4x.png`：已接收玩家参考与三个敌人按同一 128 画布并排；未修改原玩家文件。
3. 每个 `candidate-*/preview-cream-6x.png`：单个候选最近邻 6×。
4. `player-animation-roster-v1.md`：玩家动作清单、生产顺序、Body/FX、状态衔接与未来逐动作验收。

## 每个候选的文件

- `raw-sheet.png`、`prompt-used.txt`：独立 image_gen 原图和实际提示词。
- `processed/`：generate2dsprite 的去底、提取、profile 与 QC 原记录；这是处理中间物，不是最终导入图。
- `processing-log.txt`：准确处理命令和退出结果。
- `clean.png`：最终 128×128 透明 PNG，朝左、16 色、二值 alpha、脚线 y=115。
- `clockwork-foot-soldier-a/b/c.aseprite`：对应文件名的可编辑源，32-bit RGBA，1 帧，`Body-Static` 图层。
- `aseprite-roundtrip.png`：从源文件导出的核对帧；与 clean.png 的 RGBA 像素完全相同。
- `preview-cream-6x.png`、`qc.json`：视觉预览及最终检查。

## 处理与证据

原始美术均由 image_gen 独立生成。generate2dsprite 用 1×1 静态输入，并复用未改动的玩家 `character-scale-profile.json`；没有生成多动作混合图集。A 原图朝右，最终执行水平镜像；B/C 保持原朝向。去除粉底后将 alpha 以 128 为阈值二值化，三套共用 16 色无抖动色板，再作整数平移统一底部中心脚锚。源像素只做上述确定性处理，没有用代码绘制原始角色。

三个最终 bbox 高均为 104 px，玩家已接收首帧为 102 px，差约 1.96%。同一 profile 的 output origin `[64,117]` 与最终脚线 `y=115` 分别记录，没有篡改 profile。A/B 原图归一化面积相对玩家 profile 漂移 6.87%/5.84%，修正后的 C 为 9.77%；该跨角色指标保留为提示，最终大小、脚锚、透明度、色量、裁切和视觉连接单独核验。单帧的 CV=0 不能证明未来动画尺度稳定。旧 C 的 12.91% 指标仅属于 history-v1 中的方帽探索。

最终三套均 128×128、16 个不透明颜色、alpha 仅 0/255、脚线 y=115；无输出边缘接触、无空帧、无粘贴裁切。浅底看过头、手臂、剑柄、盾、髋膝、脚和发条，未见明显断口、黑色矩形空隙或裁掉的末端。C 的腰侧留白是可见弹簧结构两侧的背景，发条孔、腿间与手臂间的背景为正常负形。可编辑源全部经 Aseprite 导出逐像素核对。

汇总见 `delivery-receipt.json`、文件哈希见 `file-manifest.json`；详细每项见各候选 `qc.json`。这是作者自检，专业接收与用户视觉选择仍待主美处理。

## 范围与回传

本 thread `01a12f4d-81d7-7602-929a-75c94729f23b` / host `local`。首轮 turn `01a12f4d-8468-7a81-bf18-6f9a2fd98777`；主美 NEEDS_FIX 后 F1/F2 修正 turn `01a12f5d-524c-7c61-b29e-dd4f91512e7a`。中央制作 thread `01a0ec79-5b64-7b62-8365-bee9229512b5` 的限定包与主美本次两项纠正为输入；用户既有团队授权见 AGENTS.md §6。唯一专业收件人为主美 `01a0f2e3-3a47-78f2-819c-cbe52e8d7e47`，本修正不通知中央。

F1：仅 C 用 image_gen 编辑圆锡帽并重做其处理、源文件与对比图，A/B 文件全部哈希保持。F2：动作清单共同契约已补母帧 + character anchor sheet 的生成前展示、相机/尺度/root/脚线/留白锁定与 profile 复用，并排除倒地/击退机械套站立模板；仅文字，没有新玩家图。两项由作者提交待主美复核，不自授专业 ACCEPT。

仅写入本目录。没有制作玩家新动画或敌人动画，没有改现有玩家 Idle，没有改 docs/Assets/Packages/ProjectSettings/.meta/项目代码，没有运行 Unity 或 Git 写操作，没有购买或下载付费素材。未进行导入、Animator 接线、游戏内及设备验收。下一步停在用户选择 A/B/C。
