# LAYOUT-S1 FIX02 source candidate

FIX02 addresses [the missed pointer cancellation after re-enable](https://github.com/yyczz1/FightMatch/pull/4#discussion_r4162989184) on reviewed head `ad49aa36ec79505b51dbe5c83ef138f167cdb545`. Two C# files change by 67 added/deleted lines. A private sampling-state flag makes the first invalid sample after re-enable notify Host, while disabling the component only releases its own gate and preserves geometry.

The existing LAYOUT_05 assertions cover both valid and invalid pre-disable states. Each case starts a new pointer after the safe-area change while the component is disabled, so SafeAreaFitter cancellation cannot mask the regression. Original assertions, 12 declarations and eight Overlay lifecycles remain.

[Source receipt](source-receipt.json) and [patch](source.patch) are verbatim frozen evidence. [Publication manifest](publication-manifest.json) identifies this update; [FIX02 contract](../../../team/2026-09-30/engineering-layout-s1-pr4-fix02.md) and [independent previous review](../../../team/2026-09-30/testing-layout-s1-pr4-fix01-review-receipt.json) give scope and original finding. Prior S1/FIX01 records remain historical.

Mechanical source intake passed. Two initial intake-script errors (cleanup/top-level counting and a missing Python import) were corrected before the successful intake; product inputs and tests were not altered or executed for those checker repairs.

**SOURCE_READY / UNCOMPILED / UNIMPORTED / OLD_PREFAB_NOT_MIGRATED / NOT_PLAYABLE_CANDIDATE**. No compiler, Unity, test discovery or test execution ran. Findings remain open pending new-head GitHub review; native activation is separate.
