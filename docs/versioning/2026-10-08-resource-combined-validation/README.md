# Combined resource validation — FIX01

The current resource/Host combination needs one bounded compile followed by the fixed89-case EditMode run. The supervisor uses the existing P01 projection, exact recoverable file operations and a fresh external Bee cache, with current assembly provenance required before tests.

FIX01 corrects GitHub P1 [4212752227](https://github.com/yyczz1/FightMatch/pull/20#discussion_r4212752227): ordinary work could exhaust the total deadline and leave no time for process cleanup or restoration. Inside the same900-second cap it now reserves90 seconds for natural/TERM closure and60 for restoration; preflight, synchronization and evidence consume the work window, and a stage is not launched without its configured allocation. Original failure status and absolute cleanup limits remain.

Validation: targeted red reproduced the old deadline issue; the next round passed129 cases before a new fixture lacked json. After the minimal fixture fix and one centrally authorized continuation, round3 passed138/138 in8.695 seconds. Three rounds total16.826403543 seconds; preparation ceiling120 seconds. All127 previous expectations remain. Both failures are retained; no native Unity/process/signals/cache mutation/activation occurred. Author actual01a11889-36b5-7603-9850-86026e666c65 completed.

preparation.json contains the exact V00-to-FIX01 diff and24 frozen references. Baseline runner/checker are unchanged FIX05 references, while the complete V00 candidate is preserved at parent8e47895761e110c5db531d83df7bbf5dfa960852. inputs.json and test-cases.json retain shared1036/P01 proposed1037/exact89 inputs. These offline checks are not the Unity89-case run.

Native compile/tests, formal localization, real download/Host, Windows, Android/device and Demo acceptance remain pending. The new head requires GitHub review before a fresh C activation. Draft only; no merge/release/deployment.
