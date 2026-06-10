using UnityEngine;

namespace DesktopPet
{
    public class DesktopPetPanelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private Transform petRoot;
        [SerializeField] private string petRootName = "Monkey";

        [Header("Pet Size")]
        [SerializeField] private float minPetScale = 0.25f;
        [SerializeField] private float maxPetScale = 1.25f;
        [SerializeField] private float defaultPetScale = 0.55f;
        [SerializeField] private bool applyDefaultScaleOnStart = true;

        [Header("Panel")]
        [SerializeField] private DesktopPetSettingsPanelView settingsPanelPrefab;

        private DesktopPetSettingsPanelView settingsPanel;

        private void Awake()
        {
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
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
                Debug.LogWarning("DesktopPetPanelManager needs a Canvas.");
                return;
            }

            if (settingsPanelPrefab == null)
            {
                Debug.LogWarning("DesktopPetPanelManager needs a settings panel prefab.");
                return;
            }

            var initialScale = petRoot != null ? petRoot.localScale.x : defaultPetScale;
            settingsPanel = Instantiate(settingsPanelPrefab, canvas.transform, false);
            settingsPanel.Initialize(petRoot, minPetScale, maxPetScale, initialScale, defaultPetScale);
            settingsPanel.Close();
        }

        public void ToggleSettingsPanel()
        {
            settingsPanel?.Toggle();
        }
    }
}
