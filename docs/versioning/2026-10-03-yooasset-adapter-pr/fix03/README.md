# Adapter FIX03

Corrects GitHub finding r4170761716 on d8cc01056bfbc28391d5f9ac34a58e17f38ab111: failed global SDK initialization must not leave a partially published scope that prevents the same provider from retrying. The scope is published only after successful initialization and registration.

Only lifecycle and provider-test code changes. The original 37 test bodies/assertions, seven other source files and all twelve metadata files remain unchanged. One regression covers failure, explicit retry on the same provider, successful reuse and cleanup. The 38-name candidate passed 215 mechanical checks; compilation, actual test execution and new-head independent review remain pending at this source checkpoint.

The prior M03 receipt is preserved unchanged: its 37/37 result belongs to FIX02 and cannot validate this correction. The original source receipt and exact execution scope retain provenance and limitations. Real resource loading, Host, Windows/Android, shared adoption and Demo acceptance remain separate gates.
