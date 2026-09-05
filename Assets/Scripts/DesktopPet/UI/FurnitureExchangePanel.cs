using System;
using System.Collections.Generic;
using DesktopPet.Events;
using DesktopPet.Furniture;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public sealed class FurnitureExchangePanel : UIPanel
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button exchangeButton;
        [SerializeField] private Button debugGrantButton;
        [SerializeField] private Text furnitureNameText;
        [SerializeField] private Text countText;
        [SerializeField] private Text rarityText;
        [SerializeField] private Text ruleText;
        [SerializeField] private Text resultText;
        [SerializeField] private Image previewImage;

        private readonly List<FurnitureDefinition> candidates = new List<FurnitureDefinition>();
        private FurnitureCatalog catalog;
        private FurnitureInventory inventory;
        private FurnitureExchangeService exchangeService;
        private Action closeRequested;
        private Action backRequested;
        private int selectedIndex;

        public void Initialize(Action onClose, Action onBack)
        {
            closeRequested = onClose;
            backRequested = onBack;
            catalog = Resources.Load<FurnitureCatalog>("Config/FurnitureCatalog");
            ResolveServices();
            Bind(closeButton, RequestClose);
            Bind(backButton, RequestBack);
            Bind(previousButton, () => Cycle(-1));
            Bind(nextButton, () => Cycle(1));
            Bind(exchangeButton, ExchangeSelected);
            Bind(debugGrantButton, GrantDebugFurniture);
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            if (debugGrantButton != null) debugGrantButton.gameObject.SetActive(false);
#endif
            ApplyFont();
        }

        public override void Open()
        {
            base.Open();
            resultText.text = string.Empty;
            RefreshCandidates();
        }

        private void RefreshCandidates()
        {
            ResolveServices();
            candidates.Clear();
            if (exchangeService != null) candidates.AddRange(exchangeService.GetEligibleFurniture());
            if (selectedIndex >= candidates.Count) selectedIndex = Mathf.Max(0, candidates.Count - 1);
            var canCycle = candidates.Count > 1;
            previousButton.interactable = canCycle;
            nextButton.interactable = canCycle;
            exchangeButton.interactable = candidates.Count > 0;
            ruleText.text = exchangeService != null
                ? $"消耗同款可用家具 ×{exchangeService.RequiredCopies}，随机获得 1 件同档或更高档家具"
                : "兑换服务准备中";
            if (candidates.Count == 0)
            {
                furnitureNameText.text = "暂时没有可兑换的家具";
                countText.text = "同一种家具至少需要 3 个可用数量";
                rarityText.text = "已摆放的家具不会被消耗";
                previewImage.color = new Color(0.25f, 0.28f, 0.34f, 1f);
                return;
            }
            var item = candidates[selectedIndex];
            var entry = inventory.Get(item.id);
            furnitureNameText.text = item.displayName;
            countText.text = $"拥有 {entry.TotalOwned}　已摆 {entry.PlacedCount}　可用 {entry.AvailableCount}";
            rarityText.text = $"当前档位：{RarityName(item.rarity)}";
            previewImage.color = PreviewColor(item.id, item.rarity);
        }

        private void ExchangeSelected()
        {
            if (candidates.Count == 0 || exchangeService == null) return;
            var source = candidates[selectedIndex];
            if (!exchangeService.TryExchange(source.id, out var exchange, out var error))
            {
                resultText.text = error;
                RefreshCandidates();
                return;
            }
            var upgrade = exchange.Upgraded ? "，档位提升！" : string.Empty;
            var discovery = exchange.FirstDiscovery ? "，首次发现！" : string.Empty;
            resultText.text = $"获得 {exchange.Reward.displayName}（{RarityName(exchange.Reward.rarity)}）{upgrade}{discovery}";
            GameEventBus.Publish(new PetFeedbackEvent(
                $"兑换获得：{exchange.Reward.displayName}！", true,
                exchange.Upgraded || exchange.FirstDiscovery ? FeedbackPriority.Important : FeedbackPriority.Normal,
                exchange.Upgraded ? 4f : 2.5f));
            RefreshCandidates();
        }

        private void Cycle(int direction)
        {
            if (candidates.Count == 0) return;
            selectedIndex = (selectedIndex + direction + candidates.Count) % candidates.Count;
            resultText.text = string.Empty;
            RefreshCandidates();
        }

        private void ResolveServices()
        {
            if (inventory == null) inventory = FindObjectOfType<FurnitureInventory>();
            if (exchangeService == null) exchangeService = FindObjectOfType<FurnitureExchangeService>();
        }

        private void GrantDebugFurniture()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ResolveServices();
            if (inventory != null) inventory.Add("common_soft_bed", exchangeService != null ? exchangeService.RequiredCopies : 3);
            resultText.text = "已补充 3 件柔软猫窝，仅用于测试兑换";
            RefreshCandidates();
#endif
        }

        private void RequestClose() { if (closeRequested != null) closeRequested(); else Close(); }
        private void RequestBack() { RequestClose(); backRequested?.Invoke(); }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick.RemoveAllListeners(); button.onClick.AddListener(action);
        }

        private void ApplyFont()
        {
            var font = Font.CreateDynamicFontFromOSFont(new[] { "PingFang SC", "Microsoft YaHei", "Noto Sans CJK SC", "Arial" }, 18);
            foreach (var text in GetComponentsInChildren<Text>(true)) text.font = font;
        }

        private static string RarityName(FurnitureRarity rarity) => rarity == FurnitureRarity.Collectible ? "珍藏" : rarity == FurnitureRarity.Rare ? "稀有" : "普通";
        private static Color PreviewColor(string id, FurnitureRarity rarity)
        {
            var hash = 17; foreach (var c in id) hash = hash * 31 + c;
            return Color.HSVToRGB(Mathf.Abs(hash % 997) / 997f, rarity == FurnitureRarity.Common ? 0.42f : 0.65f, rarity == FurnitureRarity.Collectible ? 0.96f : 0.84f);
        }
    }
}
