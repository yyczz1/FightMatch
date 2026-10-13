# M12 / FIX09 SOURCE：按新上限完成 contentHash 修正

2026-10-12。本文件在派发前固定，后续结果进入最终 sidecar，不回写本输入。原诊断及预算失败保留。

## 权源与职责

用户本轮直接授权见 `central-loc-m12-approval-2026-10-12.json`（1174B，SHA256 `c2f75a25d84f2f64809cfec414f86ce4f61e2731d9fa9b594b614657beea443b`）；中央 actual `01a1306b-42a8-7992-9118-4ed11d490741` / 人类 item `01a1306b-430a-7b61-8e60-3c85a081d47f`。新 driver 上限明确为 **280KiB=286720B**，替代原278528B；其他资源/完整性边界不变。静态及准确 head GitHub 审查通过后的一次0下载/三restore/900秒许可由中央另行绑定，本 SOURCE 阶段不使用。

LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local` 唯一作者，Astra/xhigh；主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local` 唯一组内接收者。实际 author turn、派发 item、工具编号放最终 sidecar。中央接最终候选以发布 GitHub 审查，不逐项接普通进度。

## 固定输入与差量

执行 `engineering-loc-m11-diagnosis.md`（10173B，SHA256 `22052ea225136572c0a6d9416bc0ad33fb1402b1e40b44f50679214038c29ad5`）的已证根因、生产/fixture 最小修正和验收要求；该文件中的旧尺寸阻塞为历史，由本文件更新。F8 driver 278195B/SHA256 `666faf4d3ed8bc586e8edb91d2c4d07013490d045a11dc47716ef4aa5cd93dbb`、freeze SHA256 `c942f1456cdccf2c1a55f8685eb4d055aad72325d4848877e12d97a380824c0f`、已审 head `a0bfe7007be65eed6d1afda317934195cb6ebbd6` 保持只读。

1. 先将当前 `loc-m17-fix09-command-receipt.json` 逐字节备份为 `loc-m17-fix09-budget-blocked-receipt.json`，应均18449B/SHA256 `f93f26bc85179beb7649fd702c41e588560d75dafd58befcc572b56fc098b34f`。完成标准：原失败可恢复且准确绑定；之后 canonical sidecar 才能由本次最终结果替换。
2. 在新 FIX09 实现归档/归档sidecar/metadata 三来源分离；实际安装读取 `.nupkg.metadata.contentHash` 校验 lock/assets，归档 bytes/SHA256/全文SHA512 与固定 catalog 检查保持。缺文件、错误格式/版本/来源、哈希漂移均失败；fixture 匹配真实文件语义。新增定向测试先覆盖读取入口及旧红新绿，源码完成后统一运行一次。现有 M11 失败是已成立的原始反馈，不重跑真实 restore 来重新诊断。
3. 仅适配新根、实际身份、批准输入/旧证据保护、定向检查/封存路由和当前280KiB预算字段。保留旧阶段的历史预算/证据。生产逻辑留在 driver，独立脚本仅承载测试；保持无关基线、全部有效断言及启动/sidecar强制门。旧OS证据复用。

新根为 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix09`。未来外置M9/W9仅作路径文字，沿原诊断命名，本阶段不创建。M8/W8、M11、旧A/H和历史partial均只读；使用已有固定记录，不新枚举旧partial/SDK。若候选必需的固定清单确实缺失，明确缺哪一份及用途，继续独立可完成部分，不假称整树保护已成立。

## 精确写入与验证

LOC白名单：FIX09八叶 `source/m17_driver.py`、`source/m17-inputs.tsv`、`source/m17-write-set.tsv`、`source/source.patch`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/source-receipt.json`；以及本目录 `loc-m17-fix09-static-checks.py`、`loc-m17-fix09-budget-blocked-receipt.json`、最终 `loc-m17-fix09-command-receipt.json`。本文件由主程固定，LOC不修改；不另建身份、复核、接收报告或修改共享索引。

driver≤286720B、八叶总≤16MiB、最终sidecar≤64KiB。源码固定后一次相关静态差量检查完整≤10秒，入口 `/opt/homebrew/bin/python3 -I -B docs/team/2026-09-30/loc-m17-fix09-static-checks.py`；成功后一次 SOURCE 机械封存完整≤30秒，入口 `/opt/homebrew/bin/python3 -I -B TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix09/source/m17_driver.py prepare`。原预算计量不是测试次数。产出的source.patch相对F8；检查脚本准确源码和结果必须进入封存绑定。无需重复全部历史套件。

相关差量必须覆盖：真实读取接口；原M11哈希形状旧红新绿；归档/sidecar/metadata损坏和缺失；metadata版本、固定source、64B Base64；lock/assets不一致；原封闭包/图/locked-mode合同未变；当前上限及来源/sidecar门不弱化。内存fixture须标明STATIC/SIMULATED，原生还原仍未验证。真实失败先保留、组内定位最小原因，不机械重跑历史或自行扩大次数/时间；需要纠正时由主程按明确差量接续。

最终sidecar只在工具返回后记录完整命令/未截断输出、OS退出、完整墙钟、本次实际身份、准确八叶/freeze/model和旧预算备份。不运行host/restore/SDK扫描或探针/下载/Unity/B/L，不Git写入、安装工具、修改产品/API/包版本或权限。

## 返回标准

在 final 返回精确源码/检查/封存身份、预算、保护结果、必要通过/失败/未验证项及最终sidecar路径；主程用一次原工具与交付物核对接收，不做重复本地代码审查。完成候选提交中央走 GitHub PR Code Review；新准确 head 独立审查与后续材料实际结果仍是未关闭的门，作者不得自签集成ACCEPT。
