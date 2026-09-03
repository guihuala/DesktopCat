using System.Collections.Generic;
using DesktopPet.Config;
using DesktopPet.Save;
using UnityEngine;

namespace DesktopPet.Pet.Presentation
{
    public sealed class PetAppearanceController : MonoBehaviour
    {
        [SerializeField] private PetAppearanceConfig config;
        private readonly List<Renderer> sourceRenderers = new List<Renderer>();
        public int FurStyle { get; private set; }

        public void Initialize()
        {
            if (config == null) config = Resources.Load<PetAppearanceConfig>("Config/PetAppearanceConfig");
            sourceRenderers.Clear();
            foreach (var renderer in GetComponentsInChildren<Renderer>(true)) sourceRenderers.Add(renderer);
            foreach (var renderer in sourceRenderers) renderer.enabled = false;
            var saved = SaveManager.Data != null ? SaveManager.Data.appearance : null;
            FurStyle = saved != null ? Mathf.Clamp(saved.furStyle, 0, 1) : 0;
            if (saved != null && saved.hasChosenPet) Apply(FurStyle);
        }

        public void Preview(int furStyle) => Apply(furStyle);

        public void Confirm(int furStyle)
        {
            Apply(furStyle);
            if (SaveManager.Data == null) return;
            if (SaveManager.Data.appearance == null) SaveManager.Data.appearance = new AppearanceSettingsData();
            SaveManager.Data.appearance.hasChosenPet = true;
            SaveManager.Data.appearance.furStyle = FurStyle;
            SaveManager.MarkDataDirty();
        }

        private void Apply(int furStyle)
        {
            FurStyle = Mathf.Clamp(furStyle, 0, 1);
            SetVisible(true);
            ApplyFurTint();
        }

        private void SetVisible(bool visible)
        {
            RefreshRenderers();
            foreach (var renderer in sourceRenderers)
            {
                if (renderer == null) continue;
                renderer.forceRenderingOff = false;
                renderer.enabled = visible;
            }
        }

        private void ApplyFurTint()
        {
            if (config == null) return;
            var tint = FurStyle == 0 ? config.warmFur : config.coolFur;
            foreach (var renderer in sourceRenderers)
            {
                if (renderer == null) continue;
                var materials = renderer.materials;
                foreach (var material in materials)
                {
                    if (material == null) continue;
                    if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);
                    if (material.HasProperty("_Color")) material.color = tint;
                }
            }
        }

        private void RefreshRenderers()
        {
            sourceRenderers.Clear();
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
                sourceRenderers.Add(renderer);
        }

    }
}
