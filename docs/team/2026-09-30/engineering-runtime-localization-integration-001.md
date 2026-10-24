# RUNTIME-LOC-001：真实 Host 正式文本源接入缺口与最小下一包

2026-10-01 · `PREPARED_NOT_AUTHORIZED / HOST_COMPOSITION_GAP`；仅文档，不是实施、测试或集成接收。
本稿 owner SYS `01a0f2e6-29bb-7092-abf0-705b41b7bf93/local`，实际 turn `01a0f6f0-963b-7601-a4f5-e4bcc58797bf`；唯一回中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。authority：用户 AGENTS §6 站立协作授权＋中央本轮限定委托。未来 C 执行仍需另签，默认 Astra/xhigh。

## 固定输入与核定

- [LOC 设计](engineering-localization-luban-001.md) SHA256 `022c060801df22520653856c24bd78dda394deb26f946dbff1b70ec9c338380a`；[RES 设计](engineering-yooasset-001.md) SHA256 `2e12b5f24ecedef03df5795c9632318c55fe6330562731892d4c9704c54740d8`。
- [真实复开准备](testing-ugui-native-reopen-001.md) SHA256 `1bf1811ae990a32134829e4b44b576b15c9bcfa3be90e1be3473539471fe2601`；`Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs` SHA256 `12dba7d11323da883f108ea416699a6cf638a9360587c984d7d070ebeb6cc287`。
- `Assets/Scripts/FightMatch/Presentation/Localization/LocalizationService.cs` SHA256 `ca8b2c5f9b8afd864410b35f7ab9998c04a7b5ea9efdf3a51e8082b6dd54a408`；`Assets/Scripts/FightMatch/Host/FightMatchHostView.cs` SHA256 `d7a29970b0fb8b594e7114a883b82ffef92c47181218df1bec1f420dfda72136`。签发前重核输入漂移。
- 真实 `Start()` 构造空源、Bind 后在 `LocalizationNotReady` 退出；尚未加载业务 catalog、访问玩家存档或创建 Session。service 的 source 为 readonly，`IsReady` 仅检查非 null，不能代替资源／业务就绪。
- 当前 Config／Tools／Generated 是待接收候选；未见 B parser／adapter／preference 产品文件，Packages 未声明 YooAsset。文件出现、FIX20 的12／222证据及单纯引用复开均不能证明正式接入。

## 已有合同直接复用，不重写

- [LOC-B](engineering-localization-luban-impl-b.md) §4–6 有 parser／adapter 精确路径和合同，但明确禁止 Host／production loader；[依赖拆分](engineering-localization-luban-impl-b-dependency-split.md) §1–3 明确 B-CATALOG 等真实 LOC-A 接收，不能预测 schema／预算。
- [B-PREF](engineering-localization-luban-impl-b-pref.md) §5.1 已有六路径完整包：独立于 Luban 材料，可先补签实施；仍须接受的 uGUI 输入、C 串行槽及验证合同。它本身不把语言偏好接入实际 UI。
- [RES-01A](engineering-yooasset-res-01a-implementation.md) 与 [RES-01B](engineering-yooasset-res-01b-implementation.md) 已有精确包；A 是另行授权的包安装／兼容性门，B 等 A 接收后可独立于 LOC 材料实施，无 Host／真实映射权限。
- RES 设计 §18 的 C／D 仍是分包边界，缺可签发的精确文件／接口／输入清单；[bootstrap 清单](art-res-01c-bootstrap-inventory.md) §4 另列16个未接受的候选缺键，不是可直接导入资源。未找到完整 Host composition 实施合同。
- 旧包中未执行的本地 R／全量验证安排须在补签时按当前 TEAM_WORKFLOW §7、dots优先及差异验证收紧；保留历史事实，不另派 R 或机械重跑。

## 最短合法路径与可提前工作

1. 复用 LOC-A：正式 Excel／CSV → Luban → 既有配置校验／发布候选；材料、runner许可、确定性与 artifact／manifest 真实身份接收后，才签 B-CATALOG。不得直接读策划CSV或手改 Generated。
2. 材料等待期间优先准备／条件满足后批准 B-PREF 的本地代码；RES-01A／B 按其独立授权门推进。C／D可先冻结桥接职责、文件清单、失败映射与测试计划；本稿不授权配置、资源、包安装或下载。
3. C补齐同一表格权威生成的内置双语最小文本、TMP与恢复UI；D补齐 YooAsset `fm.text.full`、精确 ReleaseSet 身份／SHA／schema／source receipt 准入，以及现有六件业务发布物桥接。真实产物、映射、打包和运行验收等待相应材料接收。
4. C／D还须冻结 Host 可调用的生产 bridge／factory、生命周期及程序集引用；RES-B的adapter目前设计为internal，不能由接线者随手扩大public API。上述模块接收后才签下面 H；这不是另写 Demo loader 的许可。

## H：实际 Host 的最小接线候选

- 唯一入口保持 `FightMatchPlayerHost.Start()`。先保持C内置bootstrap可用，等待D对同一精确ReleaseSet返回受验文本bytes／manifest身份；交B-CATALOG同步解析，成功后构造adapter，再构造有源`LocalizationService`。不修改IsReady、不回填readonly、不绑定测试source。
- 首次正式UI Bind前通过B-PREF加载语言：有效手选优先，否则中文系统用简中、其余英文；保持EN／ZH-Hans与诊断占位符。六件业务catalog继续经既有校验／发布流；Session、存档兼容性和必要资源满足后，才BindView／ObserveStartup并开放业务输入。
- HostView语言选择连接B-PREF：本次语言立即生效，保存结果分别绑定saved／save_failed／save_unknown；失败后重新选择同一语言只幂等保存desired locale，不重复重绑、不进入PlayerSave恢复。复用现有两语言按钮，不新增资源。
- 取源／准入／解析失败留在C内置双语诊断／重试路径；重试同一immutable target、新epoch，旧回调不得发布。部分CodeEntered后的故障依RES §13退出／重启，不混合旧新集合。现有`ShowFailure`只能保留诊断gate；`reloadButton`调用Session.ObserveStartup，不是资源重试入口。
- 成功交接、取消、禁用／销毁与重试按D所有权释放lease／回调，保留Host现有Unbind、低内存、焦点和Session.Dispose语义；禁止静态全局source或重复Session。

H拟白名单精确六路径（仅在C／D接口闭合后补签；本轮均只读）：

```text
Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs
Assets/Scripts/FightMatch/Host/FightMatchHostView.cs
Assets/Scripts/FightMatch/Host/FightMatch.Host.asmdef
Assets/Tests/EditMode/FightMatchHost/FightMatch.Host.Tests.asmdef
Assets/Tests/EditMode/FightMatchHost/FightMatchRuntimeLocalizationIntegrationTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchRuntimeLocalizationIntegrationTests.cs.meta
```

前四项仅改装配／偏好接线及已接受项目桥接引用，不直接依赖YooAsset；后两项为新建。其余代码、已有测试、public API、场景／Prefab／既有.meta、Config／Tools／Generated、Packages／ProjectSettings、存档与旧证据禁改；需要额外路径先回中央补签，不在H吸收C／D工作。

## 定向验收与当前停点

- 聚焦验证：真实桥接→parser→service顺序；缺失／坏SHA／错误schema或ReleaseSet时零Session；成功仅一次；取消／销毁／旧epoch零迟到发布；双语初始值与跨重启偏好、三态保存反馈；无假source、路径直读或手写成品copy。准确fullname／次数／evidence root由后续激活单冻结。
- 实际Demo须经正式源启动及真实UI进入导航／战斗，证明绑定文字非占位符，且资源／存档保护成立；测试注入仅限隔离单元故障测试，不能替代真实场景验收。对应新head须GitHub PR审查，测试／设备门分别记录，既有12／222仅复用未变范围。
- 存档隔离只定位：[真实复开准备](testing-ugui-native-reopen-001.md) §2.4／G3确认现有Host无隔离根注入；须另包批准，不在H改存档或探针绕过。未闭合前不启动会触达真实玩家根的验证。
- 结论：可复用包已齐的是B-PREF与RES-A／B；立即可做的是补签／缺口冻结，不是直接修改Host。C／D精确合同、LOC-A材料接收和uGUI当前head接收仍为门。本轮只新增本文，Unity／测试／dotnet／网络／安装／Git写入／派工均未执行。
