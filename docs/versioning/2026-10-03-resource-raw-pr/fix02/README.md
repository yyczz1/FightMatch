# Raw continuation scheduling correction

Addresses GitHub P2 r4171809834 on the previous PR12 head. If another body chunk remains and posting its continuation fails, the current main-thread raw read now terminates through the existing failure and cleanup path. If all body bytes are already read, only the bounded EOF/hash completion step may finish synchronously. This preserves the existing RAW09 expectation without reading a second body chunk in the same pump.

Only provider, lifecycle and the raw test source change. The original twelve raw test methods remain byte-identical; RAW13 exercises a 131073-byte body and sustained scheduler failure, checks terminal results and closed streams before any other provider API, and checks reservation return. The unchanged release codec, B38, D13 and C11 inputs remain frozen.

The source passed 51 mechanical checks once. No C# compilation or RAW13 execution has yet been performed for this correction. Original 74/74 native evidence belongs to the prior source and is retained without edits. A fixed new native run and independent GitHub review are required; real resource packages, Host, Android/device play and shared-project adoption remain separate gates.

This commit follows 4d4b07cc1b432add8d8102290802703ed704eb21. Source hashes and the original review are archived exactly; no metadata or package files change.
