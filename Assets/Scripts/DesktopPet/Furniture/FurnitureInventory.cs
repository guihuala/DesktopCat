using System;
using System.Collections.Generic;
using DesktopPet.Events;
using DesktopPet.Save;
using UnityEngine;

namespace DesktopPet.Furniture
{
    public readonly struct FurnitureInventoryEntry
    {
        public readonly string FurnitureId;
        public readonly int TotalOwned;
        public readonly int PlacedCount;
        public int AvailableCount => Mathf.Max(0, TotalOwned - PlacedCount);

        public FurnitureInventoryEntry(string furnitureId, int totalOwned, int placedCount)
        {
            FurnitureId = furnitureId;
            TotalOwned = totalOwned;
            PlacedCount = placedCount;
        }
    }

    [DefaultExecutionOrder(-40)]
    public sealed class FurnitureInventory : MonoBehaviour
    {
        private sealed class MutableEntry
        {
            public int TotalOwned;
            public int PlacedCount;
        }

        [SerializeField] private FurnitureCatalog catalog;
        private readonly Dictionary<string, MutableEntry> entries = new Dictionary<string, MutableEntry>();
        private readonly HashSet<string> discoveredIds = new HashSet<string>();

        public int DiscoveredCount => discoveredIds.Count;

        private void Awake()
        {
            if (catalog == null) catalog = Resources.Load<FurnitureCatalog>("Config/FurnitureCatalog");
            LoadAndRepair();
        }

        public FurnitureInventoryEntry Get(string furnitureId)
        {
            return entries.TryGetValue(furnitureId, out var entry)
                ? new FurnitureInventoryEntry(furnitureId, entry.TotalOwned, entry.PlacedCount)
                : new FurnitureInventoryEntry(furnitureId, 0, 0);
        }

        public bool IsDiscovered(string furnitureId) => discoveredIds.Contains(furnitureId);

        public List<FurnitureInventoryEntry> GetAllOwned()
        {
            var result = new List<FurnitureInventoryEntry>(entries.Count);
            foreach (var pair in entries)
                if (pair.Value.TotalOwned > 0)
                    result.Add(new FurnitureInventoryEntry(pair.Key, pair.Value.TotalOwned, pair.Value.PlacedCount));
            result.Sort((left, right) => string.CompareOrdinal(left.FurnitureId, right.FurnitureId));
            return result;
        }

        public bool Add(string furnitureId, int amount = 1)
        {
            if (amount <= 0 || !IsKnownFurniture(furnitureId)) return false;
            if (!entries.TryGetValue(furnitureId, out var entry))
            {
                entry = new MutableEntry();
                entries.Add(furnitureId, entry);
            }
            var firstDiscovery = discoveredIds.Add(furnitureId);
            entry.TotalOwned = Mathf.Max(0, entry.TotalOwned + amount);
            Persist();
            Publish(furnitureId, entry, firstDiscovery);
            return true;
        }

        public bool TryReserveForPlacement(string furnitureId)
        {
            if (!entries.TryGetValue(furnitureId, out var entry) || entry.TotalOwned - entry.PlacedCount <= 0) return false;
            entry.PlacedCount++;
            Persist();
            Publish(furnitureId, entry, false);
            return true;
        }

        public bool ReleasePlaced(string furnitureId)
        {
            if (!entries.TryGetValue(furnitureId, out var entry) || entry.PlacedCount <= 0) return false;
            entry.PlacedCount--;
            Persist();
            Publish(furnitureId, entry, false);
            return true;
        }

        private void LoadAndRepair()
        {
            entries.Clear();
            discoveredIds.Clear();
            var save = SaveManager.Data != null ? SaveManager.Data.furnitureInventory : null;
            if (save == null) return;
            var repaired = false;
            if (save.discoveredIds != null)
            {
                foreach (var id in save.discoveredIds)
                {
                    if (IsKnownFurniture(id)) discoveredIds.Add(id);
                    else repaired = true;
                }
            }
            if (save.items != null)
            {
                foreach (var savedItem in save.items)
                {
                    if (savedItem == null || !IsKnownFurniture(savedItem.furnitureId)) { repaired = true; continue; }
                    if (!entries.TryGetValue(savedItem.furnitureId, out var entry))
                    {
                        entry = new MutableEntry();
                        entries.Add(savedItem.furnitureId, entry);
                    }
                    else repaired = true;
                    var total = Mathf.Max(0, savedItem.totalOwned);
                    var placed = Mathf.Clamp(savedItem.placedCount, 0, total);
                    if (total != savedItem.totalOwned || placed != savedItem.placedCount) repaired = true;
                    entry.TotalOwned += total;
                    entry.PlacedCount = Mathf.Clamp(entry.PlacedCount + placed, 0, entry.TotalOwned);
                    if (total > 0 && discoveredIds.Add(savedItem.furnitureId)) repaired = true;
                }
            }
            if (repaired)
            {
                Debug.LogWarning("家具库存存档包含重复、未知或非法数据，已安全修复。", this);
                Persist();
            }
        }

        private bool IsKnownFurniture(string furnitureId)
        {
            return !string.IsNullOrWhiteSpace(furnitureId) && catalog != null && catalog.TryGet(furnitureId, out _);
        }

        private void Persist()
        {
            if (SaveManager.Data == null) return;
            var save = SaveManager.Data.furnitureInventory ?? (SaveManager.Data.furnitureInventory = new FurnitureInventorySaveData());
            save.items = new List<FurnitureItemSaveData>(entries.Count);
            foreach (var pair in entries)
            {
                if (pair.Value.TotalOwned <= 0) continue;
                save.items.Add(new FurnitureItemSaveData
                {
                    furnitureId = pair.Key,
                    totalOwned = pair.Value.TotalOwned,
                    placedCount = pair.Value.PlacedCount
                });
            }
            save.discoveredIds = new List<string>(discoveredIds);
            SaveManager.MarkDataDirty();
        }

        private static void Publish(string furnitureId, MutableEntry entry, bool firstDiscovery)
        {
            GameEventBus.Publish(new FurnitureInventoryChangedEvent(
                furnitureId, entry.TotalOwned, entry.PlacedCount, firstDiscovery));
        }

#if UNITY_EDITOR
        [ContextMenu("调试/添加一个柔软猫窝")]
        private void DebugAddSample()
        {
            Add("common_soft_bed");
            DebugLogSample();
        }

        [ContextMenu("调试/摆放一个柔软猫窝")]
        private void DebugPlaceSample()
        {
            Debug.Log(TryReserveForPlacement("common_soft_bed") ? "已占用一个柔软猫窝用于摆放。" : "没有可用的柔软猫窝。", this);
            DebugLogSample();
        }

        [ContextMenu("调试/收回一个柔软猫窝")]
        private void DebugReleaseSample()
        {
            Debug.Log(ReleasePlaced("common_soft_bed") ? "已将一个柔软猫窝放回可用库存。" : "当前没有已摆放的柔软猫窝。", this);
            DebugLogSample();
        }

        [ContextMenu("调试/查看柔软猫窝库存")]
        private void DebugLogSample()
        {
            var entry = Get("common_soft_bed");
            Debug.Log($"柔软猫窝：总数 {entry.TotalOwned}，已摆放 {entry.PlacedCount}，可用 {entry.AvailableCount}，图鉴 {(IsDiscovered(entry.FurnitureId) ? "已解锁" : "未解锁")}。", this);
        }
#endif
    }
}
