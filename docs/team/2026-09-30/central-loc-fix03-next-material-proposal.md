# LOC FIX03 下一次材料执行提案（尚未授权）

前次M04已正常获准并实际运行，3.646504秒因固定官方归档两条路径别名被拒绝而失败；下载0、restore0。用户明确的一次重试已消耗。本文件只把新的候选动作写清，不执行它，不把普通继续开发授权当作第二次M许可。

候选源码：[PR2](https://github.com/yyczz1/FightMatch/pull/2)，head `c3e017f2fe071f71aa0ed4ad73a927fab7a8e09d`，FIX03 driver 121284B / SHA256 `6f3511bcb06ba7f85a640a58f9d6a6aaaa6d9d87247eb82fd9ac7fa2774d6b20`，model `9bc598c1c6964285f47a642f9d53f98e5e596ce33d62f7ce1269ca53d22a4753`。该准确head已收到GitHub6055811693 no-major结果，summary于08:22:29Z Completed，中央 `loc-m17-fix03-source-verdict.json` 源码限定ACCEPT；无formal review实体，不把结果改写为formal APPROVED。

建议由原LOC会话 `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local` 执行一次M，中央唯一读回实际工具输出与结果，不依赖作者向其他会话发送消息。批准后才绑定人类实际授权item和fresh执行turn，按已审中央host模板填写数据；仍使用正常工具审批，不换执行者绕过拒绝。

- 仅18个固定官方归档，响应正文合计25172764B（约25MB），地址/哈希逐项见FIX03 `authority/source-receipt.json` 的 `MOnlyBudget.officialGets`；无镜像、重定向或下载重试。
- 仅3次既定依赖还原：runner generation、tests generation、tests verification；总墙钟900秒（15分钟），工作870秒、收尾预留30秒。不得转入B/L构建或Unity。
- 新工作根：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix03`。
- 新材料根：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix03`。
- 两根必须仍缺席，已有占位则停止；原m17根和M04/源码失败记录全部只读。实际证据保存至新的 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m05`，不覆盖M04。

下载/展开/还原/写域/进程和收尾限制全部沿固定模型。完成要读取实际退出码、耗时、归档数量和字节、3次还原结果与保护状态；源码存在或26个离线检查通过不等于材料验收。失败保留原始原因，不自动第三次尝试。

待用户明确批准上述新一次M；当前没有创建新host、activation、外置根或执行任何下载/还原。
