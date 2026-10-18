using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FightMatch.Host
{
    public sealed class FightMatchLocaleDropdown : TMP_Dropdown
    {
        internal Func<bool> CanOpen;
        private bool openedByPointer;
        private bool pointerReturnSelection;
        private int pointerCloseFrame = -1;

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (CanOpen == null || !CanOpen()) return;
            openedByPointer = true;
            pointerReturnSelection = false;
            pointerCloseFrame = -1;
            SetValueWithoutNotify(-1);
            base.OnPointerClick(eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (CanOpen == null || !CanOpen()) return;
            openedByPointer = false;
            pointerReturnSelection = false;
            pointerCloseFrame = -1;
            DoStateTransition(currentSelectionState, false);
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
            openedByPointer = false;
            pointerReturnSelection = false;
            pointerCloseFrame = -1;
            DoStateTransition(currentSelectionState, false);
            base.OnMove(eventData);
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

        protected override void DestroyBlocker(GameObject blocker)
        {
            // Both Hide() and our synchronous Close() destroy the blocker. TMP can
            // call Select() after an option callback has already called Close().
            if (openedByPointer)
            {
                pointerCloseFrame = Time.frameCount;
                pointerReturnSelection = true;
                DoStateTransition(currentSelectionState, false);
            }
            blocker.SetActive(false); base.DestroyBlocker(blocker);
        }

        protected override void DestroyDropdownList(GameObject dropdownList)
        { dropdownList.SetActive(false); base.DestroyDropdownList(dropdownList); }
    }
}
