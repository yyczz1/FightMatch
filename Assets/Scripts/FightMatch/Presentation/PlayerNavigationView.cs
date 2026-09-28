using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using UnityEngine.UIElements;
using NavigationView = FightMatch.Application.PlayerNavigationView;

namespace FightMatch.Presentation
{
    public sealed class PlayerNavigationView : VisualElement, IDisposable
    {
        private readonly PlayerNavigationController controller;
        private readonly NavigationBindings bindings = new NavigationBindings();
        private readonly PlayerPermanentDetailView detail;
        private readonly PlayerNavigationRecoveryView recovery;
        private bool closed;
        public PlayerNavigationView(PlayerNavigationController controller)
        {
            this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
            name = "player-navigation";
            detail = new PlayerPermanentDetailView(controller);
            recovery = new PlayerNavigationRecoveryView(controller);
            controller.Changed += Render;
            RegisterCallback<DetachFromPanelEvent>(Detached);
            Render();
        }
        private void Detached(DetachFromPanelEvent evt) { if (evt.target == this) Dispose(); }
        internal static string Key(params string[] parts) => string.Join("|", parts.Select(x => Uri.EscapeDataString(x ?? "")));
        private void Target(string name, string text, PlayerNavigationTarget target, string reason = null)
        { bindings.Button(this, name, text, controller.NavigationHandler(target), reason); }
        private void Page(PlayerNavigationTargetKind kind, string text)
        { Target("nav-" + kind, text, new PlayerNavigationTarget { Kind = kind }); }
        private void Render()
        {
            if (closed) return;
            bindings.Dispose(); detail.ClearBindings(); recovery.ClearBindings(); Clear();
            var view = controller.View;
            Add(new Label(view.Route.ToString()) { name = "navigation-route" });
            Add(new Label(view.Status ?? "") { name = "navigation-status" });
            Add(new Label(view.Read?.Code ?? "ResolutionRequired") { name = "navigation-availability" });
            if (view.Result?.IsCommitted == true && view.Route != PlayerNavigationRoute.CommittedResult)
                Add(new Label("Previous result: " + view.Result.OriginalLookup.Record.OperationId + " / " + view.Result.OriginalCommitId) { name = "navigation-last-result" });
            if (view.Diagnostic != null) Add(new Label(view.Diagnostic.Code + " " + view.Diagnostic.FieldPath) { name = "navigation-diagnostic" });
            if (view.Read?.IsAvailable == true)
            {
                Page(PlayerNavigationTargetKind.MapAdventure, "Adventure");
                Page(PlayerNavigationTargetKind.Team, "Team"); Page(PlayerNavigationTargetKind.Bag, "Bag");
                if (view.Route == PlayerNavigationRoute.MapAdventure) Map(view);
                if (view.Route == PlayerNavigationRoute.Preparation) Preparation(view);
                if (view.Route == PlayerNavigationRoute.Team) Team(view);
                if (view.Route == PlayerNavigationRoute.Bag) Bag(view);
                if (view.Route == PlayerNavigationRoute.CraftList) Recipes(view);
                if (view.Route == PlayerNavigationRoute.Detail) { Add(detail); detail.Render(view); }
                var format = (uint)view.Read.Head.Business.Format;
                if (format == 2 || format == 3)
                    bindings.Button(this, "nav-migration", "Review save upgrade " + format + " → " + (format + 1),
                        controller.PreviewHandler(() => new PlayerNavigationDraft {
                            Kind = PlayerNavigationDraftKind.Migration, FromFormat = format, ToFormat = format + 1 }));
            }
            if (view.Route == PlayerNavigationRoute.Gate)
            {
                if (view.Status == "CreationPending" || view.Read?.Application.Phase == CandidateApplicationPhase.Unconfigured)
                    Page(PlayerNavigationTargetKind.CreationRequired, "Open profile confirmation");
                if (view.Read?.Head?.Continuation != null) Page(PlayerNavigationTargetKind.SettlementRequired, "Open settlement");
            }
            if (view.Route == PlayerNavigationRoute.Confirmation || view.Route == PlayerNavigationRoute.Recovery ||
                view.Route == PlayerNavigationRoute.CommittedResult)
            { Add(recovery); recovery.Render(view); }
            foreach (var action in new[] { PlayerNavigationAction.Back, PlayerNavigationAction.Cancel })
                bindings.Button(this, "nav-" + action, action.ToString(), controller.ActionHandler(action), view.ReasonFor(action));
            bindings.Button(this, "nav-query", "Refresh view", controller.Refresh);
        }
        private void Map(NavigationView view)
        {
            foreach (var level in view.Read.Levels)
            {
                var state = view.Read.Progression.Levels.FirstOrDefault(x => x.Level.LevelId == level.LevelId && x.Level.LevelVersion == level.LevelVersion);
                var label = level.LevelId + " / " + level.LevelVersion + (state?.OpenFact == null ? " Locked" : " Open");
                Target("level-" + Key(view.Read.Binding.PackageId, view.Read.Binding.ContentFingerprint, level.LevelId, level.LevelVersion),
                    label, new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Preparation,
                        LevelId = level.LevelId, LevelVersion = level.LevelVersion, Binding = view.Read.Binding });
                Add(new Label("Requires: " + (state?.Level.UnlockAfterLevelId ?? "—") + " / " + state?.Level.UnlockRuleId +
                    "; cleared: " + (state?.FirstClear != null)));
            }
            if (view.Read.Levels.Count == 0) Add(new Label("NoPublishedDefinition"));
        }
        private void Preparation(NavigationView view)
        {
            Add(new Label(view.Context.LevelId + " / " + view.Context.LevelVersion));
            Add(new Label(string.Join(" / ", view.Read.Roster.Slots.Select(x => x ?? "Empty"))));
            Add(new Label("EntryCheckRequired"));
            if (view.Read.Lifecycle.Enter.Reason != null) Add(new Label(view.Read.Lifecycle.Enter.Reason));
            Characters(view);
            Target("nav-battle", "Review battle entry", new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.BattleSelectionRequested },
                view.Read.Head.Business.ActiveHistory == null ? null : "ActiveAttemptConflict");
            if (view.Read.Head.Business.ActiveHistory != null)
                Page(PlayerNavigationTargetKind.ResumeBattleRequested, "Return to current battle");
        }
        private void Characters(NavigationView view)
        {
            foreach (var character in view.Read.Roster.Characters)
            {
                Target("character-" + Key(character.CharacterId), character.CharacterId + " / " + character.ClassId +
                    " Lv" + character.Level + " XP " + character.Experience,
                    new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.SelectCharacter, CharacterId = character.CharacterId });
                Add(new Label("Recovery periods: " + character.RecoveryPeriods.Count + "; revision " + character.StateRevision));
            }
            Add(new Label("Selected: " + (view.Context.SelectedCharacterId ?? "None")));
        }
        private void Team(NavigationView view)
        {
            Characters(view);
            var choices = new List<string> { "" }; choices.AddRange(view.Read.Roster.Characters.Select(x => x.CharacterId));
            var slots = new DropdownField[3];
            for (var i = 0; i < slots.Length; i++)
            {
                slots[i] = new DropdownField("Slot " + (i + 1), choices, view.Read.Roster.Slots[i] ?? "") { name = "team-slot-" + i };
                Add(slots[i]);
            }
            bindings.Button(this, "team-preview", "Review formation", controller.PreviewHandler(() => new PlayerNavigationDraft {
                Kind = PlayerNavigationDraftKind.Formation, Slots = slots.Select(x => string.IsNullOrEmpty(x.value) ? null : x.value).ToArray() }),
                view.Read.Roster.UnavailabilityReason);
            Equipment(view);
        }
        private void Bag(NavigationView view)
        {
            foreach (var item in view.Read.Inventory.Items)
                Add(new Label(item.ItemId + ": total " + item.T + ", equipped " + item.L + ", reserved " + item.R + ", free " + item.F));
            foreach (var load in view.Read.Inventory.State.Loadouts)
                Add(new Label(load.Actor.CharacterId + ": " + (load.ItemId ?? "Empty") + " × " + load.L));
            foreach (var row in view.Read.Inventory.State.ActiveCarry?.Rows ?? Array.Empty<CandidateCarryRow>())
                Add(new Label("Battle carry " + row.Actor.CharacterId + ": " + row.ItemId + " × " + row.C));
            Characters(view); Page(PlayerNavigationTargetKind.CraftList, "Crafting"); Equipment(view);
            foreach (var record in view.Read.Head.Records)
                Target("bag-operation-" + Key(record.OperationId), "Select original " + record.OperationId,
                    new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.OriginalOperation, OperationId = record.OperationId });
            bindings.Button(this, "bag-original-result", "Read selected original result",
                controller.ActionHandler(PlayerNavigationAction.SelectOriginalOperation), view.ReasonFor(PlayerNavigationAction.SelectOriginalOperation));
        }
        private void Equipment(NavigationView view)
        {
            var reason = view.Context.SelectedCharacterId == null ? "ActorSelectionRequired" : null;
            var equipReason = reason ?? (view.Read.Head.Business.ActiveHistory != null ? "ActiveAttemptConflict" : null);
            Target("equip-empty", "Clear equipment", new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail,
                PermanentKind = CandidatePermanentKind.Equip }, equipReason);
            foreach (var item in view.Read.Inventory.State.Definition.Items.Where(x => x.Kind == CandidateInventoryItemKind.OrdinaryTactical))
                Target("equip-" + Key(item.ItemId), "Equip " + item.ItemId, new PlayerNavigationTarget {
                    Kind = PlayerNavigationTargetKind.Detail, PermanentKind = CandidatePermanentKind.Equip, DefinitionId = item.ItemId }, equipReason);
            var equipped = view.Read.Inventory.State.Loadouts.FirstOrDefault(x => x.Actor.CharacterId == view.Context.SelectedCharacterId);
            Target("preference", "Item use preference", new PlayerNavigationTarget {
                Kind = PlayerNavigationTargetKind.Detail, PermanentKind = CandidatePermanentKind.SetPreference,
                DefinitionId = equipped?.ItemId }, reason ?? (equipped?.ItemId == null ? "NoItemEquipped" : null));
        }
        private void Recipes(NavigationView view)
        {
            Characters(view);
            Add(new Label(view.Permanent?.RecipeAvailability ?? "ActorSelectionRequired") { name = "recipe-availability" });
            foreach (var recipe in view.Permanent?.Definitions?.Records.Where(x => x.Kind == CandidatePermanentDefinitionKind.Recipe) ??
                Enumerable.Empty<CandidatePermanentDefinition>())
                Target("recipe-" + Key(recipe.Id, recipe.RecordVersion.ToString()), recipe.Id + " / " + recipe.RecordVersion,
                    new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail, PermanentKind = CandidatePermanentKind.Craft,
                        DefinitionId = recipe.Id });
        }
        public void Dispose()
        {
            if (closed) return;
            closed = true; controller.Changed -= Render;
            UnregisterCallback<DetachFromPanelEvent>(Detached);
            bindings.Dispose(); detail.ClearBindings(); recovery.ClearBindings();
        }
    }
}
