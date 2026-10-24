# LOC-LIC-ALT-M-PREP-01 — M 材料取得／闭包冻结签包准备

日期：2026-10-01（Asia/Shanghai）
状态：**DRAFT_WAITING_ARCH_ACCEPT**

本文件只整理未来 M（material／closure freeze）实现包需要签死的输入、路径、manifest
schema、预算字段和停止合同。它不是 M packet、不是 `M-ACCEPTED`，也不授权下载、
restore、build、publish、test、Luban、`005`、Unity、生产文件修改、外盘 cache/source 写入、
evidence 创建或 Git 操作。

准备 owner：LOC-A implementation owner，thread/host
`01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。
回传：工程主程 thread/host
`01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
未来 M delivery owner、实际 turn/message、独立 R receipt 必须由获准 M packet 重新固定；
本准备 owner 不自动成为 M 执行 owner。

## 1. authority 与当前门禁

| 输入 | 当前固定身份 | 本准备的处理 |
| --- | --- | --- |
| system design | `engineering-localization-license-alternative-system-design.md`；SHA-256 `18be0271c405bd33444fa5a5b4aa248114eef8a8e4442f5b40fdf840dfcdeb37`；881 lines / 59,206 B | 作为 accepted-for-review 固定输入；不等于 M 执行授权 |
| architecture dispatch preimage | SHA-256 `35afce4785af87f5cf0e562117a0ab2cc1dbe7429af06c8dbe17c4a82120d1ff` | 主程下发时仍在第三次 P2 最小修订，不得作为 accepted authority |
| architecture 当前只读观察 | `engineering-localization-license-alternative-architecture.md` 仍由 architecture owner 修订，文件状态 `PREPARED_NOT_AUTHORIZED`；本准备期间其 bytes/SHA 继续变化 | 不冻结并发草案；没有同一 R 的新 `ACCEPT` receipt，故本文保持 `DRAFT_WAITING_ARCH_ACCEPT` |
| license research | `research-loc-a-license-sources.md`；SHA-256 `5fa2b799a17484df59cf1ad1216edf07a007dd7ea49f40794849c35e97e00550`；170 lines / 11,681 B | 固定研究输入 |
| unique route | official Luban tag `v5.1.0` / commit `df34215d2e22035073d1af83d44e3ac0744dc3b3`，仅把两个 csproj 的 `YamlDotNet.NetCore 1.0.0` 替换为 `YamlDotNet 16.3.0` | 唯一路线；不得试第二版本、删 YAML、加 shim 或改 `.cs` |

签发 M 前必须收到同一独立 R 对最终 ARCH bytes 的唯一 `ACCEPT`，并把 accepted ARCH 的
path、SHA-256、lines、bytes、R thread/host/turn/message 和 verdict 原样写入 M packet。
当前缺少这些字段，因此本文件不能被改称 `AUTHORIZED`、`M-ACCEPTED` 或 M 执行包。

## 2. 现有材料只读盘点

本节是 2026-10-01 的只读观察。目录的 aggregate identity 使用已存在的 Git tree 或已接收
manifest；没有为盘点生成新 package graph、lock、cache 或证据。

### 2.1 official Luban source tree

source root：
`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/source/`

| 项 | 实测值 | 状态 |
| --- | --- | --- |
| Git remote | `https://github.com/focus-creative-games/luban.git` | present |
| tag / HEAD | `v5.1.0` / `df34215d2e22035073d1af83d44e3ac0744dc3b3` | exact |
| Git tree | `a8f8243ef84b2d11156f869af76061863ffb6d6b` | exact |
| tracked files / bytes | 931 / 5,675,900 B | exact |
| tracked worktree manifest SHA-256 | `02a03381e7dd14ba5d775ff198812ba3811934a0c103291d30deb858704c8ce5` | preparation-only aggregate; M must regenerate in its new root |
| `git status --porcelain` lines | `0` | clean at observation time |
| source archive receipt | none in fixed cache | **missing; M acquisition must freeze exact source transport or prove copy from this clean commit-bound tree** |

必须固定的 project preimages：

| absolute path | bytes | SHA-256 | 状态 |
| --- | ---: | --- | --- |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/source/src/Luban/Luban.csproj` | 4,201 | `5a29bc8a18a2eae222d412ac4f2485ae366ffd7146cc2c1bf397e5554dde30c9` | present；未来只允许一处 PackageReference replacement |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/source/src/Luban.DataLoader.Builtin/Luban.DataLoader.Builtin.csproj` | 1,811 | `0f84ba758efa3ec4b8f878abf3b0429f34cc23bbf9a01f81c164878658839516` | present；未来只允许一处 PackageReference replacement |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/source/src/Luban.DataTarget.Builtin/Luban.DataTarget.Builtin.csproj` | 1,669 | `935771a2b429ec8c0490cf88f022d8c31d5d2af4a73ad76fe627fe5f814a2dc8` | present；必须不变 |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/source/src/Luban.Tests/Luban.Tests.csproj` | 2,542 | `d3fc132b9ae2a3881c142d9d88d0c1ce6ab01e148dcc087dfddb634261ebb2f1` | present；future test closure authority |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/source/LICENSE` | 1,088 | `a7bfa87431fde61f7408e30cd6ce57fe676638c651f5c04dc90fa3de43d28841` | present；Luban MIT |

### 2.2 accepted runner、cache 与旧 evidence

| absolute path | bytes / count | SHA-256 | 状态与用途 |
| --- | ---: | --- | --- |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/runner/` | 132 files / 5,969,118 B | manifest `d9baacda3afc7fee9db4d92cd4199587fa5b9f1b7e4601be85028644170f2bd5` | present；只读 old oracle，不是 candidate identity |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/runner/Luban.dll` | 20,992 B | `6e60beee2b1e8e2870278e6ec363b4dc516311a6f2711bf3518b5edae056df7e` | present；只读 old oracle |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/identity/packages.lock.json` | 14,103 B | `3231b50eccd462dd529fb3d0e67c44d62ca8c3363af8367e086ed6b5f5081194` | present；旧 graph 对照，不得改写为 candidate lock |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/identity/runner-files.tsv` | — | — | **missing**；accepted manifest 的本地权威副本位于 005 evidence |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/identity/runner-files.tsv` | 12,674 B | `d9baacda3afc7fee9db4d92cd4199587fa5b9f1b7e4601be85028644170f2bd5` | present；永久只读 |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/tool-identity.json` | 910 B | `95b318ca60569d2b1e02335f65574d6a355482286eec5dce0ffa5f6506158a43` | present；永久只读 |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/input-hashes.json` | 1,007 B | `4af8ac9cbbdaf0b54da1f67ec6a499d5bb8949a6d2400d56a74425917ee4a83b` | present；永久只读 |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/commands.json` | 51,338 B | `f098f03407019eae78340c8240a59ea5e8f20fe0de1a442f8101d597397a8688` | present；只可用于 M 分析 harness DAG，不能重跑 |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-02/inputs.json` | 2,961 B | `c191f93674f74199434c39a2c31b8a34e08e6a3925dca39dcec5d64a35af3582` | present；永久只读 |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-02/third-party-license-inventory.json` | 78,477 B | `1bfeb05a91d247738b7ef36b5bdbb86b2526e6868ed5737b791f1288b23a352f` | present；历史 `PASS` 含 forbidden nuspec fallback，只作 gap 输入 |
| `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-02/commands.json` | 38,972 B | `803a5f686d5999c8b7bd515227fa7cd39b6703f553ee6e6fe30aff008e52cb85` | present；只可用于 M 分析 LOC-A harness DAG，不能重跑 |

三棵 LOC-A 历史 evidence roots 必须永久只读：

1. `.../ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf/`
   — 37 files；run `e31cbc302d357dcc0135482e284afd8816e6bbb700a6bb05c8306adfd1c93420`。
2. `.../ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-01/`
   — 15 files；run `19151d7f0cd9a2bfeaeddf090d267a35941722ba1cc9a163bba229c4cabc8dc4`。
3. `.../ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-02/`
   — 74 files；run `43c4f7d9cf78194cd97b6a2b277c40a4bde9be38ad6f359e133dab17c25e3110`。

未来 M 必须在执行前后保存 old roots 与 old cache 的递归 path/bytes/SHA manifest 并做
集合等值；它不能向这些 roots 添加 correction 文件。

### 2.3 FIX-02 已部署 package archives

以下 13 个 distinct nupkg 已由 FIX-02 inventory 绑定到 deployed runner files。它们是旧
runner 的只读材料观察，不等于 future candidate 的完整 runner/test closure，也不授权复制：

| package | absolute nupkg path | bytes | SHA-256 |
| --- | --- | ---: | --- |
| CommandLineParser 2.9.1 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/nuget/commandlineparser/2.9.1/commandlineparser.2.9.1.nupkg` | 496,069 | `02953dcb5c97eb475a4a33e4dca60305e27b5d9072296f21b1ef4d45919c8dea` |
| ExcelDataReader 3.7.0 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/nuget/exceldatareader/3.7.0/exceldatareader.3.7.0.nupkg` | 416,696 | `7222a0a533a495f3f596334a8a98f38f974394e1f75cc2922af11e4e15c22834` |
| Google.Protobuf 3.29.0 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/nuget/google.protobuf/3.29.0/google.protobuf.3.29.0.nupkg` | 1,385,488 | `b992643b3bdf6657751a7d7957bb7cba89bf159a2b156949510e93812db18b93` |
| MessagePack.Annotations 3.1.7 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/nuget/messagepack.annotations/3.1.7/messagepack.annotations.3.1.7.nupkg` | 29,820 | `f49fd5c8acc1b04931568438240c0a55fefaa9e04e4981d76eaffd2ddf26ccdc` |
| MessagePack 3.1.7 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/nuget/messagepack/3.1.7/messagepack.3.1.7.nupkg` | 947,679 | `a72dd7204ba2de829ec07d4caacf4b7c2fb6294dd081f8801f9045de4077e052` |
| Microsoft.NET.StringTools 17.11.4 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/nuget/microsoft.net.stringtools/17.11.4/microsoft.net.stringtools.17.11.4.nupkg` | 180,598 | `9567f3637e5643e88a4bd4e9bb3b434e58e017d088391845845126d29d5d5411` |
| NeoLua 1.3.14 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/nuget/neolua/1.3.14/neolua.1.3.14.nupkg` | 1,112,125 | `af8282ab27f9dafaf239558f882a42d65de2fd91bdca025ad4fc999c477d0269` |
| Newtonsoft.Json.Bson 1.0.3 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/nuget/newtonsoft.json.bson/1.0.3/newtonsoft.json.bson.1.0.3.nupkg` | 240,753 | `f05c52751b32c6800b3a4f14cbe1f37e220942afa1244362b5bc8b1b7786550b` |
| Newtonsoft.Json 13.0.1 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/nuget/newtonsoft.json/13.0.1/newtonsoft.json.13.0.1.nupkg` | 2,065,787 | `2b6b52556e27e1b7913f33eedeb95568110c746bd64afff74357f1683878323a` |
| NLog 5.3.4 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/nuget/nlog/5.3.4/nlog.5.3.4.nupkg` | 2,962,051 | `0b0af55aef556ce711cf719d54a92dee52293700b51381dd6f483ea84904af79` |
| Scriban 7.2.5 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/nuget/scriban/7.2.5/scriban.7.2.5.nupkg` | 827,831 | `13500f59e5fb2128692229f58ed2a129cd82de799ca093bf4fd13ced23b520ac` |
| Ude.NetStandard 1.2.0 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/nuget/ude.netstandard/1.2.0/ude.netstandard.1.2.0.nupkg` | 389,990 | `f43abb504e4e8aebdc89445057c01ad777a5a25b757f30e1bc5f1cf18c739e1c` |
| YamlDotNet.NetCore 1.0.0 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/nuget/yamldotnet.netcore/1.0.0/yamldotnet.netcore.1.0.0.nupkg` | 81,052 | `c88b1a0c04e38dc813c373f9775a12370f6921abdd639ee8a1e9da56b0a3ae97` |

旧 `YamlDotNet.NetCore` 只能保留为 negative/history；future candidate feed、lock、source
patch 和 expected deployed mapping 中必须为零。

## 3. 未来 create-once roots 与阶段责任

下列三个候选根当前实测均不存在；“不存在”只是 create-once 前置状态，不是创建授权：

| 未来用途 | exact root | 当前状态 |
| --- | --- | --- |
| M temp | `/private/tmp/fightmatch-loc-lic-alt-m01/loc-lic-alt-yamldotnet-16.3.0-m01/` | `ABSENT_CREATE_ONCE_READY` |
| M material | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m01/` | `ABSENT_CREATE_ONCE_READY` |
| M evidence | `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m01/` | `ABSENT_CREATE_ONCE_READY` |

未来 M packet 只有在最终 accepted ARCH 仍认可 `m01` 时才能采用这些 exact literals；若任一
root 在执行前已存在，必须 `BLOCKED_ROOT_EXISTS`，不得 merge、清空、覆盖、复用或改写。
技术失败和 R `REJECT` 都原地保留：material 标记 `REJECTED_NOT_ACCEPTED`，evidence 标记
`FAILED_EVIDENCE`；下一版只能由新 packet 使用新的 sibling suffix。

阶段责任与 return path：

| 阶段 | owner 必须由谁固定 | 允许消费 | 必须回传 | 不得承担 |
| --- | --- | --- | --- | --- |
| M acquisition / closure freeze | 工程主程在 accepted ARCH 后签出的单一 M packet；记录 delivery owner thread/host/turn | accepted ARCH/SYS/research、现有只读 source/cache/evidence、packet 逐项授权的 official URLs | 工程主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`；材料 receipts、blockers、actual roots | candidate build/publish、`Program.cs`、Unity、B/L verdict |
| independent M R | 测试主程／同一独立 R 的新审查 dispatch | 封存的 M evidence + material bytes | 工程主程；唯一 `ACCEPT` / `NEEDS_FIX` / `REJECT` | 改写 M roots、执行 B |
| B | 仅在 `M-ACCEPTED` 后由主程另签 | accepted M material，old runner oracle | 工程主程与独立 R | 补 M 材料、改 M manifests |
| L | 仅在 `B-ACCEPTED` 后由主程另签 | accepted M/B identities | 工程主程与独立实现 R | runner restore/build/publish |

## 4. future M manifest schemas

M packet 必须要求以下文件全部为 UTF-8、LF、header + deterministic ordinal rows；路径均为
对应 material root 的相对 POSIX path，不允许绝对 host path、glob、空泛“其余依赖”或
目录存在即通过。所有 SHA 使用 64 位小写 SHA-256；缺失字段不能写 `TBD` 后继续接收，
必须触发第 8 节的 blocker。

| future file | exact columns / top-level fields |
| --- | --- |
| `identity/material-files.tsv` | `relativePath, bytes, sha256, kind, acquisitionClass, authorityId, sourceReceiptId` |
| `identity/upstream-source.tsv` | `component, versionOrTag, repositoryUrl, fullCommit, treeId, transportUrl, transportBytes, transportSha256, localRoot, fileManifestSha256` |
| `identity/source-files.tsv` | `component, relativePath, bytes, sha256, gitObjectId, role` |
| `patch/patch.diff` + `identity/patch.tsv` | TSV columns `patchId, targetRelativePath, preBytes, preSha256, postBytes, postSha256, oldPackageId, oldVersion, newPackageId, newVersion, changedLineCount, patchSha256`；只能两行 PackageReference replacement、两个 target rows |
| `identity/package-files.tsv` | `graph, packageId, version, packageRelativePath, bytes, sha256, catalogSha512, catalogUrl, immutablePackageUrl, owner, signatureRelativePath, signatureSha256, signatureVerifier, signatureVerdict` |
| `identity/package-entries.tsv` | `graph, packageId, version, entryPath, assetKind, targetFramework, bytes, sha256, selected, expectedDeploymentPath` |
| `identity/package-closure.tsv` | `graph, parentId, parentVersion, packageId, version, dependencyKind, targetFramework, includeAssets, privateAssets, lockEdge` |
| `identity/license-files.tsv` | `component, version, expression, sourceUrl, sourceTag, sourceCommit, sourcePath, localRelativePath, bytes, sha256, packageRelationshipEvidence` |
| `identity/expected-deployment.tsv` | `packageId, version, packageEntryPath, packageEntryBytes, packageEntrySha256, expectedRunnerRelativePath, dependencyKind, licenseRelativePath, licenseSha256` |
| `identity/official-test-project.tsv` | `projectRelativePath, bytes, sha256, targetFramework, referenceKind, referenceId, versionOrPath, includeAssets, privateAssets` |
| `identity/runner.packages.lock.json` | NuGet lock schema；candidate runner closure only；`YamlDotNet 16.3.0` direct in both projects；old package/old-only System edges zero |
| `identity/tests.packages.lock.json` | NuGet lock schema；五 direct seeds + all test transitives/assets，独立于 runner lock |
| `identity/commands.json` | `schemaVersion, commands[]`；每项 `id, stage, purpose, executableAbsolutePath, executableSha256, argv, cwd, env, stdinPolicy, networkPolicy, expectedExit, timeoutMs, killGraceMs, stdoutMaxBytes, stderrMaxBytes, reads, creates` |
| `identity/process-budget.json` | `schemaVersion, maxTopLevelCommandCount, maxTopLevelProcessTreeCount, maxConcurrentTopLevelTrees, wallClockMs, commands[]`；每 command 含 `commandId, maxStarts, maxConcurrent, children[]`；每 child 含 `executableAbsolutePath, executableSha256, maxStarts, maxConcurrent, parentExecutable, purpose` |
| `identity/temp-budget.json` | `schemaVersion, roots[]`；每 root 含 `absolutePath, createOnce, maxBytes, maxFiles, freeSpaceFloorBytes, logCapBytes, ownerStage` |
| `identity/network-requests.json` | `schemaVersion, requests[]`；每项 `requestId, method, exactUrl, officialAuthority, maxRedirects, allowedRedirectHosts, expectedContentBytes, maxTransferBytes, expectedSha256, destinationRelativePath, timeoutMs` |
| `identity/receipts.json` | `schemaVersion, authority, owner, roots, beforeManifests, acquisitionReceipts, closureReceipts, blockers, finalStatus`；`finalStatus` 只能 `CANDIDATE_NOT_ACCEPTED` 或失败状态，不能由作者写 `M-ACCEPTED` |

M packet 必须逐项列出 manifest 文件本身的 expected path，并在封存时报告 bytes/SHA。
R 以这些 manifest 的集合等值和原始 bytes 为准，不接受只报目录 hash 的摘要。

## 5. acquisition matrix

“固定值”只抄自 SYS／ARCH／research；“M 实测冻结”栏中的未知量不能在签包前猜测。

### 5.1 Luban、YamlDotNet 与 NeoLua

| component | 已知 authority / local state | future M acquisition 或固定要求 | 当前 blocker |
| --- | --- | --- | --- |
| official Luban `v5.1.0` | repository `https://github.com/focus-creative-games/luban.git`；commit `df34215d2e22035073d1af83d44e3ac0744dc3b3`；local clean tree present；tree `a8f8243e…b6b`；license `a7bfa874…841` | 冻结 source transport/copy receipt、931-file source manifest、两个 csproj preimages、test project、Luban license；不得写旧 source tree | exact M transport request/count/bytes 或 approved offline-copy argv 未签；M owner/authority 未签 |
| `YamlDotNet 16.3.0` nupkg | immutable URL `https://api.nuget.org/v3-flatcontainer/yamldotnet/16.3.0/yamldotnet.16.3.0.nupkg`；776,880 B；SHA-256 `e068bcc1243c46c8bfdfe2f27a026bfff03cde7c67d9f37c2cdd70bd24a9dfd4`；SHA-512 `f102fb418c3afe5ec0cc5d0336b9ac6517af6f30603ab5f925a7b3622544c419bb935d1e55901147fff5a113874e55491534d5553411fb4ea198e6550c0421df`；local **missing** | 若 M 获准联网，按 exact URL 取得到新 temp/material roots；重算 hash/bytes；冻结 catalog/nuspec/entries；不得落入旧 cache | download request/redirect/transfer budget 未签；actual local receipt absent |
| YamlDotNet source | tag `v16.3.0`；commit `ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73`；source archive URL `https://codeload.github.com/aaubry/YamlDotNet/tar.gz/refs/tags/v16.3.0`；研究 capture 397,462 B / SHA-256 `4f73f08a0584af4e1cac36ccb0e3cb9d9cd982209b06f7eb10dd5e31badb1bce`；local **missing** | commit/tree 是 authority；M 重录实际 archive bytes/hash，验证 tag→commit；GitHub-generated archive hash 不得被假设永远固定 | actual M archive receipt/tree manifest absent |
| YamlDotNet package signature | nupkg entry `.signature.p7s`；SHA-256 `ba959936ca152b03068114633994e331603c10079631cbceae2e38035c346308` | 使用 fixed SDK/NuGet verifier；冻结 verifier absolute path/SHA、argv、trust input、stdout/stderr/exit/verdict | “entry present”不是 verified；verifier command/process/network policy 未签 |
| YamlDotNet selected DLL | `lib/net8.0/YamlDotNet.dll`；293,376 B；SHA-256 `86f873f682c619a0c24e263dcdd15fe027104666641c92f503516671d61edda6` | 从 exact nupkg 逐 entry 复算；写 package-entry mapping | actual M extraction/entry receipt absent |
| YamlDotNet licenses | exact commit `LICENSE.txt` URL `https://github.com/aaubry/YamlDotNet/blob/ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73/LICENSE.txt`，1,110 B / `daea8b7f1b2bab48ea12ad50b0ce91634669d99d7dd0cb64aed8dffeeef69ed3`；`LICENSE-libyaml` URL `https://github.com/aaubry/YamlDotNet/blob/ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73/LICENSE-libyaml`，1,077 B / `cb65f46df0d9125ba60862117200ec6631fef3a8169cd0b4513ffcad4ef5cb9b` | 两份均保存原始 bytes，绑定 package nuspec repository commit、source commit 和 local filenames | local license bytes absent |
| NeoLua 1.3.14 nupkg | immutable URL `https://api.nuget.org/v3-flatcontainer/neolua/1.3.14/neolua.1.3.14.nupkg`；local present 1,112,125 B / `af8282ab27f9dafaf239558f882a42d65de2fd91bdca025ad4fc999c477d0269` | 只读复制到新 material feed 或按 packet 取得同 bytes；冻结 catalog SHA-512、entry mapping、package metadata | M copy/acquisition argv 与 request budget未签 |
| NeoLua source/license | tag `v1.3.14`；commit `cb560afef9dd19b7afad6da594b44909c9739698`；license URL `https://github.com/neolithos/neolua/blob/cb560afef9dd19b7afad6da594b44909c9739698/LICENSE.md`；Apache-2.0；11,324 B / `a8ad31b1c3f40dca5a84119351b8fa8ddc868edd77fad8a8ebf6d8f2d16fa4ae`；local license/source **missing** | 冻结 tag/commit/source build relationship、license bytes、无 NOTICE receipt；不声称 nupkg cryptographically embeds commit | actual local source/license receipt absent |

### 5.2 六个 explicit license gaps

六项 nupkg 已在旧 cache 中存在，准确 package path/bytes/SHA 见第 2.3 节。旧 FIX-02
inventory 仅以 `.nuspec` 充当 license fallback；M 禁止沿用。以下字段全部必须由 M 从
official upstream 实测并冻结，当前不得给虚假 URL、commit 或 license hash：

| gap | version | M 必须填满的 exact fields | 当前状态 |
| --- | ---: | --- | --- |
| ExcelDataReader | 3.7.0 | official source URL；tag/full commit；license source path；expression；raw bytes；SHA-256；package/deployed binding | `BLOCKED_LICENSE_MATERIAL` |
| Google.Protobuf | 3.29.0 | 同上 | `BLOCKED_LICENSE_MATERIAL` |
| MessagePack | 3.1.7 | 同上 | `BLOCKED_LICENSE_MATERIAL` |
| MessagePack.Annotations | 3.1.7 | 同上；另需 exact project/package relationship，不能默认为复用 MessagePack license | `BLOCKED_LICENSE_RELATIONSHIP` |
| NLog | 5.3.4 | 同上 | `BLOCKED_LICENSE_MATERIAL` |
| Scriban | 7.2.5 | 同上 | `BLOCKED_LICENSE_MATERIAL` |

### 5.3 五个 official-test direct seeds

exact `Luban.Tests.csproj` 固定的 direct references 是：

| seed | version | source semantics | local nupkg state |
| --- | ---: | --- | --- |
| Microsoft.NET.Test.Sdk | 17.11.1 | direct test SDK seed | **missing** |
| Newtonsoft.Json | 13.0.3 | test seed；不得用 runner closure 的 13.0.1 代替 | **missing** |
| System.Text.Encoding.CodePages | 8.0.0 | direct test seed | **missing** |
| xunit | 2.9.2 | direct test framework seed | **missing** |
| xunit.runner.visualstudio | 2.8.2 | `IncludeAssets=runtime; build; native; contentfiles; analyzers; buildtransitive`；`PrivateAssets=all` | **missing** |

五项只是 seeds。M 必须冻结完整 test graph 的每个 transitive、analyzer、build、
buildTransitive、native、contentfiles、testhost、runtime asset、nupkg/source/license bytes、
lock edge 和 selected asset。当前 source 或旧 cache 中没有可接收的 candidate test lock，
以下文件都不存在：

- `.../identity/runner.packages.lock.json`；
- `.../identity/tests.packages.lock.json`；
- 以上五个 exact-version nupkg。

同理，runner 的 finite `expected package → expected deployed path/entry → license` mapping
必须由 M 依据 candidate lock、逐项 nupkg entries、source/build rules 和有限 expected
deployment list 冻结。B 只枚举两个 actual publish outputs，并对 M mapping 做集合等值与
exactly-once 核验；B 不得现场补包、补 license、反推 expected mapping 或回写 M。

## 6. future M acquisition 与 closure-only probe 边界

未来获准 M packet 必须把动作分成两个显式 phase；每个 phase 有自己的命令、网络、进程、
temp、log 和失败 receipt。

### 6.1 acquisition phase（仅在 packet 明确授权联网时）

可允许的动作只有：

1. 从 packet 的 `network-requests.json` exact official URLs 取得 Luban/YamlDotNet/NeoLua、
   六个 license gaps、五个 direct seeds及其解析出的闭包所需 package/source/license bytes；
2. 对每次请求保存 URL、redirect chain、response bytes、content hash、exit、timing 和目标；
3. 把材料写入全新 M temp root，全部 identity gate 通过后 create-once 复制到 material root；
4. 验证 package signature、catalog/nuspec/source commit/license 和 local bytes 的关系。

未列入 exact request manifest 的 URL、redirect host、额外 version 或 fallback source 均
`BLOCKED_NETWORK_DRIFT`。当前准备没有授权任何请求。

### 6.2 closure-only probe phase

M packet 可以在取得材料后另行授权只产生 closure/lock/process receipts、不产生 candidate
runner 的 probe。若采用 restore 型 closure probe，必须固定 exact project copy、isolated
package root、local feed、lock output、argv、child DAG、network policy 和全部输出；它不能
把 build/publish/test 隐藏进 restore，也不能读取 future runner output。

M 的不可变执行零值：

- candidate `dotnet build`：`0`；
- candidate `dotnet publish`：`0`；
- Luban/`005`/LOC-A production invocation：`0`；
- `Program.cs` writes：`0`；
- Unity／Android／Windows／Host／RES／LOC-B commands：`0`；
- production eight-path writes：`0`。

closure-only probe 若无法在这些零值下冻结完整 runner/test graph、expected deployment、
fixture/harness argv 或 process DAG，M 必须返回 blocker；不能把未知量推给 B。

## 7. process、temp、network 与 request signing blockers

SYS/ARCH 的 hard ceilings 只是上限，不是 exact M budget。future M packet 必须填入以下
具体值后才能签发：

1. `maxTopLevelCommandCount`：大于零的 exact integer；按 acquisition、signature、closure、
   manifest、seal 分项，不能写范围。
2. `maxTopLevelProcessTreeCount=1` 且 `maxConcurrentTopLevelTrees=1`；一次只运行一个顶层树。
3. 每个 top-level command 的 executable absolute path/SHA、完整 argv/cwd/env、maxStarts、
   timeout、kill grace、stdout/stderr cap 和 expected exit。
4. 每种 child executable 的 absolute path/SHA、parent、purpose、exact max total starts、exact
   max concurrent；包括 `dotnet`、MSBuild、NuGet、VBCSCompiler、VSTest、testhost、shell、
   checksum/archive/signature tools与任何 harness child。
5. 每个 temp/material/evidence root 的 max bytes、max files、free-space floor、log cap；hard
   ceilings仍是 M wall `≤60 min`、material `≤512 MiB`、evidence `≤128 MiB`，但 packet 必须
   给不高于 ceiling 的 exact stage numbers。
6. 每个 network request 的 exact URL/method、request count、max redirects、allowed redirect
   hosts、expected content bytes、max transfer bytes、hash、timeout和 destination；整个 M 的
   exact aggregate request count／bytes 也必须给出。
7. source copy、archive extraction、nupkg extraction、signature verification、lock generation、
   manifest sealing各自的 temp/output清单和失败后保留规则。

当前不能确定并因此阻断 M signing 的值：最终 accepted ARCH identity/R receipt；M owner
turn/message；acquisition exact request/redirect/transfer counts；五 test seeds及全部 closure
bytes；六 license gaps的 upstream bindings；runner/test locks；完整 expected-deployment
mapping；signature verifier receipt；005/LOC-A/official-test完整 child DAG；exact command
count、child counts、temp/log/free-space budgets。本文不为这些字段填写估计值。

## 8. future M acceptance checklist 与 BLOCKED return contract

### 8.1 可交独立 R 的最低条件

- [ ] accepted ARCH path/SHA/lines/bytes 与同一 R `ACCEPT` receipt 已固定；SYS/research hashes
      仍与第 1 节相同。
- [ ] delivery owner、主程 authority turn、return path、三个 create-once roots 和执行前
      absence receipts 已固定。
- [ ] official Luban source/commit/tree、两个 csproj preimages、test project和两行 patch
      exact bytes/SHA 已冻结；零 `.cs` change。
- [ ] YamlDotNet 16.3.0 nupkg/source/signature/DLL/two licenses全部取得并匹配固定值；
      `YamlDotNet.NetCore` 在 candidate feed/locks/mapping中为零。
- [ ] NeoLua nupkg、tag/commit、package/source relationship和 Apache-2.0 license bytes已冻结。
- [ ] 六个 license gaps 逐项有 official source/tag-or-commit/license raw bytes/hash/expression，
      MessagePack.Annotations关系证据独立明确。
- [ ] 五 direct test seeds及所有 transitive/analyzer/build/native/contentfiles/testhost/runtime
      assets逐项进入 test lock、package/license/source manifests。
- [ ] runner lock、test lock、finite expected-deployment mapping、package-files、license-files、
      SBOM 集合闭合，零 wildcard/unknown/duplicate owner。
- [ ] commands/process/temp/network manifests的 exact numeric fields全部填满；实际仅运行 packet
      授权的 acquisition/closure-only DAG；candidate build/publish/test为零。
- [ ] old cache/evidence before/after manifests相等；生产八路径、Git、Unity均未触碰。
- [ ] M material/evidence create-once封存，状态为 `CANDIDATE_NOT_ACCEPTED`；作者没有自判
      `M-ACCEPTED`，而是交独立 R。

### 8.2 BLOCKED 返回合同

任一 gate 不满足即停止所有依赖步骤，保留已写的新 M roots，不创建 B/L roots。回执必须
包含：

```text
status: BLOCKED_<CODE>
failedGate: <single earliest failing gate>
expected: <exact expected identity/value>
observed: <exact observed identity/value or MISSING>
completedCommands: <ordered command IDs with exit/timing>
notRunCommands: <ordered dependent command IDs>
writtenRoots: <exact new roots and manifest state>
oldRootEquality: <before/after result>
productionWrites: 0
candidateBuilds: 0
candidatePublishes: 0
returnTo: 01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local
```

允许的具体 code 至少包括：`BLOCKED_ARCH_NOT_ACCEPTED`、`BLOCKED_AUTHORITY_DRIFT`、
`BLOCKED_ROOT_EXISTS`、`BLOCKED_NETWORK_BUDGET`、`BLOCKED_NETWORK_DRIFT`、
`BLOCKED_MATERIAL_MISSING`、`BLOCKED_LICENSE_MATERIAL`、`BLOCKED_LICENSE_RELATIONSHIP`、
`BLOCKED_PACKAGE_SIGNATURE`、`BLOCKED_MATERIAL_FREEZE`、`BLOCKED_LICENSE_CLOSURE`、
`BLOCKED_PROCESS_MODEL`、`BLOCKED_PROCESS_BUDGET`、`BLOCKED_TEMP_BUDGET`、
`BLOCKED_OLD_ROOT_DRIFT`。不得用 broadened scope、额外版本、现场 build 或 B output 补洞。

## 9. 本准备完成条件

本文件完成后只表示：future M packet 的已知材料、缺口、schema、roots、owner/return path、
预算签名字段和停止合同已集中到一个可审草案。当前仍需主程完成以下签发前工作：

1. 取得最终 ARCH 的同一 R `ACCEPT` 并固定 accepted bytes；
2. 决定 M actual owner/turn，给出 acquisition／closure probe 的 exact command DAG；
3. 对第 7 节全部 signing blockers填入实测或可审 exact值；
4. 让独立 R 审查新的 M packet，而不是把本文当作 M acceptance。

本准备实际执行：只读检查本地 source/cache/evidence，唯一仓库写入为本文件；没有下载、
联网、restore、build、publish、test、Luban、`005`、Unity、生产路径、外盘或 Git 写入。
