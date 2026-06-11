using UnityEngine;
using System.Collections.Generic;
using DesktopPet.Events;

namespace DesktopPet
{
    public class UIManager : MonoBehaviour
    {
        [System.Serializable]
        public class PanelPrefab
        {
            public string id;
            public UIPanel prefab;
        }

        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private Transform panelRoot;
        [SerializeField] private WindowController windowController;
        [SerializeField] private Transform petRoot;
        [SerializeField] private string petRootName = "Monkey";

        [Header("Pet Size")]
        [SerializeField] private float minPetScale = 0.25f;
        [SerializeField] private float maxPetScale = 1.25f;
        [SerializeField] private float defaultPetScale = 0.55f;
        [SerializeField] private bool applyDefaultScaleOnStart = true;

        [Header("Panels")]
        [SerializeField] private PanelPrefab[] panelPrefabs;
        [SerializeField] private SettingsPanel settingsPanelPrefab;
        [SerializeField] private KeyCode closeTopPanelKey = KeyCode.Escape;

        private readonly Dictionary<string, UIPanel> panels = new Dictionary<string, UIPanel>();
        private readonly Dictionary<UIPanel, string> panelIds = new Dictionary<UIPanel, string>();
        private readonly List<UIPanel> openStack = new List<UIPanel>();
        private SettingsPanel settingsPanel;
        private bool clickThroughBeforePanel;
        private bool isManagingClickThrough;

        private void Awake()
        {
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }

            if (windowController == null)
            {
                windowController = FindObjectOfType<WindowController>();
            }

            if (petRoot == null)
            {
                var pet = GameObject.Find(petRootName);
                petRoot = pet != null ? pet.transform : null;
            }

            if (petRoot != null && applyDefaultScaleOnStart)
            {
                petRoot.localScale = Vector3.one * defaultPetScale;
            }
        }

        private void Start()
        {
            if (canvas == null)
            {
                Debug.LogWarning("UIManager needs a Canvas.");
                return;
            }

            if (panelRoot == null)
            {
                var root = new GameObject("Panels", typeof(RectTransform));
                var rect = root.GetComponent<RectTransform>();
                rect.SetParent(canvas.transform, false);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
                panelRoot = rect;
            }

            CreateConfiguredPanels();
            CreateSettingsPanel();
        }

        private void Update()
        {
            if (Input.GetKeyDown(closeTopPanelKey))
            {
                CloseTopPanel();
            }

            foreach (var panel in panels.Values)
            {
                if (panel.Hotkey != KeyCode.None && Input.GetKeyDown(panel.Hotkey))
                {
                    TogglePanel(panelIds[panel]);
                }
            }
        }

        public void OpenPanel(string panelId)
        {
            if (!panels.TryGetValue(panelId, out var panel))
            {
                Debug.LogWarning($"Panel not found: {panelId}");
                return;
            }

            if (panel.CloseOthersWhenOpened)
            {
                CloseAllPanels();
            }

            panel.Open();
            TrackOpenPanel(panel);
            RefreshWindowInputMode();
            GameEventBus.Publish(new PanelOpenedEvent(panelId));
        }

        public void ClosePanel(string panelId)
        {
            if (!panels.TryGetValue(panelId, out var panel))
            {
                return;
            }

            panel.Close();
            openStack.Remove(panel);
            RefreshWindowInputMode();
            GameEventBus.Publish(new PanelClosedEvent(panelId));
        }

        public void TogglePanel(string panelId)
        {
            if (!panels.TryGetValue(panelId, out var panel))
            {
                Debug.LogWarning($"Panel not found: {panelId}");
                return;
            }

            if (panel.IsOpen)
            {
                ClosePanel(panelId);
            }
            else
            {
                OpenPanel(panelId);
            }
        }

        public void CloseAllPanels()
        {
            foreach (var entry in panels)
            {
                var panel = entry.Value;
                var wasOpen = panel != null && panel.IsOpen;
                if (panel != null)
                {
                    panel.Close();
                }

                if (wasOpen)
                {
                    GameEventBus.Publish(new PanelClosedEvent(entry.Key));
                }
            }

            openStack.Clear();
            RefreshWindowInputMode();
        }

        public void ToggleSettingsPanel()
        {
            TogglePanel("settings");
        }

        private void CreateConfiguredPanels()
        {
            if (panelPrefabs == null)
            {
                return;
            }

            foreach (var entry in panelPrefabs)
            {
                if (entry == null || entry.prefab == null)
                {
                    continue;
                }

                var panel = Instantiate(entry.prefab, panelRoot, false);
                var id = string.IsNullOrWhiteSpace(entry.id) ? panel.PanelId : entry.id;
                RegisterPanel(id, panel);
            }
        }

        private void CreateSettingsPanel()
        {
            if (settingsPanelPrefab == null)
            {
                Debug.LogWarning("UIManager needs a settings panel prefab.");
                return;
            }

            var initialScale = petRoot != null ? petRoot.localScale.x : defaultPetScale;
            settingsPanel = Instantiate(settingsPanelPrefab, panelRoot, false);
            settingsPanel.Initialize(petRoot, minPetScale, maxPetScale, initialScale, defaultPetScale);
            RegisterPanel("settings", settingsPanel);
        }

        private void RegisterPanel(string panelId, UIPanel panel)
        {
            if (panel == null || string.IsNullOrWhiteSpace(panelId))
            {
                return;
            }

            panels[panelId] = panel;
            panelIds[panel] = panelId;
            panel.Close();
        }

        private void TrackOpenPanel(UIPanel panel)
        {
            openStack.Remove(panel);
            openStack.Add(panel);
        }

        private void CloseTopPanel()
        {
            for (var i = openStack.Count - 1; i >= 0; i--)
            {
                var panel = openStack[i];
                if (panel != null && panel.IsOpen)
                {
                    panel.Close();
                    openStack.RemoveAt(i);
                    RefreshWindowInputMode();
                    GameEventBus.Publish(new PanelClosedEvent(panelIds[panel]));
                    return;
                }
            }
        }

        private void RefreshWindowInputMode()
        {
            if (windowController == null)
            {
                return;
            }

            var hasBlockingPanel = false;
            foreach (var panel in panels.Values)
            {
                if (panel != null && panel.IsOpen && panel.BlockClickThrough)
                {
                    hasBlockingPanel = true;
                    break;
                }
            }

            if (hasBlockingPanel && !isManagingClickThrough)
            {
                clickThroughBeforePanel = windowController.IsClickThrough;
                isManagingClickThrough = true;
            }

            if (hasBlockingPanel)
            {
                windowController.SetClickThrough(false);
            }
            else if (isManagingClickThrough)
            {
                windowController.SetClickThrough(clickThroughBeforePanel);
                isManagingClickThrough = false;
            }
        }
    }
}
