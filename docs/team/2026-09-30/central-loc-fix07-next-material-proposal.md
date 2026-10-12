# FIX07 后的依赖还原提案：全部归档复用，零下载

2026-10-12。当前是可评审方案，尚未执行。M08 已实际运行并失败；用户授权的“一次”已使用，后续诊断与源码修正没有另启完整还原。

## 固定修正及证据

M08 在第一个还原阶段创建 obj/GUID.tmp 时被 macOS 沙盒拒绝。真实 OS 对照已定位到重复次数表达的兼容问题；FIX07 将其等价展开，保持 49 个图内 obj 目录、准确 writer、每目录最多驻留 1 个匹配文件和 16MiB 检查额度。驻留检查不是 OS 逐次写入限额。

- PR2 head `bd92f6bcedaf89751256a4a7e0084d39cd244893`；GitHub trigger6118099176。准确head审查已完成，结果6118132100未报新重大问题，中央 `loc-m17-fix07-source-verdict.json` 为源码限定ACCEPT；不代表材料还原通过。
- SOURCE：`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix07`，八叶13423756B。
- driver261874B，SHA256 `9aa2db85274369fa27365c3fa39b120fa68deed0effb527b7fccaa5a06894f48`。
- freeze2807B，SHA256 `5f747b1441f79bb43ff00f17757b36fee2e7c374d3e8dd00570d42d54b835027`；model `8c66978b5f7a86d77e158baf7b34b7f1c7ab9616987b20e263ff70ec7e6a5f58`。
- 后置命令回执 `loc-m17-fix07-command-receipt.json`35151B，SHA256 `5c027e28a18b37d2e8abca762bbb84b6304d276840b824f75f04aa7faea56797`。
- 首轮 aggregate 因中央库存摘要的 JSON 格式不一致失败；原记录保存，归一化副本未改变库存条目、原捕获时间或重新枚举目录。
- 三个实际 OS 回归符合预期，修正后的16/16差量检查完整调用6.432883秒/exit0，封存11.298968秒/exit0。修正阶段复用原 OS 证据，未新增 OS 子进程。中央实核原工具记录、八叶及远程45文件，未代做代码审查。

## 拟议一次执行

LOC 执行，中央唯一收件。31个NuGet包和1个源码归档全部只读复用；新增下载0。任何缺项或哈希不符即停止。依次只跑 runner生成、tests生成、tests锁定验证三次依赖还原，各最多180秒；含预检的完整调用最多900秒，870秒工作加30秒收尾。第一条失败即停止后续依赖步骤，无自动重试。

新材料目录：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix07`。
新工作目录：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix07`。
目录已占用即停止；保留 M08 和所有旧材料、工作目录、归档及证据。M6/W6 使用已捕获固定清单，保持其保护和局限。维持材料512MiB、工作4GiB、至少2GiB剩余空间门及已有进程限制。

源码独立接收和新一次直接授权均满足后，中央才按已审模板绑定新实际执行者、该次人类授权、准确head、八叶、侧录和新四叶证据根，生成激活记录/host并交正常工具审批。M08 的激活记录与时钟不复用。

成功需三条本次实际还原收据、进程结束证据、锁文件/资产图校验、材料清单及独立接收。B构建/官方测试、L本地化编译、Unity、APK和设备验收另行推进，本方案不覆盖；也不合并、部署、购买或发布。
