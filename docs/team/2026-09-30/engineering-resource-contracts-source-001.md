# RES-B-CONTRACTS-SOURCE-001

2026-10-03 · READY_AFTER_HOST_FREEZE，源码拆分，不是完整RES-B／加载功能或运行接受。

## 1. 任务与权属

用户已要求继续实际开发；中央本轮明确批准从RES-B拆出已定纯.NET契约、测试及两个asmdef，调整源码顺序而不解除adapter的RES-A硬依赖。复用既有C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`，显式Astra/xhigh；本包须等其HOST-SAVE-ISOLATION源码正式冻结、主程收件后才由主程派发开始。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local` 为唯一实施收件，中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 负责Git发布；C在before记录实际新turn。无新ARCH/SYS设计轮、无外部模型调用。
固定规范为[RES-B prepared-02](engineering-yooasset-res-01b-implementation.md)（SHA256 `c5a5714c5bf1f12e5d4387e7c0fa8232b898eb2578092120e51f3a86b2f4aec5`）§6/6.1/6.2，及§7.1的纯契约程序集；上游[RES设计](engineering-yooasset-001.md) SHA `2e12b5f24ecedef03df5795c9632318c55fe6330562731892d4c9704c54740d8`只读。原B §0/1的A门仅对此四源码路径提前解除；完整B、SDK/provider/URL/handle、C/D/H及运行验证仍未授权。原B provider测试要求不得删除或算作本包通过。

## 2. 唯一新建白名单

普通暂存根 `TestArtifacts/FightMatch/RES-B-CONTRACTS-001/S/` 必须ABSENT；不是Git checkout/Unity项目。仅以下10叶：
- `source/Assets/Scripts/FightMatch/AssetAccess/FightMatch.AssetAccess.asmdef`
- `source/Assets/Scripts/FightMatch/AssetAccess/FightMatchAssetContracts.cs`
- `source/Assets/Tests/EditMode/FightMatchAsset/FightMatch.AssetAccess.Tests.asmdef`
- `source/Assets/Tests/EditMode/FightMatchAsset/FightMatchAssetContractsTests.cs`
- `before.json`、`verify.py`、`source.patch`、`after.json`、`static-checks.json`、`source-receipt.json`。
全部逻辑产品文件当前应ABSENT，重核名称冲突。test asmdef本包只引用FightMatch.AssetAccess和TestAssemblies，Editor-only；future adapter引用由B剩余包加入。生产noEngineReferences，无SDK/Unity依赖。不写meta，不导入，不修改共享Assets或任何已有产品文件。测试fixture同名，namespace `FightMatch.AssetAccess.Tests`；具体非参数化测试名由作者清晰命名，封存精确fullnames，不预填虚假发现数。
本包上限：生产契约≤400物理行，测试≤350物理行（保留总6生产1000行、2测试900行预算余量给真实adapter）；两个asmdef≤50行各。不得挤压为难读单行；确需超额先报。无新增public类型/签名/operation，完整照原B §6 exhaustive inventory；AssetId允许原规范的Equals/GetHashCode/ToString覆盖。不新建provider/lease生产实现、helper文件、下载器或配置。

## 3. 可观察准则

- AssetId只接受原ASCII grammar 1..128，ordinal identity，不trim／fold／normalize，失败out=null且不回显输入；覆盖长度/字符/大小写/Unicode/穿越类负例与相等hash行为。
- Budget四参数遵守原闭区间，min/max接受，各自min−1/max+1抛ArgumentOutOfRangeException，不引入第二套可调上限。
- Diagnostic遵守code/asset/set null规则、原有enum范围及SafeDetail完整canonical grammar；非法detail整体替成reason=redacted、不保留任何片段。覆盖空/null、全部字段与允许值、排序/重复/非ASCII/边界/敏感路径和URL拒绝。
- Accepted/Rejected工厂落实原§6.2一者非null、有效asset/set、ordinal set一致、epoch与诊断一致；invalid raw set不存储，invalid组合抛出，Accepted不得接受released/null lease或无效epoch。测试可有最小test-owned lease数据stub来验证工厂，不实现或模拟资源provider/SDK成功；lease实际句柄/唯一GUID生成/释放诊断属于后续真实adapter，不冒充本包证明。
- IFightMatchAssetProvider、IFightMatchAssetLease仅落已批准接口；无资源获取、取消/下载策略、业务保存、Host/LOC接线；无YooAsset类型外泄。原请求验证顺序属于未来provider，不能用本契约测试声称已满足SDK调用顺序。
- 四逻辑产品后像/patch一致；共享全部产品、Host S全部封存叶及FIX04旧证据保持；代码只宣称SOURCE_READY，编译/测试执行NOT_RUN。

## 4. 离线检查与返程

作者用apply_patch，标准库脚本可机械生成此白名单JSON/patch；不产生pycache。verify.py只读共享文件、写本包已列证据，无subprocess/网络/Unity/编译器。开始冻结本包规范、实际身份、四目标absence；复用FIX04 before的inventoryRoots重枚举当前共享1089文件，并保护HOST-SAVE-ISOLATION-001/S全部已封存8叶及FIX04目录/既有保护清单。只读取已列证据，不扫cache或用户存档；比较前后路径集合/类型/bytes/SHA。若他人变更，停止受影响接收，不覆盖/回滚。
机械验证：4目标清单、文件行数、两asmdef JSON/引用、声明public surface与原清单、测试精确fullnames、unified patch对4个ABSENT前像内存重放等于后像、所有保护零漂移。记录实际命令UTC/exit及失败，NOT_RUN明确编译/测试/Unity/网络/设备。禁止Git分支/worktree/commit/push、meta、Packages/ProjectSettings/Config/Tools/Generated/Host/LOC/FIX04写入及任何旧被拒动作重试。
冻结后final＋source-receipt给主程实际turn/四后像SHA/patch与receipt身份/准则，不要求发送回消息；主程唯一机械收件后交中央作独立GitHub PR Code Review。无local review或重复review agent；未拿到实际新head结果为REVIEW_PENDING，实际运行另签，不把接口完成称为加载功能完成。
