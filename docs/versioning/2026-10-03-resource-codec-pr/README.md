# Resource release codec checkpoint

Adds the project-owned schema-v1 resource release-set codec. It verifies an independent boot pin, strict canonical JSON and bounded parsing, nested hashes, physical references, and separate business/text/resource identities. The immutable result returns copied canonical bytes. Matching a caller-supplied pin establishes consistency, not trusted provenance or real resource admission.

The original source passed static checks but failed its first Unity compile because one null test argument matched two overloads. FIX01 adds only an explicit byte-array cast; production and the other test bytes are unchanged. Its original mechanical list-ordering failure is preserved in the source receipt. Twelve tests remain pending at this checkpoint.

M02 compiled successfully (Unity exit 0; test assembly actually rebuilt), and Unity completed the two metadata files with their original GUIDs. Its wrapper then stopped because a previously retained ADB service had exited. The original BLOCKED_OR_FAILED receipt is preserved: no T run or passing XML is claimed. Owned-process closeout and a first test-only continuation are separate; compilation will not be repeated without a new cause.

The fixed code and metadata are published together for one independent GitHub review. Existing B code, packages, other metadata, shared project files, Host and saves remain unchanged. Validation uses the isolated fixed composite, not the entire PR branch. The synthetic codec vectors do not establish real resource contents, build provenance, Host, Android/device, or Demo acceptance.
