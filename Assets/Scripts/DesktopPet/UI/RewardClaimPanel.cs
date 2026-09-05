using System;
using System.Text;
using DesktopPet.Events;
using DesktopPet.Furniture;
using DesktopPet.Rewards;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public sealed class RewardClaimPanel : UIPanel
    {
        [SerializeField] private Text pendingText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text resultText;
        [SerializeField] private Button claimButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button placementButton;
        private FurnitureRewardClaimService claimService;
        private Action closeRequested;
        private Action placementRequested;
        private float nextRefresh;

        public void Initialize(FurnitureRewardClaimService service, Action onCloseRequested, Action onPlacementRequested)
        {
            claimService = service;
            closeRequested = onCloseRequested;
            placementRequested = onPlacementRequested;
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(ClaimAll);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(RequestClose);
            if (placementButton != null)
            {
                placementButton.onClick.RemoveAllListeners();
                placementButton.onClick.AddListener(RequestPlacement);
            }
            ApplyFont();
            Refresh();
        }

        public override void Open()
        {
            base.Open();
            Refresh();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.5f;
            Refresh();
        }

        private void ClaimAll()
        {
            ResolveService();
            if (claimService == null) return;
            claimButton.interactable = false;
            var results = claimService.ClaimAll();
            if (results.Count == 0) resultText.text = "现在还没有可以领取的家具。";
            else
            {
                var builder = new StringBuilder("本次获得：\n");
                FurnitureDefinition bestFirstDiscovery = null;
                foreach (var result in results)
                {
                    builder.Append("• ").Append(result.Furniture.displayName)
                        .Append(" · ").Append(RarityName(result.Furniture.rarity));
                    if (result.FirstDiscovery) builder.Append("  新发现！");
                    if (result.FirstDiscovery && result.Furniture.rarity != FurnitureRarity.Common &&
                        (bestFirstDiscovery == null || (int)result.Furniture.rarity > (int)bestFirstDiscovery.rarity))
                        bestFirstDiscovery = result.Furniture;
                    builder.AppendLine();
                }
                resultText.text = builder.ToString();
                if (bestFirstDiscovery != null)
                {
                    var rarity = RarityName(bestFirstDiscovery.rarity);
                    GameEventBus.Publish(new PetFeedbackEvent(
                        $"首次发现{rarity}家具：{bestFirstDiscovery.displayName}！", true,
                        FeedbackPriority.Important, 4f));
                }
                else
                {
                    GameEventBus.Publish(new PetFeedbackEvent(
                        $"获得了 {results.Count} 件家具！", true, FeedbackPriority.Normal, 2.5f));
                }
            }
            Refresh();
        }

        private void Refresh()
        {
            ResolveService();
            if (claimService == null)
            {
                pendingText.text = "家具服务准备中";
                claimButton.interactable = false;
                return;
            }
            var pending = claimService.PendingCount;
            pendingText.text = $"待领取家具  {pending} 件";
            progressText.text = pending > 0
                ? "家具已经准备好啦"
                : $"下一件还需 {claimService.SecondsUntilNext / 60d:0.0} 分钟";
            claimButton.interactable = pending > 0;
        }

        private void ResolveService()
        {
            if (claimService == null) claimService = FindObjectOfType<FurnitureRewardClaimService>();
        }

        private void RequestClose()
        {
            if (closeRequested != null) closeRequested.Invoke();
            else Close();
        }

        private void RequestPlacement()
        {
            RequestClose();
            placementRequested?.Invoke();
        }

        private void ApplyFont()
        {
            var font = Font.CreateDynamicFontFromOSFont(
                new[] { "PingFang SC", "Microsoft YaHei", "Noto Sans CJK SC", "Arial" }, 18);
            foreach (var text in GetComponentsInChildren<Text>(true)) text.font = font;
        }

        private static string RarityName(FurnitureRarity rarity)
        {
            switch (rarity)
            {
                case FurnitureRarity.Rare: return "稀有";
                case FurnitureRarity.Collectible: return "珍藏";
                default: return "普通";
            }
        }
    }
}
