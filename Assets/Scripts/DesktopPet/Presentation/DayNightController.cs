using System;
using UnityEngine;
using DesktopPet.Events;
using DesktopPet.Save;
using UnityEngine.Rendering;

namespace DesktopPet.Presentation
{
    public enum DayNightMode { FollowSystem, Day, Night }

    public sealed class DayNightController : MonoBehaviour
    {
        [SerializeField] private DayNightMode mode = DayNightMode.FollowSystem;
        [SerializeField] private Color dayAmbient = new Color(0.22f, 0.24f, 0.27f);
        [SerializeField] private Color nightAmbient = new Color(0.045f, 0.06f, 0.1f);
        [SerializeField] private Color dayBackground = new Color(0.5f, 0.65f, 0.8f, 0f);
        [SerializeField] private Color nightBackground = new Color(0.04f, 0.06f, 0.12f, 0f);
        private int lastHour = -1;
        [SerializeField] private KeyCode cycleModeKey = KeyCode.F6;

        public DayNightMode Mode => mode;
        private void Start()
        {
            if (SaveManager.Data != null && SaveManager.Data.appearance != null)
                mode = (DayNightMode)Mathf.Clamp(SaveManager.Data.appearance.dayNightMode, 0, 2);
            Apply();
        }
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(cycleModeKey))
            {
                CycleMode();
                GameEventBus.Publish(new PetFeedbackEvent($"昼夜模式：{mode}", true));
            }
            if (mode == DayNightMode.FollowSystem && DateTime.Now.Hour != lastHour) Apply();
        }
        public void SetMode(DayNightMode value) { mode = value; Apply(); GameEventBus.Publish(new DayNightModeChangedEvent((int)mode)); }
        public void CycleMode() => SetMode((DayNightMode)(((int)mode + 1) % 3));

        private void Apply()
        {
            lastHour = DateTime.Now.Hour;
            var night = mode == DayNightMode.Night || mode == DayNightMode.FollowSystem && (lastHour < 6 || lastHour >= 19);
            ConfigureMainLight();
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = night ? nightAmbient : dayAmbient;
            if (Camera.main != null) Camera.main.backgroundColor = night ? nightBackground : dayBackground;
        }

        private static void ConfigureMainLight()
        {
            var mainLight = RenderSettings.sun;
            if (mainLight == null || mainLight.type != LightType.Directional)
            {
                var lights = FindObjectsOfType<Light>();
                for (var i = 0; i < lights.Length; i++)
                {
                    if (lights[i].enabled && lights[i].type == LightType.Directional)
                    {
                        mainLight = lights[i];
                        break;
                    }
                }
            }

            if (mainLight == null)
            {
                Debug.LogWarning("No directional light was found; realtime shadows cannot be rendered.");
                return;
            }

            RenderSettings.sun = mainLight;
            mainLight.shadows = LightShadows.Soft;
            mainLight.shadowStrength = 0.82f;
            mainLight.shadowBias = 0.02f;
            mainLight.shadowNormalBias = 0.25f;
        }
    }
}
