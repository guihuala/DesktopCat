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
        private readonly List<Renderer> previewRenderers = new List<Renderer>();
        private GameObject previewInstance;
        public int FurStyle { get; private set; }

        public void Initialize()
        {
            if (config == null) config = Resources.Load<PetAppearanceConfig>("Config/PetAppearanceConfig");
            sourceRenderers.Clear();
            foreach (var renderer in GetComponentsInChildren<Renderer>(true)) sourceRenderers.Add(renderer);
            var saved = SaveManager.Data != null ? SaveManager.Data.appearance : null;
            FurStyle = saved != null ? Mathf.Clamp(saved.furStyle, 0, 1) : 0;
            if (saved != null && saved.hasChosenPet)
            {
                SetSourceVisible(true);
                ApplyFurTint(sourceRenderers);
            }
            else
            {
                SetSourceVisible(false);
                CreatePreview();
                ApplyFurTint(previewRenderers);
            }
        }

        public void Preview(int furStyle)
        {
            FurStyle = Mathf.Clamp(furStyle, 0, 1);
            if (previewInstance == null) CreatePreview();
            ApplyFurTint(previewRenderers);
        }

        public void Confirm(int furStyle)
        {
            FurStyle = Mathf.Clamp(furStyle, 0, 1);
            if (SaveManager.Data == null) return;
            if (SaveManager.Data.appearance == null) SaveManager.Data.appearance = new AppearanceSettingsData();
            SaveManager.Data.appearance.hasChosenPet = true;
            SaveManager.Data.appearance.furStyle = FurStyle;
            SaveManager.MarkDataDirty();
            DestroyPreview();
            SetSourceVisible(true);
            ApplyFurTint(sourceRenderers);
        }

        private void CreatePreview()
        {
            var prefab = Resources.Load<GameObject>("Pet/CatSelectionPreview");
            if (prefab == null)
            {
                Debug.LogError("缺少选猫预览 Prefab，请退出播放模式等待 Unity 自动生成。", this);
                return;
            }
            previewInstance = Instantiate(prefab, transform, false);
            previewInstance.name = "CatSelectionPreview";
            previewInstance.transform.SetAsLastSibling();
            previewRenderers.Clear();
            previewRenderers.AddRange(previewInstance.GetComponentsInChildren<Renderer>(true));
            foreach (var renderer in previewRenderers)
            {
                renderer.forceRenderingOff = false;
                renderer.enabled = true;
            }
        }

        private void DestroyPreview()
        {
            previewRenderers.Clear();
            if (previewInstance == null) return;
            previewInstance.SetActive(false);
            Destroy(previewInstance);
            previewInstance = null;
        }

        private void SetSourceVisible(bool visible)
        {
            foreach (var renderer in sourceRenderers)
            {
                if (renderer == null) continue;
                renderer.forceRenderingOff = false;
                renderer.enabled = visible;
            }
        }

        private void ApplyFurTint(IEnumerable<Renderer> renderers)
        {
            if (config == null) return;
            var tint = FurStyle == 0 ? config.warmFur : config.coolFur;
            foreach (var renderer in renderers)
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

        private void OnDestroy() => DestroyPreview();
    }
}
