# FIX08 后的依赖还原提案：全部归档复用，零下载

2026-10-12。可评审方案，尚未执行。M09 已实际启动并失败；此前“一次”授权已使用。源码修正与审查不会重新产生完整还原额度。

## 已定位的失败与修正

M09 首次还原在 Luban.Gdscript 项目根创建 GUID.tmp 时被拒。原规则只覆盖 obj；FIX08 增加三个还原阶段内49个准确项目根，与原49个obj父目录共同覆盖98处，按阶段/写入者绑定。每个父目录仅允许指定小写GUID形状的直接子文件，最多驻留1个、16MiB，并保留链接检查。驻留检查不是OS逐次写入额度。

- PR2当前源码head `a0bfe7007be65eed6d1afda317934195cb6ebbd6`，trigger6120173614。独立GitHub审查已完成，结果6120235084未报新重大问题；中央 `loc-m17-fix08-source-verdict.json` 为源码限定ACCEPT，不代表依赖还原已通过。
- SOURCE：`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix08`，八文件13519942B。
- driver278195B / SHA256 `666faf4d3ed8bc586e8edb91d2c4d07013490d045a11dc47716ef4aa5cd93dbb`。
- freeze2808B / SHA256 `c942f1456cdccf2c1a55f8685eb4d055aad72325d4848877e12d97a380824c0f`。
- model `edf98191eb9cf105d8e4c6361aea2b69718dc75a2380b853128bd03eb5392209`。
- 后置命令回执 `loc-m17-fix08-command-receipt.json`27675B / SHA256 `313f9cc46d470adb5689c273800ceb6f846628ba1567ee0a4d335dbce014e7e1`。
- 42/42静态/模拟差量检查通过，完整调用7.008589秒；一次封存14.938371秒通过。中央实核原工具、八文件及远程31个新blob，1883旧blob不变。
- 历史OS表达式探针复用；新增项目根规则尚无本次OS/完整NuGet通过证据。初稿超出driver净增限制已在正式检查前修正，记录保留。M09失败仍为NEEDS_FIX。

## 拟议一次执行

LOC执行、中央唯一接收。31个NuGet包和1个源码归档全部只读复用，新增下载0；缺项或哈希不符立即停止。依次最多执行runner生成、tests生成、tests锁定验证三次还原，每条最多180秒；包含准备与预检的完整调用最多900秒，870秒工作加30秒收尾。任一步失败即停止后续依赖步骤，不自动重试。

新材料目录：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix08`。
新工作目录：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix08`。
新证据目录拟为 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m10`，只含before/stdout/stderr/receipt四项；实际授权后再绑定。
上述新目录已占用即停止，不复用旧激活记录或旧host。维持材料512MiB、工作4GiB、至少2GiB剩余空间门和已有进程限制，保留所有旧归档、材料、工作目录及证据。

M7/W7按既存 `central-loc-m09-partial-capture.json`固定清单保护：3701文件、1328目录、129493955B；不重新枚举，不将其记录视为未来未列文件不存在的证明。旧保护约束同时保留。

源码独立接收和新一次直接人类授权均满足后，中央才按已审模板绑定新的实际执行者、授权item、准确源码head、八文件、侧录及新证据根，生成激活记录/host并走正常工具审批。成功需本次三条实际还原收据、进程结束证据、锁文件/资产图校验、材料清单和独立接收。

B构建/官方测试、L本地化编译、Unity、APK与设备验收另行推进。本方案不覆盖它们，不合并、部署、购买或发布。
