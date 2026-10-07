# HOST-NEXT-001/M02：仅修观察器与执行器收尾

2026-10-08。中央已wait89确认C actual `01a1173e-2a8b-7a72-b25d-b8fb01756014` completed，42叶证据实核；唯一结论见 `host-next-001-m01-integration-receipt.json`：NEEDS_FIX。用户持续开发及限定场景验证授权有效，本修正为普通技术问题，不需用户重复确认。唯一C仍 `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh，fresh actual；中央唯一接收。

## 输入与精确范围

继承已签 `engineering-host-next-001.md` 的四项观察、同P/K、32固定产品候选、N11R1场景原字节、一次图形Unity/Play、恢复和资源边界；本页仅覆盖下列工具修正。M01所有文件保持，不改写FAILED。

- M01 receipt148324B/SHA `f160b1402a22c0dc48b7b766e8ef8ff14449b5fbd817a42d37fc906c78b63da8`；observations81061B/`6d008cff198f40c94a0dc7983b8c9d4cad7d59fda7b986de679559f6a4ae2530`；restore5114B/`de74c1c2252cc521b3136ffd9bc7ac3b1713b442761ef770ef32c25720966dfe`。
- 唯二修正源：M01 `S/FightMatchHostFirstFrameProbe.cs` 32858B/SHA `36f3b8220598693a99fde268e28b4a35ec39219ac5cedcbb7b36b5070625e7d6`；`runner.py`34401B/SHA `f169868bd305aea7f5da45cc519a61f65192fdf180083e0a40071fb912581c82`。复制至新E=`TestArtifacts/FightMatch/HOST-NEXT-001/M02/`，只改本页列出的清理/监护/观测真实状态以及新E/nonce/owner绑定。
- E结构与M01合同相同，另准根 `correction.patch` 保存两文件实质修正及准备差量；不要创建其它报告体系。探针/runner上限沿400/450非空行，确需少量明确错误处理可分别至430/480行，不靠压行规避范围。
- 使用M01 archive中已自然完成的probe.meta：243B/SHA `bae24ed1a3356a9d5b5de85536cf4f89e8ae586de0bd87f3cb45f7725106913b`，GUID `a01fa6d526cf04ed3ba9ce624ae68ea2`，原字节复制至P临时probe.meta；不另产生GUID。加入probe前1014叶canonical仍 `e4caecf42b39e97d6acc517ae20c0dc1a6d97ffab0bd72b0a675b1cfbddb9050`，执行摘要仍按原公式只随新probe源计算，最终1016输入完整清单另封存。
- P现已恢复1008；备份原八覆盖前像后同步同12叶/场景2/probe2。新隔离D和TMP `fm-hn02.XXXXXXXX`，旧M01隔离/证据/TMP不动。所有产品源、场景/meta、A/B/C/D观测条件保持，不增成功LOC、物理点击或业务调用。

## 四处修正与验收

1. **空原始场景状态。** 原originalSetup=[]不能调用RestoreSceneManagerSetup。对已证明本隔离Editor最初没有可恢复场景的情形，明确记录empty-original/no-restorable-scene，关闭本次拥有的观察场景或随该唯一Editor退出释放；不调用RestoreSceneManagerSetup([])，不保存场景，也不把跳过写成restore成功。非空有效原setup仍准确恢复。Scene/Prefab磁盘身份检查及其它清理放入独立finally/错误汇总，不因一个清理异常跳过；保留首错与全部后续错。
2. **GameView真正清理。** 按本轮唯一名字定位custom相对索引，同时记录builtin数量与total=builtin+relative。Unity2022.3的RemoveCustomSize参数必须传total，内部会减builtin；不得传relative。分别记录custom数量和实际删除索引；恢复原选择后删除，鲜核本轮项缺席、其余项目和原选择保持。调用次数不等于成功；同帧Unity日志错误要进入最终错误清单。原M01遗留不得擅自删除或当作本次项，只作为固定原像保留。无SaveToHDD/EditorPrefs写。
3. **收尾持续监护。** Unity正常执行和60秒自然退出／30秒TERM确认阶段都继续每2秒检查资源、源与进程消费者；首个失败保留，后续失败另记并继续有界安全收尾，不因异常跳过监护或无限循环。不要重复给同owned PID发TERM；未知进程无信号，无SIGKILL。
4. **有证据的本轮SDK ADB。** 不把全部adb加入白名单。启动前冻结批准Unity安装内Android SDK platform-tools/adb的canonical路径与bytes/SHA，并记录当前进程基线；本轮最多识别一个在Unity启动后出现的服务器，其实际映射可执行文件必须是此固定SDK adb，PID/start/full argv鲜核，启动日志与时间线对应。只允许常规`adb -L tcp:5037 fork-server server --reply-fd <整数>`，本人UID；FD1/2如关联TMP，仅可为本轮TMP下的普通adb日志，按device/inode核对。允许仅cwd指向P目录，不允许持有P/K源、资源或编译管道；其它相关FD/未知关联立即失败。该例外只是不把有证据的日志进程误当编译消费者，**不授予kill/重启/adb命令执行或设备操作**，亦不授权删除其日志/TMP。无法取得完整身份/FD证据则记录具体缺失并停止，不猜测。旧ADB27858精确例外保持，旧新用途分开记录，不沿用已结束33714身份。与本轮SDK无关的外来ADB保持原保护。

## 执行与完成

C直接完成小修、冻结源并走正常工具审批运行一次，不先扩展调查或重造框架。准备机械≤120秒、静态最多2轮合计30秒；Unity≤240秒（含新probe必要编译）、Play一次；准备至归档总机械≤600秒。无额外I/T、旧10项/全量/下载/restore/安装/Git/Scene导出。新源/新失败是本次新增运行理由，旧四项观察不被抹去或冒称原合同成功。

沿原E≤32MiB/单log8MiB、P生成物4GiB、K1GiB、TMP16MiB/512叶、空闲2GiB及其余保护。结束封存16实际后像和本次隔离树，恢复八前像并移除八本次新增，P回原1008，旧证据/host-io保持。报告四项观察、工具清理、持续监护、native退出、恢复分别结果。无真实存档或产品接收宣称，正常交互仍LOC_BLOCKED，PR4 P1保持开放。中央用实际完成事件接收；不把来源工具的自报结果当GitHub审查。

## 执行前修订 R1

C actual `01a11762-1ac1-7040-8240-f6f19ccd6268` 指出第2点原API说明反了。中央实读[Unity官方2022.3源码](https://raw.githubusercontent.com/Unity-Technologies/UnityCsReference/2022.3/Editor/Mono/GameView/GameViewSizeGroup.cs)76–98行，确认RemoveCustomSize先TotalIndexToCustomIndex，再减builtin；现修正为传total。此前文书错误保留于此说明，M02尚未运行；其余边界/次数/预算不变，C继续其它独立准备后按修订封签执行。
