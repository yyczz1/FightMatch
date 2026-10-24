# RAW bounded payload delivery

The existing asset lease chain can expose a verified raw payload through `FightMatchRawBytes.Length` and a read-only `OpenRead()` stream. Raw acquisition uses the pinned YooAsset 3.0.6 `EnsureBundleFileAsync` path, a known module-owned package configuration, release-set length/SHA-256 validation and bounded payload ownership. Different leases share the verified payload without sharing stream positions.

This draft stacks on resource contract FIX01 commit `19179f3a68725d4f806bbc99989bf514f626acbf`. Its scope and delta are published under `docs/team/2026-09-30/engineering-res-d-raw-*.md`; the test budget amendment and original failed static receipt remain explicit. Four source files are byte-identical between the original RAW stage and FIX01.

Native evidence is copied without edits from `TestArtifacts/FightMatch/RES-RAW-JOINT-001/M01`. One import/compile and one EditMode run passed 74/74: RAW 12, prior asset/provider 38, release codec 13 and updated runtime contracts 11. I/T supervision was 80.163906/31.377607 seconds; XML execution was 4.4154187 seconds. This tests the frozen composite projection, not an entire Git branch. The original receipt correctly retains RAWHead=null because publication followed validation. The manifest maps these exact bytes to this publication.

Unity naturally generated the new test meta, GUID `0d90b639a993e44a7b7fbe6c9f730b16`. The exact 243-byte file is published at its asset path; `raw-test.cs.meta.txt` preserves the same evidence bytes.

GitHub code review is pending on this first head. Real SDK initialization, resource package download/unpack, Host wiring, localization material activation, Scene repair, Android/device play and full Demo acceptance are outside this evidence. No local/shared-project adoption or merge is implied.
