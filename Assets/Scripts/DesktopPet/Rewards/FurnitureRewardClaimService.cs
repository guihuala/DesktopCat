using System.Collections.Generic;
using DesktopPet.Furniture;
using UnityEngine;

namespace DesktopPet.Rewards
{
    public readonly struct FurnitureClaimResult
    {
        public readonly FurnitureDefinition Furniture;
        public readonly bool FirstDiscovery;

        public FurnitureClaimResult(FurnitureDefinition furniture, bool firstDiscovery)
        {
            Furniture = furniture;
            FirstDiscovery = firstDiscovery;
        }
    }

    public sealed class FurnitureRewardClaimService : MonoBehaviour
    {
        private OnlineRewardService onlineReward;
        private FurnitureDropService dropService;
        private FurnitureInventory inventory;
        private bool claiming;

        public int PendingCount => onlineReward != null ? onlineReward.PendingRewards : 0;
        public double SecondsUntilNext => onlineReward != null ? onlineReward.SecondsUntilNext : 0d;

        private void Awake()
        {
            ResolveServices();
        }

        public List<FurnitureClaimResult> ClaimAll()
        {
            var results = new List<FurnitureClaimResult>();
            if (claiming) return results;
            claiming = true;
            try
            {
                ResolveServices();
                if (onlineReward == null || dropService == null || inventory == null) return results;
                var count = onlineReward.PendingRewards;
                for (var i = 0; i < count; i++)
                {
                    var furniture = dropService.DrawOne();
                    if (furniture == null) break;
                    var firstDiscovery = !inventory.IsDiscovered(furniture.id);
                    if (!inventory.Add(furniture.id)) break;
                    if (!onlineReward.TryConsumePendingReward())
                    {
                        Debug.LogError("家具已加入库存，但待领取数量扣减失败；已停止继续领取。", this);
                        break;
                    }
                    results.Add(new FurnitureClaimResult(furniture, firstDiscovery));
                }
                return results;
            }
            finally
            {
                claiming = false;
            }
        }

        private void ResolveServices()
        {
            if (onlineReward == null) onlineReward = FindObjectOfType<OnlineRewardService>();
            if (dropService == null) dropService = FindObjectOfType<FurnitureDropService>();
            if (inventory == null) inventory = FindObjectOfType<FurnitureInventory>();
        }
    }
}
