# LOC-STANDARD-001：重复键检查

结论：标准 Luban 调用按预期以退出码 **1** 失败，诊断为真实重复键。原表保持不变。

- 原始工具：`exec-d5cec986-bc74-4c23-a15a-6bec3e5420dd`；退出码 1；完整命令耗时 2541 ms。
- 检查结果：`error.data.duplicate_key`。表 `fm.TbLocalizedText` 的主键字段 `key` 中，`fm.common.nav.adventure` 重复；诊断指向隔离 CSV 的记录 1 和记录 2。隔离 CSV 解析也确认该 key 出现在第 4、268 条 CSV 记录。
- 原始输入均未修改：`luban.conf` 315B、`__tables__.csv` 399B、`FightMatchText.csv` 65,798B；其隔离副本除 CSV 末尾追加一条完整原记录外，与源文件一致。
- Luban 在数据校验阶段退出，`invalid/raw` 没有生成。

工作目录：`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`

实际 Luban 命令及日志捕获：

```sh
FM_LUBAN_ROOT='/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0'
FM_CHECK_ROOT='/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-STANDARD-001'
FM_DOTNET="$FM_LUBAN_ROOT/dotnet-x64/dotnet"
FM_RUNNER="$FM_LUBAN_ROOT/runner"
FM_INVALID="$FM_CHECK_ROOT/invalid"
"$FM_DOTNET" "$FM_RUNNER/Luban.dll" \
  --conf "$FM_INVALID/Config/FightMatch/Luban/luban.conf" \
  -t client -d json --strict --errorFormat json -l "$FM_RUNNER/nlog.xml" \
  -x "outputDataDir=$FM_INVALID/raw" \
  > "$FM_INVALID/stdout.log" 2> "$FM_INVALID/stderr.log"
FM_CHECK_EXIT=$?
printf 'LUBAN_EXIT=%s\n' "$FM_CHECK_EXIT"
printf '%s\n' "$FM_CHECK_EXIT" > "$FM_INVALID/exit-code.txt"
exit "$FM_CHECK_EXIT"
```

工具输入：

- `dotnet`：111,744B，SHA-256 `e307c181562634fd59416032f56d506f85de5b405604dae4a20e2117e8709686`
- `Luban.dll`：20,992B，SHA-256 `6e60beee2b1e8e2870278e6ec363b4dc516311a6f2711bf3518b5edae056df7e`

隔离副本与日志：

| 文件 | 大小 | SHA-256 |
|---|---:|---|
| `invalid/Config/FightMatch/Luban/luban.conf` | 315B | `03ed736de1debd33a1d6c1ebbf30149c2876eb0458f0da34c4a819bee794dc41` |
| `invalid/Config/FightMatch/Localization/__tables__.csv` | 399B | `5312d8f5367feda5a41370b5441b2e46d861e902faef5b1bb6b1b4c3795e6c35` |
| `invalid/Config/FightMatch/Localization/FightMatchText.csv` | 65,956B | `50661a24488ef7ef271f01d473b10343d8fb5ebd8638c46c416b37f1bb4ff31f` |
| `invalid/stdout.log` | 678B | `745ace3bab13892c18d69607e5a0d4e2375e3ff6154b65ce53cab2fa22069fbd` |
| `invalid/stderr.log` | 1,109B | `2eb2661df25fb04b0bf8547619af61c251b2ceb9d8c54f601607ee1cb8547ed4` |
| `invalid/exit-code.txt` | 2B | `4355a46b19d348dc2f57c046f8ef63d4538ebb936000f3c9ee954a27460dd865` |

stdout、stderr 与退出码分别保存在上述隔离文件中；工具原始调用输出为 `LUBAN_EXIT=1`，未截断。
