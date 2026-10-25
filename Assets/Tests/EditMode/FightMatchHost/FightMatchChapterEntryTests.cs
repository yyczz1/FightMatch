using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.AssetAccess;
using FightMatch.Content;
using FightMatch.Core;
using FightMatch.Platform;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace FightMatch.Host.Tests
{
    public sealed class FightMatchChapterEntryTests
    {
        private static PublishedContentCatalog chapterCatalog;
        private static string TextRoot => Path.Combine(HostRig.Project, "Generated/FightMatch/Localization/ChapterOneTrial-r2");
        private static ExactMathBudget Math() => new ExactMathBudget(maxPrimitiveSteps: 64000000);
        private static T Take<T>(PublicationResult<T> result)
        { Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath); return result.Value; }
        private static object Get(object target, string name) => target.GetType().GetField(name,
            BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        private static LocalizationService Text()
        {
            Assert.IsTrue(FightMatchLocalizationCatalogParser.TryParse(File.ReadAllBytes(Path.Combine(TextRoot, "fm-text-v1.json")),
                FightMatchResourceBridge.ChapterTextSha256, 279, out var source, out var error), error);
            return new LocalizationService(source, SystemLanguage.English);
        }

        private static PublishedContentCatalog ChapterCatalog()
        {
            if (chapterCatalog != null) return chapterCatalog;
            var seed = Take(PublishedContentCodec.DecodeSource(File.ReadAllBytes(Path.Combine(HostRig.Project,
                "Assets/FightMatchContent/demo-r1.source.json")), ContentConsumerCapabilities.Current, Math()));
            var source = Take(ChapterContentTables.Read(Path.Combine(HostRig.Project, "Generated/FightMatch/ChapterOneTrial/r2"),
                Path.Combine(HostRig.Project, "Config/FightMatch/ChapterOneTrial"), seed, ContentConsumerCapabilities.Current, Math()));
            source.PackageId = "fixture:host-chapter"; source.DraftId = "fixture:host-chapter-draft";
            source.SourceNotes.Add("Host protocol fixture only; never authoring review or production publication.");
            var draft = new DemoContentDraft(source.DraftId); draft.Revise();
            Assert.AreEqual(draft.Revision, source.Revision); var job = draft.BeginJob();
            var result = PublishedContentCompiler.Prepare(source, job, ContentConsumerCapabilities.Current, Math());
            Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath);
            var p = result.Prepared;
            Assert.IsTrue(p.Binding.PackageId.StartsWith("fixture:", StringComparison.Ordinal));
            var review = new ContentReviewEvidence { SchemaVersion = 1, Verdict = "ACCEPT", AuthorTaskId = "fixture:host-author",
                AuthorTurnId = "fixture:host-author-turn", ReviewerTaskId = "fixture:host-reviewer", ReviewerTurnId = "fixture:host-review-turn",
                SourcePacketRangeSha256 = new string('1', 64), ApprovalBasis = "Isolated Host fixture only",
                ApprovedMappingSha256 = new string('2', 64), DraftId = p.DraftId, Revision = p.Revision,
                Binding = ContentBindingRecord.From(p.Binding), DefinitionBindings = p.DefinitionBindings.Select(DefinitionBindingRecord.From).ToList(),
                SourceBytes = p.SourceBytes.Count, SourceSha256 = p.SourceSha256, PayloadBytes = p.PayloadBytes.Count,
                PayloadSha256 = p.PayloadSha256, ValidationBytes = p.Validation.Bytes.Count, ValidationSha256 = p.Validation.Sha256 };
            var memory = new ContentMemory(); var budget = new ContentStoreBudget(Math());
            var catalog = new PublishedContentCatalog(memory, ContentConsumerCapabilities.Current);
            var publication = Take(catalog.Publish(p, p.Validation, review, job, "fixture:host-chapter-publication", budget)).Publication;
            memory.WriteImmutable(PublishedContentCatalog.ReleaseSetKey("player", FightMatchHostSession.ChapterReleaseSetId),
                Take(PublishedContentCodec.EncodeReleaseSet(new ContentReleaseSet { SchemaVersion = 1, Scope = "player",
                    ReleaseSetId = FightMatchHostSession.ChapterReleaseSetId, Binding = ContentBindingRecord.From(publication.Binding),
                    PublicationReceiptSha256 = PublishedContentCodec.Sha256(publication.ReceiptBytes.ToArray()) }, budget)), budget.MaxRecordBytes);
            return chapterCatalog = catalog;
        }

        [Test]
        public void SharedTextAdmissionReadsOnlyTwoTextAssetsAndReleasesThemWithoutACatalog()
        {
            using (var runtime = new TextRuntime())
            {
                var controller = new FightMatchBootstrapController();
                var opened = FightMatchPlayerHost.OpenBootstrapAsync(controller, runtime, runtime.Stopped, TextRuntime.Set,
                    () => true, CancellationToken.None, FightMatchResourceBridge.OpenSharedTextAsync, textOnly: true);
                Assert.IsTrue(opened.IsCompleted);
                var result = opened.GetAwaiter().GetResult();
                Assert.IsTrue(result.IsTextReady, result.DiagnosticCode); Assert.IsFalse(result.IsDataReady); Assert.IsNull(result.Catalog);
                CollectionAssert.AreEqual(new[] { "fm.text.full.artifact", "fm.text.full.manifest" }, runtime.Requests);
                Assert.IsTrue(runtime.Leases.All(x => x.IsReleased));
                Assert.AreEqual(true, controller.Snapshot.AdmissionAccepted);
                var finished = FightMatchPlayerHost.FinishBootstrapAsync(controller, runtime, runtime.Stopped, result, () => true, CancellationToken.None);
                Assert.IsTrue(finished.IsCompleted); Assert.AreSame(result, finished.GetAwaiter().GetResult());
                Assert.IsTrue(runtime.Stopped.IsCompleted);
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                    foreach (var key in new[] { "fm.startup.mode.demo", "fm.startup.mode.chapter_one_trial" })
                        Assert.IsTrue(result.TextSource.Resolve(key, locale, Array.Empty<KeyValuePair<string, string>>()).IsSuccess);
            }
        }

        [TestCase("fm.text.full.artifact")]
        [TestCase("fm.text.full.manifest")]
        public void SharedTextWrongPinsFailBeforeAnyBusinessAssetsAreRequested(string key)
        {
            using (var runtime = new TextRuntime())
            {
                runtime.Data[key][0] ^= 1;
                var task = FightMatchResourceBridge.OpenSharedTextAsync(runtime, 1, CancellationToken.None);
                Assert.IsTrue(task.IsCompleted); var result = task.GetAwaiter().GetResult();
                Assert.AreEqual("RES_TEXT", result.DiagnosticCode); Assert.IsNull(result.TextSource); Assert.IsNull(result.Catalog);
                Assert.IsTrue(runtime.Requests.All(x => x.StartsWith("fm.text.", StringComparison.Ordinal)));
                Assert.IsTrue(runtime.Leases.All(x => x.IsReleased));
            }
        }

        [Test]
        public void StartupReusesBothButtonsAndRejectsRepeatedOrStaleSelectionCallbacks()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab");
            var root = UnityEngine.Object.Instantiate(prefab); var view = root.GetComponent<FightMatchHostView>();
            var service = Text(); var calls = new List<FightMatchStartupEntry>();
            var license = File.ReadAllText(Path.Combine(HostRig.Project, "Assets/UI/FightMatch/Fonts/OFL.txt"));
            try
            {
                view.BindEntrySelection(service, license, null, calls.Add);
                // The scene activates this prefab root before receiving pointer events.
                root.SetActive(true);
                var create = (UnityEngine.UI.Button)Get(view, "createButton");
                var next = (UnityEngine.UI.Button)Get(view, "continueButton");
                Assert.IsTrue(create.gameObject.activeInHierarchy); Assert.IsTrue(next.gameObject.activeInHierarchy);
                Assert.IsFalse(((UnityEngine.UI.Button)Get(view, "reloadButton")).gameObject.activeSelf);
                Assert.IsNull(Get(view, "session")); Assert.IsNull(view.DiagnosticCode);
                var listeners = (List<KeyValuePair<UnityEngine.UI.Button, UnityAction>>)Get(view, "listeners");
                var stale = listeners.Single(x => x.Key == create).Value;
                Assert.IsTrue(ExecuteEvents.Execute(next.gameObject, new PointerEventData(EventSystem.current)
                    { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler));
                stale(); next.onClick.Invoke();
                CollectionAssert.AreEqual(new[] { FightMatchStartupEntry.ChapterOneTrial }, calls);
                Assert.IsFalse(create.interactable); Assert.IsFalse(next.interactable);
                view.BindEntrySelection(service, license, null, calls.Add);
                stale(); Assert.AreEqual(1, calls.Count);
                Assert.IsTrue(ExecuteEvents.Execute(create.gameObject, new PointerEventData(EventSystem.current)
                    { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler));
                CollectionAssert.AreEqual(new[] { FightMatchStartupEntry.ChapterOneTrial, FightMatchStartupEntry.Demo }, calls);
            }
            finally { view.Unbind(); UnityEngine.Object.DestroyImmediate(root); }
        }

        [Test]
        public void SelectedChapterRetainsTheExistingExplicitCreateConfirmAndCancelFlow()
        {
            using (var rig = new HostRig(false, ChapterCatalog(), FightMatchStartupEntry.ChapterOneTrial))
            {
                rig.Session.ObserveStartup(); Assert.IsTrue(rig.Session.CanCreate);
                var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab"));
                var view = root.GetComponent<FightMatchHostView>(); var service = Text();
                try
                {
                    view.Bind(rig.Session, service, File.ReadAllText(Path.Combine(HostRig.Project, "Assets/UI/FightMatch/Fonts/OFL.txt")));
                    var create = (UnityEngine.UI.Button)Get(view, "createButton");
                    var confirm = (UnityEngine.UI.Button)Get(view, "quitConfirmButton");
                    var cancel = (UnityEngine.UI.Button)Get(view, "quitCancelButton");
                    var before = rig.Files(); create.onClick.Invoke();
                    Assert.IsTrue(((GameObject)Get(view, "quitPopup")).activeSelf);
                    Assert.AreEqual(service.Resolve("fm.profile.chapter_trial.create.confirm_body", null).Text,
                        ((LocalizedTmpText)Get(view, "quitBody")).Target.text);
                    Assert.AreEqual(0, rig.FactoryCalls); rig.SameFiles(before);
                    cancel.onClick.Invoke(); Assert.IsFalse(((GameObject)Get(view, "quitPopup")).activeSelf); rig.SameFiles(before);
                    create.onClick.Invoke(); confirm.onClick.Invoke();
                    Assert.AreEqual(CandidateBusinessFormat.PublishedChapterV5, rig.Head.Business.Format);
                    Assert.AreEqual(LocalPlayerProfileState.Active, rig.Session.Observation.State);
                    Assert.IsNull(view.DiagnosticCode);
                }
                finally { view.Unbind(); UnityEngine.Object.DestroyImmediate(root); }
            }
        }

        [Test]
        public void TwoRealRootsKeepOriginalDemoBindingAndChapterV5WithOneOwnerAndSharedPreference()
        {
            var chapter = ChapterCatalog();
            using (var demo = new HostRig())
            {
                demo.Enter();
                var demoActivation = new FightMatchResourceActivationStore(demo.Root, new SystemActivationFiles(), "p101-1001");
                Assert.AreEqual(ActivationOutcome.Confirmed, demoActivation.EnterAtProcessStart("fixture.demo", new string('a', 64)).Outcome);
                var oldHead = demo.Head; var oldFiles = demo.Files(); var parent = Path.GetDirectoryName(demo.Root);
                var chapterRoot = Path.Combine(parent, FightMatchHostSession.ProductDirectory(FightMatchStartupEntry.ChapterOneTrial));
                var factoryCalls = 0;
                Assert.Throws<InvalidOperationException>(() => new FightMatchHostSession(chapter, chapterRoot,
                    new MacContentPublicationStorage(Path.Combine(chapterRoot, "locator")), id => { factoryCalls++; return null; },
                    FightMatchStartupEntry.ChapterOneTrial));
                Assert.AreEqual(0, factoryCalls); Assert.AreSame(oldHead, demo.Head); demo.SameFiles(oldFiles);
                demo.Session.Dispose();
                using (var trial = new HostRig(content: chapter, entry: FightMatchStartupEntry.ChapterOneTrial, root: chapterRoot))
                {
                    Assert.AreEqual(FightMatchHostSession.ChapterReleaseSetId, trial.Session.SelectedReleaseSetId);
                    Assert.AreEqual(CandidateBusinessFormat.PublishedChapterV5, trial.Head.Business.Format);
                    Assert.AreEqual(ChapterRuleDefinitions.RuleVersion, trial.Head.Business.Roster.Characters.Single().Definition.Context.RuleVersion);
                    var chapterActivation = new FightMatchResourceActivationStore(chapterRoot, new SystemActivationFiles(), "p101-1001");
                    Assert.AreEqual(ActivationOutcome.Confirmed, chapterActivation.EnterAtProcessStart("fixture.chapter", new string('b', 64)).Outcome);
                    Assert.AreEqual("fixture.demo", demoActivation.Read().Snapshot.Active.ResourceSetId);
                    Assert.AreEqual("fixture.chapter", chapterActivation.Read().Snapshot.Active.ResourceSetId);
                    var id = trial.Head.Business.PlayerId; var files = trial.Files();
                    trial.Rebuild(); Assert.AreEqual(id, trial.Head.Business.PlayerId); trial.SameFiles(files);
                    demo.SameFiles(oldFiles);
                    var preferences = new FileLocalePreferenceStore(parent, new LocalePreferenceFiles());
                    Assert.AreEqual(LocalePreferenceSaveDisposition.Saved, preferences.Save(LocaleId.ZhHans).Disposition);
                    Assert.AreEqual(LocaleId.ZhHans, new FileLocalePreferenceStore(parent, new LocalePreferenceFiles()).Load(SystemLanguage.English).Locale);
                    Assert.IsFalse(Directory.Exists(Path.Combine(chapterRoot, "settings")));
                }
                demo.Bind(); demo.Session.ObserveStartup();
                Assert.AreEqual(oldHead.Header.CommitId, demo.Head.Header.CommitId);
                Assert.AreEqual("demo-r1", demo.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry.Context.RuleVersion);
                Assert.AreEqual(FightMatchHostPage.Battle, demo.Session.Page);
                // The only intentionally shared write is the pre-existing language preference path.
                var after = demo.Files();
                foreach (var row in oldFiles) CollectionAssert.AreEqual(row.Value, after[row.Key], row.Key);
                Assert.AreEqual(oldFiles.Count + 1, after.Count);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ChapterUnknownCreationContinuesTheSamePlayerAndOperation(bool initialized)
        {
            using (var rig = new HostRig(false, ChapterCatalog(), FightMatchStartupEntry.ChapterOneTrial))
            {
                rig.Profile.FailCreateAfterWrite = !initialized; rig.Profile.FailActive = initialized;
                rig.Session.ObserveStartup(); rig.Session.CreateProfile();
                var original = rig.Session.OriginalProfile.CreateRecord;
                Assert.AreEqual(LocalPlayerProfileState.CreateIntentRecorded, rig.Session.Observation.State);
                var commit = rig.Head?.Header.CommitId;
                Assert.AreEqual(initialized, commit != null);
                rig.Profile.FailActive = false; rig.Rebuild();
                Assert.IsFalse(rig.Session.CanCreate); Assert.IsNull(rig.Session.OriginalProfile);
                rig.Session.ContinueCreation();
                Assert.AreEqual(original.RecordSha256, rig.Session.OriginalProfile.CreateRecord.RecordSha256);
                Assert.AreEqual(original.PlayerId, rig.Head.Business.PlayerId);
                Assert.AreEqual(CandidateBusinessFormat.PublishedChapterV5, rig.Head.Business.Format);
                if (initialized) Assert.AreEqual(commit, rig.Head.Header.CommitId);
                var files = rig.Files(); rig.Session.ContinueCreation(); rig.Session.ObserveStartup(); rig.SameFiles(files);
            }
        }

        private sealed class ContentMemory : IContentPublicationStorage
        {
            private readonly Dictionary<string, byte[]> data = new Dictionary<string, byte[]>();
            private sealed class Lease : IDisposable { public void Dispose() { } }
            public IDisposable AcquireWriter() => new Lease();
            public byte[] Read(string key, int limit) => data.TryGetValue(key, out var value) ? (byte[])value.Clone() : null;
            public void WriteImmutable(string key, byte[] value, int limit)
            {
                Assert.LessOrEqual(value.Length, limit);
                if (data.TryGetValue(key, out var old)) { CollectionAssert.AreEqual(old, value); return; }
                data.Add(key, (byte[])value.Clone());
            }
        }

        private sealed class TextRuntime : IFightMatchResourceRuntime, IFightMatchAssetProvider
        {
            internal const string Set = "fixture.chapter.text";
            internal readonly Dictionary<string, byte[]> Data = new Dictionary<string, byte[]> {
                { "fm.text.full.artifact", File.ReadAllBytes(Path.Combine(TextRoot, "fm-text-v1.json")) },
                { "fm.text.full.manifest", File.ReadAllBytes(Path.Combine(TextRoot, "fm-text-v1.manifest.json")) } };
            internal readonly List<string> Requests = new List<string>();
            internal readonly List<RawLease> Leases = new List<RawLease>();
            private readonly TaskCompletionSource<bool> stopped = new TaskCompletionSource<bool>();
            internal Task Stopped => stopped.Task;
            public IFightMatchAssetProvider Assets => this;
            public Task<ResourceInspection> InspectAsync(long epoch, CancellationToken token) => Task.FromResult(
                ResourceInspection.Accepted(new string('1', 32), new string('2', 32), Set, epoch, 0, 0, false, ResourceNetworkKind.Offline));
            public Task<ResourceTransportResult> PrepareAsync(ResourceDownloadPermit permit, IProgress<ResourceProgress> progress,
                CancellationToken token) => Task.FromResult(ResourceTransportResult.Verified(permit, new string('3', 32)));
            public Task<FightMatchAssetAcquireResult<T>> AcquireAsync<T>(FightMatchAssetId id, string set, AssetAcquireBudget budget, long epoch) where T : class
            {
                Requests.Add(id.Value); Assert.IsFalse(stopped.Task.IsCompleted);
                var lease = new RawLease(id, set, Data[id.Value]); Leases.Add(lease);
                return Task.FromResult(FightMatchAssetAcquireResult<T>.Accepted((IFightMatchAssetLease<T>)(object)lease, epoch, set));
            }
            public void Dispose() { foreach (var lease in Leases) lease.Dispose(); stopped.TrySetResult(true); }
        }

        private sealed class RawLease : IFightMatchAssetLease<FightMatchRawBytes>
        {
            private readonly object buffer;
            private readonly FightMatchRawBytes bytes;
            public FightMatchAssetId AssetId { get; }
            public string ReleaseSetId { get; }
            public string LeaseId { get; } = Guid.NewGuid().ToString("N");
            public bool IsReleased { get; private set; }
            public FightMatchRawBytes Asset => IsReleased ? null : bytes;
            internal RawLease(FightMatchAssetId id, string set, byte[] data)
            {
                AssetId = id; ReleaseSetId = set;
                var type = typeof(FightMatchRawBytes).Assembly.GetType("FightMatch.AssetAccess.RawBuffer", true);
                buffer = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new object[] { (byte[])data.Clone() }, null);
                bytes = (FightMatchRawBytes)Activator.CreateInstance(typeof(FightMatchRawBytes), BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new object[] { buffer, (Func<bool>)(() => IsReleased) }, null);
            }
            public void Dispose()
            {
                if (IsReleased) return; IsReleased = true;
                buffer.GetType().GetMethod("Release", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(buffer, null);
            }
        }
    }
}
