# Real Host first-frame observation

This draft preserves a fixed, runnable-in-the-recorded-local-projection diagnostic candidate above PR16. It adds the existing N11R1 saved scene and a temporary Editor observer, without changing the 1,012 product inputs accepted for Host locale wiring. The diagnostic scene is not exported over FightMatchDemo and is not a Demo release.

The exact M02 run completed once with native exit 0. It observed real Host Start, two advancing Play frames and natural Canvas callbacks, a valid first synchronous layout, and the expected null-localization startup block. It tested two EventSystem raycasts, not physical input or successful gameplay. Its cleanup and isolated projection restoration succeeded. See validation-summary.json and the unchanged raw observations.json. The original failed M01 is explicitly preserved in the summary.

Review scope: verify the first-layout versus post-cancellation distinction, diagnostic/Session/lease assertions, empty-setup and GameView cleanup, event lifecycle, bounded process supervision, and the narrow SDK ADB identity check. Existing PR4 P1 stays open until a reviewer assesses the exact combined candidate. Review does not establish pixel, device or full Demo acceptance.

The runner is the exact executed local supervisor and contains absolute evidence paths. This is an audit package, not a new CI command or an instruction to recreate the whole cache. All raw local input, process and cache snapshots remain in TestArtifacts/FightMatch/HOST-NEXT-001/M02; only scoped observations and source are published. Do not run without a fresh scoped execution contract and isolated paths.
