# Resource combined validation — V04 FIX02

V04 proves Bee IPC identities and handles normal natural-close expiry. FIX01 propagates that control condition through discover. FIX02 addresses PR20 discussion4213782936 by refreshing TERM candidates throughout the existing bounded closure window, including newly discovered and fully verified owned children. Per-PID TERM remains at most once; unknown identities receive no signal.

FIX02 changes only closure relative to a2949cfc655e95693c44b5bfd17f1252e239fe2d; other92 function/class ASTs, input bytes, activation and observed-OS-exit contracts remain unchanged. Actual discover/register/closure replay now uses independent per-PID liveness, including a surviving/reparented child and new members during TERM. Current69/69 passed once in5.869845s. Prior65/63/V02 177 results remain historical, within their reuse limits.

V03 native remains FAILED after one I; T89 has never run. V04 native remains unrun. This review checkpoint is not product, gameplay, visual or device acceptance and does not merge into master. New execution still requires12 complete assembly provenances,89 exact Passed tests, actual activation I/O/symlinks, protected source/cache, restored leaves and observed OS exit within900s including160 preparation. No KILL or retries.
