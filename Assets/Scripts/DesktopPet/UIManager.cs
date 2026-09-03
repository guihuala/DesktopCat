using UnityEngine;
using System.Collections.Generic;
using DesktopPet.Events;
using DesktopPet.Rewards;
using DesktopPet.UI;
using DesktopPet.Pet.Presentation;
using DesktopPet.Save;

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
        [SerializeField] private string petRootName = "Cat";

        [Header("Pet Size")]
        [SerializeField] private float minPetScale = 0.25f;
        [SerializeField] private float maxPetScale = 1.25f;
        [SerializeField] private float defaultPetScale = 0.55f;
        [SerializeField] private bool applyDefaultScaleOnStart = true;

        [Header("Panels")]
        [SerializeField] private PanelPrefab[] panelPrefabs;
        [SerializeField] private SettingsPanel settingsPanelPrefab;
        [SerializeField] private RewardClaimPanel rewardClaimPanelPrefab;
        [SerializeField] private FurniturePlacementPanel furniturePlacementPanelPrefab;
        [SerializeField] private PetAppearancePanel petAppearancePanelPrefab;
        [SerializeField] private KeyCode closeTopPanelKey = KeyCode.Escape;

        private readonly Dictionary<string, UIPanel> panels = new Dictionary<string, UIPanel>();
        private readonly Dictionary<UIPanel, string> panelIds = new Dictionary<UIPanel, string>();
        private readonly List<UIPanel> openStack = new List<UIPanel>();
        private SettingsPanel settingsPanel;
        private RewardClaimPanel rewardClaimPanel;
        private FurniturePlacementPanel furniturePlacementPanel;
        private PetAppearancePanel petAppearancePanel;
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
            CreateRewardClaimPanel();
            CreateFurniturePlacementPanel();
            CreatePetAppearancePanel();
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

        public void ToggleRewardClaimPanel()
        {
            TogglePanel("rewards");
        }

        public void ToggleFurniturePlacementPanel()
        {
            if (!panels.ContainsKey("furniture-placement")) CreateFurniturePlacementPanel();
            if (!panels.ContainsKey("furniture-placement")) return;
            TogglePanel("furniture-placement");
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
            settingsPanel.Initialize(
                petRoot,
                windowController,
                minPetScale,
                maxPetScale,
                initialScale,
                defaultPetScale,
                ToggleSettingsPanel);
            RegisterPanel("settings", settingsPanel);
        }

        private void CreateRewardClaimPanel()
        {
            if (rewardClaimPanelPrefab == null)
                rewardClaimPanelPrefab = Resources.Load<RewardClaimPanel>("UI/RewardClaimPanel");
            if (rewardClaimPanelPrefab == null)
            {
                Debug.LogWarning("UIManager needs a reward claim panel prefab.");
                return;
            }
            if (rewardClaimPanel != null) return;
            rewardClaimPanel = Instantiate(rewardClaimPanelPrefab, panelRoot, false);
            RegisterPanel("rewards", rewardClaimPanel);
            rewardClaimPanel.Initialize(FindObjectOfType<FurnitureRewardClaimService>(), () => ClosePanel("rewards"), ToggleFurniturePlacementPanel);
        }

        private void CreateFurniturePlacementPanel()
        {
            if (furniturePlacementPanelPrefab == null)
                furniturePlacementPanelPrefab = Resources.Load<FurniturePlacementPanel>("UI/FurniturePlacementPanel");
            if (furniturePlacementPanelPrefab == null)
            {
                Debug.LogWarning("UIManager needs a furniture placement panel prefab.");
                return;
            }
            if (furniturePlacementPanel != null)
            {
                if (!panels.ContainsKey("furniture-placement")) RegisterPanel("furniture-placement", furniturePlacementPanel);
                return;
            }
            furniturePlacementPanel = Instantiate(furniturePlacementPanelPrefab, panelRoot, false);
            RegisterPanel("furniture-placement", furniturePlacementPanel);
            furniturePlacementPanel.Initialize(() => ClosePanel("furniture-placement"));
        }

        private void CreatePetAppearancePanel()
        {
            if (petAppearancePanelPrefab == null)
                petAppearancePanelPrefab = Resources.Load<PetAppearancePanel>("UI/PetAppearancePanel");
            if (petAppearancePanelPrefab == null)
            {
                Debug.LogWarning("UIManager needs a pet appearance panel prefab.");
                return;
            }
            petAppearancePanel = Instantiate(petAppearancePanelPrefab, panelRoot, false);
            RegisterPanel("pet-appearance", petAppearancePanel);
            petAppearancePanel.Initialize(petRoot != null ? petRoot.GetComponent<PetAppearanceController>() : null,
                () => ClosePanel("pet-appearance"));
            if (SaveManager.Data == null || SaveManager.Data.appearance == null || !SaveManager.Data.appearance.hasChosenPet)
                OpenPanel("pet-appearance");
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
