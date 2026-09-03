using System;
using System.Collections.Generic;
using DesktopPet.Furniture;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public sealed class FurniturePlacementPanel : UIPanel
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private Button catBedButton;
        [SerializeField] private Button foodBowlButton;
        [SerializeField] private Button rugButton;
        [SerializeField] private Button desktopButton;
        [SerializeField] private Button toyButton;
        [SerializeField] private Text anchorTitleText;
        [SerializeField] private Text furnitureNameText;
        [SerializeField] private Text countText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text messageText;
        [SerializeField] private Image previewImage;
        [SerializeField] private Text previewGlyphText;
        [SerializeField] private Text placedStatusText;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button placeButton;
        [SerializeField] private Text placeButtonText;
        [SerializeField] private Button removeButton;

        private readonly List<FurnitureDefinition> candidates = new List<FurnitureDefinition>();
        private FurnitureCatalog catalog;
        private FurnitureInventory inventory;
        private FurniturePlacementController placement;
        private FurnitureAnchorType selectedAnchor = FurnitureAnchorType.CatBed;
        private int selectedIndex;
        private Action closeRequested;

        public void Initialize(Action onCloseRequested)
        {
            closeRequested = onCloseRequested;
            catalog = Resources.Load<FurnitureCatalog>("Config/FurnitureCatalog");
            inventory = FindObjectOfType<FurnitureInventory>();
            placement = FindObjectOfType<FurniturePlacementController>();
            Bind(closeButton, RequestClose);
            Bind(catBedButton, () => SelectAnchor(FurnitureAnchorType.CatBed));
            Bind(foodBowlButton, () => SelectAnchor(FurnitureAnchorType.FoodBowl));
            Bind(rugButton, () => SelectAnchor(FurnitureAnchorType.Rug));
            Bind(desktopButton, () => SelectAnchor(FurnitureAnchorType.Desktop));
            Bind(toyButton, () => SelectAnchor(FurnitureAnchorType.Toy));
            Bind(previousButton, () => Cycle(-1));
            Bind(nextButton, () => Cycle(1));
            Bind(placeButton, PlaceSelected);
            Bind(removeButton, RemoveCurrent);
            ApplyFont();
            SelectAnchor(selectedAnchor);
        }

        public override void Open()
        {
            base.Open();
            ResolveServices();
            RefreshCandidates();
        }

        private void SelectAnchor(FurnitureAnchorType anchorType)
        {
            selectedAnchor = anchorType;
            selectedIndex = 0;
            messageText.text = string.Empty;
            RefreshCandidates();
        }

        private void RefreshCandidates()
        {
            ResolveServices();
            candidates.Clear();
            if (catalog != null && inventory != null)
            {
                foreach (var item in catalog.Items)
                    if (item != null && item.anchorType == selectedAnchor && inventory.Get(item.id).TotalOwned > 0)
                        candidates.Add(item);
            }
            if (selectedIndex >= candidates.Count) selectedIndex = Mathf.Max(0, candidates.Count - 1);
            if (anchorTitleText != null) anchorTitleText.text = $"正在布置：{AnchorName(selectedAnchor)}";
            RefreshAnchorButtons();
            var placedId = placement != null ? placement.GetPlacedId(selectedAnchor) : string.Empty;
            if (removeButton != null) removeButton.interactable = !string.IsNullOrEmpty(placedId);
            if (previousButton != null) previousButton.interactable = candidates.Count > 1;
            if (nextButton != null) nextButton.interactable = candidates.Count > 1;
            if (candidates.Count == 0)
            {
                furnitureNameText.text = "还没有这类家具";
                countText.text = "领取家具后就能在这里摆放";
                descriptionText.text = string.Empty;
                placeButton.interactable = false;
                placeButtonText.text = "暂无可摆家具";
                if (previewImage != null) previewImage.color = new Color(0.25f, 0.28f, 0.34f, 1f);
                if (previewGlyphText != null) previewGlyphText.text = "暂无";
                if (placedStatusText != null) placedStatusText.text = string.IsNullOrEmpty(placedId) ? "当前位置：空" : "当前位置：已有家具";
                return;
            }
            var current = candidates[selectedIndex];
            var entry = inventory.Get(current.id);
            var isPlaced = placedId == current.id;
            furnitureNameText.text = $"{current.displayName} · {RarityName(current.rarity)}";
            countText.text = $"拥有 {entry.TotalOwned}　已摆 {entry.PlacedCount}　可用 {entry.AvailableCount}";
            descriptionText.text = current.description;
            if (previewImage != null) previewImage.color = PreviewColor(current.id, current.rarity);
            if (previewGlyphText != null) previewGlyphText.text = AnchorName(current.anchorType);
            if (placedStatusText != null) placedStatusText.text = isPlaced ? "● 当前正在使用" : string.IsNullOrEmpty(placedId) ? "○ 当前位置为空" : "○ 将替换当前家具";
            placeButton.interactable = isPlaced || entry.AvailableCount > 0;
            placeButtonText.text = isPlaced ? "已经摆在这里" : string.IsNullOrEmpty(placedId) ? "摆放" : "替换当前家具";
        }

        private void Cycle(int direction)
        {
            if (candidates.Count == 0) return;
            selectedIndex = (selectedIndex + direction + candidates.Count) % candidates.Count;
            RefreshCandidates();
        }

        private void PlaceSelected()
        {
            if (candidates.Count == 0 || placement == null) return;
            var item = candidates[selectedIndex];
            messageText.text = placement.TryPlace(selectedAnchor, item.id, out var error) ? $"已摆放：{item.displayName}" : error;
            RefreshCandidates();
        }

        private void RemoveCurrent()
        {
            messageText.text = placement != null && placement.Remove(selectedAnchor) ? "家具已收回库存" : "这里没有可以收回的家具";
            RefreshCandidates();
        }

        private void ResolveServices()
        {
            if (inventory == null) inventory = FindObjectOfType<FurnitureInventory>();
            if (placement == null) placement = FindObjectOfType<FurniturePlacementController>();
        }

        private void RequestClose()
        {
            if (closeRequested != null) closeRequested.Invoke();
            else Close();
        }

        private void RefreshAnchorButtons()
        {
            SetAnchorButtonState(catBedButton, FurnitureAnchorType.CatBed);
            SetAnchorButtonState(foodBowlButton, FurnitureAnchorType.FoodBowl);
            SetAnchorButtonState(rugButton, FurnitureAnchorType.Rug);
            SetAnchorButtonState(desktopButton, FurnitureAnchorType.Desktop);
            SetAnchorButtonState(toyButton, FurnitureAnchorType.Toy);
        }

        private void SetAnchorButtonState(Button button, FurnitureAnchorType type)
        {
            if (button != null && button.targetGraphic != null)
                button.targetGraphic.color = type == selectedAnchor
                    ? new Color(0.98f, 0.69f, 0.38f, 1f)
                    : new Color(0.19f, 0.22f, 0.28f, 1f);
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void ApplyFont()
        {
            var font = Font.CreateDynamicFontFromOSFont(new[] { "PingFang SC", "Microsoft YaHei", "Noto Sans CJK SC", "Arial" }, 18);
            foreach (var text in GetComponentsInChildren<Text>(true)) text.font = font;
        }

        private static string AnchorName(FurnitureAnchorType type)
        {
            switch (type)
            {
                case FurnitureAnchorType.FoodBowl: return "食盆";
                case FurnitureAnchorType.Rug: return "地毯";
                case FurnitureAnchorType.Desktop: return "桌面";
                case FurnitureAnchorType.Toy: return "玩具";
                default: return "猫窝";
            }
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

        private static Color PreviewColor(string id, FurnitureRarity rarity)
        {
            var hash = 17;
            foreach (var character in id) hash = hash * 31 + character;
            var hue = Mathf.Abs(hash % 997) / 997f;
            var saturation = rarity == FurnitureRarity.Common ? 0.42f : 0.62f;
            var value = rarity == FurnitureRarity.Collectible ? 0.95f : 0.82f;
            return Color.HSVToRGB(hue, saturation, value);
        }
    }
}
