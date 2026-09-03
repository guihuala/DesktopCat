using System.Collections.Generic;
using DesktopPet.Save;
using UnityEngine;

namespace DesktopPet.Furniture
{
    public sealed class FurniturePlacementController : MonoBehaviour
    {
        [SerializeField] private FurnitureCatalog catalog;
        [SerializeField] private GameObject anchorSetPrefab;
        private FurnitureInventory inventory;
        private readonly Dictionary<FurnitureAnchorType, FurnitureAnchor> anchors = new Dictionary<FurnitureAnchorType, FurnitureAnchor>();
        private readonly Dictionary<FurnitureAnchorType, string> placedIds = new Dictionary<FurnitureAnchorType, string>();
        private readonly Dictionary<FurnitureAnchorType, GameObject> instances = new Dictionary<FurnitureAnchorType, GameObject>();

        private void Start()
        {
            inventory = FindObjectOfType<FurnitureInventory>();
            if (catalog == null) catalog = Resources.Load<FurnitureCatalog>("Config/FurnitureCatalog");
            if (anchorSetPrefab == null) anchorSetPrefab = Resources.Load<GameObject>("Furniture/FurnitureAnchorSet");
            if (FindObjectsOfType<FurnitureAnchor>().Length == 0 && anchorSetPrefab != null) Instantiate(anchorSetPrefab);
            foreach (var anchor in FindObjectsOfType<FurnitureAnchor>())
            {
                if (!anchors.ContainsKey(anchor.AnchorType)) anchors.Add(anchor.AnchorType, anchor);
                else Debug.LogError($"家具锚点重复：{anchor.AnchorType}", anchor);
            }
            LoadPlacements();
        }

        public string GetPlacedId(FurnitureAnchorType anchorType)
        {
            return placedIds.TryGetValue(anchorType, out var id) ? id : string.Empty;
        }

        public bool TryPlace(FurnitureAnchorType anchorType, string furnitureId, out string error)
        {
            error = string.Empty;
            if (!anchors.TryGetValue(anchorType, out var anchor)) { error = "房间中缺少对应的家具位置。"; return false; }
            if (catalog == null || !catalog.TryGet(furnitureId, out var definition)) { error = "家具配置不存在。"; return false; }
            if (definition.anchorType != anchorType) { error = "这件家具不能摆在这里。"; return false; }
            if (definition.prefab == null) { error = "这件家具还没有可用的模型。"; return false; }
            if (placedIds.TryGetValue(anchorType, out var currentId) && currentId == furnitureId) return true;
            if (inventory == null || !inventory.TryReserveForPlacement(furnitureId)) { error = "库存中没有可用的这件家具。"; return false; }
            if (!string.IsNullOrEmpty(currentId)) inventory.ReleasePlaced(currentId);
            ClearInstance(anchorType);
            var instance = InstantiateAtAnchor(definition.prefab, anchor);
            instance.name = definition.displayName;
            instances[anchorType] = instance;
            placedIds[anchorType] = furnitureId;
            Persist();
            return true;
        }

        public bool Remove(FurnitureAnchorType anchorType)
        {
            if (!placedIds.TryGetValue(anchorType, out var furnitureId)) return false;
            inventory?.ReleasePlaced(furnitureId);
            placedIds.Remove(anchorType);
            ClearInstance(anchorType);
            Persist();
            return true;
        }

        private void LoadPlacements()
        {
            placedIds.Clear();
            var save = SaveManager.Data != null ? SaveManager.Data.furniturePlacement : null;
            if (save != null && save.items != null)
            {
                foreach (var item in save.items)
                {
                    var anchorType = (FurnitureAnchorType)item.anchorType;
                    if (placedIds.ContainsKey(anchorType) || !anchors.TryGetValue(anchorType, out var anchor)) continue;
                    if (catalog == null || !catalog.TryGet(item.furnitureId, out var definition) || definition.anchorType != anchorType || definition.prefab == null) continue;
                    if (inventory == null || inventory.Get(item.furnitureId).TotalOwned <= 0) continue;
                    placedIds.Add(anchorType, item.furnitureId);
                    var instance = InstantiateAtAnchor(definition.prefab, anchor);
                    instance.name = definition.displayName;
                    instances[anchorType] = instance;
                }
            }
            var counts = new Dictionary<string, int>();
            foreach (var id in placedIds.Values) counts[id] = counts.TryGetValue(id, out var count) ? count + 1 : 1;
            inventory?.ReconcilePlacedCounts(counts);
            Persist();
        }

        private void ClearInstance(FurnitureAnchorType anchorType)
        {
            if (!instances.TryGetValue(anchorType, out var instance)) return;
            if (instance != null) Destroy(instance);
            instances.Remove(anchorType);
        }

        private static GameObject InstantiateAtAnchor(GameObject prefab, FurnitureAnchor anchor)
        {
            var instance = Instantiate(prefab, anchor.ContentRoot, false);
            instance.transform.localPosition = Vector3.zero;
            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return instance;
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                if (renderers[i] != null && renderers[i].enabled) bounds.Encapsulate(renderers[i].bounds);
            var position = instance.transform.position;
            position.y += anchor.ContentRoot.position.y - bounds.min.y;
            instance.transform.position = position;
            return instance;
        }

        private void Persist()
        {
            if (SaveManager.Data == null) return;
            var save = SaveManager.Data.furniturePlacement ?? (SaveManager.Data.furniturePlacement = new FurniturePlacementSaveData());
            save.items = new List<FurniturePlacementItemSaveData>(placedIds.Count);
            foreach (var pair in placedIds)
                save.items.Add(new FurniturePlacementItemSaveData { anchorType = (int)pair.Key, furnitureId = pair.Value });
            SaveManager.MarkDataDirty();
        }
    }
}
