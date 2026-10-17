using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.AssetAccess;
using FightMatch.Content;
using FightMatch.Core;
using FightMatch.Presentation;

namespace FightMatch.Host
{
    internal sealed class FightMatchResourceBridgeResult
    {
        internal bool IsDataReady => Catalog != null && TextSource != null;
        internal PublishedContentCatalog Catalog { get; }
        internal ILocalizedTextSource TextSource { get; }
        internal string ReleaseSetId { get; }
        internal string DiagnosticCode { get; }
        internal FightMatchResourceBridgeResult(PublishedContentCatalog catalog, ILocalizedTextSource text,
            string set, string diagnostic)
        { Catalog = catalog; TextSource = text; ReleaseSetId = set; DiagnosticCode = diagnostic; }
    }

    internal static class FightMatchResourceBridge
    {
        // Accepted LOC-TOOL-01 formal output. The manifest pin binds artifact hash/count and
        // formalSourceSha256 1a96886aa29ed3e78211b78ffcba9cd79667f57ed0bf88daa7c8c7d18076a030.
        internal const string TextSha256 = "f48264f9ce94cd3bb2ef8d1b6a0be7b241916144b6d5764d11633bb6d6e456bc";
        internal const string TextManifestSha256 = "18437127bc2e2d7371f97da0990de933f0193fe202539593ef19338d47123667";
        private static readonly string[] AssetIds =
        {
            "fm.content.source", "fm.content.package", "fm.content.validation", "fm.content.review",
            "fm.content.publication", "fm.content.release-set", "fm.text.full.artifact", "fm.text.full.manifest"
        };
        private sealed class Rejection : Exception
        {
            internal readonly string Code;
            internal Rejection(string code) { Code = code; }
        }
        private static void Need(bool value, string code)
        { if (!value) throw new Rejection(code); }

        internal static async Task<FightMatchResourceBridgeResult> OpenAsync(IFightMatchResourceRuntime runtime,
            long epoch, CancellationToken cancellationToken)
        {
            var leases = new List<IFightMatchAssetLease<FightMatchRawBytes>>();
            using (cancellationToken.Register(() => runtime?.Dispose()))
            try
            {
                Need(runtime != null && epoch > 0, "RES_SCHEMA");
                cancellationToken.ThrowIfCancellationRequested();
                var inspection = await runtime.InspectAsync(epoch, cancellationToken);
                Need(inspection.IsAccepted, inspection.Diagnostic?.SafeCode ?? "RES_STALE");
                var budget = new AssetAcquireBudget(16777216, 1, 16, 16);
                var contentBudget = new ContentStoreBudget(new ExactMathBudget(maxPrimitiveSteps: 64000000));
                var files = new byte[AssetIds.Length][];
                long total = 0;
                for (var i = 0; i < files.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    FightMatchAssetId.TryCreate(AssetIds[i], out var id);
                    var result = await runtime.Assets.AcquireAsync<FightMatchRawBytes>(id, inspection.ReleaseSetId, budget, epoch);
                    if (result.Lease != null) leases.Add(result.Lease);
                    cancellationToken.ThrowIfCancellationRequested();
                    Need(result.IsAccepted && result.RequestEpoch == epoch && result.ReleaseSetId == inspection.ReleaseSetId &&
                        result.Lease.AssetId.Equals(id) && result.Lease.ReleaseSetId == inspection.ReleaseSetId, "RES_ASSET");
                    var limit = i < 6 ? Math.Min(16777216, contentBudget.MaxRecordBytes) : i == 6 ? 1048576 : 4096;
                    files[i] = Read(result.Lease.Asset, Math.Min(limit, 33554432 - total), cancellationToken);
                    total += files[i].Length;
                }
                Need(files[7].Length == 791 && Hash(files[7]) == TextManifestSha256, "RES_TEXT");
                Need(FightMatchLocalizationCatalogParser.TryParse(files[6], TextSha256, 264,
                    out var text, out _), "RES_TEXT");
                cancellationToken.ThrowIfCancellationRequested();
                var storage = FirstReleaseContentStorage.Create(files[0], files[1], files[2], files[3], files[4], files[5],
                    ContentConsumerCapabilities.Current, contentBudget);
                Need(storage.IsAccepted, "RES_CONTENT");
                cancellationToken.ThrowIfCancellationRequested();
                return new FightMatchResourceBridgeResult(new PublishedContentCatalog(storage.Value,
                    ContentConsumerCapabilities.Current), text, inspection.ReleaseSetId, null);
            }
            catch (OperationCanceledException) { return Failed("RES_STOP"); }
            catch (Rejection error) { return Failed(error.Code); }
            catch (Exception) { return Failed("RES_CONTENT"); }
            finally
            {
                // No raw lease escapes DataReady, including failures after a previous successful read.
                foreach (var lease in leases) lease.Dispose();
            }
        }
        private static FightMatchResourceBridgeResult Failed(string code) =>
            new FightMatchResourceBridgeResult(null, null, null, code);
        private static byte[] Read(FightMatchRawBytes value, long limit, CancellationToken cancellationToken)
        {
            Need(value != null && value.Length > 0 && value.Length <= limit, "RES_BUDGET");
            var bytes = new byte[checked((int)value.Length)];
            using (var stream = value.OpenRead())
            {
                var offset = 0;
                while (offset < bytes.Length)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var count = stream.Read(bytes, offset, Math.Min(65536, bytes.Length - offset));
                    Need(count > 0, "RES_HASH");
                    offset += count;
                }
                Need(stream.ReadByte() == -1, "RES_HASH");
            }
            return bytes;
        }
        private static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
    }
}
