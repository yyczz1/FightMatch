# LOC-A fixed-package license-source research

Date: 2026-10-01  
Status: research/preparation only; `BLOCKED_LICENSE_INPUT` for
`YamlDotNet.NetCore 1.0.0`  
Scope: the fixed `YamlDotNet.NetCore 1.0.0` and `NeoLua 1.3.14` packages used by
the accepted Luban `v5.1.0` runner. This report does not authorize production,
tool, dependency, or evidence changes.

## 1. Evidence boundary and method

No Luban, `005`, restore, publish, compiler, Unity, or test command was run for
this research. The eight LOC-A production paths and all 74 files in the existing
FIX-02 evidence root were left unchanged. The existing runner identity remains
132 files with manifest SHA-256
`d9baacda3afc7fee9db4d92cd4199587fa5b9f1b7e4601be85028644170f2bd5`;
the accepted source is Luban tag `v5.1.0`, commit
`df34215d2e22035073d1af83d44e3ac0744dc3b3`, as recorded in the
[FIX-02 inputs](../../../TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-02/inputs.json).

The package findings use only:

- the exact local NuGet archives already used to construct the runner;
- their NuGet.org registration records, catalog entries, and immutable archive
  URLs;
- first-party repository commits and their license/build files; and
- the byte-bound runner/package mapping in the
  [FIX-02 license inventory](../../../TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-02/third-party-license-inventory.json).

The current compiler calls a package `complete` when an archive lacks a
license/notice entry but an extracted `.nuspec` exists. That fallback is visible
in [Program.cs](../../../Tools/FightMatch.Localization.Compiler/Program.cs) and
explains why FIX-02 reports both packages complete. A `.nuspec`, remote URL, or
generic license template is not treated here as local license text.

## 2. Exact package and runner provenance

| Package | Exact NuGet archive | Runner entry and byte match | Local license/notice entry |
| --- | --- | --- | --- |
| `NeoLua 1.3.14` | 1,112,125 bytes; SHA-256 `af8282ab27f9dafaf239558f882a42d65de2fd91bdca025ad4fc999c477d0269`; SHA-512 `ddabfe25d418f9170af43aa8b756b0c84701ab9ea10c02e3189a86e374c1685d351d63cac0f032cbed831d62bab2238251512091947d0d21fce0926e52b89a44` | `lib/net5.0/Neo.Lua.dll` = runner `Neo.Lua.dll`, 321,024 bytes, SHA-256 `acfbc880041d2022c237c32fad7d0d66ee7b2ac49fda71f5bc65c78882dbc335`; `lib/net5.0/de/Neo.Lua.resources.dll` = runner resource DLL, 10,752 bytes, SHA-256 `73b4e75980a944cbd0d96f5e6adfbd000aab781da0ac7500d0f612dae06f5a99` | **No.** The archive has no `LICENSE`, `NOTICE`, `COPYING`, or equivalent entry. Its embedded `NeoLua.nuspec` is 2,033 bytes, SHA-256 `a1018c0b7c986f7489e9a44685ea8d9955bb478be0318ba48574e4d3743e69e1`, and supplies only a mutable `master`-branch `licenseUrl`. |
| `YamlDotNet.NetCore 1.0.0` | 81,052 bytes; SHA-256 `c88b1a0c04e38dc813c373f9775a12370f6921abdd639ee8a1e9da56b0a3ae97`; SHA-512 `1be08080ce325ccd6d8c5945ec533658ca5d2fe84a7e19219bfdd67d8a6d39550f6a910df246ed965ddbffcdcb8122ee0add057fc242767d59cbceb3e3a393e6` | `lib/netstandard1.0/YamlDotNet.NetCore.dll` = runner DLL, 172,032 bytes, SHA-256 `c7b7e80ce82038ed32d58a736b17bd6323571fbde0ad34fcac097cb579cfc94a` | **No.** The five-entry archive contains `_rels/.rels`, `YamlDotNet.NetCore.nuspec`, the DLL, `[Content_Types].xml`, and `.signature.p7s`. The embedded nuspec is 1,749 bytes, SHA-256 `871245d3394e3c4595ab1e6d89b151017a6fe75f1d82ec5d6ba657bb13e5d156`, and has no license, project, or repository field. |

For NeoLua, NuGet's exact-version [registration record](https://api.nuget.org/v3/registration5-semver1/neolua/1.3.14.json)
points to the immutable [archive](https://api.nuget.org/v3-flatcontainer/neolua/1.3.14/neolua.1.3.14.nupkg)
and [catalog entry](https://api.nuget.org/v3/catalog0/data/2021.11.30.11.59.32/neolua.1.3.14.json).
The catalog's SHA-512 base64 value
`3av+JdQY+RcK9Dqot1awyEcBq56hDALjGJqG43TBaF01HWPKwPAyy+2DHWK6siOCUVEgkZR9DSH84JJuUriaRA==`
matches the fixed local cache. It records version `1.3.14`, author `Neolithos`,
publication time `2021-11-30T11:56:58.083Z`, and the first-party project/license
URLs.

For YamlDotNet.NetCore, NuGet's exact-version
[registration record](https://api.nuget.org/v3/registration5-semver1/yamldotnet.netcore/1.0.0.json)
likewise points to the immutable [archive](https://api.nuget.org/v3-flatcontainer/yamldotnet.netcore/1.0.0/yamldotnet.netcore.1.0.0.nupkg)
and [catalog entry](https://api.nuget.org/v3/catalog0/data/2018.11.07.08.16.07/yamldotnet.netcore.1.0.0.json).
The catalog's SHA-512 base64 value
`G+CAgM4yXM1tjFlF7FM2WMpdL+hKfhkhm/3WfYptOVUPapEN8kbtll3b/83LgSLuCt0Ff8JCdn1Zy86z46OT5g==`
matches the fixed local cache. It records version `1.0.0`, authors `Antoine
Aubry, Adam Hathcock`, publication time `2016-10-31T08:05:45.463Z`, and **no**
license expression, license URL, project URL, repository URL, or repository
commit.

## 3. NeoLua 1.3.14: verifiable first-party license input

The first-party tag [`v1.3.14`](https://github.com/neolithos/neolua/tree/v1.3.14)
resolves to commit
[`cb560afef9dd19b7afad6da594b44909c9739698`](https://github.com/neolithos/neolua/commit/cb560afef9dd19b7afad6da594b44909c9739698).
At that exact commit,
[`NeoLua.NuGet/common.targets`](https://github.com/neolithos/neolua/blob/cb560afef9dd19b7afad6da594b44909c9739698/NeoLua.NuGet/common.targets)
sets file version `1.3.14.0`, derives package version `1.3.14`, names
`https://github.com/neolithos/neolua.git` as the repository, and names
`LICENSE.md` as the license URL. The tag's `NeoLua.csproj` targets `net5.0`,
matching the deployed package entry.

The exact first-party license input is
[`LICENSE.md` at cb560af](https://github.com/neolithos/neolua/blob/cb560afef9dd19b7afad6da594b44909c9739698/LICENSE.md):

- content: Apache License 2.0;
- bytes: 11,324;
- SHA-256: `a8ad31b1c3f40dca5a84119351b8fa8ddc868edd77fad8a8ebf6d8f2d16fa4ae`;
- separate `NOTICE`: none in the exact source tree.

This is stronger than the package's mutable `master` URL because it binds the
full license bytes to the same version tag and source commit as the package
build metadata. The archive still does not cryptographically embed the commit,
so the final local evidence should preserve both the NuGet archive identity and
the commit-bound license identity rather than claiming that one hash proves the
other.

## 4. YamlDotNet.NetCore 1.0.0: strong candidate, no exact binding

The strongest first-party source candidate is package co-author Adam Hathcock's
commit
[`3eaa273a69855d3332bbb071ea9a0d2c52972d20`](https://github.com/adamhathcock/YamlDotNet/commit/3eaa273a69855d3332bbb071ea9a0d2c52972d20),
whose patch is titled `Create NetCore project`. Its
[`YamlDotNet.NetCore/project.json`](https://github.com/adamhathcock/YamlDotNet/blob/3eaa273a69855d3332bbb071ea9a0d2c52972d20/YamlDotNet.NetCore/project.json)
sets version `1.0.0-*`, targets `netstandard1.0`, compiles the adjacent
YamlDotNet sources, and lists the same twelve framework dependencies and
minimum versions as the exact package nuspec. The commit is directly based on
upstream YamlDotNet commit
[`b758a2961bdb0b042f7f94e1fe6cddbb34756402`](https://github.com/aaubry/YamlDotNet/commit/b758a2961bdb0b042f7f94e1fe6cddbb34756402).
The package DLL also reports assembly version `0.0.1.0`, file/informational
version `0.0.0`, product `YamlDotNet`, and the same Antoine Aubry copyright
string as that source snapshot.

The candidate commit contains these concrete license inputs:

| Candidate file | Content | Bytes | SHA-256 |
| --- | --- | ---: | --- |
| [`LICENSE`](https://github.com/adamhathcock/YamlDotNet/blob/3eaa273a69855d3332bbb071ea9a0d2c52972d20/LICENSE) | MIT grant and Antoine Aubry/contributors copyright | 1,110 | `daea8b7f1b2bab48ea12ad50b0ce91634669d99d7dd0cb64aed8dffeeef69ed3` |
| [`LICENSE-libyaml`](https://github.com/adamhathcock/YamlDotNet/blob/3eaa273a69855d3332bbb071ea9a0d2c52972d20/LICENSE-libyaml) | MIT-form Kirill Simonov/libyaml copyright and grant | 1,058 | `d0d8b09800a45cd982e9568fc7669d9c1a4c330e275a821bbe24d54366d16fe9` |

These facts make `3eaa273...` a strong source candidate, but they do not close
provenance. The exact package does not name Adam Hathcock's fork, this commit,
or any repository. NuGet owner `L3Z4` has a public
[`YamlDotNet` fork](https://github.com/L3Z4/YamlDotNet), but its published
snapshot exposes upstream `b758a296...` without the NetCore project. No package
field, embedded source link, PDB, local license file, reproducible-build
receipt, or publisher statement binds the nupkg/DLL bytes to `3eaa273...`.

Therefore the exact package's license is **strongly indicated** to be the two
MIT-form files above, but it is not proven to the evidence standard requested
here. Recording either file as the package's accepted license solely because it
is a familiar MIT form would be the forbidden generic-template shortcut.
Current disposition: `BLOCKED_LICENSE_INPUT`.

## 5. Minimal append-only offline evidence-completion plan

This plan is not authorized for execution by this research.

1. Create a new, append-only sibling evidence root such as
   `TestArtifacts/FightMatch/LOC-IMPL-A/license-input-supplement-01/`. Do not
   write into, rename, or regenerate the existing 74-file FIX-02 root.
2. Use only the already fixed local NuGet cache and separately pre-staged,
   hash-bound source files. Do not run Luban, `005`, restore, publish, the LOC-A
   compiler, Unity, or any test. Do not touch the eight LOC-A production paths.
3. For each package, preserve locally: the exact `.nupkg`; its raw entry list;
   the embedded `.nuspec`; a hash/size manifest; NuGet registration/catalog
   metadata; and the selected commit-bound license/notice bytes. Recheck that
   the selected DLL entry equals the accepted runner DLL. This makes later
   review offline and independent of mutable URLs.
4. NeoLua can be marked `PASS_LICENSE_INPUT` only when the locally preserved
   `LICENSE.md` is exactly 11,324 bytes with SHA-256
   `a8ad31b1c3f40dca5a84119351b8fa8ddc868edd77fad8a8ebf6d8f2d16fa4ae`,
   and its provenance manifest binds tag `v1.3.14`, commit `cb560af...`, package
   metadata, archive hashes, and runner-entry hashes.
5. YamlDotNet.NetCore must remain `BLOCKED_LICENSE_INPUT`. Candidate files from
   `3eaa273...` may be stored only as clearly labelled research candidates, not
   accepted license evidence. Closing the block requires a newly approved
   input: a publisher/maintainer statement binding package `1.0.0` to an exact
   source commit, an independently reviewable reproducible source-to-DLL match,
   or a user-approved dependency/tool decision that replaces the untraceable
   package. None is authorized here.
6. A supplement manifest should finish with separate per-package states and an
   overall non-pass while YamlDotNet.NetCore is blocked. It must not rewrite
   FIX-02's historical `PASS_LOC_IMPL_A`, its 132-file inventory, or its hashes.

## 6. Unresolved uncertainties

- NeoLua's version/tag/repository/license relationship is first-party and
  commit-bound, but the nupkg itself does not embed a source commit or the
  license bytes.
- YamlDotNet.NetCore's exact source repository and commit are unresolved.
  `3eaa273...` is the best matching first-party candidate, not a verified
  package binding.
- It is consequently unresolved whether the exact YamlDotNet.NetCore archive
  requires both candidate license files or only the main `LICENSE`; preserving
  both is the conservative candidate set, not an acceptance verdict.
- No legal/compliance verdict, dependency replacement, or change to the
  accepted runner is made by this report.
