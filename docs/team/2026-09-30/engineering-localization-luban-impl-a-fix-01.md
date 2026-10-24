# LOC-IMPL-A-FIX-01：Luban `string#set` 分隔勘误与完整重跑

2026-10-01 · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`。本包只纠正首次
production run 已证实的 Luban v5.1.0 `string#set` 语法，并在全新证据根完整重跑
LOC-IMPL-A；不授权 LOC-IMPL-B、Unity、Host、RES、Windows、Android、设备或发布。

## 1. owner、前置 verdict 与固定输入

- 唯一交付 owner：原 LOC-A execution chat
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`；返回主程
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 本纠正 verdict：`NEEDS_FIX`。原 run 正确停止于 `FMLOC013`，没有 pass-2、
  canonical、manifest、negative、license、diff-check 或 Generated 候选；不得把失败
  raw 当产品输出。
- 原实施包只读身份：`engineering-localization-luban-impl-a.md` SHA-256
  `8e2626dad0d4b3c70854b9b7e22bdd48fce79ea91cececb7af45a9cd01a8487d`。
- 接受设计经精确技术勘误后的唯一身份：
  `engineering-localization-luban-001.md` SHA-256
  `022c060801df22520653856c24bd78dda394deb26f946dbff1b70ec9c338380a`，
  777 lines / 52,323 bytes。该勘误不改变 264 条业务记录、四值 severity 集合、
  输出合同或强校验。
- COPY 输入仍只读为 `planning-localization-draft.csv` SHA-256
  `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`，
  65,358 bytes、265 physical lines、264 records/unique；不得重做 COPY。
- 固定工具仍为 Luban tag `v5.1.0`、commit
  `df34215d2e22035073d1af83d44e3ac0744dc3b3`、SDK `8.0.425`、Luban DLL
  SHA-256 `6e60beee2b1e8e2870278e6ec363b4dc516311a6f2711bf3518b5edae056df7e`、
  132-file runner manifest SHA-256
  `d9baacda3afc7fee9db4d92cd4199587fa5b9f1b7e4601be85028644170f2bd5`。
  不下载、换版本、restore、publish 或重跑 005。

## 2. immutable 失败证据与唯一新 evidence 根

原根
`TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf/`
必须保持逐字节只读：`run.json` SHA-256
`e31cbc302d357dcc0135482e284afd8816e6bbb700a6bb05c8306adfd1c93420`，
`inputs.json` SHA-256
`07134c3d422c534be1d0211c7cbfe3aa11a5b2ae40af09d3fee789fff96d2`，失败正式源
SHA-256 `9695b4501b9d6ffda53914d0a42f61948147e2bf655385fa263e7bf916348e39`。
禁止覆盖、续写、删除或倒填成功字段。

本轮只允许 create-once 新根：

```text
TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-01/
```

其文件闭集、逐步 argv/exit/stdout/stderr/timing、pass-1/pass-2、compiler、
comparison、negative、license、run 与 diff-check 形状完全沿用原实施包 §3。若目录
已有任一结果，返回 `BLOCKED_EVIDENCE_EXISTS`，不得覆盖或另挑第三个根。

## 3. 精确生产 delta 与白名单

沿用原包八路径白名单，但从当前停止候选到纠正候选只允许先改两个文件；
`Program.cs` 的额外变化仅用于让纠正 run 严格自证新的任务／设计身份和旧失败根，
不得借机改变生成、校验或发布语义：

1. `Config/FightMatch/Localization/FightMatchText.csv` 第 2 个逻辑记录精确为：

   ```text
   ##type,string,string,string,string,string,string,string,"string#set=blocking,error,warning,info",string
   ```

   RFC 4180 解析必须仍是含控制列的 10 fields；set cell 精确为
   `string#set=blocking,error,warning,info`。
2. `Tools/FightMatch.Localization.Compiler/Program.cs` 只允许以下闭集变化：
   - `AcceptedDesignSha256` 从旧 `43f98e...` 精确更新为 §1 的
     `022c060801df22520653856c24bd78dda394deb26f946dbff1b70ec9c338380a`；
   - 保留原 `AcceptedPacketSha256`／原包 `RequireHash`，另增加 corrective packet
     path 与 `CorrectivePacketSha256`；后者的精确值是主程最后一次派发本文件时给出的
     SHA-256，`ValidateFixedInputs` 必须 `RequireHash` 本文件；
   - 增加 immutable failure root、`run.json` SHA
     `e31cbc302d357dcc0135482e284afd8816e6bbb700a6bb05c8306adfd1c93420`、
     `inputs.json` SHA
     `07134c3d422c534be1d0211c7cbfe3aa11a5b2ae40af09d3fee789fff96d2`
     与失败 formal-source SHA
     `9695b4501b9d6ffda53914d0a42f61948147e2bf655385fa263e7bf916348e39`
     的 fixed constants；`ValidateFixedInputs` 在创建新根前对旧 run/inputs 做
     `RequireHash`；
   - `EvidenceRelativeRoot` 从旧失败根精确改为
     `TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-01`。
     保留 `RunGenerate` 对 `expectedOutput` 的严格 equality gate，不得放宽、绕过或用
     symlink 指向旧根。
   - formal-source shape gate 把既有 semicolon 期望改为上述逗号值；
   - 新 `inputs.json` 的 `packet` 精确为 `LOC-IMPL-A-FIX-01`，保留原实施包的
     path/SHA，并新增 corrective packet path/SHA 与 immutable failure
     root/run/inputs/formal-source 四项身份；新 `run.json.packet` 同样精确为
     `LOC-IMPL-A-FIX-01`。不得删除原包、design、COPY、005、tool 或 owner 字段。

   除上述身份、root、shape 与 evidence 字段外，不得改变列数、顺序、集合、记录、
   参数、placeholder、双语、strict gate、调用结构或其它 compiler 行为；不得把
   provenance 检查降为 owner 外部口头回执。

其余六个现有候选文件在重跑前字节不变。只有全部门通过后，compiler 才可新建原包
已列的两个 `Generated/FightMatch/Localization/` 候选。完整生产白名单仍只有原包
列出的 8 个路径；禁止修改 `Assets/**`、`Packages/**`、`ProjectSettings/**`、
文档、`.meta`、外盘工具、旧 evidence 或 Git。

## 4. 执行顺序与验收

1. 先重核本包所有身份、原失败根 immutable、当前六个已存在候选文件 SHA 与
   `Generated` 不存在；不符即停止。
2. 实施 §3 两文件闭集 delta，重新计算并记录新的
   `formalSourceSha256`；不得预填其值。
3. 用固定 Roslyn 直接编译到新 evidence `compiler/`，运行全部 12/12 self-tests。
4. 对两个全新空 raw staging 各运行一次固定 Luban；两次都必须 exit 0。raw file set、
   bytes/SHA、canonical、manifest、semantic digest 必须逐字节一致；不得第三次挑结果。
5. 完整运行 duplicate、parameters、severity 三个负例；后两者仍由
   `FMLOC007`/`FMLOC004` 拦截，三个 published canonical count 均为 0。
6. 完整生成并核 132-file `third-party-license-inventory.json`；缺项即
   `LICENSE_INVENTORY_INCOMPLETE`。
7. 所有门通过后才发布两个 Generated 候选，并对生产白名单运行 diff-check；
   `Assets/Packages/ProjectSettings` 在 owner 起点前后身份必须相同。本包不运行 Unity。

任一步非零、identity drift、不确定、license/negative 失败或需要扩大范围时立即停止，
保留新根真实失败证据，不执行依赖步骤，不换工具、不下载、不手改 Generated。

## 5. 回传与独立审查

回传实际 changed/new files、两文件闭集 delta、新
formal/canonical/manifest/semantic SHA、
两次 raw 比较、12 self-tests、三个负例、132-file license 完整性、每步 argv/exit/timing、
diff-check、旧失败根 immutable 复核及所有未运行项。主程检查实际 patch/证据后只交
原独立 R thread `01a0e404-e8ee-7310-8388-9260babd53f1` 给出唯一
`ACCEPT`、`NEEDS_FIX` 或 `REJECT`；本技术勘误不降低原 LOC-IMPL-A 验收条件。
