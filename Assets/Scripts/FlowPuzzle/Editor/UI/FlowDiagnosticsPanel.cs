using System;
using System.Text;
using FlowPuzzle.Core;
using UnityEngine.UIElements;

namespace FlowPuzzle.Editor.UI
{
    public sealed class FlowDiagnosticsPanel
    {
        public HelpBox helpBox;
        private VisualElement actionBar;
        public Button retrySameSeedButton;
        public Button retryNewSeedButton;
        private VisualElement suggestionContainer;

        /// <summary>
        /// Non-null while a diagnostic is displayed. The window reads this to know
        /// whether retry is active and what suggestion to apply.
        /// </summary>
        public FlowFailureDiagnostic currentDiagnostic;

        public void Build(VisualElement root)
        {
            helpBox = new HelpBox("", HelpBoxMessageType.Info) { visible = false };
            root.Add(helpBox);

            actionBar = new VisualElement();
            actionBar.style.flexDirection = FlexDirection.Row;
            actionBar.style.marginTop = 4;
            actionBar.style.display = DisplayStyle.None;

            retrySameSeedButton = new Button { text = "Retry Same Seed" };
            retrySameSeedButton.style.marginRight = 4;
            actionBar.Add(retrySameSeedButton);

            retryNewSeedButton = new Button { text = "Retry New Seed" };
            actionBar.Add(retryNewSeedButton);

            root.Add(actionBar);

            suggestionContainer = new VisualElement();
            suggestionContainer.style.marginTop = 4;
            suggestionContainer.style.marginLeft = 4;
            root.Add(suggestionContainer);
        }

        public void ShowDiagnostic(FlowFailureDiagnostic diagnostic)
        {
            currentDiagnostic = diagnostic;

            if (diagnostic == null)
            {
                Clear();
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[{diagnostic.errorCode}] {diagnostic.errorMessage}");
            if (diagnostic.attemptCount > 0)
                sb.AppendLine($"Attempts: {diagnostic.attemptCount}  Seed: {diagnostic.usedSeed}");

            helpBox.messageType = HelpBoxMessageType.Error;
            helpBox.text = sb.ToString().TrimEnd();
            helpBox.visible = true;

            actionBar.style.display = DisplayStyle.Flex;
            actionBar.visible = true;

            // Render suggestions with optional Apply buttons
            suggestionContainer.Clear();
            if (diagnostic.suggestions != null && diagnostic.suggestions.Count > 0)
            {
                foreach (var s in diagnostic.suggestions)
                {
                    var row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Row;
                    row.style.marginBottom = 2;

                    var label = new Label($"  -> {s}")
                    {
                        style =
                        {
                            whiteSpace = WhiteSpace.Normal,
                            fontSize = 11,
                            flexGrow = 1
                        }
                    };
                    row.Add(label);

                    // Only show Apply Suggestion for safe scalar suggestions
                    // (Direction is a concrete value change like Increase/Decrease/Positive)
                    if (IsSafeScalarSuggestion(s))
                    {
                        var applyBtn = new Button { text = "Apply" };
                        applyBtn.style.width = 50;
                        applyBtn.style.height = 18;
                        applyBtn.style.fontSize = 10;
                        var captured = s;
                        applyBtn.clicked += () => OnApplySuggestionClicked?.Invoke(captured);
                        row.Add(applyBtn);
                    }

                    suggestionContainer.Add(row);
                }
            }
        }

        /// <summary>
        /// A suggestion is "safe scalar" when it proposes a concrete, single-parameter,
        /// single-direction adjustment that can be safely applied without ambiguity.
        /// </summary>
        private static bool IsSafeScalarSuggestion(FlowParameterSuggestion s)
        {
            if (string.IsNullOrEmpty(s.suggestedDirection)) return false;
            if (string.IsNullOrEmpty(s.suggestedValue)) return false;
            if (s.parameterName == "constraint") return false; // Not scalar — needs manual fix
            if (s.parameterName == "assetName") return false; // Not scalar
            if (s.parameterName == "outputFolder") return false; // Not scalar
            return true;
        }

        public event Action<FlowParameterSuggestion> OnApplySuggestionClicked;

        public void ShowError(string message)
        {
            currentDiagnostic = null;
            helpBox.messageType = HelpBoxMessageType.Error;
            helpBox.text = message;
            helpBox.visible = true;
            actionBar.style.display = DisplayStyle.None;
            actionBar.visible = false;
            suggestionContainer?.Clear();
        }

        public void ShowInfo(string message)
        {
            currentDiagnostic = null;
            helpBox.messageType = HelpBoxMessageType.Info;
            helpBox.text = message;
            helpBox.visible = true;
            actionBar.style.display = DisplayStyle.None;
            actionBar.visible = false;
            suggestionContainer?.Clear();
        }

        public void Clear()
        {
            currentDiagnostic = null;
            helpBox.text = "";
            helpBox.visible = false;
            actionBar.style.display = DisplayStyle.None;
            actionBar.visible = false;
            suggestionContainer?.Clear();
        }
    }
}
