using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public sealed class UIDraggablePanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private bool keepInsideParent = true;
        private RectTransform parent;
        private Vector2 pointerOffset;
        private bool dragging;

        private void Awake()
        {
            if (target == null) target = transform as RectTransform;
            parent = target != null ? target.parent as RectTransform : null;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragging = false;
            if (eventData.button != PointerEventData.InputButton.Left || target == null || parent == null) return;
            if (eventData.pointerPressRaycast.gameObject != null &&
                eventData.pointerPressRaycast.gameObject.GetComponentInParent<Selectable>() != null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, eventData.position, eventData.pressEventCamera, out var pointer);
            pointerOffset = target.anchoredPosition - pointer;
            dragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging || eventData.button != PointerEventData.InputButton.Left || target == null || parent == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, eventData.position, eventData.pressEventCamera, out var pointer)) return;
            target.anchoredPosition = pointer + pointerOffset;
            if (keepInsideParent) ClampInsideParent();
        }

        public void OnEndDrag(PointerEventData eventData) => dragging = false;

        private void ClampInsideParent()
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            var parentCorners = new Vector3[4];
            parent.GetWorldCorners(parentCorners);
            var delta = Vector3.zero;
            if (corners[0].x < parentCorners[0].x) delta.x = parentCorners[0].x - corners[0].x;
            else if (corners[2].x > parentCorners[2].x) delta.x = parentCorners[2].x - corners[2].x;
            if (corners[0].y < parentCorners[0].y) delta.y = parentCorners[0].y - corners[0].y;
            else if (corners[2].y > parentCorners[2].y) delta.y = parentCorners[2].y - corners[2].y;
            target.position += delta;
        }
    }
}
