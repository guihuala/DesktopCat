using DesktopPet.Events;
using DesktopPet.Pet.Interaction;
using UnityEngine;

namespace DesktopPet.UI
{
    public sealed class PhoneTrayController : MonoBehaviour
    {
        private PetInteractionController interaction;
        private WindowController windowController;
        private UIManager uiManager;
        [SerializeField] private GameObject panel;
        private bool clickThroughBeforeOpen;

        public void Initialize(PetInteractionController target)
        {
            interaction = target;
            windowController = FindObjectOfType<WindowController>();
            uiManager = FindObjectOfType<UIManager>();
            if (panel == null) Debug.LogError("Phone tray requires a panel assigned from the scene HUD.", this);
            else if (panel.activeSelf && windowController != null)
            {
                clickThroughBeforeOpen = windowController.IsClickThrough;
                windowController.SetClickThrough(false);
            }
        }

        public void Feed() { if (interaction != null) interaction.RequestFeed(); }
        public void Call() { if (interaction != null) interaction.RequestCall(); }
        public void Furniture() => GameEventBus.Publish(new PetFeedbackEvent("家具功能将在 M3 开放", false));
        public void Settings() { if (uiManager != null) uiManager.ToggleSettingsPanel(); }

        private void Update()
        {
            if (panel != null && panel.activeSelf && UnityEngine.Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        public void Toggle() { if (panel != null && panel.activeSelf) Close(); else Open(); }

        public void Open()
        {
            if (panel == null || panel.activeSelf) return;
            clickThroughBeforeOpen = windowController != null && windowController.IsClickThrough;
            if (windowController != null) windowController.SetClickThrough(false);
            panel.SetActive(true);
            GameEventBus.Publish(new PanelOpenedEvent("phone"));
        }

        public void Close()
        {
            if (panel == null || !panel.activeSelf) return;
            panel.SetActive(false);
            if (windowController != null) windowController.SetClickThrough(clickThroughBeforeOpen);
            GameEventBus.Publish(new PanelClosedEvent("phone"));
        }

    }
}
