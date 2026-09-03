using System;
using System.Collections.Generic;
using UnityEngine;

namespace DesktopPet.Furniture
{
    public readonly struct FurnitureDropSimulation
    {
        public readonly int Total;
        public readonly int Common;
        public readonly int Rare;
        public readonly int Collectible;

        public FurnitureDropSimulation(int total, int common, int rare, int collectible)
        {
            Total = total;
            Common = common;
            Rare = rare;
            Collectible = collectible;
        }

        public override string ToString()
        {
            if (Total <= 0) return "没有可抽取的家具";
            return $"普通 {Common * 100f / Total:0.0}% · 稀有 {Rare * 100f / Total:0.0}% · 珍藏 {Collectible * 100f / Total:0.0}%";
        }
    }

    public sealed class FurnitureDropService : MonoBehaviour
    {
        [SerializeField] private FurnitureCatalog catalog;
        [SerializeField] private FurnitureDropConfig config;
        private System.Random random;

        public bool IsReady => catalog != null && catalog.Items.Count > 0 && config != null;

        private void Awake()
        {
            if (catalog == null) catalog = Resources.Load<FurnitureCatalog>("Config/FurnitureCatalog");
            if (config == null) config = Resources.Load<FurnitureDropConfig>("Config/FurnitureDropConfig");
            random = new System.Random();
            if (!IsReady) Debug.LogError("家具抽取服务缺少 FurnitureCatalog 或 FurnitureDropConfig。", this);
        }

        public void SetDebugSeed(int seed) => random = new System.Random(seed);

        public FurnitureDefinition DrawOne()
        {
            return DrawOne(random);
        }

        public FurnitureDefinition DrawOne(System.Random source)
        {
            if (!IsReady || source == null) return null;
            var rarity = RollRarity(source);
            var pool = catalog.GetPool(rarity);
            if (pool.Count == 0)
            {
                pool = GetFallbackPool(rarity);
                Debug.LogWarning($"{rarity} 家具池为空，已降级到其他可用档位。", catalog);
            }
            return pool.Count > 0 ? pool[source.Next(pool.Count)] : null;
        }

        public FurnitureDropSimulation Simulate(int count, int seed)
        {
            var source = new System.Random(seed);
            var common = 0;
            var rare = 0;
            var collectible = 0;
            for (var i = 0; i < Mathf.Max(0, count); i++)
            {
                var item = DrawOne(source);
                if (item == null) continue;
                switch (item.rarity)
                {
                    case FurnitureRarity.Rare: rare++; break;
                    case FurnitureRarity.Collectible: collectible++; break;
                    default: common++; break;
                }
            }
            return new FurnitureDropSimulation(common + rare + collectible, common, rare, collectible);
        }

        private FurnitureRarity RollRarity(System.Random source)
        {
            var total = config.TotalWeight;
            if (total <= 0f) return FurnitureRarity.Common;
            var roll = source.NextDouble() * total;
            if (roll < config.commonWeight) return FurnitureRarity.Common;
            if (roll < config.commonWeight + config.rareWeight) return FurnitureRarity.Rare;
            return FurnitureRarity.Collectible;
        }

        private List<FurnitureDefinition> GetFallbackPool(FurnitureRarity requested)
        {
            var order = requested == FurnitureRarity.Collectible
                ? new[] { FurnitureRarity.Rare, FurnitureRarity.Common }
                : requested == FurnitureRarity.Rare
                    ? new[] { FurnitureRarity.Common, FurnitureRarity.Collectible }
                    : new[] { FurnitureRarity.Rare, FurnitureRarity.Collectible };
            foreach (var rarity in order)
            {
                var pool = catalog.GetPool(rarity);
                if (pool.Count > 0) return pool;
            }
            return new List<FurnitureDefinition>();
        }
    }
}
