using System.Collections.Generic;
using FlowPuzzle.Core;
using FlowPuzzle.Generation;
using UnityEngine.UIElements;

namespace FlowPuzzle.Editor.UI
{
    public sealed class FlowBatchReportPanel
    {
        public ListView listView;
        private List<FlowBatchItemResult> items;

        public void Build(VisualElement root)
        {
            listView = new ListView { virtualizationMethod = CollectionVirtualizationMethod.FixedHeight };
            root.Add(listView);
        }

        public void Show(FlowBatchReport report)
        {
            items = new List<FlowBatchItemResult>(report.items);
            listView.makeItem = () => new Label();
            listView.bindItem = (e, i) =>
            {
                var item = items[i];
                var status = item.success ? "OK" : "FAIL";
                ((Label)e).text = $"[{status}] Level {item.levelId}, seed {item.usedSeed}: {item.message}";
            };
            listView.itemsSource = items;
            listView.RefreshItems();
        }

        public void Clear()
        {
            listView.itemsSource = null;
            listView.RefreshItems();
        }
    }
}
