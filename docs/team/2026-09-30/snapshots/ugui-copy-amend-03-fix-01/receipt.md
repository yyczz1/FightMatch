# UGUI COPY-001-AMEND-03-FIX-01 只读快照回执

## 冻结身份

- 用途：保存已经独立 R `ACCEPT` 的 UGUI 263-key 文案输入，供后续 UGUI 实现、复核与证据追溯使用。
- 原始 live CSV：`docs/team/2026-09-30/planning-localization-draft.csv`
- 只读 snapshot CSV：`docs/team/2026-09-30/snapshots/ugui-copy-amend-03-fix-01/planning-localization-draft.csv`
- 接受的 CSV SHA-256：`d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c`
- 接受的 planning SHA-256：`8cdf0e39dc9364758843eefedf35146433ee5b650e4a42c24e152306eaaccff6`
- 原 217 个物理行前缀 SHA-256：`e546a7c3e3368b7abcde8a2353f6dc3f1157f4623a68a51e5a799cba9c8e782f`
- CSV 规模：264 个物理行，263 条数据记录。

## 独立 R

- Verdict：`ACCEPT`
- R thread：`01a0e404-e8ee-7310-8388-9260babd53f1` / `local`
- R turn：`01a0f3db-8a86-7e63-b77f-2f121cedbd0b`
- R final：`msg_0d29bf8d726d8dc0016abd67d90e4887d0b247cc4f8c806593`

## 权威与变更边界

本目录只是已冻结 UGUI 输入的只读证据，不是第二个人工编辑权威。不得直接编辑 snapshot CSV，不得把它合并回 live CSV，也不得用它覆盖后续获准的本地化迁移结果。

在 LOC 正式迁移并按已批准设计转移权威之前，live 文案权威仍为 `docs/team/2026-09-30/planning-localization-draft.csv`。任何后续 LOC 文案变化均须走其独立任务、审阅和 R；不得反向改变本快照所证明的 UGUI 接受身份。

本快照不证明运行时绑定、Luban 生成物、画面或设备结果。未包含、未预批 `fm.language.save_unknown` 或其他未来增补键。
