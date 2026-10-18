using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FightMatch.Host
{
    public sealed class FightMatchLocaleDropdown : TMP_Dropdown
    {
        internal Func<bool> CanOpen;
        private bool pointerReturnSelection;
        private int pointerCloseFrame = -1;

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (CanOpen == null || !CanOpen()) return;
            SetReturnFocus(false);
            SetValueWithoutNotify(-1);
            base.OnPointerClick(eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (CanOpen == null || !CanOpen()) return;
            SetReturnFocus(false);
            SetValueWithoutNotify(-1);
            base.OnSubmit(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            pointerReturnSelection = pointerCloseFrame == Time.frameCount;
            pointerCloseFrame = -1;
            base.OnSelect(eventData);
        }

        public override void OnMove(AxisEventData eventData)
        {
            SetReturnFocus(false);
            base.OnMove(eventData);
        }

        public override void OnCancel(BaseEventData eventData)
        {
            SetReturnFocus(false);
            base.OnCancel(eventData);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            pointerReturnSelection = false;
            pointerCloseFrame = -1;
            base.OnDeselect(eventData);
        }

        private void SetReturnFocus(bool fromPointer)
        {
            pointerReturnSelection = fromPointer;
            pointerCloseFrame = fromPointer ? Time.frameCount : -1;
            DoStateTransition(currentSelectionState, false);
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            // Retain TMP's selection for keyboard access without a pointer-close tint
            // that disappears on the next board press. Other selectable states stay intact.
            if (pointerReturnSelection && state == SelectionState.Selected) state = SelectionState.Normal;
            base.DoStateTransition(state, instant);
        }

        internal void Close()
        {
            // TMP's OnDisable destroys its list and blocker immediately, without a fade
            // coroutine or a late Select() stealing focus from a recovery/diagnostic owner.
            StopAllCoroutines();
            var wasEnabled = enabled;
            enabled = false;
            enabled = wasEnabled;
        }

        protected override DropdownItem CreateItem(DropdownItem itemTemplate)
        {
            var item = base.CreateItem(itemTemplate);
            ObserveCloseInput(item.gameObject);
            return item;
        }

        protected override GameObject CreateBlocker(Canvas rootCanvas)
        {
            var blocker = base.CreateBlocker(rootCanvas);
            ObserveCloseInput(blocker);
            return blocker;
        }

        private void ObserveCloseInput(GameObject target)
        {
            var binding = CanOpen;
            target.AddComponent<FightMatchLocaleCloseInput>().CloseInput = fromPointer =>
            {
                // ExecuteEvents snapshots handlers: TMP may close and deactivate this
                // generated object before this observer receives the same event.
                if (this != null && binding != null && ReferenceEquals(CanOpen, binding))
                    SetReturnFocus(fromPointer);
            };
        }

        protected override void DestroyBlocker(GameObject blocker)
        { blocker.SetActive(false); base.DestroyBlocker(blocker); }

        protected override void DestroyDropdownList(GameObject dropdownList)
        { dropdownList.SetActive(false); base.DestroyDropdownList(dropdownList); }
    }

    // Runtime-only observer on this dropdown's generated items and blocker. It does
    // not consume events or handle navigation/scrolling on behalf of other controls.
    internal sealed class FightMatchLocaleCloseInput : MonoBehaviour, IPointerClickHandler, ISubmitHandler, ICancelHandler
    {
        internal Action<bool> CloseInput;
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) CloseInput?.Invoke(true);
        }
        public void OnSubmit(BaseEventData eventData) => CloseInput?.Invoke(false);
        public void OnCancel(BaseEventData eventData) => CloseInput?.Invoke(false);
    }
}
