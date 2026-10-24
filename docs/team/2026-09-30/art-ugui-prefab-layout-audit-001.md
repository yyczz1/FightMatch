# ART-02：uGUI Prefab 静态布局审计 001

2026-10-01 · 只读静态审计

## 1. 责任、身份与范围

- 交付负责人：主美，thread `01a0f2e3-3a47-78f2-819c-cbe52e8d7e47` / host `local` / turn `01a0f504-990f-7c11-be0c-5fc285dcb09d`。
- 派发来源：中央AI，thread `01a0e401-511d-79f2-b47f-3ab0ade1681b` / host `local`。
- 用户授权：`AGENTS.md` 第 6 节的五角色组织与团队内派发；本包只允许新增本报告。
- 审计对象：固定的 `FightMatchRuntimeRoot.prefab`、其 `.meta`、`FightMatchDemo.unity`，对照已接收的 ART-001 灰盒与 263 键文案快照。
- 方法：只读解析 Unity YAML 的 GameObject、RectTransform、Canvas、CanvasScaler、GraphicRaycaster、布局组、LayoutElement、ScrollRect、Mask、Image、Button 与 TMP 序列化字段；逐层重建父子路径并检查固定输入身份。
- 明确未做：未运行 Unity、未打开 APK、未进入 Play Mode、未声称动态生成内容或原生 Canvas 驱动后的最终像素结果，也未检查变化中的产品源代码来替代固定候选证据。

## 2. 固定输入身份

| 输入 | 字节 | SHA-256 |
| --- | ---: | --- |
| `Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab` | 1,060,568 | `55ded35adf6fd7d4e207303801e3ada172190bea6a516443e1bcd21d3193004b` |
| `Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab.meta` | 154 | `85c3eee205f94f79652eddf5d29cce3809b76ce03a0ba92311150d129409fce2` |
| `Assets/Scenes/FightMatchDemo.unity` | 12,900 | `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd` |
| `docs/team/2026-09-30/art.md` | 19,544 | `3b9a504345e509df5085688e1c897e7e737612a4db03f5bda770ab8315eb6e57` |
| `ArtSource/FightMatch/2026-09-30/ugui-layout-prototype/README.md` | 5,065 | `33dea5d90064ec62ff6e015d00b157cd16c592b907e4e29f1099cd366d3346ff` |
| `ArtSource/FightMatch/2026-09-30/ugui-layout-prototype/asset-register.json` | 5,824 | `52eac5b72696161ae8c716468d143371f509ab654092c8e78b47e0e9c3b08b31` |
| `docs/team/2026-09-30/snapshots/ugui-copy-amend-03-fix-01/planning-localization-draft.csv` | 64,708 | `d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c` |

以上身份在审计开始时与派发值一致；第 9 节记录落盘后的复核。

## 3. 静态通过项

1. **Canvas 基线正确。** `RuntimeCanvas` 为 Screen Space Overlay，带 CanvasScaler 与 GraphicRaycaster；Scaler 序列化为 `Scale With Screen Size`、`540×960`、match `0.5`（Prefab `23267–23365`）。
2. **安全区与大层顺序正确。** `SafeAreaRoot` 全伸展并挂 `SafeAreaFitter`；直接子级顺序为 `ScreenLayer → HudLayer → PopupLayer → SystemLayer → TemplatePool`（Prefab `34519–34569`）。`BackgroundLayer` 位于安全区外、铺满 Canvas，且背景 Image 的 `RaycastTarget=0`（Prefab `10644–10718`）。
3. **场景输入骨架存在。** 固定场景实例化 Prefab GUID `c36df3cfc25424c8cb3ec6cae6be1237`，场景中只有一个名为 `EventSystem` 的对象；Prefab 本身只有一个 Canvas。
4. **主要列表滚动结构完整。** `BattleScroll`、`NavigationScroll`、`ResultScroll`、`StartupScroll` 与 `LicenseScroll` 均静态满足 `ScrollRect → Viewport(Image+Mask) → Content(VerticalLayoutGroup+ContentSizeFitter vertical preferred)`，引用非空。三个角色槽内的 `Template/Viewport/Content` 是字段内部 ScrollRect，不按长列表结构误判。
5. **按钮最小触控高度合格。** 静态枚举到的 45 个 Button 均有 `LayoutElement MinHeight=64 / PreferredHeight=72`；其 Caption 通常为 48 高。未发现固定绝对坐标把按钮放到画布外：249 个 RectTransform 中，绝大多数 anchored position 为零，唯一的主要非零纵向布局是 `ScreenLayer y=-44 / height delta=-88`。
6. **诊断占位符合约束。** 119 个 TMP 默认文本全部保留 `【if you see this, it is a bug.】`；其中 116 个完全等于该串，3 个输入字段值在末尾附加零宽空格。119 个 TMP 的 `RaycastTarget` 全为 0，没有把正式玩家文案硬编码进 Prefab。
7. **文案快照完整。** CSV 实读为 263 行、263 个唯一 key、0 重复；`key/screen/element/state_or_trigger/zh_cn/en/severity/qa_notes` 每行均非空。

这些通过项只说明静态装配基础存在，不替代第 4 节对已接收画面骨架的差异判断。

## 4. 必修问题

### F-01 · 阻断：战斗页不是已接收的五段战斗骨架

- **对象／证据：** `BattlePage/BattleContent/BattleScroll/Viewport/Content` 使用整页 VerticalLayoutGroup + ContentSizeFitter（Prefab `16751–16832`），其五个直接子项依次是 `Headline`、`BattleStatus`、`BattleHud`、`Actions`、`History`。`BattleStatus` 又把 `BoardFrame` 与 Phase、Save、Enemies、Availability、Members、Retry、Resolve、Hp、Intent、Beat、Stage、Diagnostic、Skip 共 14 项放在同一个纵向列表中（Prefab `33907–34024`）。`Stage` 是 64 高 TMP 文本行，不是敌我固定角色槽。对全部 250 个 GameObject 精确检索，未找到 `TopBar`、`BottomHud`、`AllySlots`、`EnemySlots`、`ActorSlot` 或 `ReferenceInfoDrawer`。
- **棋盘证据：** `BoardFrame` 在受父布局控制的宽度下只声明 `MinHeight=420 / PreferredHeight=420`，`PreferredWidth=-1`，且没有序列化的正方形约束组件（Prefab `17764–17855`）。在 540 基准、灰盒安全宽 508 时，父 Content 左右各 12 padding 会给它约 484 的布局宽，而高度仍为 420；当前静态约束不能保证 508×508 或任何 1:1 结果。
- **与基线的差异：** 已接收结构要求 `TopBar 56 → Stage 166/190 → BattleStatus 48 → BoardFrame 508×508 → BottomHud 142`，棋盘、触控与角色槽保持固定关系；当前候选是一个会纵向增长的功能表单，棋盘之后仍串接大量状态和操作。
- **玩家影响：** 主战斗信息层级被滚动顺序取代；棋盘可能呈非正方形，程序几何与命中区域面临拉伸／错位；敌我位置、脚点、HP/意图独立 HUD 和固定空槽没有可审查的视觉接口；底部主操作不能保证在棋盘同屏可见。
- **最小修正：** 保留现有行为引用与业务状态，不改语义；把 BattlePage 的可视容器改成明确五段。棋盘增加可静态审查的 1:1 约束与安全宽减 32 的上限；Stage 新增双方各三个固定底部中心槽；状态 48 高；BottomHud 固定承接角色卡和有限操作。历史、长诊断及额外动作进入独立抽屉／局部滚动区，不能让整块棋盘随长表单滚动。若要偏离这些值，先形成新的获批美术接口，而不是让 Prefab 暗改基线。

### F-02 · 高：默认参考层会覆盖并截获棋盘下沿

- **对象／证据：** `PopupLayer/ReferencePopup` 锚定安全区底部 `anchorMin=(0,0)`、`anchorMax=(1,0.42)`，自身 Image `RaycastTarget=1`，内部为纵向布局（Prefab `20688–20825`）。它位于 `PopupLayer`，天然绘制在 `ScreenLayer` 与 `HudLayer` 之后。
- **与基线的差异：** 已接收接口要求 `ReferenceInfoDrawer` 替换约 142 高的 BottomHud，不盖棋盘；当前 42% 安全区高度在 540 灰盒安全高 920 时约为 386.4，且战斗棋盘没有固定在其上方的独立槽位。按当前静态布局顺序，参考层与棋盘下沿存在重叠区。
- **玩家影响：** 参考说明会遮住真实棋盘下方格子，整块 42% Image 还会成为射线目标；这与“参考线和说明不偏移、不拦截真实起笔／取消”的约束相冲突。
- **最小修正：** 将参考说明作为 BattlePage 内 BottomHud 的互斥内容，顶部不得越过棋盘槽下沿；只有实际控件与必要的抽屉表面参与命中。真实起笔时同时隐藏／禁用抽屉与参考命中层，保留程序棋盘原 RectTransform。

### F-03 · 高：普通弹窗缺少“全屏阻断层 + 居中面板 + 正文局部滚动”的静态结构

- **对象／证据：** `QuitPopup`、`LanguagePopup`、`LicensePopup` 以及确认子根都直接全伸展到 SafeArea；以 `QuitPopup` 为例，根 Image 负责全屏阻断，同时根 VerticalLayoutGroup 仅有四边 12 padding（Prefab `26361–26480`）。`QuitPopup/Body` 是固定 128 高 TMP，未处于正文 ScrollRect；固定候选中只有 License 正文有专用 ScrollRect。确认根同样是全屏 VLG，未见最大宽 476、距安全区至少 32 的独立居中 Panel。
- **与基线的差异：** 普通确认应让遮罩全屏，但可见面板居中且受最大宽／边距约束；长正文只滚正文，标题与主次动作固定。当前“遮罩”和“内容面板”是同一个全屏布局根。
- **玩家影响：** 画面会读成全屏表单而非确认弹窗；长英文或动态诊断增长时，无法从静态结构保证正文滚动且按钮仍可见；横向仅 12 的内容边距也不满足 32 的弹窗安全边距。
- **最小修正：** 保留每个全屏 Raycast 阻断根，在其下新增居中 DialogPanel；面板宽度取 `min(476, safeWidth-64)`。标题与动作置于固定区，正文用独立 ScrollRect；两按钮横排不够时切全宽纵排，不缩小 48×48 热区。

### F-04 · 高：保存恢复界面位于错误的层级

- **对象／证据：** `RecoveryPopup` 是 `PopupLayer` 的第二个直接子项（PopupLayer 子级序列见 Prefab `25740–25809`；Recovery 根与父 fileID 见 `13523–13573`）。`SystemLayer` 的两个直接子项仅为 `LoadingOverlay` 与 `BlockingDiagnostic`（SystemLayer 见 `13056–13092`）。
- **与基线的差异：** 已接收接口明确要求保存恢复作为完整高优先级页面／SystemLayer 面板，不塞入普通 Popup；SystemLayer 还必须不被普通弹窗盖住。
- **玩家影响：** 当前恢复流程与普通语言、许可、退出、确认处于同一层级和调度域，无法从层级上保证恢复动作的高优先级、完整页面容量及与普通弹窗的互斥关系。
- **最小修正：** 将恢复的可视根与全屏阻断归到 SystemLayer，保留现有恢复视图引用和业务语义；按“当前头／原请求／候选／错误／安全下一步”分区，并让动作区全宽纵排。PopupLayer 只保留普通确认、详情、语言、许可、退出等普通覆盖层。

### F-05 · 中：固定页头高度与战斗可用高度未按灰盒接口实现

- **对象／证据：** `HudLayer` 顶锚、高 80，直接包含 `Back / Language / License`，其 HorizontalLayoutGroup 上下 padding 各 4，按钮静态首选高 72（Prefab `9640–9704`）。`ScreenLayer` 通过 `anchoredPosition.y=-44 / sizeDelta.y=-88` 留出 88 的顶部区域（Prefab `34025–34063`）。
- **与基线的差异：** 灰盒 TopBar 为 56，按钮热区下限 48；当前固定头占用 80，另留 8 间隔，并且 BattlePage 自身没有可识别 TopBar。该差异进一步挤压 540×960 的战斗纵向预算。
- **玩家影响：** 同样的安全区内，舞台、状态、棋盘和 BottomHud 比灰盒少 32 高的预算，当前实现因而更依赖整页滚动。
- **最小修正：** 将通用页头接口收敛到已接收的 56 高、48 高紧凑按钮，或在重新获批后更新灰盒接口；不要通过缩棋盘、缩正文或缩热区吸收差值。

## 5. 未发现的静态故障

- 未发现普通交互按钮的静态高度低于 48。
- 未发现非零固定坐标把主要页面／按钮直接放到 540×960 画布外。
- Content、Viewport、Page 等 `sizeDelta=0` 多为四向 stretch 或由 LayoutGroup/ContentSizeFitter 接管，不能把零字段本身误报为零像素。
- Background TMP／装饰文字不参与命中；背景 Image 已关闭 Raycast。

## 6. 场景 Canvas 零值覆盖：已记录，不能仅凭 YAML 定性

`FightMatchDemo.unity` 的 PrefabInstance 对 `RuntimeCanvas` RectTransform fileID `1013562322995895578` 覆盖了 pivot `(0,0)`、anchorMax `(0,0)` 和 localScale `(0,0,0)`（Scene `258–285`）。对应 Prefab 中 `RuntimeCanvas` 是 `m_RenderMode=0` 的 Screen Space Overlay 根 Canvas，并带 CanvasScaler（Prefab `23286–23352`）。

这组零 override 是确切静态事实，但根 Canvas 的 RectTransform 可由 Unity 原生 Canvas 系统驱动；在未运行 Unity 的本包内，它**不能单独证明**最终画面必定缩为零。主程／测试需在固定场景实际解析后读取 Play Mode 中的 Canvas rect、lossyScale、Canvas scaleFactor 与首帧截图：若原生驱动后仍为零，才升级为运行阻断；若被正常驱动，则保留为预期序列化证据。这里不要求主美运行 Unity，也不把该未知项用作本报告结论的依据。

## 7. 文案与真实画面检查单

### 7.1 Prefab 默认值

- 119 个 TMP 都显示诊断占位，不应改成硬编码正式文案。
- 115 个 TMP 同对象带项目自有 `LocalizedTmpText`；其序列化 key 当前为空，是否由运行时视图绑定填入需在画面与日志中验证。
- 4 个无同对象 `LocalizedTmpText` 的 TMP 是三个输入字段值：`.../PermanentDetail/Quantity/TextArea/Value`、`.../SourceTemplate/Start/TextArea/Value`、`.../SourceTemplate/Count/TextArea/Value`，以及 `LicenseBody`。前三者是动态输入值形态，LicenseBody 是运行时许可正文形态；都必须检查运行时注入后不残留占位，不能通过 Prefab 硬编码正式文本掩盖缺失绑定。

### 7.2 263 键优先截图样本

每个样本至少在 `540×960`、`1080×2400` 和四边非零 inset 的模拟安全区中各查英语／简中；动态参数使用可支持的最长实际值或测试上限。

1. **最长正文与确认容量：**
   - `fm.guest_local_only.body`：英语 191 字符；应约六行容量或正文滚动，动作始终可见。
   - `fm.battle.restart.preview_body`：163；检查重来确认正文滚动、取消为安全默认。
   - `fm.profile.data_upgrade.body`：145。
   - `fm.input.nonnegative_integer`：144。
   - `fm.save_result.notification_failed`：138，并填入最长允许的 `{errorCode}`。
   - `fm.battle.exit.preview_body`：119。
   - `fm.save_recovery.end_uncommitted_confirm`：116。
   - `fm.save_recovery.unknown`：115。
2. **长按钮：** `fm.save_recovery.continue_candidate_button`（英语 31）、`fm.battle.restart.confirm_button`（27）、`fm.result.replay_button`（26）、`fm.operation.set_item_preference`（26）、`fm.profile.creation_pending.continue_button`（26）、`fm.mobile_data.continue_button`（25）。检查全宽纵排／受控两行，不截断、不缩字号硬塞。
3. **动态摘要：** `fm.initial_download.summary` 同时填满 `{totalSize}`、`{remainingSize}`、`{networkName}`；`fm.history.action_row` 使用长角色名／操作名／敌人名；`fm.reference.condition.member` 使用长技能摘要；`fm.reference.difference.level`、`fm.battle.playback.critical_hit` 使用最大合理数字。
4. **战斗几何：** 同帧标注 TopBar、Stage、48 高 BattleStatus、正方形 BoardFrame、BottomHud；确认棋盘实际像素宽高相等、程序格线与命中重合、双方固定槽不因倒下重排。
5. **参考抽屉：** 打开默认参考时，抽屉不得进入棋盘 Rect；真实起笔后抽屉、参考线和阻断射线立即让位；检查棋盘最下行仍能拖线与取消。
6. **弹窗与恢复：** 长正文只滚正文，标题／动作固定；可见面板距安全区至少 32；保存恢复压在普通弹窗之上并保持完整页面容量。
7. **通用质量：** 0 个诊断占位残留；简中无缺字方框；英文无省略重要动作；按钮禁用态除透明度外仍有明度／饱和度差；页面反复进入无内容重复；所有非交互装饰不吞点击。

## 8. 结论

**Verdict: `NEEDS_FIX`**

固定候选已经具备可工作的 uGUI 基础设施和安全的诊断默认值，但尚未实现已接收的战斗五段布局；参考层、普通弹窗和保存恢复层级也与冻结接口不一致。修正应只改布局／层级与必要绑定，保留现有业务语义，然后由独立测试在 Unity 和设备画面上关闭第 6～7 节未知项。

## 9. 交付后完整性

落盘后应重新计算第 2 节全部固定输入；任何输入哈希漂移都会使本报告失效。本报告自身的最终字节数与 SHA-256 由交付回执填写。
