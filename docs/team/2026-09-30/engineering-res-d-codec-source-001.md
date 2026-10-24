# RES-D-CODEC-001：纯.NET pinned boot codec

2026-10-03 · `READY_FOR_SOURCE_DISPATCH`；本轮只签短合同，源码由主程真正派发。

## 1. 身份、输入与范围

授权：AGENTS §6/用户继续开发/中央批准D最小codec及下述public面。主程唯一收件 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a0ff0c-f3dd-7193-9357-e443efed3808`；ARCH裁定turn `01a0ff0e-539b-7ac3-8c6a-3f74789c469e`；SYS `01a0f2e6-29bb-7092-abf0-705b41b7bf93/local/01a0ff13-4323-7261-bd4f-5a4d18420ed9`（thread/host/turn）。
owner `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，`gpt-6-astra/xhigh`。先封存B FIX03，再由主程首次D激活绑定其receipt/清单/实际turn，直接串行转D，可与B M04/GitHub并行。FIX02仅参照：`TestArtifacts/FightMatch/RES-B-ADAPTER-001/FIX02/S/source-receipt.json`＝21078B／`d1be360c76307a7ebe98eb4bb7afc552bb95a30efb98554ea848c4248a822057`；M03历史37/37，FIX03新增1候选共38；PR9 `d8cc01056bfbc28391d5f9ac34a58e17f38ab111`代码门中央接，本稿不审。
依据[RES §12–13](engineering-yooasset-001.md)／`2e12b5f24ecedef03df5795c9632318c55fe6330562731892d4c9704c54740d8`、[CD §3–4](engineering-resource-runtime-bridge-cd-001.md)／`ea3fc6efecd6ea118675cfcf96499cb5c3192bb6af2371d7ca9637cb7bdf9378`。参考`Assets/Scripts/FightMatch/Content/PublishedContentCodec.cs`／`9a2ee42c7a76156aaf66740b7af4e3976342fa6cc408655fb68522bfe53f27a8`的BCL严格UTF8/SHA/有界读写；parser私有，不调用/抽库/加Content引用、friend、依赖或JSON包。
新增仅`Assets/Scripts/FightMatch/AssetAccess/FightMatchResourceReleaseSet.cs`与`Assets/Tests/EditMode/FightMatchAsset/FightMatchResourceReleaseSetTests.cs`（两共享路径/S已核缺席）。纯.NET/noEngine，不改asmdef；测试仅BCL/NUnit，无factory/raw DTO/SDK/Host/UI/build/admission。
public面：`FightMatch.AssetAccess.FightMatchResourceReleaseSet` sealed类；`static bool TryDecodePinned(byte[] bootBytes,string expectedBootSha256,out FightMatchResourceReleaseSet value,out string rejectionCode)`；`byte[] EncodeCanonical()`；get-only依次`int SchemaVersion`及string类型`ReleaseSetId/BusinessReleaseSetId/Platform/DescriptorSha256`。恰五属性，无public构造器；其余模型/读写/常量private。

## 2. 六件业务只读证据

根`Assets/StreamingAssets/FightMatch/`；以下顺序等于`Assets/Scripts/FightMatch/Host/FightMatchStreamingAssetsLoader.cs`／SHA `5699251053eb8203a9ee41b8915dd3e5b39912f92162e41361ef07cbee17f98b`的FileNames，不改任何字节，不把它们当新resource boot。

| 固定name/顺序 | bytes | SHA256 |
| --- | ---: | --- |
| first-release.fmsource.json | 20443 | `fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a` |
| first-release.fmpackage.bytes | 11486 | `b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129` |
| first-release.fmvalidation.bytes | 664 | `512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d` |
| first-release.fmreview.json | 4766 | `16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75` |
| first-release.fmpublish.json | 682 | `feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910` |
| first-release.fmrelease.json | 411 | `03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516` |

真实business set=`release-set:fightmatch-demo-r1`/Scope=`player`/schema=1；fmrelease与fmpublish.Binding：ContentFingerprint=package SHA、NumericContractVersion=`RC01`、PackageId=`package:fightmatch-demo-r1`、RandomContractVersion=`PC01+SC01`、RuleVersion=`demo-r1`；fmrelease.PublicationReceiptSha256=publish SHA。仅本轮输入，不硬编码未来业务准入。

## 3. 精确schema v1（各行列出完整且ordinal有序的键）

记`H`=小写64hex；`I`=1..128 ASCII `[a-z0-9][a-z0-9._-]*`且不为`latest`；`T`=1..256 ASCII `[A-Za-z0-9][A-Za-z0-9._:+-]*`；`P`=1..240 ASCII安全相对路径，段为`[A-Za-z0-9][A-Za-z0-9._-]*`、非`.`/`..`，无空段/首尾斜线/反斜线/百分号/冒号。`N`=正int32，`L`=正int64（仍受§5限额）。`F`是`{length:L,name:P,sha256:H}`。所有字段必填，无null/bool/浮点/可选键；数组无null项。

| 私有结构 | 精确键:type/常量 |
| --- | --- |
| boot | descriptor:Descriptor, descriptorSha256:H, mapping:Mapping, physicalFiles:Physical[], schemaVersion:1 |
| Descriptor | businessContent:Business, codeCompatibility:Code, publication:Publication, releaseSetId:I, resources:Resources, schemaVersion:1, text:Text |
| Business | binding:Binding, businessReleaseSetId:T（必须`release-set:`＋I）, contentReleaseSetSha256:H, files:F[6], publicationReceiptSha256:H |
| Binding | ContentFingerprint:H, NumericContractVersion:T, PackageId:T, RandomContractVersion:T, RuleVersion:T |
| Code | appBuildIdentity:T, baseProtocolVersion:N, requiredCapabilities:T[]（1..32） |
| Text | artifactLength:L, artifactName:`fm-text-v1.json`, artifactSha256:H, manifestLength:L, manifestName:`fm-text-v1.manifest.json`, manifestSha256:H, schemaVersion:N, sourceReceipt:F |
| Resources | manifestLength:L, manifestName:P, manifestSha256:H, mappingDescriptorSha256:H, packageName:`FightMatchMain`, platform:`android/macOS/windows`三选一（macOS大小写精确）, requiredScopeHashes:ScopeHash[], yooAssetPackageVersion:`3.0.6`, yooManifestPackageVersion:I |
| Publication | authorizationEvidenceId:T, operationId:T, receipt:InputReceipt, receiptSha256:H |
| InputReceipt | authorizationEvidenceId:T, businessInputsSha256:H, kind:`resource-inputs-receipt-v1`, operationId:T, schemaVersion:1, sourcePlanSha256:H, textSourceReceiptSha256:H |
| Mapping | entries:Entry[]（8..256）, schemaVersion:1 |
| Entry | assetId:I, contentLength:L, contentSha256:H, files:P[]（1..64）, kind:`raw/object`, location:P, packageName:`FightMatchMain`, platform:同Resources枚举, releaseSetId:I, scopeId:Scope, unityType:string |
| Physical | kind:`raw/bundle/manifest/receipt`, length:L, name:P, sha256:H；数组10..512项 |
| ScopeHash | scopeId:Scope, sha256:H；Scope仅`fm.content.first-release/fm.text.full/fm.ui.runtime/fm.image.runtime/fm.audio.runtime`五值 |

斜线枚举非字面值。raw.unityType为空串；object为≤128的ASCII点分类型名（至少两段`[A-Za-z_][A-Za-z0-9_]*`），不加载类型。outer I与含冒号business槽分离；mapping set/platform/package等于descriptor。实际类型/protocol/capability支持后验。

## 4. 闭合引用与单向hash

- `H(x)`以下专指SHA256(x的canonical UTF8字节)。boot.descriptorSha256=`H(descriptor)`；resources.mappingDescriptorSha256=`H(mapping)`；独立expectedBootSha256哈希完整boot，boot内无自身pin/hash字段。顺序为输入凭据/文件/mapping→descriptor→boot→Player pin→最终build receipt。
- publication.receiptSha256=`H(receipt)`；嵌入的前置receipt绑定制作前Plan SHA、text.sourceReceipt.sha256和`H(businessContent.files)`，operationId/authorizationEvidenceId等于外层。其封闭schema无descriptor/mapping/boot/最终回执hash，不循环；仅输入声明，非审批/签名证明。
- 六raw assetId按表序为前缀`fm.content.`＋`source/package/validation/review/publication/release-set`，scope=`fm.content.first-release`，各contentLength/SHA等于F。business的release/publish/Fingerprint分别等于第6/5/2 SHA。text scope恰`fm.text.full.artifact`/`fm.text.full.manifest`两raw，对应Text两组length/SHA；其它scope只object。不得缺/多/串槽；同包location唯一，可不同于物理name。
- Entry.files是物理name引用，严格有序无重复且全存在。raw恰一个kind=raw物理叶，contentLength/SHA与其一致；object仅引用bundle，contentLength=checked长度和、contentSha256=`H(按name排序的完整Physical记录数组)`。Resources.manifest三元匹配唯一kind=manifest叶；Text.sourceReceipt匹配唯一kind=receipt叶。每个Physical至少被Entry或这两个直接引用一次；共享bundle可多引用，无悬空/孤儿。物理清单不含boot/pin/最终回执；任何路径末段为`fm-resource-boot-v1.json`、`player-boot-pin.sha256`、`resource-build-receipt-v1.json`都拒绝，producer也不得以其它名字偷塞这三类产物。
- requiredScopeHashes恰覆盖全部非空scope且含两必需scope，按scopeId排序。hash输入=`{assets:[该scope完整Entry按assetId排序],files:[引用Physical去重按name排序],scopeId:s}`的canonical字节，无自身hash；manifest/receipt另由descriptor覆盖。物理正文/依赖/对象/LOC/内部业务语义均待build/admission核验。

## 5. Canonical、安全返回与固定预算

Canonical：严格UTF8无BOM（`new UTF8Encoding(false,true)`），无空白；键ordinal，拒重/未知/缺键。仅quote/backslash转义为`\"`/`\\`、控制码小写`\u00xx`；其它Unicode直接UTF8，字段ASCII限制照旧；拒坏UTF8/孤立surrogate。整数仅`[1-9][0-9]*`，拒sign/前导0/小数/exponent。entries按assetId、physical按name、capabilities/引用集合按ordinal、ScopeHash按scopeId递增；六件按§2。id/name/location重复和路径大小写别名拒绝。重编码须逐byte等于快照，无等价非canonical/trailing。
硬预算：boot/descriptor/mapping＝262144/32768/131072B；depth≤12（根=1），nodes≤16384（容器/标量/key各1），object≤16键，array≤512；string≤256 UTF16 units且1024 UTF8B、key≤64 units、number≤10位，集合另沿§3。Physical每叶≤268435456B、去重总和≤1073741824B；业务每叶≤16777216B/总≤33554432B；Text三文件各≤16777216B。先限长再clone；byte游标在进入descriptor/mapping时即计段预算，**不得构树后才验限**；节点/集合/字符串/输出先扣额再分配/遍历，数字checked解析/求和并先核剩余。无自报预算/无界扩容或Substring/反复全表搜索。
顺序/闭集码：null/空→`RES_SCHEMA`，超长→`RES_BUDGET`；pin非H或快照hash不符→`RES_TRUST`；parse/shape/canonical/引用或长度关系错误→`RES_SCHEMA`，预算/数字超限→`RES_BUDGET`，内部SHA不等→`RES_HASH`。不回显输入/exception；false/null value/code，或true/完整不可变value/null code。一次自有快照供hash/parse/canonical；私有保存canonical bytes，Encode每次clone。无public可变集合，不吞致命异常。
pin独立传入，codec不证明来源，不信boot/旁置下载物。并发写入不承诺原子快照，但成功只对应同一自有快照；后续输入/输出变异不改value。真实Player pin/签名/能力相容性另验。

## 6. 固定测试、源码封存及后续执行

Class=`FightMatch.AssetAccess.Tests.FightMatchResourceReleaseSetTests`，12个普通非参数化`[Test]`：`D01_CanonicalVectorRoundTrips`、`D02_ExternalPinIsRequired`、`D03_MatchingPinDoesNotEstablishAuthority`、`D04_ExactKeysAndTrailingAreRejected`、`D05_EncodingAndIntegerGrammarAreStrict`、`D06_ByteDepthNodeEntryStringBudgetsHold`、`D07_PhysicalLengthsAndTotalsAreBounded`、`D08_AllNestedHashesAreBound`、`D09_PhysicalAndScopeReferencesAreClosed`、`D10_BusinessAndTextSlotsStayDistinct`、`D11_SetPlatformKindAndOrderingAreExact`、`D12_InputAndOutputCopiesAreIsolated`。12为计划，非discovered/passed。
固定V0：schema均1，outer=`fm.codec.r1`，business/Binding/六文件元数据用§2，platform=`android`，protocol=1/build=`codec-v1`/capabilities=[`resource-schema-v1`]，SDK manifest version=`codec-v1`；raw location依次为六文件名及两text名，physical依次`raw/b1`..`raw/b6`、`raw/text`、`raw/text-manifest`；text/artifact、text/manifest、text/sourceReceipt、SDKmanifest、Plan的合成正文分别ASCII `x/y/z/m/p`（前三meta名沿§3，receipt名`inputs/text.receipt`、SDK manifest名`manifest/main.bytes`），operation=`build:codec:1`、authorization=`local-test-only`。V1加scope `fm.ui.runtime` 的`fm.ui.root` object/type=`UnityEngine.GameObject`/location=`ui/root`，物理`bundles/ui`正文ASCII `b`。按§4自底向上生成完整JSON与所有预期SHA，测试固定字面量；verify.py用独立BCL外的标准库算法复算，不用待测encoder生成expected。合成值不冒称真实内容admission。
负例逐门变异（重算外pin/上层hash以命中目标）：pin null/空/错/自报；键重/缺/未知；BOM/坏UTF8/surrogate/非canonical/trailing；负/0/前导0/exponent/溢出；预算0/limit±1、不可合法抵达边界须受控拒绝；各hash/receipt/六件/text错配、悬空/孤儿/别名/重复、set/platform/kind/order；输入及两输出互改隔离。D03证明自选匹配pin仅为一致性，非授权。
S=`TestArtifacts/FightMatch/RES-D-CODEC-001/S/`仅8叶：`source/`下两源码＋`before.json/after.json/source.patch/verify.py/static-checks.json/source-receipt.json`六证据，不复制基线/SDK/工程。生产≤1200行/80KiB，测试≤1000行/100KiB，S≤2MiB；超限先报、不压行。before绑定实际激活/封存FIX03清单、本文/六文件/参照hash、两目标/meta缺席及共享只读源/meta；after同组核对。**不读/冻结活投影或在写M04**，不把C合法运行判为漂移；既有B源/测试（候选38）不动。
作者用apply_patch；机械证据只写8叶。`python3 -B TestArtifacts/FightMatch/RES-D-CODEC-001/S/verify.py`≤30秒/最多3次留失败；仅标准库检查闭包/hash/行数/public面/无依赖、两路径patch重放/fixture独立hash/无外写。无Unity/C#/测试/discovery/Git/网络/meta；checkpoint为SOURCE_READY/UNCOMPILED/UNTESTED/REVIEW_PENDING或BLOCKED。共享写单owner/C串行，FIX04/M16旧拒绝不绕过。
主程wait收final身份/hash/准则/限制；另绑C单次compile＋新codec class-filter，复用B证据、不重跑37/38/全量，dots优先/本地例外单签。新head GitHub审查及定向验证后接收，作者不自批。真实build/LOC/admission/Player pin/Host设备NOT RUN；扩public/文件/决策先报delta。本轮仅新增本文，无S/派工/产品/运行/Git。
