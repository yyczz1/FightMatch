using UnityEngine;
using UnityEngine.EventSystems;

namespace FightMatch.Presentation
{
    public sealed class FightMatchStandaloneInputModule : StandaloneInputModule
    {
        public override void Process()
        {
            NormalizeCanceledTouches();
            base.Process();
        }

        internal void NormalizeCanceledTouches()
        {
            for (var i = 0; i < input.touchCount; i++)
            {
                var touch = input.GetTouch(i);
                if (touch.type == TouchType.Indirect || touch.phase != TouchPhase.Canceled ||
                    !m_PointerData.TryGetValue(touch.fingerId, out var pointer)) continue;
                var target = pointer.pointerDrag != null ? pointer.pointerDrag : pointer.pointerPress;
                if (target != null) target.GetComponentInParent<CandidateBoardElement>()?.CancelTrueTouch(touch.fingerId, touch.position);
            }
        }
    }
}
