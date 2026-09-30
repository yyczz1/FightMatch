# L1 首批资源 Unity 导入交接提案

状态：`PROPOSAL_ONLY / NOT_EXECUTED`  
执行者：现有 C（保持 Unity 串行）；本美术会话不运行 Unity、不改 `Assets/` 或 `.meta`。

## 1. 建议导入集合

建议 C 只把三张可分离来源图纳入首轮导入；`first-batch-preview.png` 保留在 `ArtSource` 作评审证据，不作为运行时纹理。

| 原图 | 建议目标（提案） | 用途 |
| --- | --- | --- |
| `courtyard-battle-background.png` | `Assets/Art/FightMatch/L1/Backgrounds/CourtyardBattleBackground.png` | 战斗页背景／场景层 |
| `warrior-stand.png` | `Assets/Art/FightMatch/L1/Characters/Warrior/WarriorStand.png` | 战士静态站姿样本 |
| `clockwork-infantry-stand.png` | `Assets/Art/FightMatch/L1/Enemies/ClockworkInfantry/ClockworkInfantryStand.png` | 发条步兵静态站姿样本 |

目标路径尚未由本轮任务包批准；C 在实际导入包中应列出精确新文件和 `.meta`，由 SD00 审批后再执行，不能仅凭本提案创建。

## 2. 建议导入设置

### 背景

- Texture Type：`Sprite (2D and UI)`；Sprite Mode：`Single`。
- Alpha Source：无；Alpha Is Transparency：关闭。
- sRGB：开启；Read/Write：关闭；Mip Maps：2D UI 直显时关闭。
- Max Size：先用 2048，保留原 1024×1536；Android 压缩先比较 ASTC 6×6 与无压缩基准，不能在未目视比较前定最终格式。
- Filter Mode：`Bilinear`；Wrap Mode：`Clamp`。
- 当前图为 2:3，项目 PanelSettings 参考分辨率为 540×960（9:16）。建议以中央 864×1536 作为 9:16 安全裁切比较，确认两侧栏杆和远景损失可接受后，再决定运行时裁切、适配填充或补绘；不要直接拉伸。

### 战士与发条步兵

- Texture Type：`Sprite (2D and UI)`；Sprite Mode：`Single`。
- Alpha Source：输入纹理 Alpha；Alpha Is Transparency：开启。
- sRGB：开启；Read/Write：关闭；Mip Maps：关闭。
- Max Size：2048；Filter Mode：`Bilinear`；Wrap Mode：`Clamp`。
- Mesh Type：首轮比较 `Tight` 与 `Full Rect`；若 UI Toolkit 布局或点击框需要稳定矩形，优先 `Full Rect`，命中区域另由 UI 定义。
- Pivot：建议 `Bottom Center`，以脚底接触点作为战台站位基准；导入后检查空白边是否导致两者脚底不齐。
- Pixels Per Unit：若使用场景 SpriteRenderer，先以 100 作对照；若只进入 UI Toolkit，按实际容器尺寸缩放，不把 PPU 当视觉比例真值。
- 不建议把两张角色图和背景放进同一图集。首轮可不建 Atlas；正式批量角色确定后再按角色／敌人组图集并核 Android 内存。

## 3. 场景和引用关系提案

- 背景建议拆成独立环境层，角色保持独立可替换 VisualElement 或 SpriteRenderer；不要烘焙使用 `first-batch-preview.png`。
- 上方角色舞台和下方现有 `CandidateBoardElement` 应保持输入边界分离；背景图只提供视觉，不应接管棋盘命中测试。
- 当前 `FightMatchHostView` 依据安全区布局，棋盘高度按可用宽度收束；接入背景时应保持现有 Board 实际几何与业务坐标，不用背景草格推导规则格。
- 血条、端点、线路、意图、文本及紧凑角色栏继续由运行时 UI 绘制；预览中的图形只说明层级和对比度。
- 最终引用应由获准的 UXML／USS、视图代码或场景对象明确持有，并在冷启动与页面返回后重新核引用；本批不指定实现方式。

## 4. 动画生产缺口

两张角色 PNG 都是扁平 RGBA 图，不可自动视为可绑定骨骼的分层源。进入动作制作前至少需要：

- 战士：头、躯干、前后腿、持剑臂、持盾臂、剑、剑鞘、盾、披巾分别整理；补齐被遮挡关节和武器背面。
- 发条步兵：头／头盔、躯干壳、前后腿、持枪臂、空手臂、长矛、背部钥匙分别整理；补齐轴关节和遮挡区。
- 关键动作：放松站姿、抓柄／备战、拔剑或举枪、战斗待机、攻击前摇、命中、受击、倒下；战士准备动作还需遵循已定 0.8 秒候选节奏。
- 技术选型（2D 骨骼或逐帧）须以关键姿态可读性为先；本批静态图不替代该决定和动作验收。

## 5. C 的建议验收步骤

1. 导入前记录三张原图的尺寸、SHA-256 和 Alpha；与 `asset-register.json` 一致。
2. 在批准的精确目标路径导入，保存 Unity 生成的 `.meta`，不得覆盖已有未知 GUID。
3. 在 540×960 参考画面核背景中央 9:16 裁切、角色脚底、敌我朝向、轮廓和棋盘读图区。
4. 在至少一个更窄和一个更宽的 Android 比例检查安全区，不拉伸人物；记录裁切策略。
5. 角色缩到实际战台高度后检查盾、剑、长矛、发条钥匙仍可辨认；透明边缘无黑边或白边。
6. 棋盘端点、路线和触摸命中继续使用实际运行时元素；背景草格不能误导真实格数。
7. 检查纹理内存、Android 压缩伪影及动态 Atlas 行为；仅在有实际证据后定最终压缩和图集。
8. 运行任务包允许的编译／设备视觉步骤，分别记录“资源导入成功”“布局可读”“Android 视觉通过”；三者不可互相代替。

## 6. 源文件与许可证交接

- 保留本目录四张 PNG 与登记文件作为首版来源记录，不以 Unity 导入副本反向覆盖。
- 正式动画若产生 PSD、Krita、Spine 或逐帧工程，应另设获准的 `ArtSource` 批次，保存分层原件、导出规则、工具版本和来源记录。
- 本批无外部素材；如后续使用 Kenney、音效库或其他第三方资源，逐件登记原网址、作者、许可证文本、下载时间、原文件哈希及修改说明。
