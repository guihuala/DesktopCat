using System;
using System.Collections.Generic;
using UnityEngine;

namespace DesktopPet.Furniture
{
    public enum FurnitureRarity { Common, Rare, Collectible }
    public enum FurnitureAnchorType { CatBed, FoodBowl, Rug, Desktop, Toy }

    [Serializable]
    public sealed class FurnitureDefinition
    {
        public string id;
        public string displayName;
        public FurnitureRarity rarity;
        public FurnitureAnchorType anchorType;
        public GameObject prefab;
        public Sprite icon;
        [TextArea(2, 4)] public string description;
        public string[] tags;
    }

    [CreateAssetMenu(menuName = "Desktop Pet/Furniture Catalog", fileName = "FurnitureCatalog")]
    public sealed class FurnitureCatalog : ScriptableObject
    {
        [SerializeField] private List<FurnitureDefinition> items = new List<FurnitureDefinition>();
        public IReadOnlyList<FurnitureDefinition> Items => items;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ValidateRuntimeCatalog()
        {
            var catalog = Resources.Load<FurnitureCatalog>("Config/FurnitureCatalog");
            if (catalog == null)
            {
                Debug.LogError("未找到家具配置 Resources/Config/FurnitureCatalog。");
                return;
            }
            if (!catalog.Validate(out var report)) Debug.LogError($"家具配置校验失败：{report}", catalog);
            var missingVisuals = 0;
            foreach (var item in catalog.items)
                if (item != null && item.prefab == null) missingVisuals++;
            if (missingVisuals > 0)
                Debug.LogWarning($"家具配置已载入，其中 {missingVisuals} 件仍在等待 Prefab 美术资源。", catalog);
        }

        public bool TryGet(string id, out FurnitureDefinition definition)
        {
            definition = items.Find(item => item != null && item.id == id);
            return definition != null;
        }

        public List<FurnitureDefinition> GetPool(FurnitureRarity rarity)
        {
            return items.FindAll(item => item != null && item.rarity == rarity);
        }

        public bool Validate(out string report)
        {
            var errors = new List<string>();
            var ids = new HashSet<string>();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) { errors.Add($"第 {i + 1} 项为空"); continue; }
                if (string.IsNullOrWhiteSpace(item.id)) errors.Add($"第 {i + 1} 项缺少 ID");
                else if (!ids.Add(item.id)) errors.Add($"家具 ID 重复：{item.id}");
                if (string.IsNullOrWhiteSpace(item.displayName)) errors.Add($"{item.id} 缺少显示名称");
            }
            report = string.Join("；", errors);
            return errors.Count == 0;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Validate(out var report)) Debug.LogError($"家具配置校验失败：{report}", this);
        }
#endif
    }
}
