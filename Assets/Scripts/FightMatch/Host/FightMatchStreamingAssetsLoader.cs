using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FightMatch.Content;
using FightMatch.Core;
using UnityEngine.Networking;

namespace FightMatch.Host
{
    public sealed class FightMatchStreamingAssetsLoader
    {
        public static IReadOnlyList<string> FileNames { get; } = Array.AsReadOnly(new[]
        {
            "first-release.fmsource.json", "first-release.fmpackage.bytes", "first-release.fmvalidation.bytes",
            "first-release.fmreview.json", "first-release.fmpublish.json", "first-release.fmrelease.json"
        });
        public PublishedContentCatalog Catalog { get; private set; }
        public string Error { get; private set; }
        public long ReceivedBytes { get; private set; }

        public IEnumerator Load(string streamingAssetsPath)
        {
            Catalog = null;
            Error = null;
            ReceivedBytes = 0;
            var files = new byte[FileNames.Count][];
            var budget = new ContentStoreBudget(new ExactMathBudget(maxPrimitiveSteps: 64000000));
            for (var i = 0; i < FileNames.Count; i++)
            {
                var baseUrl = streamingAssetsPath.Contains("://") ? streamingAssetsPath.TrimEnd('/') + "/" :
                    new Uri(Path.GetFullPath(streamingAssetsPath).TrimEnd('/') + "/").AbsoluteUri;
                var url = baseUrl + "FightMatch/" + FileNames[i];
                using (var receiver = new BoundedReceiver(Math.Min(16 * 1024 * 1024, budget.MaxRecordBytes),
                    32 * 1024 * 1024 - ReceivedBytes))
                using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, receiver, null))
                {
                    request.disposeDownloadHandlerOnDispose = false;
                    request.timeout = 30;
                    yield return request.SendWebRequest();
                    if (receiver.Rejected || request.result != UnityWebRequest.Result.Success)
                    {
                        Error = receiver.Rejected ? "ContentBudgetExceeded" : "ContentReadFailed: " + request.error;
                        yield break;
                    }
                    files[i] = receiver.TakeBytes();
                    ReceivedBytes += files[i].Length;
                }
            }
            var loaded = FirstReleaseContentStorage.Create(files[0], files[1], files[2], files[3], files[4], files[5],
                ContentConsumerCapabilities.Current, budget);
            if (!loaded.IsAccepted) { Error = loaded.RejectionCode; yield break; }
            Catalog = new PublishedContentCatalog(loaded.Value, ContentConsumerCapabilities.Current);
        }

        public sealed class BoundedReceiver : DownloadHandlerScript
        {
            private readonly MemoryStream bytes = new MemoryStream();
            private readonly long limit;
            public bool Rejected { get; private set; }
            public long Length => bytes.Length;

            public BoundedReceiver(long fileLimit, long totalRemaining) : base(new byte[8192])
            {
                if (fileLimit < 0 || totalRemaining < 0) throw new ArgumentOutOfRangeException();
                limit = Math.Min(fileLimit, totalRemaining);
            }

            protected override void ReceiveContentLengthHeader(ulong length)
            {
                if (length > (ulong)limit) Rejected = true;
            }

            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                return Append(data, dataLength);
            }

            public bool Append(byte[] data, int count)
            {
                if (Rejected || data == null || count < 0 || count > data.Length || count > limit - bytes.Length)
                {
                    Rejected = true;
                    return false;
                }
                bytes.Write(data, 0, count);
                return true;
            }

            public byte[] TakeBytes()
            {
                if (Rejected) throw new InvalidOperationException("Rejected bounded download.");
                return bytes.ToArray();
            }

            public override void Dispose()
            {
                bytes.Dispose();
                base.Dispose();
            }
        }
    }
}
