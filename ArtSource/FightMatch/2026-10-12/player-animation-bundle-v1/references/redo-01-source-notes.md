# REDO-01 人物动作参考

2026-10-13。旧版除 battle_idle 外被用户否决；11套人物重做，FX延期。来源只证明列出的动作关系，插帧与短剑盾适配属于艺术设计。

| 来源 | 实看图版 | 使用与改编 |
| --- | --- | --- |
| [Canada Sword Drill 第6章](https://www.canada.ca/en/services/defence/caf/military-identity-system/drill-manual/chapter-6.html) | 6-1-4拔剑、6-1-5收剑、6-1-2佩挂、6-1-6B行进，均已实际查看 | 右手跨身取左髋剑柄、沿鞘轴出入、完全离鞘再转腕；角色持盾，不能照搬左手扶鞘。呼吸、盾外展和腰挂鞘角度为适配。 |
| [Canada Drill 第3章](https://www.canada.ca/en/services/defence/caf/military-identity-system/drill-manual/chapter-3.html) | 3-2行进 | 双腿交替触地、承重、通过；角色采用短步和持剑盾防护，不复制军步幅度、大摆臂。 |
| [Judo Canada 后倒教材](https://judocanada.org/wp-content/uploads/2023/07/Guide-Chutes_2023_EN.pdf) | 2023版第7/8页，实际PDF渲染/view | 关节与承重/后倒连续关系；不是死亡或带武器受击教程。用户指定受击后后仰，头左脚右，去掉双掌拍地和起身，剑盾不离手。 |
| Henry Charles Angelo，1845 Infantry Sword Exercise | PDF15/17/18，已实际查看原扫描 | 横切5/6、垂直7、分两动直刺；只有刀路图与关键姿势，无完整人体连续图。用户已同意按教材路线补齐动画。具体来源证据见下方研究稿。 |

[主美攻击研究](../../../../../docs/team/2026-09-30/art-player-combat-reference-study-2026-10-13.md)保存扫描来源、页码、镜像边界。用户另外确认剑面敲盾两下，未找到官方完整教程；必须标注为表演设计，不伪造来源。

## 当前制作问题

2026-10-13 用户批准“允许逐帧像素修绘（推荐）”。实际Aseprite修绘已完成拔剑／收剑／两套移动共26底稿帧，另2个原Idle继承端帧未修改。原生成图、提示词及失败attempt保留。实际像素修绘来源与范围见各动作qc.json；本轮未新增imagegen调用。

PXL-1/2/3 已由主美在 turn 01a1344e-250b-7b60-86af-68c0acb66529 独立静态关闭（ACCEPT，限定三项），当前整套交用户看效果；主美未独立原速播放。出入鞘使用原Idle与收剑待机的局部参考像素修正头脸、盾、衣摆和靴，再修盾后手臂及鞘根遮挡；两种步态再次修第6帧的髋膝连续性。角色外观跳变属于必改问题，不能留作审美偏好。没有新增整帧继承或逐帧缩放。总74交付帧=72生成来源底稿帧（部分修绘）+2继承端帧。死亡严格统计仍按姿态例外保留，不能称数字严格通过。
