using System;
using System.Collections.Generic;
using UnityEngine;

namespace DesktopPet.Furniture
{
    public readonly struct FurnitureExchangeResult
    {
        public readonly FurnitureDefinition Source;
        public readonly FurnitureDefinition Reward;
        public readonly int Cost;
        public readonly bool FirstDiscovery;
        public readonly bool Upgraded;

        public FurnitureExchangeResult(FurnitureDefinition source, FurnitureDefinition reward, int cost, bool firstDiscovery, bool upgraded)
        {
            Source = source; Reward = reward; Cost = cost; FirstDiscovery = firstDiscovery; Upgraded = upgraded;
        }
    }

    public sealed class FurnitureExchangeService : MonoBehaviour
    {
        [SerializeField] private FurnitureCatalog catalog;
        [SerializeField] private FurnitureExchangeConfig config;
        private FurnitureInventory inventory;
        private System.Random random;

        public int RequiredCopies => config != null ? Mathf.Max(2, config.requiredCopies) : 3;

        private void Awake()
        {
            if (catalog == null) catalog = Resources.Load<FurnitureCatalog>("Config/FurnitureCatalog");
            if (config == null) config = Resources.Load<FurnitureExchangeConfig>("Config/FurnitureExchangeConfig");
            inventory = FindObjectOfType<FurnitureInventory>();
            random = new System.Random();
        }

        public List<FurnitureDefinition> GetEligibleFurniture()
        {
            ResolveInventory();
            var result = new List<FurnitureDefinition>();
            if (catalog == null || inventory == null) return result;
            foreach (var item in catalog.Items)
                if (item != null && inventory.Get(item.id).AvailableCount >= RequiredCopies) result.Add(item);
            return result;
        }

        public bool TryExchange(string sourceId, out FurnitureExchangeResult result, out string error)
        {
            result = default;
            error = string.Empty;
            ResolveInventory();
            if (catalog == null || config == null || inventory == null) { error = "兑换服务还没有准备好"; return false; }
            if (!catalog.TryGet(sourceId, out var source)) { error = "找不到要兑换的家具"; return false; }
            if (inventory.Get(sourceId).AvailableCount < RequiredCopies) { error = $"需要 {RequiredCopies} 个可用的同款家具"; return false; }

            var targetRarity = RollTargetRarity(source.rarity);
            var pool = catalog.GetPool(targetRarity);
            var candidates = pool.FindAll(item => item != null && (item.id != sourceId || pool.Count == 1));
            if (candidates.Count == 0) candidates = pool;
            if (candidates.Count == 0) { error = "目标档位没有可兑换的家具"; return false; }
            var reward = candidates[random.Next(candidates.Count)];
            if (!inventory.TryExchange(sourceId, RequiredCopies, reward.id, out var firstDiscovery))
            { error = "库存数量发生变化，请重试"; return false; }

            result = new FurnitureExchangeResult(source, reward, RequiredCopies, firstDiscovery, (int)targetRarity > (int)source.rarity);
            return true;
        }

        private FurnitureRarity RollTargetRarity(FurnitureRarity source)
        {
            if (source == FurnitureRarity.Common && random.NextDouble() < config.commonUpgradeChance) return FurnitureRarity.Rare;
            if (source == FurnitureRarity.Rare && random.NextDouble() < config.rareUpgradeChance) return FurnitureRarity.Collectible;
            return source;
        }

        private void ResolveInventory()
        {
            if (inventory == null) inventory = FindObjectOfType<FurnitureInventory>();
        }
    }
}
