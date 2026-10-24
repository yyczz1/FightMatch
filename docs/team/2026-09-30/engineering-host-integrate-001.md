# HOST-INTEGRATE-001：已接受Host接线最小共享工程接入单

2026-10-08 · PREPARED_UNRUN。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，actual `01a11794-4243-7291-966d-731c049e03fb`。依据用户继续首Demo及已验证模块自主接入授权、中央本轮有限委托；本轮仅新增本文。唯一实施者为中央签发后的C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，`gpt-6-astra/xhigh`、fresh actual；中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 为唯一完成接收者。等C当前M04结束，不能并发写同一工程。

## 1. 已核结果与交付边界

最小产品差量只有**三份Host源码替换＋两份缺失meta补入**。不复制PR16整个32叶，不改场景、Prefab、程序集、资源模块或已有测试。完成目标是共享源码具有已接受的偏好加载／选择保存／隔离接线，不是正式LOC或正常玩法可用。

本轮对PR16发布32叶逐一验封存后像并与共享路径比较：11叶已相同、4叶不同、17叶缺席。其中5叶为本包目标；11同像不写；10叶旧AssetAccess源码／程序集／测试及meta不引入；另6叶Host测试差量不属于本次产品接入，保留封存证据，不顺带覆盖测试。
共享`Assets/Scripts/FightMatch/AssetAccess/`及`Assets/Tests/EditMode/FightMatchAsset/`实际缺席；较新资源候选仍在独立TestArtifacts交付中，不能说已在共享工程运行，也不能用PR16旧资源合同取代它们。当前Host及其asmdef没有AssetAccess／YooAsset调用或引用，因此本包无资源API适配动作。
对H.inputs内FightMatch的`.cs/.asmdef`作局部比较：197叶相同，差异恰三Host目标与较新的AndroidBuild，缺少旧AssetAccess的两源码／asmdef，没有额外共享FightMatch源码。该结果是静态输入事实，不是新组合编译通过。

## 2. 固定输入与五路径白名单

R=`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`，下列路径相对R；H=`TestArtifacts/FightMatch/HOST-CONTINUATION-SOURCE-001/FIX01/T01/`；S=H+`candidate/`。**实际复制来源只用S＋产品相对路径**，不使用publication-inputs的旧source地址，也不从当前投影取后像。

| 证据 | bytes / SHA256 |
| --- | --- |
| `docs/team/2026-09-30/host-continuation-fix01-integration-receipt.json` | 3420 / `ceae4f194dcb3aaf68a8a18739d68a13ca947dda237a0bca7e7b4989a5fe69f3` |
| H+`inputs.json` | 420831 / `7c960aab0db21db94c2af3d30e46b51fcf51e452768e06828655057f01d97614` |
| H+`publication-inputs.json` | 17867 / `18c8cfd1f9e571e7922691bb7b935497d0f8a800e3ffe4a75211a9817f30d050` |

接受身份：PR16 head `030898e9a317d10b610058d867529be8b0770c7c`，中央限定ACCEPT、10/10零skip、同head GitHub无major；原review非formal APPROVED、PR未合并。直接引用原回执，不重跑十项、不派重复审查；M04／PR17的probe问题不改写这个产品接收。

| 唯一允许产品路径 | 当前前像 bytes / SHA256 | 固定后像 bytes / SHA256 |
| --- | --- | --- |
| `Assets/Scripts/FightMatch/Host/FightMatchHostSession.cs` | 18579 / `892b9d494286f18e41e6f6bfbe474df591d9c06fa19644bec36332391cf50859` | 20308 / `67d62db541c47e3e1548ec32000605b6dcecf38b98a7332195a0824cc51060a9` |
| `Assets/Scripts/FightMatch/Host/FightMatchHostView.cs` | 17703 / `e72de88638d6c25bfe4d80e2c1b8990dae5ae225c6d4fe976b4b1641b1c975b9` | 18589 / `feceefca6d529cbc9755f8f984db7b3b176bc0709730c83947ea07bb968cb687` |
| `Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs` | 6595 / `12dba7d11323da883f108ea416699a6cf638a9360587c984d7d070ebeb6cc287` | 23469 / `d3774d5c9cf20cdf9649540f2479ebb198241188ecfdf88bdae09d1122c22456` |
| `Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs.meta` | ABSENT | 243 / `7ae6d48214039b2cbdcb9fe7a39504cf0d5c08e0cba084e202eb0762aebe5047` |
| `Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs.meta` | ABSENT | 243 / `2cb6b65d3208db04467d713586cc591c6c187064cf5f222633aa13d66b19a135` |

两meta复用已接受完整MonoImporter原字节；GUID依次为`d38bb4c70325f46bb9115b83d16b9280`、`31c6100a8d9c5493082e804a5ec19121`，本轮扫共享Assets无同GUID。不是生成新GUID；三旧Host源码的meta均与H.inputs相同且保持原字节。
五目标中若实施时已等于后像，只记ALREADY_PRESENT不重写；与前后像均不符、路径变链接或GUID冲突即停止对应接入，保留并发成果，不自动覆盖。三Host源码作为同一接线单元，预检须全部满足后才落地。

## 3. 已有部分、兼容性与保护对象

- `FileLocalePreferenceStore.cs`已有13773B／`d392d6808ad1fc8156482020951404eb46390d462ce0164404180dea067cce13`；`ILocalePreferenceStore.cs`已有3495B／`e768e6e39fe0f28ec292032ff9986f79606136ad6473b43fb6d5c550487e55a0`，均与候选相同，**源码不写**。
- HostSession仅补允许已归属语言settings的空档判定，保持未知文件／链接拒绝及原创建恢复语义；HostView只接选择保存回调、三态反馈与解绑代次；PlayerHost接首次Load、共享偏好／PlayerSave根、取消／禁用／销毁和Editor隔离lease。
- 当前`FightMatch.Host.asmdef`346B／`85acabf7d4ec5e2b9970f9299ab2687cef131a4fa8213aa4f0aea9f70b4c0c7e`与H相同；Host.Editor、Host.Tests及HostAssemblyInfo也相同。ILocalePreferenceStore与Host在同程序集，Presentation的LocaleId／LocalizationService合同未变；无需增加public API／asmdef引用／包。
- 原三参数`HostView.Bind`保留并转调新四参数重载；现有HostTestFixture仍用三参数。View和PlayerHost的全部SerializeField声明前后完全相同，三脚本GUID保持；无需改序列化绑定或重新装配。
- 六份布局Presentation源码、ResponsiveLayout.meta与`Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab`均已等于H后像；Prefab1322960B／`0064bbee5e0d84dfdfd627310a9abada5093252bb26cf363d0646b38586f19d2`冻结。按uGUI技能仅接回调，不重建UI、不移动Canvas或清驱动记录。
- 较新`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`204772B／`a64fbb31f03c2ec0a5fcd88da12ac4793e4d4c75d190965c6d7f148a4b4ebc27`保留；不得倒退到H的83965B。所有资源候选、Config／Tools／Generated、Packages／ProjectSettings及其它WIP保持原样。
- 共享`Assets/Scenes/FightMatchDemo.unity`仍12900B／`0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd`；本包不接入N11R1场景／marker／临时probe，不把场景门或PR4 P1归入Host接线接收。
- Host继续调用既有`FightMatchStreamingAssetsLoader`（4318B／`5699251053eb8203a9ee41b8915dd3e5b39912f92162e41361ef07cbee17f98b`），不新增或降级资源factory。未来正式资源API接入属于另一包，不是这五叶的前提。

## 4. C签发后的最小步骤与静态验收

本包实施阶段只做离线源码接入；**Unity、Play、测试、场景保存／复开、下载／restore、真实存档访问、Git写均0**。新E=`TestArtifacts/FightMatch/HOST-INTEGRATE-001/S01/`；实施前须缺席，仅准以下证据：`activation.json,inputs.json,before.json,after.json,receipt.json`，`backup/<前三目标相对路径>`和恢复时的`rollback/<五目标相对路径>`。没有新的runner或资源副本。

1. 中央签发后C记录fresh thread/turn、本文及三固定证据身份；只读确认共享R没有Unity/构建消费者，不接触当前M04投影，不关闭用户进程。若共享Editor开着，暂缓落地，避免自动导入／默认Host启动触达真实偏好根；这不是LOC阻塞。
2. 逐项核五目标前／后像、两现有偏好源码、GUID唯一性、保护对象；形成当前`Assets/Packages/ProjectSettings`普通文件路径／大小／SHA清单，覆盖当前WIP而不是历史Git基线。读取三原源码备份并验hash；不读取任何用户存档内容。外部写者未停则不开始。
3. 通过后使用apply_patch将三源改成S对应的精确字节，并补两既有meta；不设计新逻辑，不顺手修别的测试或资源。任何中途不符转下节恢复。全部已相同时可零写交回。
4. 全量比较本次输入前后清单：差量只能是本表尚未采用的子集；五目标最终逐字节等于S，所有非白名单叶与开始时相同。再次核三SerializeField／meta未变、两GUID唯一、旧三参数调用有效、null源和诊断分支保留。静态前后身份及退出码写入after/receipt。
5. C回执分列实际写入、ALREADY_PRESENT、保护检查、原测试／审查引用、编译NOT_RUN及理由、恢复状态；只报`SOURCE_ADOPTED_COMPILE_PENDING`或`BLOCKED`，不自签Demo／玩法ACCEPT。中央唯一接收，不向LOC发送或重试通信。

离线机械操作总预算≤120秒，静态检查最多前检／后检各一次；并发漂移不是重试许可。备份及5个小证据合计≤8MiB，不复制Library／Packages缓存或32叶产品包。

## 5. 必要编译收口与LOC分界

接入已审原字节不需要再派PR16审查／重跑10项。**共享组合仍需一次覆盖实际输入的Unity编译证据**：共享AndroidBuild和测试集不同于H，旧T01的编译不能替代这一门。可把本表五后像并入下一次已获准的共享候选编译，一次完成Host/Host.Editor/现有测试程序集兼容性，不为本包另起重复I/T。
若中央需要独立编译槽，建议唯一C一次Compile≤360秒、无Play／executeMethod／runTests／重试；由中央另签精确投影、真实共享输入清单、自然meta/生成物白名单、argv、监护及恢复。**本文不授权直接对共享R启动Unity**：R还有不在本包范围的缺失测试meta等导入差量，也未复用一个已冻结的共享组合执行合同；不得偷用旧P编译较旧候选来关门。
编译验收须绑定这五后像和当次全部实际共享输入，exit0、无编译错误，证实Host源参与本轮编译或有精确一致的复用证据；如因其它WIP失败，保留原失败并单列原因，不扩大本包修复。编译未跑时可先保留已准确接入源码，中央只接收源码采用状态，不宣称共享工程已编译验证。

正式LOC仍是`new LocalizationService(null, systemLanguage)`：启动先读取偏好，再因LocalizationNotReady阻断并撤销回调／释放隔离lease，Session不开放；因此本包没有可供玩家正常操作的语言选择界面或保存演示。模块接线／meta独立于LOC材料，可先落实；**正式文本、真实按钮选择保存、资源加载后进入玩法及设备冷启动**必须等正式LOC和资源接入，不以草稿CSV、成功源或改IsReady绕过。
未加隔离参数的未来默认Host启动会涉及系统偏好路径；源码复制不会执行该路径。本包不启动默认Host，不创建／读取／修改真实PlayerSave或偏好文件。

## 6. 恢复及收件

五叶接入中断／静态失败时，仅对仍等于本次写入后像的三源恢复已验backup；新增两meta先移入E/rollback原路径后恢复缺席。已有ALREADY_PRESENT不动。出现第三方新值保留并报告，不强行回滚；所有被移走内容可从E恢复，禁止Git reset／checkout或整树恢复。
中央凭C实际完成、五后像／非范围保护证据接源码；待上节编译证据到齐才讨论组合接收。任何必须改动固定产品字节的修复另签最小包并按新head走GitHub，不能借旧审查批准新逻辑。本次writing-for-agents技能将“源码采用”和“组合运行通过”拆成可核验条件，避免把准备或旧证据当作新验收。
