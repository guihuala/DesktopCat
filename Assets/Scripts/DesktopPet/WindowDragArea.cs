using UnityEngine;
using UnityEngine.EventSystems;

namespace DesktopPet
{
    public class WindowDragArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private WindowController windowController;
        [SerializeField] private int dragMouseButton = 0;

        private void Awake()
        {
            if (windowController == null)
            {
                windowController = FindObjectOfType<WindowController>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != ToInputButton(dragMouseButton))
            {
                return;
            }

            if (windowController != null)
            {
                windowController.BeginDrag();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != ToInputButton(dragMouseButton))
            {
                return;
            }

            if (windowController != null)
            {
                windowController.EndDrag();
            }
        }

        private static PointerEventData.InputButton ToInputButton(int button)
        {
            switch (button)
            {
                case 1:
                    return PointerEventData.InputButton.Right;
                case 2:
                    return PointerEventData.InputButton.Middle;
                default:
                    return PointerEventData.InputButton.Left;
            }
        }
    }
}
