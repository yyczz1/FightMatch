# Perfect Complete Quality and Cancellation Design

## Goal

Make `Perfect Complete` prefer clean paths when all explicit designer constraints are equally satisfied, clarify its bounded search time, and let the designer stop an active completion without changing the Draft.

## Candidate ranking

The existing explicit ranking remains authoritative: coverage range, per-path length range, optional target tier, optional target score range, optional detour range, and configured turn/interaction/bottleneck preferences.

When candidates are still tied, rank them by:

1. lower total detour;
2. lower total turn count;
3. lower total occupied path cells;
4. lower path-length imbalance;
5. canonical coordinates for deterministic output.

A current clean, validated recommendation participates as the baseline. An enumerated candidate replaces it only when its quality ranking is strictly better; an exact quality tie keeps the current recommendation.

## Search limits

`Solver Timeout` is displayed as `Solver Timeout (ms)`. The existing node budget remains a second safety limit. Exhausting every legal candidate proves the selected candidate is globally best under this ranking. Reaching either limit returns the best candidate found so far when one exists and explicitly reports that global optimality was not proven.

## Cancellation

`Stop Solving` is enabled only while `Complete` or `Perfect Complete` is running. It cancels through the existing cancellation token. Cancellation never applies a partial candidate, never changes the Draft, and never creates an Undo entry.

