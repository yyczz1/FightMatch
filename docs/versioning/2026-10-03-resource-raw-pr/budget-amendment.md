# Central approval of RAW test-line adjustment

2026-10-03. The central coordinator (01a0e401-511d-79f2-b47f-3ab0ade1681b/local), acting under the user's AGENTS section 6 team authority, approved only the new test file limit changing from 600 to 610 physical lines. The limit remains 64 KiB with the same twelve tests, assertions and acceptance criteria. All production limits, four paths, public surface and friend scope remain unchanged. This is a task-budget amendment, not code or behavior acceptance.

The original source stage failed only the 610 > 600 check (128/129 passed). Its ten files remain immutable. FIX01 reused all four source candidates and the patch byte for byte, then passed one 140-check sealing run; total mechanical runs were two of the existing three-run maximum. Production totals 1788 lines/85247 bytes, the test is 610 lines/51958 bytes, and combined existing D plus RAW test lines are 1335 within the prior 1400 limit.

The authorized C input was separately updated to C FIX01 receipt 7abff33326b5dc6f0716627ed74b57fde73e8b03f1d90a47b293d7bf7f0c9c9e. No C code is modified by RAW. The planned fixed-source native regression comprises RAW12 + B38 + D13 + C11, with exact results still required. Original failures and code-review/native gates remain separate.
