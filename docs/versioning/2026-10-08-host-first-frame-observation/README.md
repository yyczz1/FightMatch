# Real Host first-frame observation and review corrections

This draft is based on PR16. Its 1,012 accepted product inputs are unchanged; it adds the fixed N11R1 scene and a temporary Editor observer. It is not a Demo release or a replacement of FightMatchDemo.

The two original PR17 P2 findings are addressed: only an OBSERVATION_COMPLETE report with A–D all PASS can pass the supervisor, and layout comparison now requires the same natural sample to be cancelled, unbound, unsubscribed and actually released. The latter uses a read of responsive.validityKnown; the snapshot gate argument's default false value alone is not release evidence.

Evidence is deliberately separated:
- M03 executed once: native exit 0, 16 supervisor predicate cases, 8 actual C# DTO cases and strict A/B/C/D observations passed. Actual cleanup and restoration passed. Its supervisor still FAILED because discovery and classification used different ps snapshots. That failed run is not rewritten.
- runner-executed-m03.py is the exact supervisor from that failed M03 run; observations.json is its unchanged raw native observation report.
- runner.py contains the subsequent narrow same-snapshot supervision fix. Its actual functions passed 21 offline replay cases, including the captured race, foreign ancestry, reused PID and identity failures. supervisor-replay-check.py and supervisor-replay-results.json contain that verification. This final supervisor was not run in Unity again.

See validation-summary.json for identities and individual cases. A future native run requires a fresh execution contract and activation; the retained M03 paths/identity in runner.py are not launch authorization. No existing cache should be recreated merely to review this package.

Normal translations, successful gameplay interaction, Android/device and pixel acceptance remain open. PR4 P1 is not automatically resolved. The original M01/M02/M03 files remain local; the initial PR17 commit preserves the earlier published M02 evidence. No merge or shared-project adoption is included.
