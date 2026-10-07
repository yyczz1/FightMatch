# Combined resource validation source checkpoint

The assembled resource modules need validation together with the current Host/UI code. This change adapts reviewed FIX05 to one Unity compile followed by one exact 89-case EditMode run in the existing P01 projection, with bounded recovery and a separate fresh activation.

Shared input is 1036 files/32657794 bytes, canonical 61f1364be566b5266468ea38a19caba1d172428dc7e5fda06b97398c89c83ba8. Projection before is1035 and proposed1037, including its existing natural LocalePreferenceStoreTests meta. The change uses17 overwrites/4 additions/2 temporary parks and recoverably parks43 exact compiler paths; a new external Bee cache avoids clearing global caches. Twelve current assembly provenance chains gate T. I/T each run at most once, only after fresh activation; total mechanical limit900 seconds. T requires the exact89 names, all Passed.

Source preparation: one offline round,127/127 passed in8.725101625 seconds;102 prior expectations retained and25 cases added. One case label changes from32 to23 source paths to match the new explicit ledger; preparation.json records fixture changes. No native Unity, real process/signals, projection/cache mutation or activation occurred. Author turn:01a1185f-c729-7e71-8028-bbae31d56960. Central verifies identities/diff/retained helper ASTs without another code review or replay run.

The exact code diff, finite evidence paths, identities and limits are in preparation.json; inputs.json and test-cases.json fix inputs and selection. Baseline files are byte-identical FIX05 references from PR18 ac5378d566e1b0cbb43c400c3f302b5f0c3f82b8, included for comparison, not new runtime entry points. Resource source gate is PR19 dc1bba0537779983647f5e261f8e1c33a1210a49. This PR changes validation source/context only and does not claim the complete branch equals the shared input.

Native I/T, formal localization, real download/Host interaction, Windows, Android/device and Demo acceptance remain pending. New runner requires independent GitHub review before C receives fresh activation. Draft only; no merge, release or deployment.
