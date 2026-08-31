using DesktopPet.Events;
using DesktopPet.Pet.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public sealed class PhoneTrayController : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.R;
        private PetInteractionController interaction;
        private WindowController windowController;
        private UIManager uiManager;
        private GameObject panel;
        private bool clickThroughBeforeOpen;

        public void Initialize(PetInteractionController target)
        {
            interaction = target;
            windowController = FindObjectOfType<WindowController>();
            uiManager = FindObjectOfType<UIManager>();
            BuildUI();
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(toggleKey)) Toggle();
            if (panel != null && panel.activeSelf && UnityEngine.Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        public void Toggle() { if (panel != null && panel.activeSelf) Close(); else Open(); }

        public void Open()
        {
            if (panel == null) return;
            clickThroughBeforeOpen = windowController != null && windowController.IsClickThrough;
            if (windowController != null) windowController.SetClickThrough(false);
            panel.SetActive(true);
            GameEventBus.Publish(new PanelOpenedEvent("phone"));
        }

        public void Close()
        {
            if (panel == null) return;
            panel.SetActive(false);
            if (windowController != null) windowController.SetClickThrough(clickThroughBeforeOpen);
            GameEventBus.Publish(new PanelClosedEvent("phone"));
        }

        private void BuildUI()
        {
            if (panel != null) return;
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;
            panel = new GameObject("PhoneTray", typeof(RectTransform), typeof(Image));
            var rect = panel.GetComponent<RectTransform>();
            rect.SetParent(canvas.transform, false);
            rect.anchorMin = new Vector2(0.5f, 0f); rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f); rect.anchoredPosition = new Vector2(0f, 18f); rect.sizeDelta = new Vector2(420f, 82f);
            panel.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 0.94f);
            AddButton("喂食", -150f, () => { interaction.RequestFeed(); Close(); });
            AddButton("呼唤", -50f, () => interaction.RequestCall());
            AddButton("家具", 50f, () => GameEventBus.Publish(new PetFeedbackEvent("家具功能将在 M3 开放", false)));
            AddButton("设置", 150f, () => { Close(); if (uiManager != null) uiManager.ToggleSettingsPanel(); });
            panel.SetActive(false);
        }

        private void AddButton(string label, float x, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(panel.transform, false); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, 0f); rect.sizeDelta = new Vector2(88f, 54f);
            go.GetComponent<Image>().color = new Color(0.25f, 0.28f, 0.36f, 1f);
            go.GetComponent<Button>().onClick.AddListener(action);
            var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.SetParent(rect, false); textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.sizeDelta = Vector2.zero;
            var text = textGo.GetComponent<Text>(); text.text = label; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); text.fontSize = 18;
        }
    }
}
