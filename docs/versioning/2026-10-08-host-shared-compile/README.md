# Host compile supervision: FIX05

FIX04 review found two remaining P1 cases: a source replaced between identity check and rename could be moved away without a safe return, and TERM authorization probes ran outside the intended 30-second closure window.

FIX05 uses bounded exchange/return slots for the enumerated transfer routes, preserves competing source and destination bytes, and refuses to report restoration when any transfer remains unresolved. TERM authorization and confirmation share one absolute deadline; every blocking probe is clamped to the remaining budget. Compiler acceptance, native launch limits, ADB identity predicates and existing atomic overwrite logic remain unchanged.

Validation: round 1 passed the prior 80 cases and failed 20 of 22 new cases (82/102). Round 2 stopped at a string syntax error before functional replay. After the explicitly recorded correction allowance, round 3 passed 102/102 in 6.216 seconds; all rounds total 10.274 seconds. The first green ended validation. Both failures remain in replay-results.json. The checker records clock-fixture adjustments and the corrected malformed lstart test input. runner.py has 574 nonblank lines. FIX05 ran no real process commands, Unity, projection writes or downloads.

The original I01 native attempt remains FAILED with a null OS exit code. Its later log append, initial CS2001, Csc events and successful-exit text do not change that result. Recovery01 separately restored 1,008 projection files without launching Unity or sending signals. All 25 prior source and 59 native evidence files remain unchanged.

Review runner.py and replay-check.py against previous-fix04-review.json and correction-fix05-plan.md. The finite future transfer slots in preparation.json are proposed execution scope, not native permission. Original inputs.json describes the historical 999 shared files plus one meta. Current shared sources have independently advanced to 1,021 files; a future run needs a new fixed-input contract. Late compilation observations are not a proven cache chain. No Android, gameplay, formal localization or Demo acceptance is claimed.
