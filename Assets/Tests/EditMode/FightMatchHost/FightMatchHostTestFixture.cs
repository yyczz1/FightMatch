using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Core;
using FightMatch.Platform;
using FlowPuzzle.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using FightMatch.Presentation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FightMatch.Host.Tests
{
    internal sealed class HostRig : IDisposable
    {
        private static PublishedContentCatalog catalog;
        internal static string Project => Path.GetDirectoryName(UnityEngine.Application.dataPath);
        internal static PublishedContentCatalog Catalog
        {
            get
            {
                if (catalog != null) return catalog;
                var files = FightMatchStreamingAssetsLoader.FileNames.Select(name => File.ReadAllBytes(
                    Path.Combine(UnityEngine.Application.streamingAssetsPath, "FightMatch", name))).ToArray();
                var result = FirstReleaseContentStorage.Create(files[0], files[1], files[2], files[3], files[4], files[5],
                    ContentConsumerCapabilities.Current, new ContentStoreBudget(new ExactMathBudget(maxPrimitiveSteps: 64000000)));
                Assert.IsTrue(result.IsAccepted, result.RejectionCode);
                return catalog = new PublishedContentCatalog(result.Value, ContentConsumerCapabilities.Current);
            }
        }
        internal readonly string Root;
        internal readonly ProfileFault Profile;
        internal SaveFault Storage;
        internal FightMatchHostSession Session;
        internal int FactoryCalls;
        internal Action BeforeStorage;
        internal CandidateApplicationSnapshot Head => Session.Application.QueryView().View.PublishedSnapshot;
        internal BattleSnapshot State => Head?.Business.ActiveHistory?.CurrentRun.CurrentSnapshot;
        internal HostRig(bool create = true)
        {
            var parent = Path.Combine(Project, "TestArtifacts/FightMatch/UGUI-01/host-io");
            Directory.CreateDirectory(parent);
            Assert.Less(Directory.GetDirectories(parent).Length, 256);
            Root = Path.Combine(parent, Guid.NewGuid().ToString("N"), "FightMatch");
            Directory.CreateDirectory(Root);
            Profile = new ProfileFault(new MacContentPublicationStorage(Path.Combine(Root, "locator")));
            Bind();
            if (create) Create();
        }
        internal void Bind()
        {
            Session = new FightMatchHostSession(Catalog, Root, Profile, id =>
            {
                FactoryCalls++;
                BeforeStorage?.Invoke();
                if (Storage == null) Storage = new SaveFault(new MacEditorSaveStorage(Path.Combine(Root, "profiles"), id, SavePurpose.PlayerSave));
                Assert.AreEqual(id, Storage.Profile.PlayerId);
                return Storage;
            });
        }
        internal void Create()
        {
            Session.ObserveStartup();
            Assert.IsTrue(Session.CanCreate, Session.Status);
            Session.CreateProfile();
            Assert.AreEqual(LocalPlayerProfileState.Active, Session.Observation.State, Session.Status);
            Assert.IsTrue(Session.Application.QueryView().View.IsPublishedHeadVerified);
        }
        internal void Rebuild()
        {
            Session.Dispose();
            Session = null;
            Storage = null;
            Bind();
            Session.ObserveStartup();
        }
        internal void Go(PlayerNavigationTargetKind kind, string operation = null)
        {
            Session.Navigation.NavigationHandler(new PlayerNavigationTarget { Kind = kind, OperationId = operation })();
        }
        internal PlayerNavigationHostRequest Enter()
        {
            var nav = Session.Navigation;
            nav.Refresh();
            var level = nav.View.Read.Levels[0];
            nav.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Preparation,
                LevelId = level.LevelId, LevelVersion = level.LevelVersion, Binding = nav.View.Read.Binding })();
            PlayerNavigationHostRequest request = null;
            Action<PlayerNavigationHostRequest> capture = value => request = value;
            nav.HostRequested += capture;
            Go(PlayerNavigationTargetKind.BattleSelectionRequested);
            nav.HostRequested -= capture;
            Assert.IsNotNull(request);
            Assert.IsNotNull(State, Session.Status);
            Assert.AreEqual(FightMatchHostPage.Battle, Session.Page);
            return request;
        }
        internal FightMatch.Application.PlayerBattleView End(FightMatchHostView view, bool restart = false)
        {
            var battle = Session.Battle;
            var preview = battle.PreviewEnd(view.BattleView.Page, restart ? CandidateApplicationKind.RestartAttempt :
                CandidateApplicationKind.ExitAttempt, battle.Refresh().Context);
            Assert.IsNotNull(preview.Confirmation, preview.Status);
            return battle.Confirm(view.BattleView.Page, preview.Confirmation);
        }
        internal Dictionary<string, byte[]> Files() => Directory.GetFiles(Root, "*", SearchOption.AllDirectories)
            .ToDictionary(x => x.Substring(Root.Length), File.ReadAllBytes);
        internal void SameFiles(Dictionary<string, byte[]> expected)
        {
            var actual = Files();
            CollectionAssert.AreEquivalent(expected.Keys, actual.Keys);
            foreach (var row in expected) CollectionAssert.AreEqual(row.Value, actual[row.Key], row.Key);
        }
        internal IReadOnlyList<FlowPos> Route(BattlePairKey pair)
        {
            var decoded = PublishedContentCodec.DecodeSource(File.ReadAllBytes(Path.Combine(UnityEngine.Application.streamingAssetsPath,
                "FightMatch/first-release.fmsource.json")), ContentConsumerCapabilities.Current, Session.Budget.Codec.Math);
            Assert.IsTrue(decoded.IsAccepted, decoded.RejectionCode);
            var source = decoded.Value;
            var level = source.Levels.Single(x => x.Level.LevelId == State.Baseline.Entry.Level.LevelId);
            var face = State.Baseline.Entry.Level.Faces.Single(x => x.FaceId == pair.FaceId);
            return level.SourceRoutes.Single(x => x.FaceId == pair.FaceId && x.PairId == pair.PairId).Cells.Select(c =>
                new FlowPos(c.x - 1, source.Coordinates == DemoCoordinateCandidate.AssumedBottomLeft ? c.y - 1 : face.Height - c.y)).ToArray();
        }
        public void Dispose() { Session?.Dispose(); }
    }

    internal sealed class ProfileFault : IContentPublicationStorage
    {
        private readonly IContentPublicationStorage inner;
        internal bool ReadFailure, FailCreateAfterWrite, FailActive;
        internal Action Writing;
        internal ProfileFault(IContentPublicationStorage inner) { this.inner = inner; }
        public IDisposable AcquireWriter() => inner.AcquireWriter();
        public byte[] Read(string key, int limit)
        {
            if (ReadFailure) throw new IOException("029 isolated locator read failure");
            return inner.Read(key, limit);
        }
        public void WriteImmutable(string key, byte[] bytes, int limit)
        {
            Writing?.Invoke();
            if (FailActive && key == LocalPlayerProfileLocator.ActiveRecordKey) throw new IOException("029 before Active");
            inner.WriteImmutable(key, bytes, limit);
            if (FailCreateAfterWrite && key == LocalPlayerProfileLocator.CreateRecordKey)
            {
                FailCreateAfterWrite = false;
                throw new IOException("029 after CreateIntent");
            }
        }
    }

    internal sealed class SaveFault : ILocalSaveStorage
    {
        private readonly ILocalSaveStorage inner;
        internal string Fault;
        internal SaveFault(ILocalSaveStorage inner) { this.inner = inner; }
        public SaveStorageProfile Profile => inner.Profile;
        public IDisposable AcquireWriterLease(bool create) => inner.AcquireWriterLease(create);
        public IEnumerable<string> EnumerateNames() => inner.EnumerateNames();
        public Stream OpenRead(string name) => inner.OpenRead(name);
        public Stream CreateWork(string name)
        {
            if (Fault == "save" && name.EndsWith(".snapshot.tmp", StringComparison.Ordinal))
            {
                Fault = null;
                throw new IOException("029 before snapshot");
            }
            return inner.CreateWork(name);
        }
        public void FlushFile(Stream stream) => inner.FlushFile(stream);
        public void PromoteNoReplace(string work, string final)
        {
            inner.PromoteNoReplace(work, final);
            if (Fault == "snapshot-after" && final.EndsWith(".snapshot", StringComparison.Ordinal))
            {
                Fault = null;
                throw new IOException("029 snapshot promoted before marker");
            }
            if (Fault == "marker-after" && final.EndsWith(".commit", StringComparison.Ordinal))
            {
                Fault = null;
                throw new IOException("029 marker published, response lost");
            }
        }
        public void DeleteUncommitted(string name) => inner.DeleteUncommitted(name);
        public void DeleteIndexedOld(string name) => inner.DeleteIndexedOld(name);
    }

    internal sealed class HostPanel : IDisposable
    {
        private readonly HostRig rig;
        private readonly GameObject eventRoot;
        private readonly UnityEngine.EventSystems.EventSystem[] previousEvents;
        private readonly HostPointerInput input;
        private readonly FightMatchStandaloneInputModule module;
        private bool disposed;
        internal FightMatchHostView View { get; private set; }
        internal GameObject Root { get; private set; }
        internal LocalizationService Localization { get; }
        internal HostPanel(HostRig rig)
        {
            this.rig = rig;
            Localization = new LocalizationService(new HostMemoryTextSource(), SystemLanguage.English);
            previousEvents = UnityEngine.Object.FindObjectsOfType<UnityEngine.EventSystems.EventSystem>().Where(x => x.enabled).ToArray();
            foreach (var previous in previousEvents) previous.enabled = false;
            eventRoot = new GameObject("HostTestEventSystem"); eventRoot.SetActive(false);
            eventRoot.AddComponent<UnityEngine.EventSystems.EventSystem>().sendNavigationEvents = false;
            module = eventRoot.AddComponent<FightMatchStandaloneInputModule>();
            input = eventRoot.AddComponent<HostPointerInput>(); module.inputOverride = input;
            module.runInEditMode = true; eventRoot.SetActive(true);
            Rebind();
        }
        internal void Rebind()
        {
            if (Root != null) { View.Unbind(); UnityEngine.Object.DestroyImmediate(Root); }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab");
            Assert.IsNotNull(prefab, "The generated Root Prefab is required.");
            Root = UnityEngine.Object.Instantiate(prefab);
            Root.SetActive(false);
            var raycasters = Root.GetComponentsInChildren<UnityEngine.UI.GraphicRaycaster>(true); Assert.AreEqual(1, raycasters.Length);
            raycasters[0].runInEditMode = true;
            View = Root.GetComponent<FightMatchHostView>(); Assert.IsNotNull(View);
            View.Bind(rig.Session, Localization, File.ReadAllText(Path.Combine(HostRig.Project, "Assets/UI/FightMatch/Fonts/OFL.txt")));
            Root.SetActive(true); Canvas.ForceUpdateCanvases();
        }
        internal void Detach() { Root.SetActive(false); }
        internal IEnumerator Ready()
        {
            yield return null; yield return null; yield return null;
            Canvas.ForceUpdateCanvases();
            var board = View.BattleView.PlaybackView.InputView.Board;
            Assert.IsNotNull(board.canvas); Assert.IsNotNull(board.canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>());
            Assert.Greater(board.rectTransform.rect.width, 0); Assert.Greater(board.rectTransform.rect.height, 0);
            Assert.IsFalse(View.DiagnosticVisible, View.DiagnosticCode);
        }
        internal void Draw(IReadOnlyList<FlowPos> route)
        {
            var board = View.BattleView.PlaybackView.InputView.Board;
            Canvas.ForceUpdateCanvases();
            for (var i = 0; i < route.Count; i++)
            {
                var point = RectTransformUtility.WorldToScreenPoint(null, board.rectTransform.TransformPoint(board.CellCenter(route[i])));
                var data = new UnityEngine.EventSystems.PointerEventData(eventRoot.GetComponent<UnityEngine.EventSystems.EventSystem>()) { position = point };
                var hits = new List<UnityEngine.EventSystems.RaycastResult>();
                eventRoot.GetComponent<UnityEngine.EventSystems.EventSystem>().RaycastAll(data, hits);
                Assert.IsTrue(hits.Count != 0 && hits[0].gameObject == board.gameObject, "The real board must be the foremost raycast hit.");
                input.Position = point; input.Down = i == 0; input.Up = i == route.Count - 1; input.Held = !input.Up;
                module.Process(); input.Down = input.Up = false;
            }
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (View != null) View.Unbind();
            View = null;
            if (Root != null) UnityEngine.Object.DestroyImmediate(Root);
            Root = null;
            if (eventRoot != null) UnityEngine.Object.DestroyImmediate(eventRoot);
            foreach (var previous in previousEvents) if (previous != null) previous.enabled = true;
        }
    }
    internal sealed class HostPointerInput : UnityEngine.EventSystems.BaseInput
    {
        internal Vector2 Position;
        internal bool Down, Up, Held;
        public override bool mousePresent => true;
        public override Vector2 mousePosition => Position;
        public override Vector2 mouseScrollDelta => Vector2.zero;
        public override bool touchSupported => false;
        public override bool GetMouseButtonDown(int button) => button == 0 && Down;
        public override bool GetMouseButtonUp(int button) => button == 0 && Up;
        public override bool GetMouseButton(int button) => button == 0 && Held;
        public override float GetAxisRaw(string name) => 0;
        public override bool GetButtonDown(string name) => false;
    }

    internal sealed class HostMemoryTextSource : ILocalizedTextSource
    {
        // COPY FIX-01 is test input only. The product does not load this authoring draft.
        internal const int ApprovedByteCount = 64708;
        internal const int ApprovedRowCount = 263;
        internal const string ApprovedSha256 = "d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c";
        private const string ParameterPattern = @"\{([A-Za-z][A-Za-z0-9_]*)\}";
        private static Entry[] approved;
        private readonly Dictionary<string, Entry> index = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private readonly string catalogDiagnostic;
        internal IReadOnlyList<Entry> Entries { get; }
        internal int ResolveCount { get; private set; }

        internal sealed class Entry
        {
            internal readonly string Key, Chinese, English, Parameters, Severity;
            internal Entry(string key, string chinese, string english, string parameters = "", string severity = "info")
            { Key = key; Chinese = chinese; English = english; Parameters = parameters; Severity = severity; }
            internal string[] ParameterNames => string.IsNullOrEmpty(Parameters) ? Array.Empty<string>() : Parameters.Split(';');
        }

        internal HostMemoryTextSource() : this(ReadApprovedDraft()) { }
        internal HostMemoryTextSource(IEnumerable<Entry> entries)
        {
            Entries = entries.ToArray();
            foreach (var entry in Entries)
            {
                if (string.IsNullOrEmpty(entry.Key) || !entry.Key.StartsWith("fm.", StringComparison.Ordinal))
                { catalogDiagnostic = "InvalidLocalizationKey"; break; }
                if (index.ContainsKey(entry.Key)) { catalogDiagnostic = "DuplicateLocalizationKey"; break; }
                if (!TrySeverity(entry.Severity, out _)) { catalogDiagnostic = "InvalidSeverity"; break; }
                index.Add(entry.Key, entry);
            }
        }

        public LocalizedTextResult Resolve(string key, LocaleId locale, IReadOnlyList<KeyValuePair<string, string>> namedArgs)
        {
            ResolveCount++;
            if (!LocalePolicy.IsKnown(locale)) return LocalizedTextResult.Failure("UnknownLocale");
            if (catalogDiagnostic != null) return LocalizedTextResult.Failure(catalogDiagnostic);
            if (key == null || !index.TryGetValue(key, out var entry)) return LocalizedTextResult.Failure("MissingLocalizationKey");
            var template = locale == LocaleId.ZhHans ? entry.Chinese : entry.English;
            if (string.IsNullOrEmpty(template)) return LocalizedTextResult.Failure("MissingLocaleText");
            var declared = entry.ParameterNames;
            var names = Regex.Matches(template, ParameterPattern).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            var remaining = Regex.Replace(template, ParameterPattern, "");
            if (remaining.IndexOfAny(new[] { '{', '}' }) >= 0 || declared.Any(string.IsNullOrEmpty) ||
                declared.Distinct(StringComparer.Ordinal).Count() != declared.Length ||
                !new HashSet<string>(declared, StringComparer.Ordinal).SetEquals(names))
                return LocalizedTextResult.Failure("InvalidTemplate");
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var arg in namedArgs ?? Array.Empty<KeyValuePair<string, string>>())
            {
                if (string.IsNullOrEmpty(arg.Key) || arg.Value == null || values.ContainsKey(arg.Key))
                    return LocalizedTextResult.Failure("InvalidNamedArguments");
                values.Add(arg.Key, arg.Value);
            }
            if (!new HashSet<string>(declared, StringComparer.Ordinal).SetEquals(values.Keys))
                return LocalizedTextResult.Failure("ParameterMismatch");
            TrySeverity(entry.Severity, out var severity);
            return LocalizedTextResult.Success(Regex.Replace(template, ParameterPattern, m => values[m.Groups[1].Value]), severity);
        }

        private static bool TrySeverity(string value, out LocalizedTextSeverity severity)
        {
            switch (value)
            {
                case "info": severity = LocalizedTextSeverity.Info; return true;
                case "warning": severity = LocalizedTextSeverity.Warning; return true;
                case "error": severity = LocalizedTextSeverity.Error; return true;
                case "blocking": severity = LocalizedTextSeverity.Blocking; return true;
                default: severity = default; return false;
            }
        }

        private static Entry[] ReadApprovedDraft()
        {
            if (approved != null) return approved;
            var path = Path.Combine(UnityEngine.Application.dataPath, "../docs/team/2026-09-30/snapshots/ugui-copy-amend-03-fix-01/planning-localization-draft.csv");
            var bytes = File.ReadAllBytes(path);
            if (bytes.Length != ApprovedByteCount) throw new InvalidDataException("Approved COPY input is incomplete.");
            using (var sha = SHA256.Create())
                if (string.Concat(sha.ComputeHash(bytes).Select(x => x.ToString("x2"))) != ApprovedSha256)
                    throw new InvalidDataException("Approved COPY input changed; obtain the new fixed mapping before testing.");
            var rows = ReadCsv(Encoding.UTF8.GetString(bytes)).ToArray();
            if (rows.Length != ApprovedRowCount + 1 || rows.Any(r => r.Length != 9))
                throw new InvalidDataException("Approved COPY shape changed.");
            return approved = rows.Skip(1).Select(r => new Entry(r[0], r[4], r[5], r[6], r[7])).ToArray();
        }

        private static IEnumerable<string[]> ReadCsv(string text)
        {
            var field = new StringBuilder(); var fields = new List<string>(); var quoted = false;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '"')
                {
                    if (quoted && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else quoted = !quoted;
                }
                else if (!quoted && c == ',') { fields.Add(field.ToString()); field.Clear(); }
                else if (!quoted && (c == '\r' || c == '\n'))
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    fields.Add(field.ToString()); field.Clear(); yield return fields.ToArray(); fields.Clear();
                }
                else field.Append(c);
            }
            if (quoted) throw new InvalidDataException("Unclosed CSV field.");
            if (field.Length != 0 || fields.Count != 0) { fields.Add(field.ToString()); yield return fields.ToArray(); }
        }
    }

}
