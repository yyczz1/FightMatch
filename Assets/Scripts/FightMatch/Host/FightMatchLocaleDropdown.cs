using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FightMatch.Host
{
    public sealed class FightMatchLocaleDropdown : TMP_Dropdown
    {
        internal Func<bool> CanOpen;

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (CanOpen == null || !CanOpen()) return;
            SetValueWithoutNotify(-1);
            base.OnPointerClick(eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (CanOpen == null || !CanOpen()) return;
            SetValueWithoutNotify(-1);
            base.OnSubmit(eventData);
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
        { blocker.SetActive(false); base.DestroyBlocker(blocker); }

        protected override void DestroyDropdownList(GameObject dropdownList)
        { dropdownList.SetActive(false); base.DestroyDropdownList(dropdownList); }
    }
}
