# COPY AMEND-05：bootstrap 16 键增补

状态：制作稿待中央接收。仅交独立增补；旧 264-key CSV 仍为受保护输入，正式合并由中央另行协调。
固定旧 CSV：`planning-localization-draft.csv`，SHA-256 `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`；265 物理行／264 记录／65358 bytes。
依据：`art-res-01c-bootstrap-inventory.md` §3–§5、`engineering-resource-runtime-bridge-cd-001.md` §2／§4；同一 CSV→Luban 文本链，英／简中；文案及正式子集、共享字形覆盖尚待接收。

| exact key | 来源 | parameters（—为空） | severity |
| --- | --- | --- | --- |
| fm.common.action.cancel | 旧 | — | info |
| fm.update_ready.title | 旧 | — | info |
| fm.update_ready.body | 旧 | — | info |
| fm.update_ready.restart_button | 旧 | — | warning |
| fm.update_ready.later_button | 旧 | — | info |
| fm.initial_download.title | 旧 | — | info |
| fm.initial_download.summary | 旧 | networkName;remainingSize;totalSize | warning |
| fm.initial_download.start_button | 旧 | — | warning |
| fm.initial_download.cancel_button | 旧 | — | info |
| fm.initial_download.failed | 旧 | errorCode | error |
| fm.mobile_data.title | 旧 | — | warning |
| fm.mobile_data.body | 旧 | remainingSize | warning |
| fm.mobile_data.continue_button | 旧 | — | warning |
| fm.mobile_data.wait_button | 旧 | — | info |
| fm.mobile_data.cancel_button | 旧 | — | info |
| fm.diagnostic.placeholder | 旧 | — | blocking |
| fm.diagnostic.missing_binding | 旧 | errorCode | blocking |
| fm.diagnostic.missing_key | 旧 | localizationKey | blocking |
| fm.diagnostic.parameter_mismatch | 旧 | localizationKey | blocking |
| fm.diagnostic.missing_glyph | 旧 | errorCode | blocking |
| fm.bootstrap.checking | 新 | — | info |
| fm.bootstrap.retry_button | 新 | — | warning |
| fm.bootstrap.offline.title | 新 | — | warning |
| fm.bootstrap.offline.body | 新 | — | error |
| fm.bootstrap.corrupt.title | 新 | — | error |
| fm.bootstrap.corrupt.body | 新 | errorCode | error |
| fm.bootstrap.storage.title | 新 | — | error |
| fm.bootstrap.storage.body | 新 | availableSpace;requiredSpace | error |
| fm.bootstrap.fatal.title | 新 | — | blocking |
| fm.bootstrap.fatal.body | 新 | errorCode | blocking |
| fm.bootstrap.restart_button | 新 | — | warning |
| fm.bootstrap.exit_button | 新 | — | info |
| fm.network.kind.wifi | 新 | — | info |
| fm.network.kind.mobile | 新 | — | warning |
| fm.network.kind.offline | 新 | — | warning |
| fm.network.kind.unknown | 新 | — | warning |

以上 20 旧＋16 新是精确 36-key bootstrap allowlist；旧键正文、参数顺序和 severity 沿原表，新增正文只在增补 CSV 中维护。

- `totalSize`／`remainingSize`／`availableSpace`／`requiredSpace`：同一 Host invariant byte formatter，ASCII 数字、`.`、空格和 B/KB/MB/GB，各 1..16 scalars；禁止负数、NaN、无限或自由单位。
- `networkName`：仅四个 `fm.network.kind.*` exact key 的当前语言成品值，1..24 scalars；禁止 SSID、运营商或外部字符串。
- `errorCode`：固定映射的 `[A-Z0-9._-]`，1..32 ASCII；`localizationKey`：仅上表 exact key，`[a-z0-9._-]`，1..96 ASCII；不显示异常、路径、URL或对象 ID。
- 参数禁止富文本、换行、双向控制字符；超界不得截断，应进入固定 fatal／binding diagnostic 并保留内部证据。文本使用 NFC，参数闭合且英中集合一致；正文不接收非法 markup。
状态沿原合同：checking 不表示下载完成；空缓存离线提示联网重试；损坏仅重取精确目标并保留旧完整集合；空间不足提示腾出空间；fatal 提供重启／退出。重试等待原请求停止，移动数据逐次确认。
本次不生成正式表／子集 JSON，不验证字体或运行时；receipt 冻结本 MD、增补 CSV、旧 CSV 身份及一次静态检查，receipt 不自 hash。
