# FIX05 封签格式修正

2026-10-12，中央actual `01a12bf9-e665-7d20-ab65-31ca1dfc8613`，沿既定SOURCE权限。原机械封签tool `exec-bc793778-78f9-49a4-935a-dc7404b6f69c` 在3.815204084秒内exit20；offline-checks的原始JSON与规范JSON语义一致、序列化字节不同，命中既存不可变检查。

已派LOC完成格式纠正：F5当前六叶逐字节保存在独立新根 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix05-seal-before/`，可附receipt.json记录原失败和各身份；新根占用即停。随后仅规范化当前F5检查JSON、完成其余派生收据/封签。最终driver148848B / SHA256 `1d294a975d47dc851eb43a3c7667818b70cf29b51936a51a6287b18d5e8bd473`保持不变，OWNER保留真实源码作者回合 `01a12c14-512d-7331-8acc-fd240ca1de4a`，封签回合独立记录。

因明确新失败，新增一次≤30秒机械封签。复用既存prepare.json中的2576文件/909目录partial清单（SHA256 `cd1fc4983e3d83bd48d897c86fe35cb016d4dad4509adf6a801df597c6203cb6`），保留原捕获时间；允许按固定清单校验，不重新采集目录。源码检查81/81保持，旧66项及81项均不再运行；不扫描SDK，不改变产品/执行器安全门，不启动材料执行。最终回执同时记录失败和纠正，当前F5派生文件可因规范化更新，旧F4/M06/identity-before只读。超时或新失败交回中央，不自动再次封签。
