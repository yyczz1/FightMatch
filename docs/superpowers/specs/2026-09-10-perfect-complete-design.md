# Perfect Complete Design

## Goal

Add a deterministic `Perfect Complete` action that evaluates multiple legal solutions within the existing solver timeout and node budget, then selects the solution that best matches the current designer parameters.

## Product behavior

- `Complete` remains the fast first-solution action.
- `Perfect Complete` enumerates candidates and keeps the best candidate found within the configured budget.
- Both actions honor endpoints and fixed constraints, remain cancellable, and produce one Undo/Redo command on success.
- A timeout after at least one candidate returns the best candidate found and reports that the search was budget-limited. Cancellation does not apply a candidate.
- No uniqueness decision is made and candidates are not rejected merely because multiple solutions exist.

## Candidate ranking

Candidates must be valid before ranking. Ranking is deterministic and lexicographic:

1. smallest coverage-range penalty;
2. smallest per-path-length-range penalty;
3. smallest target-tier penalty when enabled;
4. smallest target-score-range penalty when enabled;
5. smallest detour-range penalty when the detour range is non-zero;
6. highest match to turn, interaction, and bottleneck preferences;
7. smallest path-length imbalance;
8. canonical ordered path coordinates as a deterministic tie-breaker.

`Min/Max Endpoint Distance` does not participate because endpoints are fixed during completion. Generator attempts, batch settings, output paths, and seed are also not candidate-quality inputs. Solver timeout and node budget bound the search but do not affect candidate score.

## Architecture

- The solving layer exposes bounded solution enumeration without depending on difficulty code.
- The application layer owns candidate scoring and the perfect-completion provider because it can depend on both solving and difficulty assemblies.
- The Editor window selects the normal or perfect provider and applies the chosen result through the existing Draft snapshot command history.

