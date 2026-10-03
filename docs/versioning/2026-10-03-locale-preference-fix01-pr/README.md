# Locale preference category correction

Native discovery reached 77 matching test leaves but marked six non-runnable. The bounded diagnostic captured two parameterized-suite samples reporting prohibited characters in NUnit category names; it did not enumerate the six failing leaves. Both discovery failures remain unchanged, with zero tests executed.

FIX01 changes exactly 36 Category attribute literals on 22 lines, from PREF-xx to PREFxx. A one-to-one map preserves the 21 requirement IDs. All method bodies, parameter sources and values, assertions, helpers and the original discovery shim are byte-identical. The temporary diagnostic shim is not included in this candidate. Production locale-preference sources are unchanged.

The source passed 27 mechanical checks once. New compilation, actual all-Runnable discovery and test execution are pending; the prior 77 matches are not an authorized test list or passed count. Independent GitHub review must bind the new head. The planned native run uses a fixed composite rather than a full PR tree.

Three previously Unity-generated metadata files are included at their exact asset paths. Their 243-byte identities match the sealed original/PREF diagnostic baseline; no GUID was invented or regenerated. The publication manifest maps all changed bytes.

This commit follows PR3 head 6818405b859d71bf8c0f116299e7e9605a3d769a. Original code-review and failure evidence remain historical; this correction does not activate full localization materials or establish real Host, device, shared-project or Demo acceptance.
