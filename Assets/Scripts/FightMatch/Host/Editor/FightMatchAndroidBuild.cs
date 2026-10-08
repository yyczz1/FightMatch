using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: TestPlayerBuildModifier(typeof(FightMatch.Host.Editor.FightMatchAndroidBuild))]

namespace FightMatch.Host.Editor
{
    [InitializeOnLoad]
    public sealed class FightMatchAndroidBuild : ITestPlayerBuildModifier, IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private const string Stage = "DEMO-029-MAC-R1";
        private const string Project = "/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch";
        private const string ScenePath = "Assets/Scenes/FightMatchDemo.unity";
        private const string Ui = "Assets/UI/FightMatch/";
        private const string SettingsPath = "ProjectSettings/ProjectSettings.asset";
        private const string BuildSettingsPath = "ProjectSettings/EditorBuildSettings.asset";
        private const string Signing = "/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/AndroidBuildCache/FightMatch/signing/";
        private static readonly string[] Args = Environment.GetCommandLineArgs();
        private static Settings saved;
        private static SceneSetup[] originalScenes;
        private static string apk, runDirectory, temporaryScene;
        private static bool qa, finishing, finished, buildObserved, loggedError;
        private static BuildReport actualReport;
        public int callbackOrder => int.MaxValue;

        static FightMatchAndroidBuild()
        {
            if (!Args.Contains("-fm029QaBuildOnly") || Arg("-fm029Stage") != Stage) return;
            qa = true;
            GuardOutput(Arg("-buildPlayerPath"), "fightmatch-qa.apk");
            Require(Arg("-assemblyNames") == "FightMatch.Android.Tests" && Arg("-testPlatform") == "Android", "QA filter");
            CheckProduction();
            saved = Settings.Capture();
            originalScenes = EditorSceneManager.GetSceneManagerSetup();
            WriteSettings();
            UnityEngine.Application.logMessageReceived += OnLog;
            EditorApplication.update += ObserveQaCompletion;
            EditorApplication.wantsToQuit += RestoreBeforeQuit;
        }
        private static string Arg(string key)
        {
            var positions = Args.Select((x, i) => new { x, i }).Where(x => x.x == key).ToArray();
            if (positions.Length == 0) return null;
            Require(positions.Length == 1 && positions[0].i + 1 < Args.Length, "Duplicate or missing " + key);
            return Args[positions[0].i + 1];
        }
        private static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException("FM029: " + message);
        }
        private static string Sha(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var hash = SHA256.Create()) return string.Concat(hash.ComputeHash(stream).Select(x => x.ToString("x2")));
        }
        private static void GuardOutput(string output, string leaf)
        {
            Require(Arg("-fm029Stage") == Stage && Path.GetDirectoryName(UnityEngine.Application.dataPath) == Project, "Stage/project");
            Require(Sha(Project + "/docs/system-design/2026-09-17/demo-029-mac-r1-plan.json") ==
                "cfe1ef8ae52ba4fa9daea475d3f4571d88cdbfe47b87ce89ba7e5a8eb59bd6f1", "Plan identity");
            var prefix = leaf.EndsWith(".apk", StringComparison.Ordinal) ? "/Builds/FMDemo029/mac-r1/" : "/TestArtifacts/FMDemo029/mac-r1/runs/";
            var match = Regex.Match(output ?? "", "^" + Regex.Escape(Project + prefix) + "(00[1-9]|01[0-9]|020)/" + Regex.Escape(leaf) + "$");
            Require(match.Success && !File.Exists(output) && !Directory.Exists(output), "Exact create-new output");
            runDirectory = Project + "/TestArtifacts/FMDemo029/mac-r1/runs/" + match.Groups[1].Value;
            Require(File.Exists(runDirectory + "/run.json"), "Missing authorized run");
            var binding = JsonUtility.FromJson<RunBinding>(File.ReadAllText(runDirectory + "/run.json"));
            var expectedMode = leaf == "fightmatch-qa.apk" ? "QaBuild" : leaf == "fightmatch-demo.apk" ? "DemoBuild" : "PrepareAssets";
            Require(binding.stageId == Stage && binding.authorTurnId == "01a0eaf3-31c0-7d83-9dfc-31c24012946f" &&
                binding.mode == expectedMode && binding.argv.Contains(output), "Current author/mode/output binding");
            Require(binding.script.sha256 == Sha(Project + "/Tools/Invoke-FM029Validation.ps1") &&
                binding.rootIdentity.sha256 == Sha(Project + "/TestArtifacts/FMDemo029/mac-r1/root-identity.json"), "Run script/root identity");
            if (leaf.EndsWith(".apk", StringComparison.Ordinal)) apk = output;
        }
        private const string UguiPrefab = Ui + "Runtime/FightMatchRuntimeRoot.prefab";
        private const string UguiFont = Ui + "Fonts/NotoSansCJKsc-Regular-TMP.asset";
        private const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        private const string Placeholder = "【if you see this, it is a bug.】";
        private const string PresentationPath = "Assets/Scripts/FightMatch/Presentation/";
        private static TMPro.TMP_FontAsset preparingFont;

        // The Editor assembly creates serialized public components through their MonoScript assets.
        // It neither exposes nor invokes Presentation's internal runtime contracts.
        private static Component ComponentFromScript(GameObject target, string file)
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(PresentationPath + file + ".cs");
            Require(script != null && script.GetClass() != null && typeof(Component).IsAssignableFrom(script.GetClass()), "Missing uGUI component " + file);
            return target.AddComponent(script.GetClass());
        }
        private static void References(Component component, params object[] pairs)
        {
            Require(pairs.Length % 2 == 0, "Serialized reference pairs");
            var serialized = new SerializedObject(component);
            for (var i = 0; i < pairs.Length; i += 2)
            {
                var property = serialized.FindProperty((string)pairs[i]);
                Require(property != null, "Missing serialized field " + pairs[i]);
                var list = pairs[i + 1] as UnityEngine.Object[];
                if (list != null)
                {
                    property.arraySize = list.Length;
                    for (var j = 0; j < list.Length; j++) property.GetArrayElementAtIndex(j).objectReferenceValue = list[j];
                }
                else property.objectReferenceValue = (UnityEngine.Object)pairs[i + 1];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Id(GameObject target, string id)
        {
            var component = ComponentFromScript(target, "FightMatchViewId");
            var serialized = new SerializedObject(component);
            serialized.FindProperty("id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static string RowId(string id) => "fm.row." + Uri.EscapeDataString(id);
        private static RectTransform Rect(string name, Transform parent, bool fill = false)
        {
            var result = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            result.SetParent(parent, false);
            if (fill) { result.anchorMin = Vector2.zero; result.anchorMax = Vector2.one; result.offsetMin = result.offsetMax = Vector2.zero; }
            return result;
        }
        private static UnityEngine.UI.Image Surface(RectTransform target, Color color, bool raycast = false)
        {
            var image = target.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = color; image.raycastTarget = raycast;
            return image;
        }
        private static void Vertical(RectTransform target, bool fit = true)
        {
            var group = target.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            group.spacing = 8; group.padding = new RectOffset(12, 12, 12, 12);
            group.childAlignment = TextAnchor.UpperCenter;
            group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandWidth = true; group.childForceExpandHeight = false;
            if (fit)
                target.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        }
        private static RectTransform Group(string name, Transform parent, string id = null)
        {
            var rect = Rect(name, parent);
            Vertical(rect, false);
            if (id != null) Id(rect.gameObject, id);
            return rect;
        }
        private static RectTransform Scroll(string name, Transform parent)
        {
            var root = Rect(name, parent, true);
            var scroll = root.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
            var viewport = Rect("Viewport", root, true);
            Surface(viewport, Color.white, true);
            var mask = viewport.gameObject.AddComponent<UnityEngine.UI.Mask>(); mask.showMaskGraphic = false;
            var content = Rect("Content", viewport);
            content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1); content.sizeDelta = Vector2.zero;
            Vertical(content);
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
            scroll.vertical = true; scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            return content;
        }
        private static TMPro.TextMeshProUGUI Tmp(string name, Transform parent, float size = 24, float height = 48)
        {
            var rect = Rect(name, parent);
            var text = rect.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
            text.font = preparingFont; text.fontSharedMaterial = preparingFont.material;
            text.text = Placeholder; text.fontSize = size; text.color = Color.white;
            text.enableWordWrapping = true; text.richText = false; text.raycastTarget = false;
            text.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            var layout = rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            layout.minHeight = height; layout.preferredHeight = height;
            return text;
        }
        private static Component Label(string name, Transform parent, string id = null, float height = 64)
        {
            var text = Tmp(name, parent, 24, height);
            var binding = ComponentFromScript(text.gameObject, "Localization/LocalizedTmpText");
            References(binding, "target", text);
            if (id != null) Id(text.gameObject, id);
            return binding;
        }
        private static UnityEngine.UI.Button Button(string name, Transform parent, string id, bool reason = false)
        {
            var rect = Group(name, parent);
            var graphic = Surface(rect, new Color(.20f, .24f, .29f), true);
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = graphic;
            var layout = rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>(); layout.minHeight = 64; layout.preferredHeight = 72;
            Label("Caption", rect, height: 48);
            if (reason)
            {
                var detail = Label("DisabledReason", rect, height: 64);
                detail.gameObject.SetActive(false);
            }
            Id(rect.gameObject, id);
            return button;
        }
        private static TMPro.TMP_InputField Input(string name, Transform parent, string id)
        {
            var rect = Rect(name, parent);
            var graphic = Surface(rect, new Color(.10f, .13f, .17f), true);
            var layout = rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>(); layout.minHeight = layout.preferredHeight = 64;
            var area = Rect("TextArea", rect, true); area.offsetMin = new Vector2(12, 8); area.offsetMax = new Vector2(-12, -8);
            area.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var text = Tmp("Value", area); Stretch(text.rectTransform);
            var field = rect.gameObject.AddComponent<TMPro.TMP_InputField>();
            field.targetGraphic = graphic; field.textViewport = area; field.textComponent = text;
            field.contentType = TMPro.TMP_InputField.ContentType.Standard;
            field.SetTextWithoutNotify(Placeholder);
            if (id != null) Id(rect.gameObject, id);
            return field;
        }
        private static UnityEngine.UI.Toggle Toggle(string name, Transform parent, string id)
        {
            var rect = Rect(name, parent);
            var layout = rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>(); layout.minHeight = layout.preferredHeight = 56;
            var background = Rect("Box", rect); background.anchorMin = background.anchorMax = new Vector2(0, .5f);
            background.pivot = new Vector2(0, .5f); background.sizeDelta = new Vector2(48, 48);
            var image = Surface(background, new Color(.25f, .28f, .32f), true);
            var mark = Rect("Check", background, true); mark.offsetMin = new Vector2(10, 10); mark.offsetMax = new Vector2(-10, -10);
            var toggle = rect.gameObject.AddComponent<UnityEngine.UI.Toggle>();
            toggle.targetGraphic = image; toggle.graphic = Surface(mark, new Color(.55f, .80f, .72f));
            Id(rect.gameObject, id);
            return toggle;
        }
        private static TMPro.TMP_Dropdown Dropdown(string name, Transform parent, string id)
        {
            var rect = Rect(name, parent);
            var image = Surface(rect, new Color(.20f, .24f, .29f), true);
            var layout = rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>(); layout.minHeight = layout.preferredHeight = 64;
            var caption = Label("Caption", rect); Stretch((RectTransform)caption.transform);
            ((RectTransform)caption.transform).offsetMin = new Vector2(12, 0);
            var dropdown = rect.gameObject.AddComponent<TMPro.TMP_Dropdown>();
            dropdown.targetGraphic = image; dropdown.captionText = caption.GetComponent<TMPro.TextMeshProUGUI>();
            var template = Rect("Template", rect); template.anchorMin = Vector2.zero; template.anchorMax = new Vector2(1, 0);
            template.pivot = new Vector2(.5f, 1); template.sizeDelta = new Vector2(0, 240);
            Surface(template, new Color(.10f, .13f, .17f), true);
            var viewport = Rect("Viewport", template, true); viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var content = Rect("Content", viewport); content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1); content.sizeDelta = new Vector2(0, 56);
            var item = Rect("Item", content); item.anchorMin = new Vector2(0, 1); item.anchorMax = Vector2.one;
            item.pivot = new Vector2(.5f, 1); item.sizeDelta = new Vector2(0, 56);
            var toggle = item.gameObject.AddComponent<UnityEngine.UI.Toggle>();
            toggle.targetGraphic = Surface(item, new Color(.25f, .30f, .35f), true);
            var check = Rect("Check", item); check.anchorMin = check.anchorMax = new Vector2(0, .5f); check.sizeDelta = new Vector2(12, 12);
            check.anchoredPosition = new Vector2(16, 0); toggle.graphic = Surface(check, Color.white);
            var itemText = Label("ItemLabel", item); Stretch((RectTransform)itemText.transform);
            ((RectTransform)itemText.transform).offsetMin = new Vector2(32, 0);
            var scroll = template.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
            scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            dropdown.template = template; dropdown.itemText = itemText.GetComponent<TMPro.TextMeshProUGUI>();
            template.gameObject.SetActive(false);
            dropdown.ClearOptions();
            dropdown.AddOptions(new System.Collections.Generic.List<string> { Placeholder });
            Id(rect.gameObject, id);
            return dropdown;
        }
        private static void Stretch(RectTransform rect)
        { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static RectTransform Modal(string name, Transform parent, string id = null)
        {
            var rect = Rect(name, parent, true);
            Surface(rect, new Color(.05f, .07f, .10f, .98f), true);
            Vertical(rect, false);
            if (id != null) Id(rect.gameObject, id);
            rect.gameObject.SetActive(false);
            return rect;
        }
        private static GameObject BuildUguiRoot()
        {
            var root = new GameObject("FightMatchRuntimeRoot"); root.SetActive(false);
            var hostView = root.AddComponent<FightMatchHostView>();
            var canvasRect = Rect("RuntimeCanvas", root.transform, true);
            var canvas = canvasRect.gameObject.AddComponent<UnityEngine.Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasRect.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540, 960); scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            canvasRect.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            Surface(Rect("BackgroundLayer", canvasRect, true), new Color(.06f, .08f, .11f));
            var safe = Rect("SafeAreaRoot", canvasRect, true); ComponentFromScript(safe.gameObject, "SafeAreaFitter");
            var screen = Rect("ScreenLayer", safe, true); screen.offsetMax = new Vector2(0, -88);
            var hud = Rect("HudLayer", safe); hud.anchorMin = new Vector2(0, 1); hud.anchorMax = Vector2.one;
            hud.pivot = new Vector2(.5f, 1); hud.sizeDelta = new Vector2(0, 80);
            var hudLayout = hud.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            hudLayout.spacing = 8; hudLayout.padding = new RectOffset(8, 8, 4, 4);
            hudLayout.childControlWidth = hudLayout.childControlHeight = true; hudLayout.childForceExpandWidth = true;
            var popups = Rect("PopupLayer", safe, true);
            var systems = Rect("SystemLayer", safe, true);
            var templates = Rect("TemplatePool", safe, true); templates.gameObject.SetActive(false);
            var battleButton = Button("BattleButtonTemplate", templates, RowId("template.battle.button"));
            var navButton = Button("NavigationButtonTemplate", templates, RowId("template.navigation.button"), true);
            var memberButton = Button("MemberButtonTemplate", templates, RowId("template.member.button"));
            var textTemplate = Label("TextTemplate", templates, RowId("template.text"));
            battleButton.gameObject.SetActive(false); navButton.gameObject.SetActive(false);
            memberButton.gameObject.SetActive(false); textTemplate.gameObject.SetActive(false);

            var startup = Rect("StartupPage", screen, true); Id(startup.gameObject, "fm.page.startup");
            var startupRows = Scroll("StartupScroll", startup);
            var profileTitle = Label("ProfileTitle", startupRows); var startupStatus = Label("StartupStatus", startupRows, height: 128);
            var create = Button("Create", startupRows, "fm.action.profile.create");
            var continuation = Button("Continue", startupRows, "fm.action.profile.continue");
            var reload = Button("Reload", startupRows, RowId("profile.reload"));

            var navigation = Rect("NavigationPage", screen, true); Id(navigation.gameObject, "fm.page.navigation");
            var navRows = Scroll("NavigationScroll", navigation);
            var navView = ComponentFromScript(navRows.gameObject, "PlayerNavigationView");
            var navTitle = Label("Title", navRows); var navStatus = Label("Status", navRows);
            var mapButton = Button("Map", navRows, RowId("navigation.MapAdventure"), true);
            var partyButton = Button("Party", navRows, RowId("navigation.Team"), true);
            var inventoryButton = Button("Inventory", navRows, RowId("navigation.Bag"), true);
            var navBack = Button("Back", navRows, RowId("navigation.Back"), true);
            var cancel = Button("Cancel", navRows, RowId("navigation.Cancel"), true);
            var refresh = Button("Refresh", navRows, RowId("navigation.Refresh"), true);
            var migration = Button("Migration", navRows, RowId("navigation.Migration"), true);
            var creation = Button("Creation", navRows, RowId("navigation.Creation"), true);
            var settlement = Button("Settlement", navRows, RowId("navigation.SettlementRequired"), true);
            var map = Group("MapSection", navRows, "fm.section.map");
            var party = Group("PartySection", navRows, "fm.section.party");
            var inventory = Group("InventorySection", navRows, "fm.section.inventory");
            var preparation = Group("PreparationSection", navRows, "fm.section.preparation");
            var craft = Group("CraftSection", navRows, RowId("navigation.CraftSection"));
            var slots = new UnityEngine.Object[] {
                Dropdown("FrontSlot", party, RowId("party.slot.front")), Dropdown("MiddleSlot", party, RowId("party.slot.middle")),
                Dropdown("RearSlot", party, RowId("party.slot.rear")) };
            var partyPreview = Button("PartyPreview", party, "fm.action.party.confirm", true);
            var clear = Button("ClearEquipment", inventory, RowId("equipment.Clear"), true);
            var preference = Button("Preference", inventory, RowId("equipment.Preference"), true);
            var craftButton = Button("Craft", inventory, RowId("navigation.CraftList"), true);
            var originalResult = Button("OriginalResult", inventory, RowId("navigation.OriginalResult"), true);
            var entry = Button("Entry", preparation, "fm.action.entry.enter", true);
            var resume = Button("Resume", preparation, RowId("navigation.ResumeBattleRequested"), true);
            var recipeAvailability = Label("RecipeAvailability", craft, RowId("crafting.Availability"));
            var detailRoot = Group("PermanentDetail", navRows, RowId("permanent.Detail"));
            var detailView = ComponentFromScript(detailRoot.gameObject, "PlayerPermanentDetailView");
            var quantityLabel = Label("QuantityLabel", detailRoot); var quantity = Input("Quantity", detailRoot, RowId("permanent.Quantity"));
            var preferenceLabel = Label("PreferenceLabel", detailRoot); var enabled = Toggle("Enabled", detailRoot, RowId("permanent.Enabled"));
            var sourcesLabel = Label("SourcesLabel", detailRoot); var explicitSources = Toggle("ExplicitSources", detailRoot, RowId("permanent.ExplicitSources"));
            var sourceRows = Group("SourceRows", detailRoot);
            var sourceTemplate = Group("SourceTemplate", templates, RowId("template.source"));
            Label("Description", sourceTemplate); Label("StartLabel", sourceTemplate);
            Input("Start", sourceTemplate, null); Label("CountLabel", sourceTemplate);
            Input("Count", sourceTemplate, null); sourceTemplate.gameObject.SetActive(false);
            References(detailView, "title", Label("Title", detailRoot), "character", Label("Character", detailRoot),
                "quantityLabel", quantityLabel, "preferenceLabel", preferenceLabel, "explicitSourcesLabel", sourcesLabel,
                "inputError", Label("InputError", detailRoot, RowId("permanent.InputError")), "quantity", quantity,
                "enabledToggle", enabled, "explicitSources", explicitSources, "sourceRows", sourceRows,
                "sourceTemplate", sourceTemplate.gameObject, "previewButton", Button("Preview", detailRoot, RowId("permanent.Preview"), true),
                "textTemplate", textTemplate);

            var confirmationContainer = Rect("ConfirmationPopup", popups, true); Id(confirmationContainer.gameObject, "fm.popup.confirmation");
            var recoveryContainer = Rect("RecoveryPopup", popups, true); Id(recoveryContainer.gameObject, "fm.popup.recovery");
            var navConfirmation = Modal("NavigationConfirmation", confirmationContainer);
            var navRecovery = Modal("NavigationRecovery", recoveryContainer);
            var recoveryView = ComponentFromScript(popups.gameObject, "PlayerNavigationRecoveryView");
            References(recoveryView, "confirmationRoot", navConfirmation.gameObject, "recoveryRoot", navRecovery.gameObject,
                "confirmationRows", navConfirmation, "recoveryRows", navRecovery,
                "title", Label("Title", navRecovery), "status", Label("Status", navRecovery),
                "confirmButton", Button("Confirm", navConfirmation, RowId("save.Confirm"), true),
                "endButton", Button("End", navConfirmation, RowId("save.End"), true),
                "returnButton", Button("Return", navRecovery, RowId("save.Return"), true),
                "resumeObservedButton", Button("ResumeObserved", navRecovery, RowId("save.ResumeObserved"), true),
                "retryButton", Button("Retry", navRecovery, RowId("save.Retry"), true),
                "resolveButton", Button("Resolve", navRecovery, RowId("save.Resolve"), true),
                "refreshButton", Button("Refresh", navRecovery, RowId("save.Refresh"), true),
                "endReviewButton", Button("EndReview", navRecovery, RowId("save.EndReview"), true),
                "originalResultButton", Button("OriginalResult", navRecovery, RowId("save.OriginalResult"), true),
                "buttonTemplate", navButton, "textTemplate", textTemplate);
            References(navView, "mapSection", map, "partySection", party, "inventorySection", inventory,
                "preparationSection", preparation, "craftSection", craft, "title", navTitle, "status", navStatus,
                "recipeAvailability", recipeAvailability, "mapButton", mapButton, "partyButton", partyButton,
                "inventoryButton", inventoryButton, "backButton", navBack, "cancelButton", cancel,
                "refreshButton", refresh, "migrationButton", migration, "creationButton", creation,
                "settlementButton", settlement, "entryButton", entry, "resumeButton", resume,
                "partyPreviewButton", partyPreview, "craftButton", craftButton, "clearEquipmentButton", clear,
                "preferenceButton", preference, "originalResultButton", originalResult, "partySlots", slots,
                "detail", detailView, "recovery", recoveryView, "buttonTemplate", navButton, "textTemplate", textTemplate);

            var battle = Rect("BattlePage", screen, true); Id(battle.gameObject, "fm.page.battle");
            var battleView = ComponentFromScript(battle.gameObject, "PlayerBattleView");
            var battleContent = Rect("BattleContent", battle, true);
            var battleRows = Scroll("BattleScroll", battleContent);
            var headline = Label("Headline", battleRows);
            var status = Group("BattleStatus", battleRows);
            var playback = ComponentFromScript(status.gameObject, "CandidateBattlePlaybackView");
            var input = ComponentFromScript(status.gameObject, "CandidateBoardInputView");
            var boardRect = Rect("BoardFrame", status);
            var boardLayout = boardRect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            boardLayout.minHeight = boardLayout.preferredHeight = 420;
            var board = ComponentFromScript(boardRect.gameObject, "CandidateBoardElement");
            ((UnityEngine.UI.Graphic)board).raycastTarget = true; Id(board.gameObject, "fm.board.candidate");
            References(input, "board", board, "phase", Label("Phase", status), "save", Label("Save", status),
                "enemies", Label("Enemies", status, RowId("board.enemies"), height: 96), "availability", Label("Availability", status, RowId("board.availability")),
                "members", Group("Members", status), "memberTemplate", memberButton,
                "retry", Button("Retry", status, "fm.action.battle.retry"), "resolve", Button("Resolve", status, "fm.action.battle.resolve"));
            References(playback, "inputView", input, "hp", Label("Hp", status, RowId("playback.hp")), "intent", Label("Intent", status, RowId("playback.intent")),
                "beat", Label("Beat", status), "stage", Label("Stage", status), "diagnostic", Label("Diagnostic", status),
                "skip", Button("Skip", status, "fm.action.battle.skip"));
            var battleHud = Label("BattleHud", battleRows, RowId("battle.hud"));
            var reference = Modal("ReferencePopup", popups, "fm.popup.reference");
            reference.anchorMax = new Vector2(1, .42f);
            var referenceView = ComponentFromScript(reference.gameObject, "PlayerDefaultReferenceView");
            var closeReference = Button("CloseTemplate", reference, RowId("template.reference.close")); closeReference.gameObject.SetActive(false);
            References(referenceView, "explanation", Label("Explanation", reference, RowId("reference.explanation"), height: 96), "beat", Label("Beat", reference),
                "steps", Group("Steps", reference), "buttonTemplate", battleButton, "closeButton", closeReference);
            var result = Rect("ResultPage", screen, true); Id(result.gameObject, "fm.page.result");
            var resultRows = Scroll("ResultScroll", result);
            References(battleView, "battleContent", battleContent.gameObject, "resultRoot", result.gameObject,
                "playbackView", playback, "referenceView", referenceView, "headline", headline, "hud", battleHud,
                "actions", Group("Actions", battleRows), "history", Group("History", battleRows),
                "dialog", Modal("BattleConfirmation", confirmationContainer), "recovery", Modal("BattleRecovery", recoveryContainer),
                "receipt", resultRows, "buttonTemplate", battleButton, "textTemplate", textTemplate);

            var language = Modal("LanguagePopup", popups, "fm.popup.language");
            var languageTitle = Label("Title", language); var feedback = Label("Feedback", language);
            var english = Button("English", language, RowId("language.en")); var chinese = Button("Chinese", language, RowId("language.zhHans"));
            var languageClose = Button("Close", language, "fm.action.close");
            var license = Modal("LicensePopup", popups, "fm.popup.license");
            var licenseTitle = Label("Title", license);
            var licenseScrollRect = Rect("LicenseScrollContainer", license);
            var licenseLayout = licenseScrollRect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            licenseLayout.minHeight = 240; licenseLayout.flexibleHeight = 1;
            var licenseRows = Scroll("LicenseScroll", licenseScrollRect);
            var licenseBody = Tmp("LicenseBody", licenseRows, 20, 2000);
            var licenseClose = Button("Back", license, RowId("license.Back"));
            var quit = Modal("QuitPopup", popups, "fm.popup.quit");
            var quitTitle = Label("Title", quit); var quitBody = Label("Body", quit, height: 128);
            var quitConfirm = Button("Confirm", quit, "fm.action.quit.confirm"); var quitCancel = Button("Stay", quit, RowId("quit.Stay"));
            var loading = Modal("LoadingOverlay", systems, RowId("system.Loading")); var loadingLabel = Label("Loading", loading);
            var diagnostic = Modal("BlockingDiagnostic", systems, "fm.popup.blocking"); var diagnosticText = Label("Diagnostic", diagnostic, height: 192);
            References(hostView, "startupPage", startup.gameObject, "navigationPage", navigation.gameObject,
                "battlePage", battle.gameObject, "resultPage", result.gameObject, "languagePopup", language.gameObject,
                "licensePopup", license.gameObject, "quitPopup", quit.gameObject, "loadingOverlay", loading.gameObject,
                "blockingDiagnostic", diagnostic.gameObject, "navigationView", navView, "battleView", battleView, "board", board,
                "profileTitle", profileTitle, "startupStatus", startupStatus, "languageTitle", languageTitle, "languageFeedback", feedback,
                "licenseTitle", licenseTitle, "quitTitle", quitTitle, "quitBody", quitBody, "loadingLabel", loadingLabel,
                "diagnosticText", diagnosticText, "licenseBody", licenseBody, "createButton", create, "continueButton", continuation,
                "reloadButton", reload, "backButton", Button("Back", hud, "fm.action.back"),
                "languageButton", Button("Language", hud, "fm.action.language.open"), "languageEnglishButton", english,
                "languageChineseButton", chinese, "languageCloseButton", languageClose,
                "licenseButton", Button("License", hud, RowId("license.Open")), "licenseCloseButton", licenseClose,
                "quitConfirmButton", quitConfirm, "quitCancelButton", quitCancel);
            navigation.gameObject.SetActive(false); battle.gameObject.SetActive(false); result.gameObject.SetActive(false);
            return root;
        }

        // Frozen from the approved hierarchy, not from the migrated observations. Paths are relative to SafeAreaRoot.
        private const string LayoutTextTargets = @"HudLayer/TopBar/Back/Caption|Binding
HudLayer/TopBar/Language/Caption|Binding
HudLayer/TopBar/License/Caption|Binding
HudLayer/TopBar/Title|Binding
PopupLayer/ConfirmationPopup/RootMask/BattleConfirmationPanel/Header/Title|Binding
PopupLayer/ConfirmationPopup/RootMask/NavigationConfirmationPanel/Actions/DangerActions/End/Caption|Binding
PopupLayer/ConfirmationPopup/RootMask/NavigationConfirmationPanel/Actions/DangerActions/End/DisabledReason|Binding
PopupLayer/ConfirmationPopup/RootMask/NavigationConfirmationPanel/Actions/SafeActions/Confirm/Caption|Binding
PopupLayer/ConfirmationPopup/RootMask/NavigationConfirmationPanel/Actions/SafeActions/Confirm/DisabledReason|Binding
PopupLayer/ConfirmationPopup/RootMask/NavigationConfirmationPanel/BodyViewport/Viewport/Content/StateRows/Status|Binding
PopupLayer/ConfirmationPopup/RootMask/NavigationConfirmationPanel/Header/Title|Binding
PopupLayer/LanguagePopup/RootMask/DialogPanel/Actions/Chinese/Caption|Binding
PopupLayer/LanguagePopup/RootMask/DialogPanel/Actions/Close/Caption|Binding
PopupLayer/LanguagePopup/RootMask/DialogPanel/Actions/English/Caption|Binding
PopupLayer/LanguagePopup/RootMask/DialogPanel/BodyViewport/Viewport/Content/Feedback|Binding
PopupLayer/LanguagePopup/RootMask/DialogPanel/Header/Title|Binding
PopupLayer/LicensePopup/RootMask/DialogPanel/Actions/Back/Caption|Binding
PopupLayer/LicensePopup/RootMask/DialogPanel/BodyViewport/Viewport/Content/LicenseBody|License
PopupLayer/LicensePopup/RootMask/DialogPanel/Header/Title|Binding
PopupLayer/QuitPopup/RootMask/DialogPanel/Actions/Confirm/Caption|Binding
PopupLayer/QuitPopup/RootMask/DialogPanel/Actions/Stay/Caption|Binding
PopupLayer/QuitPopup/RootMask/DialogPanel/BodyViewport/Viewport/Content/Body|Binding
PopupLayer/QuitPopup/RootMask/DialogPanel/Header/Title|Binding
ScreenLayer/BattlePage/BattleContent/BattleStatus/Beat|Binding
ScreenLayer/BattlePage/BattleContent/BattleStatus/Phase|Binding
ScreenLayer/BattlePage/BattleContent/BattleStatus/PlaybackStage|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/HistoryDrawer/Header/Close/Caption|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/HistoryDrawer/Header/Title|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/MainRow/CommandStrip/Viewport/Content/FixedBattleActions/HistoryOpen/Caption|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/MainRow/CommandStrip/Viewport/Content/FixedBattleActions/Resolve/Caption|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/MainRow/CommandStrip/Viewport/Content/FixedBattleActions/Retry/Caption|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/MainRow/CommandStrip/Viewport/Content/FixedBattleActions/Skip/Caption|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/StatusViewport/Viewport/Content/AvailabilityFirstLine|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/StatusViewport/Viewport/Content/BattleHudFirstLine|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/StatusViewport/Viewport/Content/PlaybackDiagnostic|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/NormalHud/StatusViewport/Viewport/Content/Save|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/ReferenceInfoDrawer/BodyViewport/Viewport/Content/Beat|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/ReferenceInfoDrawer/BodyViewport/Viewport/Content/Explanation|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/ReferenceInfoDrawer/Header/Close/Caption|Binding
ScreenLayer/BattlePage/BattleContent/BottomHud/ReferenceInfoDrawer/Header/Title|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot0/Hp|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot0/Intent|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot0/Name|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot1/Hp|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot1/Intent|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot1/Name|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot2/Hp|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot2/Intent|Binding
ScreenLayer/BattlePage/BattleContent/Stage/AllySlots/ActorSlot2/Name|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot0/Hp|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot0/Intent|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot0/Name|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot1/Hp|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot1/Intent|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot1/Name|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot2/Hp|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot2/Intent|Binding
ScreenLayer/BattlePage/BattleContent/Stage/EnemySlots/ActorSlot2/Name|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Back/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Back/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Cancel/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Cancel/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/CraftSection/RecipeAvailability|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Creation/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Creation/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Inventory/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Inventory/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/ClearEquipment/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/ClearEquipment/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/Craft/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/Craft/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/OriginalResult/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/OriginalResult/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/Preference/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/InventorySection/Preference/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Map/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Map/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Migration/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Migration/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Party/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Party/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/FrontSlot/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/FrontSlot/Template/Viewport/Content/Item/ItemLabel|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/MiddleSlot/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/MiddleSlot/Template/Viewport/Content/Item/ItemLabel|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/PartyPreview/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/PartyPreview/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/RearSlot/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PartySection/RearSlot/Template/Viewport/Content/Item/ItemLabel|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/Character|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/InputError|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/PreferenceLabel|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/Preview/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/Preview/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/Quantity/TextArea/Value|Input
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/QuantityLabel|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/SourcesLabel|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PermanentDetail/Title|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PreparationSection/Entry/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PreparationSection/Entry/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PreparationSection/Resume/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/PreparationSection/Resume/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Refresh/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Refresh/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Settlement/Caption|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Settlement/DisabledReason|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Status|Binding
ScreenLayer/NavigationPage/NavigationScroll/Viewport/Content/Title|Binding
ScreenLayer/StartupPage/StartupScroll/Viewport/Content/Continue/Caption|Binding
ScreenLayer/StartupPage/StartupScroll/Viewport/Content/Create/Caption|Binding
ScreenLayer/StartupPage/StartupScroll/Viewport/Content/ProfileTitle|Binding
ScreenLayer/StartupPage/StartupScroll/Viewport/Content/Reload/Caption|Binding
ScreenLayer/StartupPage/StartupScroll/Viewport/Content/StartupStatus|Binding
SystemLayer/BlockingDiagnostic/Diagnostic|Binding
SystemLayer/LayoutDiagnostic/Message|Binding
SystemLayer/LoadingOverlay/Loading|Binding
SystemLayer/RecoveryScreen/RootMask/BattleRecoveryPanel/Header/Title|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/DangerActions/EndReview/Caption|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/DangerActions/EndReview/DisabledReason|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Refresh/Caption|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Refresh/DisabledReason|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Resolve/Caption|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Resolve/DisabledReason|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/ResumeObserved/Caption|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/ResumeObserved/DisabledReason|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Retry/Caption|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Retry/DisabledReason|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Return/Caption|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Actions/SafeActions/Return/DisabledReason|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/BodyViewport/Viewport/Content/StateRows/Status|Binding
SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel/Header/Title|Binding
TemplatePool/BattleButtonTemplate/Caption|Binding
TemplatePool/MemberButtonTemplate/Caption|Binding
TemplatePool/NavigationButtonTemplate/Caption|Binding
TemplatePool/NavigationButtonTemplate/DisabledReason|Binding
TemplatePool/SourceTemplate/Count/TextArea/Value|Input
TemplatePool/SourceTemplate/CountLabel|Binding
TemplatePool/SourceTemplate/Description|Binding
TemplatePool/SourceTemplate/Start/TextArea/Value|Input
TemplatePool/SourceTemplate/StartLabel|Binding
TemplatePool/TextTemplate|Binding";

        // LAYOUT-S1 is a separate, future-activated in-place migration. Never call PrepareResources.
        [Serializable] private sealed class LayoutFile
        {
            public string path, sha256;
            public long bytes;
        }
        [Serializable] private sealed class LayoutActivation
        {
            public string contract, status, mode, candidateProjectPath, sourceReceiptPath, sourceReceiptSha256;
            public string ownerThread, ownerTurn, issuerThread, issuerTurn, prefabGuid, sceneGuid;
            public string evidenceRoot;
            public string[] argv;
            public LayoutFile[] sources, resources, importedMetas, protectedInputs;
        }
        [Serializable] private sealed class LayoutResult
        {
            public string contract = "LAYOUT-S1-SYS-001";
            public string mode, activationSha256, sourceReceiptSha256, ownerTurn;
            public string prefabSha256, sceneSha256;
            public string[] targets, sceneObjectIds;
            public int textCount, inputCount;
            public bool reopened, protectedInputsUnchanged;
        }
        private sealed class LayoutPanel
        {
            internal RectTransform Root, Header, Body, State, Choices, Actions, Safe, Danger;
        }
        private static readonly string[] LayoutSourcePaths = {
            "Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs",
            "Assets/Scripts/FightMatch/Host/FightMatchHostView.cs",
            "Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs",
            "Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs",
            "Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs",
            "Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs",
            "Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs",
            "Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs",
            "Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs",
            "Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs",
            "Assets/Tests/EditMode/FightMatch/PlayerNavigationTestFixture.cs",
            "Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs",
            "Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs",
            "Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs",
            "Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs",
        };
        private static readonly string[] LayoutResourcePaths = {
            "Assets/Scenes/FightMatchDemo.unity",
            "Assets/Scripts/FightMatch/Host/HostAssemblyInfo.cs.meta",
            "Assets/Scripts/FightMatch/Presentation/FightMatchStandaloneInputModule.cs.meta",
            "Assets/Scripts/FightMatch/Presentation/FightMatchViewId.cs.meta",
            "Assets/Scripts/FightMatch/Presentation/Localization.meta",
            "Assets/Scripts/FightMatch/Presentation/Localization/LocaleId.cs.meta",
            "Assets/Scripts/FightMatch/Presentation/Localization/LocalizationService.cs.meta",
            "Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTextSource.cs.meta",
            "Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTmpText.cs.meta",
            "Assets/Scripts/FightMatch/Presentation/PresentationAssemblyInfo.cs.meta",
            "Assets/Scripts/FightMatch/Presentation/SafeAreaFitter.cs.meta",
            "Assets/Tests/EditMode/FightMatch/LocalePolicyTests.cs.meta",
            "Assets/Tests/EditMode/FightMatch/LocalizationContractTests.cs.meta",
            "Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs.meta",
            "Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs.meta",
            "Assets/TextMesh Pro.meta",
            "Assets/TextMesh Pro/Resources.meta",
            "Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt",
            "Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt.meta",
            "Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt",
            "Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt.meta",
            "Assets/TextMesh Pro/Resources/TMP Settings.asset",
            "Assets/TextMesh Pro/Resources/TMP Settings.asset.meta",
            "Assets/TextMesh Pro/Shaders.meta",
            "Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader",
            "Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader.meta",
            "Assets/TextMesh Pro/Shaders/TMP_SDF.shader",
            "Assets/TextMesh Pro/Shaders/TMP_SDF.shader.meta",
            "Assets/TextMesh Pro/Shaders/TMPro.cginc",
            "Assets/TextMesh Pro/Shaders/TMPro.cginc.meta",
            "Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc",
            "Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc.meta",
            "Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset",
            "Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset.meta",
            "Assets/UI/FightMatch/Runtime.meta",
            "Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab",
            "Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab.meta",
        };
        private const string LayoutStageRoot = Project + "/TestArtifacts/FightMatch/LAYOUT-S1-001";
        private static string LayoutCanonical(string path)
        {
            Require(!string.IsNullOrEmpty(path) && Path.IsPathRooted(path) && Path.GetFullPath(path) == path,
                "LAYOUT canonical absolute path");
            for (var current = path; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
                if (File.Exists(current) || Directory.Exists(current))
                    Require((File.GetAttributes(current) & FileAttributes.ReparsePoint) == 0, "LAYOUT symlink: " + current);
            return path;
        }
        private static string LayoutInputPath(string root, string relative)
        {
            Require(!string.IsNullOrEmpty(relative) && !Path.IsPathRooted(relative) && !relative.Contains('\\'), "LAYOUT relative input");
            var path = Path.GetFullPath(Path.Combine(root, relative));
            Require(path.StartsWith(root + "/", StringComparison.Ordinal) && path == root + "/" + relative, "LAYOUT escaped input");
            return LayoutCanonical(path);
        }
        private static void LayoutCheckSourceSet(LayoutFile[] files)
        {
            Require(files != null && files.Length == LayoutSourcePaths.Length &&
                files.All(x => x != null && !string.IsNullOrEmpty(x.path)) &&
                files.Select(x => x.path).OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(
                    LayoutSourcePaths.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal), "LAYOUT exact source scope");
        }
        private static void LayoutCheckFiles(string root, LayoutFile[] files)
        {
            Require(files != null && files.Length > 0 && files.Select(x => x.path).Distinct().Count() == files.Length, "LAYOUT input set");
            foreach (var file in files)
            {
                var path = LayoutInputPath(root, file.path);
                Require(Regex.IsMatch(file.sha256 ?? "", "^[a-f0-9]{64}$") && File.Exists(path) &&
                    new FileInfo(path).Length == file.bytes && Sha(path) == file.sha256, "LAYOUT input drift: " + file.path);
            }
        }
        private static LayoutActivation LayoutGuard(string mode)
        {
            Require(UnityEngine.Application.isBatchMode && !EditorApplication.isPlayingOrWillChangePlaymode &&
                !UnityEngine.Application.isPlaying && EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX,
                "LAYOUT isolated Mac edit-mode batch only");
            var allowed = new[] { "-fmLayoutContract", "-fmLayoutInputManifest", "-fmLayoutEvidenceRoot" };
            Require(Args.Where(x => x.StartsWith("-fm", StringComparison.Ordinal) || x.StartsWith("-ugui", StringComparison.Ordinal))
                .All(x => allowed.Contains(x)), "LAYOUT unknown managed flag");
            Require(Arg("-fmLayoutContract") == "LAYOUT-S1-SYS-001", "LAYOUT contract");
            var sceneRepair = mode == "RepairLayoutSceneOverrides";
            var executionRoot = LayoutStageRoot + (sceneRepair ? "/FIX04/N10" : "");
            Require(!sceneRepair || !Args.Contains("-quit"), "LAYOUT N10 asynchronous repair forbids -quit");
            var path = LayoutCanonical(Arg("-fmLayoutInputManifest"));
            Require(path == executionRoot + "/I/activation.json", "LAYOUT signed I activation path");
            var activation = JsonUtility.FromJson<LayoutActivation>(File.ReadAllText(path));
            Require(activation != null && activation.contract == "LAYOUT-S1-SYS-001" && activation.status == "P_ACTIVATED" &&
                activation.mode == mode && activation.ownerThread == (sceneRepair ? "01a0fdbc-bf1e-7780-8f7f-dec13d6d590c" : "01a0e404-d89d-7ab2-bece-3cd1df3fbc52") &&
                activation.issuerThread == "01a0f2e3-1a80-7671-a459-38d5c8de0e6b" &&
                !string.IsNullOrEmpty(activation.ownerTurn) && !string.IsNullOrEmpty(activation.issuerTurn), "LAYOUT execution lease");
            var candidate = LayoutCanonical(activation.candidateProjectPath);
            Require(candidate == LayoutStageRoot + "/projection" &&
                Path.GetDirectoryName(UnityEngine.Application.dataPath) == candidate && candidate != Project, "LAYOUT isolated candidate");
            Require(activation.evidenceRoot == executionRoot + "/P" && Arg("-fmLayoutEvidenceRoot") == activation.evidenceRoot,
                "LAYOUT exact evidence root");
            LayoutCanonical(activation.evidenceRoot);
            Require(!Directory.Exists(activation.evidenceRoot) && !File.Exists(activation.evidenceRoot), "LAYOUT create-once evidence root");
            Require(activation.argv != null && activation.argv.SequenceEqual(Args), "LAYOUT exact process argv lease");
            Require(LayoutCanonical(activation.sourceReceiptPath) == executionRoot + (sceneRepair ? "/S/source-receipt.json" : "/S/source-receipt.json") &&
                Sha(activation.sourceReceiptPath) == activation.sourceReceiptSha256, "LAYOUT S receipt identity");
            LayoutCheckSourceSet(activation.sources);
            Require(activation.resources != null && activation.resources.Length == 37 && activation.importedMetas != null &&
                activation.importedMetas.Length == 2, "LAYOUT frozen source/resource/meta cardinality");
            Require(activation.importedMetas.Select(x => x.path).OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[] {
                PresentationPath + "FightMatchResponsiveLayout.cs.meta", "Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs.meta" }),
                "LAYOUT exact natural meta set");
            Require(activation.resources.Select(x => x.path).OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(
                LayoutResourcePaths.OrderBy(x => x, StringComparer.Ordinal)), "LAYOUT exact protected 37 paths");
            Require(new[] { "Packages/manifest.json", "Packages/packages-lock.json", "ProjectSettings/ProjectSettings.asset",
                "ProjectSettings/EditorBuildSettings.asset", "docs/team/2026-09-30/snapshots/ugui-copy-amend-03-fix-01/planning-localization-draft.csv",
                UguiPrefab + ".meta", ScenePath + ".meta" }.All(x => activation.protectedInputs != null &&
                    activation.protectedInputs.Any(y => y.path == x)), "LAYOUT required protected input identities");
            LayoutCheckFiles(candidate, activation.sources); LayoutCheckFiles(candidate, activation.resources);
            LayoutCheckFiles(candidate, activation.importedMetas); LayoutCheckFiles(candidate, activation.protectedInputs);
            Require(activation.prefabGuid == "c36df3cfc25424c8cb3ec6cae6be1237" && activation.sceneGuid == "1d5124e5b55fe409d8216e78a117dec0" &&
                AssetDatabase.AssetPathToGUID(UguiPrefab) == activation.prefabGuid && AssetDatabase.AssetPathToGUID(ScenePath) == activation.sceneGuid,
                "LAYOUT native GUIDs");
            Require(activation.resources.Any(x => x.path == UguiPrefab) && activation.resources.Any(x => x.path == ScenePath) &&
                activation.resources.Any(x => x.path == UguiFont) && activation.protectedInputs.Any(x => x.path.EndsWith(".csv", StringComparison.Ordinal)),
                "LAYOUT prefab/scene/font/CSV preimages");
            var scene = SceneManager.GetActiveScene();
            Require(SceneManager.sceneCount == 1 && !scene.isDirty && string.IsNullOrEmpty(scene.path), "LAYOUT clean startup scene");
            Require(Resources.FindObjectsOfTypeAll<FightMatchPlayerHost>().All(x => EditorUtility.IsPersistent(x)), "LAYOUT no live Host");
            Directory.CreateDirectory(activation.evidenceRoot);
            return activation;
        }
        private static void LayoutWrite(LayoutActivation activation, string leaf, LayoutResult result)
        {
            Require(new[] { "native-migration.json", "native-reopen.json", "serialized-targets.json" }.Contains(leaf), "LAYOUT evidence leaf");
            result.activationSha256 = Sha(Arg("-fmLayoutInputManifest")); result.ownerTurn = activation.ownerTurn;
            result.sourceReceiptSha256 = activation.sourceReceiptSha256;
            using (var stream = new FileStream(Path.Combine(activation.evidenceRoot, leaf), FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(stream)) writer.Write(JsonUtility.ToJson(result, true));
        }
        private static T LayoutAt<T>(Transform root, string path) where T : Component
        {
            var target = root.Find(path); Require(target != null, "LAYOUT missing path " + path);
            var component = target.GetComponent<T>(); Require(component != null, "LAYOUT missing type " + typeof(T).Name + " at " + path);
            return component;
        }
        private static Component LayoutScript(Transform root, string path, string script)
        {
            var target = root.Find(path); Require(target != null, "LAYOUT script path " + path);
            var type = AssetDatabase.LoadAssetAtPath<MonoScript>(PresentationPath + script + ".cs").GetClass();
            var component = target.GetComponent(type); Require(component != null, "LAYOUT script " + script); return component;
        }
        private static void LayoutRemoveOwners(RectTransform rect)
        {
            foreach (var component in rect.GetComponents<Component>())
                if (component is UnityEngine.UI.LayoutGroup || component is UnityEngine.UI.ContentSizeFitter ||
                    component is UnityEngine.UI.AspectRatioFitter || component is UnityEngine.UI.LayoutElement)
                    UnityEngine.Object.DestroyImmediate(component);
        }
        private static RectTransform LayoutMove(Transform root, string path, Transform parent, string name)
        {
            var rect = LayoutAt<RectTransform>(root, path); rect.SetParent(parent, false); rect.name = name;
            rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity; return rect;
        }
        private static void LayoutBox(RectTransform rect, float x, float top, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -top); rect.sizeDelta = new Vector2(width, height); rect.localScale = Vector3.one;
        }
        private static void LayoutCell(RectTransform rect, float width, float height)
        {
            var element = rect.GetComponent<UnityEngine.UI.LayoutElement>() ?? rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            element.minWidth = element.preferredWidth = width; element.minHeight = element.preferredHeight = height;
            element.flexibleWidth = element.flexibleHeight = 0;
        }
        private static void LayoutHorizontal(RectTransform rect, float spacing, bool fit = false)
        {
            LayoutRemoveOwners(rect);
            var group = rect.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            group.spacing = spacing; group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandWidth = group.childForceExpandHeight = false; group.childAlignment = TextAnchor.MiddleLeft;
            if (fit) rect.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        }
        private static void LayoutRows(RectTransform rect, bool fit = true)
        {
            LayoutRemoveOwners(rect); Vertical(rect, fit);
            var group = rect.GetComponent<UnityEngine.UI.VerticalLayoutGroup>(); group.padding = new RectOffset();
        }
        private static void LayoutScroll(RectTransform root, bool horizontal = false)
        {
            var scroll = root.GetComponent<UnityEngine.UI.ScrollRect>(); Require(scroll != null, "LAYOUT ScrollRect root");
            scroll.horizontal = horizontal; scroll.vertical = !horizontal; scroll.inertia = false; scroll.elasticity = 0;
            scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            Require(scroll.viewport.parent == root && scroll.content.parent == scroll.viewport &&
                scroll.viewport.GetComponent<UnityEngine.UI.Mask>() != null, "LAYOUT scroll chain");
            scroll.viewport.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            if (horizontal)
            {
                LayoutHorizontal(scroll.content, 16, true);
                scroll.content.anchorMin = scroll.content.anchorMax = scroll.content.pivot = new Vector2(0, 1);
                scroll.content.sizeDelta = new Vector2(1624, 72); scroll.content.anchoredPosition = Vector2.zero;
            }
            else LayoutRows(scroll.content);
        }
        private static RectTransform LayoutNewScroll(string name, Transform parent)
        {
            var content = Scroll(name, parent); LayoutScroll((RectTransform)content.parent.parent); return content;
        }
        private static void LayoutButton(UnityEngine.UI.Button button, float width = -1)
        {
            var rect = (RectTransform)button.transform; LayoutRemoveOwners(rect); LayoutCell(rect, width, 48);
            var caption = LayoutAt<RectTransform>(rect, "Caption"); LayoutRemoveOwners(caption); Stretch(caption);
            caption.offsetMin = new Vector2(12, 8); caption.offsetMax = new Vector2(-12, -8);
            var text = caption.GetComponent<TMPro.TextMeshProUGUI>(); text.fontSize = 18; text.enableAutoSizing = false;
            text.enableWordWrapping = false; text.characterSpacing = text.wordSpacing = 0;
            text.overflowMode = TMPro.TextOverflowModes.Overflow;
            var reason = rect.Find("DisabledReason") as RectTransform;
            if (reason != null)
            {
                LayoutRemoveOwners(reason); Stretch(reason);
                caption.offsetMin = new Vector2(12, 24); caption.offsetMax = new Vector2(-12, -2);
                reason.offsetMin = new Vector2(12, 2); reason.offsetMax = new Vector2(-12, -24);
                var detail = reason.GetComponent<TMPro.TextMeshProUGUI>(); detail.fontSize = 10;
                detail.enableAutoSizing = false; detail.enableWordWrapping = true;
            }
        }
        private static LayoutPanel LayoutMakePanel(RectTransform root, bool sections)
        {
            LayoutRemoveOwners(root);
            var surface = root.GetComponent<UnityEngine.UI.Image>(); if (surface != null) surface.raycastTarget = false;
            var panel = new LayoutPanel { Root = root, Header = Rect("Header", root), Actions = Rect("Actions", root) };
            LayoutRows(root, false); var group = root.GetComponent<UnityEngine.UI.VerticalLayoutGroup>(); group.spacing = 0;
            LayoutCell(panel.Header, -1, 48);
            panel.Body = LayoutNewScroll("BodyViewport", root);
            var scroll = (RectTransform)panel.Body.parent.parent;
            LayoutCell(scroll, -1, 48); scroll.GetComponent<UnityEngine.UI.LayoutElement>().flexibleHeight = 1;
            panel.Header.SetSiblingIndex(0); scroll.SetSiblingIndex(1); panel.Actions.SetSiblingIndex(2);
            LayoutRows(panel.Actions, false);
            if (sections)
            {
                panel.Actions.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing = 16;
                panel.State = Rect("StateRows", panel.Body); LayoutRows(panel.State);
                panel.Choices = Rect("ChoiceRows", panel.Body); LayoutRows(panel.Choices);
                panel.Body.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().childControlHeight = false;
                panel.Safe = Rect("SafeActions", panel.Actions); LayoutRows(panel.Safe, false);
                panel.Danger = Rect("DangerActions", panel.Actions); LayoutRows(panel.Danger, false);
            }
            else panel.State = panel.Body;
            return panel;
        }
        private static RectTransform LayoutMask(RectTransform root)
        {
            LayoutRemoveOwners(root); Stretch(root);
            var graphic = root.GetComponent<UnityEngine.UI.Graphic>(); if (graphic != null) graphic.raycastTarget = false;
            var mask = Rect("RootMask", root, true); Surface(mask, new Color(.02f, .03f, .04f, .8f), true);
            return mask;
        }
        private static Component LayoutTitle(LayoutPanel panel, Transform oldRoot = null, string oldPath = null)
        {
            var label = oldRoot == null ? Label("Title", panel.Header, height: 48) :
                LayoutScript(oldRoot, oldPath, "Localization/LocalizedTmpText");
            var rect = (RectTransform)label.transform; rect.SetParent(panel.Header, false); rect.name = "Title";
            LayoutRemoveOwners(rect); Stretch(rect); return label;
        }
        private static void LayoutDecorate(RectTransform slot, bool enemy)
        {
            var art = Rect("TempArt_" + (enemy ? "Enemy" : "Ally"), slot, true);
            art.offsetMin = new Vector2(4, 4); art.offsetMax = new Vector2(-4, -4); art.SetAsFirstSibling();
            Surface(art, enemy ? new Color(.30f, .18f, .18f) : new Color(.14f, .26f, .31f));
            var edge = art.gameObject.AddComponent<UnityEngine.UI.Outline>(); edge.effectDistance = Vector2.one;
        }
        private static UnityEngine.Object[] LayoutSlots(RectTransform parent, string prefix, int count, bool enemy, bool labels)
        {
            var slots = new UnityEngine.Object[count];
            for (var i = 0; i < count; i++)
            {
                var slot = Rect(prefix + i, parent); slots[i] = slot;
                LayoutBox(slot, i * 80, 0, 72, labels ? 96 : 72); LayoutDecorate(slot, enemy);
                if (!labels) continue;
                var names = new[] { "Name", "Hp", "Intent" };
                for (var row = 0; row < names.Length; row++)
                {
                    var label = Label(names[row], slot, RowId((enemy ? "enemy" : "ally") + "-" + names[row].ToLowerInvariant() + "-" + i), 24);
                    LayoutRemoveOwners((RectTransform)label.transform); LayoutBox((RectTransform)label.transform, 0, row * 24, 72, 24);
                    label.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 12;
                }
            }
            return slots;
        }
        private static UnityEngine.Object[] LayoutSlotLabels(UnityEngine.Object[] slots, string name)
        { return slots.Cast<RectTransform>().Select(x => LayoutScript(x, name, "Localization/LocalizedTmpText")).Cast<UnityEngine.Object>().ToArray(); }
        private static void LayoutDrawerHeader(RectTransform drawer, Component title, UnityEngine.UI.Button close)
        {
            var header = Rect("Header", drawer); header.anchorMin = new Vector2(0, 1); header.anchorMax = Vector2.one;
            header.pivot = new Vector2(.5f, 1); header.sizeDelta = new Vector2(0, 48);
            title.transform.SetParent(header, false); title.name = "Title";
            var titleRect = (RectTransform)title.transform; LayoutRemoveOwners(titleRect); Stretch(titleRect); titleRect.offsetMax = new Vector2(-104, 0);
            close.transform.SetParent(header, false); close.name = "Close"; LayoutButton(close, 96);
            var closeRect = (RectTransform)close.transform; closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot = Vector2.one;
            closeRect.anchoredPosition = Vector2.zero; closeRect.sizeDelta = new Vector2(96, 48); close.gameObject.SetActive(true);
        }
        private static void LayoutDrawerBody(RectTransform content)
        {
            var root = (RectTransform)content.parent.parent; Stretch(root);
            root.offsetMax = new Vector2(0, -48);
        }
        private static void LayoutMigrateRoot(GameObject root)
        {
            root.SetActive(false);
            var safe = LayoutAt<RectTransform>(root.transform, "RuntimeCanvas/SafeAreaRoot");
            var screen = LayoutAt<RectTransform>(safe, "ScreenLayer"); Stretch(screen);
            var hud = LayoutAt<RectTransform>(safe, "HudLayer"); LayoutRemoveOwners(hud); Stretch(hud);
            var top = Rect("TopBar", hud); LayoutBox(top, 0, 0, 508, 56); LayoutHorizontal(top, 8);
            top.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().padding = new RectOffset(0, 0, 4, 4);
            var back = LayoutMove(hud, "Back", top, "Back"); LayoutButton(back.GetComponent<UnityEngine.UI.Button>(), 80);
            var battle = LayoutAt<RectTransform>(screen, "BattlePage/BattleContent");
            var oldRows = LayoutAt<RectTransform>(battle, "BattleScroll/Viewport/Content");
            var headline = LayoutMove(oldRows, "Headline", top, "Title"); LayoutCell(headline, 164, 48);
            var languageButton = LayoutMove(hud, "Language", top, "Language"); LayoutButton(languageButton.GetComponent<UnityEngine.UI.Button>(), 104);
            var licenseButton = LayoutMove(hud, "License", top, "License"); LayoutButton(licenseButton.GetComponent<UnityEngine.UI.Button>(), 136);
            var stage = Rect("Stage", battle); var status = LayoutMove(oldRows, "BattleStatus", battle, "BattleStatus"); LayoutRemoveOwners(status);
            var boardRegion = Rect("BoardRegion", battle); var board = LayoutMove(status, "BoardFrame", boardRegion, "BoardFrame");
            LayoutRemoveOwners(board); Stretch(board);
            var decoration = Rect("BoardDecoration", boardRegion, true); decoration.SetAsFirstSibling(); Surface(decoration, new Color(.12f, .14f, .18f));
            var bottom = Rect("BottomHud", battle); var normal = Rect("NormalHud", bottom, true);
            var statusRows = LayoutNewScroll("StatusViewport", normal);
            LayoutMove(status, "Save", statusRows, "Save"); LayoutMove(oldRows, "BattleHud", statusRows, "BattleHudFirstLine");
            LayoutMove(status, "Availability", statusRows, "AvailabilityFirstLine"); LayoutMove(status, "Diagnostic", statusRows, "PlaybackDiagnostic");
            var main = Rect("MainRow", normal); var members = Rect("MemberStrip", main); LayoutBox(members, 8, 0, 232, 72);
            var memberSlots = LayoutSlots(members, "MemberSlot", 3, false, false);
            var commandContent = LayoutNewScroll("CommandStrip", main); var command = (RectTransform)commandContent.parent.parent;
            LayoutScroll(command, true); LayoutBox(command, 248, 0, 252, 72);
            var fixedActions = Rect("FixedBattleActions", commandContent); LayoutHorizontal(fixedActions, 8); LayoutCell(fixedActions, 776, 72);
            var dynamicActions = LayoutMove(oldRows, "Actions", commandContent, "DynamicBattleActions");
            LayoutHorizontal(dynamicActions, 8); LayoutCell(dynamicActions, 832, 72);
            foreach (var entry in new[] { new { Name = "Retry", Width = 184 }, new { Name = "Resolve", Width = 224 }, new { Name = "Skip", Width = 200 } })
                LayoutButton(LayoutMove(status, entry.Name, fixedActions, entry.Name).GetComponent<UnityEngine.UI.Button>(), entry.Width);
            var historyOpen = Button("HistoryOpen", fixedActions, "fm.action.history.open"); LayoutButton(historyOpen, 144);
            var padding = Rect("BottomPadding", normal); padding.anchorMin = Vector2.zero; padding.anchorMax = new Vector2(1, 0);
            padding.pivot = new Vector2(.5f, 0); padding.sizeDelta = new Vector2(0, 8);
            LayoutHorizontal(status, 8); LayoutCell(LayoutAt<RectTransform>(status, "Phase"), 164, 48);
            var playbackStage = LayoutMove(status, "Stage", status, "PlaybackStage"); LayoutCell(playbackStage, 164, 48);
            LayoutCell(LayoutAt<RectTransform>(status, "Beat"), 164, 48);
            var allyGroup = Rect("AllySlots", stage); var enemyGroup = Rect("EnemySlots", stage);
            LayoutBox(allyGroup, 8, 0, 232, 96); LayoutBox(enemyGroup, 268, 0, 232, 96);
            var allies = LayoutSlots(allyGroup, "ActorSlot", 3, false, true);
            var enemies = LayoutSlots(enemyGroup, "ActorSlot", 3, true, true);
            var input = LayoutScript(battle, "BattleStatus", "CandidateBoardInputView");
            References(input, "allyStageSlots", allies, "enemyStageSlots", enemies, "memberSlots", memberSlots,
                "allyNames", LayoutSlotLabels(allies, "Name"), "allyHp", LayoutSlotLabels(allies, "Hp"), "allyIntent", LayoutSlotLabels(allies, "Intent"),
                "enemyNames", LayoutSlotLabels(enemies, "Name"), "enemyHp", LayoutSlotLabels(enemies, "Hp"), "enemyIntent", LayoutSlotLabels(enemies, "Intent"));
            foreach (var retired in new[] { "Enemies", "Members", "Hp", "Intent" })
                UnityEngine.Object.DestroyImmediate(LayoutAt<RectTransform>(status, retired).gameObject);
            var history = Rect("HistoryDrawer", bottom); var historyTitle = Label("Title", history, RowId("history-title"), 48);
            var historyClose = Button("Close", history, RowId("history-close")); LayoutDrawerHeader(history, historyTitle, historyClose);
            var historyRows = LayoutNewScroll("BodyViewport", history); LayoutDrawerBody(historyRows);
            UnityEngine.Object.DestroyImmediate(LayoutAt<RectTransform>(oldRows, "History").gameObject);
            var popups = LayoutAt<RectTransform>(safe, "PopupLayer"); var systems = LayoutAt<RectTransform>(safe, "SystemLayer");
            var reference = LayoutMove(popups, "ReferencePopup", bottom, "ReferenceInfoDrawer"); LayoutRemoveOwners(reference);
            reference.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            var referenceTitle = Label("Title", reference, height: 48);
            var referenceClose = LayoutAt<UnityEngine.UI.Button>(reference, "CloseTemplate");
            var closeId = new SerializedObject(LayoutScript(reference, "CloseTemplate", "FightMatchViewId"));
            closeId.FindProperty("id").stringValue = RowId("reference-close"); closeId.ApplyModifiedPropertiesWithoutUndo();
            LayoutDrawerHeader(reference, referenceTitle, referenceClose);
            var referenceRows = LayoutNewScroll("BodyViewport", reference); LayoutDrawerBody(referenceRows);
            foreach (var name in new[] { "Explanation", "Beat", "Steps" }) LayoutMove(reference, name, referenceRows, name);
            var steps = LayoutAt<RectTransform>(referenceRows, "Steps"); LayoutRows(steps);
            referenceRows.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().childControlHeight = false;
            var referenceActions = Rect("Actions", reference); LayoutBox(referenceActions, 0, 0, 508, 0); referenceActions.gameObject.SetActive(false);
            References(LayoutScript(bottom, "ReferenceInfoDrawer", "PlayerDefaultReferenceView"), "title", referenceTitle);
            var confirmation = LayoutAt<RectTransform>(popups, "ConfirmationPopup"); var confirmationMask = LayoutMask(confirmation);
            var navConfirm = LayoutMakePanel(LayoutMove(confirmation, "NavigationConfirmation", confirmationMask, "NavigationConfirmationPanel"), true);
            var battleConfirm = LayoutMakePanel(LayoutMove(confirmation, "BattleConfirmation", confirmationMask, "BattleConfirmationPanel"), true);
            var recovery = LayoutMove(popups, "RecoveryPopup", systems, "RecoveryScreen"); var recoveryMask = LayoutMask(recovery);
            var navRecovery = LayoutMakePanel(LayoutMove(recovery, "NavigationRecovery", recoveryMask, "NavigationRecoveryPanel"), true);
            var battleRecovery = LayoutMakePanel(LayoutMove(recovery, "BattleRecovery", recoveryMask, "BattleRecoveryPanel"), true);
            var battleConfirmationTitle = LayoutTitle(battleConfirm); Id(battleConfirmationTitle.gameObject, RowId("battle-end-title"));
            var battleRecoveryTitle = LayoutTitle(battleRecovery); Id(battleRecoveryTitle.gameObject, RowId("battle-recovery-title"));
            var navTitle = LayoutTitle(navRecovery, navRecovery.Root, "Title");
            LayoutMove(navRecovery.Root, "Status", navRecovery.State, "Status");
            var navConfirmationTitle = LayoutTitle(navConfirm); var navConfirmationStatus = Label("Status", navConfirm.State);
            LayoutMove(navConfirm.Root, "Confirm", navConfirm.Safe, "Confirm"); LayoutMove(navConfirm.Root, "End", navConfirm.Danger, "End");
            foreach (var name in new[] { "Return", "ResumeObserved", "Retry", "Resolve", "Refresh" }) LayoutMove(navRecovery.Root, name, navRecovery.Safe, name);
            LayoutMove(navRecovery.Root, "EndReview", navRecovery.Danger, "EndReview");
            UnityEngine.Object.DestroyImmediate(LayoutAt<RectTransform>(navRecovery.Root, "OriginalResult").gameObject);
            var navRecoveryView = LayoutScript(safe, "PopupLayer", "PlayerNavigationRecoveryView");
            References(navRecoveryView, "confirmationRoot", navConfirm.Root.gameObject, "recoveryRoot", navRecovery.Root.gameObject,
                "confirmationTitle", navConfirmationTitle, "recoveryTitle", navTitle, "confirmationStatus", navConfirmationStatus,
                "recoveryStatus", LayoutScript(navRecovery.State, "Status", "Localization/LocalizedTmpText"),
                "confirmationBody", navConfirm.State, "confirmationChoices", navConfirm.Choices, "confirmationActions", navConfirm.Safe,
                "confirmationDangerActions", navConfirm.Danger, "recoveryBody", navRecovery.State, "recoveryChoices", navRecovery.Choices,
                "recoverySafeActions", navRecovery.Safe, "recoveryDangerActions", navRecovery.Danger);
            var ordinary = new LayoutPanel[3]; var ordinaryNames = new[] { "LanguagePopup", "LicensePopup", "QuitPopup" };
            for (var i = 0; i < ordinary.Length; i++)
            {
                var outer = LayoutAt<RectTransform>(popups, ordinaryNames[i]); var mask = LayoutMask(outer);
                var panel = LayoutMakePanel(Rect("DialogPanel", mask), false); ordinary[i] = panel; LayoutTitle(panel, outer, "Title");
                if (i == 0)
                {
                    LayoutMove(outer, "Feedback", panel.Body, "Feedback");
                    foreach (var name in new[] { "English", "Chinese", "Close" }) LayoutMove(outer, name, panel.Actions, name);
                }
                else if (i == 1)
                {
                    LayoutMove(outer, "LicenseScrollContainer/LicenseScroll/Viewport/Content/LicenseBody", panel.Body, "LicenseBody");
                    LayoutMove(outer, "Back", panel.Actions, "Back");
                    UnityEngine.Object.DestroyImmediate(LayoutAt<RectTransform>(outer, "LicenseScrollContainer").gameObject);
                }
                else { LayoutMove(outer, "Body", panel.Body, "Body"); LayoutMove(outer, "Confirm", panel.Actions, "Confirm"); LayoutMove(outer, "Stay", panel.Actions, "Stay"); }
                outer.gameObject.SetActive(false);
            }
            var battleView = LayoutScript(screen, "BattlePage", "PlayerBattleView");
            References(battleView, "dialogTitle", battleConfirmationTitle, "recoveryTitle", battleRecoveryTitle,
                "normalHudRoot", normal.gameObject, "historyDrawerRoot", history.gameObject,
                "historyHeader", historyTitle, "historyOpenButton", historyOpen, "historyCloseButton", historyClose,
                "battleNotices", statusRows, "actions", dynamicActions, "history", historyRows,
                "dialogRoot", battleConfirm.Root.gameObject, "dialogBody", battleConfirm.State, "dialogChoices", battleConfirm.Choices,
                "dialogActions", battleConfirm.Safe, "dialogDangerActions", battleConfirm.Danger,
                "recoveryRoot", battleRecovery.Root.gameObject, "recoveryBody", battleRecovery.State, "recoveryChoices", battleRecovery.Choices,
                "recoveryActions", battleRecovery.Safe, "recoveryDangerActions", battleRecovery.Danger);
            var layoutDiagnostic = Rect("LayoutDiagnostic", systems, true); Surface(layoutDiagnostic, new Color(.08f, .04f, .04f, .98f), true);
            var layoutMessage = Label("Message", layoutDiagnostic, height: 192); Stretch((RectTransform)layoutMessage.transform);
            layoutDiagnostic.gameObject.SetActive(false);
            LayoutAt<RectTransform>(systems, "LoadingOverlay").SetSiblingIndex(0); recovery.SetSiblingIndex(1);
            layoutDiagnostic.SetSiblingIndex(2); LayoutAt<RectTransform>(systems, "BlockingDiagnostic").SetSiblingIndex(3);
            var responsive = ComponentFromScript(safe.gameObject, "FightMatchResponsiveLayout");
            var interaction = battle.gameObject.AddComponent<CanvasGroup>();
            References(responsive, "safeArea", LayoutScript(root.transform, "RuntimeCanvas/SafeAreaRoot", "SafeAreaFitter"),
                "canvas", LayoutAt<Canvas>(root.transform, "RuntimeCanvas"), "screenLayer", screen, "topBar", top,
                "battleContent", battle, "stage", stage, "battleStatus", status, "boardRegion", boardRegion, "bottomHud", bottom,
                "normalHudRoot", normal, "historyDrawerRoot", history, "referenceDrawerRoot", reference,
                "normalStatusViewport", statusRows.parent.parent, "normalMainRow", main,
                "startupViewport", LayoutAt<RectTransform>(screen, "StartupPage/StartupScroll"),
                "navigationViewport", LayoutAt<RectTransform>(screen, "NavigationPage/NavigationScroll"),
                "resultViewport", LayoutAt<RectTransform>(screen, "ResultPage/ResultScroll"),
                "dialogPanels", ordinary.Select(x => (UnityEngine.Object)x.Root).Concat(new UnityEngine.Object[] {
                    navConfirm.Root, battleConfirm.Root, navRecovery.Root, battleRecovery.Root }).ToArray(),
                "layoutDiagnostic", layoutDiagnostic.gameObject, "layoutDiagnosticText", layoutMessage, "battleInteraction", interaction);
            References(root.GetComponent<FightMatchHostView>(), "responsiveLayout", responsive,
                "confirmationPopupRoot", confirmation.gameObject, "confirmationRootMask", confirmationMask.gameObject,
                "navigationConfirmationPanel", navConfirm.Root.gameObject, "battleConfirmationPanel", battleConfirm.Root.gameObject,
                "recoveryScreenRoot", recovery.gameObject, "recoveryRootMask", recoveryMask.gameObject,
                "navigationRecoveryPanel", navRecovery.Root.gameObject, "battleRecoveryPanel", battleRecovery.Root.gameObject);
            foreach (var scroll in root.GetComponentsInChildren<UnityEngine.UI.ScrollRect>(true))
            {
                scroll.inertia = false; scroll.elasticity = 0; scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
                var oldMask = scroll.viewport.GetComponent<UnityEngine.UI.RectMask2D>();
                if (oldMask != null) UnityEngine.Object.DestroyImmediate(oldMask);
                var image = scroll.viewport.GetComponent<UnityEngine.UI.Image>() ?? Surface(scroll.viewport, Color.clear, true);
                image.raycastTarget = true;
                var mask = scroll.viewport.GetComponent<UnityEngine.UI.Mask>() ?? scroll.viewport.gameObject.AddComponent<UnityEngine.UI.Mask>();
                mask.showMaskGraphic = false;
                var rootGraphic = scroll.GetComponent<UnityEngine.UI.Graphic>(); if (rootGraphic != null) rootGraphic.raycastTarget = false;
            }
            foreach (var button in popups.GetComponentsInChildren<UnityEngine.UI.Button>(true).Concat(recovery.GetComponentsInChildren<UnityEngine.UI.Button>(true)))
                LayoutButton(button);
            var templates = LayoutAt<RectTransform>(safe, "TemplatePool");
            LayoutButton(LayoutAt<UnityEngine.UI.Button>(templates, "BattleButtonTemplate"));
            var memberTemplate = LayoutAt<UnityEngine.UI.Button>(templates, "MemberButtonTemplate"); LayoutButton(memberTemplate, 72);
            LayoutCell((RectTransform)memberTemplate.transform, 72, 72);
            var memberCaption = LayoutAt<TMPro.TextMeshProUGUI>(memberTemplate.transform, "Caption");
            memberCaption.fontSize = 12; memberCaption.enableWordWrapping = true;
            memberCaption.rectTransform.offsetMin = new Vector2(4, 4); memberCaption.rectTransform.offsetMax = new Vector2(-4, -4);
            normal.gameObject.SetActive(true); history.gameObject.SetActive(false); reference.gameObject.SetActive(false);
            confirmation.gameObject.SetActive(false); recovery.gameObject.SetActive(false);
            foreach (var panel in new[] { navConfirm, battleConfirm, navRecovery, battleRecovery }) panel.Root.gameObject.SetActive(false);
            UnityEngine.Object.DestroyImmediate(LayoutAt<RectTransform>(battle, "BattleScroll").gameObject);
        }

        private static string LayoutRelative(Transform root, Transform child)
        {
            var parts = new System.Collections.Generic.List<string>();
            for (var current = child; current != root; current = current.parent)
            { Require(current != null, "LAYOUT target outside root"); parts.Insert(0, current.name); }
            return string.Join("/", parts);
        }
        private static string[] LayoutVerifyRoot(GameObject root, bool sceneInstance, LayoutSceneCanvasSnapshot snapshot = null)
        {
            var safe = LayoutAt<RectTransform>(root.transform, "RuntimeCanvas/SafeAreaRoot");
            var texts = root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            var bindingType = AssetDatabase.LoadAssetAtPath<MonoScript>(PresentationPath + "Localization/LocalizedTmpText.cs").GetClass();
            var expected = LayoutTextTargets.Split('\n').OrderBy(x => x, StringComparer.Ordinal).ToArray();
            var actual = texts.Select(text => {
                var field = text.GetComponentInParent<TMPro.TMP_InputField>(true);
                var isInput = field != null && field.textComponent == text;
                var binding = text.GetComponent(bindingType);
                var path = LayoutRelative(safe, text.transform);
                Require(text.font == preparingFont && text.fontSharedMaterial == preparingFont.material, "LAYOUT preserved font " + path);
                Require(!text.raycastTarget && text.text == Placeholder || isInput && !text.raycastTarget &&
                    text.text == Placeholder + "\u200B", "LAYOUT placeholder " + path);
                if (isInput) Require(field.text == Placeholder, "LAYOUT input placeholder " + path);
                if (binding != null) Require(new SerializedObject(binding).FindProperty("target").objectReferenceValue == text,
                    "LAYOUT typed TMP target " + path);
                return path + "|" + (isInput ? "Input" : binding != null ? "Binding" : "License");
            }).OrderBy(x => x, StringComparer.Ordinal).ToArray();
            Require(expected.Length == 141 && actual.SequenceEqual(expected), "LAYOUT exact 141 TMP paths/types");
            Require(root.GetComponentsInChildren<TMPro.TMP_InputField>(true).Length == 3, "LAYOUT three input fields");
            var ids = LayoutViewIds(root);
            Require(ids.Distinct(StringComparer.Ordinal).Count() == ids.Length && ids.All(x => !string.IsNullOrEmpty(x)) &&
                ids.Count(x => x == "fm.action.history.open") == 1 && ids.Count(x => x == "fm.popup.reference") == 1,
                "LAYOUT unique stable view identities");
            var canvas = root.GetComponentsInChildren<Canvas>(true);
            Require(canvas.Length == 1 && canvas[0].renderMode == RenderMode.ScreenSpaceOverlay &&
                root.GetComponentsInChildren<UnityEngine.UI.GraphicRaycaster>(true).Length == 1, "LAYOUT single Overlay Canvas/raycaster");
            if (sceneInstance)
            {
                if (snapshot == null) LayoutVerifySceneCanvas(root);
                else LayoutVerifySceneCanvas(root, snapshot);
            }
            else
            {
                Require(EditorUtility.IsPersistent(root) || EditorSceneManager.IsPreviewSceneObject(root), "LAYOUT explicit prefab context");
                LayoutVerifyCanvasGeometry((RectTransform)canvas[0].transform);
            }
            var scaler = canvas[0].GetComponent<UnityEngine.UI.CanvasScaler>();
            Require(scaler != null && scaler.enabled && scaler.uiScaleMode == UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize &&
                scaler.referenceResolution == new Vector2(540, 960) && scaler.matchWidthOrHeight == .5f, "LAYOUT enabled CanvasScaler");
            var responsive = LayoutScript(root.transform, "RuntimeCanvas/SafeAreaRoot", "FightMatchResponsiveLayout");
            var fields = new SerializedObject(responsive);
            foreach (var name in new[] { "safeArea", "canvas", "screenLayer", "topBar", "battleContent", "stage", "battleStatus", "boardRegion",
                "bottomHud", "normalHudRoot", "historyDrawerRoot", "referenceDrawerRoot", "normalStatusViewport", "normalMainRow",
                "startupViewport", "navigationViewport", "resultViewport", "layoutDiagnostic", "layoutDiagnosticText", "battleInteraction" })
                Require(fields.FindProperty(name)?.objectReferenceValue != null, "LAYOUT responsive typed field " + name);
            var panels = fields.FindProperty("dialogPanels"); Require(panels != null && panels.arraySize == 7, "LAYOUT seven panels");
            var input = LayoutScript(safe, "ScreenLayer/BattlePage/BattleContent/BattleStatus", "CandidateBoardInputView");
            var inputFields = new SerializedObject(input);
            foreach (var name in new[] { "allyStageSlots", "enemyStageSlots", "memberSlots", "allyNames", "allyHp", "allyIntent", "enemyNames", "enemyHp", "enemyIntent" })
            {
                var array = inputFields.FindProperty(name); Require(array != null && array.arraySize == 3, "LAYOUT typed triple " + name);
                for (var i = 0; i < 3; i++) Require(array.GetArrayElementAtIndex(i).objectReferenceValue != null, "LAYOUT missing slot " + name + i);
            }
            var board = LayoutAt<RectTransform>(safe, "ScreenLayer/BattlePage/BattleContent/BoardRegion/BoardFrame");
            var boardScript = LayoutScript(board.parent, "BoardFrame", "CandidateBoardElement");
            Require(board.GetComponents<UnityEngine.UI.Graphic>().Length == 1 && board.GetComponent<UnityEngine.UI.Graphic>() == boardScript &&
                board.anchorMin == Vector2.zero && board.anchorMax == Vector2.one && board.offsetMin == Vector2.zero && board.offsetMax == Vector2.zero,
                "LAYOUT sole Graphic/stretch BoardFrame");
            foreach (var scroll in root.GetComponentsInChildren<UnityEngine.UI.ScrollRect>(true))
                Require(scroll.content != null && scroll.viewport != null && scroll.viewport.parent == scroll.transform &&
                    scroll.content.parent == scroll.viewport && scroll.viewport.GetComponent<UnityEngine.UI.Image>() != null &&
                    scroll.viewport.GetComponent<UnityEngine.UI.Mask>() != null && !scroll.inertia && scroll.elasticity == 0 &&
                    scroll.movementType == UnityEngine.UI.ScrollRect.MovementType.Clamped, "LAYOUT clipped local scroll");
            foreach (var graphic in root.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Where(x => x.name.StartsWith("TempArt_", StringComparison.Ordinal)))
                Require(!graphic.raycastTarget && graphic is UnityEngine.UI.Image && ((UnityEngine.UI.Image)graphic).sprite == null,
                    "LAYOUT temporary art surface");
            foreach (var component in root.GetComponentsInChildren<Component>(true))
            {
                Require(component != null, "LAYOUT missing script");
                var serialized = new SerializedObject(component); var property = serialized.GetIterator();
                while (property.NextVisible(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference)
                        Require(property.objectReferenceValue != null || property.objectReferenceInstanceIDValue == 0,
                            "LAYOUT missing reference " + property.propertyPath);
            }
            return actual;
        }
        private static long LayoutLocalId(UnityEngine.Object value)
        {
            Require(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value, out string guid, out long id) && id != 0,
                "LAYOUT persistent object identity"); return id;
        }
        private static string[] LayoutViewIds(GameObject root)
        {
            var type = AssetDatabase.LoadAssetAtPath<MonoScript>(PresentationPath + "FightMatchViewId.cs").GetClass();
            return root.GetComponentsInChildren(type, true).Select(x => new SerializedObject(x).FindProperty("id").stringValue).ToArray();
        }
        private static string[] LayoutSceneIds(FightMatchPlayerHost host, UnityEngine.EventSystems.EventSystem events)
        {
            return new UnityEngine.Object[] { host.gameObject, host, host.RuntimeRoot.gameObject, host.RuntimeRoot, events.gameObject, events }
                .Select(x => GlobalObjectId.GetGlobalObjectIdSlow(x).ToString()).ToArray();
        }
        private static void LayoutVerifyCanvasGeometry(RectTransform rect)
        {
            Require(rect.localScale == Vector3.one && rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one,
                "LAYOUT usable Canvas local scale/stretch");
            var scale = rect.lossyScale;
            Require(new[] { scale.x, scale.y, scale.z }.All(x => !float.IsNaN(x) && !float.IsInfinity(x) && x > 0),
                "LAYOUT finite positive Canvas world scale");
        }
        private static RectTransform LayoutSceneCanvas(GameObject root, out RectTransform source)
        {
            Require(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root) == UguiPrefab, "LAYOUT existing Canvas prefab link");
            var rect = LayoutAt<RectTransform>(root.transform, "RuntimeCanvas");
            source = PrefabUtility.GetCorrespondingObjectFromSource(rect);
            Require(source != null && AssetDatabase.GetAssetPath(source) == UguiPrefab && LayoutLocalId(source) == 1013562322995895578L,
                "LAYOUT exact existing Canvas source");
            LayoutVerifyCanvasGeometry(source);
            return rect;
        }
        private static bool LayoutStaleCanvasOverride(PropertyModification modification, RectTransform source)
        {
            if (modification == null || modification.target != source || !new[] {
                "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z", "m_AnchorMax.x", "m_AnchorMax.y" }.Contains(modification.propertyPath)) return false;
            return !float.TryParse(modification.value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var value) ||
                value != new SerializedObject(source).FindProperty(modification.propertyPath).floatValue;
        }
        private static void LayoutVerifySceneCanvas(GameObject root)
        {
            var rect = LayoutSceneCanvas(root, out var source);
            var canvas = rect.GetComponent<Canvas>();
            Require(root.scene.IsValid() && root.scene.isLoaded && !EditorSceneManager.IsPreviewSceneObject(root) &&
                rect.parent == root.transform && canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay &&
                canvas.enabled && canvas.gameObject.activeInHierarchy && canvas.isRootCanvas && canvas.rootCanvas == canvas &&
                rect.drivenByObject == canvas, "LAYOUT active root Overlay Canvas driven by its own Canvas");
            UnityEngine.Canvas.ForceUpdateCanvases();
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            Require(LayoutUsableSceneCanvasGeometry(rect.localScale, rect.lossyScale, rect.rect, canvas.pixelRect, canvas.scaleFactor,
                corners.Select(x => RectTransformUtility.WorldToScreenPoint(null, x)).ToArray()), "LAYOUT finite usable Canvas screen coverage");
            Require(!(PrefabUtility.GetPropertyModifications(root) ?? Array.Empty<PropertyModification>())
                .Any(x => LayoutStaleCanvasOverride(x, source)), "LAYOUT no stale saved Canvas scale/stretch overrides");
        }
        private static bool LayoutUsableSceneCanvasGeometry(Vector3 localScale, Vector3 worldScale, Rect rect,
            Rect pixels, float scaleFactor, Vector2[] corners)
        {
            var positive = new[] { localScale.x, localScale.y, localScale.z, worldScale.x, worldScale.y, worldScale.z,
                rect.width, rect.height, pixels.width, pixels.height, scaleFactor };
            if (!positive.All(x => x > 0) || !positive.Concat(new[] { rect.xMin, rect.yMin, rect.xMax, rect.yMax,
                pixels.xMin, pixels.yMin, pixels.xMax, pixels.yMax }).All(x => !float.IsNaN(x) && !float.IsInfinity(x)) ||
                corners == null || corners.Length != 4) return false;
            var expected = new[] { new Vector2(pixels.xMin, pixels.yMin), new Vector2(pixels.xMin, pixels.yMax),
                new Vector2(pixels.xMax, pixels.yMax), new Vector2(pixels.xMax, pixels.yMin) };
            return corners.Select((corner, i) => !float.IsNaN(corner.x) && !float.IsInfinity(corner.x) &&
                !float.IsNaN(corner.y) && !float.IsInfinity(corner.y) &&
                Mathf.Abs(corner.x - expected[i].x) <= .5f && Mathf.Abs(corner.y - expected[i].y) <= .5f).All(x => x);
        }
        private static void LayoutRepairSceneCanvas(GameObject root)
        {
            LayoutSceneCanvas(root, out var source);
            var modifications = PrefabUtility.GetPropertyModifications(root) ?? Array.Empty<PropertyModification>();
            var preserved = modifications.Where(x => !LayoutStaleCanvasOverride(x, source)).ToArray();
            PrefabUtility.SetPropertyModifications(root, preserved);
            LayoutVerifySceneCanvas(root);
        }
        private static bool LayoutRetiredObject(string path)
        {
            const string rows = "RuntimeCanvas/SafeAreaRoot/ScreenLayer/BattlePage/BattleContent/BattleScroll";
            if (path == rows || path == rows + "/Viewport" || path == rows + "/Viewport/Content" || path == rows + "/Viewport/Content/History") return true;
            if (new[] { "Enemies", "Members", "Hp", "Intent" }.Any(x => path == rows + "/Viewport/Content/BattleStatus/" + x)) return true;
            if (path.StartsWith("RuntimeCanvas/SafeAreaRoot/PopupLayer/RecoveryPopup/NavigationRecovery/OriginalResult", StringComparison.Ordinal)) return true;
            const string license = "RuntimeCanvas/SafeAreaRoot/PopupLayer/LicensePopup/LicenseScrollContainer";
            return path.StartsWith(license, StringComparison.Ordinal) && !path.EndsWith("/LicenseBody", StringComparison.Ordinal);
        }
        private static void LayoutProtectAfter(LayoutActivation activation)
        {
            LayoutCheckFiles(activation.candidateProjectPath, activation.sources);
            LayoutCheckFiles(activation.candidateProjectPath, activation.importedMetas);
            LayoutCheckFiles(activation.candidateProjectPath, activation.protectedInputs);
            LayoutCheckFiles(activation.candidateProjectPath, activation.resources.Where(x => x.path != UguiPrefab && x.path != ScenePath).ToArray());
            Require(AssetDatabase.AssetPathToGUID(UguiPrefab) == activation.prefabGuid && AssetDatabase.AssetPathToGUID(ScenePath) == activation.sceneGuid,
                "LAYOUT preserved GUIDs after migration");
        }
        private static LayoutResult LayoutReopen(LayoutActivation activation)
        {
            var beforePrefab = Sha(UguiPrefab); var beforeScene = Sha(ScenePath);
            string[] targets, sceneIds;
            var root = PrefabUtility.LoadPrefabContents(UguiPrefab);
            try { targets = LayoutVerifyRoot(root, false); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            try
            {
                var hosts = SceneComponents<FightMatchPlayerHost>(scene); var events = SceneComponents<UnityEngine.EventSystems.EventSystem>(scene);
                Require(hosts.Length == 1 && events.Length == 1 && hosts[0].RuntimeRoot != null, "LAYOUT preserved Host/EventSystem");
                sceneIds = LayoutSceneIds(hosts[0], events[0]);
                var modules = events[0].GetComponents<UnityEngine.EventSystems.BaseInputModule>();
                Require(modules.Length == 1 && MonoScript.FromMonoBehaviour(modules[0]) ==
                    AssetDatabase.LoadAssetAtPath<MonoScript>(PresentationPath + "FightMatchStandaloneInputModule.cs"), "LAYOUT project input module");
                Require(SceneComponents<Canvas>(scene).Length == 1 && SceneComponents<UnityEngine.UIElements.UIDocument>(scene).Length == 0 &&
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(hosts[0].RuntimeRoot) == UguiPrefab &&
                    hosts[0].FontAsset == preparingFont && hosts[0].FontLicense != null, "LAYOUT preserved scene linkage");
                LayoutVerifySceneCanvas(hosts[0].RuntimeRoot.gameObject);
                Require(LayoutVerifyRoot(hosts[0].RuntimeRoot.gameObject, true).SequenceEqual(targets), "LAYOUT reopened instance targets");
                Require(!scene.isDirty && !EditorApplication.isPlayingOrWillChangePlaymode, "LAYOUT read-only scene reopen");
            }
            finally { EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); }
            Require(Sha(UguiPrefab) == beforePrefab && Sha(ScenePath) == beforeScene, "LAYOUT reopen wrote native bytes");
            LayoutProtectAfter(activation);
            return new LayoutResult { mode = "NativeReadOnlyReopen", reopened = true, protectedInputsUnchanged = true,
                prefabSha256 = beforePrefab, sceneSha256 = beforeScene, targets = targets, sceneObjectIds = sceneIds, textCount = 141, inputCount = 3 };
        }
        public static void MigrateLayoutResources()
        {
            var activation = LayoutGuard("MigrateLayoutResources");
            preparingFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(UguiFont); Require(preparingFont != null, "LAYOUT existing font");
            var persistent = AssetDatabase.LoadAssetAtPath<GameObject>(UguiPrefab); Require(persistent != null, "LAYOUT existing prefab");
            var survivors = persistent.GetComponentsInChildren<Transform>(true)
                .Where(x => !LayoutRetiredObject(LayoutRelative(persistent.transform, x))).Select(x => LayoutLocalId(x.gameObject))
                .Concat(persistent.GetComponentsInChildren<Component>(true).Where(x => x != null &&
                    !LayoutRetiredObject(LayoutRelative(persistent.transform, x.transform)) && !(x is UnityEngine.UI.LayoutGroup) &&
                    !(x is UnityEngine.UI.ContentSizeFitter) && !(x is UnityEngine.UI.AspectRatioFitter) &&
                    !(x is UnityEngine.UI.LayoutElement) && !(x is UnityEngine.UI.RectMask2D)).Select(LayoutLocalId)).ToArray();
            var retiredIds = new[] { RowId("board.enemies"), RowId("playback.hp"), RowId("playback.intent"),
                RowId("save.OriginalResult"), RowId("template.reference.close") };
            var stableIds = LayoutViewIds(persistent).Except(retiredIds).ToArray();
            var root = PrefabUtility.LoadPrefabContents(UguiPrefab);
            try
            {
                LayoutMigrateRoot(root); LayoutVerifyRoot(root, false);
                PrefabUtility.SaveAsPrefabAsset(root, UguiPrefab, out var success); Require(success, "LAYOUT same-path native prefab save");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            persistent = AssetDatabase.LoadAssetAtPath<GameObject>(UguiPrefab);
            var afterIds = persistent.GetComponentsInChildren<Transform>(true).Select(x => LayoutLocalId(x.gameObject))
                .Concat(persistent.GetComponentsInChildren<Component>(true).Select(LayoutLocalId)).ToArray();
            Require(survivors.All(afterIds.Contains), "LAYOUT surviving object fileIDs");
            Require(stableIds.All(LayoutViewIds(persistent).Contains), "LAYOUT surviving ViewIds");
            string[] sceneIds;
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            try
            {
                var hosts = SceneComponents<FightMatchPlayerHost>(scene); var events = SceneComponents<UnityEngine.EventSystems.EventSystem>(scene);
                Require(hosts.Length == 1 && events.Length == 1 && hosts[0].RuntimeRoot != null, "LAYOUT existing scene objects");
                var host = hosts[0]; var instance = host.RuntimeRoot;
                sceneIds = LayoutSceneIds(host, events[0]);
                Require(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instance) == UguiPrefab, "LAYOUT existing prefab instance only");
                LayoutRepairSceneCanvas(instance.gameObject);
                LayoutVerifyRoot(instance.gameObject, true);
                EditorSceneManager.MarkSceneDirty(scene);
                Require(EditorSceneManager.SaveScene(scene, ScenePath), "LAYOUT same-path native scene save");
                LayoutVerifySceneCanvas(instance.gameObject);
            }
            finally { EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); }
            LayoutProtectAfter(activation);
            LayoutWrite(activation, "native-migration.json", new LayoutResult { mode = "NativeMigration", reopened = false,
                protectedInputsUnchanged = true, prefabSha256 = Sha(UguiPrefab), sceneSha256 = Sha(ScenePath), textCount = 141, inputCount = 3 });
            var result = LayoutReopen(activation);
            Require(result.sceneObjectIds.SequenceEqual(sceneIds), "LAYOUT scene object identities after reopen");
            LayoutWrite(activation, "native-reopen.json", result); LayoutWrite(activation, "serialized-targets.json", result);
            preparingFont = null;
        }
        public static void VerifyLayoutResources()
        {
            var activation = LayoutGuard("VerifyLayoutResources");
            preparingFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(UguiFont); Require(preparingFont != null, "LAYOUT existing font");
            var result = LayoutReopen(activation);
            LayoutWrite(activation, "native-reopen.json", result); LayoutWrite(activation, "serialized-targets.json", result);
            preparingFont = null;
        }

        private enum LayoutSceneRepairStage { WaitingPreSave, WaitingPostSave, WaitingReopen, Finished }
        private sealed class LayoutSceneRepairRun
        {
            public LayoutActivation activation;
            public Scene scene, emptyScene;
            public GameObject root, prefab;
            public LayoutSceneRepairStage stage;
            public int token, pendingToken, serial, armSerial, ticks, setCalls, saveCalls, forceCalls, queueCalls;
            public int decisiveSamples, sampleLogs, stageLogs;
            public int overrideFiles;
            public long overrideBytes, overrideExpectedBytes, overrideActualBytes;
            public bool overrideFileCreated, overrideComplete;
            public string overrideFile, overrideSha256;
            public double deadline;
            public bool pending, inCallback, emptySceneCreationAttempted, diagnosticsIncomplete;
            public bool? saveReturned;
            public bool immediateOverridesExact;
            public int immediateOverrideCount;
            public byte[] beforeSceneBytes;
            public PropertyModification[] originalModifications, preservedModifications;
            public Exception failure;
            public string beforePrefab, beforeScene, savedScene, observedScene, reopenPrefab, reopenScene;
            public string[] sceneIds, preserved, targets, originalOverrides;
            public readonly System.Collections.Generic.List<string> cleanupFailures = new System.Collections.Generic.List<string>();
        }
        private sealed class LayoutSceneCanvasSnapshot
        {
            public GameObject root;
            public Vector3 localScale, worldScale, position;
            public Vector2 anchorMin, anchorMax, pivot, displaySize;
            public Rect rect, pixels;
            public Matrix4x4 matrix, parentMatrix;
            public float scaleFactor;
            public Vector3[] worldCorners;
            public Vector2[] screenCorners;
            public float[] scaler;
            public bool active, dirty, stale;
            public RenderMode mode;
            public bool[] structure;
            public string identities;
            public string[] overrides, fiveOverrides;
        }
        private static LayoutSceneRepairRun layoutSceneRepairOwner;
        private static string[] LayoutOverrideKeys(PropertyModification[] modifications)
        {
            Func<UnityEngine.Object, string> identity = value => {
                if (ReferenceEquals(value, null)) return "null";
                var id = GlobalObjectId.GetGlobalObjectIdSlow(value);
                Require(id.targetObjectId != 0, "LAYOUT persistent override reference identity"); return id.ToString();
            };
            return modifications.Select(x => x == null ? "null modification" : string.Concat(new[] {
                identity(x.target), x.propertyPath, x.value, identity(x.objectReference) }
                .Select(value => value == null ? "-1:" : value.Length + ":" + value))).ToArray();
        }
        private static PropertyModification[] LayoutCopyModifications(PropertyModification[] values)
        {
            return values.Select(x => x == null ? null : new PropertyModification {
                target = x.target, propertyPath = x.propertyPath, value = x.value, objectReference = x.objectReference
            }).ToArray();
        }
        private static string LayoutRepairOverrideKey(long id, string property, string value, string reference)
        {
            return string.Concat(new[] { "GlobalObjectId_V1-1-c36df3cfc25424c8cb3ec6cae6be1237-" +
                id.ToString(System.Globalization.CultureInfo.InvariantCulture) + "-0", property, value, reference }
                .Select(field => field == null ? "-1:" : field.Length + ":" + field));
        }
        private static string[] LayoutRepairAllowedOverrides()
        {
            var result = new System.Collections.Generic.List<string>();
            foreach (var id in new long[] {
                20060541972559401L, 525732035437362821L, 2083101828246034960L, 3603424187799618609L,
                3613386703531820566L, 5408553659951591733L, 6280369087512753137L, 6783473654260464751L,
                8076177029604565045L, 8217930521597258139L, 9020564264068189081L, 9181528876988089927L })
                foreach (var property in new[] { "m_AnchorMax.x", "m_AnchorMax.y", "m_AnchorMin.x", "m_AnchorMin.y",
                    "m_SizeDelta.x", "m_SizeDelta.y", "m_AnchoredPosition.x", "m_AnchoredPosition.y" })
                    result.Add(LayoutRepairOverrideKey(id, property, "0", "null"));
            result.Add(LayoutRepairOverrideKey(6960656654093734010L, "m_SizeDelta.y", "0", "null"));
            foreach (var property in new[] { "m_AnchorMax.x", "m_AnchorMax.y", "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z" })
                result.Add(LayoutRepairOverrideKey(1013562322995895578L, property, "0", "null"));
            result.Add(LayoutRepairOverrideKey(9054230707113910417L, "m_AdditionalShaderChannelsFlag", "25", "null"));
            foreach (var id in new long[] { 2223056809955240070L, 2452066831200993119L, 2783145704458770550L,
                3538173933649760411L, 4530181991579543270L, 5257332161253344251L, 5265726911075511648L,
                7259531018279518139L, 7481568776225615129L })
                result.Add(LayoutRepairOverrideKey(id, "m_TextStyleHashCode", "-1183493901", "null"));
            return result.ToArray();
        }
        private static void LayoutVerifyRepairOverrideEnvelope(string[] preserved, string[] actual)
        {
            Require(preserved != null && preserved.Length == 22 && preserved.Distinct(StringComparer.Ordinal).Count() == 22 &&
                actual != null, "LAYOUT N10 frozen 22 override baseline");
            var originals = new System.Collections.Generic.HashSet<string>(preserved, StringComparer.Ordinal);
            var allowed = new System.Collections.Generic.HashSet<string>(LayoutRepairAllowedOverrides(), StringComparer.Ordinal);
            var seen = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
            var next = 0;
            foreach (var key in actual)
            {
                if (originals.Contains(key))
                {
                    Require(next < preserved.Length && key == preserved[next], "LAYOUT N10 original override order/count");
                    next++;
                }
                else Require(allowed.Contains(key) && seen.Add(key), "LAYOUT N10 exact additional override/count");
            }
            Require(next == preserved.Length, "LAYOUT N10 all original overrides retained");
        }
        private static Match[] LayoutRepairDiskRecords(string text)
        {
            const string header = "--- !u!1001 &1017091371\nPrefabInstance:\n";
            const string source = "  m_SourcePrefab: {fileID: 100100000, guid: c36df3cfc25424c8cb3ec6cae6be1237, type: 3}\n";
            var start = text.IndexOf(header, StringComparison.Ordinal);
            Require(start >= 0 && (start == 0 || text[start - 1] == '\n') &&
                text.IndexOf(header, start + header.Length, StringComparison.Ordinal) < 0, "LAYOUT N10 exact disk PrefabInstance");
            var end = text.IndexOf("\n--- !u!", start + header.Length, StringComparison.Ordinal);
            end = end < 0 ? text.Length : end + 1;
            var section = text.Substring(start, end - start);
            Require(section.EndsWith(source, StringComparison.Ordinal) &&
                section.IndexOf(source, StringComparison.Ordinal) == section.LastIndexOf(source, StringComparison.Ordinal),
                "LAYOUT N10 exact disk source Prefab");
            const string marker = "    m_Modifications:\n";
            var first = section.IndexOf(marker, StringComparison.Ordinal);
            Require(first >= 0 && section.IndexOf(marker, first + marker.Length, StringComparison.Ordinal) < 0,
                "LAYOUT N10 one disk modification sequence");
            var last = section.IndexOf("    m_RemovedComponents:", first + marker.Length, StringComparison.Ordinal);
            Require(last >= 0, "LAYOUT N10 disk modification boundary");
            first += start + marker.Length; last += start;
            var pattern = @"(?m)^    - target: \{fileID: (?<id>\d+), guid: c36df3cfc25424c8cb3ec6cae6be1237, type: 3\}\n" +
                @"      propertyPath: (?<property>[^\r\n]+)\n      value: (?<value>[^\r\n]*)\n      objectReference: \{fileID: 0\}\n";
            var rows = Regex.Matches(text, pattern).Cast<Match>().Where(x => x.Index >= first && x.Index + x.Length <= last).ToArray();
            Require(string.Concat(rows.Select(x => x.Value)) == text.Substring(first, last - first),
                "LAYOUT N10 complete exact disk modification blocks");
            return rows;
        }
        private static string[] LayoutRepairDiskKeys(byte[] image, bool clean)
        {
            var rows = LayoutRepairDiskRecords(new System.Text.UTF8Encoding(false, true).GetString(image));
            Require(rows.Length == (clean ? 22 : 27), "LAYOUT N10 disk modification cardinality");
            if (clean)
                Require(!rows.Any(x => x.Groups["id"].Value == "1013562322995895578" && new[] {
                    "m_AnchorMax.x", "m_AnchorMax.y", "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z"
                }.Contains(x.Groups["property"].Value)), "LAYOUT N10 no five Canvas records on disk");
            return rows.Select(x => LayoutRepairOverrideKey(long.Parse(x.Groups["id"].Value,
                System.Globalization.CultureInfo.InvariantCulture), x.Groups["property"].Value, x.Groups["value"].Value, "null")).ToArray();
        }
        private static void LayoutVerifyRepairDiskDelta(byte[] before, byte[] after)
        {
            var encoding = new System.Text.UTF8Encoding(false, true);
            var text = encoding.GetString(before);
            var rows = LayoutRepairDiskRecords(text);
            Require(rows.Length == 27, "LAYOUT N10 original disk 27 records");
            var removed = new System.Collections.Generic.List<Match>();
            foreach (var property in new[] { "m_AnchorMax.x", "m_AnchorMax.y", "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z" })
            {
                var matches = rows.Where(x => x.Groups["id"].Value == "1013562322995895578" &&
                    x.Groups["property"].Value == property).ToArray();
                Require(matches.Length == 1 && matches[0].Groups["value"].Value == "0", "LAYOUT N10 one original zero block: " + property);
                removed.Add(matches[0]);
            }
            foreach (var block in removed.OrderByDescending(x => x.Index)) text = text.Remove(block.Index, block.Length);
            Require(encoding.GetBytes(text).SequenceEqual(after), "LAYOUT N10 disk only five exact blocks deleted; all other bytes preserved");
            LayoutRepairDiskKeys(after, true);
        }
        private static LayoutSceneCanvasSnapshot LayoutCaptureSceneCanvas(GameObject root)
        {
            var rect = LayoutSceneCanvas(root, out var source); var canvas = rect.GetComponent<Canvas>();
            Require(canvas != null, "LAYOUT N10 missing Canvas snapshot structure");
            var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            Require(scaler != null, "LAYOUT N10 missing CanvasScaler snapshot structure");
            var modifications = PrefabUtility.GetPropertyModifications(root) ?? Array.Empty<PropertyModification>();
            var parent = rect.parent; var driver = rect.drivenByObject; var mode = canvas.renderMode;
            var sample = new LayoutSceneCanvasSnapshot {
                root = root, localScale = rect.localScale, worldScale = rect.lossyScale, position = rect.position,
                anchorMin = rect.anchorMin, anchorMax = rect.anchorMax, pivot = rect.pivot, rect = rect.rect,
                matrix = rect.localToWorldMatrix, parentMatrix = parent == null ? Matrix4x4.zero : parent.localToWorldMatrix,
                pixels = canvas.pixelRect, scaleFactor = canvas.scaleFactor, displaySize = canvas.renderingDisplaySize,
                worldCorners = new Vector3[4], dirty = root.scene.isDirty,
                mode = mode, structure = new[] { root.scene.IsValid(), root.scene.isLoaded, !EditorSceneManager.IsPreviewSceneObject(root),
                    parent == root.transform, mode == RenderMode.ScreenSpaceOverlay, canvas.enabled, canvas.gameObject.activeInHierarchy,
                    canvas.isRootCanvas, canvas.rootCanvas == canvas, driver == canvas },
                overrides = LayoutOverrideKeys(modifications), stale = modifications.Any(x => LayoutStaleCanvasOverride(x, source)),
                fiveOverrides = LayoutOverrideKeys(modifications.Where(x => x != null && x.target == source && new[] {
                    "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z", "m_AnchorMax.x", "m_AnchorMax.y" }.Contains(x.propertyPath)).ToArray()),
                scaler = new[] { scaler.enabled ? 1f : 0f, (float)scaler.uiScaleMode, scaler.referenceResolution.x,
                    scaler.referenceResolution.y, scaler.matchWidthOrHeight, (float)scaler.screenMatchMode, (float)scaler.physicalUnit,
                    scaler.fallbackScreenDPI, scaler.defaultSpriteDPI, scaler.dynamicPixelsPerUnit, scaler.referencePixelsPerUnit, scaler.scaleFactor },
                identities = "prefab=" + PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root) + " guid=" +
                    AssetDatabase.AssetPathToGUID(UguiPrefab) + " source=" + GlobalObjectId.GetGlobalObjectIdSlow(source) +
                    " parent=" + (parent == null ? "null" : GlobalObjectId.GetGlobalObjectIdSlow(parent).ToString()) +
                    " driver=" + (driver == null ? "null" : GlobalObjectId.GetGlobalObjectIdSlow(driver).ToString())
            };
            sample.active = sample.structure.All(x => x);
            rect.GetWorldCorners(sample.worldCorners);
            sample.screenCorners = sample.worldCorners.Select(x => RectTransformUtility.WorldToScreenPoint(null, x)).ToArray();
            return sample;
        }
        private static void LayoutVerifySceneCanvas(GameObject root, LayoutSceneCanvasSnapshot sample)
        {
            Require(sample != null && sample.root == root && sample.active, "LAYOUT active root Overlay Canvas driven by its own Canvas");
            Require(LayoutUsableSceneCanvasGeometry(sample.localScale, sample.worldScale, sample.rect, sample.pixels,
                sample.scaleFactor, sample.screenCorners), "LAYOUT finite usable Canvas screen coverage");
        }
        [Serializable]
        private sealed class LayoutOverrideLog
        {
            public string[] keys;
            public int[] original, expected, actual, expectedCounts, actualCounts, delta;
            public int firstDifference;
            public bool orderedEqual, contentEqual;
        }
        private static string LayoutDescribeOverrides(string[] original, string[] expected, string[] actual)
        {
            Require(original != null && expected != null && actual != null, "LAYOUT N08 complete override sequences");
            var keys = new System.Collections.Generic.List<string>();
            var indices = new System.Collections.Generic.Dictionary<string, int>(StringComparer.Ordinal);
            Func<string[], int[]> encode = values => values.Select(value => {
                Require(value != null, "LAYOUT N08 complete override key");
                if (!indices.TryGetValue(value, out var index))
                {
                    index = keys.Count;
                    indices.Add(value, index);
                    keys.Add(value);
                }
                return index;
            }).ToArray();
            var before = encode(original);
            var wanted = encode(expected);
            var observed = encode(actual);
            var expectedCounts = new int[keys.Count];
            var actualCounts = new int[keys.Count];
            foreach (var index in wanted) expectedCounts[index]++;
            foreach (var index in observed) actualCounts[index]++;
            var delta = actualCounts.Select((count, index) => count - expectedCounts[index]).ToArray();
            var first = -1;
            for (var index = 0; index < Math.Max(wanted.Length, observed.Length); index++)
            {
                if (index < wanted.Length && index < observed.Length && wanted[index] == observed[index]) continue;
                first = index;
                break;
            }
            return JsonUtility.ToJson(new LayoutOverrideLog {
                keys = keys.ToArray(), original = before, expected = wanted, actual = observed,
                expectedCounts = expectedCounts, actualCounts = actualCounts, delta = delta,
                firstDifference = first, orderedEqual = first == -1, contentEqual = delta.All(value => value == 0)
            });
        }
        private static void LayoutCloseOwnedScene(LayoutSceneRepairRun run)
        {
            Require(run == layoutSceneRepairOwner && run.scene.IsValid() && run.scene.isLoaded, "LAYOUT N08 close owned loaded Scene");
            var otherLoaded = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
                .Any(scene => scene.isLoaded && scene != run.scene);
            if (!otherLoaded)
            {
                Require(!run.emptySceneCreationAttempted, "LAYOUT N08 empty Scene creation already attempted");
                run.emptySceneCreationAttempted = true;
                run.emptyScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            }
            if (run.emptySceneCreationAttempted)
                Require(run.emptyScene.IsValid() && run.emptyScene.isLoaded && run.emptyScene != run.scene &&
                    string.IsNullOrEmpty(run.emptyScene.path) && run.emptyScene.rootCount == 0 && !run.emptyScene.isDirty,
                    "LAYOUT N08 owned empty Scene intact");
            Require(Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
                .Any(scene => scene.isLoaded && scene != run.scene), "LAYOUT N08 another loaded Scene before close");
            Require(EditorSceneManager.CloseScene(run.scene, true), "LAYOUT N08 close owned Scene without saving");
            run.scene = default;
        }
        private static void LayoutRecordCleanupFailure(LayoutSceneRepairRun run, string phase, Exception error)
        {
            var detail = phase + ":" + error.GetType().FullName + ":" + error.Message;
            run.cleanupFailures.Add(detail);
            run.failure = run.failure ?? error;
            try
            {
                Func<string, string> summary = value => {
                    value = value.Replace("\r", "\\r").Replace("\n", "\\n");
                    if (value.Length <= 256) return value;
                    run.diagnosticsIncomplete = true;
                    return value.Substring(0, 256) + "...[TRUNCATED]";
                };
                var first = summary(run.failure.GetType().FullName + ":" + run.failure.Message);
                var secondary = summary(detail);
                var text = "[LAYOUT-N10-STAGE] run=" + run.activation?.ownerTurn + " stage=" + run.stage +
                    " token=" + run.token + " event=" + run.serial + " kind=cleanup-error phase=" + phase +
                    " diagnosticsIncomplete=" + run.diagnosticsIncomplete + " firstFailure=" + first + " secondary=" + secondary;
                Require(System.Text.Encoding.UTF8.GetByteCount(text) <= 2048, "LAYOUT N10 cleanup summary budget");
                Require(++run.stageLogs <= 8, "LAYOUT N10 log count budget");
                Debug.Log(text);
            }
            catch (Exception logging)
            {
                run.diagnosticsIncomplete = true;
                run.cleanupFailures.Add("cleanup-summary:" + logging.GetType().FullName + ":" + logging.Message);
                run.failure = run.failure ?? logging;
            }
        }
        private static string LayoutWriteOverrideDiff(LayoutSceneRepairRun run, LayoutSceneCanvasSnapshot sample)
        {
            var root = LayoutStageRoot + "/FIX04/N10";
            var index = run.sampleLogs;
            run.overrideFile = "D/override-diff-" + index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture) + ".json";
            run.overrideExpectedBytes = -1; run.overrideActualBytes = -1;
            run.overrideFileCreated = false; run.overrideComplete = false; run.overrideSha256 = null;
            string path = null;
            try
            {
                Require(run == layoutSceneRepairOwner && run.failure == null && run.activation != null &&
                    index >= 0 && index < 9 && run.overrideFiles == index, "LAYOUT N10 one diagnostic file per sample");
                Require(run.activation.evidenceRoot == root + "/P" &&
                    LayoutCanonical(Arg("-fmLayoutInputManifest")) == root + "/I/activation.json" &&
                    LayoutCanonical(run.activation.sourceReceiptPath) == root + "/S/source-receipt.json",
                    "LAYOUT N10 fixed diagnostic activation root");
                var directory = LayoutCanonical(root + "/D");
                path = LayoutCanonical(root + "/" + run.overrideFile);
                Require(Path.GetDirectoryName(path) == directory, "LAYOUT N10 fixed diagnostic file path");
                var json = LayoutDescribeOverrides(run.originalOverrides, run.preserved, sample.overrides);
                var bytes = new System.Text.UTF8Encoding(false, true).GetBytes(json);
                run.overrideExpectedBytes = bytes.LongLength;
                Require(bytes.LongLength <= 1048576 && run.overrideBytes + bytes.LongLength <= 9437184,
                    "LAYOUT N10 diagnostic JSON byte budget");
                var summary = JsonUtility.FromJson<LayoutOverrideLog>(json);
                Require(summary != null, "LAYOUT N10 diagnostic summary");
                if (index == 0)
                {
                    Require(!Directory.Exists(directory) && !File.Exists(directory), "LAYOUT N10 create-once diagnostic directory");
                    Directory.CreateDirectory(directory);
                }
                Require(Directory.Exists(directory) && !File.Exists(path) && !Directory.Exists(path),
                    "LAYOUT N10 diagnostic directory/file availability");
                LayoutCanonical(path);
                run.overrideFiles++;
                using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                {
                    run.overrideFileCreated = true; run.overrideActualBytes = stream.Length;
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                    run.overrideActualBytes = stream.Length;
                    Require(run.overrideActualBytes == run.overrideExpectedBytes, "LAYOUT N10 diagnostic bytes expected=" +
                        run.overrideExpectedBytes + " actual=" + run.overrideActualBytes);
                    using (var hash = SHA256.Create())
                    {
                        var expectedHash = string.Concat(hash.ComputeHash(bytes).Select(x => x.ToString("x2")));
                        stream.Position = 0;
                        run.overrideSha256 = string.Concat(hash.ComputeHash(stream).Select(x => x.ToString("x2")));
                        Require(run.overrideSha256 == expectedHash, "LAYOUT N10 diagnostic hash expected=" +
                            expectedHash + " actual=" + run.overrideSha256);
                    }
                }
                run.overrideBytes += run.overrideActualBytes;
                run.overrideComplete = true;
                return " overrideDiffPath=" + run.overrideFile + " overrideDiffBytes=" + run.overrideActualBytes +
                    " overrideDiffSha256=" + run.overrideSha256 + " overrideDiffComplete=True" +
                    " overrideDiffOriginal/Expected/Actual=" + summary.original.Length + "/" + summary.expected.Length + "/" + summary.actual.Length +
                    " overrideDiffFirstDifference=" + summary.firstDifference + " overrideDiffOrderedEqual=" + summary.orderedEqual +
                    " overrideDiffContentEqual=" + summary.contentEqual;
            }
            catch (Exception error)
            {
                run.diagnosticsIncomplete = true;
                run.failure = run.failure ?? error;
                if (run.overrideFileCreated)
                {
                    try { run.overrideActualBytes = new FileInfo(path).Length; }
                    catch (Exception observation) { LayoutRecordCleanupFailure(run, "override-diff-length", observation); }
                }
                throw;
            }
        }
        private static void LayoutLogSceneCanvas(LayoutSceneRepairRun run, LayoutSceneCanvasSnapshot sample, string kind, int token)
        {
            Func<float, string> number = x => x.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            Func<System.Collections.Generic.IEnumerable<float>, string> numbers = values => string.Join(",", values.Select(number));
            var text = "run=" + run.activation?.ownerTurn + " stage=" + run.stage + " token=" + token + " event=" + run.serial +
                " kind=" + kind + " Set=" + run.setCalls + " Save=" + run.saveCalls + " returned=" + run.saveReturned +
                " Force=" + run.forceCalls + " Queue=" + run.queueCalls + " decisive=" + run.decisiveSamples;
            if (sample != null)
            {
                var positive = new[] { sample.localScale.x, sample.localScale.y, sample.localScale.z, sample.worldScale.x, sample.worldScale.y,
                    sample.worldScale.z, sample.rect.width, sample.rect.height, sample.pixels.width, sample.pixels.height, sample.scaleFactor };
                var bounds = new[] { sample.rect.xMin, sample.rect.yMin, sample.rect.xMax, sample.rect.yMax,
                    sample.pixels.xMin, sample.pixels.yMin, sample.pixels.xMax, sample.pixels.yMax };
                var expected = new[] { new Vector2(sample.pixels.xMin, sample.pixels.yMin), new Vector2(sample.pixels.xMin, sample.pixels.yMax),
                    new Vector2(sample.pixels.xMax, sample.pixels.yMax), new Vector2(sample.pixels.xMax, sample.pixels.yMin) };
                var corners = sample.screenCorners.SelectMany(x => new[] { x.x, x.y }).ToArray();
                var differences = sample.screenCorners.SelectMany((x, i) => new[] { Mathf.Abs(x.x - expected[i].x), Mathf.Abs(x.y - expected[i].y) }).ToArray();
                text += " scale/local/world/rectSize/pixelSize/factor=" + numbers(positive) + " bounds=" + numbers(bounds) +
                    " anchors/pivot/position/display=" + numbers(new[] { sample.anchorMin.x, sample.anchorMin.y, sample.anchorMax.x, sample.anchorMax.y,
                        sample.pivot.x, sample.pivot.y, sample.position.x, sample.position.y, sample.position.z, sample.displaySize.x, sample.displaySize.y }) +
                    " matrix=" + numbers(Enumerable.Range(0, 16).Select(i => sample.matrix[i])) +
                    " parentMatrix=" + numbers(Enumerable.Range(0, 16).Select(i => sample.parentMatrix[i])) +
                    " worldCorners=" + numbers(sample.worldCorners.SelectMany(x => new[] { x.x, x.y, x.z })) + " screenCorners=" + numbers(corners) +
                    " cornerErrors=" + numbers(differences) + " positive=" + string.Join(",", positive.Select(x => x > 0)) +
                    " finite=" + string.Join(",", positive.Concat(bounds).Concat(corners).Select(x => !float.IsNaN(x) && !float.IsInfinity(x))) +
                    " coverage=" + string.Join(",", differences.Select(x => x <= .5f)) + " observedStaleOverrides=" + sample.stale +
                    " preservedSequenceExact=" + sample.overrides.SequenceEqual(run.preserved) + " fiveOverrides=" + string.Join(";", sample.fiveOverrides) +
                    " dirty=" + sample.dirty + " mode=" + sample.mode + " structure(valid/loaded/notPreview/parent/overlay/enabled/active/root/ownRoot/ownDriver)=" +
                    string.Join(",", sample.structure) + " " + sample.identities +
                    " scaler(enabled/mode/refX/refY/match/screenMode/unit/fallbackDpi/spriteDpi/dynamicPpu/referencePpu/factor)=" + numbers(sample.scaler);
                text += " geometryTiming=" + (run.stage == LayoutSceneRepairStage.WaitingPreSave ? "before-only-Set" :
                    run.stage == LayoutSceneRepairStage.WaitingPostSave ? "after-Save" : "after-readonly-reopen");
                try { text += LayoutWriteOverrideDiff(run, sample); }
                catch { run.diagnosticsIncomplete = true; throw; }
            }
            else text += " originalScene=" + run.beforeScene + " savedScene=" + run.savedScene + " observedScene=" + run.observedScene +
                " originalPrefab=" + run.beforePrefab + " failure=" + run.failure?.GetType().Name + ":" + run.failure?.Message +
                " cleanupFailures=" + run.cleanupFailures.Count + " diagnosticsIncomplete=" + run.diagnosticsIncomplete +
                " overrideDiffPath=" + run.overrideFile + " overrideDiffExpectedBytes=" + run.overrideExpectedBytes +
                " overrideDiffBytes=" + run.overrideActualBytes + " overrideDiffSha256=" + run.overrideSha256 +
                " overrideDiffCreated=" + run.overrideFileCreated + " overrideDiffComplete=" + run.overrideComplete +
                " overrideDiffFileAttempts=" + run.overrideFiles + " overrideDiffTotalBytes=" + run.overrideBytes +
                " immediateOverridesExact=" + run.immediateOverridesExact + " immediateOverrideCount=" + run.immediateOverrideCount;
            text = (sample == null ? "[LAYOUT-N10-STAGE] " : "[LAYOUT-N10-SAMPLE] ") + text.Replace("\r", "\\r").Replace("\n", "\\n");
            if (System.Text.Encoding.UTF8.GetByteCount(text) > (sample == null ? 2048 : 8192) ||
                (sample == null ? run.stageLogs >= 8 : run.sampleLogs >= 9)) run.diagnosticsIncomplete = true;
            Require(System.Text.Encoding.UTF8.GetByteCount(text) <= (sample == null ? 2048 : 8192), "LAYOUT N10 log record budget");
            Require(sample == null ? ++run.stageLogs <= 8 : ++run.sampleLogs <= 9, "LAYOUT N10 log count budget");
            try { Debug.Log(text); }
            catch { run.diagnosticsIncomplete = true; throw; }
        }
        private static void LayoutArmSceneRepair(LayoutSceneRepairRun run, LayoutSceneRepairStage stage, string immediate)
        {
            Require(run == layoutSceneRepairOwner && !run.pending && run.failure == null, "LAYOUT N10 exclusive arm");
            run.stage = stage; var token = run.token + 1;
            LayoutLogSceneCanvas(run, LayoutCaptureSceneCanvas(run.root), immediate, token);
            Require(run == layoutSceneRepairOwner && run.stage == stage && run.failure == null, "LAYOUT N10 owner after sample");
            run.forceCalls++; Canvas.ForceUpdateCanvases();
            Require(run == layoutSceneRepairOwner && run.stage == stage && run.failure == null, "LAYOUT N10 owner after force");
            LayoutLogSceneCanvas(run, LayoutCaptureSceneCanvas(run.root), "after-force", token);
            Require(run == layoutSceneRepairOwner && run.stage == stage && run.failure == null, "LAYOUT N10 owner before queue");
            run.token = token; run.pendingToken = token; run.pending = true;
            run.armSerial = run.serial; run.ticks = 0; run.deadline = EditorApplication.timeSinceStartup + 5;
            run.queueCalls++; EditorApplication.QueuePlayerLoopUpdate();
        }
        private static void LayoutUpdateSceneRepair()
        {
            var run = layoutSceneRepairOwner;
            if (run == null || run.stage == LayoutSceneRepairStage.Finished) return;
            if (run.inCallback) { LayoutFinishSceneRepair(run, new InvalidOperationException("LAYOUT N10 callback reentry")); return; }
            run.inCallback = true;
            try
            {
                var token = run.pendingToken; run.serial++;
                Require(run.pending && token == run.token && token > 0 && run.serial > run.armSerial &&
                    EditorApplication.timeSinceStartup < run.deadline && !EditorApplication.isPlayingOrWillChangePlaymode &&
                    !UnityEngine.Application.isPlaying && !EditorApplication.isCompiling && !EditorApplication.isUpdating, "LAYOUT N10 pending update/deadline/editor state");
                if (++run.ticks == 1) return;
                Require(run.ticks == 2, "LAYOUT N10 exactly two subsequent update events");
                run.pending = false; run.pendingToken = 0; run.decisiveSamples++;
                var sample = LayoutCaptureSceneCanvas(run.root);
                LayoutLogSceneCanvas(run, sample, "after-two-editor-updates", token);
                Require(run == layoutSceneRepairOwner && run.failure == null && run.token == token &&
                    EditorApplication.timeSinceStartup < run.deadline, "LAYOUT N10 consumed owner/token/deadline");
                LayoutContinueSceneRepair(run, sample);
            }
            catch (Exception error) { LayoutFinishSceneRepair(run, error); }
            finally { run.inCallback = false; }
        }
        private static void LayoutContinueSceneRepair(LayoutSceneRepairRun run, LayoutSceneCanvasSnapshot sample)
        {
            var stage = run.stage;
            var hosts = SceneComponents<FightMatchPlayerHost>(run.scene); var events = SceneComponents<UnityEngine.EventSystems.EventSystem>(run.scene);
            Require(hosts.Length == 1 && events.Length == 1 && hosts[0].RuntimeRoot != null, "LAYOUT preserved Host/EventSystem");
            Require(LayoutSceneIds(hosts[0], events[0]).SequenceEqual(run.sceneIds), "LAYOUT scene object identities after reopen");
            var modules = events[0].GetComponents<UnityEngine.EventSystems.BaseInputModule>();
            Require(modules.Length == 1 && MonoScript.FromMonoBehaviour(modules[0]) ==
                AssetDatabase.LoadAssetAtPath<MonoScript>(PresentationPath + "FightMatchStandaloneInputModule.cs"), "LAYOUT project input module");
            Require(SceneComponents<Canvas>(run.scene).Length == 1 && SceneComponents<UnityEngine.UIElements.UIDocument>(run.scene).Length == 0 &&
                PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(hosts[0].RuntimeRoot) == UguiPrefab &&
                hosts[0].FontAsset == preparingFont && hosts[0].FontLicense != null, "LAYOUT preserved scene linkage");
            Require(LayoutVerifyRoot(run.root, true, sample).SequenceEqual(run.targets), "LAYOUT reopened instance targets");
            LayoutVerifyRepairOverrideEnvelope(run.preserved, sample.overrides);
            Require(Sha(UguiPrefab) == run.beforePrefab, "LAYOUT scene repair preserved prefab bytes");
            LayoutProtectAfter(run.activation);
            Require(run == layoutSceneRepairOwner && run.failure == null && run.stage == stage, "LAYOUT N10 continuation owner");
            if (run.stage == LayoutSceneRepairStage.WaitingPreSave)
            {
                Require(Sha(ScenePath) == run.beforeScene && run.saveCalls == 0, "LAYOUT N10 no save before A passes");
                if (!run.scene.isDirty) EditorSceneManager.MarkSceneDirty(run.scene);
                run.setCalls++;
                PrefabUtility.SetPropertyModifications(run.root, LayoutCopyModifications(run.preservedModifications));
                var immediate = LayoutOverrideKeys(PrefabUtility.GetPropertyModifications(run.root) ?? Array.Empty<PropertyModification>());
                run.immediateOverrideCount = immediate.Length;
                run.immediateOverridesExact = immediate.SequenceEqual(run.preserved);
                Require(run.immediateOverridesExact, "LAYOUT N10 immediate Set/Get exactly frozen 22");
                run.saveCalls++;
                try { run.saveReturned = EditorSceneManager.SaveScene(run.scene, ScenePath); }
                catch (Exception error) { run.failure = run.failure ?? error; throw; }
                finally { run.savedScene = Sha(ScenePath); }
                LayoutVerifyRepairDiskDelta(run.beforeSceneBytes, File.ReadAllBytes(ScenePath));
                LayoutLogSceneCanvas(run, null, "save-returned", run.token);
                Require(run.saveReturned == true && run.failure == null && run == layoutSceneRepairOwner, "LAYOUT native scene override repair save");
                LayoutArmSceneRepair(run, LayoutSceneRepairStage.WaitingPostSave, "post-save");
                return;
            }
            LayoutVerifyRepairDiskDelta(run.beforeSceneBytes, File.ReadAllBytes(ScenePath));
            Require(!sample.dirty && !run.scene.isDirty && !EditorApplication.isPlayingOrWillChangePlaymode &&
                Sha(ScenePath) == run.savedScene, "LAYOUT read-only scene reopen/post-save");
            LayoutCloseOwnedScene(run);
            Require(run == layoutSceneRepairOwner && run.failure == null && run.stage == stage, "LAYOUT N10 owner after close");
            if (run.stage == LayoutSceneRepairStage.WaitingPostSave) { LayoutOpenRepairReopen(run); return; }
            Require(run.stage == LayoutSceneRepairStage.WaitingReopen && Sha(UguiPrefab) == run.reopenPrefab &&
                Sha(ScenePath) == run.reopenScene, "LAYOUT reopen wrote native bytes");
            LayoutProtectAfter(run.activation);
            Require(run.setCalls == 1 && run.saveCalls == 1 && run.forceCalls == 3 && run.queueCalls == 3 &&
                run.serial == 6 && run.decisiveSamples == 3 && run.sampleLogs == 9, "LAYOUT N10 fixed boundary counts");
            var result = new LayoutResult { mode = "NativeReadOnlyReopen", reopened = true, protectedInputsUnchanged = true,
                prefabSha256 = run.reopenPrefab, sceneSha256 = run.reopenScene, targets = run.targets, sceneObjectIds = run.sceneIds, textCount = 141, inputCount = 3 };
            LayoutWrite(run.activation, "native-migration.json", new LayoutResult { mode = "NativeSceneOverrideRepair", reopened = false,
                protectedInputsUnchanged = true, prefabSha256 = run.beforePrefab, sceneSha256 = run.savedScene, textCount = 141, inputCount = 3 });
            LayoutWrite(run.activation, "native-reopen.json", result); LayoutWrite(run.activation, "serialized-targets.json", result);
            LayoutFinishSceneRepair(run, null);
        }
        private static void LayoutOpenRepairReopen(LayoutSceneRepairRun run)
        {
            run.reopenPrefab = Sha(UguiPrefab); run.reopenScene = Sha(ScenePath);
            run.prefab = PrefabUtility.LoadPrefabContents(UguiPrefab);
            Require(LayoutVerifyRoot(run.prefab, false).SequenceEqual(run.targets), "LAYOUT preserved prefab targets");
            PrefabUtility.UnloadPrefabContents(run.prefab); run.prefab = null;
            Require(run == layoutSceneRepairOwner && run.failure == null, "LAYOUT N10 owner before reopen");
            run.scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            var hosts = SceneComponents<FightMatchPlayerHost>(run.scene);
            Require(hosts.Length == 1 && hosts[0].RuntimeRoot != null, "LAYOUT preserved reopen Host");
            run.root = hosts[0].RuntimeRoot.gameObject;
            LayoutArmSceneRepair(run, LayoutSceneRepairStage.WaitingReopen, "post-open");
        }
        private static void LayoutFinishSceneRepair(LayoutSceneRepairRun run, Exception error)
        {
            run.failure = run.failure ?? error;
            if (run.stage == LayoutSceneRepairStage.Finished) return;
            run.stage = LayoutSceneRepairStage.Finished; run.pending = false; run.pendingToken = 0;
            EditorApplication.update -= LayoutUpdateSceneRepair;
            try
            {
                if (run.scene.IsValid() && run.scene.isLoaded) LayoutCloseOwnedScene(run);
            }
            catch (Exception cleanup) { LayoutRecordCleanupFailure(run, "close-owned-scene", cleanup); }
            try { if (run.prefab != null) PrefabUtility.UnloadPrefabContents(run.prefab); }
            catch (Exception cleanup) { LayoutRecordCleanupFailure(run, "unload-prefab", cleanup); }
            try { if (run.activation != null) run.observedScene = Sha(ScenePath); }
            catch (Exception cleanup) { LayoutRecordCleanupFailure(run, "observe-scene-hash", cleanup); }
            try { LayoutLogSceneCanvas(run, null, "finished", run.token); }
            catch (Exception logging) { LayoutRecordCleanupFailure(run, "terminal-log", logging); }
            finally
            {
                if (layoutSceneRepairOwner == run) { preparingFont = null; layoutSceneRepairOwner = null; }
            }
            if (!UnityEngine.Application.isBatchMode) throw run.failure ?? new InvalidOperationException("LAYOUT N10 batch only");
            EditorApplication.Exit(run.failure == null ? 0 : 1);
        }
        public static void RepairLayoutSceneOverrides()
        {
            var run = layoutSceneRepairOwner ?? new LayoutSceneRepairRun();
            try
            {
                Require(layoutSceneRepairOwner == null, "LAYOUT N10 duplicate owner entry");
                run.activation = LayoutGuard("RepairLayoutSceneOverrides"); layoutSceneRepairOwner = run;
                run.beforePrefab = Sha(UguiPrefab); run.beforeScene = Sha(ScenePath);
                run.beforeSceneBytes = File.ReadAllBytes(ScenePath);
                preparingFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(UguiFont); Require(preparingFont != null, "LAYOUT existing font");
                var persistent = AssetDatabase.LoadAssetAtPath<GameObject>(UguiPrefab); Require(persistent != null, "LAYOUT existing migrated prefab");
                run.targets = LayoutVerifyRoot(persistent, false);
                Require(run == layoutSceneRepairOwner && run.failure == null, "LAYOUT N10 owner before open");
                run.scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                var hosts = SceneComponents<FightMatchPlayerHost>(run.scene); var events = SceneComponents<UnityEngine.EventSystems.EventSystem>(run.scene);
                Require(hosts.Length == 1 && events.Length == 1 && hosts[0].RuntimeRoot != null, "LAYOUT existing scene repair objects");
                run.root = hosts[0].RuntimeRoot.gameObject; run.sceneIds = LayoutSceneIds(hosts[0], events[0]);
                LayoutSceneCanvas(run.root, out var source);
                run.originalModifications = LayoutCopyModifications(PrefabUtility.GetPropertyModifications(run.root) ?? Array.Empty<PropertyModification>());
                run.preservedModifications = LayoutCopyModifications(run.originalModifications.Where(x => !LayoutStaleCanvasOverride(x, source)).ToArray());
                Require(run.originalModifications.Length == 27 && run.preservedModifications.Length == 22, "LAYOUT N10 exactly five stale overrides");
                run.originalOverrides = LayoutOverrideKeys(run.originalModifications);
                run.preserved = LayoutOverrideKeys(run.preservedModifications);
                Require(run.originalOverrides.SequenceEqual(LayoutRepairDiskKeys(run.beforeSceneBytes, false)), "LAYOUT N10 frozen original memory/disk baseline");
                Require(run == layoutSceneRepairOwner && run.failure == null, "LAYOUT N10 owner before subscription");
                EditorApplication.update += LayoutUpdateSceneRepair;
                LayoutArmSceneRepair(run, LayoutSceneRepairStage.WaitingPreSave, "pre-update");
            }
            catch (Exception error) { LayoutFinishSceneRepair(run, error); }
        }

        public static void RecoverUguiBoardRenderer()
        {
            var output = Arg("-ugui01Output");
            var candidate = Arg("-ugui01Candidate");
            Require(UnityEngine.Application.isBatchMode && Path.GetDirectoryName(UnityEngine.Application.dataPath) == Project &&
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX, "UGUI correction project/batch/target");
            Require(Regex.IsMatch(candidate ?? "", "^[a-f0-9]{64}$") && candidate != "b9f2f64bf2aaeb930f83afdd6c1290a5895386cd870b1f7f4c07f7d0f4591669" &&
                candidate != "f21c8c41b76817cafc1ec7be812f115ac65e74122c476c60b0c559834dcc1486" && candidate != "8a3cd796241ceb6fb953544771b8a4edb4bdf272d79a3631d6367dd7ef1b1c43", "UGUI correction new candidate");
            Require(output == Project + "/TestArtifacts/FightMatch/UGUI-01/" + candidate +
                "/q4-correction-02/resources/recovery/renderer-recovery.json" && !File.Exists(output) &&
                Directory.Exists(Path.GetDirectoryName(output)), "UGUI correction exact create-new output");
            var run = JsonUtility.FromJson<UguiCorrectionRun>(File.ReadAllText(Path.Combine(Path.GetDirectoryName(output), "run.json")));
            Require(run.stageId == "UGUI-01-Q4-CORRECTION-FIX-02" && run.mode == "RecoverBoardRenderer" &&
                run.authorThreadId == "01a0e404-d89d-7ab2-bece-3cd1df3fbc52" &&
                !string.IsNullOrEmpty(run.authorTurnId) && run.authorTurnId == Arg("-ugui01AuthorTurn") &&
                run.candidateSha256 == candidate && run.sourceSha256 == Sha("Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs") &&
                run.packetSha256 == "7ef805179f0d4f7d4b7585857679f7124a7041b1f7a3b50417fc8bfbbb375f12" &&
                run.packetSha256 == Sha("docs/team/2026-09-30/engineering-ugui-01-q4-correction-fix-02.md") &&
                run.argv != null && run.argv.Contains(output) && run.argv.Contains(candidate), "UGUI correction author/source/candidate/packet");
            Require(run.sources != null && run.sources.Length == 53 && run.resources != null && run.resources.Length == 37,
                "UGUI correction exact manifests");
            Require(run.sources.Select(x => x.path).SequenceEqual(run.sources.Select(x => x.path).OrderBy(x => x, StringComparer.Ordinal)) &&
                run.sources.Select(x => x.path).Distinct().Count() == 53 && run.resources.Select(x => x.path).Distinct().Count() == 37,
                "UGUI correction manifest paths");
            foreach (var identity in run.sources.Concat(run.resources)) VerifyUguiCorrectionIdentity(identity);
            var canonical = "[" + string.Join(",", run.sources.Select(x => "{\"bytes\":" + x.bytes.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                ",\"path\":\"" + x.path + "\",\"sha256\":\"" + x.sha256 + "\"}")) + "]";
            using (var sha = SHA256.Create())
                Require(string.Concat(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(canonical)).Select(x => x.ToString("x2"))) == candidate,
                    "UGUI correction source manifest candidate");
            var correctedPaths = new[] {
                PresentationPath + "CandidateBoardElement.cs", "Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs",
                "Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs", "Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs",
                "Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs", PresentationPath + "PlayerNavigationRecoveryView.cs",
                "Assets/Tests/EditMode/FightMatch/PlayerNavigationPresentationTests.cs" };
            Require(correctedPaths.All(path => run.sources.Count(x => x.path == path) == 1) &&
                new[] { UguiPrefab, ScenePath, UguiFont, UguiPrefab + ".meta" }.All(path => run.resources.Count(x => x.path == path) == 1),
                "UGUI correction source/resource coverage");
            var prefabIdentity = run.resources.Single(x => x.path == UguiPrefab);
            Require(prefabIdentity.bytes == 1060275 && prefabIdentity.sha256 == run.prefabSha256 &&
                run.prefabSha256 == "934e5bbec0c6cec2a16afa4778fb019a3420e3432cc0d75e07703e6f4cd60487" && run.prefabGuid == "c36df3cfc25424c8cb3ec6cae6be1237" &&
                AssetDatabase.AssetPathToGUID(UguiPrefab) == run.prefabGuid, "UGUI correction original prefab identity");
            var before = UguiPrefabObjects(File.ReadAllText(UguiPrefab));
            var boardObject = before.Where(x => Regex.IsMatch(x.Value, @"(?m)^  m_Name: BoardFrame\r?$")).Select(x => x.Key).ToArray();
            Require(boardObject.Length == 1 && boardObject[0] == "4805194842302782578", "UGUI correction serialized BoardFrame identity");
            var componentIds = Regex.Matches(before[boardObject[0]], @"(?m)^  - component: \{fileID: (-?\d+)\}\r?$").Cast<Match>()
                .Select(x => x.Groups[1].Value).ToArray();
            Require(componentIds.SequenceEqual(new[] { "871518044355574362", "64232663904662438", "3845429580288467919", "7782439111565559686" }), "UGUI correction original component fileIDs");
            var classIds = componentIds.Select(x => int.Parse(Regex.Match(before[x], @"^--- !u!(\d+) ").Groups[1].Value)).ToArray();
            var scriptGuids = componentIds.Select(x => Regex.Match(before[x], @"(?m)^  m_Script: \{fileID: \d+, guid: ([a-f0-9]{32}), type: 3\}\r?$").Groups[1].Value).ToArray();
            Require(classIds.SequenceEqual(new[] { 224, 114, 114, 114 }) && scriptGuids.SequenceEqual(new[] { "", "306cc8c2b49d7114eaa3623786fc2126", "5a7f13aa50aa909428d61ff8d87781d2", "18c85b32e67ef4eb592bedc43df1e561" }),
                "UGUI correction original component class IDs/script GUIDs");
            var diskRendererCount = classIds.Count(x => x == 222);
            Require(diskRendererCount == 0, "UGUI correction serialized renderer absent");
            var nativeRendererCount = 0; string[] nativeComponentTypes = null;
            var root = PrefabUtility.LoadPrefabContents(UguiPrefab);
            Require(root != null, "UGUI correction native root loaded");
            try
            {
                var frames = root.GetComponentsInChildren<Transform>(true).Where(x => x.name == "BoardFrame").ToArray();
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(PresentationPath + "CandidateBoardElement.cs");
                Require(frames.Length == 1, "UGUI correction native BoardFrame exactly one");
                Require(script != null && AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(script)) == "5a7f13aa50aa909428d61ff8d87781d2",
                    "UGUI correction native board script identity");
                var boards = root.GetComponentsInChildren<MonoBehaviour>(true).Where(x => x != null && MonoScript.FromMonoBehaviour(x) == script).ToArray();
                Require(boards.Length == 1, "UGUI correction native CandidateBoardElement exactly one");
                Require(boards[0].gameObject == frames[0].gameObject && boards[0].enabled, "UGUI correction native board owner/enabled");
                nativeRendererCount = frames[0].GetComponents<CanvasRenderer>().Length;
                nativeComponentTypes = frames[0].GetComponents<Component>().Select(x => x == null ? "<missing>" : x.GetType().FullName).ToArray();
                Require(nativeRendererCount == 1, "UGUI correction native renderer reused exactly one");
                foreach (var identity in run.resources) VerifyUguiCorrectionIdentity(identity);
                Require(PrefabUtility.SaveAsPrefabAsset(root, UguiPrefab) != null, "UGUI correction native prefab save");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Require(AssetDatabase.AssetPathToGUID(UguiPrefab) == run.prefabGuid, "UGUI correction prefab GUID preserved");
            foreach (var identity in run.resources.Where(x => x.path != UguiPrefab)) VerifyUguiCorrectionIdentity(identity);
            var after = UguiPrefabObjects(File.ReadAllText(UguiPrefab));
            var added = after.Keys.Except(before.Keys).ToArray();
            Require(added.Length == 1 && after.Count == before.Count + 1 && after[added[0]].StartsWith("--- !u!222 ", StringComparison.Ordinal),
                "UGUI correction exactly one new CanvasRenderer fileID");
            foreach (var pair in before)
            {
                Require(after.ContainsKey(pair.Key), "UGUI correction existing fileID preserved");
                var actual = after[pair.Key];
                if (pair.Key == boardObject[0])
                {
                    var componentLine = @"(?m)^  - component: \{fileID: " + Regex.Escape(added[0]) + @"\}\r?\n";
                    Require(Regex.Matches(actual, componentLine).Count == 1, "UGUI correction one new BoardFrame component reference");
                    actual = Regex.Replace(actual, componentLine, "");
                }
                Require(actual == pair.Value, "UGUI correction existing serialized object unchanged " + pair.Key);
            }
            Require(Regex.IsMatch(after[added[0]], @"(?m)^  m_GameObject: \{fileID: " + Regex.Escape(boardObject[0]) + @"\}\r?$"),
                "UGUI correction serialized renderer owner");
            var savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(UguiPrefab);
            var savedFrame = savedPrefab.GetComponentsInChildren<Transform>(true).Single(x => x.name == "BoardFrame");
            Require(savedFrame.GetComponents<CanvasRenderer>().Length == 1, "UGUI correction saved renderer exactly one");
            File.WriteAllText(output, JsonUtility.ToJson(new UguiCorrectionResult { candidateSha256 = candidate, prefabGuid = run.prefabGuid,
                beforePrefabSha256 = run.prefabSha256, afterPrefabSha256 = Sha(UguiPrefab), addedCanvasRendererFileId = added[0],
                originalObjectCount = before.Count, finalObjectCount = after.Count, otherResourcesUnchanged = true,
                diskRendererCountBefore = diskRendererCount, nativeRendererCountBefore = nativeRendererCount,
                serializedRendererCountAfter = 1, nativeComponentTypes = nativeComponentTypes }, true));
        }
        private static void VerifyUguiCorrectionIdentity(UguiCorrectionIdentity identity)
        {
            Require(identity != null && Regex.IsMatch(identity.path ?? "", @"^Assets/[A-Za-z0-9_ ./-]+$") && !identity.path.Contains("..") &&
                Regex.IsMatch(identity.sha256 ?? "", "^[a-f0-9]{64}$") && File.Exists(identity.path) &&
                new FileInfo(identity.path).Length == identity.bytes && Sha(identity.path) == identity.sha256,
                "UGUI correction frozen file identity " + identity?.path);
        }
        private static System.Collections.Generic.Dictionary<string, string> UguiPrefabObjects(string text) =>
            Regex.Matches(text, @"(?ms)^--- !u!\d+ &(-?\d+)\r?\n.*?(?=^--- !u!|\z)").Cast<Match>()
                .ToDictionary(x => x.Groups[1].Value, x => x.Value, StringComparer.Ordinal);
        [Serializable] private sealed class UguiCorrectionIdentity
        { public string path, sha256; public long bytes; }
        [Serializable] private sealed class UguiCorrectionRun
        {
            public string stageId, mode, authorThreadId, authorTurnId, candidateSha256, sourceSha256, packetSha256, prefabSha256, prefabGuid;
            public string[] argv;
            public UguiCorrectionIdentity[] sources, resources;
        }
        [Serializable] private sealed class UguiCorrectionResult
        {
            public string candidateSha256, prefabGuid, beforePrefabSha256, afterPrefabSha256, addedCanvasRendererFileId;
            public int originalObjectCount, finalObjectCount, diskRendererCountBefore, nativeRendererCountBefore, serializedRendererCountAfter;
            public string[] nativeComponentTypes;
            public bool otherResourcesUnchanged;
        }

        private static string UguiOutput(string leaf, string mode)
        {
            var output = Arg("-ugui01Output");
            Require(UnityEngine.Application.isBatchMode && Path.GetDirectoryName(UnityEngine.Application.dataPath) == Project, "UGUI-01 project/batch identity");
            Require(mode != "VerifyResources" || !new[] { "b9f2f64bf2aaeb930f83afdd6c1290a5895386cd870b1f7f4c07f7d0f4591669", "f21c8c41b76817cafc1ec7be812f115ac65e74122c476c60b0c559834dcc1486", "8a3cd796241ceb6fb953544771b8a4edb4bdf272d79a3631d6367dd7ef1b1c43" }.Any(value =>
                (output ?? "").StartsWith(Project + "/TestArtifacts/FightMatch/UGUI-01/" + value + "/", StringComparison.Ordinal)),
                "UGUI-01 historical output roots read-only");
            Require(Regex.IsMatch(output ?? "", "^" + Regex.Escape(Project + "/TestArtifacts/FightMatch/UGUI-01/") +
                "[a-f0-9]{64}/resources/" + (mode == "PrepareResources" ? "prepare/" : "reopen/") + Regex.Escape(leaf) + "$") ||
                (mode == "VerifyResources" && leaf == "resource-reopen.json" &&
                 Regex.IsMatch(output ?? "", "^" + Regex.Escape(Project + "/TestArtifacts/FightMatch/UGUI-01/") +
                    "[a-f0-9]{64}/q4-correction-02/resources/(reopen|final-reopen)/resource-reopen\\.json$")), "UGUI-01 exact output path");
            Require(!File.Exists(output) && Directory.Exists(Path.GetDirectoryName(output)), "UGUI-01 create-new output");
            var bindingPath = Path.Combine(Path.GetDirectoryName(output), "run.json");
            Require(File.Exists(bindingPath), "UGUI-01 resource run identity");
            var run = JsonUtility.FromJson<UguiResourceRun>(File.ReadAllText(bindingPath));
            Require(run.stageId == "UGUI-01" && run.authorThreadId == "01a0e404-d89d-7ab2-bece-3cd1df3fbc52" &&
                !string.IsNullOrEmpty(run.authorTurnId) && run.authorTurnId == Arg("-ugui01AuthorTurn") && run.mode == mode &&
                run.sourceSha256 == Sha("Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs") &&
                run.argv != null && run.argv.Contains(output), "UGUI-01 source/author/mode binding");
            Require(EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX, "UGUI-01 Mac resource target");
            return output;
        }
        public static void PrepareResources()
        {
            var output = UguiOutput("resource-preparation.json", "PrepareResources");
            var initialScene = SceneManager.GetActiveScene();
            Require(SceneManager.sceneCount == 1 && string.IsNullOrEmpty(initialScene.path) && !initialScene.isDirty, "Clean batch startup scene required");
            Require(Sha(ScenePath) == "257708df579d05a29eb3edd945214b5308297e88e3c8cd91bbc2cc8aae0e2f40", "UGUI-01 original scene");
            Require(!File.Exists(UguiPrefab) && !File.Exists(UguiFont), "UGUI-01 new assets only; repair requires a scoped recovery");
            ValidateOfficialTmp();
            var source = AssetDatabase.LoadAssetAtPath<Font>(Ui + "Fonts/NotoSansCJKsc-Regular.otf");
            Require(source != null && AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source)) == "960a9cda7a6b34935b8c18586488d314", "Pinned Noto source");
            preparingFont = TMPro.TMP_FontAsset.CreateFontAsset(source, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                1024, 1024, TMPro.AtlasPopulationMode.Dynamic, true);
            Require(preparingFont != null, "Noto TMP creation");
            preparingFont.name = "NotoSansCJKsc-Regular-TMP";
            preparingFont.material.shader = Shader.Find("TextMeshPro/Distance Field");
            preparingFont.material.name = "NotoSansCJKsc-Regular-TMP Material";
            AssetDatabase.CreateAsset(preparingFont, UguiFont);
            AssetDatabase.AddObjectToAsset(preparingFont.material, preparingFont);
            foreach (var atlas in preparingFont.atlasTextures)
            { atlas.name = preparingFont.name + " Atlas"; AssetDatabase.AddObjectToAsset(atlas, preparingFont); }
            Require(preparingFont.TryAddCharacters(Placeholder, out var missing) && string.IsNullOrEmpty(missing), "Noto diagnostic glyphs");
            var settings = AssetDatabase.LoadAssetAtPath<TMPro.TMP_Settings>(TmpSettingsPath);
            Require(settings != null, "Official TMP Settings");
            var fields = new SerializedObject(settings);
            fields.FindProperty("m_defaultFontAsset").objectReferenceValue = preparingFont;
            fields.FindProperty("m_defaultFontAssetPath").stringValue = Ui + "Fonts/";
            fields.FindProperty("m_fallbackFontAssets").arraySize = 0;
            fields.FindProperty("m_defaultSpriteAsset").objectReferenceValue = null;
            fields.FindProperty("m_defaultStyleSheet").objectReferenceValue = null;
            fields.FindProperty("m_defaultSpriteAssetPath").stringValue = "";
            fields.FindProperty("m_defaultColorGradientPresetsPath").stringValue = "";
            fields.FindProperty("m_enableEmojiSupport").boolValue = false;
            fields.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(settings); AssetDatabase.SaveAssets();
            if (!AssetDatabase.IsValidFolder(Ui + "Runtime")) AssetDatabase.CreateFolder(Ui.TrimEnd('/'), "Runtime");
            var root = BuildUguiRoot();
            GameObject prefab;
            try
            {
                prefab = PrefabUtility.SaveAsPrefabAsset(root, UguiPrefab);
                Require(prefab != null, "Root Prefab save");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var hosts = SceneComponents<FightMatchPlayerHost>(scene);
            Require(hosts.Length == 1, "One existing Host");
            var host = hosts[0];
            var oldDocument = host.GetComponent<UnityEngine.UIElements.UIDocument>();
            Require(oldDocument != null, "Expected old document for migration");
            UnityEngine.Object.DestroyImmediate(oldDocument);
            Require(SceneComponents<UnityEngine.EventSystems.EventSystem>(scene).Length == 0, "No pre-existing EventSystem");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, host.transform);
            References(host, "fontAsset", preparingFont, "fontLicense", AssetDatabase.LoadAssetAtPath<TextAsset>(Ui + "Fonts/OFL.txt"),
                "runtimeRoot", instance.GetComponent<FightMatchHostView>());
            instance.SetActive(true);
            var events = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
            ComponentFromScript(events, "FightMatchStandaloneInputModule");
            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene, ScenePath), "UGUI scene save"); AssetDatabase.SaveAssets();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            File.WriteAllText(output, JsonUtility.ToJson(new UguiResourceResult {
                stage = "UGUI-01", mode = "Prepared; independent process reopen pending", scene = ScenePath, prefab = UguiPrefab,
                font = UguiFont, sceneSha256 = Sha(ScenePath), prefabSha256 = Sha(UguiPrefab), fontSha256 = Sha(UguiFont) }, true));
            preparingFont = null;
        }
        public static void VerifyUguiResources()
        {
            var output = UguiOutput("resource-reopen.json", "VerifyResources");
            ValidateOfficialTmp();
            var font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(UguiFont);
            var settings = AssetDatabase.LoadAssetAtPath<TMPro.TMP_Settings>(TmpSettingsPath);
            Require(font != null && settings != null && TMPro.TMP_Settings.instance == settings && TMPro.TMP_Settings.defaultFontAsset == font, "Reopened TMP settings/font");
            Require(font.material != null && font.material.shader == Shader.Find("TextMeshPro/Distance Field") && AssetDatabase.IsSubAsset(font.material) &&
                font.atlasTextures.Length > 0 && font.atlasTextures.All(x => x != null && AssetDatabase.IsSubAsset(x)) &&
                font.atlasPopulationMode == TMPro.AtlasPopulationMode.Dynamic && font.isMultiAtlasTexturesEnabled &&
                font.atlasWidth == 1024 && font.atlasHeight == 1024 && font.atlasPadding == 9, "Reopened embedded dynamic Noto font");
            var settingsFields = new SerializedObject(settings);
            foreach (var name in new[] { "m_leadingCharacters", "m_followingCharacters" })
                Require(settingsFields.FindProperty(name).objectReferenceValue != null, "Reopened CJK line-breaking table");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            try
            {
                var hosts = SceneComponents<FightMatchPlayerHost>(scene);
                var canvases = SceneComponents<UnityEngine.Canvas>(scene); var events = SceneComponents<UnityEngine.EventSystems.EventSystem>(scene);
                Require(hosts.Length == 1 && canvases.Length == 1 && events.Length == 1, "Single Host/Canvas/EventSystem");
                Require(SceneComponents<UnityEngine.UIElements.UIDocument>(scene).Length == 0, "Zero runtime UIDocument");
                var modules = events[0].GetComponents<UnityEngine.EventSystems.BaseInputModule>();
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(PresentationPath + "FightMatchStandaloneInputModule.cs");
                Require(modules.Length == 1 && MonoScript.FromMonoBehaviour(modules[0]) == script, "Project input module only");
                Require(hosts[0].RuntimeRoot != null && hosts[0].FontAsset == font && hosts[0].FontLicense != null &&
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(hosts[0].RuntimeRoot) == UguiPrefab, "Reopened Host resource bindings");
                var scaler = canvases[0].GetComponent<UnityEngine.UI.CanvasScaler>();
                Require(canvases[0].renderMode == RenderMode.ScreenSpaceOverlay && scaler.referenceResolution == new Vector2(540, 960) &&
                    scaler.matchWidthOrHeight == .5f && canvases[0].GetComponent<UnityEngine.UI.GraphicRaycaster>() != null, "Overlay/scaler/raycaster");
                var texts = hosts[0].RuntimeRoot.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                var linkedInputs = 0;
                foreach (var text in texts)
                {
                    Require(text.font == font && text.fontSharedMaterial == font.material, "TMP font/material binding");
                    var input = text.GetComponentInParent<TMPro.TMP_InputField>(true);
                    if (input != null && ReferenceEquals(input.textComponent, text))
                    {
                        linkedInputs++;
                        Require(input.text == Placeholder && (text.text == Placeholder || text.text == Placeholder + "\u200B"),
                            "TMP input placeholder and single caret sentinel");
                    }
                    else Require(text.text == Placeholder, "TMP exact placeholder binding");
                }
                Require(texts.Length == 119 && linkedInputs == 3 && texts.Length - linkedInputs == 116, "TMP placeholder component counts");
                foreach (var component in scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Component>(true)))
                {
                    Require(component != null, "Missing scene script");
                    var serialized = new SerializedObject(component); var property = serialized.GetIterator();
                    while (property.NextVisible(true))
                        if (property.propertyType == SerializedPropertyType.ObjectReference)
                            Require(property.objectReferenceValue != null || property.objectReferenceInstanceIDValue == 0, "Missing serialized reference " + property.propertyPath);
                }
                var oldPaths = new[] { Ui + "FightMatchPanelSettings.asset", Ui + "FightMatchTheme.tss", Ui + "Fonts/NotoSansCJKsc-Regular.asset" };
                var dependencies = AssetDatabase.GetDependencies(new[] { ScenePath, UguiPrefab, UguiFont, TmpSettingsPath }, true);
                Require(!dependencies.Intersect(oldPaths).Any(), "No new old-resource references");
            }
            finally { EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); }
            File.WriteAllText(output, JsonUtility.ToJson(new UguiResourceResult { stage = "UGUI-01", mode = "Independent process reopened",
                scene = ScenePath, prefab = UguiPrefab, font = UguiFont, sceneSha256 = Sha(ScenePath),
                prefabSha256 = Sha(UguiPrefab), fontSha256 = Sha(UguiFont) }, true));
        }
        private static T[] SceneComponents<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<T>(true)).ToArray();
        private static void ValidateOfficialTmp()
        {
            var paths = new[] { "Assets/TextMesh Pro", "Assets/TextMesh Pro/Resources", TmpSettingsPath,
                "Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt", "Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt",
                "Assets/TextMesh Pro/Shaders", "Assets/TextMesh Pro/Shaders/TMP_SDF.shader", "Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader",
                "Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc", "Assets/TextMesh Pro/Shaders/TMPro.cginc" };
            var guids = new[] { "f54d1bd14bd3ca042bd867b519fee8cc", "243e06394e614e5d99fab26083b707fa", "3f5b5dff67a942289a9defa416b206f3",
                "d82c1b31c7e74239bff1220585707d2b", "fade42e8bc714b018fac513c043d323b", "e9f693669af91aa45ad615fc681ed29f",
                "68e6db2ebdc24f95958faec2be5558d6", "fe393ace9b354375a9cb14cdbbc28be4", "3997e2241185407d80309a82f9148466", "407bc68d299748449bbf7f48ee690f8d" };
            for (var i = 0; i < paths.Length; i++) Require(AssetDatabase.AssetPathToGUID(paths[i]) == guids[i], "Official TMP GUID " + paths[i]);
            Require(Sha(paths[7]) == "970db4b1e73b8289c1dd56cff60877f426b49c9682620b2d14da6cd70b64cb9c" &&
                Sha(paths[7] + ".meta") == "f2112f50c72762d4e6731391495ea11f76975d677b698e9bcfdd38275ea18f9d", "Official Mobile fallback bytes");
            Require(Shader.Find("TextMeshPro/Distance Field") != null && Shader.Find("TextMeshPro/Mobile/Distance Field") != null,
                "SDF standard and Mobile shader dependency");
            Require(File.ReadAllText(paths[6]).Contains("Fallback \"TextMeshPro/Mobile/Distance Field\""), "Official fallback declaration");
            foreach (var path in new[] { paths[6], paths[7] })
                Require(!ShaderUtil.ShaderHasError(AssetDatabase.LoadAssetAtPath<Shader>(path)), "TMP shader compilation dependency " + path);
        }
        [Serializable] private sealed class UguiResourceRun
        { public string stageId, authorThreadId, authorTurnId, mode, sourceSha256; public string[] argv; }
        [Serializable] private sealed class UguiResourceResult
        { public string stage, mode, scene, prefab, font, sceneSha256, prefabSha256, fontSha256; }
        private static void CheckProduction()
        {
            Require(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android, "Android target");
            Require(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) == "com.yyczz1.fightmatch", "Production package");
            Require(PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP &&
                PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64, "IL2CPP/ARM64");
            Require(PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android) == ManagedStrippingLevel.Minimal, "Minimal stripping");
            Require((int)PlayerSettings.Android.minSdkVersion == 22 && (int)PlayerSettings.Android.targetSdkVersion == 32 &&
                PlayerSettings.Android.bundleVersionCode == 1 && PlayerSettings.bundleVersion == "0.1", "Version/SDK");
            Require(!EditorUserBuildSettings.buildAppBundle && !PlayerSettings.Android.useAPKExpansionFiles, "Single APK required");
            Require(EditorUserBuildSettings.androidCreateSymbols == AndroidCreateSymbols.Disabled, "No separate symbols package");
        }
        [System.Runtime.InteropServices.DllImport("libSystem.B.dylib", EntryPoint = "umask")]
        private static extern ushort ProcessUmask(ushort mask);
        private static string UnixMetadata(string program, params string[] arguments)
        {
            var escaped = arguments.Select(x => "\"" + x.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
            var start = new System.Diagnostics.ProcessStartInfo(program, string.Join(" ", escaped)) {
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            using (var process = System.Diagnostics.Process.Start(start))
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                Require(process.ExitCode == 0 || (program == "/usr/bin/find" && process.ExitCode == 1 && error.Length == 0),
                    "Private cache metadata check failed");
                return output.TrimEnd();
            }
        }
        private static void CheckPrivateCaches()
        {
            Require(ProcessUmask(63) == 63, "Unity must inherit umask 077 before signing");
            var gradle = Environment.GetEnvironmentVariable("GRADLE_USER_HOME");
            Require(gradle == "/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/AndroidBuildCache/FightMatch/gradle-user-home", "Private Gradle root");
            var paths = new[] { Project + "/Library/Bee", gradle, gradle + "/gradle.properties" };
            var metadata = paths.Select(path => {
                for (var current = path; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
                    Require((File.GetAttributes(current) & FileAttributes.ReparsePoint) == 0, "Private cache path contains a link");
                var expected = path.EndsWith(".properties", StringComparison.Ordinal) ? "501:600:Regular File" : "501:700:Directory";
                var observed = UnixMetadata("/usr/bin/stat", "-f", "%u:%Lp:%HT", path);
                Require(observed == expected, "Private cache owner, mode or type differs: " + path);
                Require(!Regex.IsMatch(UnixMetadata("/bin/ls", "-lde", path), @"(?m)^\s*\d+:"), "Private cache has an ACL");
                return path + "|" + observed + "|no ACL|umask 077";
            }).ToArray();
            Require(File.ReadAllText(gradle + "/gradle.properties") == "org.gradle.daemon=false\n", "Dedicated no-daemon configuration differs");
            saved.privateCacheMetadata = saved.privateCacheMetadata.Concat(metadata).ToArray();
            Require(UnixMetadata("/usr/bin/find", paths[0], gradle, "-type", "l", "-print", "-quit").Length == 0,
                "Private generated cache contains a link");
            foreach (var path in UnixMetadata("/usr/bin/find", paths[0], gradle, "-type", "f", "-exec", "/usr/bin/grep",
                "-a", "-l", "-F", "-f", Signing + "fightmatch-local-release.pass", "{}", "+").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var before = UnixMetadata("/usr/bin/stat", "-f", "%u:%Lp:%HT", path);
                Require(before.StartsWith("501:", StringComparison.Ordinal) && before.EndsWith(":Regular File", StringComparison.Ordinal), "Private signing cache ownership/type");
                Require(!Regex.IsMatch(UnixMetadata("/bin/ls", "-lde", path), @"(?m)^\s*\d+:"), "Private signing file has an ACL");
                if (before != "501:600:Regular File") UnixMetadata("/bin/chmod", "600", path);
                Require(UnixMetadata("/usr/bin/stat", "-f", "%u:%Lp:%HT", path) == "501:600:Regular File", "Private signing file mode");
                saved.privateCacheMetadata = saved.privateCacheMetadata.Concat(new[] { path + "|501:600:Regular File|no ACL" }).ToArray();
            }
        }
        private static void Sign()
        {
            CheckPrivateCaches();
            Require(File.Exists(Signing + "fightmatch-local-release.jks") && File.Exists(Signing + "fightmatch-local-release.pass"), "Preserved signing key");
            var password = File.ReadAllText(Signing + "fightmatch-local-release.pass").TrimEnd('\r', '\n');
            Require(password.Length >= 24, "Signing password format");
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = Signing + "fightmatch-local-release.jks";
            PlayerSettings.Android.keyaliasName = "fightmatch";
            PlayerSettings.Android.keystorePass = password;
            PlayerSettings.Android.keyaliasPass = password;
        }
        public static void BuildDemo()
        {
            GuardOutput(Arg("-fm029Output"), "fightmatch-demo.apk");
            CheckProduction();
            saved = Settings.Capture();
            WriteSettings();
            try
            {
                Sign();
                Directory.CreateDirectory(Path.GetDirectoryName(apk));
                actualReport = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath },
                    target = BuildTarget.Android, targetGroup = BuildTargetGroup.Android,
                    locationPathName = apk, options = BuildOptions.Development });
                WriteReport(actualReport);
                Require(actualReport.summary.result == BuildResult.Succeeded && File.Exists(apk), "Demo build failed");
            }
            finally { Restore(); }
        }
        public BuildPlayerOptions ModifyOptions(BuildPlayerOptions options)
        {
            if (!qa) return options;
            Require(saved != null && options.target == BuildTarget.Android && Arg("-assemblyNames") == "FightMatch.Android.Tests", "QA modifier binding");
            Require(options.scenes.Length >= 1 && Regex.IsMatch(options.scenes[0], "^Assets/InitTestScene[0-9]+\\.unity$"), "One UTF bootstrap scene");
            Require(Directory.GetFiles("Assets", "InitTestScene*.unity").Length == 1, "Unexpected temporary scene");
            temporaryScene = options.scenes[0];
            saved.temporaryScene = temporaryScene;
            saved.temporarySceneSha256 = Sha(temporaryScene);
            saved.temporaryMetaSha256 = Sha(temporaryScene + ".meta");
            saved.temporarySceneBytes = new FileInfo(temporaryScene).Length;
            saved.temporaryMetaBytes = new FileInfo(temporaryScene + ".meta").Length;
            options.locationPathName = apk;
            options.scenes = new[] { temporaryScene };
            options.options &= ~(BuildOptions.AutoRunPlayer | BuildOptions.ConnectToHost | BuildOptions.WaitForPlayerConnection |
                BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler);
            Require((options.options & BuildOptions.IncludeTestAssemblies) != 0, "QA test assemblies required");
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.yyczz1.fightmatch.qa");
            Sign();
            WriteSettings();
            buildObserved = true;
            Directory.CreateDirectory(Path.GetDirectoryName(apk));
            return options;
        }
        public void OnPostprocessBuild(BuildReport report)
        {
            if (saved == null || report.summary.outputPath != apk) return;
            actualReport = report;
            WriteReport(report);
        }
        public void OnPreprocessBuild(BuildReport report)
        {
            if (saved != null && report.summary.outputPath == apk) actualReport = report;
        }
        private static void OnLog(string text, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) loggedError = true;
        }
        private static void ObserveQaCompletion()
        {
            if (finished || finishing || BuildPipeline.isBuildingPlayer || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (!buildObserved && !loggedError) return;
            finishing = true;
            EditorApplication.delayCall += FinishQa;
        }
        private static void FinishQa()
        {
            var success = false;
            try
            {
                Require(actualReport != null && actualReport.summary.outputPath == apk, "No current QA BuildReport");
                WriteReport(actualReport);
                saved.naturalCleanup = temporaryScene != null && !File.Exists(temporaryScene) && !File.Exists(temporaryScene + ".meta") &&
                    saved.ContextMatches() && saved.sceneSetup.SequenceEqual(SceneState());
                Require(saved.naturalCleanup, "UTF natural cleanup/Dispose incomplete");
                Require(actualReport.summary.result == BuildResult.Succeeded && File.Exists(apk) && !loggedError, "QA build failed");
                success = true;
            }
            catch (Exception error) { saved.failure = error.Message; Debug.LogError(error.Message); }
            finally
            {
                try { Restore(); }
                catch (Exception error) { success = false; saved.failure = error.Message; WriteSettings(); Debug.LogError(error.Message); }
                finished = true;
                EditorApplication.update -= ObserveQaCompletion;
                EditorApplication.Exit(success && saved.restored ? 0 : 1);
            }
        }
        private static bool RestoreBeforeQuit()
        {
            if (saved != null && !saved.restored) Restore();
            return true;
        }
        private static string[] SceneState() => EditorSceneManager.GetSceneManagerSetup()
            .Select(x => x.path + "|" + x.isLoaded + "|" + x.isActive).ToArray();
        private static void WriteSettings() => File.WriteAllText(runDirectory + "/temporary-settings.json", JsonUtility.ToJson(saved, true));
        private static void Restore()
        {
            if (saved == null || saved.restored) return;
            saved.Apply();
            if (qa && !saved.sceneSetup.SequenceEqual(SceneState())) EditorSceneManager.RestoreSceneManagerSetup(originalScenes);
            if (qa && temporaryScene != null && File.Exists(temporaryScene)) AssetDatabase.DeleteAsset(temporaryScene);
            AssetDatabase.SaveAssets();
            Require(saved.ContextMatches() && saved.PlatformMatches(), "Temporary fields did not restore");
            Require(!qa || saved.sceneSetup.SequenceEqual(SceneState()), "Original scenes did not restore");
            saved.settingsBeforeExactRestoreSha256 = Sha(SettingsPath);
            saved.buildSettingsBeforeExactRestoreSha256 = Sha(BuildSettingsPath);
            // Restore the captured serialization as well as the verified API values; temporary empty keys must not leak.
            File.WriteAllText(SettingsPath, saved.settingsText);
            File.WriteAllText(BuildSettingsPath, saved.buildSettingsText);
            Require(File.ReadAllText(SettingsPath) == saved.settingsText && File.ReadAllText(BuildSettingsPath) == saved.buildSettingsText, "Settings bytes");
            Require(!qa || temporaryScene == null || (!File.Exists(temporaryScene) && !File.Exists(temporaryScene + ".meta")), "Temporary scene remains");
            saved.restored = true;
            WriteSettings();
            CheckPrivateCaches();
            WriteSettings();
        }
        private static void WriteReport(BuildReport report)
        {
            var summary = report.summary;
            File.WriteAllText(runDirectory + "/build-report.json", JsonUtility.ToJson(new BuildResultRecord {
                stage = Stage, result = summary.result.ToString(), output = summary.outputPath, target = summary.platform.ToString(),
                options = summary.options.ToString(), errors = summary.totalErrors, warnings = summary.totalWarnings,
                startedUtc = summary.buildStartedAt.ToUniversalTime().ToString("O"), endedUtc = summary.buildEndedAt.ToUniversalTime().ToString("O"),
                bytes = File.Exists(apk) ? new FileInfo(apk).Length : 0, sha256 = File.Exists(apk) ? Sha(apk) : null,
                package = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android),
                backend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android).ToString(),
                architecture = PlayerSettings.Android.targetArchitectures.ToString(),
                stripping = PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android).ToString(),
                files = report.GetFiles().Select(x => x.role + "|" + x.path + "|" + x.size).ToArray() }, true));
        }
        [Serializable] private sealed class ResourceResult
        {
            public string stage, scene, font, fontSourceSha256, sceneSha256, androidIdentifier;
            public long fontBytes;
            public bool sceneBindingsReopened;
        }
        [Serializable] private sealed class FileIdentity { public string sha256; }
        [Serializable] private sealed class RunBinding
        {
            public string stageId, authorTurnId, mode;
            public string[] argv;
            public FileIdentity script, rootIdentity;
        }
        [Serializable] private sealed class BuildResultRecord
        {
            public string stage, result, output, target, options, startedUtc, endedUtc, sha256, package, backend, architecture, stripping;
            public int errors, warnings;
            public long bytes;
            public string[] files;
        }
#pragma warning disable 618
        [Serializable] private sealed class Settings
        {
            public string identifier, product, aot, keystore, alias, settingsText, buildSettingsText;
            public string legacySocket = "Not accessed: installed UTF uses its Unity 2021.2+ non-legacy Android path.";
            public bool strip, background, resizable, splash, wait, nullChecks, bundle, expansion, customKey, autoGraphics;
            public int backend, architecture, fullScreen, resolution, lightmapping;
            public string[] graphics, sceneSetup, buildScenes;
            public string[] privateCacheMetadata = new string[0];
            public bool[] enabledScenes;
            public string temporaryScene, temporarySceneSha256, temporaryMetaSha256, settingsBeforeExactRestoreSha256, buildSettingsBeforeExactRestoreSha256, failure;
            public long temporarySceneBytes, temporaryMetaBytes;
            public bool naturalCleanup, restored;
            [NonSerialized] private string keyPassword, aliasPassword;
            internal static Settings Capture() => new Settings {
                identifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android), product = PlayerSettings.productName,
                aot = PlayerSettings.aotOptions, strip = PlayerSettings.stripEngineCode, background = PlayerSettings.runInBackground,
                fullScreen = (int)PlayerSettings.fullScreenMode, resolution = (int)PlayerSettings.displayResolutionDialog,
                resizable = PlayerSettings.resizableWindow, splash = PlayerSettings.SplashScreen.show,
                wait = EditorUserBuildSettings.waitForPlayerConnection, nullChecks = EditorUserBuildSettings.explicitNullChecks,
                lightmapping = (int)Lightmapping.giWorkflowMode,
                backend = (int)PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android), architecture = (int)PlayerSettings.Android.targetArchitectures,
                bundle = EditorUserBuildSettings.buildAppBundle, expansion = PlayerSettings.Android.useAPKExpansionFiles,
                customKey = PlayerSettings.Android.useCustomKeystore, keystore = PlayerSettings.Android.keystoreName,
                alias = PlayerSettings.Android.keyaliasName, keyPassword = PlayerSettings.Android.keystorePass, aliasPassword = PlayerSettings.Android.keyaliasPass,
                autoGraphics = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android),
                graphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).Select(x => x.ToString()).ToArray(),
                sceneSetup = SceneState(), buildScenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray(),
                enabledScenes = EditorBuildSettings.scenes.Select(x => x.enabled).ToArray(),
                settingsText = File.ReadAllText(SettingsPath), buildSettingsText = File.ReadAllText(BuildSettingsPath) };
            internal bool ContextMatches() => product == PlayerSettings.productName && aot == PlayerSettings.aotOptions &&
                background == PlayerSettings.runInBackground && fullScreen == (int)PlayerSettings.fullScreenMode &&
                resolution == (int)PlayerSettings.displayResolutionDialog && resizable == PlayerSettings.resizableWindow &&
                splash == PlayerSettings.SplashScreen.show && nullChecks == EditorUserBuildSettings.explicitNullChecks &&
                lightmapping == (int)Lightmapping.giWorkflowMode && buildScenes.SequenceEqual(EditorBuildSettings.scenes.Select(x => x.path)) &&
                enabledScenes.SequenceEqual(EditorBuildSettings.scenes.Select(x => x.enabled));
            internal bool PlatformMatches() => identifier == PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) &&
                strip == PlayerSettings.stripEngineCode && wait == EditorUserBuildSettings.waitForPlayerConnection &&
                backend == (int)PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) &&
                architecture == (int)PlayerSettings.Android.targetArchitectures && bundle == EditorUserBuildSettings.buildAppBundle &&
                expansion == PlayerSettings.Android.useAPKExpansionFiles && autoGraphics == PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android) &&
                graphics.SequenceEqual(PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).Select(x => x.ToString())) &&
                customKey == PlayerSettings.Android.useCustomKeystore && keystore == PlayerSettings.Android.keystoreName && alias == PlayerSettings.Android.keyaliasName;
            internal void Apply()
            {
                if (identifier != PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)) PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, identifier);
                if (strip != PlayerSettings.stripEngineCode) PlayerSettings.stripEngineCode = strip;
                if (product != PlayerSettings.productName) PlayerSettings.productName = product;
                if (aot != PlayerSettings.aotOptions) PlayerSettings.aotOptions = aot;
                if (background != PlayerSettings.runInBackground) PlayerSettings.runInBackground = background;
                if (fullScreen != (int)PlayerSettings.fullScreenMode) PlayerSettings.fullScreenMode = (FullScreenMode)fullScreen;
                if (resolution != (int)PlayerSettings.displayResolutionDialog) PlayerSettings.displayResolutionDialog = (ResolutionDialogSetting)resolution;
                if (resizable != PlayerSettings.resizableWindow) PlayerSettings.resizableWindow = resizable;
                if (splash != PlayerSettings.SplashScreen.show) PlayerSettings.SplashScreen.show = splash;
                if (wait != EditorUserBuildSettings.waitForPlayerConnection) EditorUserBuildSettings.waitForPlayerConnection = wait;
                if (nullChecks != EditorUserBuildSettings.explicitNullChecks) EditorUserBuildSettings.explicitNullChecks = nullChecks;
                if (lightmapping != (int)Lightmapping.giWorkflowMode) Lightmapping.giWorkflowMode = (Lightmapping.GIWorkflowMode)lightmapping;
                if (backend != (int)PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android)) PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, (ScriptingImplementation)backend);
                if (architecture != (int)PlayerSettings.Android.targetArchitectures) PlayerSettings.Android.targetArchitectures = (AndroidArchitecture)architecture;
                if (bundle != EditorUserBuildSettings.buildAppBundle) EditorUserBuildSettings.buildAppBundle = bundle;
                if (expansion != PlayerSettings.Android.useAPKExpansionFiles) PlayerSettings.Android.useAPKExpansionFiles = expansion;
                if (autoGraphics != PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android)) PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, autoGraphics);
                if (customKey != PlayerSettings.Android.useCustomKeystore) PlayerSettings.Android.useCustomKeystore = customKey;
                if (keystore != PlayerSettings.Android.keystoreName) PlayerSettings.Android.keystoreName = keystore;
                if (alias != PlayerSettings.Android.keyaliasName) PlayerSettings.Android.keyaliasName = alias;
                PlayerSettings.Android.keystorePass = keyPassword;
                PlayerSettings.Android.keyaliasPass = aliasPassword;
                if (!buildScenes.SequenceEqual(EditorBuildSettings.scenes.Select(x => x.path)) ||
                    !enabledScenes.SequenceEqual(EditorBuildSettings.scenes.Select(x => x.enabled)))
                    EditorBuildSettings.scenes = buildScenes.Select((path, i) => new EditorBuildSettingsScene(path, enabledScenes[i])).ToArray();
            }
        }
#pragma warning restore 618
    }
}
