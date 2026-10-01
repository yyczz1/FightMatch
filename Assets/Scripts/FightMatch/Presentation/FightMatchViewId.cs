using System;
using System.Collections.Generic;
using UnityEngine;

namespace FightMatch.Presentation
{
    [DisallowMultipleComponent]
    public sealed class FightMatchViewId : MonoBehaviour
    {
        [SerializeField] private string id;
        public string Id => id;
        private static readonly HashSet<string> Fixed = new HashSet<string>(StringComparer.Ordinal)
        {
            "fm.page.startup", "fm.page.navigation", "fm.page.battle", "fm.page.result",
            "fm.section.map", "fm.section.party", "fm.section.inventory", "fm.section.preparation",
            "fm.popup.language", "fm.popup.reference", "fm.popup.confirmation", "fm.popup.recovery",
            "fm.popup.license", "fm.popup.quit", "fm.popup.blocking", "fm.board.candidate",
            "fm.action.profile.create", "fm.action.profile.continue", "fm.action.language.open",
            "fm.action.map.level01", "fm.action.party.confirm", "fm.action.entry.enter",
            "fm.action.battle.retry", "fm.action.battle.resolve", "fm.action.battle.skip",
            "fm.action.history.open", "fm.action.reference.open", "fm.action.victory.settle",
            "fm.action.result.map", "fm.action.result.party", "fm.action.result.inventory",
            "fm.action.result.replay", "fm.action.back", "fm.action.close", "fm.action.quit.confirm"
        };

        internal void Assign(string value)
        {
            if (!Known(value)) throw new ArgumentException("Unknown FightMatch view ID.", nameof(value));
            id = value;
        }
        internal static string Member(string characterId) => Dynamic("fm.member.", characterId);
        internal static string Row(string businessId) => Dynamic("fm.row.", businessId);
        private static string Dynamic(string prefix, string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("A stable business ID is required.", nameof(value));
            return prefix + Uri.EscapeDataString(value);
        }
        private static bool Known(string value)
        {
            return value != null && (Fixed.Contains(value) ||
                value.StartsWith("fm.member.", StringComparison.Ordinal) && value.Length > 10 ||
                value.StartsWith("fm.row.", StringComparison.Ordinal) && value.Length > 7);
        }
        internal static bool Validate(Transform root, out string diagnostic)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var binding in root.GetComponentsInChildren<FightMatchViewId>(true))
            {
                if (!Known(binding.id)) { diagnostic = "UnknownViewId"; return false; }
                if (!ids.Add(binding.id)) { diagnostic = "DuplicateViewId"; return false; }
            }
            diagnostic = null; return true;
        }
        internal static T Find<T>(Transform root, string value) where T : Component
        {
            foreach (var binding in root.GetComponentsInChildren<FightMatchViewId>(true))
                if (binding.id == value) return binding.GetComponent<T>();
            return null;
        }
    }
}
