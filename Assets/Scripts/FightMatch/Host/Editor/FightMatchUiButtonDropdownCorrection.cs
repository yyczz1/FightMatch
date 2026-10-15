using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace FightMatch.Host.Editor
{
    public static class FightMatchUiButtonDropdownCorrection
    {
        private const string PrefabPath = "Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab";
        private const string Placeholder = "【if you see this, it is a bug.】";

        [MenuItem("FightMatch/UI/Apply Button and Language Dropdown Correction")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Apply the correction outside Play mode.");
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var safe = root.transform.Find("RuntimeCanvas/SafeAreaRoot");
                var top = safe.Find("HudLayer/TopBar");
                var host = root.GetComponent<FightMatchHostView>();
                var fields = new SerializedObject(host);
                var dropdown = top.Find("Language").GetComponent<FightMatchLocaleDropdown>();
                if (dropdown == null)
                {
                    var old = top.Find("Language");
                    var font = old.GetComponentInChildren<TextMeshProUGUI>(true).font;
                    var index = old.GetSiblingIndex();
                    UnityEngine.Object.DestroyImmediate(old.gameObject);
                    dropdown = CreateLanguageDropdown(top, font);
                    dropdown.transform.SetSiblingIndex(index);
                }
                var feedbackPanel = safe.Find("HudLayer/LanguageFeedback");
                if (feedbackPanel == null)
                {
                    var feedback = safe.Find("PopupLayer/LanguagePopup/RootMask/DialogPanel/BodyViewport/Viewport/Content/Feedback");
                    feedbackPanel = CreateFeedbackPanel(safe.Find("HudLayer"), feedback);
                }
                WireLanguage(fields, dropdown, feedbackPanel);
                fields.ApplyModifiedPropertiesWithoutUndo();
                var layout = safe.GetComponents<Component>().Single(x => x != null && x.GetType().Name == "FightMatchResponsiveLayout");
                var layoutFields = new SerializedObject(layout);
                layoutFields.FindProperty("languageFeedbackPanel").objectReferenceValue = feedbackPanel;
                var panels = layoutFields.FindProperty("dialogPanels");
                var oldPopup = safe.Find("PopupLayer/LanguagePopup");
                if (oldPopup != null)
                {
                    var kept = Enumerable.Range(0, panels.arraySize).Select(i => panels.GetArrayElementAtIndex(i).objectReferenceValue)
                        .Where(x => x != null && !((Component)x).transform.IsChildOf(oldPopup)).ToArray();
                    panels.arraySize = kept.Length;
                    for (var i = 0; i < kept.Length; i++) panels.GetArrayElementAtIndex(i).objectReferenceValue = kept[i];
                    UnityEngine.Object.DestroyImmediate(oldPopup.gameObject);
                }
                layoutFields.ApplyModifiedPropertiesWithoutUndo();
                foreach (var button in root.GetComponentsInChildren<UnityEngine.UI.Button>(true)) ConfigureButton(button);
                ConfigureTopBar(top);
                if (PrefabUtility.SaveAsPrefabAsset(root, PrefabPath) == null)
                    throw new InvalidOperationException("Could not save the corrected runtime prefab.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        internal static void ConfigureButton(UnityEngine.UI.Button button)
        {
            var rect = (RectTransform)button.transform;
            var member = button.name == "MemberButtonTemplate";
            var compact = member || button.transform.parent.name == "TopBar" ||
                button.transform.parent.name == "Header" || button.transform.parent.name == "FixedActions";
            var fit = button.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (fit != null) UnityEngine.Object.DestroyImmediate(fit);
            var horizontal = button.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            if (horizontal != null) UnityEngine.Object.DestroyImmediate(horizontal);
            var group = Get<UnityEngine.UI.VerticalLayoutGroup>(button.gameObject);
            group.padding = member ? new RectOffset(2, 2, 2, 2) : new RectOffset(8, 8, 2, 2);
            group.spacing = 4; group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandWidth = true; group.childForceExpandHeight = compact;
            var cell = Get<UnityEngine.UI.LayoutElement>(button.gameObject);
            cell.minHeight = member ? 72 : 48; cell.preferredHeight = compact ? cell.minHeight : -1;
            cell.flexibleHeight = 0;
            foreach (var text in button.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                ConfigureText(text, compact);
                var textFit = text.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                if (textFit != null) UnityEngine.Object.DestroyImmediate(textFit);
                var textCell = Get<UnityEngine.UI.LayoutElement>(text.gameObject);
                textCell.minHeight = textCell.preferredHeight = -1; textCell.flexibleHeight = 0;
                textCell.minWidth = textCell.preferredWidth = -1; textCell.flexibleWidth = 1;
                text.rectTransform.localScale = Vector3.one;
                text.alignment = text.name == "DisabledReason" ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Center;
            }
            rect.localScale = Vector3.one;
        }

        private static void ConfigureText(TextMeshProUGUI text, bool compact)
        {
            text.text = Placeholder; text.fontSize = 18; text.fontSizeMin = 16; text.fontSizeMax = 18;
            text.enableAutoSizing = compact; text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow; text.maxVisibleLines = int.MaxValue;
            text.characterSpacing = text.wordSpacing = text.lineSpacing = text.paragraphSpacing = 0;
            text.characterWidthAdjustment = 0; text.lineSpacingAdjustment = 0;
            text.margin = Vector4.zero; text.raycastTarget = false;
        }

        private static void ConfigureTopBar(Transform top)
        {
            var group = top.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            group.spacing = 8; group.padding = new RectOffset(0, 0, 4, 4);
            group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandWidth = group.childForceExpandHeight = false;
            foreach (var item in new[] { ("Back", 64f), ("Title", 136f), ("Language", 104f), ("License", 180f) })
            {
                var cell = Get<UnityEngine.UI.LayoutElement>(top.Find(item.Item1).gameObject);
                cell.minWidth = cell.preferredWidth = item.Item2; cell.flexibleWidth = 0;
                cell.minHeight = cell.preferredHeight = 48; cell.flexibleHeight = 0;
            }
            ConfigureText(top.Find("Title").GetComponent<TextMeshProUGUI>(), true);
        }

        internal static FightMatchLocaleDropdown CreateLanguageDropdown(Transform parent, TMP_FontAsset font)
        {
            var rect = Rect("Language", parent); var image = Surface(rect);
            var dropdown = rect.gameObject.AddComponent<FightMatchLocaleDropdown>();
            dropdown.targetGraphic = image; dropdown.alphaFadeSpeed = 0;
            var cell = rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            cell.minWidth = cell.preferredWidth = 104; cell.minHeight = cell.preferredHeight = 48;
            var caption = Label("Caption", rect, font); Inset(caption.rectTransform, 8, 20);
            var placeholder = Label("Placeholder", rect, font); Inset(placeholder.rectTransform, 8, 20);
            dropdown.captionText = caption; dropdown.placeholder = placeholder;
            var arrow = Rect("Arrow", rect); arrow.anchorMin = arrow.anchorMax = new Vector2(1, .5f);
            arrow.anchoredPosition = new Vector2(-10, 0); arrow.sizeDelta = new Vector2(8, 8);
            arrow.localRotation = Quaternion.Euler(0, 0, 45);
            var arrowImage = Surface(arrow); arrowImage.color = Color.white; arrowImage.raycastTarget = false;
            var options = Rect("OptionLabels", rect);
            Label("English", options, font); Label("Chinese", options, font); options.gameObject.SetActive(false);
            var template = Rect("Template", rect); template.anchorMin = template.anchorMax = new Vector2(1, 0);
            template.pivot = new Vector2(1, 1); template.sizeDelta = new Vector2(180, 96); Surface(template).raycastTarget = false;
            var viewport = Rect("Viewport", template); Fill(viewport); Surface(viewport);
            viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
            var content = Rect("Content", viewport); content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1); content.sizeDelta = new Vector2(0, 48);
            var item = Rect("Item", content); item.anchorMin = new Vector2(0, 1); item.anchorMax = Vector2.one;
            item.pivot = new Vector2(.5f, 1); item.sizeDelta = new Vector2(0, 48);
            var toggle = item.gameObject.AddComponent<UnityEngine.UI.Toggle>(); toggle.targetGraphic = Surface(item);
            var check = Rect("Check", item); check.anchorMin = check.anchorMax = new Vector2(0, .5f);
            check.anchoredPosition = new Vector2(12, 0); check.sizeDelta = new Vector2(8, 8);
            toggle.graphic = Surface(check); toggle.graphic.color = Color.white; toggle.graphic.raycastTarget = false;
            var itemText = Label("ItemLabel", item, font); Inset(itemText.rectTransform, 24, 8);
            var scroll = template.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
            scroll.inertia = false; scroll.elasticity = 0;
            scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            dropdown.template = template; dropdown.itemText = itemText; template.gameObject.SetActive(false);
            dropdown.options = new System.Collections.Generic.List<TMP_Dropdown.OptionData> {
                new TMP_Dropdown.OptionData(Placeholder), new TMP_Dropdown.OptionData(Placeholder) };
            // Serialize the diagnostic caption unchanged; Bind selects the native -1 placeholder state.
            var serialized = new SerializedObject(dropdown); serialized.FindProperty("m_Value").intValue = -1;
            serialized.ApplyModifiedPropertiesWithoutUndo(); caption.text = placeholder.text = Placeholder;
            var id = Script(rect.gameObject, "FightMatchViewId");
            var idFields = new SerializedObject(id); idFields.FindProperty("id").stringValue = "fm.action.language.open";
            idFields.ApplyModifiedPropertiesWithoutUndo();
            return dropdown;
        }

        internal static Transform CreateFeedbackPanel(Transform parent, Transform feedback)
        {
            var panel = Rect("LanguageFeedback", parent); panel.pivot = new Vector2(0, 1); panel.sizeDelta = new Vector2(492, 48);
            Surface(panel).raycastTarget = false;
            var group = panel.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            group.padding = new RectOffset(10, 10, 8, 8); group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandWidth = true; group.childForceExpandHeight = false;
            panel.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            feedback.SetParent(panel, false); feedback.name = "Feedback";
            ConfigureText(feedback.GetComponent<TextMeshProUGUI>(), false);
            var cell = Get<UnityEngine.UI.LayoutElement>(feedback.gameObject);
            cell.minHeight = cell.preferredHeight = -1; cell.flexibleHeight = 0;
            feedback.gameObject.SetActive(false); panel.gameObject.SetActive(false);
            return panel;
        }

        internal static void WireLanguage(SerializedObject fields, FightMatchLocaleDropdown dropdown, Transform panel)
        {
            fields.FindProperty("languageDropdown").objectReferenceValue = dropdown;
            fields.FindProperty("languageCaption").objectReferenceValue = Binding(dropdown.transform.Find("Placeholder"));
            fields.FindProperty("languageEnglishOption").objectReferenceValue = Binding(dropdown.transform.Find("OptionLabels/English"));
            fields.FindProperty("languageChineseOption").objectReferenceValue = Binding(dropdown.transform.Find("OptionLabels/Chinese"));
            fields.FindProperty("languageFeedback").objectReferenceValue = Binding(panel.Find("Feedback"));
            fields.FindProperty("languageFeedbackPanel").objectReferenceValue = panel.gameObject;
        }
        private static Component Binding(Transform transform) => transform.GetComponents<Component>().Single(x => x != null && x.GetType().Name == "LocalizedTmpText");
        private static Component Script(GameObject target, string name)
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/FightMatch/Presentation/" + name + ".cs");
            if (script == null || script.GetClass() == null) throw new InvalidOperationException("Missing script: " + name);
            return target.AddComponent(script.GetClass());
        }
        private static TextMeshProUGUI Label(string name, Transform parent, TMP_FontAsset font)
        {
            var text = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font; text.fontSharedMaterial = font.material; text.color = Color.white; text.richText = false;
            ConfigureText(text, true); text.alignment = TextAlignmentOptions.Center;
            var binding = Script(text.gameObject, "Localization/LocalizedTmpText");
            var fields = new SerializedObject(binding); fields.FindProperty("target").objectReferenceValue = text;
            fields.ApplyModifiedPropertiesWithoutUndo(); return text;
        }
        private static T Get<T>(GameObject go) where T : Component => go.GetComponent<T>() ?? go.AddComponent<T>();
        private static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); return rect;
        }
        private static UnityEngine.UI.Image Surface(RectTransform rect)
        {
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = new Color(.20f, .24f, .29f, 1);
            image.raycastTarget = true; return image;
        }
        private static void Fill(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static void Inset(RectTransform rect, float left, float right)
        { Fill(rect); rect.offsetMin = new Vector2(left, 2); rect.offsetMax = new Vector2(-right, -2); }
    }
}
