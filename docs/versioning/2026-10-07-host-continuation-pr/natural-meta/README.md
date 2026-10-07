# Naturally completed Unity test metadata

FIX01-M03 ran Unity 2022.3.18f1 once. The Editor exited successfully and compiled the corrected Host.Tests assembly, but the execution wrapper stopped before tests because the new test meta grew from the initial 59-byte GUID-only file to Unity's 243-byte MonoImporter document. The original failed wrapper receipt is preserved; tests had not run at that point.

This commit preserves that exact naturally imported document. The GUID is unchanged. Added values are the default MonoImporter fields: empty external/default references, serialized version 2, execution order 0, no icon, and empty user/bundle data. There is no C# source change.

Provenance: HOST-CONTINUATION-SOURCE-001/FIX01/M03/receipt.json (9090 bytes, SHA256 d6d3ee5f364ef83f3f433703971d1a3626582fdb3a834ffeb01f13cb0f50c132); I/editor.log SHA256 237b00bf77cb5c1b900b4de12ee9524c1fbdb74b8979018f74ba96fc1355a0fe. The original 59-byte artifact remains in M01RecoveryR1. Current validation input is 1012 files / 32086333 bytes / canonical SHA256 786fa16c0c7d1c294dd407f2b58d15c2e68011c08f0dac7f4b2c5ebc3e305c4a.

FIX01-T01 will verify and reuse the actual M03 compile evidence before running the same ten pending tests. GitHub review must reference this new head; the previous head's no-major result remains historical. This metadata checkpoint does not claim test, device, layout, or Demo acceptance.
