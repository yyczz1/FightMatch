using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using UnityEngine;
using UnityEngine.Events;
using NavigationView = FightMatch.Application.PlayerNavigationView;

namespace FightMatch.Presentation
{
    public sealed class PlayerNavigationView : MonoBehaviour, IDisposable
    {
        [SerializeField] private RectTransform mapSection;
        [SerializeField] private RectTransform partySection;
        [SerializeField] private RectTransform inventorySection;
        [SerializeField] private RectTransform preparationSection;
        [SerializeField] private RectTransform craftSection;
        [SerializeField] private LocalizedTmpText title;
        [SerializeField] private LocalizedTmpText status;
        [SerializeField] private LocalizedTmpText recipeAvailability;
        [SerializeField] private UnityEngine.UI.Button mapButton;
        [SerializeField] private UnityEngine.UI.Button partyButton;
        [SerializeField] private UnityEngine.UI.Button inventoryButton;
        [SerializeField] private UnityEngine.UI.Button backButton;
        [SerializeField] private UnityEngine.UI.Button cancelButton;
        [SerializeField] private UnityEngine.UI.Button refreshButton;
        [SerializeField] private UnityEngine.UI.Button migrationButton;
        [SerializeField] private UnityEngine.UI.Button creationButton;
        [SerializeField] private UnityEngine.UI.Button settlementButton;
        [SerializeField] private UnityEngine.UI.Button entryButton;
        [SerializeField] private UnityEngine.UI.Button resumeButton;
        [SerializeField] private UnityEngine.UI.Button partyPreviewButton;
        [SerializeField] private UnityEngine.UI.Button craftButton;
        [SerializeField] private UnityEngine.UI.Button clearEquipmentButton;
        [SerializeField] private UnityEngine.UI.Button preferenceButton;
        [SerializeField] private UnityEngine.UI.Button originalResultButton;
        [SerializeField] private TMPro.TMP_Dropdown[] partySlots;
        [SerializeField] private PlayerPermanentDetailView detail;
        [SerializeField] private PlayerNavigationRecoveryView recovery;
        [SerializeField] private UnityEngine.UI.Button buttonTemplate;
        [SerializeField] private LocalizedTmpText textTemplate;
        private readonly NavigationBindings bindings = new NavigationBindings();
        private PlayerNavigationController controller;
        private LocalizationService localization;
        private int generation;
        public PlayerNavigationController Controller => controller;

        internal void Bind(PlayerNavigationController navigation, LocalizationService service)
        {
            Unbind();
            controller = navigation ?? throw new ArgumentNullException(nameof(navigation));
            localization = service ?? throw new ArgumentNullException(nameof(service));
            generation++;
            detail.Bind(controller, localization);
            recovery.Bind(controller, localization);
            controller.Changed += Render;
            Render();
        }
        internal static string Key(params string[] parts) => string.Join("|", parts.Select(x => Uri.EscapeDataString(x ?? "")));
        private void Target(UnityEngine.UI.Button button, string key, PlayerNavigationTarget target, string reason = null,
            string reasonKey = null, Func<KeyValuePair<string, string>[]> reasonArgs = null)
        { bindings.Button(button, localization, key, controller.NavigationHandler(target), reason, reasonKey, reasonArgs: reasonArgs); }
        private void Page(UnityEngine.UI.Button button, PlayerNavigationTargetKind kind, string key)
        { Target(button, key, new PlayerNavigationTarget { Kind = kind }); }
        private void Row(Transform parent, string identity, string key, params KeyValuePair<string, string>[] args)
        { bindings.Row(textTemplate, parent, FightMatchViewId.Row(identity), localization, key, args); }
        private void DynamicRow(Transform parent, string identity, string key, Func<KeyValuePair<string, string>[]> arguments)
        { bindings.DynamicRow(textTemplate, parent, FightMatchViewId.Row(identity), localization, key, arguments); }
        private void Render()
        {
            if (controller == null) return;
            Render(controller.View);
        }
        internal void Render(NavigationView view)
        {
            if (controller == null) return;
            bindings.Dispose();
            foreach (var section in new[] { mapSection, partySection, inventorySection, preparationSection, craftSection })
                section.gameObject.SetActive(false);
            foreach (var button in new[] { mapButton, partyButton, inventoryButton, migrationButton, creationButton,
                settlementButton, entryButton, resumeButton, partyPreviewButton, craftButton, clearEquipmentButton,
                preferenceButton, originalResultButton, backButton, cancelButton, refreshButton }) button.gameObject.SetActive(false);
            if (view.Read?.Application.Phase == CandidateApplicationPhase.Disposed)
            {
                title.gameObject.SetActive(false); status.gameObject.SetActive(false);
                detail.Render(view); recovery.Render(view); return;
            }
            var routeKey = view.Route == PlayerNavigationRoute.MapAdventure ? "fm.map.title" :
                view.Route == PlayerNavigationRoute.Preparation ? "fm.entry.title" :
                view.Route == PlayerNavigationRoute.Team ? "fm.party.title" :
                view.Route == PlayerNavigationRoute.Bag ? "fm.inventory.title" :
                view.Route == PlayerNavigationRoute.CraftList ? "fm.crafting.title" : null;
            // Detail, recovery and startup each own their title and state surface.
            title.gameObject.SetActive(routeKey != null);
            if (routeKey != null) bindings.Text(title, localization, routeKey);
            status.gameObject.SetActive(false);
            if (!NavigationBindings.HasRecoverySurface(view) && !NavigationBindings.IsStartup(view.Read?.Application.Phase))
                bindings.States(status, textTemplate, transform, localization, view, "navigation");
            detail.Render(view);
            recovery.Render(view);
            if (view.Read?.IsAvailable == true)
            {
                Page(mapButton, PlayerNavigationTargetKind.MapAdventure, "fm.common.nav.adventure");
                Page(partyButton, PlayerNavigationTargetKind.Team, "fm.common.nav.party");
                Page(inventoryButton, PlayerNavigationTargetKind.Bag, "fm.common.nav.inventory");
                if (view.Route == PlayerNavigationRoute.MapAdventure) { Map(view); mapSection.gameObject.SetActive(true); }
                if (view.Route == PlayerNavigationRoute.Preparation) { Preparation(view); preparationSection.gameObject.SetActive(true); }
                if (view.Route == PlayerNavigationRoute.Team) { Team(view); partySection.gameObject.SetActive(true); }
                if (view.Route == PlayerNavigationRoute.Bag) { Bag(view); inventorySection.gameObject.SetActive(true); }
                if (view.Route == PlayerNavigationRoute.CraftList) { Recipes(view); craftSection.gameObject.SetActive(true); }
                var format = (uint)view.Read.Head.Business.Format;
                if ((format == 2 || format == 3) && !NavigationBindings.HasRecoverySurface(view))
                {
                    bindings.Button(migrationButton, localization, "fm.profile.data_upgrade.title", controller.PreviewHandler(() => new PlayerNavigationDraft {
                        Kind = PlayerNavigationDraftKind.Migration, FromFormat = format, ToFormat = format + 1 }));
                }
            }
            if (view.Route == PlayerNavigationRoute.Gate)
            {
                if (view.Status == "CreationPending" || view.Read?.Application.Phase == CandidateApplicationPhase.Unconfigured)
                    Page(creationButton, PlayerNavigationTargetKind.CreationRequired, view.Status == "CreationPending" ?
                        "fm.profile.creation_pending.continue_button" : "fm.profile.create.button");
                if (view.Read?.Head?.Continuation != null)
                    Page(settlementButton, PlayerNavigationTargetKind.SettlementRequired, "fm.victory.settle.button");
            }
            bindings.Button(backButton, localization, "fm.common.action.back", controller.ActionHandler(PlayerNavigationAction.Back), view.ReasonFor(PlayerNavigationAction.Back));
            bindings.Button(cancelButton, localization, "fm.common.action.cancel", controller.ActionHandler(PlayerNavigationAction.Cancel), view.ReasonFor(PlayerNavigationAction.Cancel));
            var currentGeneration = generation;
            bindings.Button(refreshButton, localization, "fm.common.action.refresh", () => { if (generation == currentGeneration) controller?.Refresh(); });
        }
        private void Map(NavigationView view)
        {
            foreach (var level in view.Read.Levels)
            {
                var identity = Key(view.Read.Binding.PackageId, view.Read.Binding.ContentFingerprint, level.LevelId, level.LevelVersion);
                var state = view.Read.Progression.Levels.FirstOrDefault(x => x.Level.LevelId == level.LevelId && x.Level.LevelVersion == level.LevelVersion);
                var id = level.LevelId == "level:ch01-01" ? "fm.action.map.level01" : FightMatchViewId.Row("level." + identity);
                bindings.CloneButton(buttonTemplate, mapSection, id, localization,
                    NavigationBindings.LevelKey(level.LevelId, level.LevelVersion), controller.NavigationHandler(new PlayerNavigationTarget {
                    Kind = PlayerNavigationTargetKind.Preparation, LevelId = level.LevelId, LevelVersion = level.LevelVersion, Binding = view.Read.Binding }));
                // WAITING_FOR_COPY_KEYS: non-L1 locked/prerequisite state; no prerequisite exists for initially-open L1.
                Row(mapSection, "level.state." + identity, state?.OpenFact == null ? "" : "fm.map.level.available");
                if (state?.Level.UnlockAfterLevelId != null) Row(mapSection, "level.requirements." + identity, "");
                // WAITING_FOR_COPY_KEYS: cleared state outside the current completed L1-only segment.
                if (state?.FirstClear != null) Row(mapSection, "level.cleared." + identity,
                    level.LevelId == "level:ch01-01" && level.LevelVersion == "1" && view.Read.Levels.Count == 1 ? "fm.map.no_next_level" : "");
            }
            if (view.Read.Levels.Count == 0) Row(mapSection, "map.NoPublishedDefinition", "fm.map.no_published_levels");
        }
        private void Preparation(NavigationView view)
        {
            DynamicRow(preparationSection, "preparation.Level", "fm.entry.level.label", () => new[] {
                NavigationBindings.Arg("levelName", NavigationBindings.Resolve(localization,
                    NavigationBindings.LevelKey(view.Context.LevelId, view.Context.LevelVersion))) });
            DynamicRow(preparationSection, "preparation.Party", "fm.entry.party.label", () => new[] {
                NavigationBindings.Arg("partySummary", NavigationBindings.Party(localization, view.Read.Roster.Slots)) });
            // Lifecycle.Enter is only a readiness hint; the host still performs the full entry check.
            if (view.Read.Lifecycle.Enter.Reason != null && view.Read.Lifecycle.Enter.Reason != "ActiveAttemptConflict")
                    Row(preparationSection, "preparation.EntryReason", NavigationBindings.ReasonKey(NavigationReasonDomain.Entry, view.Read.Lifecycle.Enter.Reason));
            Characters(view, preparationSection);
            Target(entryButton, "fm.entry.enter.button", new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.BattleSelectionRequested },
                view.Read.Head.Business.ActiveHistory == null ? null : "ActiveAttemptConflict", "fm.entry.active_battle.blocked");
            if (view.Read.Head.Business.ActiveHistory != null)
                Page(resumeButton, PlayerNavigationTargetKind.ResumeBattleRequested, "fm.map.active_battle.continue_button");
        }
        private void Characters(NavigationView view, Transform parent)
        {
            foreach (var character in view.Read.Roster.Characters)
            {
                bindings.CloneButton(buttonTemplate, parent, FightMatchViewId.Member(character.CharacterId), localization, "fm.party.member.level",
                    controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.SelectCharacter, CharacterId = character.CharacterId }),
                    captionArgs: () => new[] { NavigationBindings.Arg("characterName", NavigationBindings.Resolve(localization,
                        NavigationBindings.CharacterKey(character.CharacterId))), NavigationBindings.Arg("level", character.Level) });
                var definition = character.Definition; var offset = character.Level - 1;
                var required = definition.XpBase + offset * (definition.XpLinear + definition.XpQuadratic * offset);
                Row(parent, "character.experience." + character.CharacterId, "fm.party.member.experience",
                    NavigationBindings.Arg("currentXp", character.Experience), NavigationBindings.Arg("requiredXp", required));
                if (character.IsReady && character.ActiveRecovery == null)
                    Row(parent, "character.state." + character.CharacterId, "fm.party.member.ready");
                else Row(parent, "character.state." + character.CharacterId, "fm.party.member.recovering",
                    NavigationBindings.Arg("remainingTime", NavigationBindings.RemainingTime(character.ActiveRecovery)));
            }
            if (view.Context.SelectedCharacterId == null)
                Row(parent, "character.Selected", view.Route == PlayerNavigationRoute.Preparation ?
                    "fm.entry.member.select_prompt" : "fm.inventory.character_required");
            else DynamicRow(parent, "character.Selected", "fm.entry.member.selected", () => new[] {
                NavigationBindings.Arg("characterName", NavigationBindings.Resolve(localization,
                    NavigationBindings.CharacterKey(view.Context.SelectedCharacterId))) });
        }
        private void Team(NavigationView view)
        {
            Characters(view, partySection);
            var choices = new List<string> { null }; choices.AddRange(view.Read.Roster.Characters.Select(x => x.CharacterId));
            if (partySlots == null || partySlots.Length != 3) throw new InvalidOperationException("Navigation requires three serialized formation slots.");
            foreach (var slot in partySlots)
            {
                slot.ClearOptions();
                slot.AddOptions(choices.Select(x => new TMPro.TMP_Dropdown.OptionData(LocalizedTmpText.Placeholder)).ToList());
            }
            for (var i = 0; i < partySlots.Length; i++)
                partySlots[i].SetValueWithoutNotify(Math.Max(0, choices.IndexOf(view.Read.Roster.Slots[i])));
            var currentGeneration = generation;
            Action refreshOptions = () => {
                if (controller == null || generation != currentGeneration) return;
                foreach (var slot in partySlots)
                {
                    for (var i = 0; i < slot.options.Count; i++)
                    {
                        var option = localization.Resolve(choices[i] == null ? "fm.party.slot.empty" : NavigationBindings.CharacterKey(choices[i]), null);
                        slot.options[i].text = option.IsSuccess ? option.Text : LocalizedTmpText.Placeholder;
                    }
                    slot.RefreshShownValue();
                    var caption = slot.captionText.GetComponent<LocalizedTmpText>();
                    bindings.Text(caption, localization, choices[slot.value] == null ? "fm.party.slot.empty" : NavigationBindings.CharacterKey(choices[slot.value]));
                }
            };
            foreach (var slot in partySlots)
            {
                UnityAction<int> changed = _ => refreshOptions();
                slot.onValueChanged.AddListener(changed);
                bindings.Detach(() => slot.onValueChanged.RemoveListener(changed));
            }
            Action<LocaleId> localeChanged = _ => refreshOptions();
            localization.LocaleChanged += localeChanged;
            var currentLocalization = localization;
            bindings.Detach(() => currentLocalization.LocaleChanged -= localeChanged);
            refreshOptions();
            bindings.Button(partyPreviewButton, localization, "fm.common.action.review_changes", controller.PreviewHandler(() => new PlayerNavigationDraft {
                Kind = PlayerNavigationDraftKind.Formation, Slots = partySlots.Select(x => choices[x.value]).ToArray() }), view.Read.Roster.UnavailabilityReason,
                reasonDomain: NavigationReasonDomain.Party);
            Equipment(view, partySection);
        }
        private void Bag(NavigationView view)
        {
            foreach (var item in view.Read.Inventory.Items)
            {
                DynamicRow(inventorySection, "inventory.item." + item.ItemId, "fm.inventory.item.row", () => new[] {
                    NavigationBindings.Arg("itemName", NavigationBindings.Resolve(localization, NavigationBindings.ItemKey(item.ItemId))),
                    NavigationBindings.Arg("amount", item.T) });
                Row(inventorySection, "inventory.held." + item.ItemId, "fm.inventory.quantity.held", NavigationBindings.Arg("amount", item.T));
                Row(inventorySection, "inventory.equipped." + item.ItemId, "fm.inventory.quantity.equipped", NavigationBindings.Arg("amount", item.L));
                Row(inventorySection, "inventory.reserved." + item.ItemId, "fm.inventory.quantity.reserved", NavigationBindings.Arg("amount", item.R));
                Row(inventorySection, "inventory.available." + item.ItemId, "fm.inventory.quantity.available", NavigationBindings.Arg("amount", item.F));
            }
            if (view.Read.Inventory.Items.All(x => x.T.IsZero)) Row(inventorySection, "inventory.Empty", "fm.inventory.empty");
            foreach (var load in view.Read.Inventory.State.Loadouts)
            {
                if (load.ItemId == null) DynamicRow(inventorySection, "loadout." + load.Actor.CharacterId, "fm.inventory.no_equipment", () => new[] {
                    NavigationBindings.Arg("characterName", NavigationBindings.Resolve(localization, NavigationBindings.CharacterKey(load.Actor.CharacterId))) });
                // WAITING_FOR_COPY_KEYS: non-L1 equipped-item loadout and active battle carry summaries.
                else Row(inventorySection, "loadout." + load.Actor.CharacterId, "");
            }
            foreach (var row in view.Read.Inventory.State.ActiveCarry?.Rows ?? Array.Empty<CandidateCarryRow>())
                Row(inventorySection, "carry." + Key(row.Actor.CharacterId, row.ItemId), "");
            Characters(view, inventorySection);
            Page(craftButton, PlayerNavigationTargetKind.CraftList, "fm.crafting.title");
            Equipment(view, inventorySection);
            foreach (var record in view.Read.Head.Records)
            {
                var operationKey = NavigationBindings.OperationKey(record.Intent);
                if (operationKey == null) continue;
                bindings.CloneButton(buttonTemplate, inventorySection, FightMatchViewId.Row("bag.operation." + record.OperationId), localization,
                    operationKey,
                    controller.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.OriginalOperation, OperationId = record.OperationId }));
            }
            bindings.Button(originalResultButton, localization, "fm.save_recovery.lookup_button",
                controller.ActionHandler(PlayerNavigationAction.SelectOriginalOperation), view.ReasonFor(PlayerNavigationAction.SelectOriginalOperation));
        }
        private void Equipment(NavigationView view, Transform parent)
        {
            var reason = view.Context.SelectedCharacterId == null ? "ActorSelectionRequired" : null;
            var equipReason = reason ?? (view.Read.Head.Business.ActiveHistory != null ? "ActiveAttemptConflict" : null);
            clearEquipmentButton.transform.SetParent(parent, false);
            preferenceButton.transform.SetParent(parent, false);
            Target(clearEquipmentButton, "fm.operation.clear_equipment", new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail,
                PermanentKind = CandidatePermanentKind.Equip }, equipReason,
                equipReason == "ActiveAttemptConflict" ? "fm.inventory.active_battle.locked" : null);
            // Nonempty Equip is unpublished; no item action is manufactured from an internal definition ID.
            var equipped = view.Read.Inventory.State.Loadouts.FirstOrDefault(x => x.Actor.CharacterId == view.Context.SelectedCharacterId);
            Target(preferenceButton, "fm.operation.set_item_preference", new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail,
                PermanentKind = CandidatePermanentKind.SetPreference, DefinitionId = equipped?.ItemId }, reason ?? (equipped?.ItemId == null ? "NoItemEquipped" : null),
                reason == null && equipped?.ItemId == null ? "fm.inventory.no_equipment" : null,
                reason == null && equipped?.ItemId == null ? (Func<KeyValuePair<string, string>[]>)(() => new[] {
                    NavigationBindings.Arg("characterName", NavigationBindings.Resolve(localization,
                        NavigationBindings.CharacterKey(view.Context.SelectedCharacterId))) }) : null);
        }
        private void Recipes(NavigationView view)
        {
            Characters(view, craftSection);
            var reason = view.Permanent?.RecipeAvailability ?? "ActorSelectionRequired";
            recipeAvailability.gameObject.SetActive(reason != "Defined");
            if (reason != "Defined") bindings.Text(recipeAvailability, localization,
                reason == "NoPublishedDefinition" ? "fm.crafting.no_recipes" : NavigationBindings.ReasonKey(NavigationReasonDomain.Permanent, reason));
            // Craft and its cost/output/source surfaces remain deferred (N-D03/N-P05).
        }
        public void Unbind()
        {
            generation++;
            bindings.Dispose();
            if (controller != null) controller.Changed -= Render;
            detail?.Unbind(); recovery?.Unbind();
            controller = null; localization = null;
        }
        public void Dispose() { Unbind(); }
        private void OnDisable() { Unbind(); }
        private void OnDestroy() { Unbind(); }
    }

    internal enum NavigationReasonDomain { Navigation, Entry, Party, Equipment, Permanent, Migration, Recovery, Notification }

    // Owns only this render's listeners, text bindings and clones. Business epoch/token protection stays in the controller.
    internal sealed class NavigationBindings : IDisposable
    {
        private readonly List<Action> detach = new List<Action>();
        private readonly List<LocalizedTmpText> texts = new List<LocalizedTmpText>();
        private readonly List<GameObject> rows = new List<GameObject>();
        private int generation;
        internal static KeyValuePair<string, string> Arg(string name, object value)
        { return new KeyValuePair<string, string>(name, value == null ? null : Convert.ToString(value, CultureInfo.InvariantCulture)); }
        internal void Detach(Action action) { detach.Add(action); }
        internal void Text(LocalizedTmpText text, LocalizationService localization, string key, params KeyValuePair<string, string>[] args)
        {
            if (text == null) throw new InvalidOperationException("A serialized navigation TMP binding is missing.");
            if (!texts.Contains(text)) texts.Add(text);
            text.Bind(localization, key, args);
        }
        internal void DynamicText(LocalizedTmpText text, LocalizationService localization, string key,
            Func<KeyValuePair<string, string>[]> arguments)
        {
            var currentGeneration = generation;
            Action<LocaleId> refresh = _ => {
                if (generation == currentGeneration && text != null) Text(text, localization, key, arguments());
            };
            localization.LocaleChanged += refresh;
            detach.Add(() => localization.LocaleChanged -= refresh);
            refresh(localization.CurrentLocale);
        }
        internal void Button(UnityEngine.UI.Button button, LocalizationService localization, string key, Action action,
            string reason = null, string reasonKey = null, Func<KeyValuePair<string, string>[]> captionArgs = null,
            Func<KeyValuePair<string, string>[]> reasonArgs = null, NavigationReasonDomain reasonDomain = NavigationReasonDomain.Navigation)
        {
            if (button == null) throw new InvalidOperationException("A serialized navigation button is missing.");
            button.gameObject.SetActive(false);
            var labels = button.GetComponentsInChildren<LocalizedTmpText>(true);
            if (labels.Length < 2) throw new InvalidOperationException("Navigation buttons require caption and reason TMP bindings.");
            if (captionArgs == null) Text(labels[0], localization, key); else DynamicText(labels[0], localization, key, captionArgs);
            labels[1].gameObject.SetActive(reason != null);
            if (reason != null)
            {
                var resolvedKey = reasonKey ?? ReasonKey(reasonDomain, reason);
                if (reasonArgs == null) Text(labels[1], localization, resolvedKey, ReasonArguments(resolvedKey, reason));
                else DynamicText(labels[1], localization, resolvedKey, reasonArgs);
            }
            var currentGeneration = generation;
            UnityAction listener = () => {
                if (currentGeneration == generation && button != null && button.isActiveAndEnabled && button.interactable) action();
            };
            button.onClick.AddListener(listener);
            detach.Add(() => { if (button != null) { button.onClick.RemoveListener(listener); button.interactable = false; } });
            button.interactable = reason == null;
            button.gameObject.SetActive(true);
        }
        internal static string CharacterKey(string id) => id == "W" ? "fm.name.character.w" : "";
        internal static string LevelKey(string id, string version) => id == "level:ch01-01" && version == "1" ? "fm.name.level.ch01_01" : "";
        internal static string ItemKey(string id)
        { return id == "item:tin" ? "fm.name.item.tin" : id == "item:wood" ? "fm.name.item.wood" : ""; }
        internal static string OperationKey(PreparedCandidateApplicationIntent intent)
        {
            var quote = intent?.GetPermanent();
            return OperationKey(intent?.Kind, quote?.Kind, intent?.Kind == CandidateApplicationKind.PermanentRequest &&
                quote?.Kind == CandidatePermanentKind.Equip && quote.DefinitionId == null);
        }
        // null is an explicitly unpublished operation; empty is an unknown fact and must diagnose.
        internal static string OperationKey(CandidateApplicationKind? kind, CandidatePermanentKind? permanent = null, bool clearsEquipment = false)
        {
            switch (kind)
            {
                case CandidateApplicationKind.InitializeProfile: return "fm.operation.initialize_profile";
                case CandidateApplicationKind.EnterAttempt: return "fm.operation.enter_attempt";
                case CandidateApplicationKind.EnterFormation: return "fm.operation.enter_formation";
                case CandidateApplicationKind.Attack: return "fm.operation.attack";
                case CandidateApplicationKind.Link: return "fm.operation.link";
                case CandidateApplicationKind.Rollback: return "fm.operation.rollback";
                case CandidateApplicationKind.SettleVictory: return "fm.operation.settle_victory";
                case CandidateApplicationKind.ExitAttempt: return "fm.operation.exit_attempt";
                case CandidateApplicationKind.RestartAttempt: return "fm.operation.restart_attempt";
                case CandidateApplicationKind.SetFormation: return "fm.operation.set_formation";
                case CandidateApplicationKind.AdvanceRecovery: return "fm.operation.advance_recovery";
                case CandidateApplicationKind.MigrateRoster:
                case CandidateApplicationKind.MigratePermanent: return "fm.profile.data_upgrade.title";
                case CandidateApplicationKind.PermanentRequest:
                    switch (permanent)
                    {
                        case CandidatePermanentKind.Equip: return clearsEquipment ? "fm.operation.clear_equipment" : null;
                        case CandidatePermanentKind.SetPreference: return "fm.operation.set_item_preference";
                        case CandidatePermanentKind.Craft:
                        case CandidatePermanentKind.UseExperienceCards:
                        case CandidatePermanentKind.LearnSkill:
                        case CandidatePermanentKind.ConfirmTeachingExplanation:
                        case CandidatePermanentKind.BeginTeachingGift: return null;
                        default: return "";
                    }
                default: return "";
            }
        }
        internal static string Resolve(LocalizationService localization, string key)
        {
            var result = localization.Resolve(key, null);
            return result.IsSuccess ? result.Text : null;
        }
        internal static string Party(LocalizationService localization, IReadOnlyList<string> slots)
        {
            var names = slots.Select(x => Resolve(localization, x == null ? "fm.party.slot.empty" : CharacterKey(x))).ToArray();
            return names.Any(x => x == null) ? null : string.Join(" / ", names);
        }
        internal static string RemainingTime(CandidateRecoveryPeriod period)
        {
            try
            {
                if (period?.Duration == null || period.Elapsed == null || period.Duration.Numerator.Sign <= 0 ||
                    period.Elapsed.Numerator.Sign < 0) return null;
                var budget = new ExactMathBudget();
                if (period.Elapsed.Compare(period.Duration, budget) > 0) return null;
                return BattleText.Duration(period.Duration.Subtract(period.Elapsed, budget));
            }
            catch (ExactMathLimitException) { return null; }
            catch (ArithmeticException) { return null; }
            catch (ArgumentException) { return null; }
        }
        internal static string ReasonKey(NavigationReasonDomain domain, string code, string field = null,
            CandidateApplicationPhase? phase = null, bool committed = false)
        {
            if (!Enum.IsDefined(typeof(NavigationReasonDomain), domain)) return "";
            if (domain == NavigationReasonDomain.Notification)
                return committed && code == "NotificationFailure" ? "fm.save_result.notification_failed" : "";
            switch (code)
            {
                case "ActorSelectionRequired": return "fm.inventory.character_required";
                case "NoReadyMember": return domain == NavigationReasonDomain.Entry || domain == NavigationReasonDomain.Party ? "fm.party.no_deployable" : "";
                case "SaveFailed": return phase == CandidateApplicationPhase.CommitUnknown ? "fm.save_recovery.unknown" :
                    phase == CandidateApplicationPhase.PendingPreparation ? "fm.save_recovery.blocking_notice" : "fm.save_recovery.failed";
                case "CommitUnknown": return "fm.save_recovery.unknown";
                case "CreationPending": return "fm.profile.creation_pending.body";
                case "SettlementRequired": return "fm.victory.pending.title";
                case "ResolutionRequired": return "fm.save_recovery.blocking_notice";
                case "Pending": return domain == NavigationReasonDomain.Recovery ? "fm.save_recovery.blocking_notice" : "";
                case "NotificationFailure": return "";
                case "UnsupportedBinding":
                    if (domain == NavigationReasonDomain.Entry || domain == NavigationReasonDomain.Navigation && field == "Navigation.Level")
                        return "fm.entry.binding_unsupported";
                    break;
                case "ActiveAttemptConflict":
                    if (domain == NavigationReasonDomain.Entry) return "fm.entry.active_battle.blocked";
                    if (domain == NavigationReasonDomain.Party) return "fm.party.active_battle.locked";
                    if (domain == NavigationReasonDomain.Equipment) return "fm.inventory.active_battle.locked";
                    break;
                case "AttemptActive":
                    if (domain == NavigationReasonDomain.Equipment) return "fm.inventory.active_battle.locked";
                    break;
                case "OperationSelectionRequired": return "fm.save_recovery.select_operation";
                case "CandidateSelectionRequired": return "fm.save_recovery.select_candidate";
                case "OriginalOwnerRequired": return "fm.save_recovery.return_to_original_flow";
                case "RetryRequired": return "fm.save_recovery.retry_before_resolve";
                case "PreviewRequired":
                case "EndConfirmationRequired": return "fm.common.state.review_required";
                case "ResultRequired": return "fm.save_recovery.result_required";
                case "StaleContext":
                case "StaleNavigationContext":
                case "StaleNavigationToken":
                case "StaleConfirmation":
                case "StaleHostRequest": return "fm.common.state.changed_review_again";
                case "RosterMigrationRequired": return domain == NavigationReasonDomain.Party ? "fm.party.upgrade_required" : "fm.profile.data_upgrade.body";
                case "NoPublishedDefinition": return domain == NavigationReasonDomain.Permanent ? "fm.crafting.no_recipes" : "";
                case "Ended": return domain == NavigationReasonDomain.Recovery ? "fm.save_recovery.ended_uncommitted" : "";
            }
            return ErrorCode(code) == null ? "" : "fm.common.business_attention";
        }
        internal static string ErrorCode(string code)
        {
            // These stable player codes are deliberately independent of fields, exceptions and enum spelling.
            switch (code)
            {
                case "SaveFailed": return "SAVE_FAILED";
                case "NotificationFailure": return "NOTIFICATION_FAILED";
                case "UnsupportedBinding": return "UNSUPPORTED_CONTENT";
                case "ActiveAttemptConflict":
                case "AttemptActive": return "BATTLE_ACTIVE";
                case "InconsistentBinding": return "INVALID_SELECTION";
                case "InvalidValue":
                case "MissingField": return "INVALID_INPUT";
                case "Limit": return "REQUEST_LIMIT";
                case "UnsupportedSchema": return "DATA_UPGRADE_REQUIRED";
                case "InvalidNavigationTransition":
                case "InvalidPhase": return "ACTION_UNAVAILABLE";
                case "LevelSelectionRequired": return "LEVEL_REQUIRED";
                case "NoActiveBattle": return "NO_ACTIVE_BATTLE";
                case "ConfirmationConsumed": return "REVIEW_REQUIRED";
                case "CostSelectionRequired": return "COST_SELECTION_REQUIRED";
                case "StorageFailure": return "STORAGE_UNAVAILABLE";
                case "NoSave": return "SAVE_NOT_FOUND";
                case "IncompleteOperationHistory": return "SAVE_HISTORY_INCOMPLETE";
                case "NotCommitted": return "SAVE_NOT_COMMITTED";
                case "BuilderFailed": return "REQUEST_BUILD_FAILED";
                case "BuilderRejected": return "REQUEST_REJECTED";
                case "Busy": return "REQUEST_BUSY";
                case "RestoreRequired":
                case "RecoveryBlocked": return "RECOVERY_REQUIRED";
                default: return null;
            }
        }
        internal static KeyValuePair<string, string>[] ReasonArguments(string key, string code)
        {
            return key == "fm.save_recovery.failed" || key == "fm.save_result.notification_failed" || key == "fm.common.business_attention" ?
                new[] { Arg("errorCode", ErrorCode(code)) } : Array.Empty<KeyValuePair<string, string>>();
        }
        internal static bool HasRecoverySurface(NavigationView view) => view.Route == PlayerNavigationRoute.Confirmation ||
            view.Route == PlayerNavigationRoute.Recovery || view.Route == PlayerNavigationRoute.CommittedResult;
        internal static bool IsStartup(CandidateApplicationPhase? phase) => phase == CandidateApplicationPhase.Unconfigured ||
            phase == CandidateApplicationPhase.InitializationReady || phase == CandidateApplicationPhase.CreationConfirmationRequired;
        internal static bool VerifiedResult(CandidateApplicationCallResult result) => result?.IsCommitted == true &&
            result.OriginalLookup?.IsFound == true && result.View?.IsPublishedHeadVerified == true &&
            result.OriginalCommitId == result.OriginalLookup.OriginalCommitId && result.LookupViewCommitId == result.View.PublishedSnapshot?.Header.CommitId;
        internal static bool PreferenceSaved(CandidateApplicationCallResult result) => VerifiedResult(result) &&
            result.OriginalLookup.Record.Intent.Kind == CandidateApplicationKind.PermanentRequest &&
            result.OriginalLookup.Record.Intent.GetPermanent()?.Kind == CandidatePermanentKind.SetPreference;
        private static bool HiddenState(NavigationView view, string code)
        {
            if (code == null || code == "Ready" || code == "Unconfigured" || code == "InitializationReady" ||
                code == "CreationConfirmationRequired" || code == "Disposed" || code == "Defined" ||
                code == "PendingPreparation" || code == "Completed" && VerifiedResult(view.Result)) return true;
            var target = view.HostRequest?.Kind;
            return target == PlayerNavigationTargetKind.CreationRequired && code == "CreationRequired" ||
                target == PlayerNavigationTargetKind.BattleSelectionRequested && code == "BattleSelectionRequested" ||
                target == PlayerNavigationTargetKind.ResumeBattleRequested && code == "ResumeBattleRequested" ||
                target == PlayerNavigationTargetKind.SettlementRequired && code == "SettlementRequired" ||
                target == PlayerNavigationTargetKind.RootBackRequested && code == "RootBackRequested";
        }
        private static NavigationReasonDomain ReasonDomain(NavigationView view, string field)
        {
            if (field == "Navigation.Level") return NavigationReasonDomain.Entry;
            if (field == "Navigation.Migration") return NavigationReasonDomain.Migration;
            if (field == "Navigation.PermanentKind" || field?.StartsWith("Permanent.", StringComparison.Ordinal) == true)
            {
                var permanentKind = view.Route == PlayerNavigationRoute.Detail ? (CandidatePermanentKind?)view.DetailKind :
                    view.Confirmation?.Quote?.Kind ?? view.Confirmation?.OriginalPermanentKind;
                return permanentKind == CandidatePermanentKind.Equip || permanentKind == CandidatePermanentKind.SetPreference ?
                    NavigationReasonDomain.Equipment : NavigationReasonDomain.Permanent;
            }
            if (field?.StartsWith("Formation.", StringComparison.Ordinal) == true) return NavigationReasonDomain.Party;
            return HasRecoverySurface(view) ? NavigationReasonDomain.Recovery : NavigationReasonDomain.Navigation;
        }
        internal void States(LocalizedTmpText status, LocalizedTmpText template, Transform parent,
            LocalizationService localization, NavigationView view, string prefix)
        {
            status.gameObject.SetActive(false);
            if (view.Read?.Application.Phase == CandidateApplicationPhase.Disposed) return;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var notification = VerifiedResult(view.Result) ? view.Result.NotificationFailure : null;
            Action<string, CandidateApplicationDiagnostic, string> show = (code, diagnostic, identity) => {
                if (HiddenState(view, code) || notification != null && diagnostic == notification) return;
                var domain = ReasonDomain(view, diagnostic?.FieldPath);
                if (code == "Ended") domain = NavigationReasonDomain.Recovery;
                var key = ReasonKey(domain, code, diagnostic?.FieldPath, view.Read?.Application.Phase);
                var args = ReasonArguments(key, code);
                var semantic = key + "|" + (args.Length == 0 ? "" : args[0].Value);
                if (!seen.Add(semantic)) return;
                if (!status.gameObject.activeSelf) { Text(status, localization, key, args); status.gameObject.SetActive(true); }
                else Row(template, parent, FightMatchViewId.Row(prefix + "." + identity), localization, key, args);
            };
            var phase = view.Read?.Application.Phase;
            var primary = phase == CandidateApplicationPhase.SaveFailed ? "SaveFailed" :
                phase == CandidateApplicationPhase.CommitUnknown ? "CommitUnknown" : view.Status;
            show(primary, view.Diagnostic, "Status");
            // Read-side storage details explain the same failed/unknown save, not a second player error.
            if (primary != "SaveFailed" && primary != "CommitUnknown")
            {
                if (view.Read?.IsAvailable == false) show(view.Read.Code, view.Read.Diagnostic, "Availability");
                if (view.Diagnostic != null) show(view.Diagnostic.Code, view.Diagnostic, "Diagnostic");
            }
            if (notification != null && view.Route == PlayerNavigationRoute.CommittedResult)
            {
                var key = ReasonKey(NavigationReasonDomain.Notification, notification.Code, committed: true);
                Row(template, parent, FightMatchViewId.Row(prefix + ".NotificationFailure"), localization, key, ReasonArguments(key, notification.Code));
            }
        }
        internal GameObject Clone(GameObject template, Transform parent, string id)
        {
            if (template == null || template.activeInHierarchy) throw new InvalidOperationException("Navigation row prototypes must belong to the inactive TemplatePool.");
            var row = UnityEngine.Object.Instantiate(template, template.transform.parent, false);
            row.SetActive(false);
            row.transform.SetParent(parent, false);
            var identity = row.GetComponent<FightMatchViewId>();
            if (identity == null) throw new InvalidOperationException("Navigation row prototypes require a serialized view ID.");
            foreach (var descendant in row.GetComponentsInChildren<FightMatchViewId>(true))
                if (descendant != identity) descendant.Assign(FightMatchViewId.Row(PlayerNavigationView.Key(id, descendant.Id)));
            identity.Assign(id);
            rows.Add(row);
            return row;
        }
        internal void CloneButton(UnityEngine.UI.Button template, Transform parent, string id, LocalizationService localization,
            string key, Action action, string reason = null, string reasonKey = null,
            Func<KeyValuePair<string, string>[]> captionArgs = null)
        { Button(Clone(template.gameObject, parent, id).GetComponent<UnityEngine.UI.Button>(), localization, key, action, reason, reasonKey, captionArgs); }
        internal void Row(LocalizedTmpText template, Transform parent, string id, LocalizationService localization, string key,
            params KeyValuePair<string, string>[] args)
        {
            var row = Clone(template.gameObject, parent, id);
            Text(row.GetComponent<LocalizedTmpText>(), localization, key, args);
            row.SetActive(true);
        }
        internal void DynamicRow(LocalizedTmpText template, Transform parent, string id, LocalizationService localization,
            string key, Func<KeyValuePair<string, string>[]> arguments)
        {
            var row = Clone(template.gameObject, parent, id);
            DynamicText(row.GetComponent<LocalizedTmpText>(), localization, key, arguments);
            row.SetActive(true);
        }
        public void Dispose()
        {
            generation++;
            foreach (var action in detach) action();
            detach.Clear();
            foreach (var text in texts) if (text != null) text.Unbind();
            texts.Clear();
            foreach (var row in rows)
            {
                if (row == null) continue;
                row.SetActive(false); row.transform.SetParent(null, false);
                if (UnityEngine.Application.isPlaying) UnityEngine.Object.Destroy(row); else UnityEngine.Object.DestroyImmediate(row);
            }
            rows.Clear();
        }
    }
}
