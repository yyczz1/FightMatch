using System.Collections;
using System.IO;
using System.Linq;
using FightMatch.Content;
using FightMatch.Core;
using FightMatch.Host;
using FightMatch.Platform;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FightMatch.Android.Tests
{
    [UnityPlatform(RuntimePlatform.Android)]
    public sealed class AndroidContentPlayerTests
    {
        internal static PublishedContentCatalog Catalog;
        internal static IEnumerator Load()
        {
            if (Catalog != null) yield break;
            var loader = new FightMatchStreamingAssetsLoader();
            yield return loader.Load(UnityEngine.Application.streamingAssetsPath);
            Assert.IsNull(loader.Error, loader.Error);
            Assert.IsNotNull(loader.Catalog);
            Assert.AreEqual(38452, loader.ReceivedBytes);
            Catalog = loader.Catalog;
        }
        [UnityTest]
        public IEnumerator A03_ActualJarContentAdmissionAndReflectionExecuteUnderIl2Cpp()
        {
            AndroidQaRun.Initialize();
            Assert.That(UnityEngine.Application.streamingAssetsPath, Does.StartWith("jar:file:"));
            yield return Load();
            var binding = Catalog.GetCurrentBinding("player", FightMatchHostSession.ReleaseSetId, ContentConsumerCapabilities.Current);
            Assert.IsTrue(binding.IsAccepted, binding.RejectionCode);
            var publication = Catalog.ResolveExact(binding.Value, ContentConsumerCapabilities.Current);
            Assert.IsTrue(publication.IsAccepted, publication.RejectionCode);
            Assert.Greater(publication.Value.Definitions.Levels.Count, 0);
            Assert.IsTrue(publication.Value.GetDefaultReferences().All(x => x.IsAccepted));
            if (AndroidQaRun.Current.phase == AndroidQaRun.Phases[0]) CheckContentStorage();
            AndroidQaRun.Write("content-" + AndroidQaRun.Pid + ".json", new ContentResult {
                streamingAssetsPath = UnityEngine.Application.streamingAssetsPath, packageId = binding.Value.PackageId,
                fingerprint = binding.Value.ContentFingerprint, bytes = 38452, references = publication.Value.GetDefaultReferences().Count });
        }
        private static void CheckContentStorage()
        {
            var root = Path.Combine(AndroidQaRun.StorageRoot, "content");
            var store = new AndroidContentPublicationStorage(root);
            const string key = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            Assert.IsNull(store.Read(key, 8));
            Assert.Throws<System.InvalidOperationException>(() => store.WriteImmutable(key, new byte[] { 1 }, 8));
            using (store.AcquireWriter())
            {
                store.WriteImmutable(key, new byte[] { 1, 2, 3 }, 8);
                store.WriteImmutable(key, new byte[] { 1, 2, 3 }, 8);
                Assert.Throws<ContentStorageException>(() => store.WriteImmutable(key, new byte[] { 4 }, 8));
                Assert.Throws<ContentStorageException>(() => store.Read(key, 2));
            }
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, new AndroidContentPublicationStorage(root).Read(key, 8));
            Assert.Throws<System.ArgumentException>(() => store.Read("../escape", 8));
        }
        [System.Serializable] private sealed class ContentResult
        {
            public string streamingAssetsPath, packageId, fingerprint;
            public int bytes, references;
        }
    }
}
