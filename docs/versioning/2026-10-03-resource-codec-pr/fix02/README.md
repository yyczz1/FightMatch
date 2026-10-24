# Resource codec FIX02 review checkpoint

PR10's independent review found that two raw logical slots could claim the same physical file when their payload hashes and lengths matched. FIX02 adds raw-name ownership checks while retaining shared bundles for object resources.

The source delta is two production lines and one 41-line D13 regression. All original twelve test bodies, helpers and literal fixtures remain unchanged. D13 accepts equal payloads stored as distinct raw files, then checks that aliasing both slots to one physical file is rejected after hashes and the external pin are recomputed.

The source receipt records 126 static checks, not compiled or executed C# tests. The old candidate's T-continuation was cancelled before Unity started. A separate fixed D13 + C10 composite run is planned; its result is not claimed here. Original M01/M02 failures and review findings remain preserved. The prior review's ADB ownership wording is corrected only by the included erratum.

The same two Unity-generated metadata files remain unchanged. No API, schema, dependencies, Host/UI wiring, shared project adoption, Android or device acceptance is included. GitHub independent review is required on this new head. Publication uses Git data only; local Git and master are unchanged.
