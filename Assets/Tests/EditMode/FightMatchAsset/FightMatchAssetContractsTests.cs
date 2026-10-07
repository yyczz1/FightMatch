using System;
using NUnit.Framework;

namespace FightMatch.AssetAccess.Tests
{
    public sealed class FightMatchAssetContractsTests
    {
        private const string Set = "release-2026.10";
        private static FightMatchAssetId Id(string value = "fm.text.full")
        {
            Assert.IsTrue(FightMatchAssetId.TryCreate(value, out var id));
            return id;
        }
        private static FightMatchAssetDiagnostic Diagnostic(string detail = "", FightMatchAssetDiagnosticCode code = FightMatchAssetDiagnosticCode.SdkFailure,
            FightMatchAssetDiagnosticStage stage = FightMatchAssetDiagnosticStage.Acquire) =>
            new FightMatchAssetDiagnostic(code, stage, Id(), Set, false, detail);

        [Test]
        public void AssetIdsAcceptExactAsciiBoundsAndUseOrdinalIdentity()
        {
            foreach (var value in new[] { "a", "0", "a._-09", "a..b", "latest", new string('z', 128) })
            {
                var first = Id(value); var second = Id(value);
                Assert.AreEqual(value, first.Value); Assert.AreEqual(value, first.ToString());
                Assert.IsTrue(first.Equals(second)); Assert.IsTrue(first.Equals((object)second));
                Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
                Assert.IsFalse(first.Equals(null)); Assert.IsFalse(first.Equals(value));
            }
            Assert.IsFalse(Id("a").Equals(Id("b")));
            Assert.IsFalse(Id("a-b").Equals(Id("a_b")));
        }

        [Test]
        public void AssetIdsRejectMalformedUnicodeCaseAndPathInputWithoutRetainingIt()
        {
            foreach (var value in new[] { null, "", new string('a', 129), "A", "aB", ".a", "_a", "-a",
                " a", "a ", "a\n", "a\0b", "../a", "a/../b", "/private/user", @"C:\save", "https://host/a",
                "a:b", "a%2f", "a?b", "a#b", "a@b", "é", "e\u0301", "ａ", "中文", "a😀" })
            {
                var output = Id();
                Assert.IsFalse(FightMatchAssetId.TryCreate(value, out output));
                Assert.IsNull(output);
            }
        }

        [Test]
        public void BudgetsAcceptBothInclusiveBoundsAndRejectEveryAdjacentOutOfRangeValue()
        {
            var min = new AssetAcquireBudget(1, 1, 1, 1);
            Assert.AreEqual(1, min.MaxRawBytes); Assert.AreEqual(1, min.MaxConcurrentAcquisitions);
            Assert.AreEqual(1, min.MaxQueuedCallbacks); Assert.AreEqual(1, min.MaxRetainedLeases);
            var max = new AssetAcquireBudget(67108864, 32, 1024, 4096);
            Assert.AreEqual(67108864, max.MaxRawBytes); Assert.AreEqual(32, max.MaxConcurrentAcquisitions);
            Assert.AreEqual(1024, max.MaxQueuedCallbacks); Assert.AreEqual(4096, max.MaxRetainedLeases);
            var invalid = new Action[] {
                () => new AssetAcquireBudget(0, 1, 1, 1), () => new AssetAcquireBudget(67108865, 1, 1, 1),
                () => new AssetAcquireBudget(1, 0, 1, 1), () => new AssetAcquireBudget(1, 33, 1, 1),
                () => new AssetAcquireBudget(1, 1, 0, 1), () => new AssetAcquireBudget(1, 1, 1025, 1),
                () => new AssetAcquireBudget(1, 1, 1, 0), () => new AssetAcquireBudget(1, 1, 1, 4097)
            };
            foreach (var action in invalid) Assert.Throws<ArgumentOutOfRangeException>(() => action());
        }

        [Test]
        public void DiagnosticsEnforceEnumRangesAndAssetNullability()
        {
            foreach (FightMatchAssetDiagnosticCode code in Enum.GetValues(typeof(FightMatchAssetDiagnosticCode)))
            foreach (FightMatchAssetDiagnosticStage stage in Enum.GetValues(typeof(FightMatchAssetDiagnosticStage)))
            {
                var invalidId = code == FightMatchAssetDiagnosticCode.InvalidAssetId;
                var asset = invalidId ? null : Id();
                var set = invalidId ? null : Set;
                var diagnostic = new FightMatchAssetDiagnostic(code, stage, asset, set, true, null);
                Assert.AreEqual(code, diagnostic.Code); Assert.AreEqual(stage, diagnostic.Stage);
                Assert.AreSame(asset, diagnostic.AssetId); Assert.AreEqual(set, diagnostic.ReleaseSetId);
                Assert.IsTrue(diagnostic.Retryable); Assert.AreEqual("", diagnostic.SafeDetail);
            }
            Assert.Throws<ArgumentOutOfRangeException>(() => Diagnostic(code: (FightMatchAssetDiagnosticCode)(-1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => Diagnostic(code: (FightMatchAssetDiagnosticCode)12));
            Assert.Throws<ArgumentOutOfRangeException>(() => Diagnostic(stage: (FightMatchAssetDiagnosticStage)(-1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => Diagnostic(stage: (FightMatchAssetDiagnosticStage)8));
            Assert.Throws<ArgumentException>(() => new FightMatchAssetDiagnostic(FightMatchAssetDiagnosticCode.InvalidAssetId,
                FightMatchAssetDiagnosticStage.ValidateRequest, Id(), null, false, ""));
            Assert.Throws<ArgumentException>(() => new FightMatchAssetDiagnostic(FightMatchAssetDiagnosticCode.UnknownAsset,
                FightMatchAssetDiagnosticStage.ValidateRequest, null, Set, false, ""));
            Assert.Throws<ArgumentException>(() => new FightMatchAssetDiagnostic(FightMatchAssetDiagnosticCode.InvalidAssetId,
                FightMatchAssetDiagnosticStage.ValidateRequest, null, Set, false, ""));
        }

        [Test]
        public void SafeDetailsAcceptEveryAllowedCategoryAndCanonicalMaximum()
        {
            foreach (var value in new[] { "failed", "pending", "cancelled", "timeout" })
                Assert.AreEqual("status=" + value, Diagnostic("status=" + value).SafeDetail);
            foreach (var value in new[] { "invalid-asset-id", "unknown-asset", "wrong-asset-type", "invalid-release-set",
                "wrong-release-set", "budget-exceeded", "package-unavailable", "manifest-unavailable",
                "location-unavailable", "sdk-failure", "stale-epoch", "invalid-remote-url", "released-lease", "redacted" })
                Assert.AreEqual("reason=" + value, Diagnostic("reason=" + value).SafeDetail);
            foreach (var value in new[] { "primary", "fallback", "none" })
                Assert.AreEqual("host=" + value, Diagnostic("host=" + value).SafeDetail);
            foreach (var value in new[] { "http=100", "http=599", "count=0", "count=1", "count=9999999999999999999",
                "manifest=" + new string('a', 64), "sha256=" + new string('0', 64), "" })
                Assert.AreEqual(value, Diagnostic(value).SafeDetail);
            var maximum = "count=9999999999999999999;host=fallback;http=599;manifest=" + new string('a', 64) +
                ";reason=manifest-unavailable;sha256=" + new string('f', 64) + ";status=cancelled";
            Assert.AreEqual(239, maximum.Length);
            Assert.AreEqual(maximum, Diagnostic(maximum).SafeDetail);
            Assert.AreEqual("reason=redacted", Diagnostic(maximum + "x").SafeDetail);
            Assert.AreEqual("", Diagnostic(null).SafeDetail);
        }

        [Test]
        public void SafeDetailsRedactEntireInvalidOrSensitiveInput()
        {
            foreach (var detail in new[] { " ", "\t", "\n", "\0", "状态=failed", "status=FAILED", "status=failed\n",
                "unknown=value", "=failed", "status=", "status=failed;", ";status=failed", "status=failed;;reason=sdk-failure",
                "status=failed;reason=sdk-failure", "reason=sdk-failure;reason=redacted", "count=00", "count=01",
                "count=-1", "count=+1", "count=1.0", "count=10000000000000000000", "count=１", "count=1e2",
                "http=099", "http=600", "http=1000", "http=10", "http=1a0", "host=secondary",
                "manifest=" + new string('a', 63), "manifest=" + new string('a', 65), "sha256=" + new string('A', 64),
                "sha256=" + new string('g', 64), "reason=not-allowed", "reason=sdk-failure=extra",
                "reason=sdk-failure;status=https://private.example/?token=secret#fragment",
                "reason=sdk-failure;status=/Users/private/save", @"reason=sdk-failure;status=C:\secret",
                "reason=sdk-failure;status=user@host", "reason=sdk-failure;status=%2Fsecret",
                "reason=sdk-failure;status=é", new string('a', 240) })
                Assert.AreEqual("reason=redacted", Diagnostic(detail).SafeDetail);
        }

        [Test]
        public void InvalidReleaseSetsAreDiscardedAndNeverEchoed()
        {
            foreach (var raw in new[] { null, "", "latest", "LATEST", "bad set", "../private", "https://private/?token=secret", new string('a', 129) })
            {
                var diagnostic = new FightMatchAssetDiagnostic(FightMatchAssetDiagnosticCode.WrongReleaseSet,
                    FightMatchAssetDiagnosticStage.ValidateRequest, Id(), raw, false, "status=failed");
                Assert.IsNull(diagnostic.ReleaseSetId);
                Assert.AreEqual("reason=invalid-release-set", diagnostic.SafeDetail);
                var result = FightMatchAssetAcquireResult<object>.Rejected(diagnostic, 1, raw);
                Assert.IsNull(result.ReleaseSetId); Assert.IsNull(result.Lease); Assert.IsFalse(result.IsAccepted);
                Assert.AreSame(diagnostic, result.Diagnostic);
                Assert.Throws<ArgumentException>(() => new FightMatchAssetDiagnostic(FightMatchAssetDiagnosticCode.SdkFailure,
                    FightMatchAssetDiagnosticStage.Acquire, Id(), raw, false, ""));
            }
            var sensitive = "https://private/?token=secret";
            var error = Assert.Throws<ArgumentException>(() => FightMatchAssetAcquireResult<object>.Accepted(new LeaseData(), 1, sensitive));
            StringAssert.DoesNotContain(sensitive, error.Message);
            var boundedWrongSet = new FightMatchAssetDiagnostic(FightMatchAssetDiagnosticCode.WrongReleaseSet,
                FightMatchAssetDiagnosticStage.ValidateRequest, Id(), "latest-1", false, "reason=wrong-release-set");
            Assert.AreEqual("latest-1", boundedWrongSet.ReleaseSetId);
        }

        // Test-owned lease data only: no provider, resource acquisition, handle, or SDK behavior.
        private sealed class LeaseData : IFightMatchAssetLease<object>
        {
            public FightMatchAssetId AssetId { get; set; } = Id();
            public string ReleaseSetId { get; set; } = Set;
            public string LeaseId { get; set; } = "0123456789abcdef0123456789abcdef";
            public object Asset { get; set; } = new object();
            public bool IsReleased { get; set; }
            public void Dispose() { IsReleased = true; Asset = null; }
        }

        [Test]
        public void AcceptedResultsContainExactlyOneLiveLeaseAndEchoPositiveEpochAndSet()
        {
            foreach (var epoch in new[] { 1L, long.MaxValue })
            {
                var lease = new LeaseData();
                var result = FightMatchAssetAcquireResult<object>.Accepted(lease, epoch, Set);
                Assert.IsTrue(result.IsAccepted); Assert.AreSame(lease, result.Lease);
                Assert.IsNull(result.Diagnostic); Assert.AreEqual(epoch, result.RequestEpoch);
                Assert.AreEqual(Set, result.ReleaseSetId); Assert.IsFalse(result.Lease.IsReleased);
                Assert.IsNotNull(result.Lease.Asset); Assert.IsNotNull(result.Lease.AssetId);
            }
        }

        [Test]
        public void AcceptedResultsRejectMissingReleasedMismatchedOrInvalidLeaseData()
        {
            Assert.Throws<ArgumentNullException>(() => FightMatchAssetAcquireResult<object>.Accepted(null, 1, Set));
            foreach (var epoch in new[] { 0L, -1L, long.MinValue })
                Assert.Throws<ArgumentOutOfRangeException>(() => FightMatchAssetAcquireResult<object>.Accepted(new LeaseData(), epoch, Set));
            foreach (var set in new[] { null, "", "latest", "Release-2026.10", "another-valid-set" })
                Assert.Throws<ArgumentException>(() => FightMatchAssetAcquireResult<object>.Accepted(new LeaseData(), 1, set));
            var mutations = new Action<LeaseData>[] {
                x => x.Dispose(), x => x.IsReleased = true, x => x.Asset = null, x => x.AssetId = null,
                x => x.ReleaseSetId = null, x => x.ReleaseSetId = "latest", x => x.ReleaseSetId = "other",
                x => x.LeaseId = null, x => x.LeaseId = new string('a', 31), x => x.LeaseId = new string('a', 33),
                x => x.LeaseId = new string('A', 32), x => x.LeaseId = new string('g', 32)
            };
            foreach (var mutate in mutations)
            {
                var lease = new LeaseData(); mutate(lease);
                Assert.Throws<ArgumentException>(() => FightMatchAssetAcquireResult<object>.Accepted(lease, 1, Set));
            }
        }

        [Test]
        public void RejectedResultsContainOnlyDiagnosticAndEnforceOrdinalSetAgreement()
        {
            var diagnostic = Diagnostic("reason=sdk-failure");
            var rejected = FightMatchAssetAcquireResult<object>.Rejected(diagnostic, long.MaxValue, Set);
            Assert.IsFalse(rejected.IsAccepted); Assert.IsNull(rejected.Lease);
            Assert.AreSame(diagnostic, rejected.Diagnostic); Assert.AreEqual(Set, rejected.ReleaseSetId);
            Assert.AreEqual(long.MaxValue, rejected.RequestEpoch);
            Assert.Throws<ArgumentNullException>(() => FightMatchAssetAcquireResult<object>.Rejected(null, 1, Set));
            foreach (var set in new[] { null, "", "latest", Set.ToUpperInvariant(), "another-valid-set" })
                Assert.Throws<ArgumentException>(() => FightMatchAssetAcquireResult<object>.Rejected(diagnostic, 1, set));
            var invalidAsset = new FightMatchAssetDiagnostic(FightMatchAssetDiagnosticCode.InvalidAssetId,
                FightMatchAssetDiagnosticStage.ValidateRequest, null, null, false, "reason=invalid-asset-id");
            var invalidResult = FightMatchAssetAcquireResult<object>.Rejected(invalidAsset, 1, "../private");
            Assert.IsNull(invalidResult.ReleaseSetId); Assert.IsNull(invalidResult.Diagnostic.AssetId);
            Assert.IsNull(invalidResult.Lease);
            Assert.Throws<ArgumentException>(() => FightMatchAssetAcquireResult<object>.Rejected(invalidAsset, 1, Set));
        }

        [Test]
        public void RejectedEpochsPreserveEarlierFailuresAndRequireMatchingValidationStage()
        {
            var stale = Diagnostic("reason=stale-epoch", FightMatchAssetDiagnosticCode.StaleEpoch,
                FightMatchAssetDiagnosticStage.ValidateRequest);
            foreach (var epoch in new[] { 0L, -1L, long.MinValue })
            {
                var result = FightMatchAssetAcquireResult<object>.Rejected(stale, epoch, Set);
                Assert.AreEqual(epoch, result.RequestEpoch); Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Lease);
                Assert.Throws<ArgumentOutOfRangeException>(() => FightMatchAssetAcquireResult<object>.Rejected(Diagnostic(), epoch, Set));
                Assert.Throws<ArgumentOutOfRangeException>(() => FightMatchAssetAcquireResult<object>.Rejected(
                    Diagnostic("reason=stale-epoch", FightMatchAssetDiagnosticCode.StaleEpoch), epoch, Set));
            }
            var earlier = new[] {
                new FightMatchAssetDiagnostic(FightMatchAssetDiagnosticCode.InvalidAssetId,
                    FightMatchAssetDiagnosticStage.ValidateRequest, null, null, false, "reason=invalid-asset-id"),
                new FightMatchAssetDiagnostic(FightMatchAssetDiagnosticCode.WrongReleaseSet,
                    FightMatchAssetDiagnosticStage.ValidateRequest, Id(), null, false, ""),
                Diagnostic("reason=budget-exceeded", FightMatchAssetDiagnosticCode.BudgetExceeded, FightMatchAssetDiagnosticStage.ValidateRequest)
            };
            foreach (var diagnostic in earlier)
                Assert.AreSame(diagnostic, FightMatchAssetAcquireResult<object>.Rejected(diagnostic, 0, diagnostic.ReleaseSetId).Diagnostic);
            var wrongValidSet = Diagnostic("reason=wrong-release-set", FightMatchAssetDiagnosticCode.WrongReleaseSet,
                FightMatchAssetDiagnosticStage.ValidateRequest);
            Assert.Throws<ArgumentOutOfRangeException>(() => FightMatchAssetAcquireResult<object>.Rejected(wrongValidSet, 0, Set));
            var completedStale = Diagnostic("reason=stale-epoch", FightMatchAssetDiagnosticCode.StaleEpoch,
                FightMatchAssetDiagnosticStage.ValidateResult);
            Assert.AreSame(completedStale, FightMatchAssetAcquireResult<object>.Rejected(completedStale, 42, Set).Diagnostic);
        }
    }
}
