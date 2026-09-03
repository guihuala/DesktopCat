#if UNITY_EDITOR
using DesktopPet.Furniture;
using UnityEditor;
using UnityEngine;

namespace DesktopPet.Editor
{
    [InitializeOnLoad]
    public static class FurniturePrototypePrefabBuilder
    {
        private const string RootFolder = "Assets/Resources/Furniture";
        private const string PrototypeFolder = RootFolder + "/Prototype";
        private const string MaterialFolder = PrototypeFolder + "/Materials";
        private const string AnchorPrefabPath = RootFolder + "/FurnitureAnchorSet.prefab";

        static FurniturePrototypePrefabBuilder()
        {
            EditorApplication.delayCall += BuildMissingAssets;
        }

        [MenuItem("Desktop Pet/生成缺失的原型家具 Prefab")]
        public static void BuildMissingAssets()
        {
            EnsureFolder("Assets/Resources", "Furniture");
            EnsureFolder(RootFolder, "Prototype");
            EnsureFolder(PrototypeFolder, "Materials");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(AnchorPrefabPath) == null) BuildAnchorSet();
            BuildFurniturePlaceholders();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void BuildAnchorSet()
        {
            var root = new GameObject("FurnitureAnchorSet");
            AddAnchor(root.transform, "猫窝位置", FurnitureAnchorType.CatBed, new Vector3(-1.05f, 0.03f, 0.65f));
            AddAnchor(root.transform, "食盆位置", FurnitureAnchorType.FoodBowl, new Vector3(1.05f, 0.03f, 0.65f));
            AddAnchor(root.transform, "地毯位置", FurnitureAnchorType.Rug, new Vector3(0f, 0.025f, 0.15f));
            AddAnchor(root.transform, "桌面位置", FurnitureAnchorType.Desktop, new Vector3(0.72f, 0.18f, -0.58f));
            AddAnchor(root.transform, "玩具位置", FurnitureAnchorType.Toy, new Vector3(-0.62f, 0.04f, -0.18f));
            PrefabUtility.SaveAsPrefabAsset(root, AnchorPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void AddAnchor(Transform parent, string name, FurnitureAnchorType type, Vector3 position)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            var anchor = item.AddComponent<FurnitureAnchor>();
            var serialized = new SerializedObject(anchor);
            serialized.FindProperty("anchorType").enumValueIndex = (int)type;
            serialized.FindProperty("contentRoot").objectReferenceValue = item.transform;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildFurniturePlaceholders()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<FurnitureCatalog>("Assets/Resources/Config/FurnitureCatalog.asset");
            if (catalog == null) return;
            var serialized = new SerializedObject(catalog);
            var items = serialized.FindProperty("items");
            for (var i = 0; i < items.arraySize; i++)
            {
                var item = items.GetArrayElementAtIndex(i);
                var prefabProperty = item.FindPropertyRelative("prefab");
                var id = item.FindPropertyRelative("id").stringValue;
                var displayName = item.FindPropertyRelative("displayName").stringValue;
                var anchorType = (FurnitureAnchorType)item.FindPropertyRelative("anchorType").enumValueIndex;
                var path = $"{PrototypeFolder}/{id}.prefab";
                var assignedPath = AssetDatabase.GetAssetPath(prefabProperty.objectReferenceValue);
                if (!string.IsNullOrEmpty(assignedPath) && !assignedPath.StartsWith(PrototypeFolder)) continue;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    var primitive = PrimitiveFor(anchorType);
                    var instance = GameObject.CreatePrimitive(primitive);
                    instance.name = displayName + "（原型）";
                    instance.transform.localScale = ScaleFor(anchorType);
                    var collider = instance.GetComponent<Collider>();
                    if (collider != null) Object.DestroyImmediate(collider);
                    prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
                    Object.DestroyImmediate(instance);
                }
                ApplyPrototypeLook(path, id, i, item.FindPropertyRelative("rarity").enumValueIndex, anchorType);
                prefabProperty.objectReferenceValue = prefab;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }

        private static void ApplyPrototypeLook(string prefabPath, string id, int index, int rarity, FurnitureAnchorType anchorType)
        {
            var materialPath = $"{MaterialFolder}/{id}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader) { name = id + "_Prototype" };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            var palette = new[]
            {
                new Color(0.91f, 0.48f, 0.38f), new Color(0.42f, 0.69f, 0.91f),
                new Color(0.55f, 0.78f, 0.49f), new Color(0.93f, 0.72f, 0.30f),
                new Color(0.68f, 0.55f, 0.86f), new Color(0.36f, 0.80f, 0.72f),
                new Color(0.95f, 0.58f, 0.72f), new Color(0.72f, 0.57f, 0.40f),
                new Color(0.50f, 0.74f, 0.96f), new Color(0.96f, 0.55f, 0.25f),
                new Color(0.75f, 0.45f, 0.92f), new Color(0.98f, 0.84f, 0.35f)
            };
            var color = palette[index % palette.Length];
            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            var renderer = root.GetComponentInChildren<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            root.transform.localScale = ScaleFor(anchorType) * (1f + index * 0.018f);
            root.transform.localRotation = Quaternion.Euler(0f, index * 17f, 0f);
            if (rarity > 0 && root.transform.Find("稀有装饰") == null)
            {
                var accent = GameObject.CreatePrimitive(rarity == 2 ? PrimitiveType.Sphere : PrimitiveType.Cube);
                accent.name = "稀有装饰";
                accent.transform.SetParent(root.transform, false);
                accent.transform.localPosition = new Vector3(0f, 0.65f, 0f);
                accent.transform.localScale = rarity == 2 ? Vector3.one * 0.32f : new Vector3(0.28f, 0.16f, 0.28f);
                var collider = accent.GetComponent<Collider>();
                if (collider != null) Object.DestroyImmediate(collider);
                accent.GetComponent<Renderer>().sharedMaterial = material;
            }
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static PrimitiveType PrimitiveFor(FurnitureAnchorType type)
        {
            switch (type)
            {
                case FurnitureAnchorType.Toy: return PrimitiveType.Sphere;
                case FurnitureAnchorType.FoodBowl: return PrimitiveType.Cylinder;
                case FurnitureAnchorType.Rug: return PrimitiveType.Cylinder;
                default: return PrimitiveType.Cube;
            }
        }

        private static Vector3 ScaleFor(FurnitureAnchorType type)
        {
            switch (type)
            {
                case FurnitureAnchorType.CatBed: return new Vector3(0.72f, 0.14f, 0.58f);
                case FurnitureAnchorType.FoodBowl: return new Vector3(0.28f, 0.08f, 0.28f);
                case FurnitureAnchorType.Rug: return new Vector3(0.7f, 0.025f, 0.7f);
                case FurnitureAnchorType.Desktop: return new Vector3(0.35f, 0.4f, 0.25f);
                default: return Vector3.one * 0.22f;
            }
        }

        private static void EnsureFolder(string parent, string name)
        {
            var path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
