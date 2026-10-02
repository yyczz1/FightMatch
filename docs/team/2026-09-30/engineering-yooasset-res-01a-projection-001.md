# RES-01A-P01 — 独立投影包图探针增补

2026-10-03 · 范围及固定下载来源已获中央批准；M03 I实际exit0及T17/17、owned[]已由主程03:16:43机械收件（491cc9 exit0），T续跑receipt 8692B／SHA `381ad56bad92eedbfc30e4b50123691ac0f40899c386d13180b3dc042b470edb`。现可派准备后一次绑定；首Unity前不下载。唯一C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh；主程唯一收件再交中央，QA `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local`独立证据接收，Git发布/PR审查由中央协调。

## 覆盖旧包的准确边界

继承[RES-01A旧合同](engineering-yooasset-res-01a-implementation.md)（SHA `7822656fac5c8a6eeeec08f37f71f6d48784897f98abbb7ce739c5358094e111`）的固定版本、异步UPM、来源/包图/程序集/许可证、helper归档清理及fresh compile验收；设计SHA `2e12b5f24ecedef03df5795c9632318c55fe6330562731892d4c9704c54740d8`不改。本增补只把目标改为自有P01投影，解除对旧uGUI交付/PR4场景的排期依赖；共享项目包采纳、Android/发行仍未接收。只有本固定投影图通过，后续adapter才能按该图另包实施；不能称共享RES-A已经完成。

唯一新根为 `TestArtifacts/FightMatch/RES-01A/P01/`（起始ABSENT）。`projection/`仅从正式收件后的M03所用M02/projection的Assets/Packages/ProjectSettings逐叶复制：998原源＋10固定自然meta（以M03/T-continuation绑定的完整import后像为准，GUID保持；不是M02中途失败时的59B前像），若已有精确3534B默认SceneTemplateSettings.json则一并固定；完整input identity在`run.json`中绑定。现有S/M01/M02/M03/共享及旧缓存全只读，零旧Library/Temp/ScriptAssemblies/Artifacts复制。新`package-cache/`可按源身份COW复用已核51包缓存、不hardlink；新增下载只能写新自有cache/generated。采用新的canonical≤40字节、非链接、当前UID的`/private/tmp/fm-res-a.XXXXXXXX` socket小临时根，16MiB/512叶，不复用/清理M03 tmp；大型数据仍外置。

投影最终可接受差异仍仅`Packages/manifest.json`及`Packages/packages-lock.json`，必须由一次`Client.AddAndRemove`产生，不手写/复制JSON。暂态仅`Assets/Editor/ProjectBootstrap/FightMatchYooAssetProbeInstaller.cs`（apply_patch创建）及Unity自然生成的`.cs.meta`、`Assets/Editor.meta`、`Assets/Editor/ProjectBootstrap.meta`；预存在的父路径/meta零改。所有本包新建helper/meta归档并按精确路径移除，fresh compile之前须全部缺席；现有1008源/meta及其它Settings保持。

## 固定包与有界执行

1. 目标只为YooAsset3.0.6/fullSHA `3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804`，唯一Add URL `https://github.com/tuyoogame/YooAsset.git?path=/Assets/YooAsset#3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804`。允许UPM从该GitHub来源取固定包，并从`https://packages.unity.com`解析/获取SBP1.21.25及其官方响应指定的分发URL；记录实际URL、Git revision、两包源身份和锁文件hash。禁止镜像、浮动版本、手动clone/curl安装、依赖更新或业务资源下载。现有三builtin模块为1.0.0；预计只新增YooAsset与SBP两个节点，实际其余版本/source/depth/edges不得漂移，超出旧包四声明依赖集合即失败。
2. 一次UPM Install（异步update poll、无-quit、≤300s，网络窗口同限）→固定commit/四声明依赖/真实PackageInfo及每个asmdef名称/hash、Apache-2.0来源均核验→归档/移除helper→一次fresh compile≤360s。Editor固定M03同一2022.3.18f1 Intel及hash，显式`-batchmode -nographics -buildTarget StandaloneOSX`；Install使用旧包固定executeMethod，compile带-quit。无额外Search/Resolve、发现/测试/Player或其它Unity启动。agent/监督器重试均0；UPM内部行为按实际日志记录，不延长窗口或另发请求。只有实际固定身份成功才通过，不能以manifest出现条目代替解析/编译成功。
3. 失败政策按中央新指示改为原件封存并停止，不向共享采纳，不再发动UPM Remove或rollback compile；此项覆盖旧包所有失败/独立拒绝后的回滚要求，旧A针对共享采纳的回滚门仍保留。已生成的本包helper/meta仍按上述精确归档清理；失败包图/cache保留，不手动修复或删cache。不得用换版本/source/目录重试。
4. 准备≤120s、helper≤180非空行、监督runner≤300非空行；单UPM请求/最多两Editor进程串行、没有第二并行owner。网络仅上述一次UPM窗口≤300s；不声称缓存大小等于传输字节。新cache总量≤2GiB、projection生成物≤4GiB、全部P01占用≤7GiB、证据≤32MiB、每日志≤8MiB，磁盘可用≥4GiB。超限停止新工作，沿M03安全收尾60s自然→精确owned单次TERM→30s，无SIGKILL或全局kill；不安装Editor/CLI/Pipeline、不改license或读取凭据。

## 一次绑定与证据

沿旧A§6非rollback证据文件封闭名单（相对P01根），额外只允许`projection-inputs.json`、`cache-source.json`、`runner.py`、`process-events.jsonl`、`process-after.json`；旧rollback/qa-rejection各文件标not_applicable，不补造。`run.json`须在首Unity前分别绑定保留FAILED的M03原receipt/I-result和已收T-continuation receipt/hash（不得把原receipt改称通过）、998源/10meta及可选精确默认Settings、source/cache副本清单hash、51包前图/两包文件前像、真实C turn、实际tmp/路径、helper/runner hash、完整Install/compile argv、预算及本增补hash。不再绑定旧C或旧共享root。没有未填占位符才执行。

C先按本包制作最小helper/runner及前检，主程对准确输入/执行边界一次机械收件后派发实际运行；不是新设计或重复代码审查。helper遵循旧A§7固定Install行为，不实施Remove入口/请求（本隔离增补禁止其执行）；独立GitHub审查上下文保留归档helper/source和两包diff，由中央按既有代码审查规则处理。最终返回实际请求/进程/graph/assembly/compile、来源/license、只改投影两包文件、helper缺席及共享/历史保护、完整owned闭包与未运行项；作者不自给ACCEPT。正式通过才解锁后续adapter，不接收Android/R2或真实资源加载。
